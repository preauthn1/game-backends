package wlzb

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
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/roomlogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/scorestorage"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
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

	t.Log("SlotPatternID", logic.SlotPatternID)
	t.Log("BetCounts", logic.BetCounts)
	t.Log("BetRate", logic.BetRates)
	t.Log("MaxBetCount", logic.MaxBetCount)
	t.Log("MaxBetRate", logic.MaxBetRate)
	t.Log("BigWinRule", logic.BigWinRule)
	t.Log("BigWinNotify", logic.BigWinNotify)
	t.Log("EndActionTime", logic.EndActionTime)
	t.Log("MinCarryMoney", logic.MinCarryMoney)
	t.Log("SymbolsDesc", logic.SymbolsDesc)
	t.Log("PayLineDesc", logic.PayLineDesc)

	t.Log("DefaultReelNum", logic.DefaultReelNum)
	t.Log("DefaultRowNum", logic.DefaultRowNum)

	t.Log("PayLineMode", logic.PayLineMode)
	// t.Log("PaylineMatcher", logic.PaylineMatcher)

	t.Log("Jackpots", logic.Jackpots)

	t.Log("=== SlotPattern ===")
	t.Log(logic.SlotPattern)

	t.Log("=== MainGameTable ===", len(logic.MainGameTable))
	for _, row := range logic.MainGameTable {
		t.Log(row)
	}

	t.Log("=== SlotPayLine ===", len(logic.SlotPayLine))
	// for _, row := range logic.SlotPayLine {
	// 	t.Log(row)
	// }

	t.Log("=== SlotPayTable ===", len(logic.SlotPayTable))
	for _, row := range logic.SlotPayTable {
		t.Log(row)
	}

	t.Log("=== SymbolAttributes ===", len(logic.SymbolAttributes))
	for _, row := range logic.SymbolAttributes {
		t.Log(row)
	}

	t.Log("=== RespinTable ===", len(logic.RespinTable))
	// for _, row := range logic.RespinTable {
	// 	t.Log(row)
	// }

	t.Log("=== SlotSymbolDistribution ===", len(logic.SlotSymbolDistribution))
	// for _, row := range logic.SlotSymbolDistribution {
	// 	t.Log(row)
	// }

	t.Log("=== JackpotTable ===", len(logic.JackpotTable))
	// for _, row := range logic.JackpotTable {
	// 	t.Log(row)
	// }

	t.Log("=== PickGameTable ===", len(logic.PickGameTable))
	// for _, row := range logic.PickGameTable {
	// 	t.Log(row)
	// }

	t.Log("=== BonusTable ===", len(logic.BonusTable))
	// for _, row := range logic.BonusTable {
	// 	t.Log(row)
	// }

	t.Log("=== FreeSpinTable ===", len(logic.FreeSpinTable))
	for _, row := range logic.FreeSpinTable {
		t.Log(row)
	}

}

func TestLogic_UserEnter(t *testing.T) {
	conf.CLogLevel = 3
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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		slot_test.PrintSendMessgeFunc(t, userID, m)

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		handlers1 := make(map[int32]func())
		handlers2 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_FreeGameRecord)
			example.invalid = new(netproto.Slot_FreeSpinDetail)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.UserBet = proto.Int64(10)
			example.invalid.FreeSpinGroup = proto.Int32(502)
			example.invalid.Times = proto.Int32(2)
			example.invalid.ChooseRate = proto.Int32(1)
			example.invalid.WinMoney = proto.Int64(300)
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

}

