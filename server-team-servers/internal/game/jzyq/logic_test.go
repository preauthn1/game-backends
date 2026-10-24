package jzyq

import (
	"context"
	"reflect"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelog"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

func TestLogic_Config(t *testing.T) {

	t.Run("Bet Area", func(t *testing.T) {
		ncfg := cfg.Clone()
		{
			conf := ncfg.GetConfig("gameserver")
			conf.Sections["bet"] = config.Section{
				"16": "2,5000",
				"17": "6, 1000",
				"18": "6 ,1000",

				"32": "2,5000",
				"33": "6, 1000",
				"34": "6 ,1000",
			}
		}

		grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
		logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

		conf := logic.Game.BetConfig
		t.Log(conf)

		if conf.Odd(Goal) != 2 {
			t.Errorf("Bet Area Goal Odd want %v, but %v", 2, conf.Odd(Goal))
		}
		if conf.MaxBet(Goal) != 5000 {
			t.Errorf("Bet Area Goal MaxBet want %v, but %v", 5000, conf.MaxBet(Goal))
		}
		if conf.Odd(GoalInside) != 6 {
			t.Errorf("Bet Area GoalInside Odd want %v, but %v", 6, conf.Odd(GoalInside))
		}
		if conf.MaxBet(GoalInside) != 1000 {
			t.Errorf("Bet Area GoalInside MaxBet want %v, but %v", 1000, conf.MaxBet(GoalInside))
		}
		if conf.Odd(GoalOutside) != 6 {
			t.Errorf("Bet Area GoalOutside Odd want %v, but %v", 6, conf.Odd(GoalOutside))
		}
		if conf.MaxBet(GoalOutside) != 1000 {
			t.Errorf("Bet Area GoalOutside MaxBet want %v, but %v", 1000, conf.MaxBet(GoalOutside))
		}

		if conf.Odd(Lost) != 2 {
			t.Errorf("Bet Area Lost Odd want %v, but %v", 2, conf.Odd(Lost))
		}
		if conf.MaxBet(Lost) != 5000 {
			t.Errorf("Bet Area Lost MaxBet want %v, but %v", 5000, conf.MaxBet(Lost))
		}
		if conf.Odd(LostInside) != 6 {
			t.Errorf("Bet Area LostInside Odd want %v, but %v", 6, conf.Odd(LostInside))
		}
		if conf.MaxBet(LostInside) != 1000 {
			t.Errorf("Bet Area LostInside MaxBet want %v, but %v", 1000, conf.MaxBet(LostInside))
		}
		if conf.Odd(LostOutside) != 6 {
			t.Errorf("Bet Area LostOutside Odd want %v, but %v", 6, conf.Odd(LostOutside))
		}
		if conf.MaxBet(LostOutside) != 1000 {
			t.Errorf("Bet Area LostOutside MaxBet want %v, but %v", 1000, conf.MaxBet(LostOutside))
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
		if logic.Game.BetConfig.Allow(200) {
			t.Errorf("Bet Amount %d want %v, but %v", 200, false, logic.Game.BetConfig.Allow(200))
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

func TestLogic_WatchGame(t *testing.T) {

	if testing.Short() {
		return
	}

	ncfg := cfg.Clone()
	{
		sec := ncfg.GetConfigSection("gameserver", "gameserver")
		sec["winline"] = "10000"
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	logic.SysPool.UpdateSysPool(-100)

	conf.CLogLevel = mlog.InfoLevel
	conf.TimerFastMode = 500

	ch := make(chan bool, 5)
	defer close(ch)

	logic.RegisterAfterGameEndFunc(func() { ch <- true })

	logic.GameStartFunc()

	<-ch
}

// 輸分流水
//
// 開：進球-線外
// 玩家A
// - 有效下注：[得分][1000] + [得分-線內][200] + [得分-線外][100] = 1300
// - 贏錢：[得分][1000][2倍] + [得分-線外][100][3.8倍] = 2380
// - 稅金：0
// - 流水：[得分-線內][200] = 200
// - 玩家更新：2280 - 0 - 1300 = 1080
// 玩家B
// - 有效下注：[得分][500] + [得分-線內][300] = 800
// - 贏錢：[進球][500][2倍] = 1000
// - 稅金：0
// - 流水：[得分-線內][300] = 300
// - 玩家更新：1000 - 0 - 800 = 200
func TestLogic_WatchGameWithoutMessage_LossOnlyFlowCalctor(t *testing.T) {

	ncfg := cfg.Clone()
	{
		// [gameinfo]
		// flowtype=1                  // 流水統計方式 0: 輸分流水 1: 有效投注流水 Default: 0

		conf := ncfg.GetConfig("gameserver")
		conf.Sections["gameinfo"] = config.Section{
			"flowtype": "0",
		}
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == 10003 {
			t.Log(userid, m, m.MsgData)
		}
	}

	// conf.CLogLevel = mlog.InfoLevel
	// conf.TimerFastMode = 1

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 5000)
	r2, p2 := fixtures.NewGameRobot(200, "白痴", 3000)
	r3, p3 := fixtures.NewGameRobot(300, "神經病", 3000)
	u4, p4 := fixtures.NewGameUser(500, "王八蛋", 5000)

	grs.PlayerManager.SetPlayer(p1)
	grs.PlayerManager.SetPlayer(p2)
	grs.PlayerManager.SetPlayer(p3)
	grs.PlayerManager.SetPlayer(p4)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)
	r2.UserInfo.UserState.OnUserQueue(r2.UserInfo, logic.TableManager.Room)
	r3.UserInfo.UserState.OnUserQueue(r3.UserInfo, logic.TableManager.Room)
	u4.UserState.OnUserQueue(u4, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(r2.UserInfo, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(r3.UserInfo, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(u4, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	logic.Game.Next = append(logic.Game.Next, GoalOutside)

	<-start

	ctxG := logic.GameLog.Context()

	logic.Game.Bet(u1.UserID, Goal, 500)
	logic.Game.Bet(u1.UserID, GoalInside, 100)
	logic.Game.Bet(u1.UserID, GoalInside, 100)
	logic.Game.Bet(u1.UserID, GoalInside, 100)

	logic.Game.Bet(r2.UserInfo.UserID, Lost, 100)
	logic.Game.Bet(r2.UserInfo.UserID, Lost, 100)
	logic.Game.Bet(r2.UserInfo.UserID, Lost, 100)
	logic.Game.Bet(r2.UserInfo.UserID, GoalOutside, 100)

	logic.Game.Bet(r3.UserInfo.UserID, Goal, 500)
	logic.Game.Bet(r3.UserInfo.UserID, LostInside, 100)

	logic.Game.Bet(u4.UserID, Goal, 1000)
	logic.Game.Bet(u4.UserID, GoalOutside, 100)
	logic.Game.Bet(u4.UserID, GoalInside, 100)
	logic.Game.Bet(u4.UserID, GoalInside, 100)

	done()

	<-stop

	{ // Assert u1
		var bet int64 = 800
		var win int64 = 1000
		var tax int64 = 0
		var flow int64 = 300
		var update int64 = win - tax - bet

		if u1.BetAmount != bet {
			t.Errorf("User Total BetAmount want %v, but %v", bet, u1.BetAmount)
		}
		if u1.FlowAmount != flow {
			t.Errorf("User Total FlowAmount want %v, but %v", flow, u1.FlowAmount)
		}

		ctxU := ctxG.Users[u1.UserID]
		if ctxU.BetScore != bet {
			t.Errorf("User PerGameLog BetScore want %v, but %v", bet, ctxU.BetScore)
		}
		if ctxU.PayoutScore != win {
			t.Errorf("User PerGameLog PayoutScore want %v, but %v", win, ctxU.PayoutScore)
		}
		if ctxU.TaxAmount != tax {
			t.Errorf("User PerGameLog TaxAmount want %v, but %v", tax, ctxU.TaxAmount)
		}
		if ctxU.ValidScore != flow {
			t.Errorf("User PerGameLog ValidScore want %v, but %v", flow, ctxU.ValidScore)
		}
		if ctxU.UpdateScore != update {
			t.Errorf("User PerGameLog UpdateScore want %v, but %v", update, ctxU.UpdateScore)
		}
	}

	{ // Assert u4
		var bet int64 = 1300
		var win int64 = 2000 + 380
		var tax int64 = 0
		var flow int64 = 200
		var update int64 = win - tax - bet

		if u4.BetAmount != bet {
			t.Errorf("User Total BetAmount want %v, but %v", bet, u4.BetAmount)
		}
		if u4.FlowAmount != flow {
			t.Errorf("User Total FlowAmount want %v, but %v", flow, u4.FlowAmount)
		}

		ctxU := ctxG.Users[u4.UserID]
		if ctxU.BetScore != bet {
			t.Errorf("User PerGameLog BetScore want %v, but %v", bet, ctxU.BetScore)
		}
		if ctxU.PayoutScore != win {
			t.Errorf("User PerGameLog PayoutScore want %v, but %v", win, ctxU.PayoutScore)
		}
		if ctxU.TaxAmount != tax {
			t.Errorf("User PerGameLog TaxAmount want %v, but %v", tax, ctxU.TaxAmount)
		}
		if ctxU.ValidScore != flow {
			t.Errorf("User PerGameLog ValidScore want %v, but %v", flow, ctxU.ValidScore)
		}
		if ctxU.UpdateScore != update {
			t.Errorf("User PerGameLog UpdateScore want %v, but %v", update, ctxU.UpdateScore)
		}
	}

}

// 有效投注流水
//
// 開：進球-線外
// 玩家A
// - 有效下注：[得分][1000] + [得分-線內][200] + [得分-線外][100] = 1300
// - 贏錢：[得分][1000][2倍] + [得分-線外][100][3.8倍] = 2380
// - 稅金：0
// - 流水：[得分][1000] + [得分-線內][200] + [得分-線外][100] = 1300
// - 玩家更新：2280 - 0 - 1300 = 1080
// 玩家B
// - 有效下注：[得分][500] + [得分-線內][300] = 800
// - 贏錢：[進球][500][2倍] = 1000
// - 稅金：0
// - 流水：[得分][500] + [得分-線內][300] = 800
// - 玩家更新：1000 - 0 - 800 = 200
func TestLogic_WatchGameWithoutMessage_EffectiveBetFlowCalctor(t *testing.T) {

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

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == 10003 {
			t.Log(userid, m, m.MsgData)
		}
	}

	// conf.CLogLevel = mlog.InfoLevel
	// conf.TimerFastMode = 1

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 5000)
	r2, p2 := fixtures.NewGameRobot(200, "白痴", 3000)
	r3, p3 := fixtures.NewGameRobot(300, "神經病", 3000)
	u4, p4 := fixtures.NewGameUser(500, "王八蛋", 5000)

	grs.PlayerManager.SetPlayer(p1)
	grs.PlayerManager.SetPlayer(p2)
	grs.PlayerManager.SetPlayer(p3)
	grs.PlayerManager.SetPlayer(p4)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)
	r2.UserInfo.UserState.OnUserQueue(r2.UserInfo, logic.TableManager.Room)
	r3.UserInfo.UserState.OnUserQueue(r3.UserInfo, logic.TableManager.Room)
	u4.UserState.OnUserQueue(u4, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(r2.UserInfo, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(r3.UserInfo, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(u4, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	logic.Game.Next = append(logic.Game.Next, GoalOutside)

	<-start

	ctxG := logic.GameLog.Context()

	logic.Game.Bet(u1.UserID, Goal, 500)
	logic.Game.Bet(u1.UserID, GoalInside, 100)
	logic.Game.Bet(u1.UserID, GoalInside, 100)
	logic.Game.Bet(u1.UserID, GoalInside, 100)

	logic.Game.Bet(r2.UserInfo.UserID, Lost, 100)
	logic.Game.Bet(r2.UserInfo.UserID, Lost, 100)
	logic.Game.Bet(r2.UserInfo.UserID, Lost, 100)
	logic.Game.Bet(r2.UserInfo.UserID, GoalOutside, 100)

	logic.Game.Bet(r3.UserInfo.UserID, Goal, 500)
	logic.Game.Bet(r3.UserInfo.UserID, LostInside, 100)

	logic.Game.Bet(u4.UserID, Goal, 1000)
	logic.Game.Bet(u4.UserID, GoalOutside, 100)
	logic.Game.Bet(u4.UserID, GoalInside, 100)
	logic.Game.Bet(u4.UserID, GoalInside, 100)

	done()

	<-stop

	{ // Assert u1
		var bet int64 = 800
		var win int64 = 1000
		var tax int64 = 0
		var flow int64 = 800
		var update int64 = win - tax - bet

		if u1.BetAmount != bet {
			t.Errorf("User Total BetAmount want %v, but %v", bet, u1.BetAmount)
		}
		if u1.FlowAmount != flow {
			t.Errorf("User Total FlowAmount want %v, but %v", flow, u1.FlowAmount)
		}

		ctxU := ctxG.Users[u1.UserID]
		if ctxU.BetScore != bet {
			t.Errorf("User PerGameLog BetScore want %v, but %v", bet, ctxU.BetScore)
		}
		if ctxU.PayoutScore != win {
			t.Errorf("User PerGameLog PayoutScore want %v, but %v", win, ctxU.PayoutScore)
		}
		if ctxU.TaxAmount != tax {
			t.Errorf("User PerGameLog TaxAmount want %v, but %v", tax, ctxU.TaxAmount)
		}
		if ctxU.ValidScore != flow {
			t.Errorf("User PerGameLog ValidScore want %v, but %v", flow, ctxU.ValidScore)
		}
		if ctxU.UpdateScore != update {
			t.Errorf("User PerGameLog UpdateScore want %v, but %v", update, ctxU.UpdateScore)
		}
	}

	{ // Assert u4
		var bet int64 = 1300
		var win int64 = 2000 + 380
		var tax int64 = 0
		var flow int64 = 1300
		var update int64 = win - tax - bet

		if u4.BetAmount != bet {
			t.Errorf("User Total BetAmount want %v, but %v", bet, u4.BetAmount)
		}
		if u4.FlowAmount != flow {
			t.Errorf("User Total FlowAmount want %v, but %v", flow, u4.FlowAmount)
		}

		ctxU := ctxG.Users[u4.UserID]
		if ctxU.BetScore != bet {
			t.Errorf("User PerGameLog BetScore want %v, but %v", bet, ctxU.BetScore)
		}
		if ctxU.PayoutScore != win {
			t.Errorf("User PerGameLog PayoutScore want %v, but %v", win, ctxU.PayoutScore)
		}
		if ctxU.TaxAmount != tax {
			t.Errorf("User PerGameLog TaxAmount want %v, but %v", tax, ctxU.TaxAmount)
		}
		if ctxU.ValidScore != flow {
			t.Errorf("User PerGameLog ValidScore want %v, but %v", flow, ctxU.ValidScore)
		}
		if ctxU.UpdateScore != update {
			t.Errorf("User PerGameLog UpdateScore want %v, but %v", update, ctxU.UpdateScore)
		}
	}

}

func TestLogic_RichestList(t *testing.T) {
	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 1000)
	u2, p2 := fixtures.NewGameUser(200, "白痴", 2000)
	u3, p3 := fixtures.NewGameUser(300, "神經病", 3000)
	u4, p4 := fixtures.NewGameUser(400, "林北謀錢", 4000)
	u5, p5 := fixtures.NewGameUser(500, "王八蛋", 5000)
	u6, p6 := fixtures.NewGameUser(600, "金郝洨", 6000)
	u7, p7 := fixtures.NewGameUser(700, "錢少少", 7000)
	u8, p8 := fixtures.NewGameUser(800, "老子超有錢", 8000)
	u9, p9 := fixtures.NewGameUser(900, "老子沒錢", 9000)

	grs.PlayerManager.SetPlayer(p1)
	grs.PlayerManager.SetPlayer(p2)
	grs.PlayerManager.SetPlayer(p3)
	grs.PlayerManager.SetPlayer(p4)
	grs.PlayerManager.SetPlayer(p5)
	grs.PlayerManager.SetPlayer(p6)
	grs.PlayerManager.SetPlayer(p7)
	grs.PlayerManager.SetPlayer(p8)
	grs.PlayerManager.SetPlayer(p9)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)
	u2.UserState.OnUserQueue(u2, logic.TableManager.Room)
	u3.UserState.OnUserQueue(u3, logic.TableManager.Room)
	u4.UserState.OnUserQueue(u4, logic.TableManager.Room)
	u5.UserState.OnUserQueue(u5, logic.TableManager.Room)
	u6.UserState.OnUserQueue(u6, logic.TableManager.Room)
	u7.UserState.OnUserQueue(u7, logic.TableManager.Room)
	u8.UserState.OnUserQueue(u8, logic.TableManager.Room)
	u9.UserState.OnUserQueue(u9, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), 1)
	logic.UserEnter(u2, logic.TableNo(), 2)
	logic.UserEnter(u3, logic.TableNo(), 3)
	logic.UserEnter(u4, logic.TableNo(), 4)
	logic.UserEnter(u5, logic.TableNo(), 5)
	logic.UserEnter(u6, logic.TableNo(), 6)
	logic.UserEnter(u7, logic.TableNo(), 7)
	logic.UserEnter(u8, logic.TableNo(), 8)
	logic.UserEnter(u9, logic.TableNo(), 9)

	<-start

	all := logic.AllTableUser()
	if len(all) != 9 {
		t.Errorf("Table user length want %v, but %v", 9, len(all))
	}

	richests := logic.richest(7)
	if len(richests) != 7 {
		t.Errorf("Richest list length want %v, but %v", 7, len(richests))
		return
	}

	if richests[0] != u9 {
		t.Errorf("Richest[0] want %v, but %v", u9.UserID, richests[0].UserID)
	}
	if richests[1] != u8 {
		t.Errorf("Richest[1] want %v, but %v", u8.UserID, richests[1].UserID)
	}
	if richests[2] != u7 {
		t.Errorf("Richest[2] want %v, but %v", u7.UserID, richests[2].UserID)
	}
	if richests[3] != u6 {
		t.Errorf("Richest[3] want %v, but %v", u6.UserID, richests[3].UserID)
	}
	if richests[4] != u5 {
		t.Errorf("Richest[4] want %v, but %v", u5.UserID, richests[4].UserID)
	}
	if richests[5] != u4 {
		t.Errorf("Richest[5] want %v, but %v", u4.UserID, richests[5].UserID)
	}
	if richests[6] != u3 {
		t.Errorf("Richest[6] want %v, but %v", u3.UserID, richests[6].UserID)
	}

	done()

	<-stop
}

func TestLogic_WatchGameWithMessage(t *testing.T) {

	ncfg := cfg.Clone()
	{
		sec := ncfg.GetConfigSection("gameserver", "robot")
		sec["enable"] = "1"
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
		t.Skip()
	}

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		u := logic.GetUserByUserID(userid)
		if u != nil && u.IsRobot() {
			grs.RobotService.SendGameMessage(u, m)
			return
		}
		if userid != 100 {
			return
		}
		// t.Log(m, m.MsgData)
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 5000)
	r2, p2 := fixtures.NewGameRobot(200, "白痴", 3000)
	r3, p3 := fixtures.NewGameRobot(300, "神經病", 3000)
	u4, p4 := fixtures.NewGameUser(500, "王八蛋", 5000)

	grs.RobotService.GetRobotManager().AddRobot(r2)
	grs.RobotService.GetRobotManager().AddRobot(r3)

	grs.PlayerManager.SetPlayer(p1)
	grs.PlayerManager.SetPlayer(p2)
	grs.PlayerManager.SetPlayer(p3)
	grs.PlayerManager.SetPlayer(p4)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)
	r2.UserInfo.UserState.OnUserQueue(r2.UserInfo, logic.TableManager.Room)
	r3.UserInfo.UserState.OnUserQueue(r3.UserInfo, logic.TableManager.Room)
	u4.UserState.OnUserQueue(u4, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(r2.UserInfo, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(r3.UserInfo, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)
	logic.UserEnter(u4, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	logic.Game.Next = append(logic.Game.Next, GoalInside)

	<-start

	uBet(logic, u1, Goal, 500)
	uBet(logic, u1, GoalInside, 100)
	uBet(logic, u1, GoalOutside, 100)

	done()

	<-stop

	ctx, done = context.WithCancel(context.Background())
	defer done()

	<-start

	done()
	<-stop
}

func TestLogic_WatchGameWithMessage_BetMinCarry(t *testing.T) {

	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	betRet := make(chan *netproto.JZYQ_BetResponse, 5)

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == int32(netproto.JZYQ_GameMessageClassID_JZYQBetRetID) {
			betRet <- m.MsgData.(*netproto.JZYQ_BetResponse)
		}
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 1000)

	grs.PlayerManager.SetPlayer(p1)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	<-start

	uBet(logic, u1, Goal, 500)

	done()

	<-stop

	{ // ret
		ret := <-betRet
		if ret.GetCode() != netproto.JZYQ_ResultCode_JZYQ_ResultCodeCarryMoneyNotEnough {
			t.Errorf("Bet Response Code want %v, but %v", netproto.JZYQ_ResultCode_JZYQ_ResultCodeCarryMoneyNotEnough, ret.GetCode())
		}
	}
}

func TestLogic_WatchGameWithMessage_NotBetStep(t *testing.T) {

	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	betRet := make(chan *netproto.JZYQ_BetResponse, 5)

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == int32(netproto.JZYQ_GameMessageClassID_JZYQBetRetID) {
			betRet <- m.MsgData.(*netproto.JZYQ_BetResponse)
		}
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 5000)

	grs.PlayerManager.SetPlayer(p1)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	uBet(logic, u1, Goal, 500)

	<-start

	done()

	<-stop

	{ // ret
		ret := <-betRet
		if ret.GetCode() != netproto.JZYQ_ResultCode_JZYQ_ResultCodeNotBetStep {
			t.Errorf("Bet Response Code want %v, but %v", netproto.JZYQ_ResultCode_JZYQ_ResultCodeNotBetStep, ret.GetCode())
		}
	}
}

func TestLogic_WatchGameWithMessage_NoMoney(t *testing.T) {

	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	betRet := make(chan *netproto.JZYQ_BetResponse, 5)

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == int32(netproto.JZYQ_GameMessageClassID_JZYQBetRetID) {
			betRet <- m.MsgData.(*netproto.JZYQ_BetResponse)
		}
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 5000)

	grs.PlayerManager.SetPlayer(p1)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	var skip int

	<-start

	uBet(logic, u1, Goal, 500)
	skip++
	uBet(logic, u1, Goal, 5000)

	done()

	<-stop

	{ // ret
		for i := 0; i < skip; i++ {
			<-betRet
		}
		ret := <-betRet
		if ret.GetCode() != netproto.JZYQ_ResultCode_JZYQ_ResultCodeMoneyNotEnough {
			t.Errorf("Bet Response Code want %v, but %v", netproto.JZYQ_ResultCode_JZYQ_ResultCodeMoneyNotEnough, ret.GetCode())
		}
	}
}

func TestLogic_WatchGameWithMessage_BetTypeOverMax(t *testing.T) {

	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	betRet := make(chan *netproto.JZYQ_BetResponse, 50)

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == int32(netproto.JZYQ_GameMessageClassID_JZYQBetRetID) {
			betRet <- m.MsgData.(*netproto.JZYQ_BetResponse)
		}
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 500000)

	grs.PlayerManager.SetPlayer(p1)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	var skip int

	<-start

	for i := 0; i < 20; i++ {
		uBet(logic, u1, Goal, 5000)
		skip++
	}

	uBet(logic, u1, Goal, 500)

	done()

	<-stop

	{ // ret
		for i := 0; i < skip; i++ {
			<-betRet
		}
		ret := <-betRet
		if ret.GetCode() != *netproto.JZYQ_ResultCode_JZYQ_ResultCodeBetTypeOverLimit.Enum() {
			t.Errorf("Bet Response Code want %v, but %v", netproto.JZYQ_ResultCode_JZYQ_ResultCodeBetTypeOverLimit, ret.GetCode())
		}
	}
}

func TestLogic_WatchGameWithMessage_IllegalBetAmount(t *testing.T) {

	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	betRet := make(chan *netproto.JZYQ_BetResponse, 50)

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == int32(netproto.JZYQ_GameMessageClassID_JZYQBetRetID) {
			betRet <- m.MsgData.(*netproto.JZYQ_BetResponse)
		}
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 500000)

	grs.PlayerManager.SetPlayer(p1)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	var skip int

	<-start

	uBet(logic, u1, Goal, 1)

	done()

	<-stop

	{ // ret
		for i := 0; i < skip; i++ {
			<-betRet
		}
		ret := <-betRet
		if ret.GetCode() != netproto.JZYQ_ResultCode_JZYQ_ResultCodeIllegalBetMoney {
			t.Errorf("Bet Response Code want %v, but %v", netproto.JZYQ_ResultCode_JZYQ_ResultCodeIllegalBetMoney, ret.GetCode())
		}
	}
}

func TestLogic_WatchGameWithMessage_AlreadyBetOtherSide(t *testing.T) {

	ncfg := cfg.Clone()

	grs := fixtures.NewTestGameRoomServerWithConfig(ncfg)
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	if testing.Short() {
		logic.SetGameLog(gamelog.NewEmptyGameLog())
	}

	betRet := make(chan *netproto.JZYQ_BetResponse, 50)

	logic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		if userid != 100 {
			return
		}
		if m.SClassID == int32(netproto.JZYQ_GameMessageClassID_JZYQBetRetID) {
			betRet <- m.MsgData.(*netproto.JZYQ_BetResponse)
		}
	}

	start := make(chan bool, 5)

	stop := make(chan bool, 5)

	ctx, done := context.WithCancel(context.Background())
	defer done()

	logic.RegistHooksAfter(WaitBet, func() { start <- true })
	logic.RegistHooksAfter(WaitBet, func() { <-ctx.Done() })
	logic.RegisterAfterGameEndFunc(func() { stop <- true })

	u1, p1 := fixtures.NewGameUser(100, "智障", 500000)

	grs.PlayerManager.SetPlayer(p1)

	u1.UserState.OnUserQueue(u1, logic.TableManager.Room)

	logic.UserEnter(u1, logic.TableNo(), logic.TableManager.GetFreeSeat().SeatNo)

	var skip int

	<-start

	uBet(logic, u1, Goal, 100)
	skip++
	uBet(logic, u1, Lost, 100)

	done()

	<-stop

	{ // ret
		for i := 0; i < skip; i++ {
			<-betRet
		}
		ret := <-betRet
		if ret.GetCode() != netproto.JZYQ_ResultCode_JZYQ_ResultCodeAlreadyBetOtherSide {
			t.Errorf("Bet Response Code want %v, but %v", netproto.JZYQ_ResultCode_JZYQ_ResultCodeAlreadyBetOtherSide, ret.GetCode())
		}
	}
}

func uBet(logic *Logic, u *gamelogic.GameUser, t BetType, n int64) {

	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.JZYQ_GameMessageClassID_JZYQBetID)

	data := new(netproto.JZYQ_BetRequest)
	data.Bet = new(netproto.JZYQ_Bet)
	data.Bet.Type = TransforBetType(t).Enum()
	data.Bet.Money = proto.Int64(n)

	m.MsgData, _ = proto.Marshal(data)

	logic.GameMessage(u, m)
}
