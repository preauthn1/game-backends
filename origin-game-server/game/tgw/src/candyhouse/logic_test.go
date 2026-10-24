package candyhouse

import (
	"conf"
	"config"
	"event"
	"fixtures"
	"fmt"
	"gamelogic"
	"msg"
	"netproto"
	"roomserver"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
)

func TestLogic_GameAfterWinJackpot(t *testing.T) {
	conf.CLogLevel = 4
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServer()
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	logic.SlotPattern = &protodata.SlotPattern{
		Id:                      proto.Uint32(1),
		MainGameTableGroupId:    proto.Uint32(1),
		SymbolAttributesGroupId: proto.Uint32(1),
		PayTableGroupId:         proto.Uint32(1),
		LineType:                proto.String("fixed"),
		PayLineGroupId:          proto.Uint32(1),
		JackpotGroup:            proto.Uint32(1),
	}
	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Id: proto.Uint32(1), GroupId: proto.Uint32(1), Symbol: proto.String("w1"), IsGoldsymbol: proto.Int32(0), IsWild: proto.Int32(1)},
	}
	logic.MainGameTable = &protodata.MainGameTable{
		Id:            proto.Uint32(1),
		GroupId:       proto.Uint32(1),
		SymbolGroupId: proto.Uint32(1),
		ReelNum:       proto.Uint32(5),
		RowNum:        proto.Uint32(3),
		IsGoldsymbol:  proto.Int32(0),
	}
	logic.SlotPayLine = []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(1), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
	}
	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("left"), Symbol: proto.String("w1"), PayType: proto.String("jackpot"), Count: proto.Int32(5)},
	}
	logic.JackpotTable = []*protodata.JackpotTable{
		&protodata.JackpotTable{Id: proto.Int32(1), GroupId: proto.Int32(1), Increment: proto.Int32(1), IsMultiLevel: proto.Int32(0), IsCeling: proto.Int32(0), StartUp: proto.Int64(100000)},
	}

	logic.MaxBetCount = 1000
	logic.DefaultReelNum = int32(logic.MainGameTable.GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable.GetRowNum())

	logic.PayLineMode = slot.PayLineMode(logic.SlotPattern.GetLineType())

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.initJackpot()

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("First Spin", func(tt *testing.T) {
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		}

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_GameResultID) {
				data := m.MsgData.(*netproto.TGW_MSG_GameResult)
				fmt.Printf("BetMoney: %d, WinMoney: %d, JackpotMoney: %d, FinalMoney: %d\n", data.GetBetMoney(), data.GetWinMoney(), data.GetJackpotMoney(), data.GetFinalMoney())

				if data.GetBetMoney() != 1000 {
					tt.Errorf("BetMoney want %d, but %d", 1000, data.GetBetMoney())
				}

				if data.GetWinMoney() != 0 {
					tt.Errorf("WinMoney want %d, but %d", 0, data.GetWinMoney())
				}

				if data.GetJackpotMoney() != 100000 {
					tt.Errorf("JackpotMoney want %d, but %d", 100000, data.GetJackpotMoney())
				}

				if data.GetFinalMoney() != logic.GameUser.Score+100000-1000 {
					tt.Errorf("FinalMoney want %d, but %d", logic.GameUser.Score+100000-1000, data.GetFinalMoney())
				}

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionSpinID)

		m := new(netproto.TGW_MSG_Spin)
		m.Money = proto.Int64(1000)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	time.Sleep(100 * time.Millisecond)

	t.Run("Second Spin", func(tt *testing.T) {
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		}

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_GameResultID) {
				data := m.MsgData.(*netproto.TGW_MSG_GameResult)
				fmt.Printf("BetMoney: %d, WinMoney: %d, JackpotMoney: %d, FinalMoney: %d\n", data.GetBetMoney(), data.GetWinMoney(), data.GetJackpotMoney(), data.GetFinalMoney())

				if data.GetBetMoney() != 1000 {
					tt.Errorf("BetMoney want %d, but %d", 1000, data.GetBetMoney())
				}

				if data.GetWinMoney() != 0 {
					tt.Errorf("WinMoney want %d, but %d", 0, data.GetWinMoney())
				}

				if data.GetJackpotMoney() != 0 {
					tt.Errorf("JackpotMoney want %d, but %d", 0, data.GetJackpotMoney())
				}

				if data.GetFinalMoney() != logic.GameUser.Score-1000 {
					tt.Errorf("FinalMoney want %d, but %d", logic.GameUser.Score-1000, data.GetFinalMoney())
				}

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionSpinID)

		m := new(netproto.TGW_MSG_Spin)
		m.Money = proto.Int64(1000)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	time.Sleep(100 * time.Millisecond)

	t.Run("3th Spin", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_GameResultID) {
				data := m.MsgData.(*netproto.TGW_MSG_GameResult)
				fmt.Printf("BetMoney: %d, WinMoney: %d, JackpotMoney: %d, FinalMoney: %d\n", data.GetBetMoney(), data.GetWinMoney(), data.GetJackpotMoney(), data.GetFinalMoney())

				if data.GetBetMoney() != 1000 {
					tt.Errorf("BetMoney want %d, but %d", 1000, data.GetBetMoney())
				}

				if data.GetWinMoney() != 0 {
					tt.Errorf("WinMoney want %d, but %d", 0, data.GetWinMoney())
				}

				if data.GetJackpotMoney() != 0 {
					tt.Errorf("JackpotMoney want %d, but %d", 0, data.GetJackpotMoney())
				}

				if data.GetFinalMoney() != logic.GameUser.Score-1000 {
					tt.Errorf("FinalMoney want %d, but %d", logic.GameUser.Score-1000, data.GetFinalMoney())
				}

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionSpinID)

		m := new(netproto.TGW_MSG_Spin)
		m.Money = proto.Int64(1000)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	time.Sleep(100 * time.Millisecond)

	t.Run("4th Spin", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_GameResultID) {
				data := m.MsgData.(*netproto.TGW_MSG_GameResult)
				fmt.Printf("BetMoney: %d, WinMoney: %d, JackpotMoney: %d, FinalMoney: %d\n", data.GetBetMoney(), data.GetWinMoney(), data.GetJackpotMoney(), data.GetFinalMoney())

				if data.GetBetMoney() != 1000 {
					tt.Errorf("BetMoney want %d, but %d", 1000, data.GetBetMoney())
				}

				if data.GetWinMoney() != 0 {
					tt.Errorf("WinMoney want %d, but %d", 0, data.GetWinMoney())
				}

				if data.GetJackpotMoney() != 0 {
					tt.Errorf("JackpotMoney want %d, but %d", 0, data.GetJackpotMoney())
				}

				if data.GetFinalMoney() != logic.GameUser.Score-1000 {
					tt.Errorf("FinalMoney want %d, but %d", logic.GameUser.Score-1000, data.GetFinalMoney())
				}

			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionSpinID)

		m := new(netproto.TGW_MSG_Spin)
		m.Money = proto.Int64(1000)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})
}

