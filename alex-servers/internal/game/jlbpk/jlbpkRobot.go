package jlbpk

import (
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
)

//机器人逻辑
type JlbpkRobotLogic struct {
	*robotlogic.RobotLogicBase
}

//新建一个机器人逻辑
func NewJlbpkRobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *JlbpkRobotLogic {
	pRobot := new(JlbpkRobotLogic)
	pRobot.RobotLogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)

	return pRobot
}
