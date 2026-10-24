package msghandler

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
	ps.AddBigIntInput("lngCouponAmount", mm.GetCouponAmount())
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrGs_UserLogout", ps)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	var err error
	_, err = DbUtil.ExecProc(proc)
	var retcode int32 = 1

	for i := 0; err != nil && i < 100; i++ {
		mlog.Error("执行存储过程PrGs_UserLogout错误, %v", err)
		_, err = DbUtil.ExecProc(proc) //重试一次
		if err == nil {
			mlog.Warn("第%v次重试执行存储过程PrGs_UpdateUserMoney成功,msgid: %d, userid: %d", i+1, rpc.GetQueueID(), mm.GetUserID())
			break
		}
	}

	if err != nil {
		ctx.Error("PrGs_UserLogout失败,msgid: %d, userid: %d, score: %d, serverid: %d, taxamount: %d", rpc.GetQueueID(), mm.GetUserID(), mm.GetUpdateAmount(), mm.GetServerID(), mm.GetTaxAmount())
		ctx.Error("/*【退房需补单】*/ USE CenterDB; %s \nGO\n/*【end需补单】*/", proc.GetSqlString())
		retcode = 0
	}


	return []*msg.Message{GetDBRetMsg(retcode)}
}
