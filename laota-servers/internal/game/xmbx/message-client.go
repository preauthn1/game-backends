package xmbx

import (
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
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
	data.WinMoney = proto.Int64(logic.GameWinMoney)
	data.JackpotMoney = proto.Int64(logic.GameWinJackpot)
	data.FinalMoney = proto.Int64(logic.GameUser.Score)
	data.ReelResult = logic.BuildReelResult(logic.GameResultManager, logic.GamePayResult)
	data.PayGroups = logic.BuildPayGroups(logic.GamePayResult)
	if logic.FreeSpin != nil {
		data.FreeGameWinMoney = proto.Int64(logic.FreeSpin.WinMoney)
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendLightningPrepareMessage 傳送Lightning準備畫面
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10001 (XMBX_GameMessageClassID_XMBX_LightningPrepareID)
//   * Message = { LightningStep: 0, Count: 3, ReelResult: [{},{},...], LockReels: [ {Reel: 1, Row: 1}, {}, ... ] }
func (logic *Logic) SendLightningPrepareMessage(user *gamelogic.GameUser) {

	if logic.LightningGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningPrepareID)

	data := new(netproto.Slot_MSG_LightningPrepare)
	data.IsFreeGame = proto.Bool(logic.LightningGame.IsFreeGame)

	step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

	data.LightningStep = proto.Int32(int32(logic.LightningGame.ShowStep + 1))
	data.Count = proto.Int32(int32(step.LastChance + 1))
	data.ReelSetting = logic.NetprotoSlotReelSetting
	data.BetMoney = proto.Int64(logic.LightningGame.BetMoney)
	data.BetRate = proto.Int64(logic.LightningGame.BetRate)

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

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendLightningResultMessage 傳送Lightning輪軸結果
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10002 (XMBX_GameMessageClassID_XMBX_LightningResultID)
//   * Message = { LightningStep: 0, Count: 3, ReelResult: [{},{},...], LockReels: [ {Reel: 1, Row: 1}, {}, ... ] }
func (logic *Logic) SendLightningResultMessage(user *gamelogic.GameUser) {

	if logic.LightningGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningResultID)
	data := new(netproto.Slot_MSG_LightningResult)

	step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

	data.BetMoney = proto.Int64(0)
	data.WinMoney = proto.Int64(step.WinMoney)
	data.FinalMoney = proto.Int64(logic.GameUser.Score)
	if logic.FreeSpin != nil {
		data.FreeGameWinMoney = proto.Int64(logic.FreeSpin.WinMoney)
	}

	data.LightningStep = proto.Int32(int32(logic.LightningGame.ShowStep))

	if logic.LightningGame.ShowStep+1 < len(logic.LightningGame.Steps) {
		data.Count = proto.Int32(int32(step.LastChance + 1))
	} else {
		data.Count = proto.Int32(int32(step.LastChance))
	}

	data.ReelResult = logic.BuildReelResult(step.GameResultManager, nil)

	for _, award := range step.Awards {
		payout := new(netproto.Slot_LightningPayout)

		payout.WinMoney = proto.Int64(award.WinMoney)
		payout.Type = proto.String(award.Type.String())
		if award.Type == slot.PayTypeJackpot {
			payout.JackpotType = proto.String(award.JackpotType.String())
		}
		payout.Position = &netproto.Slot_ReelPosition{Reel: proto.Int32(award.Position.Reel), Row: proto.Int32(award.Position.Row)}
		if award.Symbol == "" {
			data.JackpotMoney = proto.Int64(award.WinMoney)
		}

		data.PayGroups = append(data.PayGroups, payout)
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

func replaceGoldBallSymbol(settings []*netproto.Slot_ReelSetting) {
	for _, row := range settings {
		for _, symbol := range row.GetSymbols() {
			if strings.HasPrefix(symbol.GetSymbolName(), "g") {
				symbol.SymbolName = proto.String("g1")
			}
		}
	}
}
