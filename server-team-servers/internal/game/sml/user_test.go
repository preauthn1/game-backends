package sml

import (
	"reflect"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
)

func TestUser_Bet(t *testing.T) {

	t.Run("EnableHedge", func(t *testing.T) {
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

		t.Run("Bet 100 to Save", func(t *testing.T) {
			err := u.Bet(Save, 100, false)
			if err == nil {
				t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorAlreadyBetOtherSide{}), err)
				return
			}
			if reflect.TypeOf(err) != reflect.TypeOf(&ErrorAlreadyBetOtherSide{}) {
				t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorAlreadyBetOtherSide{}), reflect.TypeOf(err))
				return
			}
		})

		t.Run("Bet 10 to Goal Right", func(t *testing.T) {
			if err := u.Bet(GoalRight, 10, false); err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}
			if u.BetSummary[GoalRight] != 10 {
				t.Errorf("BetSummary[GoalRight] want %v, but %v", 10, u.BetSummary[GoalRight])
			}
		})

		t.Run("Bet 10 to Save Middle", func(t *testing.T) {
			if err := u.Bet(SaveMiddle, 10, false); err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}
			if u.BetSummary[SaveMiddle] != 10 {
				t.Errorf("BetSummary[SaveMiddle] want %v, but %v", 10, u.BetSummary[SaveMiddle])
			}
		})
	})

	t.Run("DisableHedge", func(t *testing.T) {
		gu, _ := fixtures.NewGameUser(100, "9527", 500)

		u := NewUser(gu)

		t.Run("Bet 400 to Goal", func(t *testing.T) {
			if err := u.Bet(Goal, 400, true); err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}
			if u.BetSummary[Goal] != 400 {
				t.Errorf("BetSummary[Goal] want %v, but %v", 400, u.BetSummary[Goal])
			}
		})

		t.Run("Bet 200, No money", func(t *testing.T) {
			err := u.Bet(Goal, 200, true)
			if err == nil {
				t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorMoneyNotEnough{}), err)
				return
			}
			if reflect.TypeOf(err) != reflect.TypeOf(&ErrorMoneyNotEnough{}) {
				t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorMoneyNotEnough{}), reflect.TypeOf(err))
				return
			}
		})

		t.Run("Bet 100 to Save", func(t *testing.T) {
			err := u.Bet(Save, 100, true)
			if err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}
			if u.BetSummary[Save] != 100 {
				t.Errorf("BetSummary[Save] want %v, but %v", 100, u.BetSummary[Save])
			}
		})

	})
}

func TestUser_Award(t *testing.T) {

	conf := NewBetConfig(DefaultBetTypes...)

	gu, _ := fixtures.NewGameUser(100, "9527", 1000)

	u := NewUser(gu)

	u.Bet(Goal, 100, false)
	u.Bet(GoalLeft, 100, false)
	u.Bet(GoalMiddle, 100, false)
	u.Bet(GoalRight, 100, false)

	u.Bet(SaveLeft, 100, false)
	u.Bet(SaveMiddle, 100, false)
	u.Bet(SaveRight, 100, false)

	win := u.Award(GoalRight, *conf)

	var expBet int64 = 700
	var expWin int64 = 200 + 580

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
	if u.WinSummary[GoalLeft] != 0 {
		t.Errorf("u.WinSummary[GoalLeft] want %v, but %v", 0, u.WinSummary[GoalLeft])
	}
	if u.WinSummary[GoalMiddle] != 0 {
		t.Errorf("u.WinSummary[GoalMiddle] want %v, but %v", 0, u.WinSummary[GoalMiddle])
	}
	if u.WinSummary[GoalRight] != 580 {
		t.Errorf("u.WinSummary[GoalRight] want %v, but %v", 580, u.WinSummary[GoalRight])
	}

	if u.WinSummary[Save] != 0 {
		t.Errorf("u.WinSummary[Save] want %v, but %v", 0, u.WinSummary[Save])
	}
	if u.WinSummary[SaveLeft] != 0 {
		t.Errorf("u.WinSummary[SaveLeft] want %v, but %v", 0, u.WinSummary[SaveLeft])
	}
	if u.WinSummary[SaveMiddle] != 0 {
		t.Errorf("u.WinSummary[SaveMiddle] want %v, but %v", 0, u.WinSummary[SaveMiddle])
	}
	if u.WinSummary[SaveRight] != 0 {
		t.Errorf("u.WinSummary[SaveRight] want %v, but %v", 0, u.WinSummary[SaveRight])
	}
}
