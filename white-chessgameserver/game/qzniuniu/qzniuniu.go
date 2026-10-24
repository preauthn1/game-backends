//抢庄牛牛

package main

import (
	"config"
	"event"
	"gamelogic"
	qznngamelogic "qznngamelogic_rndz"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
)

func main() {
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return qznngamelogic.NewQZNNGameLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return qznngamelogic.NewQZNNRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.StartGameLogicServer()
}
