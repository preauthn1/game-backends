package roulette

type FlowCalctor interface {
	Calc([]*Bet) (int64, error)
}

// LossOnlyFlowCalctor 輸分流水計算器
type LossOnlyFlowCalctor struct{}

// String
func (c LossOnlyFlowCalctor) String() string {
	return "輸分流水"
}

// Calc 計算流水
func (c LossOnlyFlowCalctor) Calc(bets []*Bet) (int64, error) {
	// 沒有贏分的下注，就是流水
	var flow int64

	for _, b := range bets {
		if b.Win == 0 {
			flow += b.Count
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
func (c EffectiveBetFlowCalctor) Calc(bets []*Bet) (int64, error) {
	// 只要有投注就是流水
	var flow int64

	for _, b := range bets {
		flow += b.Count
	}

	return flow, nil
}
