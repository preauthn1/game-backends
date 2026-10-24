package msghandler

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

type GetUserAngentUserLevelInfoHandler struct {
}

func (rh *GetUserAngentUserLevelInfoHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) &&
		sclsid == int32(netproto.HallMsgClassID_GetAgentUserLevelInfoID)
}

func (rh *GetUserAngentUserLevelInfoHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.AgentUserLevelInfomMsg{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.AgentUserLevelInfomMsg)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	proc := db.NewProcedure("PrPs_UserAgent_GetUserLevelInfo", ps)
	dbret, err := DbUtil.ExecProc(proc)
	retcode := int32(dbret.GetReturnValue())

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentUserLevelInfoRetID), nil)
	retdata := new(netproto.AgentUserLevelInfomRet)

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程 PrPs_UserAgent_GetUserLevelInfo 错误,%v", err)
		ctx.Error(strMsg)
	} else {
		if retcode == 1 {
			tb := dbret[0]
			if len(tb.Rows) > 0 {
				retdata.Stage1 = proto.Int32(int32(tb.GetValueByColName(0, "Stage1").(int64)))
				retdata.Stage2 = proto.Int32(int32(tb.GetValueByColName(0, "Stage2").(int64)))
				retdata.Stage3 = proto.Int32(int32(tb.GetValueByColName(0, "Stage3").(int64)))

			} else {
				retdata.Stage1 = proto.Int32(int32(0))
				retdata.Stage2 = proto.Int32(int32(0))
				retdata.Stage3 = proto.Int32(int32(0))
			}
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}
