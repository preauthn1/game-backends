package aladdin

import (
	"gamelogic"
	"mlog"
	"reflect"
	"sort"
	"strconv"
	"strings"

	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
)

// Bet 判斷下注是否
func (logic *Logic) Bet(user *gamelogic.GameUser, money int64) (ok bool, retcode int32, betmoney int64) {

	logic.GameBetMoney = money
	logic.GamePayMoney = money * int64(len(logic.SlotPayLine))

	// 檢查狀態是否可下注
	if logic.GameState != slot.GameStateBet {
		return false, slot.BetStateStepError, logic.GamePayMoney
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
func (logic *Logic) Spin(stepno int, isRespin bool, symbolGroupID int32, prev *StepResult) {

	// 產生遊戲結果
	manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, logic.SlotSymbolDistribution)

	step := NewStepResult(stepno, isRespin, logic.GameBetMoney, logic.GamePayMoney, logic.SymbolAttributes, manager, prev)

	// 紀錄開獎符號
	logic.GameLog.AddGameStep("第 %d 次輪軸結果, 是否Respin: %t，使用SymbolGroupID: %d", step.Order, step.IsRespin, step.UnexpandResult.SymbolGroupID)
	mlog.Debug("第 %d 次輪軸結果, 是否Respin: %t，使用SymbolGroupID: %d", step.Order, step.IsRespin, step.UnexpandResult.SymbolGroupID)

	step.UnexpandResult.RowMode()
	for i, row := range step.UnexpandResult.GetSymbolString() {
		logic.GameLog.AddGameStep("第 %d 行: %s", i+1, strings.Join(row, ", "))
		mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
	}

	if step.IsExpand {
		logic.GameLog.AddGameStep("第 %d 軸出現拓展符號: %s，拓展後結果", step.ExpandReel, step.ExpandSymbol)
		mlog.Debug("第 %d 軸出現拓展符號: %s，拓展後結果", step.ExpandReel, step.ExpandSymbol)

		step.ExpandResult.RowMode()
		for i, row := range step.ExpandResult.GetSymbolString() {
			logic.GameLog.AddGameStep("第 %d 行: %s", i+1, strings.Join(row, ", "))
			mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
		}
	}

	// 計算遊戲結果
	logic.CalcGameResult(step)

	// 計算遊戲獎勵
	logic.CalcGameAward(step)

	logic.StepResults = append(logic.StepResults, step)

	sort.Slice(logic.StepResults, func(i, j int) bool {
		return logic.StepResults[i].Order < logic.StepResults[j].Order
	})
}

// CalcGameResult 計算遊戲結果
func (logic *Logic) CalcGameResult(step *StepResult) {

	step.GamePayResult = logic.SlotLogicBase.CalcGameResult(step.ExpandResult)

}

// CalcGameAward 計算遊戲獎勵
func (logic *Logic) CalcGameAward(step *StepResult) {

	step.GameWinMoney = logic.SlotLogicBase.CalcGameAward(step.GamePayResult, step.GameBetMoney)

	logic.AddSlotGameHistoryLogByStep(logic.GameUser, step)

	// 寫分
	logic.UpdateUserScore(logic.GameUser, step)

	logic.GameWinMoney += step.GameWinMoney

	for _, result := range step.GamePayResult {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		// 獲得再轉一次
		if result.GetPayType() == slot.PayTypeRespin {

			if len(step.ExpandReel) == 0 {
				continue
			}

			// 找出RespinTable對應的SymbolGroupID
			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			respinGroupID := int32(f)
			var respinTable *protodata.RespinTable

			for _, row := range logic.RespinTable {
				if row.GetGroupId() != respinGroupID {
					continue
				}

				var reels []int32

				reelstrs := strings.Split(row.GetAppearReel(), ",")
				for _, reelstr := range reelstrs {
					f, _ := strconv.ParseFloat(reelstr, 64)
					reels = append(reels, int32(f))
				}

				sort.Slice(reels, func(i, j int) bool {
					return reels[i] < reels[j]
				})

				if reflect.DeepEqual(reels, step.AllExpandReels) {
					respinTable = row
					break
				}
			}

			if respinTable != nil {
				logic.GameLog.AddGameStep("贏得Respin， %+v", respinTable)
				mlog.Debug("贏得Respin， %+v\n", respinTable)

				logic.Spin(step.Order+1, true, int32(respinTable.GetRespinValue()), step)
			}

			continue
		}

	}

}
