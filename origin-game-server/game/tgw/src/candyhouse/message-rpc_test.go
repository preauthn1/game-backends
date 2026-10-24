package candyhouse

import (
	"gamelogic"
	"msg"
	"netproto"
	"reflect"
	"rpcmsg"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
)

func TestLogic_recvBonusData(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.BonusTable = []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("2"), PerProb: proto.Int32(1000000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("3"), PerProb: proto.Int32(500000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("4"), PerProb: proto.Int32(250000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("5"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("6"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("7"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("8"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("9"), PerProb: proto.Int32(500)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100)},
	}
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738}

	data := &netproto.TGW_MSG_BonusGameRecord{
		UserID: proto.Int32(9936738),
		Records: []*netproto.TGW_BonusGameRecord{
			&netproto.TGW_BonusGameRecord{ID: proto.Int32(1), Location: proto.Int32(4), RewareValue: proto.Int64(10), BonusID: proto.Int32(1)},
			&netproto.TGW_BonusGameRecord{ID: proto.Int32(2), Location: proto.Int32(0), RewareValue: proto.Int64(10), BonusID: proto.Int32(3)},
			&netproto.TGW_BonusGameRecord{ID: proto.Int32(3), Location: proto.Int32(0), RewareValue: proto.Int64(10), BonusID: proto.Int32(1)},
			&netproto.TGW_BonusGameRecord{ID: proto.Int32(4), Location: proto.Int32(0), RewareValue: proto.Int64(10), BonusID: proto.Int32(2)},
		},
	}

	b, _ := proto.Marshal(data)

	rpcMsg := new(rpcmsg.RPCMessage)
	rpcMsg.MsgData = b

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameRetID),
		MsgData:  rpcMsg,
	}

	logic.recvBonusData(m)

	if logic.BonusGame.GameBetMoney != 10 {
		t.Errorf("GameBetMoney want %d, but %d", 10, logic.BonusGame.GameBetMoney)
	}
	if logic.BonusGame.GameCount != 4 {
		t.Errorf("GameCount want %d, but %d", 4, logic.BonusGame.GameCount)
	}
	if logic.BonusGame.WinMoney != 110 {
		t.Errorf("WinMoney want %d, but %d", 110, logic.BonusGame.WinMoney)
	}
	if len(logic.BonusGame.Process) != 4 {
		t.Errorf("Process length want %d, but %d", 4, len(logic.BonusGame.Process))
	}

	if logic.BonusGame.Process[1].ID != 1 {
		t.Errorf("Process[1].ID want %d, but %d", 1, logic.BonusGame.Process[1].ID)
	}
	if logic.BonusGame.Process[1].Location != 4 {
		t.Errorf("Process[1].Location want %d, but %d", 4, logic.BonusGame.Process[1].Location)
	}
	if logic.BonusGame.Process[1].Money != 20 {
		t.Errorf("Process[1].Money want %d, but %d", 20, logic.BonusGame.Process[1].Money)
	}

	if logic.BonusGame.Process[2].ID != 2 {
		t.Errorf("Process[2].ID want %d, but %d", 2, logic.BonusGame.Process[2].ID)
	}
	if logic.BonusGame.Process[2].Location != 0 {
		t.Errorf("Process[2].Location want %d, but %d", 0, logic.BonusGame.Process[2].Location)
	}
	if logic.BonusGame.Process[2].Money != 40 {
		t.Errorf("Process[2].Money want %d, but %d", 40, logic.BonusGame.Process[2].Money)
	}

	if logic.BonusGame.Process[3].ID != 3 {
		t.Errorf("Process[3].ID want %d, but %d", 3, logic.BonusGame.Process[3].ID)
	}
	if logic.BonusGame.Process[3].Location != 0 {
		t.Errorf("Process[3].Location want %d, but %d", 0, logic.BonusGame.Process[3].Location)
	}
	if logic.BonusGame.Process[3].Money != 20 {
		t.Errorf("Process[3].Money want %d, but %d", 20, logic.BonusGame.Process[3].Money)
	}

	if logic.BonusGame.Process[4].ID != 4 {
		t.Errorf("Process[4].ID want %d, but %d", 4, logic.BonusGame.Process[4].ID)
	}
	if logic.BonusGame.Process[4].Location != 0 {
		t.Errorf("Process[4].Location want %d, but %d", 0, logic.BonusGame.Process[4].Location)
	}
	if logic.BonusGame.Process[4].Money != 30 {
		t.Errorf("Process[4].Money want %d, but %d", 30, logic.BonusGame.Process[4].Money)
	}

}

func TestLogic_recvBonusData_none_data(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.BonusTable = []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("2"), PerProb: proto.Int32(1000000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("3"), PerProb: proto.Int32(500000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("4"), PerProb: proto.Int32(250000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("5"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("6"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("7"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("8"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("9"), PerProb: proto.Int32(500)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100)},
	}
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738}

	data := &netproto.TGW_MSG_BonusGameRecord{
		UserID:  proto.Int32(9936738),
		Records: []*netproto.TGW_BonusGameRecord{},
	}

	b, _ := proto.Marshal(data)

	rpcMsg := new(rpcmsg.RPCMessage)
	rpcMsg.MsgData = b

	m := &msg.Message{
		BClassID: int32(netproto.MessageBClassID_Game),
		SClassID: int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameRetID),
		MsgData:  rpcMsg,
	}

	logic.recvBonusData(m)

	if logic.BonusGame != nil {
		t.Errorf("User has no BonusGame Record, should not have BonusGame")
	}
}

func TestLogic_ResetBonusGame(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID) {
			t.Errorf("SClassID want %d, but %d", netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.TGW_MSG_ResetBonusGame)
		if !ok {
			t.Errorf("MsgData want type *netproto.TGW_MSG_ResetBonusGame")
		}

		if !reflect.DeepEqual(data.GetUserID(), logic.GameUser.UserID) {
			t.Errorf("UserID want %v, but, %v", logic.GameUser.UserID, data.GetUserID())
		}

	}

	logic.ResetBonusGame(logic.GameUser)
}

func TestLogic_RecordBonusGame(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID) {
			t.Errorf("SClassID want %d, but %d", netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.TGW_MSG_RecordBonusGame)
		if !ok {
			t.Errorf("MsgData want type *netproto.TGW_MSG_RecordBonusGame")
		}

		if !reflect.DeepEqual(data.GetUserID(), logic.GameUser.UserID) {
			t.Errorf("UserID want %v, but, %v", logic.GameUser.UserID, data.GetUserID())
		}

		if data.GetRecords() == nil {
			t.Errorf("Records should not be nil")
		}

		if data.GetRecords().GetID() != 1 {
			t.Errorf("Records.ID want %v, but, %v", 1, data.GetRecords().GetID())
		}

		if data.GetRecords().GetLocation() != 0 {
			t.Errorf("Records.Location want %v, but, %v", 0, data.GetRecords().GetLocation())
		}

		if data.GetRecords().GetRewareValue() != 20 {
			t.Errorf("Records.RewareValue want %v, but, %v", 20, data.GetRecords().GetRewareValue())
		}

		if data.GetRecords().GetBonusID() != 1 {
			t.Errorf("Records.BonuseID want %v, but, %v", 1, data.GetRecords().GetBonusID())
		}

	}

	logic.RecordBonusGame(logic.GameUser, 1, 1, 0, 20)
}

func TestLogic_ReadBonusGame(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID) {
			t.Errorf("SClassID want %d, but %d", netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.TGW_MSG_ReadBonusGame)
		if !ok {
			t.Errorf("MsgData want type *netproto.TGW_MSG_ReadBonusGame")
		}

		if !reflect.DeepEqual(data.GetUserID(), logic.GameUser.UserID) {
			t.Errorf("UserID want %v, but, %v", logic.GameUser.UserID, data.GetUserID())
		}

	}

	logic.ReadBonusGame(logic.GameUser)
}

