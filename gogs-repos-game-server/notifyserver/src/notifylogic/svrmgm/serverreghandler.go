package svrmgm

import (
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
)

import (
	"example.invalid/golang/protobuf/proto"
	"msgserver"
	"time"
)

//服务器注册处理器
type ServerRegHandler struct {
	svrlist   *ServerList
	msgServer *msgserver.MsgServer
}

//创建服务器注册处理器
func NewServerRegHandler(svrlist *ServerList, mserver *msgserver.MsgServer) *ServerRegHandler {
	svrreghandler := new(ServerRegHandler)
	svrreghandler.svrlist = svrlist
	svrreghandler.msgServer = mserver
	return svrreghandler
}

func (svrreg *ServerRegHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_ServerRegID)
}

func (svrreg *ServerRegHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.NotifyServerRegInfo{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		ctx.Error("解析notifyserver注册消息错误,%v", err)
		ctx.Server.CloseConn(clt)
		return nil
	}

	example.invalid("收到服务器注册信息,type:%d,serverid:%d, gameid:%d,ip:%s", mm.GetServerType(), mm.GetServerID(), mm.GetGameID(), clt.IPAddress)

	//注册服务器信息
	svrreg.svrlist.RegServer(clt, int(mm.GetServerType()), int(mm.GetServerID()), int(mm.GetGameID()), mm.GetHostAndBackup(), mm.GetGameServerAddr(), mm.GetFlagID())

	if mm.GetGameID() == int32(0) {
		NotifyGameServerRegHall(svrreg, mm)
	}

	return nil
}

//當大廳重啟時, NotifyServer通知GameServer馬上註冊大廳
func NotifyGameServerRegHall(svrReg *ServerRegHandler, data *netproto.NotifyServerRegInfo) {
	svrs := svrReg.svrlist.GetAllServer()
	time.Sleep(2 * time.Second)
	for _, svr := range svrs {
		if int32(svr.GameID) != 0 {
			msg := new(msg.Message)
			msg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
			msg.SClassID = int32(netproto.NotifyServerClassID_ServerHostAndBackUp)
			svrHostData := new(netproto.HostAndBackUp)
			svrHostData.Master = new(netproto.HostInfo)
			svrHostData.Slave = new(netproto.HostInfo)
			mlog.Debug("[masterslave] svr.HostAndBackup[%d]", svr.HostAndBackup)
			if svr.HostAndBackup == int32(0) {
				svrHostData.Master.GameID = proto.Int32(int32(svr.GameID))
				svrHostData.Master.ServerID = proto.Int32(int32(svr.ServerID))
				svrHostData.Slave.GameID = proto.Int32(int32(0))
				svrHostData.Slave.ServerID = proto.Int32(int32(0))
			} else {
				svrHostData.Master.GameID = proto.Int32(int32(0))
				svrHostData.Master.ServerID = proto.Int32(int32(0))
				svrHostData.Slave.GameID = proto.Int32(int32(svr.GameID))
				svrHostData.Slave.ServerID = proto.Int32(int32(svr.ServerID))
			}
			mlog.Debug("[masterslave] svrHostData.Master[%d, %d], Slave[%d, %d]",
				svrHostData.Master.GameID, svrHostData.Master.ServerID, svrHostData.Slave.GameID, svrHostData.Slave.ServerID)

			msg.MsgData = svrHostData
			svrReg.msgServer.SendMessage(svr.ConnInfo, msg)
		}
	}
}
