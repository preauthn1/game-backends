package by

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/internal/monitorserver/clientframework"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

type BySettingHandler struct {
}

func NewBySettingHandler() *BySettingHandler {
	return new(BySettingHandler)
}

func (by *BySettingHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.BY_GameMessageClassID_BYSettingID)
}

func (by *BySettingHandler) OnMessage(client *clientframework.CtxBase, m *msg.Message) {
	setting := &netproto.BY_Setting{}
	byteMsgData := m.MsgData.([]byte)
	proto.Unmarshal(byteMsgData, setting)
}

func (by *BySettingHandler) SetCallback(cb func(m *msg.Message)) {

}
