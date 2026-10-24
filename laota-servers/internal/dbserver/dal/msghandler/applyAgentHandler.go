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
	"example.invalid/ServerTeam/Servers/pkg/util"
)


//客户端申请代理的请求处理
type ApplyAgentHandler struct {
}

func (rh *ApplyAgentHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_ApplyAgentID)
}

func (rh *ApplyAgentHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.ApplyAgent{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.ApplyAgent)
	areaid := mm.GetAreaID()
	name := mm.GetName()
	tel := mm.GetTel()
	qq := mm.GetQQ()
	wxno := mm.GetWXNo()
	memo := mm.GetMemo()
	alipay := mm.GetAlipay()
	userid := rpc.GetUserID()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddIntInput("intAreaID", areaid)
	ps.AddVarcharInput("chvName", name, 32)
	ps.AddVarcharInput("chvTel", tel, 20)
	ps.AddVarcharInput("chvQQ", qq, 20)
	ps.AddVarcharInput("chvWXNo", wxno, 20)
	ps.AddVarcharInput("chvMemo", memo, 256)
	ps.AddVarcharInput("chvAlipay", alipay, 256)
	ps.AddVarcharOutput("chvErrMsg", "")
	proc := db.NewProcedure("PrPs_ApplyAgent", ps)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	ret, err = dbutil.DbUtil.ExecProc(proc)

	var code int32 = 0
	retmessage := ""
	if err != nil {
		ctx.Error("执行存储过程PrPs_ApplyAgent错误, %v", err)
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
	retmsg.SClassID = int32(netproto.HallMsgClassID_ApplyAgentRetID)
	retmsg.MsgData = dbutil.GetRetMsg(code, retmessage)

	return []*msg.Message{retmsg}
}
