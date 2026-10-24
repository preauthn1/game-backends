package msghandler

import (
	"db"
	"example.invalid/golang/protobuf/proto"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//写游戏记录详情
type SuperToolAddChangeLogHandler struct {
}

func (rh *SuperToolAddChangeLogHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_SuperToolAddChangeLogID)
}

func (rh *SuperToolAddChangeLogHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SuperToolAddChangeLog{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.SuperToolAddChangeLog)
	gameid := mm.GetGameID()
	serverid := rpcInfo.GetRouteServerID()
	tableno := mm.GetTableNo()
	ltype := mm.GetType()
	result := mm.GetResult()
	operator := mm.GetOperator()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intGameID", gameid)
	ps.AddIntInput("intServerID", serverid)
	ps.AddIntInput("intTableNo", tableno)
	ps.AddIntInput("intType", ltype)
	ps.AddVarcharInput("chvResult", result, -1)
	ps.AddVarcharInput("chvOperator", operator, 50)

	proc := db.NewProcedure("PrGs_SuperTool_AddChangeLog", ps)
	//mlog.System(proc.GetSqlString())
	_, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_SuperTool_AddChangeLog错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}

	return nil
}
