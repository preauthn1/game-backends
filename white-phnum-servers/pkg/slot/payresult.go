package slot

import (
	"strconv"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// PayResult 比對賠付結果
type PayResult struct {
	IsMatch        bool                    // 是否比對成功
	HasWild        bool                    // 是否有使用百搭
	WayCount       int32                   // 連線數量
	PayCount       int32                   // 共幾個Reel
	Multi          int32                   // 加乘倍數
	SymbolName     string                  // 符號
	SymbolIDs      []int32                 // 中獎的符號ID
	BlinkPositions []Position              // 中獎符號位置
	SymbolPayLines []*SymbolPayLine        // 中獎的連線與符號
	Rule           *protodata.SlotPayTable // 中獎的規則
}

// SymbolPayLine 中獎的連線與符號
type SymbolPayLine struct {
	HasWild      bool     // 是否有使用百搭
	SymbolIDs    []int32  // 中獎的符號ID
	PayLine      *PayLine // 中獎的連線
	SymbolNames  []string // 中獎的符號
	MultipleStep []int32  // 加乘階段 w2 w3 = > [2, 3]
}

func (line *SymbolPayLine) String() string {
	return line.PayLine.String()
}

// GetMultipleValue 取得加乘倍數
func (line *SymbolPayLine) GetMultipleValue() int32 {
	var multiVal int32
	for _, multi := range line.MultipleStep {
		multiVal += multi
	}
	if multiVal == 0 {
		multiVal = 1
	}
	return multiVal
}

// GetPayType 取得中獎項目
func (result *PayResult) GetPayType() PayType {
	return PayType(result.Rule.GetPayType())
}

// CalcWinMoney 計算贏得的金額
func (result *PayResult) CalcWinMoney(betMoney, betRate int64) int64 {
	if !result.IsMatch || result.Rule == nil {
		return 0
	}

	switch result.GetPayType() {
	default:
		return 0
	case PayTypeChip:
		f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

		var win int64
		if len(result.SymbolPayLines) > 0 {
			for _, line := range result.SymbolPayLines {
				win += betMoney * int64(f) * int64(line.GetMultipleValue())
			}
		} else {
			win = betMoney * int64(f) * int64(result.WayCount) * int64(result.Multi)
		}

		return win

	case PayTypeChipXRate:
		f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

		var win int64
		if len(result.SymbolPayLines) > 0 {
			for _, line := range result.SymbolPayLines {
				win += betMoney * int64(f) * int64(line.GetMultipleValue())
			}
		} else {
			win = betMoney * betRate * int64(f) * int64(result.WayCount) * int64(result.Multi)
		}

		return win
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
