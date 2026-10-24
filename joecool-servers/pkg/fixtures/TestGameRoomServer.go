package fixtures

import (
	"path"	
	"os"	

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/gamequeue"
	"example.invalid/ServerTeam/Servers/pkg/gameserver"
	"example.invalid/ServerTeam/Servers/pkg/gameserver/serverdefense"
	"example.invalid/ServerTeam/Servers/pkg/module/logicmodule"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/robot"
	"example.invalid/ServerTeam/Servers/pkg/roomlogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/scorestorage"
)

type TestGameRoomServer struct {
	*logicmodule.GameServerLogicModule
	GameServer           *gameserver.GameServer
	GameLogicManager     *gamelogic.GameLogicManager
	Room                 *gamelogic.Room
	RoomActionController *roomlogic.RoomActionController
	RoomActionManager    *roomlogic.RoomActionManager
	RobotService         *robot.RobotService
	sstorage             scorestorage.UserScoreStorage
}

func NewTestGameRoomServer() (tgrs *TestGameRoomServer) {
	tgrs = new(TestGameRoomServer)
	tgrs.GameServerLogicModule = logicmodule.NewGameServerLogicModule()	
	currentPath, _ := os.Getwd()
	configDir := path.Join(currentPath, "config")	
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
	tgrs.GameServer = NewGameServerByTest([]*network.ServerCfg{gscfg}, servercfg.DBServerAddr, servercfg.NotifyServerAddr, servercfg.HttpServerAddr, int(servercfg.ServerID), conf.ServerType_GameServer, servermode, int(servercfg.GameID), defenseCfg, servercfg.Flag)
	tgrs.PlayerManager = tgrs.GameServer.GameServerRunCtx.PlayerManager
	//createRoom 创建房间对象
	rumanager := gamelogic.NewRoomUserManager(tgrs.GameServer.GameServerRunCtx.PlayerManager)
	tgrs.Room = gamelogic.NewRoom(servercfg, rumanager)

	//超端服務
	tgrs.Room.SuperToolService = roomserver.SuperToolCreator(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.GameServer.GameServerRunCtx.DBMsgQueue)
	tgrs.Room.SuperToolService.Start()

	//创建房间逻辑管理器
	tgrs.GameLogicManager = gamelogic.NewGameLogicManager(tgrs.Room, cfgs, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, roomserver.GameLogicCreator)
	//console 寫分
	tgrs.sstorage = new(scorestorage.ConsoleUserScoreStorage)

	//房间逻辑模块
	tgrs.RoomActionManager = roomlogic.NewRoomActionManager(tgrs.Room.RoomUserManager, tgrs.Room, tgrs.GameLogicManager, nil, tgrs.GameServer, tgrs.sstorage, nil, servercfg, tgrs.ServerStatus)

	//构造游戏撮合器
	tgrs.RoomActionController = roomlogic.NewRoomActionController(tgrs.Room.RoomUserManager, tgrs.RoomActionManager, tgrs.Room, cfgs)

	tgrs.RobotService = robot.NewRobotService(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.sstorage, tgrs.GameServer.GameServerRunCtx.DBMsgQueue, tgrs.GameLogicManager, cfgs, tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, roomserver.RobotLogicCreator, tgrs.RoomActionController, tgrs.ServerStatus)
	//tgrs.robotService = robot.NewRobotService(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.sstorage, tgrs.GameServer.GameServerRunCtx.DBMsgQueue, tgrs.GameLogicManager, cfgs, tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, RobotLogicCreator, tgrs.RoomActionController, tgrs.ServerStatus)
	tgrs.RoomActionManager.SetRobotService(tgrs.RobotService)

	tgrs.RobotService.StartRobotService()

	//tgrs.robotService = robot.NewRobotService(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.sstorage, tgrs.GameServer.GameServerRunCtx.DBMsgQueue, tgrs.GameLogicManager, cfgs, tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, RobotLogicCreator, tgrs.RoomActionController, tgrs.ServerStatus)
	//tgrs.RoomActionManager.SetRobotService(tgrs.RobotService)

	queuetype := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER).GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "queuetype")
	//定时撮合
	if queuetype == roomlogic.QueueType_TimeInterval {
		gmq := gamequeue.NewIntervalMatchGameQueue(tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, tgrs.RoomActionController, cfgs)
		tgrs.RoomActionManager.SetGameQueue(gmq)
	} else if queuetype == roomlogic.QueueType_QueueUserCount { //固定人数撮合
		gmq := gamequeue.NewUserCountGameQueue(tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, tgrs.RoomActionController, cfgs)
		tgrs.RoomActionManager.SetGameQueue(gmq)
	} else if queuetype == roomlogic.QueueType_WinLoseUser {
		gmq := gamequeue.NewWinLoseUserGameQueue(tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, tgrs.RoomActionController, cfgs)
		tgrs.RoomActionManager.SetGameQueue(gmq)
	}
	//设置桌子对应的逻辑实例
	for _, t := range tgrs.Room.Tables {
		t.GameLogic = tgrs.GameLogicManager.GetGameLogic(t.TableNo)
	}
	//_ = roomlogic.NewRoomLogicModule(gs, tgrs.Room, tgrs.GameLogicManager, roomserver.RobotLogicCreator, cfgs)
	return
}

