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
type SuperToolUpdVIPUserHandler struct {
}

func (rh *SuperToolUpdVIPUserHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_SuperToolUpdVipUserID)
}

func (rh *SuperToolUpdVIPUserHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SuperToolVipUserData{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.SuperToolVipUserData)
	userid := mm.GetUserID()
	completeamount := mm.GetCompleteAmount()
	complete := mm.GetComplete()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddBigIntInput("intCompleteAmount", completeamount)
	ps.AddIntInput("intComplete", complete)
	proc := db.NewProcedure("PrGs_SuperTool_UpdVIPUser", ps)
	ret, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_SuperTool_UpdVIPUser错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	} else {
		retcode = int32(ret.GetReturnValue())
	}

	return []*msg.Message{GetDBRetMsg(retcode)}

}
