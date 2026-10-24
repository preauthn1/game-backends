package jjbx

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

func TestLogic_JackpotGame(t *testing.T) {

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
				// logic_test.go:79: id:324 groupId:7 linkType:"any" symbol:"w1" payType:"mysteryjackpotR" count:2 payout:"8:58837,18:64000,38:70000,68:67000,88:70045" useType:"all"
				&protodata.SlotPayTable{Id: proto.Int32(324), GroupId: proto.Int32(7), LinkType: proto.String("any"), Symbol: proto.String("w1"), PayType: proto.String("mysteryjackpotR"), Count: proto.Int32(2), Payout: proto.String("8:10000000000,18:10000000000,38:10000000000,68:10000000000,88:10000000000"), UseType: proto.String("all")},
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
