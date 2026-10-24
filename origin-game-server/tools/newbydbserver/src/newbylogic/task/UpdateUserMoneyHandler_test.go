package task

import (
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"testing"
)

func TestUpdateUserMoneyHandler(t *testing.T) {
	initDBConnection()

	for i:=0; i<1000; i++ {
		mlog.System("times [%v]", i)
		TestUpdateHandler(t)
	}
}

func TestUpdateHandler(t *testing.T) {
	msgdata := new(netproto.UpdateUserMoney)
	msgdata.MsgID = proto.Int32(10)
	msgdata.UserID = proto.Int32(1000003)
	msgdata.UpdateAmount = proto.Int32(1000)
	msgdata.TaxAmount = proto.Int32(0)
	msgdata.ServerID = proto.Int32(300)
	msgdata.CouponAmount = proto.Int32(0)
	msgData, _ := proto.Marshal(msgdata)
	m := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_UpdateUserMoneyID), msgData)
	handler := new(UpdateUserMoneyHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	//ret := handler.OnMessage(ctx, clt, m)
	////if ret[0].BClassID != int32(netproto.MessageBClassID_NewByDBServer) || ret[0].SClassID != int32(netproto.NewBYDBServerClassID_UpdateUserMoneyRetID) {
	//	t.Error("OnMessage 錯誤")
	//}
	//t.Log("查看回傳訊息", ret[0].MsgData.(*netproto.NewByDBRegMsg).GetChvMsg())
}