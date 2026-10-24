package dfdc

import (
	"fmt"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/mock"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestDFDCNewLogic(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	slot_test.PrintLogic(t, logic.SlotLogicBase)
}

func TestDFDCLogic_UserEnter(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		slot_test.PrintSendMessgeFunc(t, userID, m, int32(netproto.DFDC_GameMessageClassID_DFDCSettingID))

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(10 * time.Millisecond)
}

func TestDFDCLogic_GameMessage_Spin_WinFreeSpin(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}
	}

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	time.Sleep(10 * time.Millisecond)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		fmt.Println("SendMessgeFunc", userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		fmt.Println("SendRPCMessageFunc", m)
	}

	t.Run("1'th Spin, Get FreeSpin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

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
	})

	t.Run("2'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("3'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("4'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("5'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("6'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("7'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("8'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("9'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("10'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	t.Run("11'th Spin, In FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})

	if logic.FreeSpin != nil {
		t.Errorf("FreeSpin Should be nil")
	}

	t.Run("12'th Spin, With out FreeGame", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

		time.Sleep(10 * time.Millisecond)
	})
}

func TestDFDCLogic_GameMessage_Spin_NoWin(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}
	}

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	user.Score = 9999999

	t.Run("Spin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(1000)
		m.GoldenSymbol = proto.Int32(88)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

	})
}

func TestDFDCLogic_GameMessage_Spin_WinNormal(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(9), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}
	}

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	user.Score = 9999999

	t.Run("Spin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

		m := new(netproto.DFDC_StartBet)
		m.Money = proto.Int64(1000)
		m.GoldenSymbol = proto.Int32(88)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

	})
}

func printSendMessgeFunc(t *testing.T, userID int32, m *msg.Message) {

	if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCSettingID) {
		data := m.MsgData.(*netproto.DFDC_Setting)

		t.Logf("[Game Setting]\n")

		t.Logf(" - BetCount: %d\n", data.GetBetCount())
		t.Logf(" - RowCounts: %d\n", data.GetRowCounts())
		t.Logf(" - TotalPayLine: %d\n", data.GetTotalPayLine())
		t.Logf(" - BigWinStep: %d\n", data.GetBigWinStep())
		t.Logf(" - PayLineMode: %d\n", data.GetPayLineMode())

		t.Log(" - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			t.Logf("   - %v\n", row)
		}
		t.Log(" - SymbolPayTable:")
		for _, row := range data.GetSymbolPayTable() {
			t.Logf("   - %v\n", row)
		}

		t.Log(" - UnitPrefix:")
		for _, row := range data.GetUnitPrefix() {
			t.Logf("   - %v\n", row)
		}
	}

	if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCStateID) {
		data := m.MsgData.(*netproto.DFDC_GameStatus)

		t.Logf("[Status] State: %d\n", data.GetState())
	}

	if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCGameResult) {
		data := m.MsgData.(*netproto.DFDC_GameResultReel)

		t.Logf("[GameResult]\n")

		for _, row := range data.GetPayAllGroup() {
			t.Log("  - PayAllGroup")
			for _, d := range row.GetPayAllDetail() {
				t.Logf("    - PayAllDetail: %v", d)
			}
		}
	}
}

func printSendRPCMessgeFunc(t *testing.T, safe bool, m *msg.Message, handlers map[int32]func()) {
	if handlers != nil {
		if f, ok := handlers[m.SClassID]; ok {
			f()
		}
	}
}

func userEnter(logic *Logic, user *gamelogic.GameUser, t *testing.T, safe bool, m *msg.Message) {
	handlers1 := make(map[int32]func())

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
		mm := mock.EmptyFreeSpinRPCMessage(logic.SlotLogicBase, user)
		logic.OnRecvRPCMessage(mm)
	}

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
		mm := mock.EmptyJackpotPoolRPCMessage(logic.SlotLogicBase, user)
		logic.OnRecvRPCMessage(mm)
	}

	handlers1[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {
		detail := mock.EmptyBonusGameDetail(logic.SlotLogicBase, user)
		mm := mock.MarshalBonusGame(detail)
		logic.OnRecvRPCMessage(mm)
	}

	slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)
}
