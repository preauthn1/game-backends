//2人牛牛

package main

import (
	"config"
	"event"
	"gamelogic"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"srnngamelogic"
)

func main() {
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return srnngamelogic.NewSRNNGameLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return srnngamelogic.NewSRNNRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.StartGameLogicServer()
}
