package jjbx

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

func TestNewLogic(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	slot_test.PrintLogic(t, logic.SlotLogicBase)
}

func TestLogic_UserEnter(t *testing.T) {

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

func TestLogic_SelectGame_Rate8_15JackpotGrand(t *testing.T) {

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

	t.Run("Win", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)
			printSendMessgeFunc(tt, userID, m)
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
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("g3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(701), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
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

func TestLogic_SelectGame_Rate8_Win(t *testing.T) {

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
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(721), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
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
		// example.invalid.IsCoupon = proto.Bool(false)
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

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

		detail := new(netproto.Slot_MSG_SelectGameRecord)
		example.invalid = new(netproto.Slot_SelectGameInfo)
		example.invalid.UserID = proto.Int32(user.UserID)
		example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
		example.invalid.FlagID = proto.Int32(int32(f))
		example.invalid.Times = proto.Int32(0)
		// example.invalid.OriginID = proto.Int32(327)
		// example.invalid.Data = proto.String(`{"GroupID":702,"BetMoney":5,"BetRate":88,"GameResultManager":{"GameResult":[[{"id":6954,"groupId":705,"reelNum":1,"symbol":"p9","proportion":1,"order":1},{"id":6988,"groupId":705,"reelNum":1,"symbol":"g3","proportion":1,"order":2},{"id":7012,"groupId":705,"reelNum":1,"symbol":"g2","proportion":1,"order":3}],[{"id":7026,"groupId":705,"reelNum":2,"symbol":"p6","proportion":1,"order":1},{"id":7063,"groupId":705,"reelNum":2,"symbol":"g3","proportion":1,"order":2},{"id":7086,"groupId":705,"reelNum":2,"symbol":"g1","proportion":1,"order":3}],[{"id":7115,"groupId":705,"reelNum":3,"symbol":"g5","proportion":1,"order":1},{"id":7137,"groupId":705,"reelNum":3,"symbol":"g2","proportion":1,"order":2},{"id":7165,"groupId":705,"reelNum":3,"symbol":"g5","proportion":1,"order":3}],[{"id":7189,"groupId":705,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"id":7215,"groupId":705,"reelNum":4,"symbol":"g5","proportion":1,"order":2},{"id":7241,"groupId":705,"reelNum":4,"symbol":"g6","proportion":1,"order":3}],[{"id":7253,"groupId":705,"reelNum":5,"symbol":"p8","proportion":1,"order":1},{"id":7271,"groupId":705,"reelNum":5,"symbol":"p1","proportion":1,"order":2},{"id":7296,"groupId":705,"reelNum":5,"symbol":"p1","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":705},"Data":{"GMoney":6700}}`)

		b, _ := proto.Marshal(detail)

		_m := new(rpcmsg.RPCMessage)
		_m.MsgData = b

		mm := new(msg.Message)
		mm.BClassID = int32(netproto.MessageBClassID_Game)
		mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
		mm.MsgData = _m

		logic.OnRecvRPCMessage(mm)
	}

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {

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
