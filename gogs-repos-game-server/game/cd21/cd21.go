package main
import (
	"config"
	"event"
	"gamelogic"
	"cd21gamelogic"
	"roomserver"
	"robot/robotcomm"
	"robot/robotlogic"
)

func main() {
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return cd21gamelogic.NewCD21gameLogic(t, tfManager, cfgs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return cd21gamelogic.NewCD21RobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}
	roomserver.StartGameLogicServer()
}
