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

// 裂變3說明
func TestSplitGetRewardTipsHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.SplitGetRewardTipsMsg)
	msgdata.UserID = proto.Int32(56022519)

	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_SplitGetRewardTipsID), rpcMessage)
	handler := new(SplitGetRewardTipsHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_Hall) || ret[0].SClassID != int32(netproto.HallMsgClassID_SplitGetRewardTipsRetID) {
		t.Error("OnMessage 錯誤")
	}

}
