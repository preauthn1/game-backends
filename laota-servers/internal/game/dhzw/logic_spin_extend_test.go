package dhzw

import (
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
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestLogic_Spin_Extend(t *testing.T) {

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

	t.Run("Spin", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		// 2019/12/28 20:39:13.873942 logic-print-log.go:33: [debug] 第 1 行: p1, p1, p1, w1, w14
		// 2019/12/28 20:39:13.873952 logic-print-log.go:33: [debug] 第 2 行: p2, p2, p2, p2, p2
		// 2019/12/28 20:39:13.874011 logic-print-log.go:33: [debug] 第 3 行: p3, p3, p3, p3, p3
		// 2019/12/28 20:39:13.874043 logic-print-log.go:33: [debug] 第 4 行: p4, p4, p4, p4, p4
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w14"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		}

		logic.ExtraRule = []*protodata.ExtraRule{
			&protodata.ExtraRule{Id: proto.Int32(1), GroupId: proto.Int32(1), UseType: proto.String("all"), Condition: proto.String("AppearSymbol"), ConditionValue: proto.String("w14|Reel|2,3,4,5"), Action: proto.String("ExpandReel"), ActionValue: proto.String("w14")},
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
	})
}
