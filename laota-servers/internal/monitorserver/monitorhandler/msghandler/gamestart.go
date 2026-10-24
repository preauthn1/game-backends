package msghandler

import (
	"example.invalid/ServerTeam/Servers/internal/monitorserver/clientframework"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

type GameStartHandler struct {
}

func NewGameStartHandler() *GameStartHandler {
	return new(GameStartHandler)
}

func (by *GameStartHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_GameStartID)
}

func (by *GameStartHandler) OnMessage(client *clientframework.CtxBase, m *msg.Message) {
	mlog.System("遊戲開始")
}

func (by *GameStartHandler) SetCallback(cb func(m *msg.Message)) {

}
