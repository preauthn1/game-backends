package robotaction

import (
	"event"
	"gamelogic"
	"module/logicmodule"
	"robot/robotcomm"
	"rpcmsg"
	"scorestorage"
)

//机器人动作处理器运行环境
type RobotActionHandlerCtx struct {
	Room             *gamelogic.Room
	ActionController gamelogic.ActionController
	RobotManager     *robotcomm.RobotManager
	TfManager        *event.TimerFuncManager
	RobotCfg         *robotcomm.RobotConfig
	RpcMsgQueue      *rpcmsg.RPCMsgQueue
	ScoreStorage     scorestorage.UserScoreStorage
	GameLogics       *gamelogic.GameLogicManager
	EventChannel     *event.EventDispatcher
	ServerStatus     logicmodule.ServerStatusProvider
}

const (
	MatchTable_PoolingOneRobot          = 0 //配桌方式, 輪詢每張桌子每次放一個機器人
	QueueType_PoolingNRateRobot         = 1 //配桌方式, 輪詢每張桌子每次放N個機器人
	QueueType_FixedRobot                = 2 //2固定配機器人可配置上限
	MatchTable_PoolingUserWaitTimeRobot = 3 //當真人超過某數量時調整機器人配桌時間增加
	MatchTable_SaveLossUser             = 4 //拯救输太多的玩家, 让输的玩家拿最好牌, 其他座位玩家都是总输赢为正
)

//创建机器人动作处理器环境对象
func NewRobotActionHandlerCtx() *RobotActionHandlerCtx {
	ctx := new(RobotActionHandlerCtx)

	return ctx
}

//机器人动作处理器接口定义
type RobotActionHandler interface {
	//启动动作
	Start(ctx *RobotActionHandlerCtx)
}

//机器人动作处理器组
type RobotActionHandlerGroup struct {
	handlers []RobotActionHandler
}

//创建机器人动作处理器组
func NewRobotActionHandlerGroup() *RobotActionHandlerGroup {
	actg := new(RobotActionHandlerGroup)

	return actg
}

//添加处理器
func (rahg *RobotActionHandlerGroup) AddHandler(h RobotActionHandler) {
	rahg.handlers = append(rahg.handlers, h)
}

//启动处理器
func (rahg *RobotActionHandlerGroup) Start(ctx *RobotActionHandlerCtx) {
	for _, h := range rahg.handlers {
		h.Start(ctx)
	}
}
