package msghandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

//写游戏记录详情
type AddPerGameLogHandler struct {
}

func (rh *AddPerGameLogHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_DealLogID)
}

func (rh *AddPerGameLogHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.DealLog{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.DealLog)
	// gameid := mm.GetGameID()
	serverid := mm.GetServerID()
	gamedata := mm.GetGameData()
	usergameret := mm.GetUserGameRet()
	begintime := mm.GetGameBeginTime()
	endtime := mm.GetGameEndTime()
	taxamount := mm.GetTaxAmount()
	// tableno := mm.GetTableNo()
	// njrecord := mm.GetNextJiangRecord()
	// winPrizeID := int64(0)
	// if njrecord != nil {
	// 	ps := db.NewSqlParameters()
	// 	ps.AddIntInput("intGameID", gameid)
	// 	ps.AddIntInput("intServerID", serverid)
	// 	ps.AddIntInput("intTableNo", tableno)
	// 	ps.AddVarcharInput("chvAdminUser", njrecord.GetAdminUser(), 20)
	// 	ps.AddVarcharInput("chvPrizeType", njrecord.GetJiangType(), 50)
	// 	proc := db.NewProcedure("PrGs_AddWinPrizeLog", ps)
	// 	ret, err := DealLogDbUtil.ExecProc(proc)

	// 	if err != nil {
	// 		ctx.Error("执行存储过程PrGs_AddWinPrizeLog错误, %v, %s", err, proc.GetSqlString())
	// 		winPrizeID = 0
	// 	}
	// 	winPrizeID = ret.GetReturnValue()
	// }
	ps := db.NewSqlParameters()
	// ps.AddIntInput("intGameID", gameid)
	ps.AddIntInput("intServerID", serverid)
	ps.AddVarcharInput("chvGameData", gamedata, -1)
	ps.AddVarcharInput("chvUserGameRet", usergameret, -1)
	ps.AddVarcharInput("dtmGameBeginTime", begintime, 32)
	ps.AddVarcharInput("dtmGameEndTime", endtime, 32)
	ps.AddBigIntInput("lngTaxAmount", taxamount)
	ps.AddIntInput("intGroupID", mm.GetGroupID())
	// ps.AddIntInput("intWinPrizeID", int32(winPrizeID))

	proc := db.NewProcedure("PrGs_AddPerGameLog", ps)
	//mlog.System(proc.GetSqlString())
	_, err := DealLogDbUtil.ExecProc(proc)

	var retcode int32 = 1
	if err != nil {
		ctx.Error("执行存储过程PrGs_AddPerGameLog错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}

	return nil
}
