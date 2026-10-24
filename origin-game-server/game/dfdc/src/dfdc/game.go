package dfdc

import (
	"DataCfgSlot"
	"gamelogic"
	"math"
	"math/rand"
	"mlog"
	"strconv"
	"strings"
	"time"

	"example.invalid/golang/protobuf/proto"
)

// Bet 玩家下注
func (logic *Logic) Bet(user *gamelogic.GameUser, rate int32, money int64) bool {

	total := int64(rate) * money

	if logic.GameState != GameStateBet {
		mlog.Debug("[white] 非下注階段，不給玩家下注")
		logic.SendBetRetMessage(user, BetStateStepError, total)
		return false
	}

	if money == 0 && logic.FreeSpinCount == 0 {
		mlog.Debug("[white] 沒有Free Spin，下注不能0元")
		logic.SendBetRetMessage(user, BetStateMoneyNotEnough, total)
		return false
	}

	logic.GameBetRate = rate
	logic.GameBetMoney = money

	if logic.FreeSpinCount > 0 {
		logic.GameIsFree = true
		logic.GamePayMoney = 0
		logic.GamePayCoupon = 0
		logic.GameLog.AddGameStep("玩家 %d; FreeSpin 剩餘次數: %d", user.UserID, logic.FreeSpinCount)
		logic.FreeSpinCount--
		logic.UpdateFreeSpin(logic.GameUser)
	} else {
		if user.Coupon > 0 {
			if user.Coupon >= total {
				logic.GamePayCoupon = total
				logic.GamePayMoney = 0
			} else {
				logic.GamePayCoupon = user.Coupon
				logic.GamePayMoney = total - logic.GamePayCoupon
			}
			logic.GameIsCoupon = true
		} else {
			logic.GamePayMoney = total
			logic.GameIsCoupon = false
		}
	}

	// 檢查玩家金額
	if user.Score+user.Coupon < logic.GamePayMoney+logic.GamePayCoupon {
		logic.SendBetRetMessage(user, BetStateMoneyNotEnough, logic.GamePayMoney+logic.GamePayCoupon)
		return false
	}

	// 檢查最低下注金額
	if user.Score+user.Coupon < logic.MinBetMoney {
		logic.SendBetRetMessage(user, BetStateMoneyLessMin, logic.GamePayMoney+logic.GamePayCoupon)
		return false
	}

	logic.GameLog.AddGameStep("玩家 %d; 下注: %d; 選擇符號: %d; 金額: %d; 花費金額: %d; 花費禮碼: %d", user.UserID, money, rate, total, logic.GamePayMoney, logic.GamePayCoupon)

	// 告知玩家下注狀態
	logic.SendBetRetMessage(user, BetStateSuccess, logic.GamePayMoney+logic.GamePayCoupon)

	return true
}

