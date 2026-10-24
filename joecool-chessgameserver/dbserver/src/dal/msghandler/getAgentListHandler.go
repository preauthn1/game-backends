package msghandler

import (
	"fmt"

	proto "example.invalid/golang/protobuf/proto"
)

import (
	"db"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"util"
)

//获取用户邮件列表
type GetAgentListHandler struct {
}

func (rh *GetAgentListHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_GetAgentListID)
}

func (rh *GetAgentListHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.RequestAgentList{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.RequestAgentList)
	areaid := mm.GetAreaID()

	if rpcinfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	userid := rpcinfo.GetUserID()
	ps := db.NewSqlParameters()
	ps.AddIntInput("intAreaID", areaid)
	ps.AddIntInput("intUserID", userid)
	proc := db.NewProcedure("PrPs_GetPayAgent", ps)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	//dbret, err := DbUtil.ExecProc(proc)
	var dbret db.RecordSet
	var err error
	if userid < 150000000 {	//方舟
		dbret, err = DbUtil.ExecProc(proc)
	} else {	//其他公司
		dbret, err = CompanyDbUtil.ExecProc(proc)
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetAgentListRetID), nil)
	retdata := new(netproto.AgentList)

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrPs_GetPayAgent错误, %v", err)
		ctx.Error(strMsg)
	} else {
		if dbret.GetRetTableCount() > 0 {
			tbData := dbret[0]
			for i := 0; i < len(tbData.Rows); i++ {
				agentDetail := new(netproto.AgentDeatil)

				agentDetail.Name = proto.String(tbData.GetValueByColName(i, "Name").(string))
				agentDetail.WXNo = proto.String(tbData.GetValueByColName(i, "WXNo").(string))
				agentDetail.QQ = proto.String(tbData.GetValueByColName(i, "QQ").(string))
				agentDetail.Alipay = proto.String(tbData.GetValueByColName(i, "Alipay").(string))
				agentDetail.Jiuliao = proto.String(tbData.GetValueByColName(i, "Jiuliao").(string))
				retdata.AgentList = append(retdata.AgentList, agentDetail)
			}
		}
	}

	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}
