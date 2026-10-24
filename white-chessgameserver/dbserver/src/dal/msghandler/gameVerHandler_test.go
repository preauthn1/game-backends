package msghandler

import (
	"example.invalid/golang/protobuf/proto"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"testing"
)

func TestGameVerHandler_OnMessage(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(3) //bjl 3
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo
	gamever := new(netproto.GameVer)
	gamever.GameID = proto.Int32(3)
	gamever.ServerID = proto.Int32(10)

	msgData, _ := proto.Marshal(gamever)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_DBServer), int32(netproto.DBServerClassID_GameVerID), rpcMessage)
	handler := new(GameVerHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_DBServer) || ret[0].SClassID != int32(netproto.DBServerClassID_GameVerRetID) {
		t.Error("OnMessage 錯誤")
	}
}
