package dhzw

import (
	"fmt"
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

func TestLogic_FreeGame_SpinToEnd(t *testing.T) {

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

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

		if len(logic.SelectChances) == 0 {
			tt.Errorf("SelectChance should not be 0")
		}
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose", func(tt *testing.T) {
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
				break
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	for leftFreeGame != nil && leftFreeGame.GetCount() > 0 {

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

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

func TestLogic_FreeGame_WinCount(t *testing.T) {
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

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
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

		if len(logic.SelectChances) == 0 {
			tt.Errorf("SelectChance should not be 0")
		}
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	// var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)

			// if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
			// 	data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
			// 	leftFreeGame = data
			// }
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
				break
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Count", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

func TestLogic_FreeGame_WinJackpotGame(t *testing.T) {
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

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

		if len(logic.SelectChances) == 0 {
			tt.Errorf("SelectChance should not be 0")
		}
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	// var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)

			// if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
			// 	data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
			// 	leftFreeGame = data
			// }
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
				break
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Jackpot Game", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
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

		if len(logic.JackpotGameChances) != 1 {
			tt.Errorf("JackpotGameChance want %d, but %d", 1, len(logic.JackpotGameChances))
		}
	})
}

func TestLogic_FreeGame_SpinOut_AppearJackpotGame(t *testing.T) {
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

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

		if len(logic.SelectChances) == 0 {
			tt.Errorf("SelectChance should not be 0")
		}
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	var leftFreeGame *netproto.Slot_MSG_LeftFreeGameData

	t.Run("Choose", func(tt *testing.T) {
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
				break
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Jackpot Game 1", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
				data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
				leftFreeGame = data
			}
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
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

		if len(logic.JackpotGameChances) != 1 {
			tt.Errorf("JackpotGameChance want %d, but %d", 1, len(logic.JackpotGameChances))
		}
	})

	t.Run("Win Jackpot Game 2", func(tt *testing.T) {
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

		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(28)

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if len(logic.JackpotGameChances) != 2 {
			tt.Errorf("JackpotGameChance want %d, but %d", 2, len(logic.JackpotGameChances))
		}
	})

	var records []*netproto.Slot_BonusGameRecord

	for leftFreeGame != nil && leftFreeGame.GetCount() > 0 {

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		}

		t.Run(fmt.Sprintf("Free Spin left %d", leftFreeGame.GetCount()), func(tt *testing.T) {
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
				printSendMessgeFunc(tt, userID, m)

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
					data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
					leftFreeGame = data
				}

				if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BonusGameID) {
					data := m.MsgData.(*netproto.Slot_MSG_BonusGame)
					records = data.Records
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

func TestLogic_FreeGame_WinWild(t *testing.T) {

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

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(16), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(17), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(18), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(19), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(20), GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

		if len(logic.SelectChances) == 0 {
			tt.Errorf("SelectChance should not be 0")
		}
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	t.Run("Choose", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
				break
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.FreeSpin == nil {
			tt.Errorf("FreeSpin should not be nil")
		}
	})

	t.Run("Win Wild", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(16), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(17), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(18), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(19), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(20), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
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

	t.Run("Win Wild", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(16), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p9"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(17), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p9"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(18), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p9"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(19), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p9"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(20), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p9"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
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

func TestLogic_FreeGame_WinExtendWild(t *testing.T) {

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

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

		if len(logic.SelectChances) == 0 {
			tt.Errorf("SelectChance should not be 0")
		}
	})

	if len(opts) == 0 {
		t.Errorf("Game should has Select Game options")
	}

	t.Run("Choose", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionChooseGameID)

		m := new(netproto.Slot_MSG_ChooseGame)

		var opt *netproto.Slot_ChooseGameOption
		for _, row := range opts {
			if row.GetType() == "free_spins" {
				opt = row
				break
			}
		}

		m.Option = opt

		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)

		if logic.FreeSpin == nil {
			tt.Errorf("FreeSpin should not be nil")
		}
	})

	t.Run("Win Wild", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w24"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(16), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(17), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(18), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(19), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(20), GroupId: proto.Uint32(805), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		}

		logic.ExtraRule = []*protodata.ExtraRule{
			&protodata.ExtraRule{Id: proto.Int32(1), GroupId: proto.Int32(1), UseType: proto.String("all"), Condition: proto.String("AppearSymbol"), ConditionValue: proto.String("w24|Any"), Action: proto.String("ExpandReel"), ActionValue: proto.String("w24")},
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
