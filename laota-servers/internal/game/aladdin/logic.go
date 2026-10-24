package aladdin

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
)

// Logic Aladdin
type Logic struct {
	*baselogic.SlotLogicBase

	StepResults []*StepResult // 每次轉完遊戲結果，理論上最多三次
}

// NewLogic create Aladdin game logic
func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Logic {
	logic := new(Logic)
	logic.SlotLogicBase = baselogic.NewSlotLogicBase(t, tfManager, cfgs, Protodatas)
	logic.SlotLogicBase.GameMessageFunc = logic.GameMessageFunc
	logic.SlotLogicBase.OnRecvRPCMessageFunc = logic.OnRecvRPCMessageFunc

	logic.RegisterBeforeGameStartFunc(func() {})
	logic.RegisterAfterGameStartFunc(func() {
		logic.Clean()
	})
	logic.RegisterBeforeGameEndFunc(func() {})
	logic.RegisterAfterGameEndFunc(func() {})

	return logic
}

// UserEnter 玩家進入
func (logic *Logic) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserEnter 發生錯誤, %v", rec)
		}
	}()

	logic.SlotLogicBase.UserEnter(user, tableno, seatno)

	go func() {
		logic.SendSettingMessage(user)
		logic.UpdateState(slot.GameStateBet)
	}()
}

// UserRecover 玩家斷線重連
func (logic *Logic) UserRecover(user *gamelogic.GameUser) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserRecover 發生錯誤, %v", rec)
		}
	}()

	logic.SlotLogicBase.UserRecover(user)

	go func() {
		logic.SendSettingMessage(user)
		logic.UpdateState(slot.GameStateBet)
	}()
}

// UserLeave 玩家離開
func (logic *Logic) UserLeave(user *gamelogic.GameUser) bool {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserLeave 發生錯誤, %v", rec)
		}
	}()

	logic.StepResults = make([]*StepResult, 0)

	return logic.SlotLogicBase.UserLeave(user)
}

// GameMessageFunc on recived client message
func (logic *Logic) GameMessageFunc(user *gamelogic.GameUser, gmsg *msg.Message) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("GameMessageFunc 發生錯誤, %v", rec)
		}
	}()

	// 玩家Spin
	if gmsg.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID) {

		data := &netproto.Slot_MSG_Spin{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		ok, ret, money := logic.Bet(user, data.GetMoney(), int64(logic.MaxBetRate))

		// 下注狀態
		logic.SendBetStatusMessageToUser(user, ret, money)

		if !ok {
			return
		}

		logic.UpdateState(slot.GameStateAward)

		// 開始轉
		logic.Spin(1, false, logic.DefaultSymbolGroupID, nil)

		// 計算流水
		logic.CalcFlowAmount(user)

		// 傳送開獎結果
		logic.SendGameResultMessage(user)

		// 調整狀態為等待
		logic.UpdateState(slot.GameStateWait)

		logic.GameEndLog()

		logic.GameEndFunc()

		logic.UpdateState(slot.GameStateBet)

		return
	}
}

// PiSaiMessage on recived client message
func (logic *Logic) PiSaiMessage(user *gamelogic.GameUser, gmsg *msg.Message) {}

// Clean 清除遊戲資料
func (logic *Logic) Clean() {
	logic.SlotLogicBase.Clean()

	logic.StepResults = make([]*StepResult, 0)
}
