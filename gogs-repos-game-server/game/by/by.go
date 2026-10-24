package main

import (
	"bylogic"
	"config"
	"event"
	"gamelogic"
	//	"os"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	//	"runtime/pprof"
)

func main() {
	//f, _ := os.Create("pprof.txt")
	//pprof.StartCPUProfile(f)
	//defer pprof.StopCPUProfile()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return bylogic.NewByLogic(t, tfManager, cfgs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return bylogic.NewBYRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.StartGameLogicServer()
}
