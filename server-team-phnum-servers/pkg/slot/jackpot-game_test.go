package slot

import (
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
		t.Log(row)
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
		t.Log(row)
	}

	game.Process = process

	t.Log(game.GetProcessData())

}
