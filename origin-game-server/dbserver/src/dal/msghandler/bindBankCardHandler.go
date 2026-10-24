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

type BindBankCardHandler struct {
}

func (rh *BindBankCardHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_BindBankCardID)
}

func (rh *BindBankCardHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.BankCardInfo{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.BankCardInfo)

	bankcardnum := mm.GetBankCardNum()
	realname := mm.GetRealName()
	bankcode := mm.GetBankCode()
	vcode := mm.GetVCode()
	codetype := mm.GetCodeType()

	userid := rpcinfo.GetUserID()
	cer := rpcinfo.GetCer()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddVarcharInput("chvCer", cer, 32)
	ps.AddVarcharInput("chvCode", vcode, 6)
	ps.AddIntInput("tintCodeType", codetype)
	ps.AddVarcharInput("chvRealName", realname, 64)
	ps.AddVarcharInput("chvUnionpayBank", bankcode, 5)
	ps.AddVarcharInput("chvUnionpayAccount", bankcardnum, 64)
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_BindUnionPayAccount", ps)
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	ret, err = DbUtil.ExecProc(proc)

	var code int32 = 0
	retmessage := ""
	if err != nil {
		ctx.Error("PrPs_BindUnionPayAccount, %v", err)
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
	retmsg.SClassID = int32(netproto.HallMsgClassID_BindBankCardRetID)
	retmsg.MsgData = getRetMsg(code, retmessage)

	return []*msg.Message{retmsg}
}
