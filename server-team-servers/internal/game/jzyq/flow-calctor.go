package jzyq

type FlowCalctor interface {
	Calc(BetSummary, WinSummary map[BetType]int64) (int64, error)
}

// LossOnlyFlowCalctor 輸分流水計算器
type LossOnlyFlowCalctor struct{}

// String
func (c LossOnlyFlowCalctor) String() string {
	return "輸分流水"
}

// Calc 計算流水
func (c LossOnlyFlowCalctor) Calc(BetSummary, WinSummary map[BetType]int64) (int64, error) {
	// 沒有贏分的下注，就是流水
	var flow int64

	for t, n := range BetSummary {
		if WinSummary[t] == 0 {
			flow += n
		}
	}

	return flow, nil
}

// EffectiveBetFlowCalctor 有效投注流水計算器
type EffectiveBetFlowCalctor struct{}

// String
func (c EffectiveBetFlowCalctor) String() string {
	return "有效投注流水"
}

// Calc 計算流水
func (c EffectiveBetFlowCalctor) Calc(BetSummary, WinSummary map[BetType]int64) (int64, error) {
	// 只要有投注就是流水
	var flow int64
	for _, v := range BetSummary {
		flow += v
	}

	return flow, nil
}
