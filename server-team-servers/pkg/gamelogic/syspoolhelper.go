package gamelogic

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

// 水池模式
const (
	SysPoolModeNone   int = iota // 0 無水池
	SysPoolModeSingle            // 1 單機不寫檔模式, 重開服務水池就從贏錢線開始
	SysPoolModeDB                // 2 資料庫模式
)

// 水池吃稅
const (
	SysPoolDeductTaxNone int = iota // 0 不吃稅
	SysPoolDeductTax                // 1 吃稅
)

// SysPoolHelper ?
type SysPoolHelper struct {
	glc            *LogicBase
	cfgs           *config.Group
	mode           int
	ctrlRate       int   // 水池低於贏錢線開系統有利獎的機率
	pool           int64 // 水池
	winLine        int64 // 贏錢線
	deductTax      int   // 水池吃稅模式
	autoDeductRate int   // 自動吃水比例
}

// NewSysPoolHelper ?
func NewSysPoolHelper(glc *LogicBase, cfgs *config.Group) *SysPoolHelper {
	h := new(SysPoolHelper)
	h.glc = glc
	h.cfgs = cfgs
	if cfgs != nil {
		gameServerConfig := cfgs.GetConfig("gameserver")
		h.mode = gameServerConfig.GetConfigInt("gameserver", "syspoolmode")
		h.ctrlRate = gameServerConfig.GetConfigInt("gameserver", "syspoolctrlrate")
		h.winLine = int64(gameServerConfig.GetConfigInt("gameserver", "winline"))
		h.pool = int64(gameServerConfig.GetConfigInt("gameserver", "winline"))
		h.deductTax = gameServerConfig.GetConfigInt("gameserver", "syspooldeducttax")
		h.autoDeductRate = gameServerConfig.GetConfigInt("gameserver", "syspoolautodeductrate")
	} else {
		h.mode = SysPoolModeNone
		h.ctrlRate = 0
		h.winLine = 0
		h.pool = 0
		h.deductTax = 0
		h.autoDeductRate = 0
	}

	return h
}

// GetMode ?
func (h *SysPoolHelper) GetMode() int {
	return h.mode
}

// GetPool 取得系統水池
func (h *SysPoolHelper) GetPool() int64 {
	if h.mode == SysPoolModeDB {
		return h.glc.GetSuperToolService().GetSuperToolHandler().GetSysPool()
	} else if h.mode == SysPoolModeSingle {
		return h.pool
	}
	return 0
}

// GetWinLine 取得贏錢線
func (h *SysPoolHelper) GetWinLine() int64 {
	if h.mode == SysPoolModeDB {
		return h.glc.GetSuperToolService().GetSuperToolHandler().GetWinLine()
	} else if h.mode == SysPoolModeSingle {
		return h.winLine
	}
	return 0
}

// UpdateSysPool 更新系統水池 *deprecated
func (h *SysPoolHelper) UpdateSysPool(updatePool int64) {
	example.invalid("水池異動[%d] 原水池[%d] 異動[%d] 更新後水池[%d]", h.mode, h.GetPool(), updatePool, h.GetPool()+updatePool)
	if h.mode == SysPoolModeDB {
		h.glc.GetSuperToolService().GetSuperToolHandler().UpdateSysPool(updatePool)
	} else if h.mode == SysPoolModeSingle {
		h.pool += updatePool
	}
}

// UpdateSysPool 更新系統水池 updatePool 基本異動金額 tax 稅金 ValidAmount 有效投注額
func (h *SysPoolHelper) UpdateSysPoolAndTax(updatePool int64, tax int64, ValidAmount int64) (log string) {
	autoDeductAmount := int64(0)
	changePoolAmount := updatePool
	if h.deductTax == SysPoolDeductTax {
		changePoolAmount -= tax
	}
	if h.autoDeductRate > 0 {
		autoDeductAmount = ValidAmount * int64(h.autoDeductRate) / 1000
		changePoolAmount -= autoDeductAmount
	}
	log = fmt.Sprintf("基本異動[%d] 水池吃稅[%d][%d] 水池自動吃水比例[%d][%d] 水池模式[%d] 原水池[%d] 異動[%d] 更新後水池[%d]",
		updatePool, h.deductTax, -tax, h.autoDeductRate, -autoDeductAmount, h.mode, h.GetPool(), changePoolAmount, h.GetPool()+changePoolAmount)
	example.invalid(log)
	if h.mode == SysPoolModeDB {
		h.glc.GetSuperToolService().GetSuperToolHandler().UpdateSysPool(changePoolAmount)
	} else if h.mode == SysPoolModeSingle {
		h.pool += changePoolAmount
	}
	return
}

// IsCtrlSystemWin 判斷是否進行系統有利獎
func (h *SysPoolHelper) IsCtrlSystemWin() (ok bool) {
	if h.mode != SysPoolModeNone && h.GetPool() < h.GetWinLine() && util.IsChance(h.ctrlRate, 100) {
		ok = true
	}

	example.invalid("水池模式[%d] 水池[%d] 水線[%d] 有利系統機率[%d][%t]", h.mode, h.GetPool(), h.GetWinLine(), h.ctrlRate, ok)
	return
}

// ToString 顯示水池狀態
func (h *SysPoolHelper) ToString() string {
	ctrlRate := h.ctrlRate
	if h.mode == SysPoolModeNone {
		ctrlRate = 0
	}
	return fmt.Sprintf("水池模式[%d] 水池[%d] 水線[%d] 有利系統機率[%d]", h.mode, h.GetPool(), h.GetWinLine(), ctrlRate)
}
