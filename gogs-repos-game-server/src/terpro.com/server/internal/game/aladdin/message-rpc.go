package aladdin

import (
	"gamelogic"
	"msg"
	"netproto"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/server/pkg/slot"
)

// AddSlotGameHistoryLogByStep 新增歷程紀錄
func (logic *Logic) AddSlotGameHistoryLogByStep(user *gamelogic.GameUser, step *StepResult) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotAddGameLogID)

	data := new(netproto.Slot_HistoryLogData)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	data.ServerID = proto.Int32(logic.TableManager.Room.ServerID)

	data.CurrentAmount = proto.Int64(user.Score)
	data.CurrentCoupon = proto.Int64(user.Coupon)
	data.BetMoney = proto.Int64(step.GamePayMoney)
	data.BetCoupon = proto.Int64(0)
	data.WinMoney = proto.Int64(step.GameWinMoney)
	data.FinalScore = proto.Int64(user.Score + step.GameWinMoney - step.GamePayMoney)
	data.FinalCoupon = proto.Int64(user.Coupon)

	var historyType int32
	if !step.IsRespin {
		historyType = slot.HistoryTypeMAIN
	} else {
		historyType = slot.HistoryTypeRESPIN
	}

	data.GameType = proto.Int32(historyType)
	data.LogType = proto.Int32(slot.GameTypeREEL)

	step.ExpandResult.ReelMode()
	data.GameData = proto.String(step.ExpandResult.ToSymbolString())

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}
