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

//修改头像
type BindGuestAccountHandler struct {
}

func (rh *BindGuestAccountHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_BindGuestAccountID)
}

func (rh *BindGuestAccountHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.BindGuestAccount{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.BindGuestAccount)

	userid := rpcinfo.GetUserID()
	tel := mm.GetTel()
	vcode := mm.GetVCode()
	passwd := mm.GetPassword()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddVarcharInput("chvCode", vcode, 32)
	ps.AddVarcharInput("chvTel", tel, 32)
	ps.AddVarcharInput("chvPassword", passwd, 32)
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_User_BindGuest", ps)
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	if userid < 150000000 {	//方舟
		ret, err = DbUtil.ExecProc(proc)
	} else {	//其他公司
		ret, err = CompanyDbUtil.ExecProc(proc)
	}

	//fmt.Println(proc.GetSqlString())
	var code int32 = 0
	retmessage := ""
	if err != nil {
		ctx.Error("执行存储过程PrPs_User_BindGuest错误, %v", err)
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
	retmsg.SClassID = int32(netproto.HallMsgClassID_BindGuestAccountRetID)
	retmsg.MsgData = getRetMsg(code, retmessage)

	return []*msg.Message{retmsg}
}
