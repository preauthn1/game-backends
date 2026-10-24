package baselogic

import (
	"mlog"
	"sort"
	"strconv"

	"example.invalid/server/pkg/slot"
)

// CalcGameResult 計算遊戲結果
func (logic *SlotLogicBase) CalcGameResult(grm *slot.GameResultManager) []*slot.PayResult {

	results := make([]*slot.PayResult, 0)

	// // 依照SlotPayTable檢查中了哪些
	for _, rule := range logic.SlotPayTable {
		match, result := logic.PaylineMatcher.IsMatch(grm, rule)
		if !match {
			continue
		}
		results = append(results, result)
	}

	// 所有賠線中獎規則
	lineRewareMap := make(map[slot.LinkType]map[uint32]*slot.PayResult)

	// 依照連線個數做排序，多到少
	sort.Slice(results, func(i, j int) bool {
		return results[i].PayCount > results[j].PayCount
	})

	// 過濾已經出現過的連線，並取贏多的那筆規則
	for _, result := range results {
		if !result.IsMatch || result.Rule == nil {
			continue
		}
		if result.Rule.GetPayType() != slot.PayTypeChip.String() {
			continue
		}

		for _, line := range result.SymbolPayLines {
			if _, ok := lineRewareMap[slot.LinkType(result.Rule.GetLinkType())]; !ok {
				lineRewareMap[slot.LinkType(result.Rule.GetLinkType())] = make(map[uint32]*slot.PayResult)
			}

			if _, ok := lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()]; !ok {
				lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()] = result
				continue
			}

			f1, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			reware1 := int64(f1)

			f2, _ := strconv.ParseFloat(lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()].Rule.GetPayout(), 64)
			reware2 := int64(f2)

			if reware1 > reware2 {
				lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()].RemoveLine(line.PayLine.Origin.GetLineId())
			} else {
				result.RemoveLine(line.PayLine.Origin.GetLineId())
			}
		}
	}

	tmp := make([]*slot.PayResult, 0, len(results))
	// 整理沒有連線(被移除後)的規則
	for _, result := range results {
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

	return tmp
}

// CalcGameAward 計算遊戲獎勵
func (logic *SlotLogicBase) CalcGameAward(results []*slot.PayResult, betMoney int64) int64 {

	var winMoney int64

	for _, result := range results {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		switch result.GetPayType() {
		case slot.PayTypeChip:
			// 計算贏得的金額
			win := result.CalcWinMoney(betMoney)
			winMoney += win

			logic.GameLog.AddGameStep("開獎符號: %s, 比對順序: %s, 符號連續 %d 個; 符號賠率: %d; 連線數總和: %d", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, (win / int64(result.WayCount) / betMoney), result.WayCount)
			mlog.Debug("開獎符號: %s, 比對順序: %s, 符號連續 %d 個; 符號賠率: %d; 連線數總和: %d", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, (win / int64(result.WayCount) / betMoney), result.WayCount)

			for _, line := range result.SymbolPayLines {
				logic.GameLog.AddGameStep("連線內容: %v", line.PayLine)
			}
		}
	}

	return winMoney

}