func NewTestGameRoomServerWithConfig(cfgs *config.ConfigGroup) (tgrs *TestGameRoomServer) {
	tgrs = new(TestGameRoomServer)
	tgrs.GameServerLogicModule = logicmodule.NewGameServerLogicModule()
	cfg := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)
	serverInfo := cfgs.GetConfig(gamelogic.CONFIGFILENAME_SERVERINFO)

	//获取配置信息
	servercfg := gamelogic.GetGameServerConfig(serverInfo, cfg)
	servercfg.DealLogType = 0
	//gameserver侦听地址
	gscfg := network.NewServerCfg(servercfg.ServerAddr, conf.TCPSERVERCONN_ROOM_CLIENT, conf.TCPSERVERNAME_ROOM_CLIENT)

	servermode := cfg.GetConfigInt("defense", "servermode")
	defenseCfg := serverdefense.ParseDefenseConfig(cfg)
	tgrs.GameServer = NewGameServerByTest([]*network.ServerCfg{gscfg}, servercfg.DBServerAddr, servercfg.NotifyServerAddr, servercfg.HttpServerAddr, int(servercfg.ServerID), conf.ServerType_GameServer, servermode, int(servercfg.GameID), defenseCfg, servercfg.Flag)
	tgrs.PlayerManager = tgrs.GameServer.GameServerRunCtx.PlayerManager
	//createRoom 创建房间对象
	rumanager := gamelogic.NewRoomUserManager(tgrs.GameServer.GameServerRunCtx.PlayerManager)
	tgrs.Room = gamelogic.NewRoom(servercfg, rumanager)

	//超端服務
	tgrs.Room.SuperToolService = roomserver.SuperToolCreator(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.GameServer.GameServerRunCtx.DBMsgQueue)
	tgrs.Room.SuperToolService.Start()

	//创建房间逻辑管理器
	tgrs.GameLogicManager = gamelogic.NewGameLogicManager(tgrs.Room, cfgs, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, roomserver.GameLogicCreator)
	//console 寫分
	tgrs.sstorage = new(scorestorage.ConsoleUserScoreStorage)

	//房间逻辑模块
	tgrs.RoomActionManager = roomlogic.NewRoomActionManager(tgrs.Room.RoomUserManager, tgrs.Room, tgrs.GameLogicManager, nil, tgrs.GameServer, tgrs.sstorage, nil, servercfg, tgrs.ServerStatus)

	//构造游戏撮合器
	tgrs.RoomActionController = roomlogic.NewRoomActionController(tgrs.Room.RoomUserManager, tgrs.RoomActionManager, tgrs.Room, cfgs)

	tgrs.RobotService = robot.NewRobotService(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.sstorage, tgrs.GameServer.GameServerRunCtx.DBMsgQueue, tgrs.GameLogicManager, cfgs, tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, roomserver.RobotLogicCreator, tgrs.RoomActionController, tgrs.ServerStatus)
	//tgrs.robotService = robot.NewRobotService(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.sstorage, tgrs.GameServer.GameServerRunCtx.DBMsgQueue, tgrs.GameLogicManager, cfgs, tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, RobotLogicCreator, tgrs.RoomActionController, tgrs.ServerStatus)
	tgrs.RoomActionManager.SetRobotService(tgrs.RobotService)

	tgrs.RobotService.StartRobotService()

	//tgrs.robotService = robot.NewRobotService(tgrs.Room, tgrs.GameServer.GameServerRunCtx.TimerFuncManager, tgrs.sstorage, tgrs.GameServer.GameServerRunCtx.DBMsgQueue, tgrs.GameLogicManager, cfgs, tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, RobotLogicCreator, tgrs.RoomActionController, tgrs.ServerStatus)
	//tgrs.RoomActionManager.SetRobotService(tgrs.RobotService)

	queuetype := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER).GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "queuetype")
	//定时撮合
	if queuetype == roomlogic.QueueType_TimeInterval {
		gmq := gamequeue.NewIntervalMatchGameQueue(tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, tgrs.RoomActionController, cfgs)
		tgrs.RoomActionManager.SetGameQueue(gmq)
	} else if queuetype == roomlogic.QueueType_QueueUserCount { //固定人数撮合
		gmq := gamequeue.NewUserCountGameQueue(tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, tgrs.RoomActionController, cfgs)
		tgrs.RoomActionManager.SetGameQueue(gmq)
	} else if queuetype == roomlogic.QueueType_WinLoseUser {
		gmq := gamequeue.NewWinLoseUserGameQueue(tgrs.GameServer.GameServerRunCtx.MsgServer.EventChannel, tgrs.RoomActionController, cfgs)
		tgrs.RoomActionManager.SetGameQueue(gmq)
	}
	//设置桌子对应的逻辑实例
	for _, t := range tgrs.Room.Tables {
		t.GameLogic = tgrs.GameLogicManager.GetGameLogic(t.TableNo)
	}
	//_ = roomlogic.NewRoomLogicModule(gs, tgrs.Room, tgrs.GameLogicManager, roomserver.RobotLogicCreator, cfgs)
	return
}
