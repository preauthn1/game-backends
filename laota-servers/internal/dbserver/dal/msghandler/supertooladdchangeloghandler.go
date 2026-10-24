package msghandler

import (
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/db/dbutil"
)

import (
	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
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
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
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
	_, err := dbutil.DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_SuperTool_AddChangeLog错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	}

	return []*msg.Message{dbutil.GetDBRetMsg(retcode)}

	// return nil
}
