package msghandler

import (
	"example.invalid/golang/protobuf/proto"
)

import (
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

import (
	"testing"
)

func TestSlotGetGameLogHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(1000787)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.Slot_LoadHistoryData)
	msgdata.UserID = rpcInfo.UserID
	msgdata.GameID = proto.Int32(25)
	msgdata.StartTime = proto.String("2019-06-14")
	msgdata.EndTime = proto.String("2019-06-14")
	msgdata.PageIndex = proto.Int32(1)
	msgdata.PageSize = proto.Int32(10)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotGetGameLogID), rpcMessage)
	handler := new(SlotGetGameLogHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_Game) || ret[0].SClassID != int32(netproto.GameMessageClassID_SlotSendGameLogID) {
		t.Error("OnMessage 錯誤")
	}
}
