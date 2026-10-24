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
)

func TestRecevSelectGame(t *testing.T) {

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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	detail := new(netproto.Slot_MSG_SelectGameRecord)
	example.invalid = new(netproto.Slot_SelectGameInfo)
	example.invalid.UserID = proto.Int32(user.UserID)
	example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
	example.invalid.FlagID = proto.Int32(int32(f))
	example.invalid.OriginID = proto.Int32(336)
	example.invalid.Times = proto.Int32(1)
	example.invalid.Data = proto.String(`{"GroupID":702,"BetMoney":5,"BetRate":88,"GameResultManager":{"GameResult":[[{"id":6954,"groupId":705,"reelNum":1,"symbol":"p9","proportion":1,"order":1},{"id":6988,"groupId":705,"reelNum":1,"symbol":"g3","proportion":1,"order":2},{"id":7012,"groupId":705,"reelNum":1,"symbol":"g2","proportion":1,"order":3}],[{"id":7026,"groupId":705,"reelNum":2,"symbol":"p6","proportion":1,"order":1},{"id":7063,"groupId":705,"reelNum":2,"symbol":"g3","proportion":1,"order":2},{"id":7086,"groupId":705,"reelNum":2,"symbol":"g1","proportion":1,"order":3}],[{"id":7115,"groupId":705,"reelNum":3,"symbol":"g5","proportion":1,"order":1},{"id":7137,"groupId":705,"reelNum":3,"symbol":"g2","proportion":1,"order":2},{"id":7165,"groupId":705,"reelNum":3,"symbol":"g5","proportion":1,"order":3}],[{"id":7189,"groupId":705,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"id":7215,"groupId":705,"reelNum":4,"symbol":"g5","proportion":1,"order":2},{"id":7241,"groupId":705,"reelNum":4,"symbol":"g6","proportion":1,"order":3}],[{"id":7253,"groupId":705,"reelNum":5,"symbol":"p8","proportion":1,"order":1},{"id":7271,"groupId":705,"reelNum":5,"symbol":"p1","proportion":1,"order":2},{"id":7296,"groupId":705,"reelNum":5,"symbol":"p1","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":705},"Data":{"GMoney":6700}}`)

	b, _ := proto.Marshal(detail)

	_m := new(rpcmsg.RPCMessage)
	_m.MsgData = b

	mm := new(msg.Message)
	mm.BClassID = int32(netproto.MessageBClassID_Game)
	mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID)
	mm.MsgData = _m

	logic.recvSelectGameTimes(mm)

	if len(logic.SelectChances) != 1 {
		t.Error("SelectChance should has one time")
	}

	t.Logf("%+v\n", logic.SelectChances[0])
}

func TestRecevLightning(t *testing.T) {

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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	detail := new(netproto.Slot_MSG_LightningRecord)
	detail.UserID = proto.Int32(user.UserID)
	detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)
	detail.FlagID = proto.Int32(int32(f))
	detail.Data = proto.String(`{"steps":[{"respin_id":708,"rule_id":336,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":701,"reelNum":2,"symbol":"p2","proportion":1,"order":2},{"groupId":701,"reelNum":3,"symbol":"p3","proportion":1,"order":2},{"groupId":701,"reelNum":4,"symbol":"p4","proportion":1,"order":2},{"groupId":701,"reelNum":5,"symbol":"p5","proportion":1,"order":2}],[{"groupId":701,"reelNum":1,"symbol":"p1","proportion":1,"order":3},{"groupId":701,"reelNum":2,"symbol":"p2","proportion":1,"order":3},{"groupId":701,"reelNum":3,"symbol":"p3","proportion":1,"order":3},{"groupId":701,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":701,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":701},"last_chance":5,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":0,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"p3","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"p1","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"p2","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"p3","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":4,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":0,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"p1","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"p2","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"p3","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":3,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":0,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"p1","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"p2","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"p3","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":2,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":0,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"g8","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"p1","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"p2","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"p3","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":1,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":0,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"g8","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"g8","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"p2","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"p3","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":0,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":367,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"g8","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"g8","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"b1","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"p4","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":0,"awards":null,"win_money":0,"log":null},{"respin_id":708,"rule_id":367,"game_result":{"GameResult":[[{"groupId":701,"reelNum":1,"symbol":"g1","proportion":1,"order":1},{"groupId":701,"reelNum":2,"symbol":"g2","proportion":1,"order":1},{"groupId":701,"reelNum":3,"symbol":"g3","proportion":1,"order":1},{"groupId":701,"reelNum":4,"symbol":"g4","proportion":1,"order":1},{"groupId":701,"reelNum":5,"symbol":"g5","proportion":1,"order":1}],[{"groupId":701,"reelNum":1,"symbol":"g6","proportion":1,"order":2},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":3,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":4,"symbol":"g7","proportion":1,"order":2},{"groupId":721,"reelNum":5,"symbol":"g8","proportion":1,"order":2}],[{"groupId":721,"reelNum":1,"symbol":"g8","proportion":1,"order":3},{"groupId":721,"reelNum":2,"symbol":"g7","proportion":1,"order":3},{"groupId":721,"reelNum":3,"symbol":"g8","proportion":1,"order":3},{"groupId":721,"reelNum":4,"symbol":"b1","proportion":1,"order":3},{"groupId":721,"reelNum":5,"symbol":"p5","proportion":1,"order":3}]],"Reel":5,"Row":3,"SymbolGroupID":721},"last_chance":1,"awards":null,"win_money":0,"log":null}],"origin_id":708,"bet_money":10,"bet_rate":8,"show_step":7,"is_free_game":false,"symbol_group_id":721,"is_limit":true,"limit_symbol_group_id":726,"limit_point":15}`)

	b, _ := proto.Marshal(detail)

	_m := new(rpcmsg.RPCMessage)
	_m.MsgData = b

	mm := new(msg.Message)
	mm.BClassID = int32(netproto.MessageBClassID_Game)
	mm.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadRetID)
	mm.MsgData = _m

	logic.recvLightningData(mm)

	if logic.LightningGame == nil {
		t.Error("Lightning should be built")
	}

	t.Logf("%+v\n", logic.LightningGame)
}

