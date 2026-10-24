package msghandler

import (
	"db"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"util"
	"strings"
)

// USER 加入下线(净利润)
type NetProfitAgentAddUserRelationHandler struct {
}

func (rh *NetProfitAgentAddUserRelationHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) &&
		sclsid == int32(netproto.HallMsgClassID_NetProfitAgentAddUserRelationID)
}

func (rh *NetProfitAgentAddUserRelationHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.AddUserRelationMsg{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.AddUserRelationMsg)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddVarcharInput("chvInviteCode", mm.GetInviteCode(), 20)
	ps.AddVarcharOutput("chvMsg", "")

	proc := db.NewProcedure("PrPs_NetProfitAgent_AddUserRelation", ps)
	//mlog.System("[weihao] %v", proc.GetSqlString())
	ret, err := DbUtil.ExecProc(proc)

	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()

	var retcode int32 = 0
	var chvMsg string
	if err != nil {
		ctx.Error("执行存储过程PrPs_NetProfitAgent_AddUserRelation错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	} else {
		retcode = int32(ret.GetReturnValue())
		chvMsg = ret.GetOutputParamValue("chvMsg").(string)

		iErr := strings.Index(chvMsg, "错误")
		if iErr > -1 {
			retcode = 0
		}
	}

	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
	retmsg.SClassID = int32(netproto.HallMsgClassID_NetProfitAgentAddUserRelationRetID)
	data := new(netproto.AddUserRelationRet)
	data.RetCode = proto.Int32(retcode)
	data.ChvMsg = proto.String(chvMsg)
	retmsg.MsgData = data

	return []*msg.Message{retmsg}
}
