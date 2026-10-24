package roomlogic

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/module/logicmodule"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/serverhandler"
)

//游戏服务器注册
type GameServerReg struct {
	clientAddr   string
	notifyAddr   string
	httpAddr     string
	serverid     int32
	rpcmsgqueue  *rpcmsg.RPCMsgQueue
	room         *gamelogic.Room
	serverStatus logicmodule.ServerStatusProvider

	gameServerRegRet *netproto.GameServerStartRet
	regHallHandler   *serverhandler.HallServerRegHandler
}

//创建游戏服务器注册处理器对象
func NewGameServerReg(clientAddr string, notifyAddr string, httpAddr string, serverid int32, dbmsgqueue *rpcmsg.RPCMsgQueue, room *gamelogic.Room, serverStatus logicmodule.ServerStatusProvider) *GameServerReg {
	gsreg := new(GameServerReg)
	gsreg.clientAddr = clientAddr
	gsreg.notifyAddr = notifyAddr
	gsreg.httpAddr = httpAddr
	gsreg.serverid = serverid
	gsreg.rpcmsgqueue = dbmsgqueue
	gsreg.room = room
	gsreg.serverStatus = serverStatus

	return gsreg
}

// 將 regHallHandler 配置給 遊戲注冊器使用
func (hsreg *GameServerReg) SetRegHallHandler(regHallHandler *serverhandler.HallServerRegHandler) {
	hsreg.regHallHandler = regHallHandler
}

func (hsreg *GameServerReg) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_GameServerStartRetID)
}

func (hsreg *GameServerReg) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.GameServerStartRet{}
		return mm
	})

	mm := rmm.(*netproto.GameServerStartRet)

	hsreg.gameServerRegRet = mm

	example.invalid("收到dbserver返回注册信息.code:%d,msg:%s,android ver:%s,ios ver:%s,status:%d,msg:%s", mm.GetCode(), mm.GetMessage(), mm.GetAndroidVersion(), mm.GetIOSVersion(), mm.GetStatus(), mm.GetStatusMessage())

	if mm.GetCode() == 0 {
		ctx.Error("向DBServer注册失败.%s", mm.GetMessage())
	} else {
		example.invalid("向DBServer注册成功")
		//收到DBServer注冊返回後即時向HallServer更新客戶端資料
		gv := new(netproto.GameVerInfo)
		gv.AndroidVersion = hsreg.gameServerRegRet.AndroidVersion
		gv.IOSVersion = hsreg.gameServerRegRet.IOSVersion
		hsreg.regHallHandler.RefreshGameVer(gv)
	}

	//服务器未开放
	if mm.GetStatus() == 0 {
		hsreg.serverStatus.SetServerStatus(conf.ServerStatus_Recover, mm.GetStatusMessage())
	}
}

//通知服务器启动
func (hsreg *GameServerReg) SendDBServerStartMsg() {
	svrmsg := new(msg.Message)
	svrmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	svrmsg.SClassID = int32(netproto.DBServerClassID_GameServerStartID)

	svrdata := new(netproto.GameServerStart)
	svrdata.ClientAddr = proto.String(hsreg.clientAddr)
	svrdata.NotifyAddr = proto.String(hsreg.notifyAddr)
	svrdata.ServerID = proto.Int32(hsreg.serverid)
	svrdata.GameID = proto.Int32(hsreg.room.GameInfo.GameID)
	svrdata.LoginMoney = proto.Int32(hsreg.room.LoginMoney)
	svrdata.ServerName = proto.String(hsreg.room.ServerName)
	svrdata.ShowName = proto.String(hsreg.room.Flag)
	svrdata.HttpAddr = proto.String(hsreg.httpAddr)

	svrmsg.MsgData = svrdata

	hsreg.rpcmsgqueue.PushMessage(netproto.CreateEmptyRPCInfo(hsreg.serverid), false, svrmsg)

}

type GameVerProvider interface {
	GetVer() *netproto.GameVerInfo
}

type RPCGameVerProvider struct {
	gsreg *GameServerReg
}

func NewRPCGameVerProvider(gsreg *GameServerReg) *RPCGameVerProvider {
	gvp := new(RPCGameVerProvider)
	gvp.gsreg = gsreg

	return gvp
}

func (gsv *RPCGameVerProvider) GetVer() *netproto.GameVerInfo {
	gv := new(netproto.GameVerInfo)
	if gsv.gsreg.gameServerRegRet != nil {
		gv.AndroidVersion = gsv.gsreg.gameServerRegRet.AndroidVersion
		gv.IOSVersion = gsv.gsreg.gameServerRegRet.IOSVersion
	}

	return gv
}
