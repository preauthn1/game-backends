package task

import (
	"example.invalid/golang/protobuf/proto"
	"module"
	"msg"
	"netproto"
	"network"
	"testing"
)

func TestSuperToolFishVIPUserHandler(t *testing.T) {
	initDBConnection()

	msgdata := new(netproto.SuperToolFishVIPUserMsg)
	msgdata.GameID = proto.Int32(25)
	msgdata.UserID = proto.Int32(0)
	msgdata.CompleteAmount = proto.Int32(0)

	msgData, _ := proto.Marshal(msgdata)

	m := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_SuperToolFishVIPUserID), msgData)
	handler := new(SuperToolFishVIPUserHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_NewByDBServer) || ret[0].SClassID != int32(netproto.NewBYDBServerClassID_SuperToolFishVIPUserRetID) {
		t.Error("OnMessage 錯誤")
	}

	t.Log("查看回傳訊息", ret[0].MsgData.(*netproto.SuperToolFishGetVIPUserRewardDetail).GetFishVIPUserList())
}
