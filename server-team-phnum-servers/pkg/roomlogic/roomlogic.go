package roomlogic

import (
	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/gamequeue"
	"example.invalid/ServerTeam/Servers/pkg/gameserver"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/module/logicmodule"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/robot"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
	"example.invalid/ServerTeam/Servers/pkg/roomutil"
	"example.invalid/ServerTeam/Servers/pkg/scorestorage"
	"example.invalid/ServerTeam/Servers/pkg/serverhandler"
)

const (
	QueueType_TimeInterval   = 0 //撮合方式,定时撮合
	QueueType_QueueUserCount = 1 //撮合方式,排队人数
	QueueType_WinLoseUser    = 2 //輸贏對戰,真人排隊
)

//AB服
const (
	HostType_Master        = 0 //正式
	HostType_SlaveToMaster = 1 //備援轉正式
	HostType_Slave         = 2 //備援
)

//大厅逻辑
type RoomLogic struct {
	*logicmodule.GameServerLogicModule
	room                 *gamelogic.Room
	roomuserManager      *gamelogic.RoomUserManager
	serverCfg            *gamelogic.GameServerConfig
	configs              *config.ConfigGroup
	verProvider          GameVerProvider
	gameServer           *gameserver.GameServer
	gameLogics           *gamelogic.GameLogicManager
	robotService         *robot.RobotService
	sstorage             scorestorage.UserScoreStorage
	roomactionController *RoomActionController
	roomactionmanager    *RoomActionManager
	regHallHandler       *serverhandler.HallServerRegHandler
	//vipUserService       *gamelogic.VipUserService
}

//创建房间逻辑对象
func NewRoomLogicModule(gs *gameserver.GameServer, room *gamelogic.Room, logics *gamelogic.GameLogicManager, robotLogicCreator robotlogic.RobotLogicFunc, cfgs *config.ConfigGroup) *RoomLogic {
	rl := new(RoomLogic)
	rl.GameServerLogicModule = logicmodule.NewGameServerLogicModule()

	rl.gameServer = gs
	rl.room = room
	rl.configs = cfgs
	rl.roomuserManager = room.RoomUserManager
	rl.serverCfg = gamelogic.GetGameServerConfig(cfgs.Configs[gamelogic.CONFIGFILENAME_SERVERINFO], cfgs.Configs[gamelogic.CONFIGFILENAME_GAMESERVER])
	rl.gameLogics = logics

	dbMsgQueue := gs.GameServerRunCtx.DBMsgQueue
	if rl.serverCfg.ScoreMode == gamelogic.ScoreUpdateMode_Logout {
		rl.sstorage = scorestorage.NewLogoutUserScoreStorage(room, dbMsgQueue)
	} else if rl.serverCfg.ScoreMode == gamelogic.ScoreUpdateMode_PiSai {
		rl.sstorage = scorestorage.NewPiSaiScoreStorage(room, dbMsgQueue)
	} else {
		rl.sstorage = scorestorage.NewNowUserScoreStorage(room, dbMsgQueue)
	}

	rl.roomactionmanager = NewRoomActionManager(rl.roomuserManager, rl.room, logics, nil, gs, rl.sstorage, nil, rl.serverCfg, rl.ServerStatus)

	//构造游戏撮合器
	rac := NewRoomActionController(rl.roomuserManager, rl.roomactionmanager, rl.room, cfgs)
	rl.roomactionController = rac

	//初始化机器人服务
	rl.initRobotService(robotLogicCreator)

	//设置机器人逻辑
	rl.roomactionmanager.robotService = rl.robotService

	queuetype := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER).GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "queuetype")

	//定时撮合
	if queuetype == QueueType_TimeInterval {
		gmq := gamequeue.NewIntervalMatchGameQueue(gs.GameServerRunCtx.MsgServer.EventChannel, rac, cfgs)
		rl.roomactionmanager.SetGameQueue(gmq)
	} else if queuetype == QueueType_QueueUserCount { //固定人数撮合
		gmq := gamequeue.NewUserCountGameQueue(gs.GameServerRunCtx.MsgServer.EventChannel, rac, cfgs)
		rl.roomactionmanager.SetGameQueue(gmq)
	} else if queuetype == QueueType_WinLoseUser {
		gmq := gamequeue.NewWinLoseUserGameQueue(gs.GameServerRunCtx.MsgServer.EventChannel, rac, cfgs)
		rl.roomactionmanager.SetGameQueue(gmq)
	}
	//設置監控及黑白名單服務
	//rl.vipUserService = gamelogic.NewVipUserService(room, gs.GameServerRunCtx.TimerFuncManager, gs.GameServerRunCtx.DBMsgQueue)
	//rl.vipUserService.Start()
	//設置詐金花監控名單服務
	//gameid := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER).GetConfigInt("gameserver", "gameid")
	//if gameid == 3 {
	//	rl.zjhVipUserService = gamelogic.NewZjhVipUserService(room, gs.GameServerRunCtx.TimerFuncManager, gs.GameServerRunCtx.DBMsgQueue)
	//	rl.zjhVipUserService.Start()
	//	room.SetZjhVipUserService(rl.zjhVipUserService)
	//	//Zjh超端
	//	rl.RPCMsgHandler.AddHandler(gamelogic.NewRefreshZjhVipUserHandler(rl.zjhVipUserService.GetVipUserProvider()))
	//}

	//gameid := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER).GetConfigInt("gameserver", "gameid")
	//设置桌子对应的逻辑实例
	for _, t := range room.Tables {
		t.GameLogic = logics.GetGameLogic(t.TableNo)
		//t.GameLogic.SetVipUserProvider(rl.vipUserService.GetVipUserProvider())
		//t.GameLogic.SetVipUserService(rl.vipUserService)
		//t.GameLogic.SetZjhVipUserService(rl.zjhVipUserService)
	}

	return rl
}

