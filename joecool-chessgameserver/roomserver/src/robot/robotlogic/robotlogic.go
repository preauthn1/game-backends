package robotlogic

import (
	"config"
	"event"
	"gamelogic"
	"msg"
	"robot/robotcomm"
)

type RobotLogicFunc func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) RobotLogic

//机器人逻辑
type RobotLogic interface {
	//响应游戏消息
	OnGameMessage(robot *robotcomm.Robot, m *msg.Message)

	//游戏开始
	OnGameStart(robot *robotcomm.Robot)

	//游戏结束
	OnGameEnd(robot *robotcomm.Robot)
}

//机器人逻辑基类
type RobotLogicBase struct {
	Table        *gamelogic.Table
	TFManager    *event.TimerFuncManager
	Configs      *config.ConfigGroup
	GameLogic    gamelogic.GameLogic
	EventChannel *event.EventDispatcher
	RobotConfig  *robotcomm.RobotConfig
}

//创建机器人逻辑基类对象实例
func NewRobotLogicBase(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *RobotLogicBase {
	robotGlc := new(RobotLogicBase)
	robotGlc.Table = table
	robotGlc.TFManager = tfManager
	robotGlc.Configs = configs
	robotGlc.GameLogic = glc
	robotGlc.EventChannel = evtChannel

	//解析机器人配置
	robotGlc.RobotConfig = robotCfg

	return robotGlc
}

//游戏消息
func (lgc *RobotLogicBase) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {

}

//游戏开始
func (lgc *RobotLogicBase) OnGameStart(robot *robotcomm.Robot) {

}

//游戏结束
func (lgc *RobotLogicBase) OnGameEnd(robot *robotcomm.Robot) {

}

//延迟执行
func (lgc *RobotLogicBase) DelayRun(delay int32, f func()) {
	lgc.TFManager.NewTimer(delay, f)
}

//发送游戏消息
func (lgc *RobotLogicBase) SendGameMessage(ur *gamelogic.GameUser, m *msg.Message) {
	lgc.DelayRun(lgc.RobotConfig.GameActionRndWaitSpan(), func() {
		lgc.GameLogic.GameMessage(ur, m)
	})
}

//发送游戏消息
func (lgc *RobotLogicBase) SendGameMessageNow(ur *gamelogic.GameUser, m *msg.Message) {
	lgc.GameLogic.GameMessage(ur, m)
}
