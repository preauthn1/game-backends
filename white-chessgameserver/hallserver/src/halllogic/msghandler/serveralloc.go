package msghandler

import (
	"fmt"
	"halllogic/hallutil"
	"mlog"
	"netproto"
	"roomlogic"
)

//服务器分配策略
type ServerAllocStrategy interface {
	GetServerStatus(usercount int32) string                                                       //获取服务器状态描述
	GetServerAddr(serverlist *GameServerList, gameid int32, flag string) *netproto.GameServerAddr //分配服务器
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

func (dftstrategy *DefaultServerAllocStrategy) GetServerAddr(serverlist *GameServerList, gameid int32, flag string) *netproto.GameServerAddr {

	var addr *netproto.GameServerAddr
	serverlist.RangeGameServer(func(svr *GameServerInfo) {
		if svr.ServerRegInfo.GetFlag() == flag && svr.ServerRegInfo.GetGameID() == gameid && addr == nil {
			addr = StrategyLiangHao(svr, dftstrategy, gameid, flag, addr)
		}
	})

	if addr != nil {
		mlog.Debug("取得可用 port[%v]", addr.GetPort())
		return addr
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
