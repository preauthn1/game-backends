package msghandler

import (
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"db"
	"fmt"
)

type UpdateJackpotPoolHandler struct {
}

func (rh *UpdateJackpotPoolHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.LHJ_GameMessageClassID_LHJUpdateJackpotPoolID)
}

func (rh *UpdateJackpotPoolHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.LHJ_GetJackpotRet{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}
	mm := rmm.(*netproto.LHJ_GetJackpotRet)
	ps := db.NewSqlParameters()
	ps.AddIntInput("sintServerID", mm.GetServerID())
	ps.AddIntInput("intFlagID", mm.GetFlagID())
	ps.AddIntInput("intTableNo", mm.GetTableNo())
	ps.AddIntInput("intJackPotID", mm.GetJackpotID())
	ps.AddBigIntInput("intSpins", mm.GetSpinStack())
	ps.AddBigIntInput("lngPoolAmount", mm.GetPoolAmount())
	ps.AddBigIntInput("lngJPAmount", mm.GetJPAmountOut())

	proc := db.NewProcedure("PrGs_WildCat_JackpotPool_U", ps)
	_, err := WildCatDbUtil.ExecProc(proc)
	var retcode int32 = 1
	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrGs_WildCat_JackpotPool_U错误, %v", err)
		ctx.Error(strMsg)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}
}