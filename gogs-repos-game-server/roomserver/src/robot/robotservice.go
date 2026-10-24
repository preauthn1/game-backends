package robot

import (
	"config"
	"event"
	"gamelogic"
	"math/rand"
	"mlog"
	"module/logicmodule"
	"msg"
	"robot/robotaction"
	"robot/robotcomm"
	"robot/robotlogic"
	"rpcmsg"
	"scorestorage"
	"util"
)

//机器人服务
type RobotService struct {
	robotActionCtx    *robotaction.RobotActionHandlerCtx
	robotLogics       *robotlogic.RobotLogicManager
	gameLogics        *gamelogic.GameLogicManager
	robotKicker       robotaction.RobotKicker
	robotActionHander robotaction.RobotActionHandler
	robotConfig       *robotcomm.RobotConfig
	room              *gamelogic.Room
	actionController  gamelogic.ActionController
	robotManager      *robotcomm.RobotManager
	serverStatus      logicmodule.ServerStatusProvider
	tfManager         *event.TimerFuncManager
}

//创建机器人服务
func NewRobotService(room *gamelogic.Room, tfManager *event.TimerFuncManager, sstorage scorestorage.UserScoreStorage, rpcmsgQueue *rpcmsg.RPCMsgQueue, gameLogics *gamelogic.GameLogicManager, cfgs *config.ConfigGroup, evtChannel *event.EventDispatcher, rblgcf robotlogic.RobotLogicFunc, actionController gamelogic.ActionController, serverStatus logicmodule.ServerStatusProvider) *RobotService {
	//解析机器人配置
	robotCfg := robotcomm.ParseRobotConfig(cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER))
	//机器人管理器
	robotManager := robotcomm.NewRobotManager()

	rbs := new(RobotService)

	//机器人动作处理器上下文
	rbs.robotActionCtx = robotaction.NewRobotActionHandlerCtx()
	rbs.robotActionCtx.Room = room
	rbs.robotActionCtx.RobotManager = robotManager
	rbs.robotActionCtx.TfManager = tfManager
	rbs.robotActionCtx.ScoreStorage = sstorage
	rbs.robotActionCtx.RpcMsgQueue = rpcmsgQueue
	rbs.robotActionCtx.GameLogics = gameLogics
	rbs.robotActionCtx.RobotCfg = robotCfg
	rbs.robotActionCtx.EventChannel = evtChannel
	rbs.robotActionCtx.ActionController = actionController
	rbs.robotActionCtx.ServerStatus = serverStatus

	//如果机器人未开启
	if !robotCfg.Enable {
		rblgcf = robotlogic.EmptyRobotLogicCreator
	}

	rbs.robotLogics = robotlogic.NewRobotLogicManager(room, robotCfg, cfgs, tfManager, rblgcf, gameLogics, evtChannel, robotManager)
	rbs.gameLogics = gameLogics
	rbs.robotConfig = robotCfg
	rbs.room = room
	rbs.actionController = actionController
	rbs.robotManager = robotManager
	rbs.serverStatus = serverStatus
	rbs.tfManager = tfManager

	rbs.initRobotActionHandler(serverStatus)

	return rbs
}

func (rbs *RobotService) GetRobotActionCtx() *robotaction.RobotActionHandlerCtx {
	return rbs.robotActionCtx
}

//发送游戏消息
func (rbs *RobotService) SendGameMessage(ur *gamelogic.GameUser, m *msg.Message) {

	tableno := ur.TableNo
	rglc := rbs.robotLogics.GetRobotLogic(tableno)
	if rglc == nil {
		return
	}

	//如果不是游戏中的用户
	if ur.IsReady() || ur.IsQueue() || ur.IsFree() || ur.IsSit() {
		return
	}

	robot := rbs.GetRobotByID(ur.UserID)
	if robot == nil {
		return
	}

	rglc.OnGameMessage(robot, m)
}

//踢人
func (rbs *RobotService) KickUser(ur *gamelogic.GameUser) {
	rbs.robotKicker.LogoutRobotUser(ur)
}

//游戏开始
func (rbs *RobotService) GameStart(ur *gamelogic.GameUser) {
	tableno := ur.TableNo
	rglc := rbs.robotLogics.GetRobotLogic(tableno)
	if rglc == nil {
		return
	}

	robot := rbs.GetRobotByID(ur.UserID)
	if robot == nil {
		return
	}

	//判断机器人换牌概率
	r := rand.New(util.RndSeed)
	// 智商 * rnd(0-1) >= 10 - 換牌設置
	// 5 * 0.9 >= 10 - 5
	//
	if int32(r.Float32()*float32(robot.RebotLevel)) >= 10-rbs.robotConfig.MaxCardsRndLvl {
		glc := rbs.gameLogics.GetGameLogic(tableno)
		if glc != nil {
			glc.ExchangeMaxCard(ur.UserID)
		}
	}

	rglc.OnGameStart(robot)
}

