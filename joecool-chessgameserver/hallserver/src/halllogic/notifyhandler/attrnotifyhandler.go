package notifyhandler

import (
	"module"
	"msg"
	"netproto"
	"player"
)

import proto "example.invalid/golang/protobuf/proto"

type AttrNotifyHandler struct {
}

func NewAttrNotifyHandler() *AttrNotifyHandler {
	attr := new(AttrNotifyHandler)

	return attr
}

func (ann *AttrNotifyHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {

	mmbyte := m.MsgData.([]byte)

	mm := &netproto.AttrChangeList{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		ctx.Error("解析用户属性变化消息错误,%v", err)
		return
	}

	for _, attr := range mm.Attrs {
		urid := attr.GetUserID()

		//构造属性改变消息
		attrmsg := new(msg.Message)
		attrmsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
		attrmsg.SClassID = int32(netproto.NotifyServerClassID_AttrChangeID)

		attrdata := new(netproto.AttrChangeList)
		attrdata.Attrs = append(attrdata.Attrs, attr)
		attrmsg.MsgData = attrdata

		//广播属性变化
		if urid == 0 {
			ctx.Server.SendAllMessage(attrmsg)
		} else {
			pr := player.GetPlayerByUserID(urid)
			if pr == nil {
				continue
			}

			//发送消息
			ctx.Server.SendMessageByConnID(pr.Conn.ConnID(), attrmsg)
		}

		example.invalid("通知用户%d属性变化,%v", attr.GetUserID(), attr)
	}
}

func (ann *AttrNotifyHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_AttrChangeID)
}
