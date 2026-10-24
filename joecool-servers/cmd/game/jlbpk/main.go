package main

import (
	"example.invalid/ServerTeam/Servers/internal/game/jlbpk"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

func main() {

	roomserver.GameLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) gamelogic.GameLogic {
		return jlbpk.NewJlbpkLogic(table, tfManager, configs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return jlbpk.NewJlbpkRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	//roomserver.SuperToolCreator = func(room *gamelogic.Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) gamelogic.ISuperToolService {
	//	return jlbpk.NewJlbpkSuperToolService(room, tfm, dbMsgQueue)
	//}

	roomserver.StartGameLogicServer()
}
