package jzyq

import (
	"math"
	"reflect"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
)

func TestGame_User(t *testing.T) {

	game, _ := NewGame()
	game.BetConfig.AddAllowAmounts(100)

	u1, _ := fixtures.NewGameUser(100, "小明", 5000)
	u2, _ := fixtures.NewGameUser(200, "小華", 3000)
	u3, _ := fixtures.NewGameUser(300, "大寶", 1000)

	t.Run("Add User", func(t *testing.T) {
		t.Run("u1", func(t *testing.T) {
			game.AddUser(u1)
			if game.Users[100] == nil {
				t.Errorf("User %d want exist in game", u1.UserID)
			} else if game.Users[100].GameUser != u1 {
				t.Errorf("User %d GameUser want %p, but %p", u1.UserID, u1, game.Users[100].GameUser)
			}
		})
		t.Run("u2", func(t *testing.T) {
			game.AddUser(u2)
			if game.Users[200] == nil {
				t.Errorf("User %d want exist in game", u2.UserID)
			} else if game.Users[200].GameUser != u2 {
				t.Errorf("User %d GameUser want %p, but %p", u2.UserID, u2, game.Users[200].GameUser)
			}
		})
		t.Run("u3", func(t *testing.T) {
			game.AddUser(u3)
			if game.Users[300] == nil {
				t.Errorf("User %d want exist in game", u3.UserID)
			} else if game.Users[300].GameUser != u3 {
				t.Errorf("User %d GameUser want %p, but %p", u3.UserID, u3, game.Users[300].GameUser)
			}
		})
	})

	t.Run("User Bet", func(t *testing.T) {
		game.Step = WaitBet

		t.Run("u1 Bet 100 in Goal", func(t *testing.T) {
			err := game.Bet(u1.UserID, Goal, 100)
			if err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}

			if game.BetSummary[Goal] != 100 {
				t.Errorf("Game Bet of Goal want %v, but %v", 100, game.BetSummary[Goal])
			}
		})
	})

	t.Run("Delete User", func(t *testing.T) {
		game.DeleteUser(u1.UserID)

		if game.Users[100] != nil {
			t.Errorf("User %d want delete from game", u1.UserID)
		}

		if game.BetSummary[Goal] != 0 {
			t.Errorf("Game Bet of Goal want %v, but %v", 0, game.BetSummary[Goal])
		}
	})

}

func TestGame_Bet(t *testing.T) {

	game, _ := NewGame()
	game.BetConfig.AddAllowAmounts(100, 500, 1000, 5000, 1)

	u1, _ := fixtures.NewGameUser(100, "小明", 50000)

	game.AddUser(u1)

	t.Run("Not in WaitBet step", func(t *testing.T) {
		err := game.Bet(u1.UserID, Goal, 100)
		if err == nil {
			t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorStepNotAllow{}), err)
			return
		}
		if reflect.TypeOf(err) != reflect.TypeOf(&ErrorStepNotAllow{}) {
			t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorStepNotAllow{}), reflect.TypeOf(err))
		}
	})

	game.Step = WaitBet

	game.BetMinCarray = 100000

	t.Run("Less than BetMinCarry", func(t *testing.T) {
		err := game.Bet(u1.UserID, Goal, 100)
		if err == nil {
			t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorCarryMoneyNotEnough{}), err)
			return
		}
		if reflect.TypeOf(err) != reflect.TypeOf(&ErrorCarryMoneyNotEnough{}) {
			t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorCarryMoneyNotEnough{}), reflect.TypeOf(err))
		}
	})

	game.BetMinCarray = 1000

	t.Run("Bet 1000 Goal", func(t *testing.T) {
		err := game.Bet(u1.UserID, Goal, 1000)
		if err != nil {
			t.Errorf("Bet return error want %v, but %v", nil, err)
			return
		}
		if game.BetSummary[Goal] != 1000 {
			t.Errorf("BetSummary Goal want %v, but %v", 1000, game.BetSummary[Goal])
		}
	})

	t.Run("Bet 500 Lost Inside", func(t *testing.T) {
		err := game.Bet(u1.UserID, LostInside, 500)
		if err != nil {
			t.Errorf("Bet return error want %v, but %v", nil, err)
			return
		}
		if game.BetSummary[LostInside] != 500 {
			t.Errorf("BetSummary LostInside want %v, but %v", 500, game.BetSummary[LostInside])
		}
	})

	t.Run("Over Limit Lost Outside", func(t *testing.T) {
		t.Run("Full Limit", func(t *testing.T) {

			var exp int64 = 0

			for i := 0; i < 5; i++ {
				err := game.Bet(u1.UserID, LostOutside, 1000)
				if err != nil {
					t.Errorf("Bet return error want %v, but %v", nil, err)
					return
				}
				exp += 1000
				if game.BetSummary[LostOutside] != exp {
					t.Errorf("BetSummary LostOutside want %v, but %v", exp, game.BetSummary[LostOutside])
				}
			}
		})
		t.Run("Over", func(t *testing.T) {
			err := game.Bet(u1.UserID, LostOutside, 1)
			if err == nil {
				t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorBetTypeOverLimit{}), err)
				return
			}
			if reflect.TypeOf(err) != reflect.TypeOf(&ErrorBetTypeOverLimit{}) {
				t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorBetTypeOverLimit{}), reflect.TypeOf(err))
			}
		})
	})

	game.UserMaxBet = 10000

	t.Run("Over to Personal", func(t *testing.T) {
		t.Run("Full Limit", func(t *testing.T) {
			var exp int64 = 1000
			for i := 0; i < 3; i++ {
				err := game.Bet(u1.UserID, Goal, 1000)
				if err != nil {
					t.Errorf("Bet return error want %v, but %v", nil, err)
					return
				}
				exp += 1000
				if game.BetSummary[Goal] != exp {
					t.Errorf("BetSummary Goal want %v, but %v", exp, game.BetSummary[Goal])
				}
			}

		})

		t.Run("Over", func(t *testing.T) {
			err := game.Bet(u1.UserID, Goal, 1000)
			if err == nil {
				t.Errorf("Bet return error want %v, but %v", reflect.TypeOf(&ErrorBetTypeOverLimit{}), err)
				return
			}
			if reflect.TypeOf(err) != reflect.TypeOf(&ErrorBetTypeOverLimit{}) {
				t.Errorf("Bet return error type want %v, but %v", reflect.TypeOf(&ErrorBetTypeOverLimit{}), reflect.TypeOf(err))
			}
		})
	})

}

