package roulette

import (
	"reflect"
	"testing"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
)

func TestLogic_Config(t *testing.T) {

	t.Run("Bet Area", func(t *testing.T) {
		ncfg := cfg.Clone()
		{
			conf := ncfg.GetConfig("gameserver")
			conf.Sections["bet"] = config.Section{
				"1":  "1.5,10000",
				"2":  "1.5,10000",
				"3":  "1.5,10000",
				"4":  "1.5,10000",
				"5":  "1.5,10000",
				"6":  "1.5,10000",
				"7":  "2.5,5000",
				"8":  "2.5,5000",
				"9":  "2.5,5000",
				"10": "2.5,5000",
				"11": "2.5,5000",
				"12": "2.5,5000",
				"13": "37,5000",
				"14": "18,5000",
				"15": "12,5000",
				"16": "9,5000",
				"17": "6,5000",
				"18": "47,5000",
			}
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		conf := logic.Game.BetConfig
		t.Log(conf)

		if conf.Odd(Odd) != 1.5 {
			t.Errorf("Bet Area Odd Odd want %v, but %v", 1.5, conf.Odd(Odd))
		}
		if conf.MaxBet(Odd) != 10000 {
			t.Errorf("Bet Area Odd MaxBet want %v, but %v", 10000, conf.MaxBet(Odd))
		}

		if conf.Odd(Even) != 1.5 {
			t.Errorf("Bet Area Even Odd want %v, but %v", 1.5, conf.Odd(Even))
		}
		if conf.MaxBet(Even) != 10000 {
			t.Errorf("Bet Area Even MaxBet want %v, but %v", 10000, conf.MaxBet(Even))
		}

		if conf.Odd(Red) != 1.5 {
			t.Errorf("Bet Area Red Odd want %v, but %v", 1.5, conf.Odd(Red))
		}
		if conf.MaxBet(Red) != 10000 {
			t.Errorf("Bet Area Red MaxBet want %v, but %v", 10000, conf.MaxBet(Red))
		}

		if conf.Odd(Black) != 1.5 {
			t.Errorf("Bet Area Black Odd want %v, but %v", 1.5, conf.Odd(Black))
		}
		if conf.MaxBet(Black) != 10000 {
			t.Errorf("Bet Area Black MaxBet want %v, but %v", 10000, conf.MaxBet(Black))
		}

		if conf.Odd(First12) != 2.5 {
			t.Errorf("Bet Area First12 Odd want %v, but %v", 2.5, conf.Odd(First12))
		}
		if conf.MaxBet(First12) != 5000 {
			t.Errorf("Bet Area First12 MaxBet want %v, but %v", 5000, conf.MaxBet(First12))
		}

		if conf.Odd(Second12) != 2.5 {
			t.Errorf("Bet Area Second12 Odd want %v, but %v", 2.5, conf.Odd(Second12))
		}
		if conf.MaxBet(Second12) != 5000 {
			t.Errorf("Bet Area Second12 MaxBet want %v, but %v", 5000, conf.MaxBet(Second12))
		}

		if conf.Odd(Third12) != 2.5 {
			t.Errorf("Bet Area Third12 Odd want %v, but %v", 2.5, conf.Odd(Third12))
		}
		if conf.MaxBet(Third12) != 5000 {
			t.Errorf("Bet Area Third12 MaxBet want %v, but %v", 5000, conf.MaxBet(Third12))
		}

		if conf.Odd(FirstRow) != 2.5 {
			t.Errorf("Bet Area FirstRow Odd want %v, but %v", 2.5, conf.Odd(FirstRow))
		}
		if conf.MaxBet(FirstRow) != 5000 {
			t.Errorf("Bet Area FirstRow MaxBet want %v, but %v", 5000, conf.MaxBet(FirstRow))
		}

		if conf.Odd(SecondRow) != 2.5 {
			t.Errorf("Bet Area SecondRow Odd want %v, but %v", 2.5, conf.Odd(SecondRow))
		}
		if conf.MaxBet(SecondRow) != 5000 {
			t.Errorf("Bet Area SecondRow MaxBet want %v, but %v", 5000, conf.MaxBet(SecondRow))
		}

		if conf.Odd(ThirdRow) != 2.5 {
			t.Errorf("Bet Area ThirdRow Odd want %v, but %v", 2.5, conf.Odd(ThirdRow))
		}
		if conf.MaxBet(ThirdRow) != 5000 {
			t.Errorf("Bet Area ThirdRow MaxBet want %v, but %v", 5000, conf.MaxBet(ThirdRow))
		}

		if conf.Odd(OneNumber) != 37 {
			t.Errorf("Bet Area OneNumber Odd want %v, but %v", 37, conf.Odd(OneNumber))
		}
		if conf.MaxBet(OneNumber) != 5000 {
			t.Errorf("Bet Area OneNumber MaxBet want %v, but %v", 5000, conf.MaxBet(OneNumber))
		}

		if conf.Odd(TwoNumber) != 18 {
			t.Errorf("Bet Area TwoNumber Odd want %v, but %v", 18, conf.Odd(TwoNumber))
		}
		if conf.MaxBet(TwoNumber) != 5000 {
			t.Errorf("Bet Area TwoNumber MaxBet want %v, but %v", 5000, conf.MaxBet(TwoNumber))
		}

		if conf.Odd(ThreeNumber) != 12 {
			t.Errorf("Bet Area ThreeNumber Odd want %v, but %v", 12, conf.Odd(ThreeNumber))
		}
		if conf.MaxBet(ThreeNumber) != 5000 {
			t.Errorf("Bet Area ThreeNumber MaxBet want %v, but %v", 5000, conf.MaxBet(ThreeNumber))
		}

		if conf.Odd(FourNumber) != 9 {
			t.Errorf("Bet Area FourNumber Odd want %v, but %v", 9, conf.Odd(FourNumber))
		}
		if conf.MaxBet(FourNumber) != 5000 {
			t.Errorf("Bet Area FourNumber MaxBet want %v, but %v", 5000, conf.MaxBet(FourNumber))
		}

		if conf.Odd(SixNumber) != 6 {
			t.Errorf("Bet Area SixNumber Odd want %v, but %v", 6, conf.Odd(SixNumber))
		}
		if conf.MaxBet(SixNumber) != 5000 {
			t.Errorf("Bet Area SixNumber MaxBet want %v, but %v", 5000, conf.MaxBet(SixNumber))
		}

		if conf.Odd(SuperZero) != 47 {
			t.Errorf("Bet Area SuperZero Odd want %v, but %v", 47, conf.Odd(SuperZero))
		}
		if conf.MaxBet(SuperZero) != 5000 {
			t.Errorf("Bet Area SuperZero MaxBet want %v, but %v", 5000, conf.MaxBet(SuperZero))
		}
	})

	t.Run("Bet Count", func(t *testing.T) {

		ncfg := cfg.Clone()
		{
			sec := ncfg.GetConfigSection("gameserver", "gameserver")
			sec["betcount"] = "100, 300, 400, 500, 600"
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		if !logic.Game.BetConfig.Allow(100) {
			t.Errorf("Bet Amount %d want %v, but %v", 100, true, logic.Game.BetConfig.Allow(100))
		}
		if !logic.Game.BetConfig.Allow(300) {
			t.Errorf("Bet Amount %d want %v, but %v", 300, true, logic.Game.BetConfig.Allow(300))
		}
		if !logic.Game.BetConfig.Allow(400) {
			t.Errorf("Bet Amount %d want %v, but %v", 400, true, logic.Game.BetConfig.Allow(400))
		}
		if !logic.Game.BetConfig.Allow(500) {
			t.Errorf("Bet Amount %d want %v, but %v", 500, true, logic.Game.BetConfig.Allow(500))
		}
		if !logic.Game.BetConfig.Allow(600) {
			t.Errorf("Bet Amount %d want %v, but %v", 600, true, logic.Game.BetConfig.Allow(600))
		}
		if logic.Game.BetConfig.Allow(50) {
			t.Errorf("Bet Amount %d want %v, but %v", 50, false, logic.Game.BetConfig.Allow(50))
		}
	})

	t.Run("Bet Carry Money", func(t *testing.T) {

		ncfg := cfg.Clone()
		{
			sec := ncfg.GetConfigSection("gameserver", "gameinfo")
			sec["betmincarry"] = "5000"
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		if logic.Game.BetMinCarray != 5000 {
			t.Errorf("Bet Min Carray want %v, but %v", 5000, logic.Game.BetMinCarray)
		}
	})

	t.Run("Record Size", func(t *testing.T) {
		ncfg := cfg.Clone()
		{
			conf := ncfg.GetConfig("gameserver")
			conf.Sections["record"] = config.Section{
				"maxsize": "789",
			}
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		if logic.Game.RecordMaxSize != 789 {
			t.Errorf("Record Max Size want %v, but %v", 789, logic.Game.RecordMaxSize)
		}
	})

	t.Run("Animate Time", func(t *testing.T) {
		ncfg := cfg.Clone()
		{
			// [animate]
			// start=3s            // 遊戲開始動畫時間
			// bet=15s             // 下注時間
			// award=5s            // 表演開獎時間
			// finish=2s           // 遊戲結束時間

			conf := ncfg.GetConfig("gameserver")
			conf.Sections["animate"] = config.Section{
				"start":  "5s",
				"bet":    "6s",
				"award":  "7s",
				"finish": "8s",
			}
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		if logic.GameStepTimeout[Start] != 5*time.Second {
			t.Errorf("GameStepTimeout[Start] want %v, but %v", 5*time.Second, logic.GameStepTimeout[Start])
		}
		if logic.GameStepTimeout[WaitBet] != 6*time.Second {
			t.Errorf("GameStepTimeout[WaitBet] want %v, but %v", 6*time.Second, logic.GameStepTimeout[WaitBet])
		}
		if logic.GameStepTimeout[ShowAward] != 7*time.Second {
			t.Errorf("GameStepTimeout[ShowAward] want %v, but %v", 7*time.Second, logic.GameStepTimeout[ShowAward])
		}
		if logic.GameStepTimeout[Finish] != 8*time.Second {
			t.Errorf("GameStepTimeout[Finish] want %v, but %v", 8*time.Second, logic.GameStepTimeout[Finish])
		}
	})

	t.Run("FlowCalc LossOnly", func(t *testing.T) {
		ncfg := cfg.Clone()
		{
			// [gameinfo]
			// flowtype=0                  // 流水統計方式 0: 輸分流水 1: 有效投注流水 Default: 0

			conf := ncfg.GetConfig("gameserver")
			conf.Sections["gameinfo"] = config.Section{
				"flowtype": "0",
			}
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		if reflect.TypeOf(logic.FlowCalctor) != reflect.TypeOf(LossOnlyFlowCalctor{}) {
			t.Errorf("FlowCalctor want %v, but %v", reflect.TypeOf(LossOnlyFlowCalctor{}), reflect.TypeOf(logic.FlowCalctor))
		}
	})

	t.Run("FlowCalc EffectiveBet", func(t *testing.T) {
		ncfg := cfg.Clone()
		{
			// [gameinfo]
			// flowtype=1                  // 流水統計方式 0: 輸分流水 1: 有效投注流水 Default: 0

			conf := ncfg.GetConfig("gameserver")
			conf.Sections["gameinfo"] = config.Section{
				"flowtype": "1",
			}
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		if reflect.TypeOf(logic.FlowCalctor) != reflect.TypeOf(EffectiveBetFlowCalctor{}) {
			t.Errorf("FlowCalctor want %v, but %v", reflect.TypeOf(EffectiveBetFlowCalctor{}), reflect.TypeOf(logic.FlowCalctor))
		}
	})
}
