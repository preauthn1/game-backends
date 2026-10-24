package xmbx

import (
	"encoding/json"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestLightningGame_New(t *testing.T) {

	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	logic.LightningGame = NewLightningGame(logic, false)

	t.Log(logic.LightningGame)
}

func TestLightningGame_MarshalJson(t *testing.T) {

	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	t.Run("New Game", func(tt *testing.T) {
		logic.LightningGame = NewLightningGame(logic, false)

		if b, err := json.MarshalIndent(logic.LightningGame, "", "    "); err == nil {
			t.Log(string(b))
		} else {
			t.Error(err)
		}

		t.Log(logic.LightningGame.GetLastStep())
	})

	t.Run("Origin Step", func(tt *testing.T) {
		respin := &protodata.RespinTable{Id: proto.Uint32(601), GroupId: proto.Int32(601), RespinCount: proto.Uint32(3), RespinType: proto.String("Lightning"), RespinValue: proto.Uint32(603)}
		rule := &protodata.SlotPayTable{Id: proto.Int32(262), GroupId: proto.Int32(6), LinkType: proto.String("any"), Symbol: proto.String("g1"), PayType: proto.String("lightning"), Count: proto.Int32(6), Payout: proto.String("601.0"), UseType: proto.String("all")}

		manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
		// manager.GenerateGameResult(logic.DefaultSymbolGroupID, logic.SlotSymbolDistribution)
		manager.GameResult = slot.GameResult{
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("g1")},
			},
		}

		logic.LightningGame.AddStep(NewLightningStep(respin, rule, manager, int(respin.GetRespinCount())))

		if b, err := json.MarshalIndent(logic.LightningGame, "", "    "); err == nil {
			t.Log(string(b))
		} else {
			t.Error(err)
		}

		t.Log(logic.LightningGame.GetLastStep())
	})

	t.Run("Create Step", func(tt *testing.T) {

		logic.LightningGame.Spin()

		if b, err := json.MarshalIndent(logic.LightningGame, "", "    "); err == nil {
			t.Log(string(b))
		} else {
			t.Error(err)
		}

		t.Log(logic.LightningGame.GetLastStep())
	})
}
