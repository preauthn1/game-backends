package jjbx

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// ActionPickBonus 玩家選擇 Bonus Icon
func (logic *Logic) ActionPickBonus(user *gamelogic.GameUser, gmsg *msg.Message) {

	if logic.JackpotGame == nil {
		mlog.Error("沒有JackpotGame")
		return
	}

	detail := &netproto.Slot_MSG_PickBonus{
		Record: &netproto.Slot_BonusGameRecord{},
	}
	if err := proto.Unmarshal(gmsg.MsgData.([]byte), detail); err != nil {
		mlog.Error("[玩家選擇Jackpot Icon] 接收玩家封包解析錯誤. %v", err)
		return
	}

	logic.JackpotGame.Choose(detail.Record.GetID(), detail.Record.GetLocation(), int32(detail.Record.GetRewareValue()))
	logic.GameLog.AddGameStep("用户 %d 選擇第: %d次; 選擇位置: %d, 圖案: %d", user.UserID, detail.Record.GetID(), detail.Record.GetLocation(), detail.Record.GetRewareValue())
	mlog.Debug("用户 %d 選擇第: %d次; 選擇位置: %d, 圖案: %d", user.UserID, detail.Record.GetID(), detail.Record.GetLocation(), detail.Record.GetRewareValue())

	// 返還用戶傳送的資訊
	logic.SendRecvBonusGameRecord(user, detail)

	if !logic.JackpotGame.IsFinished() {
		// 更新DB
		logic.RecordJackpotGame(logic.GameUser)
		return
	}

	logic.SendJackpotMessageToUser(user)

	// 結束遊戲
	logic.GamePayMoney = 0
	logic.GameWinMoney = logic.JackpotGame.WinMoney
	logic.GameWinJackpot = logic.JackpotGame.WinMoney

	logic.GameLog.AddGameStep("JackpotGame圖案順序: %s", logic.JackpotGame.GetProcessData())
	logic.GameLog.AddGameStep("JackpotGame獎金: %d", logic.GameWinMoney)

	mlog.Debug("JackpotGame圖案順序: %s", logic.JackpotGame.GetProcessData())
	mlog.Debug("JackpotGame獎金: %d", logic.GameWinMoney)

	logic.AddSlotGameHistoryLog(user, slot.HistoryTypeJPNOLINK, slot.GameTypeOPEN, logic.JackpotGame.GetProcessData())

	logic.RecordJackpotGame(logic.GameUser)

	// 調整狀態為等待
	logic.UpdateState(slot.GameStateWait)

	if logic.FreeSpin != nil {
		logic.UpdateFreeSpin(user)
	}

	// 寫分
	logic.UpdateUserScore(user)

	// 計算流水
	logic.CalcFlowAmount(user)

	logic.RestJackpotGame(user)

	// 重置 JackpotGame
	logic.JackpotGame = nil

	logic.GameEndLog()

	logic.GameEndFunc()

	return

}
