package jzyq

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
	Type  BetType
	Limit int64
	Now   int64
}

func (e ErrorBetTypeOverLimit) Error() string {
	return fmt.Sprintf("%v will over to limit %d, now %d", e.Type, e.Limit, e.Now)
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
