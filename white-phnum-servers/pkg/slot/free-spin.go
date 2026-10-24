package slot

import (
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// FreeSpin 免費遊戲
type FreeSpin struct {
	Origin        *protodata.FreeSpinTable            // 原始FreeSpinTable
	SymbolGroupID int32                               // 使用的SymbolGroup
	Symbols       []*protodata.SlotSymbolDistribution // 此次FreeSpin會用到的Symbols
	WinMultiple   []int32                             // FreeSpin中獎後會加乘
	Count         int                                 // 剩餘次數
	WinCount      int                                 // 累計中獎次數
	WinMoney      int64                               // 累計中獎金額
	BetMoney      int64                               // 下注金額
	BetRate       int64                               // 下注倍率
	IsCoupon      bool                                // 是否使用禮碼贏得
}

// NewFreeSpin 建立免費遊戲
func NewFreeSpin(origin *protodata.FreeSpinTable, symbols []*protodata.SlotSymbolDistribution, betMoney int64, betRate int64, isCoupon bool) *FreeSpin {

	freeSpin := new(FreeSpin)

	freeSpin.Origin = origin
	freeSpin.SymbolGroupID = int32(origin.GetFreeSymbolGroupId())

	for _, symbol := range symbols {
		if symbol.GetGroupId() != origin.GetFreeSymbolGroupId() {
			continue
		}

		freeSpin.Symbols = append(freeSpin.Symbols, symbol)
	}

	freeSpin.Count = int(origin.GetFreeSpinCount())
	freeSpin.BetMoney = betMoney
	freeSpin.BetRate = betRate
	freeSpin.IsCoupon = isCoupon

	if origin.GetMultiplierType() == "level" {

		multis := strings.Split(origin.GetMultiplierValue(), ",")

		for _, multi := range multis {
			f, err := strconv.ParseFloat(multi, 64)
			if err != nil {
				continue
			}
			freeSpin.WinMultiple = append(freeSpin.WinMultiple, int32(f))
		}
	}

	return freeSpin
}

// GetMultipleValue 取得目前中獎加乘倍率
func (freeSpin *FreeSpin) GetMultipleValue() int32 {

	length := len(freeSpin.WinMultiple)

	if length == 0 {
		return 1
	}

	if freeSpin.WinCount >= length {
		return freeSpin.WinMultiple[length-1]
	}

	return freeSpin.WinMultiple[freeSpin.WinCount]
}