// Spin 開始拉
func (logic *Logic) Spin(user *gamelogic.GameUser) {
	logic.GameState = GameStateAward
	logic.SendStateMessage(user)

	// 判斷當前開獎符號，決定使用哪個 SymbolGroupID
	if logic.GameIsFree {
		// 免費遊戲，找FreeSpinTable
		for _, row := range logic.FreeSpinTable {
			if row.GetGroupId() != logic.FreeSpinGroup {
				continue
			}
			// 下注倍率
			if row.GetGoldCondition() == ConditionRate {
				i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 32)
				if int32(i) == logic.GameBetRate {
					logic.GameSymbolGroupID = row.GetFreeSymbolGroupId()
					logic.GameReelNum = row.GetFreeReelNum()
					logic.GameRowNum = row.GetFreeRowNum()
					break
				}
			}

			// 金額數量
			if row.GetGoldCondition() == ConditionAmount {
				i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 64)
				if i == logic.GameBetMoney {
					logic.GameSymbolGroupID = row.GetFreeSymbolGroupId()
					logic.GameReelNum = row.GetFreeReelNum()
					logic.GameRowNum = row.GetFreeRowNum()
					break
				}
			}
		}

	} else {
		// 一般遊戲，找MainGameTable
		for _, row := range logic.MainGameTable {
			// 下注倍率
			if row.GetGoldCondition() == ConditionRate {
				i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 32)
				if int32(i) == logic.GameBetRate {
					logic.GameSymbolGroupID = row.GetSymbolGroupId()
					logic.GameReelNum = row.GetReelNum()
					logic.GameRowNum = row.GetRowNum()
					break
				}
			}

			// 金額數量
			if row.GetGoldCondition() == ConditionAmount {
				i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 64)
				if i == logic.GameBetMoney {
					logic.GameSymbolGroupID = row.GetSymbolGroupId()
					logic.GameReelNum = row.GetReelNum()
					logic.GameRowNum = row.GetRowNum()
					break
				}
			}
		}
	}

	// 累積Jackpot獎池
	if !logic.GameIsFree && !logic.GameIsCoupon {
		for _, jackpot := range logic.Jackpots {
			jackpot.Increase(int64(logic.GameBetRate) * logic.GameBetMoney)
			logic.GameLog.AddGameStep("水池 %d 增加(N/10000) N=%d , 累積獎金增加 %d, 當前最大出獎金額 %d", jackpot.Level.Int(), jackpot.TotalContribution, jackpot.AwardTotal, jackpot.GetCurrentPool())

			// 更新Jackpot
			logic.UpdateJackpotPool(0, jackpot.GetIncrease(int64(logic.GameBetRate)*logic.GameBetMoney), 0, jackpot.Level.Int())
		}
	}

	logic.SendJackpotPoolMessage(user)

	logic.GameLog.AddGameStep("此局群組ID: %d", logic.GameSymbolGroupID)

	// 產生遊戲結果
	logic.CreateGameResult()

	// 紀錄開獎符號
	for i, row := range logic.GameResult.GetSymbolString() {
		logic.GameLog.AddGameStep("第 %d 行出現符號: %s", i+1, strings.Join(row, ", "))
	}

	// 計算遊戲結果
	logic.CalcGameResult()

	// 計算遊戲獎勵
	logic.CalcGameAward()

	// 計算流水
	logic.CalcFlowAmount(user)

	// 紀錄
	if !logic.GameIsFree {
		logic.AddSlotGameHistoryLog(user, HistoryTypeMAIN, GameTypeREEL, logic.GameResult.Transfor().ToSymbolString())
	} else {
		logic.AddSlotGameHistoryLog(user, HistoryTypeFREE, GameTypeREEL, logic.GameResult.Transfor().ToSymbolString())
	}

	// 調整狀態為開獎動畫
	logic.GameState = GameStateAwardAction
	logic.SendStateMessage(user)

	// 傳送FreeSpin次數
	logic.SendFreeSpinCountMessage(user)

	// 判斷是否贏得BonusGame
	if logic.GameWinBonusGame && !logic.GameIsCoupon {
		logic.GameLog.AddGameStep("即將進入JackpotGame")

		var jackpots []*Jackpot
		for _, jackpot := range logic.Jackpots {
			jackpots = append(jackpots, jackpot)
		}

		logic.JackpotGame = NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, logic.GameBetRate, logic.GameBetMoney)

		logic.JackpotGame.Play()

		logic.GameLog.AddGameStep("JackpotGame獎金層級: %s", logic.JackpotGame.WinJackpot.Level)

		// 發送開獎內容
		logic.SendJackpotGameDetailMessage(user)

		logic.JackpotGame.WinJackpot.AwardTotal += logic.JackpotGame.WinMoney
		logic.FreeSpinWinMoney += logic.JackpotGame.WinMoney

		if logic.JackpotGame.WinJackpot.IsNeedReset() {
			logic.UpdateJackpotPool(0, -logic.JackpotGame.WinJackpot.TotalContribution, -logic.JackpotGame.WinJackpot.AwardTotal+logic.JackpotGame.WinMoney, logic.JackpotGame.WinJackpot.Level.Int())
		} else {
			logic.UpdateJackpotPool(0, 0, logic.JackpotGame.WinMoney, logic.JackpotGame.WinJackpot.Level.Int())
		}
	}

	// 傳送開獎結果
	logic.SendGameResultMessage(user)

	// 調整狀態為等待
	logic.GameState = GameStateWait
	logic.SendStateMessage(user)

	logic.RegisterAnimation(AnimationGameEnd, logic.EndActionTime, func() {
		// 寫分
		logic.UpdateUserScore(user)

		logic.GameEndFunc()

		logic.GameState = GameStateBet
		logic.SendStateMessage(user)
	})
}

// GetReelSymbol 整理Symbol -> map[reel] map[row] []Symbol = 每格可以開出的Symbol
//
// @Param grpID SymbolGroupID
//
// @Param symbols 所有Symbol
func (logic *Logic) GetReelSymbol(grpID uint32, symbols []*DataCfgSlot.SlotSymbolDistribution) map[uint32]map[uint32][]*DataCfgSlot.SlotSymbolDistribution {

	ans := make(map[uint32]map[uint32][]*DataCfgSlot.SlotSymbolDistribution)

	for _, symbol := range symbols {
		if grpID != symbol.GetGroupId() {
			continue
		}

		reel := symbol.GetReelNum()
		index := symbol.GetOrder()

		if ans[reel] == nil {
			ans[reel] = make(map[uint32][]*DataCfgSlot.SlotSymbolDistribution)
		}

		ans[reel][index] = append(ans[reel][index], symbol)
	}

	return ans
}

