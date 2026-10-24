package msghandler

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

type GuestLogonHandler struct {
}

func (h *GuestLogonHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_GuestLoginID)
}

func (h *GuestLogonHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.GuestLogin{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.GuestLogin)

	hdtype := mm.GetHDType()
	hdcode := mm.GetHDCode()
	ip := rpc.GetIPAddress()
	siteid := mm.GetSiteID()
	version := mm.GetVersion()
	inviteCode := mm.GetInviteCode()
	serverid := rpc.GetRouteServerID()
	connToken := rpc.GetConnToken()
	_ = mm.GetGuestKey()

	//防止机器人搞事
	//if guestKey != "rayonisgod" {
	//	return guestKeyFail()
	//}

	//快速登入是否需要兩次
	key := fmt.Sprintf("%d_%s", siteid, hdcode)
	if v := HDCodeMap.Get(key); FastLogonStat && v == nil {
		HDCodeMap.Set(key, key)
		return canNotLogonFirst()
	}

	ps := db.NewSqlParameters()
	ps.AddVarcharInput("chvVersion", version, 16)
	ps.AddIntInput("tnyHDType", hdtype)
	ps.AddVarcharInput("chvHDCode", hdcode, 64)
	ps.AddVarcharInput("chvIPAddress", ip, 15)
	ps.AddIntInput("sintPlazaServerID", serverid)
	ps.AddIntInput("intAreaID", siteid)
	ps.AddVarcharInput("chvInviteCode", inviteCode, 10)
	ps.AddIntOutput("intUserID", 0)
	ps.AddVarcharOutput("chvCertification", "")
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_UserLogon_Fast", ps)
	//ret, err := DbUtil.ExecProc(proc)
	//var err error
	ret, err := DbUtil.ExecProc(proc)

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
		mlog.Error("执行存储过程PrPs_UserLogon_Fast出错%v", err)
		logret = &netproto.UserLoginRet{}
		logret.Code = proto.Int32(0)
		logret.Message = proto.String(fmt.Sprintf("服务器错误."))
		retmsg.MsgData = logret
	} else {
		logret = parseUserLogonRet(ret)
		logret.HDCode = proto.String(hdcode)
		logret.HDType = proto.Int32(hdtype)
		retmsg.MsgData = logret
		if retmsg.MsgData == nil {
			mlog.Error("执行 parseUserLogonRet 出错")
			retmsg.MsgData = getRetMsg(0, "服务器错误.")
		}
	}

	//連線驗證
	if connToken != "" {
		status := int32(2)
		if logret.GetCode() == 1 {
			status = 1
		}
		updateProtectNodeStatus(logret.GetUserID(), connToken, status)
	}

	return []*msg.Message{retmsg}
}

func canNotLogonFirst() []*msg.Message {
	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_LoginRetID), nil)
	logret := &netproto.UserLoginRet{}
	logret.Code = proto.Int32(0)
	logret.Message = proto.String(fmt.Sprintf("请再进行登入一次."))
	retmsg.MsgData = logret
	return []*msg.Message{retmsg}
}

func guestKeyFail() []*msg.Message {
	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_LoginRetID), nil)
	logret := &netproto.UserLoginRet{}
	logret.Code = proto.Int32(0)
	logret.Message = proto.String(fmt.Sprintf("重要数据遗失，请先更新或重新下载登入"))
	retmsg.MsgData = logret
	return []*msg.Message{retmsg}
}