// 將 regHallHandler 配置給 遊戲注冊器使用
func (rl *RoomLogic) SetRegHallHandler(regHallHandler *serverhandler.HallServerRegHandler) {
	rl.regHallHandler = regHallHandler
}

func (rl *RoomLogic) Name() string {
	return "roomlogic"
}

func (rl *RoomLogic) Run(ctx *module.MouduleCtx) error {

	rl.Init(ctx, rl.gameServer.GameServerRunCtx.PlayerManager, rl.gameServer.GameServerRunCtx.DBMsgQueue, rl.serverCfg.GetHttpServerCfg())

	//启动机器人
	rl.robotService.StartRobotService()

	rl.registerEvtHandler(ctx)

	rl.registerRCPMsgHandler(ctx)

	rl.registerMsgHandler(ctx)

	rl.registerNotifyMsgHandler(ctx)

	rl.reigsterHttpMsgHandler(ctx)

	command.AddCommand(NewRoomLogicCmd(rl))
	command.AddCommand(NewRoomStateCmd(rl))

	return nil
}

func (rl *RoomLogic) Stop() {

}

func (rl *RoomLogic) SetHallregHandler(hallregHandler *serverhandler.HallServerRegHandler) {
	rl.regHallHandler = hallregHandler
}

func (rl *RoomLogic) registerEvtHandler(ctx *module.MouduleCtx) {

	ctx.OnConnect(func(clt *network.ConnInfo) bool {
		ctx.Debug("client %s connnect...", clt)

		//通知用户管理器用户连接
		rl.PlayerManager.SetPlayer(player.NewGuest(clt))

		return true
	})
	ctx.OnDisConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s disconnnect...", clt)

		rl.roomactionmanager.OnUserLeave(clt.ConnID())

		return true
	})
	ctx.OnMessage(func(clt *network.ConnInfo, m *msg.Message) bool {
		//检查服务器状态
		if clt.IsServerConn(conf.TCPSERVERNAME_ROOM_CLIENT) {
			if (rl.ServerStatus.GetServerStatus() == conf.ServerStatus_Close || rl.ServerStatus.GetServerStatus() == conf.ServerStatus_Recover) && !rl.ServerStatus.IsWhiteIP(clt.IPAddress) {
				roomutil.KickUser(rl.ServerStatus.GetStopServerTipMsg(), clt.ConnID(), roomutil.MsgTypeID_RoomKick, ctx.Server)
				return false
			}
		}

		return true
	})

}

func (rl *RoomLogic) registerMsgHandler(ctx *module.MouduleCtx) {
	//房间逻辑处理
	rl.MsgHandler.AddHandler(rl.roomactionmanager)
	//版本号
	rl.MsgHandler.AddHandler(NewGetGameVerHandler(rl.verProvider))
}

