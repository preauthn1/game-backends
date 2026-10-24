package msghandler

import (
	"fmt"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"testing"

	"example.invalid/golang/protobuf/proto"
)

func TestSlotFreeGameHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(9936738)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.Slot_FreeSpinRead)
	msgdata.GameID = proto.Int32(25)
	msgdata.FlagID = proto.Int32(2)
	msgdata.UserID = proto.Int32(9936738)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotFreeSpinRetID), rpcMessage)
	handler := new(SlotFreeGameHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_Game) || ret[0].SClassID != int32(netproto.GameMessageClassID_SlotFreeSpinDetailID) {
		t.Error("OnMessage 錯誤")
	}

	fmt.Println(ret[0].MsgData)
}

func TestSlotFreeGameRecordHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(1021470)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.Slot_FreeSpinDetail)
	msgdata.UserID = rpcInfo.UserID
	msgdata.GameID = proto.Int32(25)
	msgdata.FlagID = proto.Int32(2)
	msgdata.UserBet = proto.Int64(100)
	msgdata.FreeSpinGroup = proto.Int32(2)
	msgdata.Times = proto.Int32(10)
	msgdata.ChooseRate = proto.Int32(18)
	msgdata.WinMoney = proto.Int64(100)
	msgdata.IsCoupon = proto.Bool(true)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotFreeSpinID), rpcMessage)
	handler := new(SlotFreeGameHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	//if ret[0].BClassID != int32(netproto.MessageBClassID_Game) || ret[0].SClassID != int32(netproto.GameMessageClassID_SlotFreeSpinRetID) {
	//	t.Error("OnMessage 錯誤")
	//}
}