// RandomSymbol 依照權重隨機選擇一筆
//
// @Param symbols 該格可能會開出的Symbol
func (logic *Logic) RandomSymbol(symbols []*DataCfgSlot.SlotSymbolDistribution) *DataCfgSlot.SlotSymbolDistribution {

	if len(symbols) == 0 {
		return nil
	}

	var total int
	var split []int // 分水嶺

	// 計算總重量
	for _, symbol := range symbols {
		total += int(symbol.GetProportion())
		split = append(split, total)
	}

	if total == 0 {
		return nil
	}

	// 隨機產生該總重量
	rand.Seed(time.Now().UnixNano())
	random := rand.Intn(total)

	// 找出是哪個
	for i, row := range split {
		if random < row {
			return symbols[i]
		}
	}

	return nil
}

// CreateGameResult 產生遊戲結果
//
// 由Logic的GameSymbolGroupID，與SlotSymbolDistribution，決定每隔可能會開出的符號
//
// 再將每隔可能開出的符號去開隨機一種符號
//
// 最後將結果寫入Logic的GameResult
func (logic *Logic) CreateGameResult() {

	symbols := logic.GetReelSymbol(logic.GameSymbolGroupID, logic.SlotSymbolDistribution)

	var result GameResult

	for row := uint32(1); row <= logic.GameRowNum; row++ {
		rowsymbols := make([]*DataCfgSlot.SlotSymbolDistribution, 0, logic.GameRowNum)
		for reel := uint32(1); reel <= logic.GameReelNum; reel++ {
			symbol := logic.RandomSymbol(symbols[reel][row])
			rowsymbols = append(rowsymbols, symbol)
		}
		result = append(result, rowsymbols)
	}

	logic.GameResult = result
}

// CalcGameResult 計算遊戲結果
func (logic *Logic) CalcGameResult() {

	logic.GamePayResult = make([]*PayResult, 0)

	// 依照SlotPayTable檢查中了哪些
	// 分類
	var LeftGroup, RightGroup, AnyGroup, StaightGroup []*DataCfgSlot.SlotPayTable
	for _, rule := range logic.SlotPayTable {
		switch rule.GetLinkType() {
		case LinkTypeLeft.String():
			LeftGroup = append(LeftGroup, rule)
		case LinkTypeRight.String():
			RightGroup = append(RightGroup, rule)
		case LinkTypeAny.String():
			AnyGroup = append(AnyGroup, rule)
		case LinkTypeStraight.String():
			StaightGroup = append(StaightGroup, rule)
		}
	}

	// 先處理ANY
	for _, rule := range AnyGroup {
		var count int
		// 掃描整個GameResult有幾個對應的Symbol
		for _, row := range logic.GameResult {
			for _, symbol := range row {
				if symbol.GetSymbol() == rule.GetSymbol() {
					count++
				}
			}
		}
		if int32(count) == rule.GetCount() {
			// logic.GameAwardRule = append(logic.GameAwardRule, rule)
			result := new(PayResult)
			result.IsMatch = true
			result.Rule = rule

			logic.GamePayResult = append(logic.GamePayResult, result)
		}
	}

	gameresult := logic.GameResult
	// 翻轉GameResult以Reel為主
	if logic.GameReelNum != uint32(len(gameresult)) {
		gameresult = gameresult.Transfor()
	}
	for _, rule := range LeftGroup {
		match, result := logic.PaylineMatcher.IsMatch(gameresult, rule)
		if !match {
			continue
		}
		logic.GamePayResult = append(logic.GamePayResult, result)
	}

	gameresult = gameresult.Reverse()
	for _, rule := range RightGroup {
		match, result := logic.PaylineMatcher.IsMatch(gameresult, rule)
		if !match {
			continue
		}
		logic.GamePayResult = append(logic.GamePayResult, result)
	}

}

