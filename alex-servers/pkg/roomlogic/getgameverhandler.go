package roomlogic

import (
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

type GetGameVerHandler struct {
	verprovider GameVerProvider
}

func NewGetGameVerHandler(verprovider GameVerProvider) *GetGameVerHandler {
	gameverHandler := new(GetGameVerHandler)
	gameverHandler.verprovider = verprovider

	return gameverHandler
}

func (h *GetGameVerHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	example.invalid("client %v request game ver", clt)

	vermsg := new(msg.Message)
	vermsg.BClassID = int32(netproto.MessageBClassID_GameRoom)
	vermsg.SClassID = int32(netproto.GameRoomClassID_GameVerInfoID)

	ver := h.verprovider.GetVer()
	vermsg.MsgData = ver

	return []*msg.Message{vermsg}
}

func (h *GetGameVerHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_GameRoom) && sclsid == int32(netproto.GameRoomClassID_RequestGameVerID)
}
