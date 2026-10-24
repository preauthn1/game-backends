package msghandler

import (
	"fmt"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// 获取用户邮件列表
type GetMailDetailHandler struct{}

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

	dbret, err := DbUtil.ExecProc(proc)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetMailDetailRetID), nil)
	retdata := new(netproto.MailDetail)
	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrPs_Mail_GetMailByID错误, %v", err)
		ctx.Error(strMsg)
		return []*msg.Message{GetDBRetMsg(0)}
	} else {
		if dbret.GetRetTableCount() > 0 && len(dbret[0].Rows) > 0 {
			retdata.ID = proto.Int32(dbret[0].GetValueByColNameInt32(0, "ID"))
			retdata.Content = proto.String(dbret[0].GetValueByColNameString(0, "Content"))
			retdata.Title = proto.String(dbret[0].GetValueByColNameString(0, "title"))
			retdata.IsRead = proto.Bool(dbret[0].GetValueByColNameInt64(0, "readtype") == 1)
			retdata.SendTime = proto.String(dbret[0].GetValueByColNameString(0, "SendTime"))
		}
	}

	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
