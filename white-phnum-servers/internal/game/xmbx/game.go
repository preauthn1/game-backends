package xmbx

import (
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Bet 判斷下注是否
func (logic *Logic) Bet(user *gamelogic.GameUser, money int64) (ok bool, retcode int32, betmoney int64) {

	// 檢查狀態是否可下注
	if logic.GameState != slot.GameStateBet {
		return false, slot.BetStateStepError, logic.GamePayMoney
	}

	logic.GameBetRate = int64(len(logic.SlotPayLine))
	logic.MaxBetRate = int32(len(logic.SlotPayLine))

	logic.GameBetMoney = money
	logic.GamePayMoney = money * logic.GameBetRate

	if logic.FreeSpin != nil && logic.FreeSpin.Count > 0 {
		logic.GameBetMoney = logic.FreeSpin.BetMoney
		logic.GamePayMoney = 0
		logic.FreeSpin.Count--
	}

	// 檢查玩家金額
	if user.Score < logic.GamePayMoney {
		return false, slot.BetStateMoneyNotEnough, logic.GamePayMoney
	}

	// 檢查最低攜帶金額
	if user.Score < logic.MinCarryMoney && logic.FreeSpin == nil {
		return false, slot.BetStateMoneyLessMin, logic.GamePayMoney
	}

	return true, slot.BetStateSuccess, logic.GamePayMoney
}

// Spin 玩家轉輪軸
func (logic *Logic) Spin(symbolGroupID int32) {
	// 產生遊戲結果
	manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, logic.SlotSymbolDistribution)

	logic.GameResultManager = manager

	// 紀錄開獎符號
	logic.GameLog.AddGameStep("輪軸結果, 使用SymbolGroupID: %d", logic.GameResultManager.SymbolGroupID)
	mlog.Debug("輪軸結果, 使用SymbolGroupID: %d", logic.GameResultManager.SymbolGroupID)

	logic.GameResultManager.RowMode()
	for i, row := range logic.GameResultManager.GetSymbolString() {
		logic.GameLog.AddGameStep("第 %d 行: %s", i+1, strings.Join(row, ", "))
		mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
	}

	logic.CalcGameResult()

	logic.CalcGameAward()
}

// CalcGameResult 計算遊戲結果
func (logic *Logic) CalcGameResult() {
	logic.GamePayResult = logic.SlotLogicBase.CalcGameResult(logic.GameResultManager)
}

