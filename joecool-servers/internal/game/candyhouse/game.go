package candyhouse

import (
	"fmt"
	"math"
	"sort"
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

	var rate int64 = int64(len(logic.SlotPayLine))

	// 檢查下注金額是否合法
	var moneyOK bool
	for _, m := range logic.BetCounts {
		if int64(m) == money {
			moneyOK = true
			break
		}
	}

	if !moneyOK {
		mlog.Warn("玩家 %v 非法下注，金額: %d，倍率: %d", user, money, rate)
		return false, slot.BetStateIllegal, logic.GamePayMoney
	}

	logic.GameBetMoney = money
	logic.GamePayMoney = money * int64(len(logic.SlotPayLine))

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
	if user.Score < logic.MinCarryMoney {
		return false, slot.BetStateMoneyLessMin, logic.GamePayMoney
	}

	return true, slot.BetStateSuccess, logic.GamePayMoney
}

// Spin 玩家轉輪軸
func (logic *Logic) Spin(user *gamelogic.GameUser) {

	// 產生遊戲結果
	logic.CreateGameResult()

	// 紀錄開獎符號
	var logdesc = ""
	logdesc += fmt.Sprintf("SymbolGroupID: %d, ", logic.MainGameTable.GetSymbolGroupId())
	logdesc += fmt.Sprintf("data: ")
	logic.GameResultManager.RowMode()
	for _, row := range logic.GameResultManager.GetSymbolString() {
		logdesc += fmt.Sprintf("%s|", strings.Join(row, ", "))
	}
	logdesc = logdesc[:len(logdesc)-1]
	logic.GameLog.AddGameStep("%s", logdesc)
	mlog.Debug("%s", logdesc)

	// 計算遊戲結果
	logic.CalcGameResult()

	// 計算遊戲獎勵
	logic.CalcGameAward()

	// 計算流水
	logic.CalcFlowAmount(user)
}

// CreateGameResult 產生遊戲結果
//
// 由Logic的GameSymbolGroupID，與SlotSymbolDistribution，決定每隔可能會開出的符號
//
// 再將每隔可能開出的符號去開隨機一種符號
//
// 最後將結果寫入Logic的GameResult
func (logic *Logic) CreateGameResult() {

	logic.GameResultManager, _ = slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)

	logic.GameResultManager.GenerateGameResult(int32(logic.MainGameTable.GetSymbolGroupId()), logic.SlotSymbolDistribution)

}

// CalcGameResult 計算遊戲結果
func (logic *Logic) CalcGameResult() {

	logic.GamePayResult = make([]*slot.PayResult, 0)

	// // 依照SlotPayTable檢查中了哪些
	for _, rule := range logic.SlotPayTable {
		match, result := logic.PaylineMatcher.IsMatch(logic.GameResultManager, rule)
		if !match {
			continue
		}
		logic.GamePayResult = append(logic.GamePayResult, result)
	}

	// 所有賠線中獎規則
	lineRewareMap := make(map[uint32]*slot.PayResult)

	// 依照連線個數做排序，多到少
	sort.Slice(logic.GamePayResult, func(i, j int) bool {
		return logic.GamePayResult[i].PayCount > logic.GamePayResult[j].PayCount
	})

	// 過濾已經出現過的連線，並取贏多的那筆規則
	for _, result := range logic.GamePayResult {
		if !result.IsMatch || result.Rule == nil {
			continue
		}
		if result.Rule.GetPayType() != slot.PayTypeChip.String() {
			continue
		}

		for _, line := range result.SymbolPayLines {

			if _, ok := lineRewareMap[line.PayLine.Origin.GetLineId()]; !ok {
				lineRewareMap[line.PayLine.Origin.GetLineId()] = result
				continue
			}

			f1, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			reware1 := int64(f1)

			f2, _ := strconv.ParseFloat(lineRewareMap[line.PayLine.Origin.GetLineId()].Rule.GetPayout(), 64)
			reware2 := int64(f2)

			if reware1 > reware2 {
				lineRewareMap[line.PayLine.Origin.GetLineId()].RemoveLine(line.PayLine.Origin.GetLineId())
			} else {
				result.RemoveLine(line.PayLine.Origin.GetLineId())
			}
		}
	}

	tmp := make([]*slot.PayResult, 0, len(logic.GamePayResult))
	// 整理沒有連線(被移除後)的規則
	for _, result := range logic.GamePayResult {
		if !result.IsMatch || result.Rule == nil {
			continue
		}
		if result.Rule.GetPayType() != slot.PayTypeChip.String() {
			tmp = append(tmp, result)
			continue
		}
		if result.WayCount > 0 {
			tmp = append(tmp, result)
		}
	}
	logic.GamePayResult = tmp

}

