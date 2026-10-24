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

	logic.GameLog.AddGameStep("JackpotGame[%s] 獎金: %d, 圖案順序: %s", logic.JackpotGame.WinJackpot.Level.String(), logic.GameWinMoney, logic.JackpotGame.GetProcessData())

	logic.AddSlotGameHistoryLog(user, slot.HistoryTypeJPNOLINK, slot.LogTypeOPEN, logic.JackpotGame.GetProcessData())

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
