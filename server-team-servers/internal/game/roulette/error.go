package roulette

import "fmt"

type ErrorStepNotAllow struct {
	Action   string
	Now      GameStep
	Excepted GameStep
}

func (e ErrorStepNotAllow) Error() string {
	return fmt.Sprintf("action not allow, %s want in step %v, now %v", e.Action, e.Excepted, e.Now)
}

type ErrorMoneyNotEnough struct{}

func (e ErrorMoneyNotEnough) Error() string {
	return "money not enough"
}

type ErrorCarryMoneyNotEnough struct {
	Want int64
	But  int64
}

func (e ErrorCarryMoneyNotEnough) Error() string {
	return fmt.Sprintf("user carry money not enough, want more then %v, but %v", e.Want, e.But)
}

type ErrorUserNotExist struct {
	UserID int32
}

func (e ErrorUserNotExist) Error() string {
	return fmt.Sprintf("user %d not exist", e.UserID)
}

type ErrorBetTypeOverLimit struct {
	Type    BetType
	Numbers []uint32
	Limit   int64
	Now     int64
}

func (e ErrorBetTypeOverLimit) Error() string {
	return fmt.Sprintf("%v%v will over to limit %d, now %d", e.Type, e.Numbers, e.Limit, e.Now)
}

type ErrorBetTypeOverPersionalLimit struct {
	Type  BetType
	Limit int64
	Now   int64
}

func (e ErrorBetTypeOverPersionalLimit) Error() string {
	return fmt.Sprintf("%v will over to persional limit %d, now %d", e.Type, e.Limit, e.Now)
}

type ErrorAlreadyBetOtherSide struct {
	Side   BetType
	Reject BetType
}

func (e ErrorAlreadyBetOtherSide) Error() string {
	return fmt.Sprintf("already bet %v, could not bet %v", e.Side, e.Reject)
}

type ErrorIllegalBetAmount struct {
	Amount int64
}

func (e ErrorIllegalBetAmount) Error() string {
	return fmt.Sprintf("illegal bet amount %v", e.Amount)
}

type ErrorIllegalBetType struct {
	Type BetType
}

func (e ErrorIllegalBetType) Error() string {
	return fmt.Sprintf("illegal bet type %v", e.Type)
}

type ErrorIllegalBetNumber struct {
	Number uint32
}

func (e ErrorIllegalBetNumber) Error() string {
	return fmt.Sprintf("illegal bet number %v", e.Number)
}

type ErrorIllegalBetNumberCount struct {
	Want  int
	Count int
}

func (e ErrorIllegalBetNumberCount) Error() string {
	return fmt.Sprintf("illegal bet number count, want %d, but %d", e.Want, e.Count)
}

type ErrorNoBet struct{}

func (e ErrorNoBet) Error() string {
	return "no bet"
}
