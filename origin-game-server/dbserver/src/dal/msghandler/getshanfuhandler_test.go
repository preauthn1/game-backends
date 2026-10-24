package msghandler

import (
	"testing"
	"netproto"
	"example.invalid/golang/protobuf/proto"
	"rpcmsg"
	"msg"
	"module"
	"network"
)

func TestGetShanFuHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.GetShanFuMsg)
	msgdata.UserID = proto.Int32(1000004)

	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetShanFuID), rpcMessage)
	handler := new(GetShanFuHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_Hall) || ret[0].SClassID != int32(netproto.HallMsgClassID_GetShanFuRetID) {
		t.Error("OnMessage 錯誤")
	}
}
