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

func TestUpdateExperienceScoreHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.UpdateUserScore)
	msgdata.UserID = proto.Int32(1021470)
	msgdata.Score = proto.Int64(0)
	msgdata.ServerID = proto.Int32(0)
	msgdata.TaxAmount = proto.Int64(0)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_DBServer), int32(netproto.DBServerClassID_UpdateExperenceScore), rpcMessage)
	handler := new(UpdateExperienceScoreHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
	t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
}