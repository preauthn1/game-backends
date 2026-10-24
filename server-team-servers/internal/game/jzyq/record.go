package jzyq

type Record struct {
	RoundID    int
	Award      BetType
	BetSummary map[BetType]int64 // 下注統計
	WinSummary map[BetType]int64 // 輸贏統計
}
