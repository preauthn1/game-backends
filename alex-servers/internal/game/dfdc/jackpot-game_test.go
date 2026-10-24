package dfdc

import (
	"fmt"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/slot"
)

func TestJackpotGame_GenerateProcess(t *testing.T) {

	game := new(JackpotGame)

	jackpot1 := &slot.Jackpot{Level: slot.JackpotGrand}
	jackpot2 := &slot.Jackpot{Level: slot.JackpotMajor}
	jackpot3 := &slot.Jackpot{Level: slot.JackpotMinor}
	jackpot4 := &slot.Jackpot{Level: slot.JackpotMini}

	game.Jackpots = []*slot.Jackpot{jackpot1, jackpot2, jackpot3, jackpot4}
	game.WinJackpot = jackpot1

	process := game.GenerateProcess()

	for _, row := range process {
		fmt.Println(row)
	}

}

func TestJackpotGame_GetProcessData(t *testing.T) {

	game := new(JackpotGame)

	jackpot1 := &slot.Jackpot{Level: slot.JackpotGrand}
	jackpot2 := &slot.Jackpot{Level: slot.JackpotMajor}
	jackpot3 := &slot.Jackpot{Level: slot.JackpotMinor}
	jackpot4 := &slot.Jackpot{Level: slot.JackpotMini}

	game.Jackpots = []*slot.Jackpot{jackpot1, jackpot2, jackpot3, jackpot4}
	game.WinJackpot = jackpot1

	process := game.GenerateProcess()

	for _, row := range process {
		fmt.Println(row)
	}

	game.Process = process

	fmt.Println(game.GetProcessData())

}
