package baselogic

// IsFreeSpin 檢查目前是否為FreeSpin狀態
func (logic *SlotLogicBase) IsFreeSpin() bool {
	if logic.FreeSpin != nil && logic.FreeSpin.Count >= 0 && logic.FreeSpin.Used > 0 {
		return true
	}

	return false
}

// CheckFreeGame 檢查 FreeSpin, 檢查免費遊戲是否結束沒次數
func (logic *SlotLogicBase) CheckFreeGame() {
	if logic.FreeSpin == nil {
		return
	}

	if logic.FreeSpin.Count > 0 {
		return
	}

	logic.SendLeftFreeGameToUser(logic.GameUser)
	logic.FreeSpin = nil
}
