package msghandler

import (
	"testing"
	"netproto"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"msg"
	"module"
	"network"
)

func TestSuperToolUpdAssignRewardDetailHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.SuperToolAssignRewardDetail)
	msgdata.LogID = proto.Int32(0)
	msgdata.Idx = proto.Int32(0)
	msgdata.Description = proto.String("")
	msgdata.Reward = proto.String("")
	msgdata.Status = proto.Int32(0)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SuperToolUpdAssignRewardDetailID), rpcMessage)
	handler := new(SuperToolUpdAssignRewardDetailHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
}