func TestLogic_AddSlotGameHistoryLog(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738, Score: 500, Coupon: 0}
	logic.GamePayMoney = 10
	logic.GameWinMoney = 100
	logic.TableManager = &gamelogic.Table{
		Room: &gamelogic.Room{
			GameInfo: &gamelogic.Game{
				GameID: 26,
			},
			ServerID: 372,
		},
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.GameMessageClassID_SlotAddGameLogID) {
			t.Errorf("SClassID want %d, but %d", netproto.GameMessageClassID_SlotAddGameLogID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.Slot_HistoryLogData)
		if !ok {
			t.Errorf("MsgData want type *netproto.Slot_HistoryLogData")
		}

		if data.GetUserID() != logic.GameUser.UserID {
			t.Errorf("UserID want %v, but, %v", logic.GameUser.UserID, data.GetUserID())
		}

		if data.GetGameID() != 26 {
			t.Errorf("GameID want %v, but, %v", 26, data.GetGameID())
		}

		if data.GetServerID() != 372 {
			t.Errorf("ServerID want %v, but, %v", 372, data.GetServerID())
		}

		if data.GetCurrentAmount() != 500 {
			t.Errorf("CurrentAmount want %v, but, %v", 500, data.GetCurrentAmount())
		}

		if data.GetCurrentCoupon() != 0 {
			t.Errorf("CurrentCoupon want %v, but, %v", 0, data.GetCurrentCoupon())
		}

		if data.GetBetMoney() != 10 {
			t.Errorf("BetMoney want %v, but, %v", 10, data.GetBetMoney())
		}

		if data.GetBetCoupon() != 0 {
			t.Errorf("BetCoupon want %v, but, %v", 0, data.GetBetCoupon())
		}

		if data.GetWinMoney() != 100 {
			t.Errorf("WinMoney want %v, but, %v", 100, data.GetWinMoney())
		}

		if data.GetFinalScore() != 590 {
			t.Errorf("FinalScore want %v, but, %v", 590, data.GetFinalScore())
		}

		if data.GetFinalCoupon() != 0 {
			t.Errorf("FinalCoupon want %v, but, %v", 0, data.GetFinalCoupon())
		}

		if data.GetGameType() != slot.HistoryTypeBONUS {
			t.Errorf("GameType want %v, but, %v", slot.HistoryTypeBONUS, data.GetGameType())
		}

		if data.GetLogType() != slot.GameTypeOPEN {
			t.Errorf("LogType want %v, but, %v", slot.GameTypeOPEN, data.GetLogType())
		}

		if data.GetGameData() != "TestABC" {
			t.Errorf("GameData want %v, but, %v", "TestABC", data.GetGameData())
		}
	}

	logic.AddSlotGameHistoryLog(logic.GameUser, slot.HistoryTypeBONUS, slot.GameTypeOPEN, "TestABC")
}

