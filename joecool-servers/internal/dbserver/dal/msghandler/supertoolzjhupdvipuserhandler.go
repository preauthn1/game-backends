package msghandler

import (
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/db/dbutil"
)

import (
	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)



//写游戏记录详情
type SuperToolZjhUpdVIPUserHandler struct {
}

func (rh *SuperToolZjhUpdVIPUserHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_SuperToolZjhUpdVipUserID)
}

func (rh *SuperToolZjhUpdVIPUserHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SuperToolZjhUpdVipUserData{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.SuperToolZjhUpdVipUserData)
	userid := mm.GetUserID()


	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)

	if mm.Finish != nil {
		ps.AddBigIntInput("intFinish", mm.GetFinish())
	}
	if mm.FinishNextWinLoseNum != nil {
		ps.AddBigIntInput("intFinishNextWinLoseNum", mm.GetFinishNextWinLoseNum())
	}
	if mm.NextTable != nil {
		ps.AddIntInput("intNextTable", mm.GetNextTable())
	}
	if mm.NextType != nil {
		ps.AddBigIntInput("intNextType", mm.GetNextType())
	}


	proc := db.NewProcedure("PrGs_SuperTool_ZJH_UpdVIPUser", ps)
	mlog.System(proc.GetSqlString())
	ret, err := dbutil.DbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_SuperTool_ZJH_UpdVIPUser错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	} else {
		retcode = int32(ret.GetReturnValue())
	}

	return []*msg.Message{dbutil.GetDBRetMsg(retcode)}

}
