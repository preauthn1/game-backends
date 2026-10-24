package msghandler

import proto "example.invalid/golang/protobuf/proto"

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

type ServerRegHandler struct {
}

func (rh *ServerRegHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_ServerMgm) && sclsid == int32(netproto.ServerMgmClassID_GameServerRegister)
}

func (rh *ServerRegHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.ServerRegInfo{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.ServerRegInfo)

	serverid := mm.GetServerID()
	servertype := mm.GetServerType()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intServerID", serverid)
	ps.AddIntInput("intServerType", servertype)

	proc := db.NewProcedure("PrGs_ReRegister", ps)
	_, err := DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_ServerReg错误, %v", err)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}
}
