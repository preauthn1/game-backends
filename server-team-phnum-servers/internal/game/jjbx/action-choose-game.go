package jjbx

import (
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// ActionChooseGame 玩家選擇獎勵模式
func (logic *Logic) ActionChooseGame(user *gamelogic.GameUser, gmsg *msg.Message) {

	if logic.SelectChances == nil || len(logic.SelectChances) == 0 {
		mlog.Error("[玩家選擇獎勵] 沒有選擇機會")
		return
	}

	data := &netproto.Slot_MSG_ChooseGame{
		Option: new(netproto.Slot_ChooseGameOption),
	}

	if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
		mlog.Error("[玩家選擇獎勵] 接收玩家封包解析錯誤. %v", err)
		return
	}

	selection := logic.SelectChances[0]

	// 檢查選項是否合法
	var option *SelectOption
	for _, opt := range selection.Options {
		if string(opt.GameMode) == data.Option.GetType() && opt.GameID == data.Option.GetID() {
			option = opt
		}
	}

	if option == nil {
		mlog.Error("[玩家選擇獎勵] 玩家傳遞的選擇機會不再選項中, %v", data.Option)
		return
	}

	switch slot.GameMode(data.Option.GetType()) {
	case slot.GameModeFreeSpin:
		freeSpinTable := option.GameTable.(*protodata.FreeSpinTable)

		// 檢查本身是否是FreeSpin
		if logic.FreeSpin != nil {
			if logic.FreeSpin.Origin != freeSpinTable {
				mlog.Warn("[玩家選擇獎勵] 選擇的FreeSpin與原本的FreeSpin不同")
				return
			}

			logic.FreeSpin.Count += int(freeSpinTable.GetFreeSpinCount())

		} else {
			logic.FreeSpin = slot.NewFreeSpin(freeSpinTable, logic.SlotSymbolDistribution, selection.BetMoney, selection.BetRate, false)
		}

		logic.GameLog.AddGameStep("玩家選擇 FreeSpin, ID: %d, SymbolGroupID: %d, 次數: %d, 剩餘次數: %d", logic.FreeSpin.Origin.GetId(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)
		mlog.Debug("玩家選擇 FreeSpin, ID: %d, SymbolGroupID: %d, 次數: %d, 剩餘次數: %d", logic.FreeSpin.Origin.GetId(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)

		// 計算當前g1符號，出現g7該給多少
		var gMoney int64
		var gCnt int

		for _, reel := range selection.GameResultManager.GameResult {
			for _, symbol := range reel {
				symbol.GetSymbol()
				for _, attr := range logic.SymbolAttributes {
					if attr.GetIsGoldBall() == 1 && attr.GetSymbol() == symbol.GetSymbol() {
						gCnt++
						switch slot.PayType(attr.GetGoldballType()) {
						case slot.PayTypeChip:
							f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)
							win := selection.BetMoney * int64(f)
							gMoney += win
							logic.GameLog.AddGameStep("出現『福』(Chip), 倍率: %d, 增加 %d 獎金", int(f), win)
							mlog.Debug("出現『福』(Chip), 倍率: %d, 增加 %d 獎金", int(f), win)
						case slot.PayTypeChipXRate:
							f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)
							win := selection.BetMoney * selection.BetRate * int64(f)
							gMoney += win
							logic.GameLog.AddGameStep("出現『福』(ChipXRate), 倍率: %d, 增加 %d 獎金", int(f), win)
							mlog.Debug("出現『福』(ChipXRate), 倍率: %d, 增加 %d 獎金", int(f), win)
						}
					}
				}
			}
		}
		if gCnt > 0 {
			logic.GameLog.AddGameStep("出現 %d 個『福』, 未來免費遊戲中出現『黃幣』將增加 %d 獎金", gCnt, gMoney)
			mlog.Debug("出現 %d 個『福』, 未來免費遊戲中出現『黃幣』將增加 %d 獎金", gCnt, gMoney)
		}

		logic.GameLog.AddGameStep("初始累積金額增加 %d", gMoney)
		mlog.Debug("初始累積金額增加 %d", gMoney)

		award := new(slot.FreeSpinExtAward)
		award.TotalWinMoney = gMoney // 入場會先加一次
		award.EachCountMoney = gMoney

		logic.FreeSpin.FreeSpinExtAward = award

		logic.UpdateFreeSpin(logic.GameUser)

		logic.SendLeftFreeGameToUser(user)
	case slot.GameModeLightning:
		// 取得RespinTable
		respinTable := option.GameTable.(*protodata.RespinTable)

		logic.GameLog.AddGameStep("玩家選擇 Lightning, ID: %d, SymbolGroupID: %d", respinTable.GetId(), respinTable.GetRespinValue())
		mlog.Debug("玩家選擇 Lightning, ID: %d, SymbolGroupID: %d", respinTable.GetId(), respinTable.GetRespinValue())

		if logic.LightningGame == nil {
			logic.LightningGame = NewLightningGame(logic, respinTable, selection.BetMoney, selection.BetRate, logic.IsFreeSpin())
		}

		step := NewLightningStep(respinTable, selection.Rule, selection.GameResultManager, int(respinTable.GetRespinCount()))
		logic.LightningGame.CalcStepAward(step)

		logic.GameLog.AddGameStep("原始牌面獎勵:")
		mlog.Debug("原始牌面獎勵:")
		step.PrintAward(logic)

		logic.LightningGame.AddStep(step)
		logic.LightningGame.WinMoney += step.WinMoney

		logic.RecordLightningData(user)

		logic.SendLightningPrepareMessage(user)
	}

	// 移除本次選擇
	logic.SelectChances = logic.SelectChances[1:]

	// TODO Save DB
	logic.RecordSelectGameTimes(user)

	logic.GameEndLog()

	// 調整狀態為等待
	logic.UpdateState(slot.GameStateWait)

	logic.GameEndFunc()

}
