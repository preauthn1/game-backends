package hallutil

import (
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

type GameServerProvider interface {
	GetGameServerList() []*netproto.GameServerInfo                              //获取服务器列表
	GetGameServerAddrByFlag(gameid int32, flag string) []*netproto.GameServerAddr //根据flag获取服务器地址
	GetGameServerListByAreaID(areaid int64) []*netproto.GameServerInfo //取得渠道全部可用的遊戲服務器
	GetGameServerAddrByAreaIDAndFlag(areaid int64, gameid int32, flag string) []*netproto.GameServerAddr //根据areaid及flag获取服务器地址
}
