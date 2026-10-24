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

//取得AreaId的AreaConfig資訊
func TestGetAreaConfigHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.GetHallAreaMsg)
	msgdata.AreaID = proto.Int64(2)

	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_DBServer), int32(netproto.DBServerClassID_GetAreaConfigID), rpcMessage)
	handler := new(GetAreaConfigHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_DBServer) || ret[0].SClassID != int32(netproto.DBServerClassID_GetAreaConfigRetID) {
		t.Error("OnMessage 錯誤")
	}
}

