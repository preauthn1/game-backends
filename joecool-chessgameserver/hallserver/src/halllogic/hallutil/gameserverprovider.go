package hallutil

import (
	"netproto"
)

type GameServerProvider interface {
	GetGameServerList() []*netproto.GameServerInfo                              //获取服务器列表
	GetGameServerAddrByFlag(gameid int32, flag string) *netproto.GameServerAddr //根据flag获取服务器地址
}