// CalcGameAward 計算遊戲獎勵
func (logic *Logic) CalcGameAward() {

	logic.GameWinMoney = logic.SlotLogicBase.CalcGameAward(logic.GamePayResult, logic.GameBetMoney, logic.GameBetRate)

	if logic.FreeSpin != nil && logic.GameWinMoney > 0 {

		multi := logic.FreeSpin.GetMultipleValue()
		win := logic.GameWinMoney
		total := win * int64(multi)

		logic.FreeSpin.WinCount++
		logic.FreeSpin.WinMoney += total
		logic.GameWinMoney = total

		logic.GameLog.AddGameStep("免費遊戲中第 %d 次中獎, 加乘 %d 倍, 原中獎金額: %d, 加乘後金額: %d", logic.FreeSpin.WinCount, multi, win, total)
		mlog.Debug("免費遊戲中第 %d 次中獎, 加乘 %d 倍, 原中獎金額: %d, 加乘後金額: %d", logic.FreeSpin.WinCount, multi, win, total)
	}

	for _, result := range logic.GamePayResult {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		// 免費遊戲
		if result.GetPayType() == slot.PayTypeFreeSpin {
			// 取得贏得FreeSpin的GroupID
			f, err := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			if err != nil {
				mlog.Warn("SlotPayTable.Payout解析錯誤，%v", err)
				continue
			}
			freeSpinGroupID := int32(f)

			var freeSpinTable *protodata.FreeSpinTable

			// 取得FreeSpinTable
			for _, table := range logic.FreeSpinTable {
				if table.GetGroupId() != freeSpinGroupID {
					continue
				}
				freeSpinTable = table
			}

			if freeSpinTable == nil {
				mlog.Warn("查無FreeSpinTable, FreeSpinGroupID: %d", freeSpinGroupID)
				continue
			}

			// 檢查本身是否是FreeSpin
			if logic.FreeSpin != nil {
				if logic.FreeSpin.Origin != freeSpinTable {
					mlog.Warn("贏得的FreeSpin與原本的FreeSpin不同")
					continue
				}

				logic.FreeSpin.Count += int(freeSpinTable.GetFreeSpinCount())

			} else {
				logic.FreeSpin = slot.NewFreeSpin(freeSpinTable, logic.SlotSymbolDistribution, logic.GameBetMoney, logic.GameBetRate, false)
			}

			logic.GameLog.AddGameStep("贏得FreeSpin, ID: %d, SymbolGroupID: %d, Count: %d, 剩餘次數: %d", logic.FreeSpin.Origin.GetId(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)
			mlog.Debug("贏得FreeSpin, ID: %d, SymbolGroupID: %d, Count: %d, 剩餘次數: %d", logic.FreeSpin.Origin.GetId(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)

			logic.UpdateFreeSpin(logic.GameUser)

			continue
		}

		// Lightning
		if result.GetPayType() == slot.PayTypeLightning {

			// 取得贏得Lightning的GroupID
			f, err := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			if err != nil {
				mlog.Warn("SlotPayTable.Payout解析錯誤，%v", err)
				continue
			}

			respinGroupID := int32(f)

			// 取得RespinTable
			var respinTable *protodata.RespinTable

			// 取得RespinTable
			for _, table := range logic.RespinTable {
				if table.GetGroupId() != respinGroupID {
					continue
				}
				respinTable = table
			}

			if respinTable == nil {
				mlog.Warn("查無RespinTable, RespinGroupID: %d", respinGroupID)
				continue
			}

			logic.GameLog.AddGameStep("出現 %d 個金球，贏得Lightning機會, ID: %d, SymbolGroupID: %d", result.Rule.GetCount(), respinTable.GetId(), respinTable.GetRespinValue())
			mlog.Debug("出現 %d 個金球，贏得Lightning機會, ID: %d, SymbolGroupID: %d", result.Rule.GetCount(), respinTable.GetId(), respinTable.GetRespinValue())

			if logic.LightningGame == nil {

				var fromFreeGame bool

				if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
					fromFreeGame = true
				}

				logic.LightningGame = NewLightningGame(logic, fromFreeGame)
			}
			logic.LightningGame.AddStep(NewLightningStep(respinTable, result.Rule, logic.GameResultManager, int(respinTable.GetRespinCount())))
			logic.LightningGame.Spin()

			logic.RecordLightningData(logic.GameUser)
		}

		// Jackpot
		if result.GetPayType() == slot.PayTypeJackpot {

			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			var jackpot *slot.Jackpot
			// 找 Jackpot
			for _, jp := range logic.Jackpots {
				if jp.Origin.GetId() == int32(f) {
					jackpot = jp
				}
			}

			if jackpot != nil {

				win := jackpot.GetCurrentPoolByBet(logic.GameBetMoney*logic.GameBetRate, int64(logic.MaxBetCount)*int64(logic.MaxBetRate))

				if logic.Jackpots[slot.JackpotMajor].IsNeedReset() {
					logic.UpdateJackpotPoolNoLink(0, -logic.Jackpots[slot.JackpotMajor].TotalContribution, -logic.Jackpots[slot.JackpotMajor].AwardTotal+win, logic.Jackpots[slot.JackpotMajor].Level)
					logic.Jackpots[slot.JackpotMajor].Reset()
				} else {
					logic.UpdateJackpotPoolNoLink(0, 0, win, logic.Jackpots[slot.JackpotMajor].Level)
				}

				logic.GameWinMoney += win
				logic.GameWinJackpot = win

				logic.GameLog.AddGameStep("贏得最大獎Jackpot: %d", win)
				mlog.Debug("贏得最大獎Jackpot: %d", win)
			}

		}

	}

}
