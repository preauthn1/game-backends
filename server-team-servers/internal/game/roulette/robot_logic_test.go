package roulette

import "testing"

func TestRobotCha_GetBetType(t *testing.T) {

	if testing.Short() {
		t.Skip("skip test in short mode.")
	}

	ch := NewRobotCha(100, 5000, []BetType{Red, Black, Odd, Even, Big, Small})

	for i := 0; i < 100; i++ {
		t.Run("GetBetType", func(t *testing.T) {
			t.Log(ch.GetBetType(nil))
		})
	}
}
