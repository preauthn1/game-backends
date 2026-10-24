//斗地主

package main

import (
	"config"
	"ddzgamelogic"
	"event"
	"gamelogic"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
)

func main() {
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return ddzgamelogic.NewDDZGameLogic(t, tfManager, cfgs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return ddzgamelogic.NewDDZRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.StartGameLogicServer()
}
