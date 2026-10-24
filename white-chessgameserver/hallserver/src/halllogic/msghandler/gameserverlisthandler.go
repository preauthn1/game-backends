package msghandler

import (
	"halllogic/hallutil"
	"module"
	"msg"
	"netproto"
	"network"
)

type GameServerListHandler struct {
	serverProvider hallutil.GameServerProvider
}

//创建游戏服务器列表处理器对象
func NewGameServerListHandler(svrProvider hallutil.GameServerProvider) *GameServerListHandler {
	gshandler := new(GameServerListHandler)
	gshandler.serverProvider = svrProvider

	return gshandler
}

func (gh *GameServerListHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_RequestServerListID)
}

func (gh *GameServerListHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	example.invalid("clt %v request serverlist", clt)

	mret := new(msg.Message)
	mret.BClassID = int32(netproto.MessageBClassID_Hall)
	mret.SClassID = int32(netproto.HallMsgClassID_ServerListDataID)

	mdata := new(netproto.AllGameServerInfo)
	svrlist := gh.serverProvider.GetGameServerList()
	mdata.ServerList = svrlist

	mret.MsgData = mdata

	return []*msg.Message{mret}
}
