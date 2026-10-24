package slot

import (
	"strconv"

	"example.invalid/server/pkg/slot/protodata"
)

// PayResult 比對賠付結果
type PayResult struct {
	IsMatch        bool                    // 是否比對成功
	WayCount       int32                   // 連線數量
	PayCount       int32                   // 共幾個Reel
	SymbolName     string                  // 符號
	SymbolIDs      []int32                 // 中獎的符號ID
	SymbolPayLines []*SymbolPayLine        // 中獎的連線與符號
	Rule           *protodata.SlotPayTable // 中獎的規則
}

// SymbolPayLine 中獎的連線與符號
type SymbolPayLine struct {
	SymbolIDs []int32  // 中獎的符號ID
	PayLine   *PayLine // 中獎的連線
}

func (line *SymbolPayLine) String() string {
	return line.PayLine.String()
}

// GetPayType 取得中獎項目
func (result *PayResult) GetPayType() PayType {
	return PayType(result.Rule.GetPayType())
}

// CalcWinMoney 計算贏得的金額
func (result *PayResult) CalcWinMoney(betMoney int64) int64 {
	if !result.IsMatch || result.Rule == nil {
		return 0
	}

	switch result.Rule.GetPayType() {
	default:
		return 0
	case PayTypeChip.String():
		i, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)
		return betMoney * int64(i) * int64(result.WayCount)
	}
}

// IsWinBonusGame 是否贏得BonusGame
func (result *PayResult) IsWinBonusGame() bool {
	if !result.IsMatch || result.Rule == nil {
		return false
	}

	switch result.Rule.GetPayType() {
	default:
		return false
	case PayTypePickGame.String():
		return true
	}
}

// IsWinJackpot 是否贏得Jackpot
func (result *PayResult) IsWinJackpot() bool {
	if !result.IsMatch || result.Rule == nil {
		return false
	}

	switch result.Rule.GetPayType() {
	default:
		return false
	case PayTypeJackpot.String():
		return true
	}
}

// RemoveLine 移除既有的線
func (result *PayResult) RemoveLine(lineID uint32) {
	if !result.IsMatch || result.Rule == nil {
		return
	}

	if len(result.SymbolPayLines) == 0 {
		return
	}

	var tmp []*SymbolPayLine
	for _, line := range result.SymbolPayLines {
		if lineID == line.PayLine.Origin.GetLineId() {
			continue
		}
		tmp = append(tmp, line)
	}

	result.SymbolPayLines = tmp
	result.WayCount = int32(len(result.SymbolPayLines))
}
