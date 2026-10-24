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
type SuperToolUpdAssignRewardDetailHandler struct {
}

func (rh *SuperToolUpdAssignRewardDetailHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_SuperToolUpdAssignRewardDetailID)
}

func (rh *SuperToolUpdAssignRewardDetailHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SuperToolAssignRewardDetail{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.SuperToolAssignRewardDetail)
	logid := mm.GetLogID()
	idx := mm.GetIdx()
	reward := mm.GetReward()
	status := mm.GetStatus()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intLogID", logid)
	ps.AddIntInput("intIdx", idx)
	ps.AddVarcharInput("chvReward", reward, -1)
	ps.AddIntInput("intStatus", status)
	proc := db.NewProcedure("PrGs_SuperTool_UpdAssignRewardDetail", ps)
	//mlog.System(proc.GetSqlString())
	ret, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_SuperTool_UpdAssignRewardDetail错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	} else {
		retcode = int32(ret.GetReturnValue())
	}

	return []*msg.Message{GetDBRetMsg(retcode)}

}
