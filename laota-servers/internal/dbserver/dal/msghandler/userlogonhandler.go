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

import (
	"fmt"
	"time"
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
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
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
	ret, err = dbutil.DbUtil.ExecProc(proc)

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
		logret = dbutil.ParseUserLogonRet(ret)
		logret.Area_ID = proto.Int32(siteid)
		retmsg.MsgData = logret
		if retmsg.MsgData == nil {
			mlog.Error("执行 ParseUserLogonRet 出错")
			retmsg.MsgData = dbutil.GetRetMsg(0, "服务器错误.")
		}
	}

	if connToken != "" {
		status := int32(2)
		if logret.GetCode() == 1 {
			status = 1
		}
		dbutil.UpdateProtectNodeStatus(logret.GetUserID(), connToken, status)
	}

	// debug info
	var content = fmt.Sprintf("客户端[%d] rpc.userid:%d rpc:%v clt:%v 设置IP:%s", userid, rpc.GetUserID(), rpc, clt, ip)
	var str = fmt.Sprintf("%s %s", time.Now().Format("2006/01/02 15:04:05.000000"), content)
	var filepath = "./log/login_normal.log"
	util.FilePutContents(filepath, str + "\r\n")

	return []*msg.Message{retmsg}
}
