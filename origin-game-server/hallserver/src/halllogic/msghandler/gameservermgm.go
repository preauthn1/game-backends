package msghandler

import (
	"command"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"msg"
	"netproto"
	"network"
	"roomlogic"
	"sort"
	"sync"
	"time"
)

//游戏服务器信息
type GameServerInfo struct {
	ServerRegInfo *netproto.GameServerRegInfo //gameserver注册信息
	ConnInfo      *network.ConnInfo           //连接信息
	LastRegTime   time.Time                   //最后一次注册时间
}

//游戏服务器列表
type GameServerList struct {
	innerGameServerList map[int32]*GameServerInfo
	svrInfoLock         *sync.RWMutex
}

//创建游戏服务器列表对象
func NewGameServerList() *GameServerList {
	glist := new(GameServerList)
	glist.innerGameServerList = make(map[int32]*GameServerInfo)
	glist.svrInfoLock = &sync.RWMutex{}

	//添加命令
	command.AddCommand(NewGsMgmCmd(glist))

	return glist
}

//遍历所有游戏服务器信息
func (glist *GameServerList) RangeGameServer(f func(svrinfo *GameServerInfo)) {
	glist.svrInfoLock.RLock()
	defer glist.svrInfoLock.RUnlock()

	var keys []int
	for k := range glist.innerGameServerList {
		keys = append(keys, int(k))
	}
	sort.Ints(keys)

	for _, s := range keys {
		f(glist.innerGameServerList[int32(s)])
	}

}

//遍历所有游戏服务器信息, 使用者少至多排序
func (glist *GameServerList) RangeGameServerOrderbyUserCount(f func(svrinfo *GameServerInfo)) {
	glist.svrInfoLock.RLock()
	defer glist.svrInfoLock.RUnlock()

	var keys []int
	for k := range glist.innerGameServerList {
		keys = append(keys, int(k))
	}
	sort.Ints(keys)

	for skey, s := range keys {
		list1 := glist.innerGameServerList[int32(s)]
		list1AllUserCount := list1.ServerRegInfo.GetUserCount() + list1.ServerRegInfo.GetRobotCount()
		for ckey, c := range keys {
			list2 := glist.innerGameServerList[int32(c)]
			list2AllUserCount := list2.ServerRegInfo.GetUserCount() + list2.ServerRegInfo.GetRobotCount()

			if list2AllUserCount <= list1AllUserCount {
				keys[skey], keys[ckey] = keys[ckey], keys[skey]
			}
		}
	}

	for _, s := range keys {
		if glist.innerGameServerList[int32(s)].ServerRegInfo.GetHostAndBackup() == roomlogic.HostType_Master {
			f(glist.innerGameServerList[int32(s)])
		}
	}

}

