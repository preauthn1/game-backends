package aladdin

import (
	"conf"
	"config"
	"event"
	"fixtures"
	"fmt"
	"gamelogic"
	"msg"
	"netproto"
	"network"
	"player"
	"roomserver"
	"testing"

	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"

	"example.invalid/golang/protobuf/proto"
)

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
	fmt.Println("MaxBetCount", logic.MaxBetCount)
	fmt.Println("BigWinRule", logic.BigWinRule)
	fmt.Println("BigWinNotify", logic.BigWinNotify)
	fmt.Println("EndActionTime", logic.EndActionTime)
	fmt.Println("MinCarryMoney", logic.MinCarryMoney)
	fmt.Println("SymbolsDesc", logic.SymbolsDesc)
	fmt.Println("PayLineDesc", logic.PayLineDesc)

	fmt.Println("DefaultReelNum", logic.DefaultReelNum)
	fmt.Println("DefaultRowNum", logic.DefaultRowNum)

	fmt.Println("PayLineMode", logic.PayLineMode)
	// fmt.Println("PaylineMatcher", logic.PaylineMatcher)

	fmt.Println("Jackpots", logic.Jackpots)

	fmt.Println("=== SlotPattern ===")
	fmt.Println(logic.SlotPattern)

	fmt.Println("=== MainGameTable ===", len(logic.MainGameTable))
	// fmt.Println(logic.MainGameTable)

	fmt.Println("=== SlotPayLine ===", len(logic.SlotPayLine))
	// for _, row := range logic.SlotPayLine {
	// 	fmt.Println(row)
	// }

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
	// for _, row := range logic.SlotSymbolDistribution {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== JackpotTable ===", len(logic.JackpotTable))
	// for _, row := range logic.JackpotTable {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== PickGameTable ===", len(logic.PickGameTable))
	// for _, row := range logic.PickGameTable {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== BonusTable ===", len(logic.BonusTable))
	// for _, row := range logic.BonusTable {
	// 	fmt.Println(row)
	// }

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

	t.Run("UserLeave", func(tt *testing.T) {
		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println(userID, m, m.MsgData)
		}

		logic.UserLeave(user)

	})
}

