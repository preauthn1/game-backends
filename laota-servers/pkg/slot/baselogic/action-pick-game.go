package baselogic

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// ActionPickGame 玩家選擇 Pick Game Icon
func (logic *SlotLogicBase) ActionPickGame(user *gamelogic.GameUser, gmsg *msg.Message) {

	if logic.PickGame == nil {
		mlog.Error("沒有PickGame")
		return
	}

	detail := &netproto.Slot_MSG_PickBonus{
		Record: &netproto.Slot_BonusGameRecord{},
	}
	if err := proto.Unmarshal(gmsg.MsgData.([]byte), detail); err != nil {
		mlog.Error("[玩家選擇PickGame Icon] 接收玩家封包解析錯誤. %v", err)
		return
	}

	_, err := logic.PickGame.Choose(detail.Record.GetID(), detail.Record.GetLocation())
	if err != nil {
		mlog.Warn("[玩家選擇PickGame Icon] %v", err)
		return
	}

	// 返還用戶傳送的資訊
	logic.SendRecvBonusGameRecord(user, detail)

	if !logic.PickGame.IsFinished() {
		// 更新DB
		logic.RecordPickGame(logic.GameUser)
		return
	}

	// 結束遊戲
	logic.GamePayMoney = 0
	logic.GameWinMoney = logic.PickGame.WinMoney

	logic.GameLog.AddGameStep("PickGame 獎金: %d, 圖案順序: %s", logic.GameWinMoney, logic.PickGame.GetProcessData())

	logic.AddSlotGameHistoryLog(user, slot.HistoryTypeBONUS, slot.LogTypeOPEN, logic.PickGame.GetProcessData())

	// 調整狀態為等待
	logic.UpdateState(slot.GameStateWait)

	// 寫分
	logic.UpdateUserScore(user)

	// 計算流水
	logic.CalcFlowAmount(user)

	logic.ResetPickGame(user)

	// 重置 JackpotGame
	logic.PickGame = nil

	logic.GameEndLog()

	logic.GameEndFunc()

	return

}
