package jjbx

import (
	"encoding/json"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// SendGameResultMessage 發送遊戲結果
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20004 (Slot_GameMessageClassID_Slot_GameResultID)
//   * Message = { BetMoney: 100, WinMoney: 0, FinalMoney: 300, ReelResult: [{},{},...] }
func (logic *Logic) SendGameResultMessage(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_GameResultID)
	data := new(netproto.Slot_MSG_GameResult)

	data.BetMoney = proto.Int64(logic.GamePayMoney)
	data.WinMoney = proto.Int64(logic.GameWinMoney - logic.GameWinJackpot)
	data.JackpotMoney = proto.Int64(logic.GameWinJackpot)
	data.FinalMoney = proto.Int64(logic.GameUser.Score)
	data.ReelResult = logic.BuildReelResult(logic.GameResultManager, logic.GamePayResult)
	data.PayGroups = logic.BuildPayGroups(logic.GamePayResult)
	if logic.FreeSpin != nil {
		data.FreeGameWinMoney = proto.Int64(logic.FreeSpin.WinMoney)

		if logic.FreeSpin.FreeSpinExtAward != nil {

			if logic.FreeSpin.FreeSpinExtAward.IsFinished {
				data.WinMoney = proto.Int64(logic.GameWinMoney - logic.GameWinJackpot - logic.FreeSpin.FreeSpinExtAward.TotalWinMoney)
			}

			data.FreeGameExtAward = new(netproto.Slot_FreeGameExtAward)
			data.FreeGameExtAward.TotalCount = proto.Int32(logic.FreeSpin.FreeSpinExtAward.TotalCount)
			data.FreeGameExtAward.TotalWinMoney = proto.Int64(logic.FreeSpin.FreeSpinExtAward.TotalWinMoney)
			data.FreeGameExtAward.Count = proto.Int32(logic.FreeSpin.FreeSpinExtAward.Count)
			data.FreeGameExtAward.WinMoney = proto.Int64(logic.FreeSpin.FreeSpinExtAward.WinMoney)
			data.FreeGameExtAward.EachCountMoney = proto.Int64(logic.FreeSpin.FreeSpinExtAward.EachCountMoney)
			data.FreeGameExtAward.IsFinished = proto.Bool(logic.FreeSpin.FreeSpinExtAward.IsFinished)

			for pos, symbol := range logic.FreeSpin.FreeSpinExtAward.Symbols {
				s := new(netproto.Slot_Symbol)
				s.SymbolID = proto.Int32(int32(symbol.GetId()))
				s.SymbolName = proto.String(symbol.GetSymbol())
				s.Position = &netproto.Slot_ReelPosition{
					Reel: proto.Int32(pos.Reel),
					Row:  proto.Int32(pos.Row),
				}
				data.FreeGameExtAward.Symbols = append(data.FreeGameExtAward.Symbols, s)
			}
		}
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendSelectChanceMessage 發送遊戲結果
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20008 (Slot_GameMessageClassID_Slot_SelectGameOptionsID)
func (logic *Logic) SendSelectChanceMessage(user *gamelogic.GameUser) {

	if logic.SelectChances == nil || len(logic.SelectChances) == 0 {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID)

	data := new(netproto.Slot_MSG_ChooseGameOptions)
	data.Times = proto.Int32(int32(len(logic.SelectChances)))

	// data.Options
	chance := logic.SelectChances[0]

	data.BetMoney = proto.Int64(chance.BetMoney)
	data.BetRate = proto.Int64(chance.BetRate)

	for _, row := range chance.Options {
		opt := new(netproto.Slot_ChooseGameOption)
		opt.ID = proto.Int32(row.GameID)
		opt.Type = proto.String(string(row.GameMode))

		switch row.GameMode {
		case slot.GameModeFreeSpin:
			tb := row.GameTable.(*protodata.FreeSpinTable)
			dMap := map[string]interface{}{
				"count": tb.GetFreeSpinCount(),
				"info":  tb.GetInfo(),
			}
			if b, err := json.Marshal(dMap); err == nil {
				opt.Data = proto.String(string(b))
			}
		}

		data.Options = append(data.Options, opt)
	}
	if b, err := json.Marshal(chance.Data); err == nil {
		data.Data = proto.String(string(b))
	}

	data.ReelResult = logic.BuildReelResult(chance.GameResultManager, nil)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendLightningPrepareMessage 傳送Lightning準備畫面
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20006 (Slot_GameMessageClassID_Slot_LightningPrepareID)
//   * Message = { LightningStep: 0, Count: 3, ReelResult: [{},{},...], LockReels: [ {Reel: 1, Row: 1}, {}, ... ] }
func (logic *Logic) SendLightningPrepareMessage(user *gamelogic.GameUser) {

	if logic.LightningGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID)

	data := new(netproto.Slot_MSG_LightningPrepare)
	data.IsFreeGame = proto.Bool(logic.LightningGame.IsFreeGame)

	step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

	data.LightningStep = proto.Int32(int32(logic.LightningGame.ShowStep + 1))
	data.Count = proto.Int32(int32(step.LastChance))
	data.BetMoney = proto.Int64(logic.LightningGame.BetMoney)
	data.BetRate = proto.Int64(logic.LightningGame.BetRate)
	// data.ReelSetting = logic.NetprotoSlotReelSetting
	var symbolGroupID int32
	if logic.LightningGame.IsLimit && len(logic.LightningGame.Steps) > logic.LightningGame.LimitPoint {
		symbolGroupID = logic.LightningGame.LimitSymbolGroupID
	} else {
		symbolGroupID = logic.LightningGame.SymbolGroupID
	}
	data.ReelSetting = logic.BuildReelSetting(symbolGroupID, logic.DefaultReelNum, logic.DefaultRowNum, logic.SlotSymbolDistribution)

	step.GameResultManager.RowMode()
	for rowID, row := range step.GameResultManager.GameResult {
		for reelID, symbol := range row {
			if logic.LightningGame.IsGoldBall(symbol) {
				data.LockSymbols = append(data.LockSymbols, &netproto.Slot_LightningLock{
					SymbolID: proto.Int32(int32(symbol.GetId())),
					Position: &netproto.Slot_ReelPosition{
						Reel: proto.Int32(int32(reelID + 1)),
						Row:  proto.Int32(int32(rowID + 1)),
					},
				})
			}
		}
	}

	data.ReelResult = logic.BuildReelResult(step.GameResultManager, nil)

	// 把所有Step的Award都包進去
	for _, _step := range logic.LightningGame.Steps {
		for _, award := range _step.Awards {
			payout := new(netproto.Slot_LightningPayout)
			payout.SymbolID = proto.Int32(int32(award.Symbol.GetId()))

			payout.WinMoney = proto.Int64(award.WinMoney)
			payout.Type = proto.String(award.Type.String())
			if award.Type == slot.PayTypeJackpot {
				payout.JackpotType = proto.String(award.JackpotType.String())
			}
			if award.JackpotType != slot.JackpotMajor {
				payout.Position = &netproto.Slot_ReelPosition{Reel: proto.Int32(award.Position.Reel), Row: proto.Int32(award.Position.Row)}
			} else {
				continue
			}

			data.PastPayGroups = append(data.PastPayGroups, payout)
		}
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendLightningResultMessage 傳送Lightning輪軸結果
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20007 (Slot_GameMessageClassID_Slot_LightningResultID)
//   * Message = { LightningStep: 0, Count: 3, ReelResult: [{},{},...], LockReels: [ {Reel: 1, Row: 1}, {}, ... ] }
func (logic *Logic) SendLightningResultMessage(user *gamelogic.GameUser) {

	if logic.LightningGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_LightningResultID)
	data := new(netproto.Slot_MSG_LightningResult)

	step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

	data.BetMoney = proto.Int64(0)
	data.WinMoney = proto.Int64(step.WinMoney)
	data.FinalMoney = proto.Int64(logic.GameUser.Score)
	if logic.FreeSpin != nil {
		data.FreeGameWinMoney = proto.Int64(logic.FreeSpin.WinMoney)
	}

	data.LightningStep = proto.Int32(int32(logic.LightningGame.ShowStep))

	data.Count = proto.Int32(int32(step.LastChance))

	data.ReelResult = logic.BuildReelResult(step.GameResultManager, nil)

	for _, award := range step.Awards {
		payout := new(netproto.Slot_LightningPayout)

		payout.WinMoney = proto.Int64(award.WinMoney)
		payout.Type = proto.String(award.Type.String())
		if award.Type == slot.PayTypeJackpot {
			payout.JackpotType = proto.String(award.JackpotType.String())
		}
		if award.JackpotType != slot.JackpotGrand {
			payout.Position = &netproto.Slot_ReelPosition{Reel: proto.Int32(award.Position.Reel), Row: proto.Int32(award.Position.Row)}
		} else {
			data.JackpotMoney = proto.Int64(award.WinMoney)
			data.WinMoney = proto.Int64(step.WinMoney - award.WinMoney)
			continue
		}

		data.PayGroups = append(data.PayGroups, payout)
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}
