package msghandler

import (
	"db"
	"fmt"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"

	"example.invalid/golang/protobuf/proto"
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

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_BuyTicketRetID), nil)
	retdata := new(netproto.BuyTicketMsg)
	retdata.Area_ID = proto.Int32(mm.GetArea_ID())

	contestInfo, err := GetContestInfo(ctx, mm)
	if err != nil {
		retdata.RetCode = proto.Int32(-1)
		retdata.RetMsg = proto.String(err.Error())

		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}

	contestNo := GetContestUserData(ctx, mm)

	contestID := contestInfo.GetValueByColName(0, "ContestID").(int64)

	ps := db.NewSqlParameters()

	ps.AddIntInput("intContestID", int32(contestID))
	ps.AddIntInput("intContestNo", contestNo) //該場次的第幾局(用來判斷要不要扣錢/免費次數)
	ps.AddIntInput("intGameID", mm.GetGameID())
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddVarcharOutput("chvErrMsg", "")
	ps.AddIntOutput("intRtnCode", 0)

	proc := db.NewProcedure("PrGs_Event_StartContest", ps)

	dbret, err := DbUtil.ExecProc(proc)
	if err != nil {
		ctx.Error("执行存储过程PrGs_Event_StartContest错误, %v, %s", err, proc.GetSqlString())
		retdata.RetCode = proto.Int32(-1)
		retdata.RetMsg = proto.String("执行存储过程错误")
		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}

	if dbret.GetRetTableCount() == 0 {
		retdata.RetCode = proto.Int32(-1)
		retdata.RetMsg = proto.String("执行存储过程错误")
		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}

	retdata.RetMsg = proto.String(dbret[0].GetValueByColName(0, "chvErrMsg").(string))
	retdata.RetCode = proto.Int32(int32(dbret[0].GetValueByColName(0, "intRtnCode").(int64)))
	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}

// GetContestUserData 取得玩家比賽場次
func GetContestUserData(ctx *module.MouduleCtx, mm *netproto.GetBuyTicketMsg) int32 {

	ps := db.NewSqlParameters()
	ps.AddIntInput("intGameID", mm.GetGameID())
	ps.AddIntInput("intUserID", mm.GetUserID())

	proc := db.NewProcedure("PrGs_Event_GetContestUserData", ps)

	dbret, err := DbUtil.ExecProc(proc)
	if err != nil {
		ctx.Error("执行存储过程PrGs_Event_GetContestUserData错误, %v, %s", err, proc.GetSqlString())
		return 0
	}

	var contestNo int32 = 1

	if dbret.GetReturnValue() == 1 && dbret.GetRetTableCount() == 1 && len(dbret[0].Rows) > 0 {
		contestNo = int32(dbret[0].GetValueByColName(0, "ContestNo").(int64)) //比賽場次
		playtime := int32(dbret[0].GetValueByColName(0, "Playtime").(int64))
		if playtime <= 0 {
			contestNo++
		}
	}

	return contestNo
}

// GetContestInfo 取得比賽資訊
func GetContestInfo(ctx *module.MouduleCtx, mm *netproto.GetBuyTicketMsg) (*db.RecordTable, error) {

	sp := db.NewSqlParameters()
	sp.AddIntInput("intGameID", mm.GetGameID())

	proc := db.NewProcedure("PrGs_Event_GetContestInfo", sp)

	dbret, err := DbUtil.ExecProc(proc)
	if err != nil {
		ctx.Error("执行存储过程PrGs_Event_GetContestInfo错误, %v, %s", err, proc.GetSqlString())
		return nil, fmt.Errorf("查无比赛资讯, GameID: %d", mm.GetGameID())
	}

	if dbret.GetReturnValue() == 1 && dbret.GetRetTableCount() == 3 && len(dbret[0].Rows) > 0 {
		return dbret[0], nil
	}

	return nil, fmt.Errorf("查无比赛资讯, GameID: %d", mm.GetGameID())
}
