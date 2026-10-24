package jjbx

import (
	"fmt"
	"sort"
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
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

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

	symbolGroupIDMap := make(map[int32]bool)
	for _, row := range logic.SlotSymbolDistribution {
		symbolGroupIDMap[int32(row.GetGroupId())] = true
	}
	var groupIDs []int32
	for id := range symbolGroupIDMap {
		groupIDs = append(groupIDs, id)
	}
	sort.Slice(groupIDs, func(i, j int) bool {
		return groupIDs[i] < groupIDs[j]
	})

	t.Log("=== SlotSymbolGroups ===", len(groupIDs))
	for _, row := range groupIDs {
		t.Log("SymbolGroupID: ", row)
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

	t.Log("=== SelectionTable ===", len(logic.SelectionTable))
	for _, row := range logic.SelectionTable {
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

func TestLogic_Spin_Normal(t *testing.T) {
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

	t.Run("Random Rate 8", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Random Rate 18", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(18)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Random Rate 38", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(38)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Random Rate 68", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(68)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Random Rate 88", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(88)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Link", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
				data := m.MsgData.(*netproto.Slot_MSG_GameResult)

				if data.GetWinMoney() != 450 {
					tt.Errorf("WinMoney want %d, but %d", 450, data.GetWinMoney())
				}

			}
		}

		logic.SelectChances = make([]*SelectChance, 0)
		logic.GameState = slot.GameStateBet

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_JackpotGame(t *testing.T) {
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

	var records []*netproto.Slot_BonusGameRecord

	t.Run("Win Jackpot Game", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BonusGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_BonusGame)
				records = data.Records
			}
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}

			logic.SlotPayTable = []*protodata.SlotPayTable{
				// logic_test.go:68: id:324 groupId:7 linkType:"any" symbol:"w1" payType:"mysteryjackpot" count:2 payout:"98367" useType:"all"
				&protodata.SlotPayTable{Id: proto.Int32(324), GroupId: proto.Int32(7), LinkType: proto.String("any"), Symbol: proto.String("w1"), PayType: proto.String("mysteryjackpot"), Count: proto.Int32(2), Payout: proto.String("100000000"), UseType: proto.String("all")},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(88)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(records) == 0 {
		t.Errorf("Jackpot Game should has record")
	}

	t.Run("Pick Item", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		for i := 0; i < len(records); i++ {
			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_PickBonusID)

			m := new(netproto.Slot_MSG_PickBonus)
			m.Record = new(netproto.Slot_BonusGameRecord)
			m.Record.ID = proto.Int32(records[i].GetID())
			m.Record.Location = proto.Int32(int32(i + 1))
			m.Record.RewareValue = proto.Int64(records[i].GetRewareValue())

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		}
	})
}

func TestLogic_WinJackpot(t *testing.T) {
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

	t.Run("Win Jackpot Rate 8", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	logic.SelectChances = nil
	logic.GameState = slot.GameStateBet

	t.Run("Win Jackpot Rate 18", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(702), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(18)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	logic.SelectChances = nil
	logic.GameState = slot.GameStateBet

	t.Run("Win Jackpot Rate 38", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(703), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(38)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	logic.SelectChances = nil
	logic.GameState = slot.GameStateBet

	t.Run("Rate 68", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(704), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(68)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	logic.SelectChances = nil
	logic.GameState = slot.GameStateBet

	t.Run("Rate 88", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(705), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(88)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	logic.SelectChances = nil
	logic.GameState = slot.GameStateBet
}

func TestLogic_SelectGame_Rate8_Win(t *testing.T) {
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

	t.Run("Win Select Game", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_SelectGame_Rate8_ChooseFreeSpin_Normal(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
				data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
				opts = data.Options
			}
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose Free Spin", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for leftFreeGame != nil && leftFreeGame.GetCount() > 0 {
		t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
					data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
					leftFreeGame = data
				}
			}

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

			m := new(netproto.Slot_MSG_Spin)

			m.Money = proto.Int64(leftFreeGame.GetBetMoney())
			m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		})
	}
}

func TestLogic_SelectGame_Rate8_ChooseFreeSpin_WinCount(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
				data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
				opts = data.Options
			}
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose Free Spin", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)

		m.Money = proto.Int64(leftFreeGame.GetBetMoney())
		m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)

		m.Money = proto.Int64(leftFreeGame.GetBetMoney())
		m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Add Count", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
					data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
					leftFreeGame = data
				}
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)

		m.Money = proto.Int64(leftFreeGame.GetBetMoney())
		m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for leftFreeGame != nil && leftFreeGame.GetCount() > 0 {
		t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
					data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
					leftFreeGame = data
				}
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

			m := new(netproto.Slot_MSG_Spin)

			m.Money = proto.Int64(leftFreeGame.GetBetMoney())
			m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		})
	}
}

