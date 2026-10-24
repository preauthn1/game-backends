package main

import (
	common "example.invalid/ServerTeam/Servers/internal/game/common-slot"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

func main() {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return common.NewLogic(t, tfManager, cfgs)
	}

	roomserver.StartGameLogicServer()
}
