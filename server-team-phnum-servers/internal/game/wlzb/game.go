package wlzb

import (
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/prob"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// Bet 判斷下注是否
func (logic *Logic) Bet(user *gamelogic.GameUser, money int64) (ok bool, retcode int32, betmoney int64) {

	// 檢查狀態是否可下注
	if logic.GameState != slot.GameStateBet {
		return false, slot.BetStateStepError, logic.GamePayMoney
	}

	// 檢查下注金額是否合法
	var moneyOK bool
	for _, m := range logic.BetCounts {
		if int64(m) == money {
			moneyOK = true
			break
		}
	}

	if !moneyOK {
		mlog.Warn("玩家 %v 非法下注，金額: %d，倍率: %d", user, money, logic.GameBetRate)
		return false, slot.BetStateIllegal, logic.GamePayMoney
	}

	logic.GameBetMoney = money
	logic.GamePayMoney = money * logic.GameBetRate

	if logic.FreeSpin != nil && logic.FreeSpin.Count > 0 {
		logic.GameBetMoney = logic.FreeSpin.BetMoney
		logic.GamePayMoney = 0
		logic.FreeSpin.Count--
	}

	// 檢查最後金額是否為負數
	if logic.GamePayMoney < 0 {
		mlog.Warn("玩家 %v 非法下注，付款金額小於零 %d", user.UserID, logic.GamePayMoney)
		return false, slot.BetStateIllegal, logic.GamePayMoney
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

	// 免費遊戲檢查是否有龍加倍
	if logic.FreeSpin != nil {

		var multi int64 = 1

		for _, result := range logic.GamePayResult {

			if !result.IsMatch || result.Rule == nil {
				continue
			}

			switch result.GetPayType() {
			case slot.PayTypeChip:
				// 計算贏得的金額

				var _multi int64 = 1

				// 若有百搭加乘
				if result.HasWild {
					if multi == 1 {
						str := logic.FreeSpin.Origin.GetMultiplierValue()

						multis := strings.Split(str, ",")

						r := new(prob.Random)

						for _, row := range multis {
							vals := strings.Split(row, ":")

							m, _ := strconv.ParseFloat(vals[0], 64)
							w, _ := strconv.ParseFloat(vals[1], 64)

							r.Add(int64(m), int(w))
						}

						multi = r.Choose().(int64)
					}

					_multi = multi
				}

				result.Multi = int32(_multi)

				win := result.CalcWinMoney(logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate)

				logic.GameWinMoney += win
				logic.FreeSpin.WinMoney += win

				logic.GameLog.AddGameStep("開獎符號: %s, 比對順序: %s, 符號連續 %d 個; 符號賠率: %v; 連線數總和: %d, 出現龍: %t, 加成 %d 倍, 金額: %d", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, result.Rule.GetPayout(), result.WayCount, result.HasWild, _multi, win)
				mlog.Debug("開獎符號: %s, 比對順序: %s, 符號連續 %d 個; 符號賠率: %v; 連線數總和: %d, 出現龍: %t, 加成 %d 倍, 金額: %d", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, result.Rule.GetPayout(), result.WayCount, result.HasWild, _multi, win)

				for _, line := range result.SymbolPayLines {
					logic.GameLog.AddGameStep("連線內容: %v, 賠率加乘: %v", line.PayLine, line.GetMultipleValue())
					mlog.Debug("連線內容: %v, 賠率加乘: %v", line.PayLine, line.GetMultipleValue())
				}
			}
		}

	} else {
		// 一般遊戲錢照算
		logic.GameWinMoney = logic.SlotLogicBase.CalcGameAward(logic.GamePayResult, logic.GameBetMoney, logic.GameBetRate)
	}

	for _, result := range logic.GamePayResult {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		// 是否中FreeSpin給玩家選擇
		if result.GetPayType() == slot.PayTypeSelectGameMode {
			s, err := NewSelectChance(result.Rule, logic.FreeSpinTable, logic.GameBetMoney, logic.GameBetRate)
			if err != nil {
				continue
			}

			logic.SelectChances = append(logic.SelectChances, s)

			logic.GameLog.AddGameStep("贏得選擇遊戲機會 (%d), 目前剩餘次數: %d 次", s.GroupID, len(logic.SelectChances))
			mlog.Debug("贏得選擇遊戲機會 (%d), 目前剩餘次數: %d 次", s.GroupID, len(logic.SelectChances))

			logic.RecordSelectGameTimes(logic.GameUser)

			logic.SendSelectChanceMessage(logic.GameUser)
		}
	}

}