func TestLogic_ReadSlotGameHistoryLog(t *testing.T) {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.GameUser = &gamelogic.GameUser{UserID: 9936738, Score: 500, Coupon: 0}
	logic.GamePayMoney = 10
	logic.GameWinMoney = 100
	logic.TableManager = &gamelogic.Table{
		Room: &gamelogic.Room{
			GameInfo: &gamelogic.Game{
				GameID: 26,
			},
			ServerID: 372,
		},
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {

		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.GameMessageClassID_SlotGetGameLogID) {
			t.Errorf("SClassID want %d, but %d", netproto.GameMessageClassID_SlotGetGameLogID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.Slot_LoadHistoryData)
		if !ok {
			t.Errorf("MsgData want type *netproto.Slot_LoadHistoryData")
		}

		if data.GetUserID() != logic.GameUser.UserID {
			t.Errorf("UserID want %v, but, %v", logic.GameUser.UserID, data.GetUserID())
		}

		if data.GetGameID() != 26 {
			t.Errorf("GameID want %v, but, %v", 26, data.GetGameID())
		}

		if data.GetStartTime() != "2019-07-01" {
			t.Errorf("StartTime want %v, but, %v", "2019-07-01", data.GetStartTime())
		}

		if data.GetEndTime() != "2019-07-31" {
			t.Errorf("EndTime want %v, but, %v", "2019-07-31", data.GetEndTime())
		}

		if data.GetPageIndex() != 1 {
			t.Errorf("PageIndex want %v, but, %v", 1, data.GetPageIndex())
		}

		if data.GetPageSize() != 10 {
			t.Errorf("PageSize want %v, but, %v", 10, data.GetPageSize())
		}

	}

	logic.ReadSlotGameHistoryLog(logic.GameUser, 1, 10, "2019-07-01", "2019-07-31")
}
