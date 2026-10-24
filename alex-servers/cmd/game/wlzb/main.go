package main

import (
	"example.invalid/ServerTeam/Servers/internal/game/wlzb"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

func main() {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return wlzb.NewLogic(t, tfManager, cfgs)
	}

	roomserver.StartGameLogicServer()
}
