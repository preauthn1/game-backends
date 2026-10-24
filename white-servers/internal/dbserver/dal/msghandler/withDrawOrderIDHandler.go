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

//申請提現(淨利潤)
type WithDrawOrderIDHandler struct {
}

func (rh *WithDrawOrderIDHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) &&
		sclsid == int32(netproto.HallMsgClassID_NetProfitAgentWithDrawOrderID)
}

func (rh *WithDrawOrderIDHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.NetProfitAgentWithDrawOrderMsg{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.NetProfitAgentWithDrawOrderMsg)

	if mm.GetAmount() > 0 {
		returnMessage := rh.OnMessageInner(ctx, clt, m)
		return returnMessage
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentWithDrawOrderRetID), nil)
	retdata := new(netproto.NetProfitAgentWithDrawOrderMsgRet) // 回傳封包

	if mm.GetAmount() <= 0 {
		retdata.ChvMsg = proto.String("可提现余额不正确，金额需大于0")
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}

func (rh *WithDrawOrderIDHandler) OnMessageInner(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.NetProfitAgentWithDrawOrderMsg{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.NetProfitAgentWithDrawOrderMsg)
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddIntInput("intAmount", mm.GetAmount())
	ps.AddVarcharOutput("chvMsg", "")
	proc := db.NewProcedure("PrPs_NetProfitAgent_WithDrawOrder", ps)
	dbret, err := DbUtil.ExecProc(proc)
	retcode := int32(dbret.GetReturnValue())

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_NetProfitAgentWithDrawOrderRetID), nil)
	retdata := new(netproto.NetProfitAgentWithDrawOrderMsgRet)

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程 PrPs_NetProfitAgent_WithDrawOrder 错误,%v", err)
		ctx.Error(strMsg)
	} else {
		if retcode == 1 {
			tb := dbret[0]
			if len(tb.Rows) > 0 {
				retdata.ChvMsg = proto.String(string(tb.GetValueByColName(0, "ChvMsg").(string)))
			} else {
				retdata.ChvMsg = proto.String("")
			}
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}