func TestLogic_GameMessage_UseCase_All(t *testing.T) {

	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotPatternID = uint32(1)
		logic.SlotPattern = &protodata.SlotPattern{Id: proto.Uint32(1), MainGameTableGroupId: proto.Uint32(1)}

		logic.MainGameTable = []*protodata.MainGameTable{
			&protodata.MainGameTable{GroupId: proto.Uint32(1), SymbolGroupId: proto.Uint32(1)},
		}

		logic.DefaultSymbolGroupID = 1
		logic.DefaultReelNum = 5
		logic.DefaultRowNum = 3

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(1), PayType: proto.String("respin"), Payout: proto.String("1")},
			&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(2), PayType: proto.String("respin"), Payout: proto.String("2")},
			&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(3), PayType: proto.String("respin"), Payout: proto.String("3")},
			&protodata.SlotPayTable{Symbol: proto.String("p1"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p2"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p3"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p1"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p2"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p3"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		}

		logic.RespinTable = []*protodata.RespinTable{
			&protodata.RespinTable{GroupId: proto.Int32(1), RespinCount: proto.Uint32(1), AppearReel: proto.String("2"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(2)},
			&protodata.RespinTable{GroupId: proto.Int32(2), RespinCount: proto.Uint32(2), AppearReel: proto.String("2,4"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(3)},
			&protodata.RespinTable{GroupId: proto.Int32(3), RespinCount: proto.Uint32(3), AppearReel: proto.String("2,3,4"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(4)},
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

	t.Run("User Bet & Spin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		fmt.Println(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

}

func TestLogic_GameMessage_UseCase1(t *testing.T) {

	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotPatternID = uint32(1)
		logic.SlotPattern = &protodata.SlotPattern{Id: proto.Uint32(1), MainGameTableGroupId: proto.Uint32(1)}

		logic.MainGameTable = []*protodata.MainGameTable{
			&protodata.MainGameTable{GroupId: proto.Uint32(1), SymbolGroupId: proto.Uint32(1)},
		}

		logic.DefaultSymbolGroupID = 1
		logic.DefaultReelNum = 5
		logic.DefaultRowNum = 3

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("left"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("8")},
			&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("right"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("8")},
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

	t.Run("User Bet & Spin", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(6)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		fmt.Println(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

}

func TestLogic_GameMessage_UseCase_ExpandTwo_OneTime(t *testing.T) {

	conf.CLogLevel = 0
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.SlotPatternID = uint32(1)
		logic.SlotPattern = &protodata.SlotPattern{Id: proto.Uint32(1), MainGameTableGroupId: proto.Uint32(1)}

		logic.MainGameTable = []*protodata.MainGameTable{
			&protodata.MainGameTable{GroupId: proto.Uint32(1), SymbolGroupId: proto.Uint32(1)},
		}

		logic.DefaultSymbolGroupID = 1
		logic.DefaultReelNum = 5
		logic.DefaultRowNum = 3

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(2), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(3), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(4), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
		}

		logic.SlotPayTable = []*protodata.SlotPayTable{
			&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(1), PayType: proto.String("respin"), Payout: proto.String("1")},
			&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(2), PayType: proto.String("respin"), Payout: proto.String("2")},
			&protodata.SlotPayTable{Symbol: proto.String("w1"), LinkType: proto.String("straight"), Count: proto.Int32(3), PayType: proto.String("respin"), Payout: proto.String("3")},
			&protodata.SlotPayTable{Symbol: proto.String("p1"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p2"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p3"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("left"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p1"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p2"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p3"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
			&protodata.SlotPayTable{Symbol: proto.String("p4"), LinkType: proto.String("right"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		}

		logic.RespinTable = []*protodata.RespinTable{
			&protodata.RespinTable{GroupId: proto.Int32(1), RespinCount: proto.Uint32(1), AppearReel: proto.String("2"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(2)},
			&protodata.RespinTable{GroupId: proto.Int32(2), RespinCount: proto.Uint32(2), AppearReel: proto.String("2,4"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(3)},
			&protodata.RespinTable{GroupId: proto.Int32(3), RespinCount: proto.Uint32(3), AppearReel: proto.String("2,3,4"), RespinType: proto.String("Expand"), RespinValue: proto.Uint32(4)},
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

	t.Run("User Bet & Spin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		fmt.Println(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

}

func TestLogic_GameMessage_UseCase_1(t *testing.T) {

	conf.CLogLevel = 4
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.DefaultSymbolGroupID = 1
		logic.DefaultReelNum = 5
		logic.DefaultRowNum = 3

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
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

	t.Run("User Bet & Spin 1", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
		}

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
				fmt.Println("SendMessgeFunc", userID, m)
				data := m.MsgData.(*netproto.ALD_MSG_GameResult)
				fmt.Println("Steps Count", len(data.SlotResult))
				for _, row := range data.GetSlotResult() {
					fmt.Println("BetMoney", row.GetSlotResult().GetBetMoney())
					fmt.Println("WinMoney", row.GetSlotResult().GetWinMoney())
					fmt.Println("FinalMoney", row.GetSlotResult().GetFinalMoney())
					fmt.Println("PayGroup Count", len(row.GetSlotResult().GetPayGroups()))
					for _, _row := range row.GetSlotResult().GetPayGroups() {
						fmt.Println(_row.GetLinkType())
						fmt.Println(_row.GetBlinkPositions())
					}
				}
				return
			}
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		fmt.Println(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

	t.Run("User Bet & Spin 2", func(tt *testing.T) {

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
		}

		logic.GameState = slot.GameStateBet

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
				fmt.Println("SendMessgeFunc", userID, m)
				data := m.MsgData.(*netproto.ALD_MSG_GameResult)
				fmt.Println("Steps Count", len(data.SlotResult))
				for _, row := range data.GetSlotResult() {
					fmt.Println("BetMoney", row.GetSlotResult().GetBetMoney())
					fmt.Println("WinMoney", row.GetSlotResult().GetWinMoney())
					fmt.Println("FinalMoney", row.GetSlotResult().GetFinalMoney())
					fmt.Println("PayGroup Count", len(row.GetSlotResult().GetPayGroups()))
					for _, _row := range row.GetSlotResult().GetPayGroups() {
						fmt.Println(_row.GetLinkType())
						fmt.Println(_row.GetBlinkPositions())
					}
				}
				return
			}
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

		fmt.Println(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney, logic.GameUser.FlowAmount)

	})

}

func TestLogic_GameMessage_UseCase_2(t *testing.T) {

	conf.CLogLevel = 4
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)
	{
		logic.DefaultSymbolGroupID = 1
		logic.DefaultReelNum = 5
		logic.DefaultRowNum = 3
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

	t.Run("User Bet & Spin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
		}

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
				// 	fmt.Println("SendMessgeFunc", userID, m)
				// 	data := m.MsgData.(*netproto.ALD_MSG_GameResult)
				// 	fmt.Println("Steps Count", len(data.SlotResult))
				// 	for _, row := range data.GetSlotResult() {
				// 		fmt.Println("BetMoney", row.GetSlotResult().GetBetMoney())
				// 		fmt.Println("WinMoney", row.GetSlotResult().GetWinMoney())
				// 		fmt.Println("FinalMoney", row.GetSlotResult().GetFinalMoney())
				// 		fmt.Println("PayGroup Count", len(row.GetSlotResult().GetPayGroups()))
				// 		for _, _row := range row.GetSlotResult().GetPayGroups() {
				// 			fmt.Println(_row.GetLinkType())
				// 			fmt.Println(_row.GetBlinkPositions())
				// 		}
				// 	}
				logic.StepResults[0].ExpandResult.RowMode()
				fmt.Println(logic.StepResults[0].ExpandResult)
				for _, row := range logic.StepResults[0].GamePayResult {
					fmt.Println(row)
				}
				return
			}
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			// fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

	})

	t.Run("User Bet & Spin", func(tt *testing.T) {

		logic.GameState = slot.GameStateBet

		logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
		}

		logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
			if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
				// fmt.Println("SendMessgeFunc", userID, m)
				// data := m.MsgData.(*netproto.ALD_MSG_GameResult)
				// fmt.Println("Steps Count", len(data.SlotResult))
				// for _, row := range data.GetSlotResult() {
				// fmt.Println("BetMoney", row.GetSlotResult().GetBetMoney())
				// fmt.Println("WinMoney", row.GetSlotResult().GetWinMoney())
				// fmt.Println("FinalMoney", row.GetSlotResult().GetFinalMoney())
				// fmt.Println("PayGroup Count", len(row.GetSlotResult().GetPayGroups()))
				// for _, _row := range row.GetSlotResult().GetPayGroups() {
				// fmt.Println(_row.GetLinkType())
				// fmt.Println(_row.GetBlinkPositions())
				// }
				// }
				logic.StepResults[0].ExpandResult.RowMode()
				fmt.Println(logic.StepResults[0].ExpandResult)
				for _, row := range logic.StepResults[0].GamePayResult {
					fmt.Println(row)
				}
				return
			}
			fmt.Println("SendMessgeFunc", userID, m, m.MsgData)
		}

		logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
			// fmt.Println("SendRPCMessageFunc", m, m.MsgData)
		}

		_m := new(msg.Message)
		_m.BClassID = int32(netproto.MessageBClassID_Game)
		_m.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID)

		m := new(netproto.Slot_MSG_Spin)
		m.Money = proto.Int64(10)
		b, _ := proto.Marshal(m)

		_m.MsgData = b

		logic.GameMessage(logic.GameUser, _m)

	})

}
