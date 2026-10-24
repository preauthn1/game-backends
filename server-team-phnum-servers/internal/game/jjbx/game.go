package jjbx

import (
	"math/rand"
	"strconv"
	"strings"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// Bet 判斷下注是否
func (logic *Logic) Bet(user *gamelogic.GameUser, money int64, rate int64) (ok bool, retcode int32, betmoney int64) {

	// 檢查狀態是否可下注
	if logic.GameState != slot.GameStateBet {
		return false, slot.BetStateStepError, logic.GamePayMoney
	}

	// 檢查下注金額是否合法
	var moneyOK, rateOK bool
	for _, m := range logic.BetCounts {
		if int64(m) == money {
			moneyOK = true
			break
		}
	}

	for _, r := range logic.BetRates {
		if int64(r) == rate {
			rateOK = true
			break
		}
	}

	if !moneyOK || !rateOK {
		mlog.Warn("玩家 %v 非法下注，金額: %d，倍率: %d", user, money, rate)
		return false, slot.BetStateIllegal, logic.GamePayMoney
	}

	logic.GameBetMoney = money
	logic.GameBetRate = rate
	logic.GamePayMoney = money * rate

	if logic.FreeSpin != nil && logic.FreeSpin.Count > 0 {
		logic.GameBetMoney = logic.FreeSpin.BetMoney
		logic.GamePayMoney = 0
		logic.FreeSpin.Count--
		logic.FreeSpin.Used++
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

		switch result.Rule.GetUseType() {
		case "normal":
			if logic.IsFreeSpin() {
				continue
			}
		case "freespin":
			if !logic.IsFreeSpin() {
				continue
			}
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

				// 等 Client 來請求 Jackpot 金額在更新
				// logic.SendJackpotMessageToUser(logic.GameUser)
			}
		}

		// Jackpot Game
		if result.GetPayType() == slot.PayTypeMysteryJackpot {
			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			// 骰出中BonusGame
			// 隨機產生該總重量
			rand.Seed(time.Now().UnixNano())
			random := rand.Intn(100000000) // 億分之一

			if int(random) > int(f) {
				continue
			}

			logic.GameLog.AddGameStep("即將進入JackpotGame")
			mlog.Debug("即將進入JackpotGame")

			var jackpots []*slot.Jackpot
			for _, jackpot := range logic.Jackpots {
				jackpots = append(jackpots, jackpot)
			}

			logic.JackpotGame = slot.NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, int32(logic.GameBetRate), logic.GameBetMoney)
			logic.JackpotGame.Origin = result.Rule

			logic.JackpotGame.Play()

			logic.GameLog.AddGameStep("JackpotGame獎金層級: %s", logic.JackpotGame.WinJackpot.Level)
			mlog.Debug("JackpotGame獎金層級: %s", logic.JackpotGame.WinJackpot.Level)

			logic.JackpotGame.WinJackpot.AwardTotal += logic.JackpotGame.WinMoney
			if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
				logic.FreeSpin.WinMoney += logic.JackpotGame.WinMoney
			}

			if logic.JackpotGame.WinJackpot.IsNeedReset() {
				logic.UpdateJackpotPoolNoLink(0, -logic.JackpotGame.WinJackpot.TotalContribution, -logic.JackpotGame.WinJackpot.AwardTotal+logic.JackpotGame.WinMoney, logic.JackpotGame.WinJackpot.Level)
				logic.JackpotGame.WinJackpot.Reset()
			} else {
				logic.UpdateJackpotPoolNoLink(0, 0, logic.JackpotGame.WinMoney, logic.JackpotGame.WinJackpot.Level)
			}

			logic.RecordJackpotGame(logic.GameUser)

			continue
		}

		// Jackpot Grand
		if result.GetPayType() == slot.PayTypeMysteryGrand {
			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			// 骰出中BonusGame
			// 隨機產生該總重量
			rand.Seed(time.Now().UnixNano())
			random := rand.Intn(100000000) // 億分之一

			if int(random) > int(f) {
				continue
			}

			jp := logic.Jackpots[slot.JackpotGrand]

			win := jp.GetCurrentPoolByBet(logic.GameBetMoney*logic.GameBetRate, int64(logic.MaxBetCount*logic.MaxBetRate))

			logic.GameLog.AddGameStep("出現 %d 個 %s, 贏得Jackpot Grand獎勵: %d", result.Rule.GetCount(), result.Rule.GetSymbol(), win)
			mlog.Debug("出現 %d 個 %s, 贏得Jackpot Grand獎勵: %d", result.Rule.GetCount(), result.Rule.GetSymbol(), win)

			logic.GameWinMoney += win
			logic.GameWinJackpot = win

			jp.AwardTotal += win
			// if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
			// 	logic.FreeSpin.WinMoney += win
			// }

			if jp.IsNeedReset() {
				logic.UpdateJackpotPoolNoLink(0, -jp.TotalContribution, -jp.AwardTotal+win, jp.Level)
				jp.Reset()
			} else {
				logic.UpdateJackpotPoolNoLink(0, 0, win, jp.Level)
			}

			continue
		}

		// Select Game
		if result.GetPayType() == slot.PayTypeSelection {

			selection, err := NewSelectChance(logic, logic.GameResultManager, result.Rule, logic.GameBetMoney, logic.GameBetRate)
			if err != nil {
				mlog.Error("建立選擇遊戲錯誤. %v", err)
				continue
			}

			logic.GameLog.AddGameStep("贏得選擇機會, GroupID: %d", selection.GroupID)
			mlog.Debug("贏得選擇機會, GroupID: %d", selection.GroupID)

			// 依照本次下注倍率選擇該次選擇遊戲選項
			for _, row := range selection.Options {
				logic.GameLog.AddGameStep("  - 選項: %v", row)
				mlog.Debug("  - 選項: %v", row)
			}

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
							case slot.PayTypeChipXRate:
								f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)
								win := selection.BetMoney * selection.BetRate * int64(f)
								gMoney += win
							}
						}
					}
				}
			}
			selection.Data["GMoney"] = gMoney

			logic.SelectChances = append(logic.SelectChances, selection)

			// TODO Save DB
			logic.RecordSelectGameTimes(logic.GameUser)
		}

		// FreeSpin Add Count
		if result.GetPayType() == slot.PayTypeAddTimes {

			if logic.FreeSpin == nil {
				mlog.Error("增加免費遊戲次數錯誤, 沒有免費遊戲")
				continue
			}

			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			logic.FreeSpin.Count += int(f)

			logic.GameLog.AddGameStep("贏得免費遊戲次數, 增加 %d 次, 剩餘 %d 次", int(f), logic.FreeSpin.Count)
			mlog.Debug("贏得免費遊戲次數, 增加 %d 次, 剩餘 %d 次", int(f), logic.FreeSpin.Count)
		}
	}

}
