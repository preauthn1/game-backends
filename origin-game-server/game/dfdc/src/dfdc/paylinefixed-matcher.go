package dfdc

import (
	"DataCfgSlot"
)

// PaylineFixedMatcher 指定線賠付比對器
type PaylineFixedMatcher struct {
	Wilds []*DataCfgSlot.SymbolAttributes
	Lines []*DataCfgSlot.SlotPayLine
}

// NewPaylineFixedMatcher 建立指定線線賠付比對器，傳入所有SymbolAttributes讓Matcher抓出百搭設定
func NewPaylineFixedMatcher(attrs []*DataCfgSlot.SymbolAttributes, lines []*DataCfgSlot.SlotPayLine) *PaylineFixedMatcher {
	var wilds []*DataCfgSlot.SymbolAttributes
	for _, attr := range attrs {
		if attr.GetIsWild() == int32(1) {
			wilds = append(wilds, attr)
		}
	}

	return &PaylineFixedMatcher{
		Wilds: wilds,
		Lines: lines,
	}
}

// IsMatch 比對GameResult是否符合PayTable規則
func (matcher *PaylineFixedMatcher) IsMatch(result GameResult, rule *DataCfgSlot.SlotPayTable) (bool, *PayResult) {
	return false, nil
}
