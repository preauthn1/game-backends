package dhzw

import (
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestLogic_JackpotGame_Win(t *testing.T) {

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

	t.Run("No Win Jackpot Game", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("any"), Symbol: proto.String("w1"), PayType: proto.String("mysteryJPGR"), Count: proto.Int32(1), Payout: proto.String("28:0:0:10,38:10:0:0,58:10:0:0,88:10:0:0"), UseType: proto.String("all")},
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(100)
		m.Rate = proto.Int32(28)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.JackpotGame != nil {
			tt.Errorf("Jackpot Game should be nil")
		}

		logic.JackpotGame = nil
	})

	t.Run("Win Link Jackpot Game", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("any"), Symbol: proto.String("w1"), PayType: proto.String("mysteryJPGR"), Count: proto.Int32(1), Payout: proto.String("28:1:0:280,38:10:0:0,58:10:0:0,88:10:0:0"), UseType: proto.String("all")},
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(28)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.JackpotGame == nil {
			tt.Errorf("Jackpot Game should not be nil")
		}

		logic.JackpotGame = nil
	})
}

func TestLogic_JackpotGame_Play(t *testing.T) {

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

	t.Run("Win Link Jackpot Game", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BonusGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_BonusGame)
				records = data.Records
			}
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		}

		// logic.SlotPayTable = []*protodata.SlotPayTable{
		// 	&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("left"), Symbol: proto.String("p1"), PayType: proto.String("chip"), Count: proto.Int32(4), Payout: proto.String("500"), UseType: proto.String("all")},
		// 	&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("left"), Symbol: proto.String("bg4"), PayType: proto.String("mysteryJPGR"), Count: proto.Int32(4), Payout: proto.String("28:1:0:280,38:10:0:0,58:10:0:0,88:10:0:0"), UseType: proto.String("all")},
		// }

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(28)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.JackpotGame == nil {
			tt.Errorf("Jackpot Game should not be nil")
		}
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
