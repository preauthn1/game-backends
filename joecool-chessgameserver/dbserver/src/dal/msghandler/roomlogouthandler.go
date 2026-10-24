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
	"example.invalid/golang/protobuf/proto"
)

type RoomLogoutHandler struct {
}

func (h *RoomLogoutHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_LogoutRoomID)
}

func (h *RoomLogoutHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.LogoutGameRoomInfo{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.LogoutGameRoomInfo)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intMsgID", rpc.GetQueueID())
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddBigIntInput("lngUpdateAmount", mm.GetUpdateAmount())
	ps.AddBigIntInput("lngTaxAmount", mm.GetTaxAmount())
	ps.AddBigIntInput("lngFlowAmount", mm.GetFlowAmount())
	ps.AddIntInput("intWinCount", mm.GetWinCount())
	ps.AddIntInput("intLoseCount", mm.GetLoseCount())
	ps.AddIntInput("intDrawCount", mm.GetDrawCount())
	ps.AddVarcharInput("dtmLoginTime", mm.GetLoginTime(), 32)
	ps.AddVarcharInput("dtmLogoutTime", mm.GetLogoutTime(), 32)
	ps.AddVarcharInput("chvIPAddress", mm.GetIPAddress(), 15)
	ps.AddIntInput("tnyHDType", mm.GetHDType())
	ps.AddVarcharInput("chvHDCode", mm.GetHDCode(), 64)
	ps.AddIntInput("intGameTime", mm.GetGameTime())
	ps.AddIntInput("sintGameID", mm.GetGameID())
	ps.AddIntInput("sintServerID", mm.GetServerID())
	ps.AddBigIntInput("lngLoginMoney", mm.GetLoginMoney())
	ps.AddIntInput("tnyScoreUpdateMode", mm.GetScoreUpdateMode())
	ps.AddBigIntInput("lngTradeID", mm.GetTradeId())
	ps.AddIntInput("intLastWinLose", mm.GetLastWinLose())
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrGs_UserLogout", ps)
	//_, err := DbUtil.ExecProc(proc)
	var err error
	if mm.GetUserID() < 150000000 {	//方舟
		_, err = DbUtil.ExecProc(proc)
	} else {	//其他公司
		_, err = CompanyDbUtil.ExecProc(proc)
	}

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_UserLogout错误, %v", err)
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}

	return nil
}
