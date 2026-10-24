package task

import (
	"conf"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"msg"
	"netproto"
	"testing"
	"time"
)

func TestLoop(t *testing.T) {
	initDBConnection()
	conf.CLogLevel = 0
	for i:=0; i<10000; i++ {
		mlog.System("times [%v]", i)
		TestUserLogoutFishHandler(t)
		time.Sleep(time.Second *1)
	}
}

func TestUserLogoutFishHandler(t *testing.T) {
	msgdata := new(netproto.UserLogoutFishMsg)
	msgdata.MsgID = proto.Int32(1)
	msgdata.UserID = proto.Int32(0)
	msgdata.UpdateAmount = proto.Int32(0)
	msgdata.TaxAmount = proto.Int32(0)
	msgdata.WinCount = proto.Int32(0)
	msgdata.LoseCount = proto.Int32(0)
	msgdata.DrawCount = proto.Int32(0)
	msgdata.LoginTime = proto.String("")
	msgdata.LogoutTime = proto.String("")
	msgdata.IPAddress = proto.String("")
	msgdata.HDCode = proto.String("")
	msgdata.HDType = proto.Int32(0)
	msgdata.GameTime = proto.Int32(0)
	msgdata.GameID = proto.Int32(0)
	msgdata.ServerID = proto.Int32(0)
	msgdata.LoginMoney = proto.Int64(0)
	msgdata.ScoreUpdateMode = proto.Int32(0)
	msgdata.TradeID = proto.Int64(0)
	msgdata.LastWinLose = proto.Int32(0)
	msgdata.FlowAmount = proto.Int32(0)
	msgdata.CouponAmount = proto.Int32(0)
	msgData, _ := proto.Marshal(msgdata)
	m := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_UserLogoutFishID), msgData)
	handler := new(UserLogoutFishHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	//ctx := new(module.MouduleCtx)
	//clt := new(network.ConnInfo)
	//ret := handler.OnMessage(ctx, clt, m)
	//t.Log("查看回傳訊息", ret[0].MsgData.(*netproto.NewByDBRegMsg).GetChvMsg())
}
