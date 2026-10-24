package dfdc

import (
	"math/rand"
	"strconv"
	"strings"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Bet 判斷下注是否
func (logic *Logic) Bet(user *gamelogic.GameUser, money int64, rate int64) (ok bool, retcode int32, betmoney int64) {

	// 檢查狀態是否可下注
	if logic.GameState != slot.GameStateBet {
		return false, slot.BetStateStepError, logic.GamePayMoney
	}

	logic.GameBetMoney = money
	logic.GameBetRate = rate
	logic.GamePayMoney = money * rate

	if logic.FreeSpin != nil {
		if logic.FreeSpin.Count > 0 {
			logic.GameBetMoney = logic.FreeSpin.BetMoney
			logic.GameBetRate = logic.FreeSpin.BetRate
			logic.GamePayMoney = 0
			logic.FreeSpin.Count--

			logic.UpdateFreeSpin(user)
		} else {
			logic.FreeSpin = nil
		}
	} else {
		if user.Coupon > 0 {
			if user.Coupon >= logic.GamePayMoney {
				logic.GamePayCoupon = logic.GamePayMoney
				logic.GamePayMoney = 0
			} else {
				logic.GamePayCoupon = user.Coupon
				logic.GamePayMoney -= logic.GamePayCoupon
			}
			logic.IsCoupon = true
		} else {
			logic.IsCoupon = false
		}
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

		if result.GetPayType() == slot.PayTypeMysteryJackpot {

			if logic.FreeSpin != nil && logic.FreeSpin.IsCoupon {
				continue
			}

			if logic.IsCoupon {
				continue
			}

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

			logic.JackpotGame = NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, int32(logic.GameBetRate), logic.GameBetMoney)

			logic.JackpotGame.Play()

			logic.GameLog.AddGameStep("JackpotGame獎金層級: %s", logic.JackpotGame.WinJackpot.Level)
			mlog.Debug("JackpotGame獎金層級: %s", logic.JackpotGame.WinJackpot.Level)

			// 發送開獎內容
			logic.SendJackpotGameDetailMessage(logic.GameUser)

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

			logic.UpdateBonusGame(logic.GameUser)

			continue
		}

		if result.GetPayType() == slot.PayTypeFreeSpin {
			// 取得贏得FreeSpin的GroupID
			f, err := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			if err != nil {
				mlog.Warn("SlotPayTable.Payout解析錯誤，%v", err)
				continue
			}
			freeSpinGroupID := int32(f)

			var freeSpinTables []*protodata.FreeSpinTable
			var freeSpinTable *protodata.FreeSpinTable

			// 取得FreeSpinTable
			for _, table := range logic.FreeSpinTable {
				if table.GetGroupId() != freeSpinGroupID {
					continue
				}
				freeSpinTables = append(freeSpinTables, table)
			}

			if len(freeSpinTables) == 0 {
				mlog.Warn("查無FreeSpinTable, FreeSpinGroupID: %d", freeSpinGroupID)
				continue
			}

			// 依照倍率取得正確FreeSpinTable
			for _, table := range freeSpinTables {
				f, _ := strconv.ParseFloat(table.GetConditionValue(), 64)
				if int64(f) == logic.GameBetRate {
					freeSpinTable = table
					break
				}
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
				logic.FreeSpin = slot.NewFreeSpin(freeSpinTable, logic.SlotSymbolDistribution, logic.GameBetMoney, logic.GameBetRate, logic.IsCoupon)
			}

			logic.GameLog.AddGameStep("贏得FreeSpin, ID: %d, SymbolGroupID: %d, Count: %d, 剩餘次數: %d", logic.FreeSpin.Origin.GetId(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)
			mlog.Debug("贏得FreeSpin, ID: %d, SymbolGroupID: %d, Count: %d, 剩餘次數: %d", logic.FreeSpin.Origin.GetId(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)

			logic.UpdateFreeSpin(logic.GameUser)

			continue
		}

	}

}
