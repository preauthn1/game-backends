package svrmgm

import (
	"mlog"
	"msg"
	"msgserver"
	"netproto"
)

//刷新版本号命令
type RefreshVerCMD struct {
	msgServer  *msgserver.MsgServer
	serverlist *ServerList
}

//创建平台版本号刷新命令
func NewRefreshVerCmd(msgServer *msgserver.MsgServer, serverlist *ServerList) *RefreshVerCMD {
	refreshVerCMD := new(RefreshVerCMD)
	refreshVerCMD.msgServer = msgServer
	refreshVerCMD.serverlist = serverlist

	return refreshVerCMD
}

func (verCmd *RefreshVerCMD) Name() string {
	return "refreshver"
}

func (verCmd *RefreshVerCMD) Help() string {
	return "强制刷新平台版本号.usage:refreshver"
}

func (verCmd *RefreshVerCMD) Exec(args []string) string {
	refreshVerMsg := new(msg.Message)
	refreshVerMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
	refreshVerMsg.SClassID = int32(netproto.NotifyServerClassID_RefreshVersionID)

	refreshVerMsg.MsgData = nil

	hsLst := verCmd.serverlist.GetHallServer()
	if hsLst != nil {
		for _, hs := range hsLst {
			example.invalid("向大厅%v发送更新版本号消息", hs)
			verCmd.msgServer.SendMessage(hs.ConnInfo, refreshVerMsg)
		}
	}

	gsLst := verCmd.serverlist.GetGameServer()
	if gsLst != nil {
		for _, gs := range gsLst {
			example.invalid("向遊戲%v发送更新版本号消息", gs)
			verCmd.msgServer.SendMessage(gs.ConnInfo, refreshVerMsg)
		}
	}

	return "刷新版本号成功"
}


