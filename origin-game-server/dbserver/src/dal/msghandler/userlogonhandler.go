package msghandler

import (
	"db"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

import (
	proto "example.invalid/golang/protobuf/proto"
	"util"
)

type UserLogonHandler struct {
}

func (h *UserLogonHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_UserLoginID)
}

func (h *UserLogonHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UserLogin{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UserLogin)

	hdtype := mm.GetHDType()
	hdcode := mm.GetHDCode()
	ip := rpc.GetIPAddress()
	siteid := mm.GetSiteID()
	version := mm.GetVersion()
	serverid := rpc.GetRouteServerID()

	userid := mm.GetUserID()
	cer := mm.GetCer()
	loginname := mm.GetLoginName()
	password := mm.GetPassword()
	connToken := rpc.GetConnToken()

	ps := db.NewSqlParameters()
	ps.AddVarcharInput("chvVersion", version, 16)
	ps.AddIntInput("tnyHDType", hdtype)
	ps.AddVarcharInput("chvHDCode", hdcode, 64)
	ps.AddVarcharInput("chvIPAddress", ip, 15)
	ps.AddIntInput("sintPlazaServerID", serverid)
	ps.AddIntInput("intAreaID", siteid)

	ps.AddIntOutput("intUserID", userid)
	ps.AddVarcharOutput("chvCertification", cer)

	if loginname != "" && password != "" {
		ps.AddVarcharInput("chvLoginName", loginname, 32)
		ps.AddVarcharInput("chvPassword", password, 32)
	}

	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_UserLogon_Normal", ps)
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

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_LoginRetID), nil)

	var logret *netproto.UserLoginRet
	if err != nil {
		mlog.Error("执行存储过程PrPs_UserLogon_Normal出错%v", err)
		logret = &netproto.UserLoginRet{}
		logret.Code = proto.Int32(0)
		logret.Message = proto.String("服务器错误.")
		retmsg.MsgData = logret
	} else {
		logret = parseUserLogonRet(ret)
		retmsg.MsgData = logret
		if retmsg.MsgData == nil {
			mlog.Error("执行 parseUserLogonRet 出错")
			retmsg.MsgData = getRetMsg(0, "服务器错误.")
		}
	}

	if connToken != "" {
		status := int32(2)
		if logret.GetCode() == 1 {
			status = 1
		}
		updateProtectNodeStatus(logret.GetUserID(), connToken, status)
	}

	return []*msg.Message{retmsg}
}