func TestLogic_Spin_NoWin(t *testing.T) {
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
	user.UserState.OnUserQueue(user, grs.Room)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		slot_test.PrintSendMessgeFunc(t, userID, m)

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

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
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	time.Sleep(100 * time.Millisecond)

	t.Run("NoWin Case1", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_Spin_NormalWin(t *testing.T) {
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
	user.UserState.OnUserQueue(user, grs.Room)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		slot_test.PrintSendMessgeFunc(t, userID, m, int32(netproto.Slot_GameMessageClassID_Slot_SettingID))

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		handlers1 := make(map[int32]func())
		handlers2 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_FreeGameRecord)
			example.invalid = new(netproto.Slot_FreeSpinDetail)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.UserBet = proto.Int64(10)
			example.invalid.FreeSpinGroup = proto.Int32(502)
			example.invalid.Times = proto.Int32(2)
			example.invalid.ChooseRate = proto.Int32(1)
			example.invalid.WinMoney = proto.Int64(300)
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	t.Run("One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Two Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})
}

func TestLogic_Spin_FreeGameWin(t *testing.T) {
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
	user.UserState.OnUserQueue(user, grs.Room)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		slot_test.PrintSendMessgeFunc(t, userID, m, int32(netproto.Slot_GameMessageClassID_Slot_SettingID))

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		handlers1 := make(map[int32]func())
		handlers2 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
			detail := new(netproto.Slot_MSG_FreeGameRecord)
			example.invalid = new(netproto.Slot_FreeSpinDetail)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.UserBet = proto.Int64(10)
			example.invalid.FreeSpinGroup = proto.Int32(int32(logic.FreeSpinTable[0].GetId()))
			example.invalid.Times = proto.Int32(10)
			example.invalid.ChooseRate = proto.Int32(1)
			example.invalid.WinMoney = proto.Int64(0)
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	time.Sleep(100 * time.Millisecond)

	t.Run("One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}

			logic.FreeSpin = slot.NewFreeSpin(logic.FreeSpinTable[0], logic.SlotSymbolDistribution, 10, 1, false)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

}

func TestLogic_Spin_LastFreeGame(t *testing.T) {
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
	user.UserState.OnUserQueue(user, grs.Room)

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		handlers1 := make(map[int32]func())
		handlers2 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
			detail := new(netproto.Slot_MSG_FreeGameRecord)
			example.invalid = new(netproto.Slot_FreeSpinDetail)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.UserBet = proto.Int64(10)
			example.invalid.FreeSpinGroup = proto.Int32(int32(logic.FreeSpinTable[0].GetId()))
			example.invalid.Times = proto.Int32(2)
			example.invalid.ChooseRate = proto.Int32(1)
			example.invalid.WinMoney = proto.Int64(0)
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	logic.FreeSpin = slot.NewFreeSpin(logic.FreeSpinTable[0], logic.SlotSymbolDistribution, 10, 1, false)
	logic.FreeSpin.Count = 2

	time.Sleep(100 * time.Millisecond)

	t.Run("FreeGame One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("FreeGame One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("NormalGame Two Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

}

func TestLogic_Spin_WinSelectChance(t *testing.T) {
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
	user.UserState.OnUserQueue(user, grs.Room)

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

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
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	time.Sleep(100 * time.Millisecond)

	t.Run("Win", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("User Choose", func(tt *testing.T) {

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			slot_test.PrintSendMessgeFunc(tt, userID, m)

			printSendMessgeFunc(tt, userID, m)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

			printSendRPCMessgeFunc(tt, safe, m, nil)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.WLZB_GameMessageClassID_WLZB_ChooseGameID)

		m := new(netproto.WLZB_MSG_ChooseGame)
		m.ID = proto.Int32(9)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("FreeGame One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Again", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Again", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Win Again", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	logic.FreeSpin.Count = 1

	t.Run("FreeGame Last Time Two Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

}

func TestLogic_UserAddMoney(t *testing.T) {
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
	user.TimeSeq = 20190909225900000
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)

	conn := &network.ConnInfo{}
	p := new(player.Player)
	p.UserID = user.UserID
	p.Conn = conn
	p.UserData = user
	grs.PlayerManager.SetPlayer(p)

	grs.NotifyMsgHandler.AddHandler(roomlogic.NewNotifyPayOrderHandler(grs.Room.GameInfo.GameID, grs.Room.ServerID, grs.Room.RoomUserManager, new(scorestorage.ConsoleUserScoreStorage)))

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

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
			example.invalid.IsCoupon = proto.Bool(false)

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		handlers2[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {

			detail := new(netproto.Slot_MSG_SelectGameRecord)
			example.invalid = new(netproto.Slot_SelectGameInfo)
			example.invalid.UserID = proto.Int32(user.UserID)
			example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
			f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
			example.invalid.FlagID = proto.Int32(int32(f))
			example.invalid.OriginID = proto.Int32(0)
			example.invalid.Times = proto.Int32(0)
			example.invalid.Data = proto.String("")

			b, _ := proto.Marshal(detail)

			_m := new(rpcmsg.RPCMessage)
			_m.MsgData = b

			mm := new(msg.Message)
			mm.BClassID = int32(netproto.MessageBClassID_Game)
			mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
			mm.MsgData = _m

			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)

		printSendRPCMessgeFunc(t, safe, m, handlers2)
	}

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	t.Run("One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("Add Money", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)

				printSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				slot_test.PrintSendRPCMessgeFunc(tt, safe, m, nil)

				printSendRPCMessgeFunc(tt, safe, m, nil)
			}

		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_NotifyServer)
		_m.SClassID = int32(netproto.NotifyServerClassID_PayOrderID)

		m := new(netproto.PayOrderList)
		m.ServerID = proto.Int32(logic.TableManager.Room.ServerID)
		m.PayOrderList = []*netproto.PayOrderInfo{
			&netproto.PayOrderInfo{
				UserID:  proto.Int32(user.UserID),
				Amount:  proto.Int64(1000),
				TimeSeq: proto.Int64(20190909225959000),
			},
		}
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		grs.NotifyMsgHandler.OnNotifyMessage(_m, grs.ModuleCtx, grs.PlayerManager)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

	t.Run("One Symbol", func(tt *testing.T) {
		{
			logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
				slot_test.PrintSendMessgeFunc(tt, userID, m)
			}

			logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
				t.Log("SendRPCMessageFunc", m, m.MsgData)
			}

			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			}
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(100 * time.Millisecond)
	})

}

func printSendMessgeFunc(t *testing.T, userID int32, m *msg.Message) {

	if m.SClassID == int32(netproto.WLZB_GameMessageClassID_WLZB_GameOptionsID) {
		t.Log("[GameOptionsID]")
		t.Log(" - Times:", m.MsgData.(*netproto.WLZB_MSG_ChooseGameOptions).GetTimes())
	}

}

func printSendRPCMessgeFunc(t *testing.T, safe bool, m *msg.Message, handlers map[int32]func()) {

	if handlers != nil {
		if f, ok := handlers[m.SClassID]; ok {
			f()
		}
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameRecordID) {
		t.Log("[SelectGameRecordID]")

		data := m.MsgData.(*netproto.Slot_MSG_SelectGameRecord)

		t.Log(" - GameID:", example.invalid.GetGameID())
		t.Log(" - FlagID:", example.invalid.GetFlagID())
		t.Log(" - UserID:", example.invalid.GetUserID())
		t.Log(" - OriginID:", example.invalid.GetOriginID())
		t.Log(" - Times:", example.invalid.GetTimes())
		t.Log(" - Data:", example.invalid.GetData())

		return
	}

}
