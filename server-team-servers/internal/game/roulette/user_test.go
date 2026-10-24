package roulette

import (
	"reflect"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
)

func TestUser_BetAward(t *testing.T) {
	gu, _ := fixtures.NewGameUser(100, "9527", 1000)

	u := NewUser(gu)

	// 押大 400 成功
	if err := u.Bet(Bet{Type: Big, Count: 400}); err != nil {
		t.Errorf("Bet return error want %v, but %v", nil, err)
		return
	}

	if u.BetSum() != 400 {
		t.Errorf("BetSum want %v, but %v", 400, u.BetSum())
	}

	// 押红 200 成功
	if err := u.Bet(Bet{Type: Red, Count: 200}); err != nil {
		t.Errorf("Bet return error want %v, but %v", nil, err)
		return
	}

	if u.BetSum() != 600 {
		t.Errorf("BetSum want %v, but %v", 600, u.BetSum())
	}

	// 押红 200 失敗 錢不夠
	if err := u.Bet(Bet{Type: Red, Count: 200}); reflect.TypeOf(err) == reflect.TypeOf(ErrorMoneyNotEnough{}) {
		t.Errorf("Bet return error want %v, but %v", nil, err)
		return
	}

	// 開 31

	conf := NewBetConfig(DefaultBetTypes...)

	rtn, win := u.Award(31, *conf)
	if rtn != 400 {
		t.Errorf("Award return %v, want %v", rtn, 400)
	}
	if win != 400 {
		t.Errorf("Award return %v, want %v", win, 400)
	}
}

func TestUser_Undo(t *testing.T) {

	gu, _ := fixtures.NewGameUser(100, "9527", 1000)

	u := NewUser(gu)

	// 押大 400 成功
	if err := u.Bet(Bet{Type: Big, Count: 400}); err != nil {
		t.Errorf("Bet return error want %v, but %v", nil, err)
		return
	}

	// 押红 200 成功
	if err := u.Bet(Bet{Type: Red, Count: 200}); err != nil {
		t.Errorf("Bet return error want %v, but %v", nil, err)
		return
	}

	t.Log(u.Bets)
	t.Log(u.BetSum())

	if u.BetSum() != 600 {
		t.Errorf("BetSum want %v, but %v", 600, u.BetSum())
	}

	// 撤銷
	if err := u.Undo(); err != nil {
		t.Errorf("Undo return error want %v, but %v", nil, err)
		return
	}

	t.Log(u.Bets)
	t.Log(u.BetSum())

	if u.BetSum() != 400 {
		t.Errorf("BetSum want %v, but %v", 400, u.BetSum())
	}

}

func TestUser_BetSum(t *testing.T) {

	gu, _ := fixtures.NewGameUser(100, "9527", 1000)

	u := NewUser(gu)

	u.Bet(Bet{Type: OneNumber, Count: 100, Numbers: []uint32{1}})
	u.Bet(Bet{Type: OneNumber, Count: 200, Numbers: []uint32{2}})
	u.Bet(Bet{Type: TwoNumber, Count: 300, Numbers: []uint32{1, 2}})

	if u.BetSum() != 600 {
		t.Errorf("BetSum want %v, but %v", 600, u.BetSum())
	}

	if u.BetTypeSum(Bet{Type: OneNumber, Count: 100, Numbers: []uint32{1}}) != 100 {
		t.Errorf("BetTypeSum want %v, but %v", 100, u.BetTypeSum(Bet{Type: OneNumber, Count: 100, Numbers: []uint32{1}}))
	}

	if u.BetTypeSum(Bet{Type: OneNumber, Count: 200, Numbers: []uint32{2}}) != 200 {
		t.Errorf("BetTypeSum want %v, but %v", 200, u.BetTypeSum(Bet{Type: OneNumber, Count: 200, Numbers: []uint32{2}}))
	}

	if u.BetTypeSum(Bet{Type: TwoNumber, Count: 300, Numbers: []uint32{1, 2}}) != 300 {
		t.Errorf("BetTypeSum want %v, but %v", 300, u.BetTypeSum(Bet{Type: TwoNumber, Count: 300, Numbers: []uint32{1, 2}}))
	}

	if u.BetTypeSum(Bet{Type: TwoNumber, Count: 300, Numbers: []uint32{2, 1}}) != 300 {
		t.Errorf("BetTypeSum want %v, but %v", 300, u.BetTypeSum(Bet{Type: TwoNumber, Count: 300, Numbers: []uint32{2, 1}}))
	}

}
