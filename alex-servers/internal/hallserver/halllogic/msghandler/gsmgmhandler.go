package msghandler

import (
	"fmt"
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//GameServer管理器
type GsMgmHandler struct {
	serverList *GameServerList
	addr       string
}

//创建gameserver管理器对象
func NewGsMgmHandler(serverlist *GameServerList, addr string) *GsMgmHandler {
	h := new(GsMgmHandler)
	h.serverList = serverlist
	h.addr = addr

	return h
}

func (h *GsMgmHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_ServerMgm) && sclsid == int32(netproto.ServerMgmClassID_HallServerRegister)
}

func (h *GsMgmHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	mmbyte := m.MsgData.([]byte)

	mm := &netproto.GameServerRegInfo{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[GsMgmHandler]解析gameserver注册消息错误 serverid[%v] 將關閉客戶連線[%v]", err, clt)).Error("HallServer壓測時")
		ctx.Error("解析gameserver注册消息错误,%v", err)
		ctx.Server.CloseConn(clt)
		return nil
	}

	//检查本地地址
	connAddr := clt.GetConn().LocalAddr()
	_, localport := util.GetIPAndPort(connAddr)

	if strings.Index(h.addr, fmt.Sprintf(":%d", localport)) < 0 {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[GsMgmHandler]收到游戏服务器注册信息gameid:%d,gamename:%s,serverid:%d,servername:%s,ipaddress:%s,port:%d,flag:%s,usercount:%d,loginmoney:%d.但消息来自于:addr:%s.服务器注册侦听地址为%s", mm.GetGameID(), mm.GetGameName(), mm.GetServerID(), mm.GetServerName(), mm.GetIPAddress(), mm.GetPort(), mm.GetFlag(), mm.GetUserCount(), mm.GetLoginMoney(), connAddr, h.addr)).Error("HallServer壓測時")
		ctx.Error("收到游戏服务器注册信息gameid:%d,gamename:%s,serverid:%d,servername:%s,ipaddress:%s,port:%d,flag:%s,usercount:%d,loginmoney:%d.但消息来自于:addr:%s.服务器注册侦听地址为%s", mm.GetGameID(), mm.GetGameName(), mm.GetServerID(), mm.GetServerName(), mm.GetIPAddress(), mm.GetPort(), mm.GetFlag(), mm.GetUserCount(), mm.GetLoginMoney(), connAddr, h.addr)
		ctx.Server.CloseConn(clt)

		return nil
	}

	//msg := h.serverList.GameServerIsRunChangeToSlave(mm)
	h.serverList.GameServerIsRunChangeToSlave(mm)
	h.serverList.SetGameServer(mm, clt)

	return nil
}

//基于分布注册的游戏服务器管理器
type RemoteRegGameServerProvider struct {
	gsmgmhandler     *GsMgmHandler
	svrAllocStrategy ServerAllocStrategy
}

//创建基于分布注册的游戏服务器管理器
func NewRemoteRegGameServerProvider(gsmgmhandler *GsMgmHandler, allocStargety ServerAllocStrategy) *RemoteRegGameServerProvider {
	remoteregprovoider := new(RemoteRegGameServerProvider)
	remoteregprovoider.gsmgmhandler = gsmgmhandler
	remoteregprovoider.svrAllocStrategy = allocStargety

	return remoteregprovoider
}

func (rrgp *RemoteRegGameServerProvider) GetGameServerList() []*netproto.GameServerInfo {
	gslist := rrgp.gsmgmhandler.serverList

	gsmap := make(map[string]*netproto.GameServerInfo)

	gslist.RangeGameServer(func(gs *GameServerInfo) {
		svrkey := fmt.Sprintf("%d_%v", gs.ServerRegInfo.GetGameID(), gs.ServerRegInfo.GetServerID())

		if gs.ServerRegInfo.GetHostAndBackup() == 1 {
			//AB服. 備援轉正主機接手中
			//gsinfo := setGameServerInfo(gs, rrgp)
			//gsmap[svrkey] = gsinfo
		} else if gs.ServerRegInfo.GetHostAndBackup() == 0 {
			gsinfo := setGameServerInfo(gs, rrgp)
			gsmap[svrkey] = gsinfo

			//AB服. 0 A服主機

			for _, v := range gsmap {
				if v.GetGameID() == gs.ServerRegInfo.GetGameID() && v.GetFlag() == gs.ServerRegInfo.GetFlag() {
					break
				}
			}

			gsinfo.AndroidVersion = gs.ServerRegInfo.AndroidVersion
			gsinfo.IOSVersion = gs.ServerRegInfo.IOSVersion

			gsmap[svrkey] = gsinfo

		}
	})

	gsinfos := make([]*netproto.GameServerInfo, 0)

	for _, gs := range gsmap {
		gsinfos = append(gsinfos, gs)
	}

	return gsinfos
}
func setGameServerInfo(gs *GameServerInfo, rrgp *RemoteRegGameServerProvider) *netproto.GameServerInfo {
	gsinfo := new(netproto.GameServerInfo)
	gsinfo.GameID = gs.ServerRegInfo.GameID
	gsinfo.GameName = gs.ServerRegInfo.GameName
	gsinfo.Flag = gs.ServerRegInfo.Flag
	gsinfo.GameType = gs.ServerRegInfo.GameType
	statusname := rrgp.svrAllocStrategy.GetServerStatus(gs.ServerRegInfo.GetUserCount())
	gsinfo.Status = proto.String(statusname)
	gsinfo.LoginMoney = gs.ServerRegInfo.LoginMoney
	gsinfo.BaseScore = gs.ServerRegInfo.BaseScore
	gsinfo.Single = proto.Int32(gs.ServerRegInfo.GetSingle())
	gsinfo.KickOutMoney = gs.ServerRegInfo.KickOutMoney
	gsinfo.MaxOdd = gs.ServerRegInfo.MaxOdd
	gsinfo.Port = gs.ServerRegInfo.Port
	gsinfo.ServerID = gs.ServerRegInfo.ServerID
	return gsinfo
}

func (rrgp *RemoteRegGameServerProvider) GetGameServerAddrByFlag(gameid int32, flag string) []*netproto.GameServerAddr {

	return rrgp.svrAllocStrategy.GetServerAddr(rrgp.gsmgmhandler.serverList, gameid, flag)
}
