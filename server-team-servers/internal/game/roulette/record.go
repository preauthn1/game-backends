package roulette

type Record struct {
	RoundID int
	Award   uint32
	Bets    []*Bet // 下注統計
}
