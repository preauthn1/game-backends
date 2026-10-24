package dfdc

import (
	"DataCfgSlot"
	"strconv"
)

// PayResult 比對賠付結果
type PayResult struct {
	IsMatch    bool                      // 是否比對成功
	WayCount   int32                     // 連線數量
	PayCount   int32                     // 共幾個Reel
	SymbolName string                    // 符號
	SymbolIDs  []int32                   // 中獎的符號ID
	Rule       *DataCfgSlot.SlotPayTable // 中獎的規則
}

// CalcWinMoney 計算贏得的金額
func (result *PayResult) CalcWinMoney(betRate int32, betCount int64) int64 {
	if !result.IsMatch || result.Rule == nil {
		return 0
	}

	switch result.Rule.GetPayType() {
	default:
		return 0
	case PayTypeChip.String():
		i, _ := strconv.ParseInt(result.Rule.GetPayout(), 10, 64)
		return betCount * i * int64(result.WayCount)
	case PayTypeChipXRate.String():
		i, _ := strconv.ParseInt(result.Rule.GetPayout(), 10, 64)
		return betCount * i * int64(result.WayCount) * int64(betRate)
	}
}

// IsWinFreeSpin 是否贏得FreeSpin, 回傳FreeSpinTableGroupID
func (result *PayResult) IsWinFreeSpin() (bool, int32) {
	if !result.IsMatch || result.Rule == nil {
		return false, 0
	}

	switch result.Rule.GetPayType() {
	default:
		return false, 0
	case PayTypeFreeSpin.String():
		i, _ := strconv.ParseInt(result.Rule.GetPayout(), 10, 64)
		return true, int32(i)
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
	case PayTypeMysteryjackpot.String():
		return true
	}
}