//檢查GameServer的Master是否在執行中, 若有則要將目前要註冊的GameServer改為Slave
func (glist *GameServerList) GameServerIsRunChangeToSlave(gsreginfo *netproto.GameServerRegInfo) []*msg.Message {
	msgarr := make([]*msg.Message, 0)
	sameServerId := false
	glist.RangeGameServer(func(svrinfo *GameServerInfo) {
		mlog.Debug("[masterslave] GameServerIsRunChangeToSlave(), svrinfo[%d, %d, %d], gsreginfo[%d, %d, %d]",
			svrinfo.ServerRegInfo.GetGameID(), svrinfo.ServerRegInfo.GetServerID(), svrinfo.ServerRegInfo.GetHostAndBackup(),
			gsreginfo.GetGameID(), gsreginfo.GetServerID(), gsreginfo.GetHostAndBackup())
		mlog.Debug("[masterslave] svrinfo.UserCount[%d], gsreginfo.UserCount[%d]", svrinfo.ServerRegInfo.GetUserCount(), gsreginfo.GetUserCount())
		if svrinfo.ServerRegInfo.GetGameID() == gsreginfo.GetGameID() &&
			svrinfo.ServerRegInfo.GetServerID() == gsreginfo.GetServerID() &&
			svrinfo.ServerRegInfo.GetHostAndBackup() == gsreginfo.GetHostAndBackup() {
			sameServerId = true
		}
	})
	mlog.Debug("sameServerId[%t]", sameServerId)

	if sameServerId {
		return nil
	}

	glist.RangeGameServer(func(svrinfo *GameServerInfo) {
		mlog.Debug("[masterslave] GameServerIsRunChangeToSlave(), svrinfo[%d, %d], gsreginfo[%d, %d, %d]",
			svrinfo.ServerRegInfo.GetGameID(), svrinfo.ServerRegInfo.GetServerID(),
			gsreginfo.GetGameID(), gsreginfo.GetServerID(), gsreginfo.GetHostAndBackup())
		mlog.Debug("[masterslave] svrinfo.UserCount[%d], gsreginfo.UserCount[%d]", svrinfo.ServerRegInfo.GetUserCount(), gsreginfo.GetUserCount())
		if svrinfo.ServerRegInfo.GetGameID() == gsreginfo.GetGameID() &&
			svrinfo.ServerRegInfo.GetFlag() == gsreginfo.GetFlag() &&
			gsreginfo.GetHostAndBackup() == roomlogic.HostType_Master {

			svrHostAndBackUpMsg := new(msg.Message)
			svrHostAndBackUpMsg.BClassID = int32(netproto.MessageBClassID_ServerMgm)
			svrHostAndBackUpMsg.SClassID = int32(netproto.NotifyServerClassID_ServerHostAndBackUp)

			svrHostData := new(netproto.HostAndBackUp)
			svrHostData.Master = new(netproto.HostInfo)
			svrHostData.Slave = new(netproto.HostInfo)

			svrHostData.Master.GameID = proto.Int32(int32(svrinfo.ServerRegInfo.GetGameID()))
			svrHostData.Slave.GameID = proto.Int32(int32(svrinfo.ServerRegInfo.GetGameID()))

			if svrinfo.ServerRegInfo.GetHostAndBackup() == int32(0) {
				svrHostData.Master.ServerID = proto.Int32(int32(svrinfo.ServerRegInfo.GetServerID()))
				svrHostData.Slave.ServerID = proto.Int32(int32(gsreginfo.GetServerID()))
			} else if gsreginfo.GetHostAndBackup() == int32(0) {
				svrHostData.Master.ServerID = proto.Int32(int32(gsreginfo.GetServerID()))
				svrHostData.Slave.ServerID = proto.Int32(int32(svrinfo.ServerRegInfo.GetServerID()))
			}

			mlog.Debug("修改slave gsHost[%v] master[%v] slave[%v]", gsreginfo.GetHostAndBackup(),
				svrHostData.Master.GetServerID(), svrHostData.Slave.GetServerID())

			svrHostAndBackUpMsg.MsgData = svrHostData

			msgarr = append(msgarr, svrHostAndBackUpMsg)
		}
	})

	return msgarr
}

//设置游戏服务器信息
func (glist *GameServerList) SetGameServer(gsreginfo *netproto.GameServerRegInfo, conninfo *network.ConnInfo) {
	glist.svrInfoLock.Lock()
	defer glist.svrInfoLock.Unlock()

	gsinfo := new(GameServerInfo)
	gsinfo.ServerRegInfo = gsreginfo
	gsinfo.ConnInfo = conninfo
	gsinfo.LastRegTime = time.Now()

	glist.innerGameServerList[gsreginfo.GetServerID()] = gsinfo
}

//移除游戏服务器信息
func (glist *GameServerList) RemoveGameServer(serverid int32) {
	if _, ok := glist.innerGameServerList[serverid]; ok {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[GameServerList]移除游戏服务器信息 serverid[%v]", serverid)).System("HallServer壓測時")
		delete(glist.innerGameServerList, serverid)
	}
}

//根据连接信息移除服务器
func (glist *GameServerList) RemoveGameServerByConn(conninfo *network.ConnInfo) {
	glist.svrInfoLock.Lock()
	defer glist.svrInfoLock.Unlock()

	svrid := make([]int32, 0)

	for id, s := range glist.innerGameServerList {
		if s.ConnInfo == conninfo {
			svrid = append(svrid, int32(id))
		}
	}

	for _, id := range svrid {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[GameServerList]移除游戏服务器信息 serverid[%v]", int32(id))).Error("HallServer壓測時")
		delete(glist.innerGameServerList, int32(id))
	}
}

//根据服务器ID获取服务器信息
func (glist *GameServerList) GetGameServerByID(serverid int32) *netproto.GameServerRegInfo {
	glist.svrInfoLock.RLock()
	defer glist.svrInfoLock.RUnlock()

	gsinfo := glist.innerGameServerList[serverid]

	if gsinfo == nil {
		return nil
	}

	return gsinfo.ServerRegInfo
}
