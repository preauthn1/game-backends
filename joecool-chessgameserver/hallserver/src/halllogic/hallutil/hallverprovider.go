package hallutil

import (
	"netproto"
)

type HallVerProvider interface {
	GetVer(areaId int64) *netproto.HallVerInfo
}

//平台版本号加载器
type HallVerLoader interface {
	//加载平台版本号
	LoadHallVer()
}