// CalcGameAward 計算遊戲獎勵
func (logic *Logic) CalcGameAward() {

	for _, result := range logic.GamePayResult {
		if !result.IsMatch || result.Rule == nil {
			continue
		}

		// 是否贏得免費遊戲
		if ok, freeGrpID := result.IsWinFreeSpin(); ok {

			// 找出正確的FreeSpinGroup
			for _, row := range logic.FreeSpinTable {
				if row.GetGroupId() != freeGrpID {
					continue
				}

				// 下注倍率
				if row.GetGoldCondition() == ConditionRate {
					i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 32)
					if int32(i) == logic.GameBetRate {
						logic.FreeSpinGroup = row.GetGroupId()
						logic.FreeSpinCount += int32(row.GetFreeSpinCount())
						logic.GameLog.AddGameStep("玩家贏得FreeSpin; 群組: %d, 次數: %d", row.GetGroupId(), row.GetFreeSpinCount())
						break
					}
				}

				// 金額數量
				if row.GetGoldCondition() == ConditionAmount {
					i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 64)
					if i == logic.GameBetMoney {
						logic.FreeSpinGroup = row.GetGroupId()
						logic.FreeSpinCount += int32(row.GetFreeSpinCount())
						logic.GameLog.AddGameStep("玩家贏得FreeSpin; 群組: %d, 次數: %d", row.GetGroupId(), row.GetFreeSpinCount())
						break
					}
				}
			}

			if logic.FreeSpinLimit != 0 && logic.FreeSpinLimit < logic.FreeSpinCount {
				logic.FreeSpinCount = logic.FreeSpinLimit
			}

			logic.GameWinFreeSpin = true

			logic.UpdateFreeSpin(logic.GameUser)

			continue
		}

		// 是否贏得額外遊戲
		if result.IsWinBonusGame() {

			i, _ := strconv.ParseInt(result.Rule.GetPayout(), 10, 64)

			// 骰出中BonusGame
			// 隨機產生該總重量
			rand.Seed(time.Now().UnixNano())
			random := rand.Intn(100000000) // 億分之一

			if int64(random) > i {
				continue
			}

			logic.GameWinBonusGame = true

			continue
		}

		// 計算贏得的金額
		win := result.CalcWinMoney(logic.GameBetRate, logic.GameBetMoney)
		logic.GameWinMoney += win
		logic.FreeSpinWinMoney += win
		logic.GameLog.AddGameStep("開獎符號: %s, 符號連續 %d 個; 符號賠率: %d; 連線數總和: %d", result.SymbolName, result.PayCount, (win / int64(result.WayCount) / logic.GameBetMoney), result.WayCount)

	}

	if logic.GameIsFree {
		logic.UpdateFreeSpin(logic.GameUser)
	}

}

// CalcFlowAmount 計算流水，增加本次異動值(贏的錢-花的錢)，使用禮碼不累計
func (logic *Logic) CalcFlowAmount(user *gamelogic.GameUser) {
	if logic.GameIsCoupon {
		return
	}

	var diff float64

	diff = float64(logic.GameWinMoney) - float64(logic.GamePayMoney)

	user.FlowAmount += int64(math.Abs(diff))
}

// UpgradeGoldSymbol 檢查是否可升級 (SlotSymbolDistribution表內已經換過符號，可不用換)
//
// 前置條件: logic要有GameBetRate, SymbolAttributes
func (logic *Logic) UpgradeGoldSymbol(symbol *DataCfgSlot.SlotSymbolDistribution) *DataCfgSlot.SlotSymbolDistribution {

	for _, row := range logic.SymbolAttributes {
		if row.GetIsGoldsymbol() == int32(1) {
			continue
		}

		// 下注倍率
		if row.GetGoldCondition() == ConditionRate {
			i, _ := strconv.ParseInt(row.GetConditionValue(), 10, 32)
			if int32(i) == logic.GameBetRate {
				symbol.Symbol = proto.String(row.GetReplaceGoldsymbol())
				break
			}
		}

		// 金額數量
		if row.GetGoldCondition() == ConditionAmount {

			break
		}
	}

	return symbol
}

// UpdateUserScore 更新玩家分數
func (logic *Logic) UpdateUserScore(user *gamelogic.GameUser) {

	if user.IsRobot() {
		return
	}

	logic.GameLog.AddGameStep("更新玩家金幣, 下注金幣: %d, 贏得獎金: %d", logic.GamePayMoney, logic.GameWinMoney)

	logic.UpdateUserScoreFunc(user.UserID, logic.GameWinMoney-logic.GamePayMoney, 0, -logic.GamePayCoupon)

	if logic.GameUser != nil {
		logic.GameLog.GameEnd(
			map[int32]int64{logic.GameUser.UserID: logic.GameWinMoney - logic.GamePayMoney},
			map[int32]int64{logic.GameUser.UserID: -logic.GamePayCoupon},
		)
	}

	logic.GameWinMoney = 0
	logic.GamePayMoney = 0
	logic.GamePayCoupon = 0
}
