package dfdc

import (
	"fmt"
	"testing"
)

func TestJackpotGame_GenerateProcess(t *testing.T) {

	game := new(JackpotGame)

	jackpot1 := &Jackpot{Level: JackpotGrand}
	jackpot2 := &Jackpot{Level: JackpotMajor}
	jackpot3 := &Jackpot{Level: JackpotMinor}
	jackpot4 := &Jackpot{Level: JackpotMini}

	game.Jackpots = []*Jackpot{jackpot1, jackpot2, jackpot3, jackpot4}
	game.WinJackpot = jackpot1

	process := game.GenerateProcess()

	for _, row := range process {
		fmt.Println(row)
	}

}

func TestJackpotGame_GetProcessData(t *testing.T) {

	game := new(JackpotGame)

	jackpot1 := &Jackpot{Level: JackpotGrand}
	jackpot2 := &Jackpot{Level: JackpotMajor}
	jackpot3 := &Jackpot{Level: JackpotMinor}
	jackpot4 := &Jackpot{Level: JackpotMini}

	game.Jackpots = []*Jackpot{jackpot1, jackpot2, jackpot3, jackpot4}
	game.WinJackpot = jackpot1

	process := game.GenerateProcess()

	for _, row := range process {
		fmt.Println(row)
	}

	game.Process = process

	fmt.Println(game.GetProcessData())

}
