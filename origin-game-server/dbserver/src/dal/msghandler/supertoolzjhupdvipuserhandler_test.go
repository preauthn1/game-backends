package msghandler

import (
	"testing"
	"netproto"
	"rpcmsg"
	"msg"
	"module"
	"network"
	"example.invalid/golang/protobuf/proto"
)

func TestSuperToolZjhUpdVIPUserHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.SuperToolZjhUpdVipUserData)
	msgdata.UserID = proto.Int32(1021470)
	msgdata.Finish = proto.Int64(0)
	msgdata.FinishNextWinLoseNum = proto.Int64(0)
	msgdata.NextTable = proto.Int32(0)
	msgdata.NextType = proto.Int64(0)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SuperToolZjhUpdVipUserID), rpcMessage)
	handler := new(SuperToolZjhUpdVIPUserHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
}