package jzyq

import (
	"reflect"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
)

func TestUser_Bet(t *testing.T) {
	gu, _ := fixtures.NewGameUser(100, "9527", 500)

	u := NewUser(gu)

	t.Run("Bet 400 to Goal", func(t *testing.T) {
		if err := u.Bet(Goal, 400, false); err != nil {
			t.Errorf("Bet return error want %v, but %v", nil, err)
			return
		}
		if u.BetSummary[Goal] != 400 {
			t.Errorf("BetSummary[Goal] want %v, but %v", 400, u.BetSummary[Goal])
		}
	})

	t.Run("Bet 200, No money", func(t *testing.T) {
		err := u.Bet(Goal, 200, false)
		if err == nil {
			t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorMoneyNotEnough{}), err)
			return
		}
		if reflect.TypeOf(err) != reflect.TypeOf(&ErrorMoneyNotEnough{}) {
			t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorMoneyNotEnough{}), reflect.TypeOf(err))
			return
		}
	})

	t.Run("Bet 100 to Lost", func(t *testing.T) {
		err := u.Bet(Lost, 100, false)
		if err == nil {
			t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorAlreadyBetOtherSide{}), err)
			return
		}
		if reflect.TypeOf(err) != reflect.TypeOf(&ErrorAlreadyBetOtherSide{}) {
			t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorAlreadyBetOtherSide{}), reflect.TypeOf(err))
			return
		}
	})

	t.Run("Bet 10 to Goal Inside", func(t *testing.T) {
		if err := u.Bet(GoalInside, 10, false); err != nil {
			t.Errorf("Bet return error want %v, but %v", nil, err)
			return
		}
		if u.BetSummary[GoalInside] != 10 {
			t.Errorf("BetSummary[GoalInside] want %v, but %v", 10, u.BetSummary[GoalInside])
		}
	})

	t.Run("Bet 10 to Lost Outside", func(t *testing.T) {
		if err := u.Bet(LostOutside, 10, false); err != nil {
			t.Errorf("Bet return error want %v, but %v", nil, err)
			return
		}
		if u.BetSummary[LostOutside] != 10 {
			t.Errorf("BetSummary[LostOutside] want %v, but %v", 10, u.BetSummary[LostOutside])
		}
	})
}

func TestUser_Award(t *testing.T) {

	conf := NewBetConfig(DefaultBetTypes...)

	gu, _ := fixtures.NewGameUser(100, "9527", 1000)

	u := NewUser(gu)

	u.Bet(Goal, 100, false)
	u.Bet(GoalInside, 100, false)
	u.Bet(GoalOutside, 100, false)

	u.Bet(LostInside, 100, false)
	u.Bet(LostOutside, 100, false)

	win := u.Award(GoalInside, *conf)

	var expBet int64 = 500
	var expWin int64 = 200 + 380

	if win != expWin {
		t.Errorf("Award return win want %v, but %v", expWin, win)
	}

	sBet := u.BetSum()
	if sBet != expBet {
		t.Errorf("BetSum want %v, but %v", expBet, sBet)
	}

	sWin := u.WinSum()
	if sWin != expWin {
		t.Errorf("WinSum want %v, but %v", expWin, sWin)
	}

	if u.WinSummary[Goal] != 200 {
		t.Errorf("u.WinSummary[Goal] want %v, but %v", 200, u.WinSummary[Goal])
	}
	if u.WinSummary[GoalInside] != 380 {
		t.Errorf("u.WinSummary[GoalInside] want %v, but %v", 380, u.WinSummary[GoalInside])
	}
	if u.WinSummary[GoalOutside] != 0 {
		t.Errorf("u.WinSummary[GoalOutside] want %v, but %v", 0, u.WinSummary[GoalOutside])
	}

	if u.WinSummary[Lost] != 0 {
		t.Errorf("u.WinSummary[Lost] want %v, but %v", 0, u.WinSummary[Lost])
	}
	if u.WinSummary[LostInside] != 0 {
		t.Errorf("u.WinSummary[LostInside] want %v, but %v", 0, u.WinSummary[LostInside])
	}
	if u.WinSummary[LostOutside] != 0 {
		t.Errorf("u.WinSummary[LostOutside] want %v, but %v", 0, u.WinSummary[LostOutside])
	}

}
