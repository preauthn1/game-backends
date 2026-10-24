package svrmgm

import (
	"conf"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"msg"
	"msgserver"
	"netproto"
	"roomlogic"
	"strconv"
	"time"
)

//切換Master與Slave
type SvrMasterSlaveCmd struct {
	msgServer  *msgserver.MsgServer
	serverlist *ServerList
}

//創建master slave命令對象
func NewSvrMasterSlaveCmd(msgSvr *msgserver.MsgServer, serverlist *ServerList) *SvrMasterSlaveCmd {
	mscmd := new(SvrMasterSlaveCmd)
	mscmd.msgServer = msgSvr
	mscmd.serverlist = serverlist

	return mscmd
}

func (msCmd *SvrMasterSlaveCmd) Name() string {
	return "masterslave"
}

func (msCmd *SvrMasterSlaveCmd) Help() string {
	return "切換AB服. usage: masterslave change [gameid] | 切正式. masterslave up [serverid] | 切備原. masterslave down [serverid]"
}

func (msCmd *SvrMasterSlaveCmd) Exec(args []string) string {
	operation := args[0]

	switch operation {
	case "gamelist":
		gsLst := msCmd.serverlist.GetGameServer()
		s := ""
		for _, v := range gsLst {
			s += fmt.Sprintf("gamelist GameId[%v] ServerId[%v] FlagId [%v] HostAndBackup[%v] Addr[%v] \n", v.GameID, v.ServerID, v.FlagID, v.HostAndBackup, v.GameServerAddr)
		}
		return s
	case "up":
		serverId, err := strconv.Atoi(args[1])
		if err != nil {
			return "error, " + err.Error()
		}
		svr := msCmd.serverlist.FindServer(conf.ServerType_GameServer, serverId)
		if svr == nil {
			return fmt.Sprintf("error, server %d not exists.", serverId)
		}
		svr.HostAndBackup = 0
		svrHostAndBackUpMsg := new(msg.Message)
		svrHostAndBackUpMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
		svrHostAndBackUpMsg.SClassID = int32(netproto.NotifyServerClassID_ServerHostAndBackUp)
		svrHostData := new(netproto.HostAndBackUp)
		svrHostData.Master = new(netproto.HostInfo)
		svrHostData.Slave = new(netproto.HostInfo)
		svrHostData.Master.GameID = proto.Int32(int32(svr.GameID))
		svrHostData.Master.ServerID = proto.Int32(int32(svr.ServerID))
		svrHostData.Slave.GameID = proto.Int32(int32(0))
		svrHostData.Slave.ServerID = proto.Int32(int32(0))
		svrHostAndBackUpMsg.MsgData = svrHostData
		msCmd.msgServer.SendMessage(svr.ConnInfo, svrHostAndBackUpMsg)
		return fmt.Sprintf("server %d up done.", serverId)
	case "down":
		serverId, err := strconv.Atoi(args[1])
		if err != nil {
			return "error, " + err.Error()
		}
		svr := msCmd.serverlist.FindServer(conf.ServerType_GameServer, serverId)
		if svr == nil {
			return fmt.Sprintf("error, server %d not exists.", serverId)
		}
		svr.HostAndBackup = 2
		svrHostAndBackUpMsg := new(msg.Message)
		svrHostAndBackUpMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
		svrHostAndBackUpMsg.SClassID = int32(netproto.NotifyServerClassID_ServerHostAndBackUp)
		svrHostData := new(netproto.HostAndBackUp)
		svrHostData.Master = new(netproto.HostInfo)
		svrHostData.Slave = new(netproto.HostInfo)
		svrHostData.Master.GameID = proto.Int32(int32(0))
		svrHostData.Master.ServerID = proto.Int32(int32(0))
		svrHostData.Slave.GameID = proto.Int32(int32(svr.GameID))
		svrHostData.Slave.ServerID = proto.Int32(int32(svr.ServerID))
		svrHostAndBackUpMsg.MsgData = svrHostData
		msCmd.msgServer.SendMessage(svr.ConnInfo, svrHostAndBackUpMsg)
		return fmt.Sprintf("server %d down done.", serverId)
	case "change":
		masterGameId, err := strconv.Atoi(args[1])

		if err != nil {
			return "error, master gameid error"
		}

		gsLst := msCmd.serverlist.GetGameServer()
		masterServers := make([]*ServerInfo , 0)
		slaveServers := make([]*ServerInfo , 0)
		for _, v := range gsLst {
			if v.GameID == masterGameId {
				if v.HostAndBackup == roomlogic.HostType_Master {
					masterServers = append(masterServers, v)
				} else {
					slaveServers = append(slaveServers, v)
				}
			}
		}
		// 我們允許只要同場次有服務器存在就好, 不需要存同場次的服務器數量一定要一樣
		// 從 master 來找 flagid 在 slave 是否存在, 不存在則不允許切換
		validateMS := true
		for _, ms := range masterServers {
			hasSlave := false
			for _, ss := range slaveServers {
				if ms.FlagID == ss.FlagID {
					hasSlave = true
				}
			}
			if hasSlave == false {
				validateMS = false
			}
		}
		if validateMS == false {
			return "error, serverid not match 失敗, 正式與備援沒有對應相同數量"
		}

		//切換
		for _, masterServer := range masterServers {
			var slaveServer *ServerInfo
			for _, ss := range slaveServers { //找出對應場次的slave server做切換
				if masterServer.FlagID == ss.FlagID {
					slaveServer = ss
					break
				}
			}
			svrHostAndBackUpMsg := new(msg.Message)
			svrHostAndBackUpMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
			svrHostAndBackUpMsg.SClassID = int32(netproto.NotifyServerClassID_ServerHostAndBackUp)
			svrHostData := new(netproto.HostAndBackUp)
			svrHostData.Master = new(netproto.HostInfo)
			svrHostData.Slave = new(netproto.HostInfo)
			svrHostData.Master.GameID = proto.Int32(int32(masterGameId))
			svrHostData.Master.ServerID = proto.Int32(int32(slaveServer.ServerID)) //切換
			svrHostData.Slave.GameID = proto.Int32(int32(masterGameId))
			svrHostData.Slave.ServerID = proto.Int32(int32(masterServer.ServerID)) //切換
			svrHostData.Slave.GameServerAddr = proto.String(slaveServer.GameServerAddr) //告訴正式服，備援服的地址
			mlog.System("master slave change, master is %d, slave is %d, client go to gameserveraddr %s", slaveServer.ServerID, masterServer.ServerID, slaveServer.GameServerAddr)
			svrHostAndBackUpMsg.MsgData = svrHostData
			msCmd.msgServer.SendMessage(masterServer.ConnInfo, svrHostAndBackUpMsg)
			time.Sleep(200 * time.Millisecond)
			msCmd.msgServer.SendMessage(slaveServer.ConnInfo, svrHostAndBackUpMsg)
			msCmd.stopGameServerRoom(svrHostData.Slave)
			masterServer.HostAndBackup = roomlogic.HostType_Slave
		}
		for _, ss := range slaveServers {
			ss.HostAndBackup = roomlogic.HostType_Master
		}
		//masterServerId := 0
		//slaveServerId := 0
		//for _, v := range gsLst {
		//	if v.GameID == masterGameId && v.HostAndBackup == roomlogic.HostType_Master {
		//		slaveServerId = v.ServerID
		//	}
		//	if v.GameID == masterGameId && v.HostAndBackup == roomlogic.HostType_Slave {
		//		masterServerId = v.ServerID
		//	}
		//
		//	svrHostAndBackUpMsg := new(msg.Message)
		//	svrHostAndBackUpMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
		//	svrHostAndBackUpMsg.SClassID = int32(netproto.NotifyServerClassID_ServerHostAndBackUp)
		//
		//	svrHostData := new(netproto.HostAndBackUp)
		//	svrHostData.Master = new(netproto.HostInfo)
		//	svrHostData.Slave = new(netproto.HostInfo)
		//	svrHostData.Master.GameID = proto.Int32(int32(masterGameId))
		//	svrHostData.Master.ServerID = proto.Int32(int32(masterServerId))
		//	svrHostData.Slave.GameID = proto.Int32(int32(masterGameId))
		//	svrHostData.Slave.ServerID = proto.Int32(int32(slaveServerId))
		//	svrHostAndBackUpMsg.MsgData = svrHostData
		//	svrSlave := msCmd.serverlist.FindServer(conf.ServerType_GameServer, slaveServerId)
		//	if svrSlave != nil {
		//
		//		for _, g := range gsLst {
		//			if g.ServerID == masterServerId {
		//				svrHostData.Slave.GameServerAddr = proto.String(g.GameServerAddr)
		//				mlog.System("master slave change, master %d, slave %d, client go to gameserveraddr %s", masterServerId, slaveServerId, g.GameServerAddr)
		//				svrHostAndBackUpMsg.MsgData = svrHostData
		//				msCmd.msgServer.SendMessage(svrSlave.ConnInfo, svrHostAndBackUpMsg)
		//			}
		//		}
		//	} else {
		//		mlog.System("slave not find server id : %v", slaveServerId)
		//	}
		//	time.Sleep(200 * time.Millisecond)
		//
		//	svrMaster := msCmd.serverlist.FindServer(conf.ServerType_GameServer, masterServerId)
		//	if svrMaster != nil {
		//		msCmd.msgServer.SendMessage(svrMaster.ConnInfo, svrHostAndBackUpMsg)
		//	} else {
		//		mlog.System("master not find server id : %v", masterServerId)
		//	}
		//	msCmd.stopGameServerRoom(svrHostData.Slave)
		//
		//}
		//
		//for _, v := range gsLst {
		//	if v.GameID == masterGameId {
		//		if v.HostAndBackup == roomlogic.HostType_Master {
		//			v.HostAndBackup = roomlogic.HostType_Slave
		//		} else {
		//			v.HostAndBackup = roomlogic.HostType_Master
		//		}
		//	}
		//}

		return "master slave done."
	}

	return "master slave done."
}

//關閉遊戲房間
func (msCmd *SvrMasterSlaveCmd) stopGameServerRoom(slaveInfo *netproto.HostInfo) {
	svrstopMsg := new(msg.Message)
	svrstopMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
	svrstopMsg.SClassID = int32(netproto.NotifyServerClassID_ServerStopID)

	svrstopData := new(netproto.ServerStop)
	svrstopData.Type = proto.Int32(0)
	svrstopData.Status = proto.Int32(int32(0))

	svrstopData.GameID = slaveInfo.GameID
	svrstopData.ServerID = slaveInfo.ServerID
	svrstopData.Message = proto.String("-")
	svrstopMsg.MsgData = svrstopData

	svrSlave := msCmd.serverlist.FindServer(conf.ServerType_GameServer, int(slaveInfo.GetServerID()))
	if svrSlave != nil {
		msCmd.msgServer.SendMessage(svrSlave.ConnInfo, svrstopMsg)
	}
}