//游戏结束
func (rbs *RobotService) GameEnd(ur *gamelogic.GameUser) {
	if ur.IsReady() {
		return
	}

	//通知机器人逻辑游戏结束
	tableno := ur.TableNo
	rglc := rbs.robotLogics.GetRobotLogic(tableno)
	if rglc == nil {
		return
	}

	robot := rbs.GetRobotByID(ur.UserID)
	if robot == nil {
		return
	}

	rglc.OnGameEnd(robot)
	//檢查機器人是否需要在遊戲後離開
	if robot.IsLeave() {
		rbs.KickUser(ur)
		return
	}

	//检查机器人金币数
	if ur.Score < rbs.robotConfig.MinMoneyAmount {
		rbs.KickUser(ur)
		return
	}

	//检查服务器状态
	if !rbs.serverStatus.IsServerRuning() {
		rbs.KickUser(ur)
		return
	}

	//检查机器人是否继续游戏
	rbs.checkRobotFree(ur)

}

//检查是否让机器人站立
func (rbs *RobotService) checkRobotFree(ur *gamelogic.GameUser) {
	table := rbs.room.GetTableByNo(ur.TableNo)
	maxuser := rbs.robotConfig.TableMaxUserCount
	curusercount := int32(table.TableUserCount())

	odds := rbs.robotConfig.GameEndLeaveOdds
	if curusercount > maxuser {
		odds = rbs.robotConfig.MaxUserGameEndLeaveOdds
	}

	rndv := util.RndInt(0, 100)

	//随机概率站立
	if int(100*odds) >= rndv {
		rbs.robotFree(ur, nil)

		mlog.Debug("游戏结束,机器人%v随机站立", ur)
		return
	}

	//检查人数
	//	minuser := rbs.robotConfig.TableMinUserCount
	//	maxuser := rbs.robotConfig.TableMaxUserCount

	//	curusercount := int32(table.TableUserCount())

	//	if curusercount <= minuser || curusercount > maxuser {
	//		rbs.robotFree(ur, table)

	//		example.invalid("游戏结束,机器人%v站立.当前桌子人数%d", ur, curusercount)
	//		return
	//	}

	//准备
	rbs.tfManager.NewTimer(rbs.robotConfig.GameReadyRndWaitSpan(), func() {
		if ur.TableNo == table.TableNo {
			rbs.actionController.UserReady(ur)
		}
	})
}

func (rbs *RobotService) robotFree(ur *gamelogic.GameUser, cktable *gamelogic.Table) {
	rbs.tfManager.NewTimer(rbs.robotConfig.GameReadyRndWaitSpan(), func() {
		if ur.IsSit() || ur.IsReady() {
			//需要检查桌子上人数
			if cktable != nil {
				curusercount := int32(cktable.TableUserCount())
				minuser := rbs.robotConfig.TableMinUserCount
				maxuser := rbs.robotConfig.TableMaxUserCount

				if curusercount <= minuser || curusercount > maxuser {
					rbs.actionController.UserFree(ur)
				} else {
					rbs.actionController.UserReady(ur)
				}
			} else {
				rbs.actionController.UserFree(ur)
			}

		}
	})
}

//根据用户id获取机器人
func (rbs *RobotService) GetRobotByID(userid int32) *robotcomm.Robot {
	return rbs.robotManager.GetRobot(userid)
}

//启动机器人服务器
func (rbs *RobotService) StartRobotService() {
	if !rbs.robotConfig.Enable {
		return
	}

	rbs.robotActionHander.Start(rbs.robotActionCtx)

}

//获取机器人加载拦截器
func (rbs *RobotService) GetRobotLoaderHookHandler() logicmodule.RPCHookHandler {
	return rbs.robotKicker.(*robotaction.RobotLoader)
}

//初始化机器人动作处理器
func (rbs *RobotService) initRobotActionHandler(serverStatus logicmodule.ServerStatusProvider) {

	rbahg := robotaction.NewRobotActionHandlerGroup()
	//机器人加载
	robotloader := robotaction.NewRobotLoader(serverStatus)
	rbs.robotKicker = robotloader
	rbahg.AddHandler(robotloader)

	if rbs.robotConfig.MatchTableMode == robotaction.MatchTable_PoolingOneRobot {
		//机器人配桌
		rbsqueue := robotaction.NewRobotQueueHandler()
		rbahg.AddHandler(rbsqueue)

		//机器人主动入桌
		rbash := robotaction.NewRobotAutoSitHandler()
		rbahg.AddHandler(rbash)
	} else if rbs.robotConfig.MatchTableMode == robotaction.QueueType_PoolingNRateRobot {
		//Ｎ個機器人配桌
		rbsqueue := robotaction.NewRobotRateQueueHandler()
		rbahg.AddHandler(rbsqueue)
	} else if rbs.robotConfig.MatchTableMode == robotaction.QueueType_FixedRobot {
		//固定配機器人可配置上限
		rbsqueue := robotaction.NewRobotFixedQueueHandler()
		rbahg.AddHandler(rbsqueue)
	} else if rbs.robotConfig.MatchTableMode == robotaction.MatchTable_PoolingUserWaitTimeRobot {
		//當真人超過某數量時調整機器人配桌時間增加
		rbsqueue := robotaction.NewRobotDynamicWaitTimeByRealUserHandler()
		rbahg.AddHandler(rbsqueue)
	}

	rbs.robotActionHander = rbahg
}

func (rbs *RobotService) GetRobotManager() *robotcomm.RobotManager {
	return rbs.robotManager
}

//獲取機器人邏輯管理
func (rbs *RobotService) GetRobotLogicManager() *robotlogic.RobotLogicManager {
	return rbs.robotLogics
}
