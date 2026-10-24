package msghandler

import (
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"testing"

	"example.invalid/golang/protobuf/proto"
)

func TestSlotAddGameLogHandler(t *testing.T) {
	initDBConnection()
	rpcInfo := netproto.CreateEmptyRPCInfo(1)
	rpcInfo.UserID = proto.Int32(1021470)
	rpcMessage := new(rpcmsg.RPCMessage)
	rpcMessage.RPCInfo = rpcInfo

	msgdata := new(netproto.Slot_HistoryLogData)
	msgdata.UserID = rpcInfo.UserID
	msgdata.GameID = proto.Int32(25)
	msgdata.BetMoney = proto.Int64(100)
	msgdata.BetCoupon = proto.Int64(40)
	msgdata.GameData = proto.String("p3,p11,p3,|p7,p9,p9,|p9,s1,p2,|w1,p9,p2,|p3,p3,s1,")
	msgdata.LogType = proto.Int32(1)
	msgdata.GameType = proto.Int32(1)
	msgdata.FinalScore = proto.Int64(200)
	msgdata.FinalCoupon = proto.Int64(40)
	msgdata.WinMoney = proto.Int64(100)
	msgdata.CurrentAmount = proto.Int64(100)
	msgdata.CurrentCoupon = proto.Int64(80)
	msgdata.ServerID = proto.Int32(367)
	msgData, _ := proto.Marshal(msgdata)
	rpcMessage.MsgData = msgData
	m := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotAddGameLogID), rpcMessage)
	handler := new(SlotAddGameLogHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}
	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	//if ret[0].BClassID != int32(netproto.MessageBClassID_Game) || ret[0].SClassID != int32(netproto.GameMessageClassID_SlotSendGameLogID) {
	//	t.Error("OnMessage 錯誤")
	//}
}
