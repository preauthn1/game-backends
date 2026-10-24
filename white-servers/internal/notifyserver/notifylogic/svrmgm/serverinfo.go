package svrmgm

import (
	"fmt"
	"sync"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

//服务器信息
type ServerInfo struct {
	ConnInfo       *network.ConnInfo //网络连接
	ServerType     int               //服务器类型
	ServerID       int               //服务器ID
	GameID         int               //游戏ID,大厅则为0或者-1
	HostAndBackup  int32
	GameServerAddr string
	FlagID         string
}

func (svr *ServerInfo) String() string {
	return fmt.Sprintf("serverid :%d,servertype:%d,gameid:%d,conn:%v,flagid:%v", svr.ServerID, svr.ServerType, svr.GameID, svr.ConnInfo, svr.FlagID)
}

func NewServerInfo(conninfo *network.ConnInfo, serverType int, serverID int, gameid int, hostAndBackup int32, gameServerAddr string, flagId string) *ServerInfo {
	svrinfo := new(ServerInfo)
	svrinfo.ConnInfo = conninfo
	svrinfo.ServerType = serverType
	svrinfo.ServerID = serverID
	svrinfo.GameID = gameid
	svrinfo.HostAndBackup = hostAndBackup
	svrinfo.GameServerAddr = gameServerAddr
	svrinfo.FlagID = flagId

	return svrinfo
}

//服务器列表
type ServerList struct {
	servers map[string]*ServerInfo //服务器信息
	svrLock *sync.RWMutex          //服务器访问锁
}

//创建服务器集合
func NewServerList() *ServerList {
	svrlst := new(ServerList)
	svrlst.servers = make(map[string]*ServerInfo)
	svrlst.svrLock = &sync.RWMutex{}

	return svrlst
}

//注册服务器
func (svrlst *ServerList) RegServer(conninfo *network.ConnInfo, svrType int, svrID int, gameID int, hostAndBackup int32, gameServerAddr string, flagId string) *ServerInfo {
	sign := svrlst.getServerSign(svrType, svrID)
	svrinfo := NewServerInfo(conninfo, svrType, svrID, gameID, hostAndBackup, gameServerAddr, flagId)

	svrlst.svrLock.Lock()
	svrlst.servers[sign] = svrinfo
	svrlst.svrLock.Unlock()

	return svrinfo
}

//获取服务器标识
func (svrlst *ServerList) getServerSign(svrtype int, svrid int) string {
	sign := fmt.Sprintf("%d_%d", svrtype, svrid)

	return sign
}

//获取所有大厅服务器
func (svrlst *ServerList) GetHallServer() []*ServerInfo {
	return svrlst.GetServerByServerType(conf.ServerType_HallServer)
}

//获取所有游戏服务器
func (svrlst *ServerList) GetGameServer() []*ServerInfo {
	return svrlst.GetServerByServerType(conf.ServerType_GameServer)
}

//获取所有大厅服务器
func (svrlst *ServerList) GetServerByServerType(serverType int) []*ServerInfo {
	svrlst.svrLock.Lock()
	defer svrlst.svrLock.Unlock()

	hsvrs := make([]*ServerInfo, 0)

	for _, svr := range svrlst.servers {
		if svr.ServerType == serverType {
			hsvrs = append(hsvrs, svr)
		}
	}

	return hsvrs
}

func (svrlst *ServerList) GetAllServer() []*ServerInfo {
	svrlst.svrLock.Lock()
	defer svrlst.svrLock.Unlock()

	svrs := make([]*ServerInfo, 0)

	for _, svr := range svrlst.servers {
		svrs = append(svrs, svr)
	}

	return svrs
}

//根据游戏ID获取服务器列表
func (svrlst *ServerList) GetServerByGameID(gameid int) []*ServerInfo {
	svrlst.svrLock.Lock()
	defer svrlst.svrLock.Unlock()

	svrs := make([]*ServerInfo, 0)

	for _, svr := range svrlst.servers {
		if svr.GameID == gameid {
			svrs = append(svrs, svr)
		}
	}

	return svrs
}

//查找服务器信息
func (svrlst *ServerList) FindServer(serverType int, serverID int) *ServerInfo {
	svrlst.svrLock.Lock()
	defer svrlst.svrLock.Unlock()

	for _, svr := range svrlst.servers {
		if svr.ServerID == serverID && svr.ServerType == serverType {
			return svr
		}
	}

	return nil
}

//根据连接获取服务器信息
func (svrlst *ServerList) GetServerByConn(conninfo *network.ConnInfo) *ServerInfo {
	svrlst.svrLock.Lock()
	defer svrlst.svrLock.Unlock()

	for _, svr := range svrlst.servers {
		if svr.ConnInfo == conninfo {
			return svr
		}
	}

	return nil
}

//根据连接移除服务器信息
func (svrlst *ServerList) RemoveServerByConn(conninfo *network.ConnInfo) {
	svr := svrlst.GetServerByConn(conninfo)

	if svr == nil {
		return
	}

	sign := svrlst.getServerSign(svr.ServerType, svr.ServerID)

	svrlst.svrLock.Lock()
	delete(svrlst.servers, sign)
	svrlst.svrLock.Unlock()

}
