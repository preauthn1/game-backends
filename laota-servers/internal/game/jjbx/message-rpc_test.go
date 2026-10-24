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
