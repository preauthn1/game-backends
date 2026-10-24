package main

import (
	"config"
	"dfdclogic"
	"event"
	"gamelogic"
	"roomserver"
	"rpcmsg"
)

func main() {
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return dfdclogic.NewDFDCLogic(t, tfManager, cfgs)
		// return dfdc.NewLogic(t, tfManager, cfgs)
	}

	//roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
	//	return dfdclogic.NewLhjRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	//}

	roomserver.SuperToolCreator = func(room *gamelogic.Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) gamelogic.ISuperToolService {
		return gamelogic.NewBrGameSuperToolServiceBase(room, tfm, dbMsgQueue)
	}

	roomserver.StartGameLogicServer()
}
