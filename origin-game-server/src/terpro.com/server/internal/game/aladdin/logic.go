package aladdin

import (
	"config"
	"event"
	"gamelogic"
	"mlog"
	"msg"
	"netproto"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/baselogic"
)

// Logic Aladdin
type Logic struct {
	*baselogic.SlotLogicBase

	StepResults []*StepResult // 每次轉完遊戲結果，理論上最多三次
}

// NewLogic create Aladdin game logic
func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Logic {
	logic := new(Logic)
	logic.SlotLogicBase = baselogic.NewSlotLogicBase(t, tfManager, cfgs, protodatas)
	logic.SlotLogicBase.GameMessageFunc = logic.GameMessageFunc
	logic.SlotLogicBase.OnRecvRPCMessageFunc = logic.OnRecvRPCMessageFunc

	logic.RegisterBeforeGameStartFunc(func() { logic.Clean() })
	logic.RegisterAfterGameStartFunc(func() {})
	logic.RegisterBeforeGameEndFunc(func() {})
	logic.RegisterAfterGameEndFunc(func() {})

	return logic
}

// UserEnter 玩家進入
func (logic *Logic) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {
	logic.SlotLogicBase.UserEnter(user, tableno, seatno)

	logic.SendSettingMessage(user)

	logic.UpdateState(slot.GameStateBet)
}

// GameMessageFunc on recived client message
func (logic *Logic) GameMessageFunc(user *gamelogic.GameUser, gmsg *msg.Message) {

	// 玩家Spin
	if gmsg.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID) {

		data := &netproto.Slot_MSG_Spin{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		logic.GameLog.GameBegin()

		ok, ret, money := logic.Bet(user, data.GetMoney())

		// 下注狀態
		logic.SendBetStatusMessageToUser(user, ret, money)

		if !ok {
			return
		}

		logic.GameLog.AddGameStep("玩家 %d; 下注: %d", user.UserID, money)
		mlog.Debug("玩家 %d; 下注: %d", user.UserID, money)

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

		logic.Clean()

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
