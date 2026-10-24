package roulette

import (
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
)

func TestGame_User(t *testing.T) {

	game, _ := NewGame()
	game.BetConfig.AddAllowAmounts(100)

	u1, _ := fixtures.NewGameUser(100, "小明", 5000000)
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

		t.Run("u1 Bet 100 in FirstRow", func(t *testing.T) {
			err := game.Bet(u1.UserID, Bet{Type: FirstRow, Count: 100})
			if err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}

			if game.BetTypeSum(FirstRow) != 100 {
				t.Errorf("Game Bet of FirstRow want %v, but %v", 100, game.BetTypeSum(FirstRow))
			}
		})

		t.Run("u1 Bet 1 , 2 in TwoNumber", func(t *testing.T) {
			err := game.Bet(u1.UserID, Bet{Type: TwoNumber, Count: 60000, Numbers: []uint32{1, 2}})
			if err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}
		})

		t.Run("u1 Bet 2 , 3 in TwoNumber", func(t *testing.T) {
			err := game.Bet(u1.UserID, Bet{Type: TwoNumber, Count: 60000, Numbers: []uint32{2, 3}})
			if err != nil {
				t.Errorf("Bet return error want %v, but %v", nil, err)
				return
			}
		})

		t.Run("u1 Bet 1 , 2 in TwoNumber want over", func(t *testing.T) {
			err := game.Bet(u1.UserID, Bet{Type: TwoNumber, Count: 60000, Numbers: []uint32{1, 2}})
			if err == nil {
				t.Errorf("Bet return error want %v, but %v", "BetTypeSum over", err)
				return
			}
		})
	})

	t.Run("Delete User", func(t *testing.T) {
		game.DeleteUser(u1.UserID)

		if game.Users[100] != nil {
			t.Errorf("User %d want delete from game", u1.UserID)
		}

		if game.BetTypeSum(FirstRow) != 0 {
			t.Errorf("Game Bet of FirstRow want %v, but %v", 0, game.BetTypeSum(FirstRow))
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
		game.AddUser(u3.UserInfo)

		game.Step = WaitBet
		game.Bet(u1.UserID, Bet{Type: FirstRow, Count: 1})
		game.Bet(u1.UserID, Bet{Type: Big, Count: 10})
		game.Bet(u2.UserID, Bet{Type: Red, Count: 100})
		game.Bet(u2.UserID, Bet{Type: Even, Count: 500})
		game.Bet(u3.UserInfo.UserID, Bet{Type: Black, Count: 1})

		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		// award should be 11 | 15 | 17
		if award != 11 && award != 15 && award != 17 {
			t.Errorf("Award want %v | %v | %v , but %v", 11, 15, 17, award)
		}
	})

}

func TestGame_Award_SuperZero(t *testing.T) {

	game, _ := NewGame()
	game.Next = append(game.Next, 0)
	game.BetConfig.AddAllowAmounts(1, 10, 100, 500)

	u1, _ := fixtures.NewGameUser(100, "9527", 5000)

	game.AddUser(u1)

	game.Step = WaitBet

	t.Run("Bet SuperZero", func(t *testing.T) {
		err := game.Bet(u1.UserID, Bet{Type: SuperZero, Count: 100})
		if err != nil {
			t.Errorf("Bet return error want %v, but %v", nil, err)
			return
		}
	})

	t.Run("Award SuperZero", func(t *testing.T) {
		award, err := game.Award()
		if err != nil {
			t.Errorf("Award return error want %v, but %v", nil, err)
			return
		}

		if award != 0 {
			t.Errorf("Award want %v, but %v", 0, award)
		}
	})
}
