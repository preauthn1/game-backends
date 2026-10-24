package dfdc

import (
	"DataCfgSlot"
	"strings"
)

// PaylineAllMatcher 全線賠付比對器
type PaylineAllMatcher struct {
	Wilds []*DataCfgSlot.SymbolAttributes
}

// NewPaylineAllMatcher 建立全線賠付比對器，傳入所有SymbolAttributes讓Matcher抓出百搭設定
func NewPaylineAllMatcher(attrs []*DataCfgSlot.SymbolAttributes) *PaylineAllMatcher {
	var wilds []*DataCfgSlot.SymbolAttributes
	for _, attr := range attrs {
		if attr.GetIsWild() == int32(1) {
			wilds = append(wilds, attr)
		}
	}

	return &PaylineAllMatcher{
		Wilds: wilds,
	}
}

// IsMatch 比對GameResult是否符合PayTable規則
func (matcher *PaylineAllMatcher) IsMatch(result GameResult, rule *DataCfgSlot.SlotPayTable) (bool, *PayResult) {

	paylineres := new(PayResult)
	paylineres.SymbolName = rule.GetSymbol()

	reelCnts := make([]int, 0, len(result))

	for _, reel := range result {
		var repeat int

		for _, symbol := range reel {

			// 檢查符號相同
			if symbol.GetSymbol() == paylineres.SymbolName {
				repeat++
				paylineres.SymbolIDs = append(paylineres.SymbolIDs, int32(symbol.GetId()))
			}

			// 是否百搭
			for _, wild := range matcher.Wilds {
				if wild.GetSymbol() != symbol.GetSymbol() {
					continue
				}
				// 是否是例外
				isEx := false
				exts := strings.Split(wild.GetWildEx(), ",")
				for _, ext := range exts {
					if ext == paylineres.SymbolName {
						isEx = true
					}
				}
				// 不是例外則可搭
				if !isEx {
					repeat++
					paylineres.SymbolIDs = append(paylineres.SymbolIDs, int32(symbol.GetId()))
				}
			}
		}

		if repeat == 0 {
			break
		}

		paylineres.PayCount++
		reelCnts = append(reelCnts, repeat)
	}

	if paylineres.PayCount != rule.GetCount() {
		return false, paylineres
	}

	paylineres.IsMatch = true
	paylineres.WayCount = 1
	for _, cnt := range reelCnts {
		paylineres.WayCount *= int32(cnt)
	}
	paylineres.Rule = rule

	return true, paylineres
}
