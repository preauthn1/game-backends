package fixtures

import (
	"path"
	"runtime"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/gameserver"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverdefense"
	game_serverhandler "example.invalid/ServerTeam/Servers/pkg/gameserver/serverhandler"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/roomlogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

func NewGameServerByTest(cfg []*network.ServerCfg, dbserveraddr string, nsserveraddr string, httpaddr string, serverid int, servertype int, servermode int, gameid int, defenseCfg *serverdefense.DefenseConfig, flagId string) *gameserver.GameServer {
	gs := new(gameserver.GameServer)
	//固定服务器为单线程环境
	msgthrottle := 1

	gs.DBServerAddr = dbserveraddr
	gs.NSServerAddr = nsserveraddr
	gs.LocalAddr = cfg
	gs.HttpServerAddr = httpaddr
	gs.IsRuning = false
	gs.ServerHandler = game_serverhandler.NewGameServerHandlerGroup()
	gs.ServerID = serverid
	gs.ServerType = servertype
	gs.ServerMode = servermode

	//加入服务器逻辑处理器
	gs.ServerHandler.AddHandler(game_serverhandler.NewLogonCheckHandler(gameserver.GUESTCONNTIMEOUT))
	gs.ServerHandler.AddHandler(game_serverhandler.NewGameServerRegHandler(300, int32(serverid), int32(servertype)))
	//注册协调服务器
	gs.ServerHandler.AddHandler(game_serverhandler.NewNotifyHandler(nsserveraddr, serverid, servertype, gameid, 0, gs.LocalAddr[0].Addr, flagId))

	//创建消息服务器
	svr := msgserver.NewFacadeMsgServer(1, 0)
	//用户管理器
	playerManager := player.NewPlayerManager(svr)
	//模块服务器
	ms := module.NewModuleServer(svr)

	gs.GameServerRunCtx = new(game_serverhandler.GameServerCtx)

	gs.GameServerRunCtx.MsgServer = svr
	gs.GameServerRunCtx.ModuleServer = ms
	gs.GameServerRunCtx.PlayerManager = playerManager
	gs.GameServerRunCtx.TimerFuncManager = event.NewTimerFuncManager(svr.EventChannel)
	gs.GameServerRunCtx.Throttle = msgthrottle
	//攔截RPC隊列
	gs.GameServerRunCtx.DBMsgQueue = NewDBMsgQueueByTest()

	return gs
}

//攔截RPC隊列
func NewDBMsgQueueByTest() *rpcmsg.RPCMsgQueue {
	rt := rpcmsg.NewRPCMsgQueue()
	return rt
}

func StartGameLogicByTest() (glm *gamelogic.GameLogicManager, room *gamelogic.Room) {

	_, filename, _, _ := runtime.Caller(1)
	configDir := path.Dir(filename) + "/../../config"
	cfgs := new(config.ConfigGroup)
	cfgs.LoadConfig(configDir, 0)
	cfg := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)
	serverInfo := cfgs.GetConfig(gamelogic.CONFIGFILENAME_SERVERINFO)

	//获取配置信息
	servercfg := gamelogic.GetGameServerConfig(serverInfo, cfg)
	servercfg.DealLogType = 0
	//gameserver侦听地址
	gscfg := network.NewServerCfg(servercfg.ServerAddr, conf.TCPSERVERCONN_ROOM_CLIENT, conf.TCPSERVERNAME_ROOM_CLIENT)

	servermode := cfg.GetConfigInt("defense", "servermode")
	defenseCfg := serverdefense.ParseDefenseConfig(cfg)
	gs := NewGameServerByTest([]*network.ServerCfg{gscfg}, servercfg.DBServerAddr, servercfg.NotifyServerAddr, servercfg.HttpServerAddr, int(servercfg.ServerID), conf.ServerType_GameServer, servermode, int(servercfg.GameID), defenseCfg, servercfg.Flag)

	//createRoom 创建房间对象
	rumanager := gamelogic.NewRoomUserManager(gs.GameServerRunCtx.PlayerManager)
	room = gamelogic.NewRoom(servercfg, rumanager)
	//创建房间逻辑管理器
	glm = gamelogic.NewGameLogicManager(room, cfgs, gs.GameServerRunCtx.TimerFuncManager, roomserver.GameLogicCreator)
	//房间逻辑模块
	_ = roomlogic.NewRoomLogicModule(gs, room, glm, roomserver.RobotLogicCreator, cfgs)
	return
}
