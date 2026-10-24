package dhzw

import (
	"testing"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestDHZWLogic_Background_Win_JP(t *testing.T) {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	c := make(chan bool)
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) {
			data := m.MsgData.(*netproto.Slot_MSG_Status)
			if data.GetState() == 0 {
				c <- true
				close(c)
			}
			return
		}
	}

	user.UserState.OnUserQueue(user, logic.GetTable().Room)
	go logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	<-c

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		// slot_test.PrintSendMessgeFunc(tt, userID, m)
	}

	t.Run("Win", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
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

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame == nil {
			tt.Errorf("Jackpot Game should not be nil")
		}
	})
}

func TestDHZWLogic_Background_Win_Bonus(t *testing.T) {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	c := make(chan bool)
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) {
			data := m.MsgData.(*netproto.Slot_MSG_Status)
			if data.GetState() == 0 {
				c <- true
				close(c)
			}
			return
		}
	}

	user.UserState.OnUserQueue(user, logic.GetTable().Room)
	go logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	<-c

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		// slot_test.PrintSendMessgeFunc(tt, userID, m)
	}

	logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
	}

	t.Run("Win Left", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
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

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame != nil {
			tt.Errorf("Jackpot Game should be nil")
		}

		logic.JackpotGame = nil
	})

	t.Run("Win Right", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
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

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame != nil {
			tt.Errorf("Jackpot Game should be nil")
		}

		logic.JackpotGame = nil
	})

	t.Run("Win Adjacent", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("adjacent"), Symbol: proto.String("bg4"), PayType: proto.String("chip"), Count: proto.Int32(3), Payout: proto.String("100"), UseType: proto.String("all")},
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

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame != nil {
			tt.Errorf("Jackpot Game should be nil")
		}

		logic.JackpotGame = nil
	})
}

func TestDHZWLogic_Background_Spin(t *testing.T) {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	c := make(chan bool)
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) {
			data := m.MsgData.(*netproto.Slot_MSG_Status)
			if data.GetState() == 0 {
				c <- true
				close(c)
			}
			return
		}
	}

	user.UserState.OnUserQueue(user, logic.GetTable().Room)
	go logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	<-c

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		// slot_test.PrintSendMessgeFunc(tt, userID, m)
	}

	t.Run("Win", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		}

		logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
			&protodata.ReelBgDistribution{GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("left"), Symbol: proto.String("p1"), PayType: proto.String("chip"), Count: proto.Int32(4), Payout: proto.String("500"), UseType: proto.String("all")},
			&protodata.SlotPayTable{GroupId: proto.Int32(8), LinkType: proto.String("left"), Symbol: proto.String("bg4"), PayType: proto.String("mysteryJPGR"), Count: proto.Int32(4), Payout: proto.String("28:1:0:280,38:10:0:0,58:10:0:0,88:10:0:0"), UseType: proto.String("all")},
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

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame != nil {
			tt.Errorf("Jackpot Game should be nil")
		}

		logic.JackpotGame = nil
	})
}

func TestDHZWLogic_Background_ReplaceBG(t *testing.T) {

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	c := make(chan bool)
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) {
			data := m.MsgData.(*netproto.Slot_MSG_Status)
			if data.GetState() == 0 {
				c <- true
				close(c)
			}
			return
		}
	}

	user.UserState.OnUserQueue(user, logic.GetTable().Room)
	go logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	<-c

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		// slot_test.PrintSendMessgeFunc(tt, userID, m)
	}

	logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), Order: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), Order: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), Order: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), Order: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), Order: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
	}

	logic.ReelBgDistribution = []*protodata.ReelBgDistribution{
		&protodata.ReelBgDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(828), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
	}

	logic.ExtraRule = []*protodata.ExtraRule{
		&protodata.ExtraRule{Id: proto.Int32(1), GroupId: proto.Int32(1), UseType: proto.String("all"), Condition: proto.String("AfterBackground"), ConditionValue: proto.String("left:bg4:4"), Action: proto.String("ReplaceBGR"), ActionValue: proto.String("28:Reel:5:bg4:1000:1000,38:Reel:5:bg4:4970:10000,58:Reel:5:bg4:3750:10000,88:Reel:5:bg4:3750:10000")},
	}

	var records []*netproto.Slot_BonusGameRecord

	t.Run("Win", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			// slot_test.PrintSendMessgeFunc(tt, userID, m)

			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
				data := m.MsgData.(*netproto.Slot_MSG_GameResult)
				jpmoney := data.GetJackpotMoney()
				if jpmoney == 0 {
					tt.Error("Jackpot Money should not be 0")
				}
				if jpmoney != data.GetBackgroundInfo().GetWinMoney() {
					tt.Error("Background WinMoney should equle to Jackpot Money")
				}
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
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(28)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(10 * time.Millisecond)

		if logic.JackpotGame == nil {
			tt.Errorf("Jackpot Game should not be nil")
		}
	})

	t.Run("Pick Item", func(tt *testing.T) {

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

			time.Sleep(10 * time.Millisecond)
		}
	})

	t.Run("Play more", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		m.Rate = proto.Int32(28)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		time.Sleep(10 * time.Millisecond)
	})
}
