package dbhandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/internal/game/xmbx/dbhandler/service"

	dbutil "example.invalid/ServerTeam/Servers/pkg/db/util"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// LightningHandler Jackpot相關處理
type LightningHandler struct {
	Service *service.LightningService
}

// NewLightningHandler create LightningHandler
func NewLightningHandler() *LightningHandler {
	h := new(LightningHandler)
	h.Service = new(service.LightningService)

	return h
}

// IsHook 是否Mapping到此Handler
func (h *LightningHandler) IsHook(bclsid int32, sclsid int32) bool {
	if bclsid != int32(netproto.MessageBClassID_Game) {
		return false
	}

	return sclsid == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadID) || sclsid == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningRecordID)
}

// OnMessage 接收訊息
func (h *LightningHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	switch netproto.XMBX_GameMessageClassID(m.SClassID) {
	case netproto.XMBX_GameMessageClassID_XMBX_LightningReadID:
		return h.Read(ctx, clt, m)
	case netproto.XMBX_GameMessageClassID_XMBX_LightningRecordID:
		return h.Record(ctx, clt, m)
	}

	return []*msg.Message{}
}

// Read 讀取Lightning資訊
func (h *LightningHandler) Read(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.XMBX_MSG_LightningRead{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.XMBX_MSG_LightningRead)

	_data, err := h.Service.Read(mm.GetUserID(), mm.GetGameID(), mm.GetFlagID())
	if err != nil {
		mlog.Error("Read Lightning Error. %v", err)
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	data := new(netproto.XMBX_MSG_LightningRecord)
	data.UserID = proto.Int32(mm.GetUserID())
	data.GameID = proto.Int32(mm.GetGameID())
	data.FlagID = proto.Int32(mm.GetFlagID())
	data.Data = proto.String(_data)

	retMsg := new(msg.Message)
	retMsg.BClassID = int32(netproto.MessageBClassID_Game)
	retMsg.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadRetID)
	retMsg.MsgData = data

	return []*msg.Message{retMsg}
}

// Record 紀錄Lightning資訊
func (h *LightningHandler) Record(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.XMBX_MSG_LightningRecord{}
		return mm
	})

	if rmm == nil {
		mlog.Error("Record Lightning Error. Parse Message Error.")
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.XMBX_MSG_LightningRecord)

	if err := h.Service.Record(mm.GetUserID(), mm.GetGameID(), mm.GetFlagID(), mm.GetData()); err != nil {
		mlog.Error("Record Lightning Error. %v", err)
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	return []*msg.Message{dbutil.GetDBRetMsg(1)}

}
