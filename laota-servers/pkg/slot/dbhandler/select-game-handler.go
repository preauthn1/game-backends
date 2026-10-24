package dbhandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	dbutil "example.invalid/ServerTeam/Servers/pkg/db/dbutil"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// SelectGameHander 寫入及讀取選擇遊戲機會
type SelectGameHander struct{}

// IsHook 是否Mapping到此Handler
func (h *SelectGameHander) IsHook(bclsid int32, sclsid int32) bool {
	if bclsid != int32(netproto.MessageBClassID_Game) {
		return false
	}

	return sclsid == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID) || sclsid == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameRecordID)
}

// OnMessage 接收訊息
func (h *SelectGameHander) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	switch netproto.Slot_GameMessageClassID(m.SClassID) {
	case netproto.Slot_GameMessageClassID_Slot_SelectGameReadID:
		return h.ReadSelectGame(ctx, clt, m)
	case netproto.Slot_GameMessageClassID_Slot_SelectGameRecordID:
		return h.RecordSelectGame(ctx, clt, m)
	}

	return []*msg.Message{}
}

// ReadSelectGame 查詢選擇遊戲機會
func (h *SelectGameHander) ReadSelectGame(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_SelectGameRead{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_MSG_SelectGameRead)

	const ProcedureName = "PrGs_Slot_SelectGameRead"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", mm.GetUserID()) // 使用者ID
	ps.AddIntInput("intGameID", mm.GetGameID()) // GameID
	ps.AddIntInput("intFlagID", mm.GetFlagID()) // FlagID
	ps.AddVarcharOutput("chvMsg", "")           // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		mlog.Error("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	rtnmsg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		mlog.Error("Procedure %s execute error. %v", ProcedureName, rtnmsg)
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	data := new(netproto.Slot_MSG_SelectGameRecord)
	info := new(netproto.Slot_SelectGameInfo)
	info.UserID = proto.Int32(mm.GetUserID())
	info.GameID = proto.Int32(mm.GetGameID())
	info.FlagID = proto.Int32(mm.GetFlagID())

	for i := 0; i < len(dbret[0].Rows); i++ {

		if val, ok := dbret[0].GetValueByColName(i, "UserID").(int64); ok {
			info.UserID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "GameID").(int64); ok {
			info.GameID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "FlagID").(int64); ok {
			info.FlagID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "OriginID").(int64); ok {
			info.OriginID = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Times").(int64); ok {
			info.Times = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Data").(string); ok {
			info.Data = proto.String(val)
		}

	}

	example.invalid = info

	retMsg := new(msg.Message)
	retMsg.BClassID = int32(netproto.MessageBClassID_Game)
	retMsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
	retMsg.MsgData = data

	return []*msg.Message{retMsg}
}

// RecordSelectGame 更新選擇遊戲機會
func (h *SelectGameHander) RecordSelectGame(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_SelectGameRecord{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_MSG_SelectGameRecord)

	if example.invalid == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	var ProcedureName string
	ps := db.NewSqlParameters()

	// 若是沒有資料就Reset
	if example.invalid.GetOriginID() == 0 && example.invalid.GetTimes() == 0 && len(example.invalid.GetData()) == 0 {
		ProcedureName = "PrGs_Slot_SelectGameReset"
		ps.AddIntInput("intUserID", example.invalid.GetUserID()) // 使用者ID
		ps.AddIntInput("intGameID", example.invalid.GetGameID()) // GameID
		ps.AddIntInput("intFlagID", example.invalid.GetFlagID()) // FlagID
		ps.AddVarcharOutput("chvMsg", "")                // chvMsg的輸出
	} else {
		ProcedureName = "PrGs_Slot_SelectGameRecord"
		ps.AddIntInput("intUserID", example.invalid.GetUserID())      // 使用者ID
		ps.AddIntInput("intGameID", example.invalid.GetGameID())      // GameID
		ps.AddIntInput("intFlagID", example.invalid.GetFlagID())      // FlagID
		ps.AddIntInput("intOriginID", example.invalid.GetOriginID())  // OriginID
		ps.AddIntInput("intTimes", example.invalid.GetTimes())        // Times
		ps.AddVarcharInput("txtData", example.invalid.GetData(), 255) // Data
		ps.AddVarcharOutput("chvMsg", "")                     // chvMsg的輸出
	}

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		mlog.Error("Procedure %s execute error. %v; SQL: %s", ProcedureName, err, proc.GetSqlString())
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	rtnmsg := dbret.GetOutputParamValueString("chvMsg")

	if dbret.GetReturnValue() != 1 {
		mlog.Error("Procedure %s execute error. %v", ProcedureName, rtnmsg)
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	return []*msg.Message{dbutil.GetDBRetMsg(1)}
}