func TestLogic_SelectGame_Rate8_ChooseFreeSpin_WinJackpotGame(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
				data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
				opts = data.Options
			}
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose Free Spin", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for leftFreeGame != nil && leftFreeGame.GetCount() > 0 {

		t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
					data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
					leftFreeGame = data
				}

				if leftFreeGame.GetCount() > 0 {
					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
						data := m.MsgData.(*netproto.Slot_MSG_GameResult)
						if data.GetWinMoney() != 3000 {
							tt.Errorf("WinMoney want %d, but %d", 3000, data.GetWinMoney())
						}
					}
				}
			}

			if leftFreeGame.GetCount() > 0 {
				logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				}

				logic.SlotPayTable = []*protodata.SlotPayTable{
					// logic_test.go:79: id:345 groupId:7 linkType:"any" symbol:"g7" payType:"mysterygrand" count:1 payout:"98367" useType:"freespin"
					&protodata.SlotPayTable{Id: proto.Int32(345), GroupId: proto.Int32(7), LinkType: proto.String("any"), Symbol: proto.String("g7"), PayType: proto.String("mysterygrand"), Count: proto.Int32(1), Payout: proto.String("100000000"), UseType: proto.String("freespin")},
					// logic_test.go:79: id:276 groupId:7 linkType:"left" symbol:"p2" payType:"chip" count:4 payout:"10" useType:"all"
					&protodata.SlotPayTable{Id: proto.Int32(276), GroupId: proto.Int32(7), LinkType: proto.String("left"), Symbol: proto.String("p2"), PayType: proto.String("chip"), Count: proto.Int32(5), Payout: proto.String("100"), UseType: proto.String("all")},
				}
			} else {
				logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				}

				logic.SlotPayTable = []*protodata.SlotPayTable{
					// logic_test.go:79: id:345 groupId:7 linkType:"any" symbol:"g7" payType:"mysterygrand" count:1 payout:"98367" useType:"freespin"
					&protodata.SlotPayTable{Id: proto.Int32(345), GroupId: proto.Int32(7), LinkType: proto.String("any"), Symbol: proto.String("g7"), PayType: proto.String("mysterygrand"), Count: proto.Int32(1), Payout: proto.String("100000000"), UseType: proto.String("freespin")},
				}
			}
			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

			m := new(netproto.Slot_MSG_Spin)

			m.Money = proto.Int64(leftFreeGame.GetBetMoney())
			m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		})
	}

}

