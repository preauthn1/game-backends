package by

import (
	"example.invalid/ServerTeam/Servers/internal/monitorserver/clientframework"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

type NotifyFishHandler struct{}

func NewNotifyFishHandler() *NotifyFishHandler {
	return new(NotifyFishHandler)
}

func (by *NotifyFishHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.BY_GameMessageClassID_BYNotifyFish)
}

func (by *NotifyFishHandler) OnMessage(client *clientframework.CtxBase, m *msg.Message) {
}

func (by *NotifyFishHandler) SetCallback(cb func(m *msg.Message)) {

}
