package msghandler

import (
	"fmt"

	proto "example.invalid/golang/protobuf/proto"
)

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//获取用户邮件列表
type GetMailDetailHandler struct {
}

func (rh *GetMailDetailHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_GetMailDetailID)
}

func (rh *GetMailDetailHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.RequestMailDetail{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}
	mm := rmm.(*netproto.RequestMailDetail)

	userid := rpc.GetUserID()
	cer := rpc.GetCer()
	id := mm.GetID()
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddIntInput("intMsgID", id)
	ps.AddVarcharInput("chvCer", cer, 32)
	proc := db.NewProcedure("PrPs_Mail_GetMailByID", ps)
	//dbret, err := DbUtil.ExecProc(proc)
	var dbret db.RecordSet
	var err error
	if userid < 150000000 {	//方舟
		dbret, err = DbUtil.ExecProc(proc)
	} else {	//其他公司
		dbret, err = CompanyDbUtil.ExecProc(proc)
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailDetailRetID), nil)
	retdata := new(netproto.MailDetail)
	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrPs_Mail_GetMailByID错误, %v", err)
		ctx.Error(strMsg)
		return []*msg.Message{GetDBRetMsg(0)}
	} else {
		if dbret.GetRetTableCount() > 0 {
			retdata.ID = proto.Int32(int32(dbret[0].GetValueByColName(0, "ID").(int64)))
			retdata.Content = proto.String(dbret[0].GetValueByColName(0, "Content").(string))
			retdata.Title = proto.String(dbret[0].GetValueByColName(0, "title").(string))
			retdata.IsRead = proto.Bool(dbret[0].GetValueByColName(0, "readtype").(int64) == 1)
			retdata.SendTime = proto.String(dbret[0].GetValueByColName(0, "SendTime").(string))
		}
	}

	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
