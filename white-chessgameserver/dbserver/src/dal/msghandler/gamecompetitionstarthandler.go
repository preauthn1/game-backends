package msghandler

import (
	"db"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//報名參加活動
type GameCompetitionStartHandler struct {
}

func (fsu *GameCompetitionStartHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_BuyTicketID)
}

func (fsu *GameCompetitionStartHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.GetBuyTicketMsg{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.GetBuyTicketMsg)

	contestId, tbData1 := GetContestInfo(mm, ctx)
	contestNo := GetContestUserData(mm, ctx)

	retdata := new(netproto.BuyTicketMsg)
	if contestId != 0 {
		ps := db.NewSqlParameters()
		ps.AddIntInput("intContestID", contestId)
		ps.AddIntInput("intContestNo", contestNo) //該場次的第幾局(用來判斷要不要扣錢/免費次數)
		ps.AddIntInput("intGameID", mm.GetGameID())
		ps.AddIntInput("intUserID", mm.GetUserID())
		ps.AddVarcharOutput("chvErrMsg", "")
		ps.AddIntOutput("intRtnCode", 0)
		proc := db.NewProcedure("PrGs_Event_StartContest", ps)
		dbret, err := DbUtil.ExecProc(proc)
		//retcode := int32(dbret.GetReturnValue())
		if err != nil {
			ctx.Error("执行存储过程PrGs_Event_StartContest错误, %v, %s", err, proc.GetSqlString())
			//retcode = int32(0)
		}

		tbData2 := dbret[0]
		retdata.RetMsg = proto.String(tbData2.GetValueByColName(0, "chvErrMsg").(string))
		retdata.RetCode = proto.Int32(int32(tbData2.GetValueByColName(0, "intRtnCode").(int64)))
	} else {
		retdata.RetMsg = proto.String(tbData1.GetValueByColName(0, "chvErrMsg").(string))
		retdata.RetCode = proto.Int32(-1)
		mlog.Error("err : %v", retdata.GetRetMsg())
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BuyTicketRetID), nil)
	retmsg.MsgData = retdata

	return []*msg.Message{retmsg}
}

//取得玩家比賽場次
func GetContestUserData(mm *netproto.GetBuyTicketMsg, ctx *module.MouduleCtx) int32 {
	ps1 := db.NewSqlParameters()
	ps1.AddIntInput("intGameID", mm.GetGameID())
	ps1.AddIntInput("intUserID", mm.GetUserID())
	proc1 := db.NewProcedure("PrGs_Event_GetContestUserData", ps1)
	dbret, err := DbUtil.ExecProc(proc1)

	retcode := int32(dbret.GetReturnValue())
	if err != nil {
		ctx.Error("执行存储过程PrGs_Event_GetContestUserData错误, %v, %s", err, proc1.GetSqlString())
		retcode = int32(0)
	}
	contestNo := int32(1)
	tbData1 := dbret[0]
	if retcode == 1 {
		if dbret.GetRetTableCount() == 1 && len(tbData1.Rows) > 0 {
			contestNo = int32(tbData1.GetValueByColName(0, "ContestNo").(int64)) //比賽場次
			playtime := int32(tbData1.GetValueByColName(0, "Playtime").(int64))
			if playtime <= 0 {
				contestNo++
			}
		}
	}


	return contestNo
}

//取得比賽資訊
func GetContestInfo(mm *netproto.GetBuyTicketMsg, ctx *module.MouduleCtx) (int32, *db.RecordTable) {
	ps1 := db.NewSqlParameters()
	ps1.AddIntInput("intGameID", mm.GetGameID())
	proc1 := db.NewProcedure("PrGs_Event_GetContestInfo", ps1)
	dbret, err := DbUtil.ExecProc(proc1)
	retcode := int32(dbret.GetReturnValue())
	if err != nil {
		ctx.Error("执行存储过程PrGs_Event_GetContestInfo错误, %v, %s", err, proc1.GetSqlString())
		retcode = int32(0)
	}
	contestId := int32(0)
	tbData1 := dbret[0]
	if retcode == 1 {
		if dbret.GetRetTableCount() == 3 {
			//比賽資訊
			contestId = int32(tbData1.GetValueByColName(0, "ContestID").(int64)) //比賽活動ID
		}
	}
	return contestId, tbData1
}
