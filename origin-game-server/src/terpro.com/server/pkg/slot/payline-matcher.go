package slot

import (
	"sort"
	"strings"

	"example.invalid/server/pkg/slot/protodata"
)

// PaylineMatcher 賠付線比對器
type PaylineMatcher interface {
	IsMatch(*GameResultManager, *protodata.SlotPayTable) (bool, *PayResult)
}

// BasicPayLineMatcher 基本賠線比對器，實作 MatchAny，MatchStraight
type BasicPayLineMatcher struct {
	MatchLeft  func(*GameResultManager, *protodata.SlotPayTable) (bool, *PayResult)
	MatchRight func(*GameResultManager, *protodata.SlotPayTable) (bool, *PayResult)
}

// IsMatch 是否符合
func (m *BasicPayLineMatcher) IsMatch(result *GameResultManager, rule *protodata.SlotPayTable) (bool, *PayResult) {
	if result == nil || rule == nil {
		return false, nil
	}

	switch LinkType(rule.GetLinkType()) {
	case LinkTypeAny:
		return m.MatchAny(result, rule)
	case LinkTypeLeft:
		return m.MatchLeft(result, rule)
	case LinkTypeRight:
		return m.MatchRight(result, rule)
	case LinkTypeStraight:
		return m.MatchStraight(result, rule)
	}

	return false, nil
}

// MatchAny ANY 規則
func (m *BasicPayLineMatcher) MatchAny(result *GameResultManager, rule *protodata.SlotPayTable) (bool, *PayResult) {

	var count int
	var symbolIDs []int32
	// 掃描整個GameResult有幾個對應的Symbol
	for _, row := range result.GameResult {
		for _, symbol := range row {
			if symbol.GetSymbol() == rule.GetSymbol() {
				count++
				symbolIDs = append(symbolIDs, int32(symbol.GetId()))
			}
		}
	}

	sort.Slice(symbolIDs, func(i, j int) bool {
		return symbolIDs[i] < symbolIDs[j]
	})

	payresult := new(PayResult)

	if int32(count) == rule.GetCount() {
		payresult.IsMatch = true
		payresult.SymbolName = rule.GetSymbol()
		payresult.Rule = rule
		payresult.WayCount = 1
		payresult.PayCount = int32(count)
		payresult.SymbolIDs = symbolIDs
	}

	return payresult.IsMatch, payresult
}

// MatchStraight Straight 規則
func (m *BasicPayLineMatcher) MatchStraight(result *GameResultManager, rule *protodata.SlotPayTable) (bool, *PayResult) {

	result.ReelMode()

	var count int
	var symbolIDs []int32

	var symbolMap = make(map[string]bool)
	symbols := strings.Split(rule.GetSymbol(), ",")
	for _, symbol := range symbols {
		symbolMap[symbol] = true
	}

	// 掃描整個GameResult有幾個對應的Symbol
	for _, reel := range result.GameResult {
		var hasDiff bool
		for _, symbol := range reel {
			if val, ok := symbolMap[symbol.GetSymbol()]; !ok || !val {
				hasDiff = true
				break
			}
		}
		if hasDiff {
			continue
		}

		for _, symbol := range reel {
			symbolIDs = append(symbolIDs, int32(symbol.GetId()))
		}

		count++
	}

	sort.Slice(symbolIDs, func(i, j int) bool {
		return symbolIDs[i] < symbolIDs[j]
	})

	payresult := new(PayResult)

	if int32(count) == rule.GetCount() {
		payresult.IsMatch = true
		payresult.SymbolName = rule.GetSymbol()
		payresult.Rule = rule
		payresult.WayCount = 1
		payresult.PayCount = int32(count)
		payresult.SymbolIDs = symbolIDs
	}

	return payresult.IsMatch, payresult
}
