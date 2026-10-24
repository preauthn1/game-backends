package notifyhandler

import (
	"module"
	"msg"
	"netproto"
	"player"
)

//公告通知处理器
type AnnNotifyHandler struct {
}

func NewAnnNotifyHandler() *AnnNotifyHandler {
	ann := new(AnnNotifyHandler)

	return ann
}

func (ann *AnnNotifyHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	ctx.Server.SendAllMessage(m)

	example.invalid("广播公告")
}

func (ann *AnnNotifyHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_AnnID)
}
