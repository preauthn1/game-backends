package dfdc

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
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestProto(t *testing.T) {
	protodatas.Print()
}

func TestNewLogic(t *testing.T) {

	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	fmt.Println("SlotPatternID", logic.SlotPatternID)
	fmt.Println("BetCounts", logic.BetCounts)
	fmt.Println("BetRates", logic.BetRates)
	fmt.Println("MaxBetCount", logic.MaxBetCount)
	fmt.Println("MaxBetRate", logic.MaxBetRate)
	fmt.Println("BigWinRule", logic.BigWinRule)
	fmt.Println("BigWinNotify", logic.BigWinNotify)
	fmt.Println("EndActionTime", logic.EndActionTime)
	fmt.Println("MinCarryMoney", logic.MinCarryMoney)
	fmt.Println("SymbolsDesc", logic.SymbolsDesc)
	fmt.Println("PayLineDesc", logic.PayLineDesc)

	fmt.Println("DefaultReelNum", logic.DefaultReelNum)
	fmt.Println("DefaultRowNum", logic.DefaultRowNum)
	fmt.Println("DefaultSymbolGroupID", logic.DefaultSymbolGroupID)

	fmt.Println("PayLineMode", logic.PayLineMode)
	// fmt.Println("PaylineMatcher", logic.PaylineMatcher)

	fmt.Println("Jackpots", logic.Jackpots)

	fmt.Println("=== SlotPattern ===")
	fmt.Println(logic.SlotPattern)

	fmt.Println("=== MainGameTable ===", len(logic.MainGameTable))
	for _, row := range logic.MainGameTable {
		fmt.Println(row)
	}

	fmt.Println("=== SlotPayLine ===", len(logic.SlotPayLine))
	for _, row := range logic.SlotPayLine {
		fmt.Println(row)
	}

	fmt.Println("=== SlotPayTable ===", len(logic.SlotPayTable))
	for _, row := range logic.SlotPayTable {
		fmt.Println(row)
	}

	fmt.Println("=== SymbolAttributes ===", len(logic.SymbolAttributes))
	for _, row := range logic.SymbolAttributes {
		fmt.Println(row)
	}

	fmt.Println("=== RespinTable ===", len(logic.RespinTable))
	for _, row := range logic.RespinTable {
		fmt.Println(row)
	}

	fmt.Println("=== SlotSymbolDistribution ===", len(logic.SlotSymbolDistribution))
	for _, row := range logic.SlotSymbolDistribution {
		fmt.Println(row)
	}

	fmt.Println("=== JackpotTable ===", len(logic.JackpotTable))
	for _, row := range logic.JackpotTable {
		fmt.Println(row)
	}

	fmt.Println("=== PickGameTable ===", len(logic.PickGameTable))
	for _, row := range logic.PickGameTable {
		fmt.Println(row)
	}

	fmt.Println("=== BonusTable ===", len(logic.BonusTable))
	for _, row := range logic.BonusTable {
		fmt.Println(row)
	}

	fmt.Println("=== FreeSpinTable ===", len(logic.FreeSpinTable))
	for _, row := range logic.FreeSpinTable {
		fmt.Println(row)
	}

}

func TestLogic_UserEnter_UserLeave(t *testing.T) {

	conf.CLogLevel = 3
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)

	t.Run("UserEnter", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println(userID, m, m.MsgData)
		}

		logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)

	})

	time.Sleep(1500 * time.Millisecond)

	t.Run("UserLeave", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println(userID, m, m.MsgData)
		}

		logic.UserLeave(user)

	})
}

func TestLogic_GameMessage_Spin_WinFreeSpin(t *testing.T) {

	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(11), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},

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

	t.Run("1'th Spin, Get FreeSpin", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

	})

	t.Run("2'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("3'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("4'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("5'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("6'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("7'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("8'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("9'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("10'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("11'th Spin, In FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("12'th Spin, With out FreeGame", func(tt *testing.T) {

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
		m.Money = proto.Int64(10)
		m.GoldenSymbol = proto.Int32(8)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		t.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

}

func TestLogic_GameMessage_Spin_NoWin(t *testing.T) {
	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

func TestLogic_GameMessage_Spin_WinNormal(t *testing.T) {
	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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
