package dbhandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// FreeSpinHander 寫入及讀取免費遊戲
type FreeSpinHander struct{}

// IsHook 是否Mapping到此Handler
func (h *FreeSpinHander) IsHook(bclsid int32, sclsid int32) bool {
	if bclsid != int32(netproto.MessageBClassID_Game) {
		return false
	}

	return sclsid == int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID) || sclsid == int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID)
}

// OnMessage 接收訊息
func (h *FreeSpinHander) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	switch netproto.Slot_GameMessageClassID(m.SClassID) {
	case netproto.Slot_GameMessageClassID_Slot_FreeGameReadID:
		return h.Read(ctx, clt, m)
	case netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID:
		return h.Record(ctx, clt, m)
	}

	return []*msg.Message{}
}

// Read 查詢免費遊戲
func (h *FreeSpinHander) Read(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_FreeGameRead{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_MSG_FreeGameRead)

	const ProcedureName = "PrGs_Slot_FreeSpinRead"

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

	data := new(netproto.Slot_MSG_FreeGameRecord)
	info := new(netproto.Slot_FreeSpinDetail)
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
			info.OriginID = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "FreeSpinGroup").(int64); ok {
			info.FreeSpinGroup = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "Times").(int64); ok {
			info.Times = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "UserBet").(int64); ok {
			info.UserBet = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "ChooseRate").(int64); ok {
			info.ChooseRate = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "WinMoney").(int64); ok {
			info.WinMoney = proto.Int64(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "WinTimes").(int64); ok {
			info.WinTimes = proto.Int32(int32(val))
		}

		if val, ok := dbret[0].GetValueByColName(i, "IsCoupon").(int64); ok {
			if val == 1 {
				info.IsCoupon = proto.Bool(true)
			} else {
				info.IsCoupon = proto.Bool(false)
			}
		}
	}

	example.invalid = info

	retMsg := new(msg.Message)
	retMsg.BClassID = int32(netproto.MessageBClassID_Game)
	retMsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
	retMsg.MsgData = data

	return []*msg.Message{retMsg}
}

// Record 更新免費遊戲
func (h *FreeSpinHander) Record(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_FreeGameRecord{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_MSG_FreeGameRecord)

	if example.invalid == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	const ProcedureName = "PrGs_Slot_FreeSpinRecord"
	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", example.invalid.GetUserID())               // 使用者ID
	ps.AddIntInput("intGameID", example.invalid.GetGameID())               // GameID
	ps.AddIntInput("intFlagID", example.invalid.GetFlagID())               // FlagID
	ps.AddBigIntInput("intUserBet", example.invalid.GetUserBet())          // UserBet
	ps.AddIntInput("intFreeSpinGroup", example.invalid.GetFreeSpinGroup()) // FreeSpinGroup
	ps.AddIntInput("intTimes", example.invalid.GetTimes())                 // Times
	ps.AddIntInput("intChooseRate", example.invalid.GetChooseRate())       // ChooseRate
	ps.AddBigIntInput("lngWinMoney", example.invalid.GetWinMoney())        // WinMoney
	if example.invalid.GetIsCoupon() {
		ps.AddTinyIntInput("tnyIsCoupon", 1) // IsCoupon
	} else {
		ps.AddTinyIntInput("tnyIsCoupon", 0) // IsCoupon
	}
	ps.AddBigIntInput("intOriginID", example.invalid.GetOriginID()) // OriginID
	ps.AddIntInput("intWinTimes", example.invalid.GetWinTimes())    // WinTimes
	ps.AddVarcharOutput("chvMsg", "")                       // chvMsg的輸出

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