func TestRecevJackpotGame(t *testing.T) {

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

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)

	f, _ := strconv.ParseFloat(logic.TableManager.Room.Flag, 64)

	record := &netproto.Slot_MSG_BonusGameRecord{
		Detail: []*netproto.Slot_BonusGameDetail{
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(1), Location: proto.Int32(5), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Major")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(2), Location: proto.Int32(6), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Mini")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(3), Location: proto.Int32(7), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Grand")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(4), Location: proto.Int32(0), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Minor")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(5), Location: proto.Int32(0), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Grand")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(6), Location: proto.Int32(0), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Minor")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(7), Location: proto.Int32(0), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Major")},
			&netproto.Slot_BonusGameDetail{UserID: proto.Int32(user.UserID), GameID: proto.Int32(logic.TableManager.Room.GameInfo.GameID), FlagID: proto.Int32(int32(f)), OriginID: proto.Int32(324), TurnID: proto.Int32(8), Location: proto.Int32(0), BetMoney: proto.Int32(100), BetRate: proto.Int32(88), Data: proto.String("Minor")},
		},
	}

	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:1 Location:0 BetMoney:100 BetRate:88 Data:"Major"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:2 Location:0 BetMoney:100 BetRate:88 Data:"Mini"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:3 Location:0 BetMoney:100 BetRate:88 Data:"Grand"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:4 Location:0 BetMoney:100 BetRate:88 Data:"Minor"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:5 Location:0 BetMoney:100 BetRate:88 Data:"Grand"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:6 Location:0 BetMoney:100 BetRate:88 Data:"Minor"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:7 Location:0 BetMoney:100 BetRate:88 Data:"Major"
	//  - Detail - UserID:9000001 GameID:1004 FlagID:5 OriginID:324 TurnID:8 Location:0 BetMoney:100 BetRate:88 Data:"Minor"

	b, _ := proto.Marshal(record)

	_m := new(rpcmsg.RPCMessage)
	_m.MsgData = b

	mm := new(msg.Message)
	mm.BClassID = int32(netproto.MessageBClassID_Game)
	mm.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadRetID)
	mm.MsgData = _m

	logic.recvJackpotGame(mm)

	if logic.JackpotGame == nil {
		t.Error("JackpotGame should exist")
	}

	t.Log("JackpotGame")
	t.Log(" - Origin", logic.JackpotGame.Origin)
	t.Log(" - Jackpots", logic.JackpotGame.Jackpots)
	t.Log(" - MaxBetMoney", logic.JackpotGame.MaxBetMoney)
	t.Log(" - MaxBetRate", logic.JackpotGame.MaxBetRate)
	t.Log(" - GameBetMoney", logic.JackpotGame.GameBetMoney)
	t.Log(" - GameBetRate", logic.JackpotGame.GameBetRate)
	t.Log(" - WinJackpot", logic.JackpotGame.WinJackpot)
	t.Log(" - WinMoney", logic.JackpotGame.WinMoney)
	t.Log(" - Process:")
	for turnID, row := range logic.JackpotGame.Process {
		t.Logf("   - Turn: %d,  %+v\n", turnID, row)
	}
}
