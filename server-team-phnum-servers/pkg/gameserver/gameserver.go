package gameserver

import (
	"errors"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/httpsvr"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverdefense"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverdefense/blackip"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverhandler"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/supertool"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/module/logicmodule"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

const (
	GUESTCONNTIMEOUT = 60
)

const (
	SUPERTOOL_DYNAMIC = 1 //配桌型
)

//游戏服务器对象
type GameServer struct {
	DBServerAddr   string                    //dbserver地址
	NSServerAddr   string                    //nsserver地址
	LocalAddr      []*network.ServerCfg      //侦听地址
	HttpServerAddr string                    //http服务侦听地址
	SuperToolCgf   gamelogic.SuperToolConfig //超端設定檔

	GameServerRunCtx *serverhandler.GameServerCtx          //服务器运行环境
	ServerHandler    *serverhandler.GameServerHandlerGroup //服务器组件
	IsRuning         bool                                  //是否运行中
	ServerID         int                                   //服务器ID
	ServerType       int                                   //服务器类型
	ServerMode       int                                   //服务器模式
	GameID           int                                   //游戏ID
	FlagID           string

	msgServer  *msgserver.MsgServer         // 消息服务器
	defenseCfg *serverdefense.DefenseConfig //防护配置

	msgthrottle     int //消息并发数
	rpciplstHandler *blackip.RPCIPBlackListHandler
	rpciplst        *blackip.RPCBlackIPList
	NotifyHandler   *serverhandler.NotifyHandler

	gameHttpSvr      *httpsvr.GameHttpServer //游戏http服务
	gameSuperToolSvr supertool.ISuperTool    //遊戲超端服務
}

//创建gameserver对象
func NewGameServer(cfg []*network.ServerCfg, dbserveraddr string, nsserveraddr string, httpaddr string, serverid int, servertype int, servermode int, gameid int, defenseCfg *serverdefense.DefenseConfig, hostAndBackup int32, superToolCfg gamelogic.SuperToolConfig, flagId string, LianyunID int32) *GameServer {
	//固定服务器为单线程环境
	msgthrottle := 1

	gs := new(GameServer)
	gs.DBServerAddr = dbserveraddr
	gs.NSServerAddr = nsserveraddr
	gs.LocalAddr = cfg
	gs.HttpServerAddr = httpaddr
	gs.SuperToolCgf = superToolCfg

	gs.IsRuning = false
	gs.ServerHandler = serverhandler.NewGameServerHandlerGroup()
	gs.ServerID = serverid
	gs.ServerType = servertype
	gs.ServerMode = servermode
	gs.defenseCfg = defenseCfg
	gs.FlagID = flagId

	//加入服务器逻辑处理器
	gs.ServerHandler.AddHandler(serverhandler.NewLogonCheckHandler(GUESTCONNTIMEOUT))
	gs.ServerHandler.AddHandler(serverhandler.NewGameServerRegHandler(300, int32(serverid), int32(servertype), LianyunID))
	//注册协调服务器
	ntfHandler := serverhandler.NewNotifyHandler(nsserveraddr, serverid, servertype, gameid, hostAndBackup, gs.LocalAddr[0].Addr, flagId)
	command.AddCommand(serverhandler.NewNotifyHandlerCmd(ntfHandler))
	gs.ServerHandler.AddHandler(ntfHandler)
	gs.NotifyHandler = ntfHandler
	gs.msgthrottle = msgthrottle

	return gs
}

//初始化
func (gs *GameServer) Init() error {
	if gs.GameServerRunCtx != nil {
		return errors.New("服务器已经初始化.")
	}

	//创建消息服务器
	svr := msgserver.NewFacadeMsgServer(gs.msgthrottle, gs.ServerMode)
	gs.msgServer = svr

	//加入TCP服务器
	for _, adr := range gs.LocalAddr {
		svr.AddTCPServer(adr)
	}

	//db消息队列
	rpcdbqueue := rpcmsg.NewRPCMsgQueue()

	err := rpcdbqueue.Connect(gs.DBServerAddr)

	if err != nil {
		mlog.Error("连接到dbserver失败,%v,服务器启动失败", err)
		return err
	}
	mlog.System("连接dbserver成功.")
	rpcdbqueue.Start()
	mlog.System("db 消息队列启动.")

	//用户管理器
	playerManager := player.NewPlayerManager(svr)
	//模块服务器
	ms := module.NewModuleServer(svr)

	gs.GameServerRunCtx = new(serverhandler.GameServerCtx)

	gs.GameServerRunCtx.DBMsgQueue = rpcdbqueue
	gs.GameServerRunCtx.MsgServer = svr
	gs.GameServerRunCtx.ModuleServer = ms
	gs.GameServerRunCtx.PlayerManager = playerManager
	gs.GameServerRunCtx.TimerFuncManager = event.NewTimerFuncManager(svr.EventChannel)
	gs.GameServerRunCtx.Throttle = gs.msgthrottle

	//初始化服务器防护方案
	gs.initServerDefense()

	//加入rpc ip黑名单命令
	command.AddCommand(blackip.NewRPCBlackIPListCommand(gs.rpciplst))

	//初始化Http服务
	if gs.HttpServerAddr != "" {
		gs.gameHttpSvr = httpsvr.NewGameHttpServer(gs.HttpServerAddr, gs.msgServer.EventChannel)
	}

	//初始化超端服務
	if gs.SuperToolCgf.SuperToolModeType == SUPERTOOL_DYNAMIC {
		gs.gameSuperToolSvr = supertool.NewDynamicSuperTool(rpcdbqueue, gs.ServerID, gs.GameID, gs.FlagID)
	}

	return nil

}

//启动服务器
func (gs *GameServer) Start() error {
	if gs.IsRuning {
		return errors.New("服务器已经启动")
	}

	err := gs.GameServerRunCtx.ModuleServer.Run()

	if err != nil {
		return err
	}

	gs.ServerHandler.Start(gs.GameServerRunCtx)
	gs.IsRuning = true

	//启动http服务
	if gs.gameHttpSvr != nil {
		gs.gameHttpSvr.Start()
	}

	//啟動超端服務
	if gs.gameSuperToolSvr != nil {
		gs.gameSuperToolSvr.Start()
	}

	return nil
}

func (gs *GameServer) Stop() {
	if !gs.IsRuning {
		return
	}

	gs.IsRuning = false
	gs.ServerHandler.Stop()
	gs.GameServerRunCtx.DBMsgQueue.Stop()
	gs.GameServerRunCtx.ModuleServer.Stop()
	gs.GameServerRunCtx.MsgServer.Stop()

}

//初始化服务器防护
func (gs *GameServer) initServerDefense() {
	if !gs.defenseCfg.Enable {
		return
	}

	blockiptime := gs.defenseCfg.BlockIPTime
	cc_maxconn := gs.defenseCfg.MaxConn
	cc_conntimes := gs.defenseCfg.ConnTimesLimit
	cc_senddata := gs.defenseCfg.SendDataLimint
	cc_sendmsg := gs.defenseCfg.SendMsgLimit

	proto_default := gs.defenseCfg.ProtoLimitDefault
	proto_special := gs.defenseCfg.ProtoLimitSpecial

	whiteiplist := gs.defenseCfg.WhiteIPList

	//当前网络连接遍历
	connprovider := serverdefense.NewServerConnListProvider(gs.msgServer)

	//ip黑名单
	//rpc ip黑名单处理器
	gs.rpciplstHandler = blackip.NewRPCIPBlackListHandler(gs.GameServerRunCtx.DBMsgQueue, int32(gs.ServerID), gs.defenseCfg.ServerDesc)

	loadipfunc := func(f func(map[string]string)) {
		gs.rpciplstHandler.LoadBlockIPList(f)
	}
	blockipfunc := func(ip string, sec int, memo string) {
		gs.rpciplstHandler.BlockIP(ip, sec, memo)
	}
	iplistprovider := blackip.NewRPCBlackIPList(60, loadipfunc, blockipfunc)
	gs.rpciplst = iplistprovider

	//cc防护
	ccdef := serverdefense.NewCCDefense(cc_maxconn, nil, nil, nil)
	ccdef.ParseConnTimesLimit(cc_conntimes)
	ccdef.ParseSendDataLimit(cc_senddata)
	ccdef.ParseSendMsgLimit(cc_sendmsg)
	ccdef.BlockIPTime = blockiptime
	ccdef.WhiteIPList = whiteiplist

	ccdef.SetConnProvider(connprovider)
	ccdef.SetIPListProvider(iplistprovider)
	mlog.System("初始化cc防护成功")

	//协议防护
	protodef := serverdefense.NewMsgProtoDefense(nil, nil)
	protodef.ParseDefaultProtoLimit(proto_default)
	protodef.ParseSpecialProtoLimit(proto_special)
	protodef.BlockIPTime = blockiptime
	protodef.WhiteIPList = whiteiplist

	protodef.SetConnProvider(connprovider)
	protodef.SetIPListProvider(iplistprovider)
	mlog.System("初始化协议防护成功")

	//黑名单防护
	blockipdef := serverdefense.NewBlackIPDefense()
	blockipdef.WhiteIPList = whiteiplist

	blockipdef.SetConnProvider(connprovider)
	blockipdef.SetIPListProvider(iplistprovider)
	mlog.System("初始化黑名单防护成功")

	//注册消息服务器逻辑组件
	gs.msgServer.GetLogicHandlerGroup().RegisterHandler(ccdef).RegisterHandler(protodef).RegisterHandler(blockipdef)

}

//初始化rpc ip黑名单
func (gs *GameServer) InitRPCBlackIPList(rpchkHGroup *logicmodule.RPCHookHandlerGroup) {
	if gs.defenseCfg.Enable {
		rpchkHGroup.AddHandler(gs.rpciplstHandler)
		gs.rpciplst.StartLoadIPList()
	}
}
