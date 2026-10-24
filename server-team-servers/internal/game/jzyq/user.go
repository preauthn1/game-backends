package jzyq

import (
	"sync"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

type User struct {
	sync.RWMutex

	UserID     int32
	GameUser   *gamelogic.GameUser
	BetSummary map[BetType]int64 // 下注統計
	WinSummary map[BetType]int64 // 贏錢統計
}

func NewUser(gu *gamelogic.GameUser) *User {
	u := &User{
		UserID:     gu.UserID,
		GameUser:   gu,
		BetSummary: make(map[BetType]int64),
		WinSummary: make(map[BetType]int64),
	}
	return u
}

func (u *User) BetSum() int64 {
	u.RLock()
	defer u.RUnlock()

	var sum int64

	for _, n := range u.BetSummary {
		sum += n
	}

	return sum
}

func (u *User) WinSum() int64 {
	u.RLock()
	defer u.RUnlock()

	var sum int64

	for _, n := range u.WinSummary {
		sum += n
	}

	return sum
}

func (u *User) Bet(t BetType, n int64, enablehedge bool) error {

	if u.GameUser.Score < n+u.BetSum() {
		return &ErrorMoneyNotEnough{}
	}

	u.Lock()
	defer u.Unlock()

	if !enablehedge {
		if t == Goal && u.BetSummary[Lost] > 0 {
			return &ErrorAlreadyBetOtherSide{Side: Lost, Reject: t}
		}
		if t == Lost && u.BetSummary[Goal] > 0 {
			return &ErrorAlreadyBetOtherSide{Side: Goal, Reject: t}
		}
	}

	u.BetSummary[t] += n

	return nil
}

func (u *User) Award(award BetType, conf BetConfig) int64 {

	var win int64

	u.Lock()
	defer u.Unlock()

	for t, n := range u.BetSummary {
		if t.Match(award) {
			w := int64(float64(n) * conf.Odd(t))
			win += w
			u.WinSummary[t] = w
		}
	}

	return win
}

func (u *User) Clear() {
	u.Lock()
	defer u.Unlock()

	u.BetSummary = make(map[BetType]int64)
	u.WinSummary = make(map[BetType]int64)
}
