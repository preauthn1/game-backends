package main

import (
	"consolegamelogic"

	"config"
	"event"
	"gamelogic"
	"roomserver"
)

func main() {
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return consolegamelogic.NewConsoleGameLogic(t, tfManager, cfgs)
	}

	roomserver.StartGameLogicServer()

	//	p := poker.NewPoker(true, false)

	//	fmt.Println(p.String())

	//	p.Shuffle()

	//	fmt.Println(p.String())
}
