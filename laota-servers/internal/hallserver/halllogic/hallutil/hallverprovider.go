package hallutil

import (
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/module"
)

type HallVerProvider interface {
	GetVer(ctx *module.MouduleCtx, clt *network.ConnInfo , areaId int64) *netproto.HallVerInfo
	GetShowGameFlagByArea(areaId int64) []*netproto.AreaShowGameFlag
	GetGameServerListByArea(areaid int64) []*netproto.PlatformGameServer
}

//平台版本号加载器
type HallVerLoader interface {
	//加载平台版本号
	LoadHallVer()
}
