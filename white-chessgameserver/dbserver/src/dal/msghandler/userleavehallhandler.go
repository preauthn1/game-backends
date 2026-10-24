package msghandler

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

import (
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
)

//用户离开广场
type UserLeaveHallHandler struct {
}

func (h *UserLeaveHallHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_UserLogoutID)
}

func (h *UserLeaveHallHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UserLogout{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UserLogout)

	mlog.NewDefaultLogger().Info("user logout hall.%v", mm)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddIntInput("sintServerID", rpc.GetRouteServerID())
	ps.AddVarcharInput("chvCer", mm.GetCer(), 32)

	proc := db.NewProcedure("PrPs_UserLeave", ps)
	_, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrPs_UserLeave错误, %v", err)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}

}
