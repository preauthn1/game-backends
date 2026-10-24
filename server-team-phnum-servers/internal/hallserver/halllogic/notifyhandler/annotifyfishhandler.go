package notifyhandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/player"
)

//捕魚公告
type AnnNotifyFishHandler struct {
}

func NewAnnNotifyFishHandler() *AnnNotifyFishHandler {
	ann := new(AnnNotifyFishHandler)

	return ann
}

func (ann *AnnNotifyFishHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_AnnFishID)
}

func (ann *AnnNotifyFishHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	mmbyte := m.MsgData.([]byte)
	mm := &netproto.BY_NotifyFishMsg{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		ctx.Error("解析捕魚大廳接收notifyserver注册消息错误,%v", err)
		return
	}

	example.invalid("捕魚 大廳接收 公告 : %v", mm.GetTipMessage())

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.BY_GameMessageClassID_BYNotifyFish)
	pdata := new(netproto.BY_NotifyFishMsg)
	pdata.TipMessage = mm.TipMessage
	pmsg.MsgData = pdata

	ctx.Server.SendAllMessage(pmsg)
	example.invalid("广播捕魚公告")
}