func TestLogic_SelectGame_Rate8_ChooseFreeSpin_WinJackpotNotLast(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
				data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
				opts = data.Options
			}
		}

		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose Free Spin", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for leftFreeGame != nil && leftFreeGame.GetCount() > 0 {

		t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
			if leftFreeGame.GetCount() > 1 {
				logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				}

				logic.SlotPayTable = []*protodata.SlotPayTable{
					// logic_test.go:79: id:345 groupId:7 linkType:"any" symbol:"g7" payType:"mysterygrand" count:1 payout:"98367" useType:"freespin"
					&protodata.SlotPayTable{Id: proto.Int32(345), GroupId: proto.Int32(7), LinkType: proto.String("any"), Symbol: proto.String("g7"), PayType: proto.String("mysterygrand"), Count: proto.Int32(1), Payout: proto.String("100000000"), UseType: proto.String("freespin")},
					// logic_test.go:79: id:276 groupId:7 linkType:"left" symbol:"p2" payType:"chip" count:4 payout:"10" useType:"all"
					&protodata.SlotPayTable{Id: proto.Int32(276), GroupId: proto.Int32(7), LinkType: proto.String("left"), Symbol: proto.String("p2"), PayType: proto.String("chip"), Count: proto.Int32(5), Payout: proto.String("100"), UseType: proto.String("all")},
				}

				logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
					slot_test.PrintSendMessgeFunc(tt, userID, m)
					printSendMessgeFunc(tt, userID, m)

					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
						data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
						leftFreeGame = data
					}

					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
						data := m.MsgData.(*netproto.Slot_MSG_GameResult)
						if data.GetWinMoney() != 3000 {
							tt.Errorf("WinMoney want %d, but %d", 3000, data.GetWinMoney())
						}
						if data.GetJackpotMoney() != 181818 {
							tt.Errorf("JackpotMoney want %d, but %d", 181818, data.GetJackpotMoney())
						}
					}
				}

			} else {
				logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
					&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(706), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				}

				logic.SlotPayTable = []*protodata.SlotPayTable{
					// logic_test.go:79: id:345 groupId:7 linkType:"any" symbol:"g7" payType:"mysterygrand" count:1 payout:"98367" useType:"freespin"
					&protodata.SlotPayTable{Id: proto.Int32(345), GroupId: proto.Int32(7), LinkType: proto.String("any"), Symbol: proto.String("g7"), PayType: proto.String("mysterygrand"), Count: proto.Int32(1), Payout: proto.String("100000000"), UseType: proto.String("freespin")},
					// logic_test.go:79: id:276 groupId:7 linkType:"left" symbol:"p2" payType:"chip" count:4 payout:"10" useType:"all"
					&protodata.SlotPayTable{Id: proto.Int32(276), GroupId: proto.Int32(7), LinkType: proto.String("left"), Symbol: proto.String("p2"), PayType: proto.String("chip"), Count: proto.Int32(5), Payout: proto.String("100"), UseType: proto.String("all")},
				}

				logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
					slot_test.PrintSendMessgeFunc(tt, userID, m)
					printSendMessgeFunc(tt, userID, m)

					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
						data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
						leftFreeGame = data
					}

					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
						data := m.MsgData.(*netproto.Slot_MSG_GameResult)
						if data.GetWinMoney() != 3000 {
							tt.Errorf("WinMoney want %d, but %d", 3000, data.GetWinMoney())
						}
						if data.GetJackpotMoney() != 0 {
							tt.Errorf("JackpotMoney want %d, but %d", 0, data.GetJackpotMoney())
						}
					}
				}
			}
			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

			m := new(netproto.Slot_MSG_Spin)

			m.Money = proto.Int64(leftFreeGame.GetBetMoney())
			m.Rate = proto.Int32(int32(leftFreeGame.GetBetRate()))

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		})
	}

}

func TestLogic_SelectGame_Rate8_ChooseLightning_SumBall(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
					data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
					opts = data.Options
				}
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if len(opts) == 0 {
			tt.Errorf("Game should has Select Game options")
		}
	})

	t.Run("Choose Lightning", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				// if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID) {
				// 	time.Sleep(300 * time.Millisecond)

				// 	data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

				// 	Prepare = data
				// }

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "lightning" {
				opt = row
			}
		}

		m.Option = opt

		tt.Log("Send:", m)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame == nil {
			tt.Errorf("Game should gen Lightn Game")
		}

		if logic.LightningGame.WinMoney != 11080 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 11080, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 1 - g7", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(1)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 22160 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 22160, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 2 - g8, g7 one time", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(2)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 66480 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 66480, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 3 - x", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(3)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 66480 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 66480, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 4 - g8", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(4)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 132960 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 132960, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 5 - g8", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(5)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 265920 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 265920, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 6 - g7 Add one time", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("b1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(6)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 277000 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 277000, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 7 - g8 Add one time", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("b1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(7)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.LightningGame.WinMoney != 554000 {
			tt.Errorf("Lightn Game WinMoney want %d, bug %d", 554000, logic.LightningGame.WinMoney)
		}
	})

	t.Run("Show Step 8 - g8 g8", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g7"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g8"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

		m := new(netproto.Slot_MSG_LightningShow)
		m.LightningStep = proto.Int32(8)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		// if logic.LightningGame.WinMoney != 11080+11080+11080+33240+11080+77560+155120+11080+321320+642640 {
		// 	tt.Errorf("Lightn Game WinMoney want %d, bug %d", 11080+11080+11080+33240+11080+77560+155120+11080+321320+642640, logic.LightningGame.WinMoney)
		// }
	})

}

func TestLogic_SelectGame_Rate8_ChooseLightning_LimitToEnd(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
					data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
					opts = data.Options
				}
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("b1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(726), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var Prepare *netproto.Slot_MSG_LightningPrepare

	t.Run("Choose Lightning", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID) {
					time.Sleep(300 * time.Millisecond)

					data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

					Prepare = data
				}

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "lightning" {
				opt = row
			}
		}

		m.Option = opt

		tt.Log("Send:", m)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for Prepare != nil {
		t.Run(fmt.Sprintf("Show Step %d", Prepare.GetLightningStep()), func(tt *testing.T) {
			{
				logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
					slot_test.PrintSendMessgeFunc(tt, userID, m)
					printSendMessgeFunc(tt, userID, m)

					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID) {
						time.Sleep(300 * time.Millisecond)

						data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

						Prepare = data
					}

				}
			}

			tmp := Prepare
			Prepare = nil

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

			m := new(netproto.Slot_MSG_LightningShow)
			m.LightningStep = proto.Int32(tmp.GetLightningStep())

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		})
	}
}

