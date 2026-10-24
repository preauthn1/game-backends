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

import proto "example.invalid/golang/protobuf/proto"

type RequestUserInfoHandler struct {
}

func (h *RequestUserInfoHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_RequestUserHallInfoID)
}

func (h *RequestUserInfoHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.RequestUserHallInfo{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.RequestUserHallInfo)

	userid := rpc.GetUserID()
	areaid := mm.GetSiteID()

	ps := db.NewSqlParameters()
	ps.AddIntOutput("intUserID", userid)
	ps.AddIntInput("intAreaID", areaid)
	ps.AddVarcharOutput("chvCertification", "")
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_User_GetUserInfo", ps)
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	if userid < 150000000 {	//方舟
		ret, err = DbUtil.ExecProc(proc)
	} else {	//其他公司
		ret, err = CompanyDbUtil.ExecProc(proc)
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UserHallInfoID), nil)

	if err != nil || ret.GetRetTableCount() <= 0 {
		mlog.Error("执行存储过程PrPs_User_GetUserInfo出错%v", err)
		logret := &netproto.UserLoginRet{}
		logret.Code = proto.Int32(0)
		logret.Message = proto.String("服务器错误.")
		retmsg.MsgData = logret

	} else {
		logret := parseUserLogonRet(ret)
		retmsg.MsgData = logret
	}

	return []*msg.Message{retmsg}
}
