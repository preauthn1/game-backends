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

func TestUpdateBeginnerOfferHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.FindFishBeginnerOfferByUserID)
	msgdata.UserID = proto.Int32(1021470)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_UpdateBeginnerOfferID), rpcMessage)
	handler := new(UpdateBeginnerOfferHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
}