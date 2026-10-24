package notifyhandler

import (
	"mlog"
	"module"
	"msg"
	"netproto"
	"player"
	"util"
)

import "example.invalid/golang/protobuf/proto"

type UserNotifyHandler struct {
}

func NewUserNotifyHandler() *UserNotifyHandler {
	attr := new(UserNotifyHandler)

	return attr
}

func (h *UserNotifyHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	defer util.PanicErrStack()

	mmbyte := m.MsgData.([]byte)
	mm := &netproto.UserNotifyList{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		mlog.Error("解析錯誤 UserNotifyHandler")
	}
	for _, info := range mm.InfoList {
		if pr := player.GetPlayerByUserID(info.GetUserID());pr != nil && pr.IsOnline {
			msg := new(msg.Message)
			msg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
			msg.SClassID = int32(netproto.NotifyServerClassID_UserNotifyRetH2CID)
			msg.MsgData = info
			ctx.Server.SendMessageByConnID(pr.Conn.ConnID(), msg)
		}
	}
}

func (h *UserNotifyHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_UserNotifyRetN2HID)
}