func TestLogic_SelectGame_Rate8_ChooseLightning_Jackpot(t *testing.T) {
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

	var opts []*netproto.Slot_ChooseGameOption

	t.Run("Win Select Game", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
					data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)
					opts = data.Options
				}
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var Prepare *netproto.Slot_MSG_LightningPrepare

	t.Run("Choose Lightning", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID) {
					time.Sleep(300 * time.Millisecond)

					data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

					Prepare = data
				}

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "lightning" {
				opt = row
			}
		}

		m.Option = opt

		tt.Log("Send:", m)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for Prepare != nil {
		t.Run(fmt.Sprintf("Show Step %d", Prepare.GetLightningStep()), func(tt *testing.T) {
			{
				logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
					slot_test.PrintSendMessgeFunc(tt, userID, m)
					printSendMessgeFunc(tt, userID, m)

					if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID) {
						time.Sleep(300 * time.Millisecond)

						data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

						Prepare = data
					}

				}
			}

			tmp := Prepare
			Prepare = nil

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionLightningSpinID)

			m := new(netproto.Slot_MSG_LightningShow)
			m.LightningStep = proto.Int32(tmp.GetLightningStep())

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(100 * time.Millisecond)
		})
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

	handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

		detail := new(netproto.Slot_MSG_SelectGameRecord)
		example.invalid = new(netproto.Slot_SelectGameInfo)
		example.invalid.UserID = proto.Int32(user.UserID)
		example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		example.invalid.FlagID = proto.Int32(int32(f))
		example.invalid.Times = proto.Int32(0)

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers2[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {

		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)

		detail := new(netproto.Slot_MSG_BonusGameRecord)
		detail.Detail = append(detail.Detail, &netproto.Slot_BonusGameDetail{
			UserID:   proto.Int32(user.UserID),
			GameID:   proto.Int32(logic.TableManager.Room.GameInfo.GameID),
			FlagID:   proto.Int32(int32(f)),
			OriginID: proto.Int32(0),
			TurnID:   proto.Int32(1),
			Location: proto.Int32(0),
			BetMoney: proto.Int32(10),
			BetRate:  proto.Int32(8),
			Data:     proto.String(""),
		})

		detail.Detail = append(detail.Detail, &netproto.Slot_BonusGameDetail{
			UserID:   proto.Int32(user.UserID),
			GameID:   proto.Int32(logic.TableManager.Room.GameInfo.GameID),
			FlagID:   proto.Int32(int32(f)),
			OriginID: proto.Int32(0),
			TurnID:   proto.Int32(2),
			Location: proto.Int32(0),
			BetMoney: proto.Int32(10),
			BetRate:  proto.Int32(8),
			Data:     proto.String(""),
		})

		detail.Detail = append(detail.Detail, &netproto.Slot_BonusGameDetail{
			UserID:   proto.Int32(user.UserID),
			GameID:   proto.Int32(logic.TableManager.Room.GameInfo.GameID),
			FlagID:   proto.Int32(int32(f)),
			OriginID: proto.Int32(0),
			TurnID:   proto.Int32(3),
			Location: proto.Int32(0),
			BetMoney: proto.Int32(10),
			BetRate:  proto.Int32(8),
			Data:     proto.String(""),
		})

		detail.Detail = append(detail.Detail, &netproto.Slot_BonusGameDetail{
			UserID:   proto.Int32(user.UserID),
			GameID:   proto.Int32(logic.TableManager.Room.GameInfo.GameID),
			FlagID:   proto.Int32(int32(f)),
			OriginID: proto.Int32(0),
			TurnID:   proto.Int32(4),
			Location: proto.Int32(0),
			BetMoney: proto.Int32(10),
			BetRate:  proto.Int32(8),
			Data:     proto.String(""),
		})

		detail.Detail = append(detail.Detail, &netproto.Slot_BonusGameDetail{
			UserID:   proto.Int32(user.UserID),
			GameID:   proto.Int32(logic.TableManager.Room.GameInfo.GameID),
			FlagID:   proto.Int32(int32(f)),
			OriginID: proto.Int32(0),
			TurnID:   proto.Int32(5),
			Location: proto.Int32(0),
			BetMoney: proto.Int32(10),
			BetRate:  proto.Int32(8),
			Data:     proto.String(""),
		})

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

	printSendRPCMessgeFunc(t, safe, m, handlers2)
}
