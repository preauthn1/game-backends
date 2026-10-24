package dfdc

import (
	"math/rand"
	"strconv"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// Spin 玩家轉輪軸
func (logic *Logic) Spin(symbolGroupID int32) {

	// 產生遊戲結果
	manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, logic.SlotSymbolDistribution)

	logic.GameResultManager = manager

	// 紀錄開獎符號
	logic.LogGameResult(manager)

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

		// logic.GameLog.AddGameStep("免費遊戲中第 %d 次中獎, 加乘 %d 倍, 原中獎金額: %d, 加乘後金額: %d", logic.FreeSpin.WinCount, multi, win, total)
	}

	for _, result := range logic.GamePayResult {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		if result.GetPayType() == slot.PayTypeMysteryJackpot {

			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			// 骰出中BonusGame
			// 隨機產生該總重量
			rand.Seed(time.Now().UnixNano())
			random := rand.Intn(1e8) // 億分之一

			if int(random) > int(f) {
				continue
			}

			var jackpots []*slot.Jackpot
			for _, jackpot := range logic.Jackpots {
				jackpots = append(jackpots, jackpot)
			}

			logic.JackpotGame = slot.NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, int32(logic.GameBetRate), logic.GameBetMoney)
			logic.JackpotGame.Origin = result.Rule
			logic.JackpotGame.Play()

			win := logic.CalcJackpot(logic.JackpotGame.WinJackpot)

			if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
				logic.FreeSpin.WinMoney += win
			}

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 JackpotGame[%s], 獎金: %d, 將於翻牌結束後結算", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, logic.JackpotGame.WinJackpot.Level.String(), win)

			logic.RecordJackpotGame(logic.GameUser)

			continue
		}

		if result.GetPayType() == slot.PayTypeFreeSpin {

			logic.WinFreeSpin(result.Rule)

			logic.UpdateFreeSpin(logic.GameUser)

			continue
		}

	}

}
