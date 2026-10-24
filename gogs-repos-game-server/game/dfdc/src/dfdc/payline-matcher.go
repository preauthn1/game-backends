package dfdc

import (
	"DataCfgSlot"
)

// PaylineMatcher 賠付線比對器
type PaylineMatcher interface {
	IsMatch(GameResult, *DataCfgSlot.SlotPayTable) (bool, *PayResult)
}
