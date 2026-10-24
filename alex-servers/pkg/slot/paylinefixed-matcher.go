package slot

import (
	"sort"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// PaylineFixedMatcher 指定線賠付比對器
type PaylineFixedMatcher struct {
	*BasicPayLineMatcher
	NormalLine  []*PayLine
	ReverseLine []*PayLine
}

// NewPaylineFixedMatcher 建立指定線線賠付比對器，傳入所有SymbolAttributes讓Matcher抓出百搭設定
func NewPaylineFixedMatcher(attrs []*protodata.SymbolAttributes, lines []*protodata.SlotPayLine) *PaylineFixedMatcher {

	matcher := new(PaylineFixedMatcher)
	matcher.BasicPayLineMatcher = NewBasicPayLineMatcher(attrs)

	matcher.NormalLine = make([]*PayLine, 0, len(lines))
	matcher.ReverseLine = make([]*PayLine, 0, len(lines))

	for _, line := range lines {
		matcher.NormalLine = append(matcher.NormalLine, NewPayLine(line))
		matcher.ReverseLine = append(matcher.ReverseLine, NewPayLine(line).Reverse())
	}

	matcher.BasicPayLineMatcher.MatchLeft = matcher.MatchLeft
	matcher.BasicPayLineMatcher.MatchRight = matcher.MatchRight

	return matcher
}

func (matcher *PaylineFixedMatcher) match(result *GameResultManager, rule *protodata.SlotPayTable, lines []*PayLine) (bool, *PayResult) {
	result.ReelMode()

	paylineres := new(PayResult)
	paylineres.SymbolName = rule.GetSymbol()
	paylineres.Multi = 1

	symbolMap := make(map[string]bool)

	symbols := strings.Split(rule.GetSymbol(), ",")
	for _, symbol := range symbols {
		symbolMap[symbol] = true
	}

	for _, line := range lines {

		symbolPayLine := new(SymbolPayLine)
		symbolPayLine.PayLine = line

		var repeat int

		// 保險起見，比對前先把Position排序過
		sort.Slice(line.Positions, func(i, j int) bool {
			return line.Positions[i].Reel < line.Positions[j].Reel
		})

		// 檢查該位置是否為指定符號
		for _, position := range line.Positions {
			if position.Reel <= 0 || position.Row <= 0 {
				break
			}

			if result.Reel < position.Reel {
				break
			}

			if len(result.GameResult[position.Reel-1]) < int(position.Row) {
				break
			}

			symbol := result.GameResult[position.Reel-1][position.Row-1]

			var match bool

			for symbolName := range symbolMap {
				if symbol.GetSymbol() == symbolName {
					repeat++
					symbolPayLine.SymbolIDs = append(symbolPayLine.SymbolIDs, int32(symbol.GetId()))
					symbolPayLine.SymbolNames = append(symbolPayLine.SymbolNames, symbol.GetSymbol())
					match = true
					break
				}
			}

			if match {
				continue
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
					symbolPayLine.SymbolIDs = append(symbolPayLine.SymbolIDs, int32(symbol.GetId()))
					symbolPayLine.SymbolNames = append(symbolPayLine.SymbolNames, symbol.GetSymbol())
					match = true
					symbolPayLine.HasWild = true
				}
			}

			if !match {
				break
			}

		}

		// 檢查該線上不能全是百搭，當前規則符號本身是百搭則不需檢查
		var wildlock = true

		if len(matcher.Wilds) == 0 {
			wildlock = false
		}

		var isWild bool
		for _, wild := range matcher.Wilds {
			if wild.GetSymbol() == rule.GetSymbol() {
				isWild = true
				wildlock = false
				break
			}
		}

		if !isWild {
			for i := 0; i < repeat; i++ {
				position := line.Positions[i]
				symbol := result.GameResult[position.Reel-1][position.Row-1]

				var inWild bool
				// 是否百搭
				for _, wild := range matcher.Wilds {
					if wild.GetSymbol() == symbol.GetSymbol() {
						inWild = true
						break
					}
				}

				if !inWild {
					wildlock = false
					break
				}
			}

			// 都是百搭，規則符號是否也都是百搭
			if wildlock && len(symbolMap) > 1 {
				wildlock = false
			}

		}

		if repeat == int(rule.GetCount()) && !wildlock {

			for _, symbolName := range symbolPayLine.SymbolNames {
				for _, wild := range matcher.Wilds {
					if symbolName != wild.GetSymbol() {
						continue
					}
					if wild.GetMultiplier() > 1 {
						symbolPayLine.MultipleStep = append(symbolPayLine.MultipleStep, wild.GetMultiplier())
					}
				}
			}

			paylineres.SymbolPayLines = append(paylineres.SymbolPayLines, symbolPayLine)
		}
	}

	if len(paylineres.SymbolPayLines) == 0 {
		return false, paylineres
	}

	paylineres.IsMatch = true
	paylineres.Rule = rule
	paylineres.WayCount = int32(len(paylineres.SymbolPayLines))
	paylineres.PayCount = rule.GetCount()

	for _, row := range paylineres.SymbolPayLines {
		if row.HasWild {
			paylineres.HasWild = true
			break
		}
	}

	return true, paylineres
}

// MatchLeft 比對GameResult是否符合PayTable規則
func (matcher *PaylineFixedMatcher) MatchLeft(result *GameResultManager, rule *protodata.SlotPayTable) (bool, *PayResult) {
	result.ReelMode()
	return matcher.match(result, rule, matcher.NormalLine)
}

// MatchRight 比對GameResult是否符合PayTable規則
func (matcher *PaylineFixedMatcher) MatchRight(result *GameResultManager, rule *protodata.SlotPayTable) (bool, *PayResult) {
	// 最後要轉回來
	defer func() {
		result = result.Reverse()
	}()
	result.ReelMode()
	// 轉過去
	result = result.Reverse()
	return matcher.match(result, rule, matcher.ReverseLine)
}