func (rl *RoomLogic) registerRCPMsgHandler(ctx *module.MouduleCtx) {
	rl.RPCMsgHandler.AddHandler(rl.roomactionmanager) //房间dbserver返回消息

	//游戏启动注册
	gsreg := NewGameServerReg(rl.serverCfg.RealAddr, rl.serverCfg.NotifyServerAddr, rl.serverCfg.HttpServerAddr, rl.room.ServerID, rl.DBMsgQueue, rl.room, rl.ServerStatus)
	// gsreg := NewGameServerReg(rl.serverCfg.ServerAddr, rl.serverCfg.NotifyServerAddr, rl.serverCfg.HttpServerAddr, rl.room.ServerID, rl.DBMsgQueue, rl.room, rl.ServerStatus)
	// 將 regHallHandler 配置給 遊戲注冊器使用
	gsreg.SetRegHallHandler(rl.regHallHandler)
	rl.RPCMsgHandler.AddHandler(gsreg)
	gsreg.SendDBServerStartMsg()
	rpcverprovider := NewRPCGameVerProvider(gsreg)
	rl.verProvider = rpcverprovider
	//客戶端版本命令
	command.AddCommand(NewCVersionCmd(rl.verProvider))
	//机器人登录
	rl.RPCMsgHandler.AddHandler(rl.robotService.GetRobotLoaderHookHandler())

	//版本號更新, 需要配置 regHallHandler
	rl.RPCMsgHandler.AddHandler(NewRefreshVerHandler(rpcverprovider, rl.regHallHandler))
	//VIP用戶更新
	//rl.RPCMsgHandler.AddHandler(gamelogic.NewRefreshVipUserHandler(rl.vipUserService.GetVipUserProvider()))
	//超端啟動註冊
	rl.RPCMsgHandler.AddHandler(NewSuperToolHandler(gsreg.room, rl.DBMsgQueue, rl.room.ServerID, rl.gameServer.SuperToolCgf, rl.gameServer.GameServerRunCtx.TimerFuncManager, rl.room.GameInfo.GameID, rl.room.Flag))

	rl.RPCMsgHandler.AddHandler(rl.room.SuperToolService.GetSuperToolHandler())
}

//注册通知处理器
func (rl *RoomLogic) registerNotifyMsgHandler(ctx *module.MouduleCtx) {
	rl.NotifyMsgHandler.AddHandler(NewNotifyRefreshVerHandler(rl.DBMsgQueue, rl.room.GameInfo.GameID, rl.room.ServerID))
	rl.NotifyMsgHandler.AddHandler(NewRoomMasterSlaveHandler(rl, rl.DBMsgQueue, rl.room.GameInfo.GameID, rl.room.ServerID))
	rl.NotifyMsgHandler.AddHandler(NewNotifyPayOrderHandler(rl.room.GameInfo.GameID, rl.room.ServerID, rl.roomuserManager, rl.sstorage))
}

//注册Http消息处理器
func (rl *RoomLogic) reigsterHttpMsgHandler(ctx *module.MouduleCtx) {
	//如果要启动Http服务
	if rl.serverCfg.HttpServerAddr != "" {
		//http处理器
		rl.HttpMsgHandler.AddHandler(rl.roomactionmanager)
	}
}

///初始化机器人逻辑
func (rl *RoomLogic) initRobotService(robotLogicCreator robotlogic.RobotLogicFunc) {

	//初始化机器人服务
	rl.robotService = robot.NewRobotService(rl.room, rl.gameServer.GameServerRunCtx.TimerFuncManager, rl.sstorage, rl.gameServer.GameServerRunCtx.DBMsgQueue, rl.gameLogics, rl.configs, rl.gameServer.GameServerRunCtx.MsgServer.EventChannel, robotLogicCreator, rl.roomactionController, rl.ServerStatus)
}

//重新讀取設定檔
func (rl *RoomLogic) ReloadGameServerConfig() {
	cfgs := config.GetConfigGroup(conf.ConfigDirName)

	for i := int32(0); i < rl.room.TableCount; i++ {
		rl.gameLogics.GetGameLogic(i).RefreshConfigGroup(cfgs)
	}
}

//更新AB服
func (rl *RoomLogic) UpdateHostAndBackUp(newHostType int32, gameServerAddr string) {
	rl.roomactionController.room.HostAndBackup = newHostType
	rl.roomactionmanager.room.HostAndBackup = newHostType
	rl.gameServer.NotifyHandler.HostAndBackup = newHostType
	rl.roomactionmanager.room.NewGameServerAddr = gameServerAddr
}

//获取机器人管理器
func (rl *RoomLogic) GetRobotManager() *robotcomm.RobotManager {
	return rl.robotService.GetRobotManager()
}
