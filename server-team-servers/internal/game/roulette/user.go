package roulette

import (
	"reflect"
	"sort"
	"sync"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

type User struct {
	sync.RWMutex

	UserID   int32
	GameUser *gamelogic.GameUser
	Bets     []*Bet // 下注統計
}

func NewUser(gu *gamelogic.GameUser) *User {
	return &User{
		UserID:   gu.UserID,
		GameUser: gu,
		Bets:     make([]*Bet, 0),
	}
}

func (u *User) BetSum() int64 {
	u.RLock()
	defer u.RUnlock()

	var sum int64

	for _, b := range u.Bets {
		sum += b.Count
	}

	return sum
}

// BetTypeSum 下注類型總和 同區域 同號碼
func (u *User) BetTypeSum(s Bet) int64 {
	u.RLock()
	defer u.RUnlock()

	var sum int64

	// sort b numbers
	sort.Slice(s.Numbers, func(i, j int) bool {
		return s.Numbers[i] < s.Numbers[j]
	})

	for _, b := range u.Bets {
		if b.Type != s.Type {
			continue
		}
		if !reflect.DeepEqual(b.Numbers, s.Numbers) {
			continue
		}
		sum += b.Count
	}

	return sum

}

func (u *User) WinSum() int64 {
	u.RLock()
	defer u.RUnlock()

	var sum int64

	for _, b := range u.Bets {
		sum += b.Win
	}

	return sum
}

func (u *User) RtnSum() int64 {
	u.RLock()
	defer u.RUnlock()

	var sum int64

	for _, b := range u.Bets {
		sum += b.Rtn
	}

	return sum
}

func (u *User) Bet(b Bet) error {

	if u.GameUser.Score < b.Count+u.BetSum() {
		return &ErrorMoneyNotEnough{}
	}

	u.Lock()
	defer u.Unlock()

	// sort b numbers
	sort.Slice(b.Numbers, func(i, j int) bool {
		return b.Numbers[i] < b.Numbers[j]
	})

	u.Bets = append(u.Bets, &b)

	return nil
}

func (u *User) Undo() error {
	u.Lock()
	defer u.Unlock()

	if len(u.Bets) == 0 {
		return &ErrorNoBet{}
	}

	u.Bets = u.Bets[:len(u.Bets)-1]
	return nil
}

func (u *User) Award(n uint32, conf BetConfig) (int64, int64) {

	var rtn, win int64

	u.Lock()
	defer u.Unlock()

	for _, b := range u.Bets {
		if b.Match(n) {
			r, w := b.Payout(conf, false)
			rtn += r
			win += w
		}
	}

	return rtn, win
}

func (u *User) Clear() {
	u.Lock()
	defer u.Unlock()

	u.Bets = make([]*Bet, 0)
}
