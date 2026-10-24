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

//封锁Ip
type BlockIPHandler struct {
}

func (rh *BlockIPHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_BlockIPID)
}

func (rh *BlockIPHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.BlockIP{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.BlockIP)
	ip := mm.GetIPAddress()
	sec := mm.GetSec()
	memo := mm.GetMemo()

	ps := db.NewSqlParameters()
	ps.AddVarcharInput("chvIPAddress", ip, 15)
	ps.AddIntInput("intSec", sec)
	ps.AddVarcharInput("chvMemo", memo, 64)

	proc := db.NewProcedure("PrSys_BlockIP", ps)
	_, err := dbutil.DbUtil.ExecProc(proc)

	if err != nil {
		ctx.Error("执行存储过程PrSys_BlockIP错误, %v", err)
	}

	return []*msg.Message{dbutil.GetDBRetMsg(0)}
}
