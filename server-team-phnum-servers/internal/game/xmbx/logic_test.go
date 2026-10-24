package xmbx

import (
	"strconv"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestProto(t *testing.T) {
	protodatas.Print()
}

func TestNewLogic(t *testing.T) {

	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	t.Log("SlotPatternID", logic.SlotPatternID)
	t.Log("BetCounts", logic.BetCounts)
	t.Log("BetRate", logic.BetRates)
	t.Log("MaxBetCount", logic.MaxBetCount)
	t.Log("MaxBetRate", logic.MaxBetRate)
	t.Log("BigWinRule", logic.BigWinRule)
	t.Log("BigWinNotify", logic.BigWinNotify)
	t.Log("EndActionTime", logic.EndActionTime)
	t.Log("MinCarryMoney", logic.MinCarryMoney)
	t.Log("SymbolsDesc", logic.SymbolsDesc)
	t.Log("PayLineDesc", logic.PayLineDesc)
	t.Log("UnitPrefix", logic.UnitPrefix)
	t.Log("Descriptions", logic.Descriptions)

	t.Log("DefaultReelNum", logic.DefaultReelNum)
	t.Log("DefaultRowNum", logic.DefaultRowNum)
	t.Log("DefaultSymbolGroupID", logic.DefaultSymbolGroupID)

	t.Log("GameBetRate", logic.GameBetRate)

	t.Log("PayLineMode", logic.PayLineMode)
	// t.Log("PaylineMatcher", logic.PaylineMatcher)

	t.Log("Jackpots", logic.Jackpots)

	t.Log("=== SlotPattern ===")
	t.Log(logic.SlotPattern)

	t.Log("=== MainGameTable ===", len(logic.MainGameTable))
	for _, row := range logic.MainGameTable {
		t.Log(row)
	}

	t.Log("=== SlotPayLine ===", len(logic.SlotPayLine))
	// for _, row := range logic.SlotPayLine {
	// 	t.Log(row)
	// }

	t.Log("=== SlotPayTable ===", len(logic.SlotPayTable))
	for _, row := range logic.SlotPayTable {
		t.Log(row)
	}

	t.Log("=== SymbolAttributes ===", len(logic.SymbolAttributes))
	for _, row := range logic.SymbolAttributes {
		t.Log(row)
	}

	t.Log("=== RespinTable ===", len(logic.RespinTable))
	for _, row := range logic.RespinTable {
		t.Log(row)
	}

	t.Log("=== SlotSymbolDistribution ===", len(logic.SlotSymbolDistribution))
	for _, row := range logic.SlotSymbolDistribution {
		if row.GetGroupId() != 603 {
			continue
		}
		t.Log(row)
	}

	t.Log("=== JackpotTable ===", len(logic.JackpotTable))
	for _, row := range logic.JackpotTable {
		t.Log(row)
	}

	t.Log("=== PickGameTable ===", len(logic.PickGameTable))
	// for _, row := range logic.PickGameTable {
	// 	t.Log(row)
	// }

	t.Log("=== BonusTable ===", len(logic.BonusTable))
	// for _, row := range logic.BonusTable {
	// 	t.Log(row)
	// }

	t.Log("=== FreeSpinTable ===", len(logic.FreeSpinTable))
	for _, row := range logic.FreeSpinTable {
		t.Log(row)
	}
}

func TestLogic_UserEnter(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		slot_test.PrintSendMessgeFunc(t, userID, m)

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)
}

