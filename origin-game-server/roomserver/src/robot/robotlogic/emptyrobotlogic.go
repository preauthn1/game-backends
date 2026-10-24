package robotlogic

import (
	"config"
	"event"
	"gamelogic"
	"msg"
	"robot/robotcomm"
)

//空的机器人逻辑实现
type EmptyRobotLogic struct {
}

func (rbglc *EmptyRobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {

}

func (rbglc *EmptyRobotLogic) OnGameStart(robot *robotcomm.Robot) {

}

func (rbglc *EmptyRobotLogic) OnGameEnd(robot *robotcomm.Robot) {

}

var emptyRobotLogic *EmptyRobotLogic

func init() {
	emptyRobotLogic = new(EmptyRobotLogic)
}

var EmptyRobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) RobotLogic {
	return emptyRobotLogic
}