func TestLogic_GameMessage_PickBonusGame(t *testing.T) {
	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServer()
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	logic.SlotPattern = &protodata.SlotPattern{
		Id:                      proto.Uint32(1),
		MainGameTableGroupId:    proto.Uint32(1),
		SymbolAttributesGroupId: proto.Uint32(1),
		PayTableGroupId:         proto.Uint32(1),
		LineType:                proto.String("fixed"),
		PayLineGroupId:          proto.Uint32(1),
		JackpotGroup:            proto.Uint32(1),
	}
	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Id: proto.Uint32(1), GroupId: proto.Uint32(1), Symbol: proto.String("w1"), IsGoldsymbol: proto.Int32(0), IsWild: proto.Int32(1)},
	}
	logic.MainGameTable = &protodata.MainGameTable{
		Id:            proto.Uint32(1),
		GroupId:       proto.Uint32(1),
		SymbolGroupId: proto.Uint32(1),
		ReelNum:       proto.Uint32(5),
		RowNum:        proto.Uint32(3),
		IsGoldsymbol:  proto.Int32(0),
	}
	logic.SlotPayLine = []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(1), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
	}
	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(9), Payout: proto.String("1")},
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(8), Payout: proto.String("2")},
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(7), Payout: proto.String("3")},
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(6), Payout: proto.String("4")},
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(5), Payout: proto.String("5")},
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(4), Payout: proto.String("6")},
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("b1"), PayType: proto.String("pickgame"), Count: proto.Int32(3), Payout: proto.String("7")},
	}
	logic.JackpotTable = []*protodata.JackpotTable{
		&protodata.JackpotTable{Id: proto.Int32(1), GroupId: proto.Int32(1), Increment: proto.Int32(1), IsMultiLevel: proto.Int32(0), IsCeling: proto.Int32(0), StartUp: proto.Int64(100000)},
	}
	logic.PickGameTable = []*protodata.PickGameTable{
		&protodata.PickGameTable{Id: proto.Uint32(1), GroupId: proto.Int32(999), PickCount: proto.Uint32(9), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(2), GroupId: proto.Int32(999), PickCount: proto.Uint32(8), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(3), GroupId: proto.Int32(999), PickCount: proto.Uint32(7), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(4), GroupId: proto.Int32(999), PickCount: proto.Uint32(6), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(5), GroupId: proto.Int32(999), PickCount: proto.Uint32(5), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(6), GroupId: proto.Int32(999), PickCount: proto.Uint32(4), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(7), GroupId: proto.Int32(999), PickCount: proto.Uint32(3), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
	}
	logic.BonusTable = []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("2"), PerProb: proto.Int32(1000000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("3"), PerProb: proto.Int32(500000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("4"), PerProb: proto.Int32(250000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("5"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("6"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("7"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("8"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("9"), PerProb: proto.Int32(500)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100)},
	}

	logic.MaxBetCount = 1000
	logic.DefaultReelNum = int32(logic.MainGameTable.GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable.GetRowNum())

	logic.PayLineMode = slot.PayLineMode(logic.SlotPattern.GetLineType())

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.initJackpot()

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	// Init BonusGame
	logic.BonusGame = NewBonusGame(logic.BonusTable, logic.BonusGameSize, 4, 10)

	time.Sleep(100 * time.Millisecond)

	t.Run("Pick Error", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(99),
			Location:    proto.Int32(3),
			RewareValue: proto.Int64(4),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	t.Run("First Pick", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(1),
			Location:    proto.Int32(3),
			RewareValue: proto.Int64(4),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	time.Sleep(100 * time.Millisecond)

	t.Run("Second Pick", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(2),
			Location:    proto.Int32(5),
			RewareValue: proto.Int64(1),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	time.Sleep(100 * time.Millisecond)

	t.Run("3th Pick", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(3),
			Location:    proto.Int32(6),
			RewareValue: proto.Int64(8),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	time.Sleep(100 * time.Millisecond)

	t.Run("4th Pick", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(4),
			Location:    proto.Int32(9),
			RewareValue: proto.Int64(2),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	t.Run("Is Finished", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(4),
			Location:    proto.Int32(9),
			RewareValue: proto.Int64(2),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	t.Run("No Record", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	logic.BonusGame = nil

	t.Run("BonusGame Not Exist", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickRetID) {
				data := m.MsgData.(*netproto.TGW_MSG_PickRet)

				if data.GetRecord() == nil {
					tt.Errorf("Record is nil")
				}
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID)

		m := new(netproto.TGW_MSG_Pick)
		m.Record = &netproto.TGW_BonusGameRecord{
			ID:          proto.Int32(4),
			Location:    proto.Int32(9),
			RewareValue: proto.Int64(2),
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})
}

func TestLogic_GameMessage_LoadHistory(t *testing.T) {
	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServer()
	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	logic.SlotPattern = &protodata.SlotPattern{
		Id:                      proto.Uint32(1),
		MainGameTableGroupId:    proto.Uint32(1),
		SymbolAttributesGroupId: proto.Uint32(1),
		PayTableGroupId:         proto.Uint32(1),
		LineType:                proto.String("fixed"),
		PayLineGroupId:          proto.Uint32(1),
		JackpotGroup:            proto.Uint32(1),
	}
	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Id: proto.Uint32(1), GroupId: proto.Uint32(1), Symbol: proto.String("w1"), IsGoldsymbol: proto.Int32(0), IsWild: proto.Int32(1)},
	}
	logic.MainGameTable = &protodata.MainGameTable{
		Id:            proto.Uint32(1),
		GroupId:       proto.Uint32(1),
		SymbolGroupId: proto.Uint32(1),
		ReelNum:       proto.Uint32(5),
		RowNum:        proto.Uint32(3),
		IsGoldsymbol:  proto.Int32(0),
	}
	logic.SlotPayLine = []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(1), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
	}
	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{Id: proto.Int32(1), GroupId: proto.Int32(1), LinkType: proto.String("left"), Symbol: proto.String("w1"), PayType: proto.String("jackpot"), Count: proto.Int32(5)},
	}
	logic.JackpotTable = []*protodata.JackpotTable{
		&protodata.JackpotTable{Id: proto.Int32(1), GroupId: proto.Int32(1), Increment: proto.Int32(1), IsMultiLevel: proto.Int32(0), IsCeling: proto.Int32(0), StartUp: proto.Int64(100000)},
	}

	logic.MaxBetCount = 1000
	logic.DefaultReelNum = int32(logic.MainGameTable.GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable.GetRowNum())

	logic.PayLineMode = slot.PayLineMode(logic.SlotPattern.GetLineType())

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.initJackpot()

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("Error Proto", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_LoadHistoryID)

		m := new(netproto.Slot_LoadHistoryData)
		m.PageIndex = proto.Int32(1)
		m.PageSize = proto.Int32(10)
		m.StartTime = proto.String("2019-01-01")
		m.EndTime = proto.String("2019-01-01")
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})

	t.Run("Normal Proto", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_LoadHistoryID)

		m := new(netproto.Slot_LoadHistoryData)
		m.PageIndex = proto.Int32(1)
		m.PageSize = proto.Int32(10)
		m.StartTime = proto.String("2019-01-01")
		m.EndTime = proto.String("2019-01-01")
		m.UserID = proto.Int32(1)
		m.GameID = proto.Int32(26)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)
	})
}
