package msghandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/internal/monitorserver/clientframework"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

//找回银行密码
type TipMessageHandler struct {
	callback func(m *msg.Message)
}

func NewTipMessageHandler() *TipMessageHandler {
	reqh := new(TipMessageHandler)
	return reqh
}

func (rh *TipMessageHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Common) && sclsid == int32(netproto.PlatformCommonClassID_TipMessageID)
}

func (rh *TipMessageHandler) OnMessage(client *clientframework.CtxBase, m *msg.Message) {
	msg := new(netproto.TipMessage)
	byteMsgData := m.MsgData.([]byte) //須將interface轉[]byte
	proto.Unmarshal(byteMsgData, msg)
	//mlog.System("連線 %v, 收到提示消息, %v", client.TcpAddr, msg.GetMessage())
}

func (rh *TipMessageHandler) SetCallback(f func(m *msg.Message)) {

}
