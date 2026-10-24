package main

import (
	"config"
	"event"
	"gamelogic"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"rpcmsg"
	"zhajinhualogic"
)

func main() {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return zhajinhualogic.NewZhaJinHuaLogic(t, tfManager, cfgs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return zhajinhualogic.NewZJHRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.SuperToolCreator = func(room *gamelogic.Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) gamelogic.ISuperToolService {
		return zhajinhualogic.NewZjhSuperToolService(room, tfm, dbMsgQueue)
	}

	zhajinhualogic.GetNaturalOddsSetting()

	roomserver.StartGameLogicServer()
}
