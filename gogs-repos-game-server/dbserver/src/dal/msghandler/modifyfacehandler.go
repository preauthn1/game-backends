package msghandler

import (
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
	"util"
)

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//修改头像
type ModifyFaceHandler struct {
}

func (rh *ModifyFaceHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_ModifyFaceID)
}

func (rh *ModifyFaceHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.ModifyFace{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.ModifyFace)

	userid := rpcinfo.GetUserID()
	ip := rpcinfo.GetIPAddress()
	faceid := mm.GetFaceID()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddIntInput("intFaceID", faceid)
	ps.AddVarcharInput("chvIPAddress", ip, 15)
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_User_ModifyFaceID", ps)
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	ret, err = DbUtil.ExecProc(proc)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	var code int32 = 0
	retmessage := ""
	if err != nil {
		ctx.Error("执行存储过程PrPs_User_ModifyFaceID错误, %v", err)
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
	retmsg.SClassID = int32(netproto.HallMsgClassID_ModifyFaceRetID)
	retmsg.MsgData = getRetMsg(code, retmessage)

	return []*msg.Message{retmsg}
}
