package dfdclogic

import (
	"conf"
	"config"
	"event"
	"fixtures"
	"fmt"
	"gamelogic"
	"msg"
	"netproto"
	"roomserver"
	"rpcmsg"
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot/protodata"
)

func TestJackpot(t *testing.T) {
	conf.CLogLevel = 4
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewDFDCLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServer()
	logic := grs.GameLogicManager.GetGameLogic(0).(*DFDCLogic)
	logic.DFDCConfig.normalSymbolDistTable = []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(19), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
	}
	logic.KickUserFunc = func(userID int32, msg string) {}
	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {}
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameID) {
			data := m.MsgData.(*netproto.DFDC_BonusGame)
			if data.GetMoney() == 0 {
				t.Errorf("出現Jackpot Money = 0")
			}
		}
	}

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)

	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	go func() {
		for {
			data := new(netproto.Slot_JackpotAllData)
			data.AllData = []*netproto.Slot_JackpotData{
				&netproto.Slot_JackpotData{GameID: proto.Int32(25), FlagID: proto.Int32(1), TableNo: proto.Int32(0), Level: proto.Int32(1), Contribution: proto.Int64(0), TotalContribution: proto.Int64(0), AwardTotal: proto.Int64(0)},
				&netproto.Slot_JackpotData{GameID: proto.Int32(25), FlagID: proto.Int32(1), TableNo: proto.Int32(0), Level: proto.Int32(2), Contribution: proto.Int64(0), TotalContribution: proto.Int64(0), AwardTotal: proto.Int64(0)},
				&netproto.Slot_JackpotData{GameID: proto.Int32(25), FlagID: proto.Int32(1), TableNo: proto.Int32(0), Level: proto.Int32(3), Contribution: proto.Int64(0), TotalContribution: proto.Int64(0), AwardTotal: proto.Int64(0)},
				&netproto.Slot_JackpotData{GameID: proto.Int32(25), FlagID: proto.Int32(1), TableNo: proto.Int32(0), Level: proto.Int32(4), Contribution: proto.Int64(0), TotalContribution: proto.Int64(0), AwardTotal: proto.Int64(0)},
			}
			b, _ := proto.Marshal(data)
			m := &msg.Message{
				MsgData: &rpcmsg.RPCMessage{MsgData: b},
			}
			logic.getJackpotData(m)
			time.Sleep(10 * time.Nanosecond)
		}
	}()

	time.Sleep(500 * time.Millisecond)

	for i := 0; i < 100; i++ {
		t.Run(fmt.Sprintf("Time-%d", i+1), func(tt *testing.T) {

			_m := new(msg.Message)
			_m.BClassID = int32(netproto.MessageBClassID_Game)
			_m.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID)

			m := new(netproto.DFDC_StartBet)
			m.Money = proto.Int64(1000)
			m.GoldenSymbol = proto.Int32(88)
			b, _ := proto.Marshal(m)

			_m.MsgData = b

			logic.GameMessage(user, _m)

			time.Sleep(50 * time.Millisecond)
		})
	}

}
