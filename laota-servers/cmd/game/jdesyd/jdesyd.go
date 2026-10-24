package main

import (
	"example.invalid/ServerTeam/Servers/internal/game/jdesyd"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
)

func main() {
	roomserver.GameLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) gamelogic.GameLogic {
		return jdesydLogic.NewJdesydLogic(table, tfManager, configs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return jdesydLogic.NewJdesydRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.StartGameLogicServer()
}
