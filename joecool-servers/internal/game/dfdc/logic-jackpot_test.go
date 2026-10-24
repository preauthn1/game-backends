package dfdc

import (
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestDFDCLogic_JackpotGame_WinOnMainGame(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(10 * time.Millisecond)

	logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
	}

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{Id: proto.Int32(56), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("w1"), PayType: proto.String("mysteryJPG"), Count: proto.Int32(1), Payout: proto.String("1e8:0:1e8")},
	}

	t.Run("Win", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame == nil {
			t.Errorf("Jackpot Game should not be nil")
		}
	})
}

func TestDFDCLogic_JackpotGame_WinOnFreeGame(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(10 * time.Millisecond)

	logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
	}

	t.Run("Win FreeGame", func(tt *testing.T) {
		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(10 * time.Millisecond)

		if logic.FreeSpin == nil {
			t.Errorf("FreeGame should not be nil")
		}
	})

	var records []*netproto.DFDC_BonusGameDetail

	t.Run("Win JackpotGame", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameID) {
				data := m.MsgData.(*netproto.DFDC_BonusGame)
				records = data.BonusDetail
			}
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{Id: proto.Int32(56), GroupId: proto.Int32(1), LinkType: proto.String("any"), Symbol: proto.String("w1"), PayType: proto.String("mysteryJPG"), Count: proto.Int32(1), Payout: proto.String("1e8:0:1e8")},
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame == nil {
			t.Errorf("Jackpot Game should not be nil")
		}
	})

	t.Log(records)

	t.Run("Pick Item", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		for i := 0; i < len(records); i++ {
			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameIconID)

			m := new(netproto.DFDC_BonusGameDetail)
			m.Id = proto.Int32(records[i].GetId())
			m.Place = proto.Int32(int32(i + 1))
			m.Icon = proto.Int32(records[i].GetIcon())

			tt.Log(m)

			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(logic.GameUser, _m)

			time.Sleep(10 * time.Millisecond)
		}

		if logic.JackpotGame != nil {
			t.Errorf("JackpotGame should be clean")
		}
	})
}