// CalcGameAward 計算遊戲獎勵
func (logic *Logic) CalcGameAward() {

	for _, result := range logic.GamePayResult {
		if !result.IsMatch || result.Rule == nil {
			continue
		}

		// 是否贏得額外遊戲
		if result.IsWinBonusGame() {
			// 取得額外遊戲的GroupID
			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 32)

			var pickgame *protodata.PickGameTable

			for _, row := range logic.PickGameTable {
				if row.GetId() == uint32(f) {
					pickgame = row
					break
				}
			}

			if pickgame != nil {
				logic.GameWinBonusGame = true

				var bonusTables []*protodata.BonusTable
				for _, row := range logic.BonusTable {
					if row.GetGroupId() != int32(pickgame.GetRewardValue()) {
						continue
					}
					bonusTables = append(bonusTables, row)
				}

				logic.BonusGame = NewBonusGame(bonusTables, logic.BonusGameSize, int32(pickgame.GetPickCount()), logic.GameBetMoney)
			}

			continue
		}

		// 是否贏得Jackpot
		if result.IsWinJackpot() {
			jackpot := logic.Jackpots[slot.JackpotSingle]

			win := jackpot.GetCurrentPoolByBet(logic.GameBetMoney, int64(logic.MaxBetCount))

			logic.GameWinMoney += win
			logic.GameWinJackpot = win

			jackpot.AwardTotal += win

			var paylines []string
			for _, line := range result.SymbolPayLines {
				paylines = append(paylines, strconv.Itoa(int(line.PayLine.Origin.GetLineId()))+" : "+line.PayLine.Origin.GetContent())
			}
			logic.GameLog.AddGameStep("贏得Jackpot; 金額: %d; 連線賠線: %s", win, strings.Join(paylines, " | "))
			// fmt.Printf("贏得Jackpot; 金額: %d; 連線賠線: %s\n", win, strings.Join(paylines, " | "))

			if jackpot.IsNeedReset() {
				logic.UpdateJackpotPool(0, -jackpot.TotalContribution, -jackpot.AwardTotal+win, jackpot.Level)
				jackpot.Reset()
			} else {
				logic.UpdateJackpotPool(0, 0, win, jackpot.Level)
			}

			continue
		}

		// 計算贏得的金額
		_, win := result.CalcWinMoney(logic.GameBetMoney, 1)
		logic.GameWinMoney += win
		var paylines []string
		for _, line := range result.SymbolPayLines {
			paylines = append(paylines, strconv.Itoa(int(line.PayLine.Origin.GetLineId()))+" : "+line.PayLine.Origin.GetContent())
		}
		logic.GameLog.AddGameStep("開獎符號: %s, 符號連續 %d 個; 符號賠率: %d; 連線數總和: %d; 連線賠線: %s", result.SymbolName, result.PayCount, (win / int64(result.WayCount) / logic.GameBetMoney), result.WayCount, strings.Join(paylines, " | "))
		// fmt.Printf("開獎符號: %s, 符號連續 %d 個; 符號賠率: %d; 連線數總和: %d; 連線賠線: %s\n", result.SymbolName, result.PayCount, (win / int64(result.WayCount) / logic.GameBetMoney), result.WayCount, strings.Join(paylines, " | "))

	}

}

// CalcFlowAmount 計算流水，增加本次異動值(贏的錢-花的錢)，使用禮碼不累計
func (logic *Logic) CalcFlowAmount(user *gamelogic.GameUser) {

	var diff float64

	diff = float64(logic.GameWinMoney) - float64(logic.GamePayMoney)

	user.FlowAmount += int64(math.Abs(diff))
}
