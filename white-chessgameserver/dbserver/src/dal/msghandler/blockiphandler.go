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
		return []*msg.Message{GetDBRetMsg(0)}
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
	_, err := DbUtil.ExecProc(proc)

	if err != nil {
		ctx.Error("执行存储过程PrSys_BlockIP错误, %v", err)
	}

	return []*msg.Message{GetDBRetMsg(0)}
}
