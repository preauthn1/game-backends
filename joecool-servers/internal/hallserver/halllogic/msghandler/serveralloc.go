package msghandler

import (
	"fmt"
	"example.invalid/ServerTeam/Servers/internal/hallserver/halllogic/hallutil"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/roomlogic"
)

//服务器分配策略
type ServerAllocStrategy interface {
	GetServerStatus(usercount int32) string                                                       //获取服务器状态描述
	GetServerAddr(serverlist *GameServerList, gameid int32, flag string) []*netproto.GameServerAddr //分配服务器
	GetServerAddrByAreaId(serverlist *GameServerList, areaid int64, gameid int32, flag string) []*netproto.GameServerAddr //分配服务器
}

//默认的服务器分配策略
type DefaultServerAllocStrategy struct {
	Strategy []*hallutil.ServerAllocStrategyList
}

//创建默认的服务器分配策略
func NewDefaultServerAllocStrategy(strategy []*hallutil.ServerAllocStrategyList) *DefaultServerAllocStrategy {
	dftallocStrategy := new(DefaultServerAllocStrategy)
	dftallocStrategy.Strategy = strategy
	return dftallocStrategy
}

func (dftstrategy *DefaultServerAllocStrategy) GetServerStatus(usercount int32) string {
	return "未知"
}

func (dftstrategy *DefaultServerAllocStrategy) GetServerAddr(serverlist *GameServerList, gameid int32, flag string) []*netproto.GameServerAddr {
	addrArray := make([]*netproto.GameServerAddr, 0)
	serverlist.RangeGameServerOrderbyUserCount(func(svr *GameServerInfo) {
		if svr.ServerRegInfo.GetFlag() == flag && svr.ServerRegInfo.GetGameID() == gameid {
			var addr *netproto.GameServerAddr
			addr = StrategyLiangHao(svr, dftstrategy, gameid, flag, addr)
			addrArray = append(addrArray, addr)
		}
	})

	if addrArray != nil && len(addrArray) != 0 {
		return addrArray
	}

	mlog.NewDefaultLogger().SetData(fmt.Sprintf("[GsMgmHandler] gameid[%v] flag[%v] 服务器分配策略找不到適合空間，請儘速洽資訊人員.", gameid, flag)).Error("HallServer")
	return nil
}

func (dftstrategy *DefaultServerAllocStrategy) GetServerAddrByAreaId(serverlist *GameServerList, areaId int64, gameid int32, flag string) []*netproto.GameServerAddr {
	addrArray := make([]*netproto.GameServerAddr, 0)
	serverlist.RangeGameServerOrderbyUserCountByAreaID(areaId, func(svr *GameServerInfo) {
		if svr.ServerRegInfo.GetFlag() == flag && svr.ServerRegInfo.GetGameID() == gameid {
			var addr *netproto.GameServerAddr
			addr = StrategyLiangHao(svr, dftstrategy, gameid, flag, addr)
			addrArray = append(addrArray, addr)
		}
	})

	if addrArray != nil && len(addrArray) != 0 {
		return addrArray
	}

	mlog.NewDefaultLogger().SetData(fmt.Sprintf("[GsMgmHandler] gameid[%v] flag[%v] 服务器分配策略找不到適合空間，請儘速洽資訊人員.", gameid, flag)).Error("HallServer")
	return nil
}

//分配可用空間
func StrategyLiangHao(svr *GameServerInfo, dftstrategy *DefaultServerAllocStrategy, gameid int32, flag string, addr *netproto.GameServerAddr) *netproto.GameServerAddr {
	urcount := svr.ServerRegInfo.GetUserCount() + svr.ServerRegInfo.GetRobotCount()
	for _, v := range dftstrategy.Strategy {
		if _, ok := v.GameIdList[svr.ServerRegInfo.GetGameID()]; ok {
			if urcount <= *v.MaxUser && svr.ServerRegInfo.GetHostAndBackup() == roomlogic.HostType_Master {
				addr = new(netproto.GameServerAddr)
				addr.ServerID = svr.ServerRegInfo.ServerID
				addr.IPAddress = svr.ServerRegInfo.IPAddress
				addr.Port = svr.ServerRegInfo.Port
			}
		}
	}

	return addr
}
