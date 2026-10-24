package msghandler

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"

	proto "example.invalid/golang/protobuf/proto"
)

// TGWBonusGameHandler 處理糖果屋的BonusGame
type TGWBonusGameHandler struct{}

// IsHook 是否Mapping到此處理器
func (h *TGWBonusGameHandler) IsHook(bclsid int32, sclsid int32) bool {
	if bclsid != int32(netproto.MessageBClassID_Game) {
		return false
	}

	return sclsid == int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID) || sclsid == int32(netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID) || sclsid == int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID)
}

// OnMessage 接收訊息
func (h *TGWBonusGameHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	switch netproto.TGW_GameMessageClassID(m.SClassID) {
	case netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID:
		return h.ReadBonusGame(ctx, clt, m)
	case netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID:
		return h.ResetBonusGame(ctx, clt, m)
	case netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID:
		return h.RecordBonusGame(ctx, clt, m)
	}

	return []*msg.Message{}
}

// ReadBonusGame 讀取BonusGame
func (h *TGWBonusGameHandler) ReadBonusGame(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.TGW_MSG_ReadBonusGame{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.TGW_MSG_ReadBonusGame)

	const ProcedureName = "PrGs_TGW_BonusGameRecordGet"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", mm.GetUserID()) // 使用者ID
	ps.AddVarcharOutput("chvMsg", "")           // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := DbUtil.ExecProc(proc)
	if err != nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	if dbret.GetReturnValue() != 1 {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	data := new(netproto.TGW_MSG_BonusGameRecord)
	data.UserID = proto.Int32(mm.GetUserID())
	data.Records = make([]*netproto.TGW_BonusGameRecord, 0, len(dbret[0].Rows))

	for i := 0; i < len(dbret[0].Rows); i++ {
		var bonusID, turnID, location int32
		var money int64

		if val, ok := dbret[0].GetValueByColName(i, "TurnID").(int64); ok {
			turnID = int32(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "Location").(int64); ok {
			location = int32(val)
		}

		if val, ok := dbret[0].GetValueByColName(i, "Money").(int64); ok {
			money = val
		}

		if val, ok := dbret[0].GetValueByColName(i, "BonusID").(int64); ok {
			bonusID = int32(val)
		}

		record := new(netproto.TGW_BonusGameRecord)
		record.BonusID = proto.Int32(bonusID)
		record.ID = proto.Int32(turnID)
		record.Location = proto.Int32(location)
		record.RewareValue = proto.Int64(money)

		data.Records = append(data.Records, record)
	}

	retMsg := new(msg.Message)
	retMsg.BClassID = int32(netproto.MessageBClassID_Game)
	retMsg.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameRetID)
	retMsg.MsgData = data

	return []*msg.Message{retMsg}
}

// ResetBonusGame 重置BonusGame
func (h *TGWBonusGameHandler) ResetBonusGame(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.TGW_MSG_ResetBonusGame{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.TGW_MSG_ResetBonusGame)

	const ProcedureName = "PrGs_TGW_BonusGameRecordReset"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", mm.GetUserID()) // 使用者ID
	ps.AddVarcharOutput("chvMsg", "")           // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := DbUtil.ExecProc(proc)
	if err != nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	if dbret.GetReturnValue() != 1 {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	return []*msg.Message{GetDBRetMsg(1)}
}

// RecordBonusGame 更新BonusGame
func (h *TGWBonusGameHandler) RecordBonusGame(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.TGW_MSG_RecordBonusGame{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.TGW_MSG_RecordBonusGame)

	if mm.GetRecords() == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	const ProcedureName = "PrGs_TGW_BonusGameRecordUpdate"

	ps := db.NewSqlParameters()

	ps.AddIntInput("intUserID", mm.GetUserID())                     // 使用者ID
	ps.AddIntInput("intTurnID", mm.GetRecords().GetID())            // 第幾次
	ps.AddIntInput("intLocation", mm.GetRecords().GetLocation())    // 位置
	ps.AddBigIntInput("intMoney", mm.GetRecords().GetRewareValue()) // 金額
	ps.AddIntInput("intBonusID", mm.GetRecords().GetBonusID())      // 原始Bonus
	ps.AddVarcharOutput("chvMsg", "")                               // chvMsg的輸出

	proc := db.NewProcedure(ProcedureName, ps)

	dbret, err := DbUtil.ExecProc(proc)
	if err != nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	if dbret.GetReturnValue() != 1 {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	return []*msg.Message{GetDBRetMsg(1)}
}
