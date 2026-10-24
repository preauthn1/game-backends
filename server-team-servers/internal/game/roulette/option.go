package roulette

import (
	"fmt"
)

type GameOpt interface {
	Apply(*Game) error
}

type BetConfigOpt struct {
	Conf *BetConfig
}

func (opt *BetConfigOpt) Apply(game *Game) error {
	if opt.Conf == nil {
		return fmt.Errorf("BetConfigOpt.Conf can't be nil")
	}
	game.BetConfig = opt.Conf
	return nil
}

type BetAmountOpt struct {
	Amounts []int64
}

func (opt *BetAmountOpt) Apply(game *Game) error {
	if len(opt.Amounts) == 0 {
		return fmt.Errorf("BetAmountOpt.Amounts can't be empty")
	}
	game.BetConfig.AddAllowAmounts(opt.Amounts...)
	return nil
}

type BetMinCarryMoneyOpt struct {
	Amount int64
}

func (opt *BetMinCarryMoneyOpt) Apply(game *Game) error {
	game.BetMinCarray = opt.Amount
	return nil
}

type UserMaxBetOpt struct {
	Amount int64
}

func (opt *UserMaxBetOpt) Apply(game *Game) error {
	game.UserMaxBet = opt.Amount
	return nil
}

type RecordMaxSizeOpt struct {
	Size int
}

func (opt *RecordMaxSizeOpt) Apply(game *Game) error {
	game.RecordMaxSize = opt.Size
	return nil
}

// WaterLineCheckerOpt 低於水線回傳 true
type WaterLineCheckerOpt struct {
	// Func 低於水線回傳 true
	Func func() bool
}

func (opt *WaterLineCheckerOpt) Apply(game *Game) error {
	if opt.Func == nil {
		return fmt.Errorf("WaterLineCheckerOpt.Func can't be nil")
	}
	game.WaterLineChecker = opt.Func
	return nil
}
