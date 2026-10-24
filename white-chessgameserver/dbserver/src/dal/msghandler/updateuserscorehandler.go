package msghandler

import (
	"example.invalid/golang/protobuf/proto"
)

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

type UpdateUserScoreHandler struct {
}

func (rh *UpdateUserScoreHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_UpdateUserScoreID)
}

func (rh *UpdateUserScoreHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UpdateUserScore{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UpdateUserScore)
	example.invalid("update score %v", mm)

	msgid := rpc.GetQueueID()
	userid := mm.GetUserID()
	score := mm.GetScore()
	serverid := mm.GetServerID()
	taxamount := mm.GetTaxAmount()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intMsgID", msgid)
	ps.AddIntInput("intUserID", userid)
	ps.AddBigIntInput("lngUpdateAmount", score)
	ps.AddIntInput("intServerid", serverid)
	ps.AddBigIntInput("lngTaxAmount", taxamount)

	proc := db.NewProcedure("PrGs_UpdateUserMoney", ps)
	_, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("方舟 执行存储过程PrGs_UpdateUserMoney错误, %v", err)
		retcode = 0
	}

	_, err2 := CompanyDbUtil.ExecProc(proc)
	if err2 != nil {
		ctx.Error("Company1 执行存储过程PrGs_UpdateUserMoney错误, %v", err2)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}
}
