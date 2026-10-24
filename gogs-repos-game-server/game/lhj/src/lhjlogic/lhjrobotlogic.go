package lhjlogic

import (
	"config"
	"event"
	"gamelogic"
	"msg"
	"robot/robotcomm"
	"robot/robotlogic"
)

type LhjRobotLogic struct {
	*robotlogic.RobotLogicBase
}

//新建一个机器人逻辑实例
func NewLhjRobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *LhjRobotLogic {
	rblgc := new(LhjRobotLogic)
	//rblgc.RobotLogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)
	//rblgc.parseRobotCha(configs)
	//rblgc.gameChas = make(map[int32]*RobotCha)

	return rblgc
}

func (lgc *LhjRobotLogic) OnGameEnd(robot *robotcomm.Robot) {
	//lgc.gameChas = make(map[int32]*RobotCha)
}

func (lgc *LhjRobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {

}
