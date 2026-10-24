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

//举报代理
type ReportAgentHandler struct {
}

func (rh *ReportAgentHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_ReportAgentID)
}

func (rh *ReportAgentHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.ReportAgent{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.ReportAgent)
	areaid := mm.GetAreaID()
	agentname := mm.GetAgentName()
	memo := mm.GetContent()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intAreaID", areaid)
	ps.AddVarcharInput("chvAgentName", agentname, 32)
	ps.AddVarcharInput("chvMemo", memo, 512)
	ps.AddVarcharOutput("chvErrMsg", "")
	proc := db.NewProcedure("PrPs_AddReportAgentLog", ps)
	ret, err := DbUtil.ExecProc(proc)

	var code int32 = 0
	retmessage := ""
	if err != nil {
		ctx.Error("执行存储过程PrPs_AddReportAgentLog错误, %v", err)
		code = 0
		retmessage = "服务器错误"
	} else {
		if ret.GetReturnValue() == 1 {
			code = 1
		}
		retmessage = ret.GetOutputParamValue("chvErrMsg").(string)
	}

	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
	retmsg.SClassID = int32(netproto.HallMsgClassID_ReportAgentRetID)
	retmsg.MsgData = getRetMsg(code, retmessage)

	return []*msg.Message{retmsg}
}
