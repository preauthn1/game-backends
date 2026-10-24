package msghandler

import (
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"db"
	"util"
	"mlog"
)

//更新遊戲獎金池
type UpdateGameJackpotPoolHandler struct {
}

func (es *UpdateGameJackpotPoolHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_UpdateGameJackpotScore)
}

func (es *UpdateGameJackpotPoolHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UpdateUserScore{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UpdateUserScore)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intFlagID", 0)
	ps.AddBigIntInput("lngUpdateAmount", mm.GetScore())

	proc := db.NewProcedure("PrGs_UpdateQZNN_SystemPool", ps)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	_, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_UpdateQZNN_SystemPool错误, %v", err)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}
}
