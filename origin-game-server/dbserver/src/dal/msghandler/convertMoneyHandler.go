package msghandler

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)
import (
	proto "example.invalid/golang/protobuf/proto"
	"math"
)

//联系客服
type ConvertMoneyHandler struct {
}

func (rh *ConvertMoneyHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_ConvertMoneyID)
}

func (rh *ConvertMoneyHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.ConvertMoney{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.ConvertMoney)
	amount := int64(math.Abs(float64(mm.GetAmount())))
	convertType := mm.GetConvertType()

	userid := rpcinfo.GetUserID()
	cer := rpcinfo.GetCer()
	ip := rpcinfo.GetIPAddress()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddVarcharInput("chvCer", cer, 32)
	ps.AddBigIntInput("lngAmount", amount)
	ps.AddTinyIntInput("tnyConvertType", byte(convertType))
	ps.AddVarcharInput("chvIP", ip, 15)
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_AddConvertLog", ps)
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	ret, err = DbUtil.ExecProc(proc)

	var code int32 = 0
	retmessage := ""
	if err != nil {
		ctx.Error("执行存储过程PrPs_AddConvertLog错误, %v", err)
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
	retmsg.SClassID = int32(netproto.HallMsgClassID_ConvertMoneyRetID)
	retmsg.MsgData = getRetMsg(code, retmessage)

	return []*msg.Message{retmsg}
}
