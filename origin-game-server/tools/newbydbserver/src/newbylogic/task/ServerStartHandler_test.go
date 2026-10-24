package task

import (
	"example.invalid/golang/protobuf/proto"
	"module"
	"msg"
	"netproto"
	"network"
	"testing"
)

func TestServerStartHandler(t *testing.T) {
	initDBConnection()

	msgdata := new(netproto.NewBYServerStartMsg)
	msgdata.ServerID = proto.Int32(300)
	msgdata.GameID = proto.Int32(25)
	msgdata.ServerName = proto.String("newby")
	msgdata.ShowName = proto.String("newby")
	msgdata.ClientAddr = proto.String("0.0.0.0")
	msgdata.NotifyAddr = proto.String("0.0.0.0")
	msgdata.HttpAddr = proto.String("0.0.0.0")
	msgdata.LoginMoney = proto.Int32(100)

	msgData, _ := proto.Marshal(msgdata)

	m := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_ServerStartID), msgData)
	handler := new(NewBYServerStartHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_NewByDBServer) || ret[0].SClassID != int32(netproto.NewBYDBServerClassID_ServerStartRetID) {
		t.Error("OnMessage 錯誤")
	}

	t.Log("查看回傳訊息%v , %v", ret[0].MsgData.(*netproto.NewByDBRegMsg).GetCode(),
		ret[0].MsgData.(*netproto.NewByDBRegMsg).GetChvMsg())
}
