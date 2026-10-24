package jjbx

import (
	"fmt"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestLogic_SelectGame_Rate8_Lightning_15JackpotGrand(t *testing.T) {

	conf.CLogLevel = 0
	
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

	t.Run("Win", func(tt *testing.T) {

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
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("g4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("g5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("g6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("g1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("g2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},

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
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
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

		if logic.LightningGame == nil {
			tt.Errorf("Game should gen Lightn Game")
		}
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

func TestLogic_SelectGame_Rate8_ChooseLightning_LimitToEnd(t *testing.T) {

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
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
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
