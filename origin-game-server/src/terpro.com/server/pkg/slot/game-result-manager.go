package slot

import (
	"fmt"
	"math/rand"

	"example.invalid/server/pkg/slot/protodata"
)

// GameResultManager 專門控制GameResult
type GameResultManager struct {
	GameResult    GameResult
	Reel          int32 // 共有多少直軸
	Row           int32 // 共有多少橫軸
	SymbolGroupID int32 // 此圖由哪個SymbolGroupID轉出
}

func (manager *GameResultManager) String() string {
	return fmt.Sprintf("Mode: %s\n%v", manager.Mode(), manager.GameResult.String())
}

// NewGameResultManager create GameResultManager
func NewGameResultManager(reel, row int32) (*GameResultManager, error) {
	if reel == 0 {
		return nil, fmt.Errorf("Reel number could not be 0")
	}
	if row == 0 {
		return nil, fmt.Errorf("Row number could not be 0")
	}

	manager := new(GameResultManager)
	manager.Reel = reel
	manager.Row = row

	return manager, nil
}

// CloneGameResultManager create GameResultManager
func CloneGameResultManager(origin *GameResultManager) *GameResultManager {

	manager := new(GameResultManager)
	manager.GameResult = origin.GameResult
	manager.Reel = origin.Reel
	manager.Row = origin.Row
	manager.SymbolGroupID = origin.SymbolGroupID

	return manager
}

// IsReelMode 目前第一維陣列為Reel組合
func (manager *GameResultManager) IsReelMode() bool {
	if len(manager.GameResult) == 0 {
		return false
	}
	return len(manager.GameResult) == int(manager.Reel) && len(manager.GameResult[0]) == int(manager.Row)
}

// IsRowMode 目前第一維陣列為Row組合
func (manager *GameResultManager) IsRowMode() bool {
	if len(manager.GameResult) == 0 {
		return false
	}
	return len(manager.GameResult) == int(manager.Row) && len(manager.GameResult[0]) == int(manager.Reel)
}

// Mode 取得目前模式
func (manager *GameResultManager) Mode() string {
	if manager.IsReelMode() {
		return "Reel"
	}

	if manager.IsRowMode() {
		return "Row"
	}

	return ""
}

// ReelMode 切換為ReelMode
func (manager *GameResultManager) ReelMode() {
	if len(manager.GameResult) == 0 {
		return
	}
	if manager.IsReelMode() {
		return
	}

	manager.GameResult = manager.GameResult.Transfor()
}

// RowMode 切換為RowMode
func (manager *GameResultManager) RowMode() {
	if len(manager.GameResult) == 0 {
		return
	}
	if manager.IsRowMode() {
		return
	}

	manager.GameResult = manager.GameResult.Transfor()
}

// Reverse 對調
func (manager *GameResultManager) Reverse() *GameResultManager {
	manager.GameResult = manager.GameResult.Reverse()
	return manager
}

// GenerateGameResult 產生指定GroupID的輪軸
func (manager *GameResultManager) GenerateGameResult(groupID int32, SymbolTable []*protodata.SlotSymbolDistribution) *GameResultManager {

	manager.SymbolGroupID = groupID

	result := make([][]*protodata.SlotSymbolDistribution, 0, manager.Reel)

	for reelNum := int32(1); reelNum <= manager.Reel; reelNum++ {
		rows := make([]*protodata.SlotSymbolDistribution, 0, manager.Row)
		for rowNum := int32(1); rowNum <= manager.Row; rowNum++ {
			symbols := manager.GetSymbolsByReelRow(groupID, reelNum, rowNum, SymbolTable)
			rows = append(rows, manager.randomSymbol(symbols))
		}
		result = append(result, rows)
	}

	manager.GameResult = result
	manager.RowMode()

	return manager
}

// GetSymbolsByReelRow 取得指定GroupID, ReelNum, RowNum的Symbols
func (manager *GameResultManager) GetSymbolsByReelRow(groupID, reelNum, rowNum int32, SymbolTable []*protodata.SlotSymbolDistribution) []*protodata.SlotSymbolDistribution {
	var symbols []*protodata.SlotSymbolDistribution

	for _, row := range SymbolTable {
		if groupID != int32(row.GetGroupId()) {
			continue
		}
		if reelNum != int32(row.GetReelNum()) {
			continue
		}
		if rowNum != int32(row.GetOrder()) {
			continue
		}
		symbols = append(symbols, row)
	}

	return symbols
}

// randomSymbol 依照權重隨機選擇一筆
//
// @Param symbols 該格可能會開出的Symbol
func (manager *GameResultManager) randomSymbol(symbols []*protodata.SlotSymbolDistribution) *protodata.SlotSymbolDistribution {

	if len(symbols) == 0 {
		return nil
	}

	var total int
	var split []int // 分水嶺

	// 計算總重量
	for _, symbol := range symbols {
		total += int(symbol.GetProportion())
		split = append(split, total)
	}

	if total == 0 {
		return nil
	}

	// 隨機產生該總重量
	random := rand.Intn(total)

	// 找出是哪個
	for i, row := range split {
		if random < row {
			return symbols[i]
		}
	}

	return nil
}

// GetSymbolString 取得符號二維陣列
func (manager *GameResultManager) GetSymbolString() [][]string {
	return manager.GameResult.GetSymbolString()
}

// ToSymbolString 取得 p1,p2,p3|p2,p3,p4|p3,p4,p5|p4,p5,p6|p5,p6,p7 字樣
func (manager *GameResultManager) ToSymbolString() string {
	return manager.GameResult.ToSymbolString()
}
