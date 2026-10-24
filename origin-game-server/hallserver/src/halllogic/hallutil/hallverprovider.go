package hallutil

import (
	"netproto"
	"network"
	"module"
)

type HallVerProvider interface {
	GetVer(ctx *module.MouduleCtx, clt *network.ConnInfo , areaId int64) *netproto.HallVerInfo
	GetShowGameFlagByArea(areaId int64) []*netproto.AreaShowGameFlag
}

//平台版本号加载器
type HallVerLoader interface {
	//加载平台版本号
	LoadHallVer()
}