func TestLogic_RandomSpin(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Random", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_LightningSpin(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win Lightning", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	{
		if logic.LightningGame == nil {
			t.Errorf("Should Win Lightning Game")
			return
		}

		if len(logic.LightningGame.Steps) != 5 {
			t.Errorf("LightningGame Steps length want %d, but %d", 5, len(logic.LightningGame.Steps))
			return
		}

		if logic.LightningGame.ShowStep != 0 {
			t.Errorf("LightningGame ShowStep want %d, but %d", 0, logic.LightningGame.ShowStep)
		}

		if logic.LightningGame.IsFreeGame {
			t.Errorf("LightningGame IsFreeGame want %t, but %t", false, logic.LightningGame.IsFreeGame)
		}

		if logic.LightningGame.Steps[0].LastChance != 2 {
			t.Errorf("LightningGame Steps 0: LastChance want %d, but %d", 2, logic.LightningGame.Steps[0].LastChance)
		}
		if logic.LightningGame.Steps[1].LastChance != 2 {
			t.Errorf("LightningGame Steps 1: LastChance want %d, but %d", 2, logic.LightningGame.Steps[1].LastChance)
		}
		if logic.LightningGame.Steps[2].LastChance != 1 {
			t.Errorf("LightningGame Steps 2: LastChance want %d, but %d", 1, logic.LightningGame.Steps[2].LastChance)
		}
		if logic.LightningGame.Steps[3].LastChance != 0 {
			t.Errorf("LightningGame Steps 3: LastChance want %d, but %d", 0, logic.LightningGame.Steps[3].LastChance)
		}
		if logic.LightningGame.Steps[4].LastChance != 0 {
			t.Errorf("LightningGame Steps 4: LastChance want %d, but %d", 0, logic.LightningGame.Steps[4].LastChance)
		}
	}

	t.Run("User Show Step 1 Win Again", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(1)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame.ShowStep != 1 {
		t.Errorf("LightningGame ShowStep want %d, but %d", 1, logic.LightningGame.ShowStep)
	}

	t.Run("User Show Step 2", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(2)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame.ShowStep != 2 {
		t.Errorf("LightningGame ShowStep want %d, but %d", 2, logic.LightningGame.ShowStep)
	}

	t.Run("User Show Step 3", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(3)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame.ShowStep != 3 {
		t.Errorf("LightningGame ShowStep want %d, but %d", 3, logic.LightningGame.ShowStep)
	}

	t.Run("User Show Step 4", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(4)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame != nil {
		t.Errorf("LightningGame should be cleaned")
	}
}

func TestLogic_LightningJackpotSpin(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win Lightning", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)
				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p17"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g17"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g18"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 1", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)
				printSendRPCMessgeFunc(tt, safe, m, nil)
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(1)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_FreeGameSpin(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win FreeGame", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin == nil {
		t.Errorf("Logic should win free game")
	}

	if logic.FreeSpin.Count != 10 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 10, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 1", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 9 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 9, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 2", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 8 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 8, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 3", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 7 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 7, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 4", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 6 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 6, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 5", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 5 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 5, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 6", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 4 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 4, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 7", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 3 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 3, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 8", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 2 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 2, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 9", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 1 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 1, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 10", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin != nil {
		t.Errorf("Logic should clean free game")
	}
}

func TestLogic_LightningFreeGameSpin(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win FreeGame", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin == nil {
		t.Errorf("Logic should win free game")
	}

	if logic.FreeSpin.Count != 10 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 10, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 1", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 9 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 9, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 2", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 8 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 8, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 3", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 7 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 7, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 4, Win Lightning", func(tt *testing.T) {
		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame == nil {
		t.Errorf("Game should win LightningGame")
	}

	t.Run("User Show Step 1", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(1)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 2", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(2)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 3", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(3)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 4", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(4)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame != nil {
		t.Errorf("Game should clean LightningGame")
	}

	if logic.FreeSpin.Count != 6 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 6, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 5", func(tt *testing.T) {
		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 5 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 5, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 6", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 4 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 4, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 7", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 3 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 3, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 8", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 2 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 2, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 9", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 1 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 1, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 10", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin != nil {
		t.Errorf("Logic should clean free game")
	}
}

func TestLogic_Win15GoaldBall(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win 15 Ball", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_Spin_WinLightningFreeGame(t *testing.T) {
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win FreeGame & Lightning", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(601), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(603), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin == nil {
		t.Errorf("Logic should win free game")
	}

	if logic.FreeSpin.Count != 10 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 10, logic.FreeSpin.Count)
	}

	if logic.LightningGame == nil {
		t.Errorf("Game should win LightningGame")
	}

	t.Run("User Show Step 1", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(1)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 2", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(2)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 3", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(3)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Show Step 4", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID)

		m := new(netproto.XMBX_MSG_LightningShow)
		m.LightningStep = proto.Int32(4)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.LightningGame != nil {
		t.Errorf("Game should clean LightningGame")
	}

	t.Run("In FreeGame Win 1", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 9 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 9, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 2", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 8 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 8, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 3", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 7 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 7, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 4", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 6 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 6, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 5", func(tt *testing.T) {
		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(602), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 5 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 5, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 6", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 4 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 4, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 7", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 3 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 3, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 8", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 2 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 2, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 9", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin.Count != 1 {
		t.Errorf("FreeSpin.Count want %d, but, %d", 1, logic.FreeSpin.Count)
	}

	t.Run("In FreeGame Win 10", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(5)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if logic.FreeSpin != nil {
		t.Errorf("Logic should clean free game")
	}
}

func printSendMessgeFunc(t *testing.T, userID int32, m *msg.Message) {}

func printSendRPCMessgeFunc(t *testing.T, safe bool, m *msg.Message, handlers map[int32]func()) {
	if handlers != nil {
		if f, ok := handlers[m.SClassID]; ok {
			f()
		}
	}
}

func userEnter(logic *Logic, user *gamelogic.GameUser, t *testing.T, safe bool, m *msg.Message) {
	handlers1 := make(map[int32]func())
	handlers2 := make(map[int32]func())

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {

		detail := new(netproto.Slot_MSG_FreeGameRecord)
		example.invalid = new(netproto.Slot_FreeSpinDetail)
		example.invalid.UserID = proto.Int32(user.UserID)
		example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		example.invalid.FlagID = proto.Int32(int32(f))
		example.invalid.UserBet = proto.Int64(0)
		example.invalid.FreeSpinGroup = proto.Int32(0)
		example.invalid.Times = proto.Int32(0)
		example.invalid.ChooseRate = proto.Int32(0)
		example.invalid.WinMoney = proto.Int64(0)
		example.invalid.IsCoupon = proto.Bool(false)
		example.invalid.OriginID = proto.Int64(0)

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
		detail := new(netproto.Slot_MSG_JackpotRecord)

		jp1 := new(netproto.Slot_JackpotData)
		jp1.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		jp1.FlagID = proto.Int32(int32(f))
		jp1.TableNo = proto.Int32(logic.TableManager.TableNo)
		jp1.Contribution = proto.Int64(0)
		jp1.TotalContribution = proto.Int64(0)
		jp1.AwardTotal = proto.Int64(0)
		jp1.Level = proto.Int32(1)

		jp2 := new(netproto.Slot_JackpotData)
		jp2.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ = strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		jp2.FlagID = proto.Int32(int32(f))
		jp2.TableNo = proto.Int32(logic.TableManager.TableNo)
		jp2.Contribution = proto.Int64(0)
		jp2.TotalContribution = proto.Int64(0)
		jp2.AwardTotal = proto.Int64(0)
		jp2.Level = proto.Int32(2)

		jp3 := new(netproto.Slot_JackpotData)
		jp3.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ = strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		jp3.FlagID = proto.Int32(int32(f))
		jp3.TableNo = proto.Int32(logic.TableManager.TableNo)
		jp3.Contribution = proto.Int64(0)
		jp3.TotalContribution = proto.Int64(0)
		jp3.AwardTotal = proto.Int64(0)
		jp3.Level = proto.Int32(3)

		detail.Records = append(detail.Records, jp1)
		detail.Records = append(detail.Records, jp2)
		detail.Records = append(detail.Records, jp3)

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers2[int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadID)] = func() {

		detail := new(netproto.Slot_MSG_LightningRecord)
		detail.UserID = proto.Int32(user.UserID)
		detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		detail.FlagID = proto.Int32(int32(f))
		detail.Data = proto.String("")

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

	printSendRPCMessgeFunc(t, safe, m, handlers2)
}