func TestGame_Award_AssertNextAward(t *testing.T) {

	game, _ := NewGame()

	game.Next = append(game.Next, GoalInside, GoalOutside, LostOutside)

	t.Run("Goal Inside", func(t *testing.T) {
		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		if award != GoalInside {
			t.Errorf("Award want %v, but %v", GoalInside, award)
		}
	})

	t.Run("Goal Outside", func(t *testing.T) {
		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		if award != GoalOutside {
			t.Errorf("Award want %v, but %v", GoalOutside, award)
		}
	})

	t.Run("Lost Outside", func(t *testing.T) {
		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		if award != LostOutside {
			t.Errorf("Award want %v, but %v", LostOutside, award)
		}
	})
}

func TestGame_Award_UnderWaterLine(t *testing.T) {

	t.Run("Every type will loss", func(t *testing.T) {
		options := []GameOpt{
			&WaterLineCheckerOpt{
				Func: func() bool { return true },
			},
		}

		game, _ := NewGame(options...)
		game.BetConfig.AddAllowAmounts(1, 10, 100, 500)

		u1, _ := fixtures.NewGameUser(100, "9527", 5000)
		u2, _ := fixtures.NewGameUser(200, "阿朱", 5000)
		u3, _ := fixtures.NewGameRobot(300, "阿里不打", 5000)

		game.AddUser(u1)
		game.AddUser(u2)

		game.Step = WaitBet
		game.Bet(u1.UserID, Goal, 500)
		game.Bet(u1.UserID, LostOutside, 10)
		game.Bet(u2.UserID, Lost, 100)
		game.Bet(u2.UserID, LostInside, 1)
		game.Bet(u3.UserInfo.UserID, Lost, 1000)

		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		if award != LostInside {
			t.Errorf("Award want %v, but %v", LostInside, award)
		}
	})

	t.Run("Some type is same loss", func(t *testing.T) {
		options := []GameOpt{
			&WaterLineCheckerOpt{
				Func: func() bool { return true },
			},
		}

		game, _ := NewGame(options...)
		game.BetConfig.AddAllowAmounts(1, 10, 100, 500)

		u1, _ := fixtures.NewGameUser(100, "9527", 5000)
		u2, _ := fixtures.NewGameUser(200, "阿朱", 5000)

		game.AddUser(u1)
		game.AddUser(u2)

		game.Step = WaitBet
		game.Bet(u1.UserID, Goal, 500)

		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		exp := []BetType{LostInside, LostOutside}

		switch award {
		default:
			t.Errorf("Award want in %v, but %v", exp, award)
		case LostInside, LostOutside:
		}

	})

	t.Run("Random", func(t *testing.T) {
		game, _ := NewGame()

		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		exp := []BetType{GoalInside, GoalOutside, LostInside, LostOutside}

		switch award {
		default:
			t.Errorf("Award want in %v, but %v", exp, award)
		case GoalInside, GoalOutside, LostInside, LostOutside:
		}
	})
}

func BenchmarkGame_Award(b *testing.B) {

	b.Run("Random", func(b *testing.B) {

		game, _ := NewGame()

		dist := make(map[BetType]int)
		distr := make(map[BetType]float64)

		for i := 0; i < b.N; i++ {
			game.NewRound()
			award, _ := game.Award()
			dist[award]++
		}

		for award, n := range dist {
			x := float64(n) / float64(b.N)
			distr[award] = math.Round(x*100) / 100
		}

		b.Log(b.N, distr)

		if len(game.Records) > game.RecordMaxSize {
			b.Errorf("Records size want less then %v, but %v", game.RecordMaxSize, len(game.Records))
		}
	})
}
