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
type GetMailListHandler struct {
}

func (rh *GetMailListHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_GetMailListID)
}

func (rh *GetMailListHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo := rpcmsg.GetRPCInfo(m)

	if rpcinfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	userid := rpcinfo.GetUserID()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	proc := db.NewProcedure("PrPs_Mail_GetMailList", ps)
	//dbret, err := DbUtil.ExecProc(proc)
	var dbret db.RecordSet
	var err error
	if userid < 150000000 {	//方舟
		dbret, err = DbUtil.ExecProc(proc)
	} else {	//其他公司
		dbret, err = CompanyDbUtil.ExecProc(proc)
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailListRetID), nil)
	retdata := new(netproto.MailList)

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrPs_Mail_GetMailList错误, %v", err)
		ctx.Error(strMsg)
	} else {
		if dbret.GetRetTableCount() > 0 {
			tbMail := dbret[0]
			for i := 0; i < len(tbMail.Rows); i++ {
				emailDetail := new(netproto.MailDetail)
				emailDetail.ID = proto.Int32(int32(tbMail.GetValueByColName(i, "id").(int64)))
				emailDetail.Title = proto.String(tbMail.GetValueByColName(i, "title").(string))
				emailDetail.Content = proto.String(string(""))
				emailDetail.IsRead = proto.Bool(tbMail.GetValueByColName(i, "readtype").(int64) == 1)
				emailDetail.SendTime = proto.String(tbMail.GetValueByColName(i, "SendTime").(string))
				retdata.MailList = append(retdata.MailList, emailDetail)
			}
		}
	}

	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
