package dbhandler

import (
	"example.invalid/golang/protobuf/proto"

	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/slot/dbhandler/service"
)

// GameLogHandler 遊戲歷程相關處理
type GameLogHandler struct {
	Service *service.GameLogService
}

// NewGameLogHandler create GameLogHandler
func NewGameLogHandler() *GameLogHandler {
	h := new(GameLogHandler)
	h.Service = new(service.GameLogService)

	return h
}

// IsHook 是否Mapping到此Handler
func (h *GameLogHandler) IsHook(bclsid int32, sclsid int32) bool {
	if bclsid != int32(netproto.MessageBClassID_Game) {
		return false
	}

	return sclsid == int32(netproto.Slot_GameMessageClassID_Slot_GameLogReadID) || sclsid == int32(netproto.Slot_GameMessageClassID_Slot_GameLogAddID)
}

// OnMessage 接收訊息
func (h *GameLogHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	switch netproto.Slot_GameMessageClassID(m.SClassID) {
	case netproto.Slot_GameMessageClassID_Slot_GameLogReadID:
		return h.Read(ctx, clt, m)
	case netproto.Slot_GameMessageClassID_Slot_GameLogAddID:
		return h.Add(ctx, clt, m)
	}

	return []*msg.Message{}
}

// Read 讀取遊戲歷程資訊
func (h *GameLogHandler) Read(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_GameLogRead{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_MSG_GameLogRead)

	rowcnt, records, err := h.Service.Search(mm.GetUserID(), mm.GetGameID(), mm.GetPageIndex(), mm.GetPageSize(), mm.GetStartTime(), mm.GetEndTime())
	if err != nil {
		mlog.Error("Read GameLog Error. %v", err)
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	data := new(netproto.Slot_MSG_GameLogRecord)
	data.UserID = proto.Int32(mm.GetUserID())
	data.GameID = proto.Int32(mm.GetGameID())
	data.PageCount = proto.Int32(rowcnt)
	data.Logs = records

	retMsg := new(msg.Message)
	retMsg.BClassID = int32(netproto.MessageBClassID_Game)
	retMsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_GameLogReadRetID)
	retMsg.MsgData = data

	return []*msg.Message{retMsg}
}

// Add 新增遊戲歷程資訊
func (h *GameLogHandler) Add(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_GameLogAdd{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_MSG_GameLogAdd)

	err := h.Service.Add(mm.GetGameType(), mm.GetLogType(), mm.GetUserID(), mm.GetGameID(), mm.GetServerID(), mm.GetCurrentAmount(), mm.GetCurrentCoupon(), mm.GetBetMoney(), mm.GetBetCoupon(), mm.GetWinMoney(), mm.GetFinalScore(), mm.GetFinalCoupon(), mm.GetGameData())
	if err != nil {
		mlog.Error("Add GameLog Error. %v", err)
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	return []*msg.Message{dbutil.GetDBRetMsg(1)}
}
