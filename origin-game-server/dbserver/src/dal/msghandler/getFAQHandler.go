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

//获取FAQ
type GetFAQHandler struct {
}

func (rh *GetFAQHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_GetFAQID)
}

func (rh *GetFAQHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.GetFAQ{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.GetFAQ)
	platformid := mm.GetPlatformID()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intPlatformID", platformid)
	ps.AddVarcharSizeOutput("chvContent", "", 8000)

	proc := db.NewProcedure("PrPs_GetFAQ", ps)
	ret, err := DbUtil.ExecProc(proc)

	faqContent := ""
	if err != nil {
		ctx.Error("执行存储过程PrPs_GetFAQ错误, %v", err)
	} else {
		faqContent = ret.GetOutputParamValue("chvContent").(string)
		example.invalid("返回结果：%v", faqContent)
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetFAQRetID), nil)
	retdata := new(netproto.FAQDetail)
	retdata.Content = proto.String(faqContent)
	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
