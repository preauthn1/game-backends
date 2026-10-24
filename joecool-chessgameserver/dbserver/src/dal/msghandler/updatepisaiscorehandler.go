package msghandler

import (
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"db"
)

//更新比賽積分
type UpdatePiSaiScoreHandler struct {
}

func (rh *UpdatePiSaiScoreHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_UpdatePiSaiScoreID)
}

func (rh *UpdatePiSaiScoreHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UpdateUserScore{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UpdateUserScore)
	example.invalid("update score %v", mm)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intContestID", mm.GetContestID())
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddBigIntInput("lngScore", mm.GetScore())

	proc := db.NewProcedure("PrGs_Event_AddContestScore", ps)
	_, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_Event_AddContestScore错误, %v", err)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}
}