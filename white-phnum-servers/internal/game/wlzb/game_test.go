package wlzb

import (
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestLogic_CalcGameAward(t *testing.T) {

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
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	t.Run("Normal Case1", func(tt *testing.T) {
		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}
		}

		logic.Bet(user, 1)
		logic.Spin(501)

		logic.GameResultManager.RowMode()
		tt.Log(logic.GameResultManager)
		tt.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney)
		logic.Clean()
	})

	t.Run("Win FreeGame Case1", func(tt *testing.T) {
		{
			logic.SlotSymbolDistribution = []*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(1), Symbol: proto.String("s1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(1), Order: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(501), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}
		}

		logic.Bet(user, 1)
		logic.Spin(501)

		logic.GameResultManager.RowMode()
		tt.Log(logic.GameResultManager)
		logic.Clean()
	})

	t.Run("FreeGame Case1", func(tt *testing.T) {
		{
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

		logic.FreeSpin = slot.NewFreeSpin(logic.FreeSpinTable[0], logic.SlotSymbolDistribution, 1, 1, false)

		logic.Spin(502)

		logic.GameResultManager.RowMode()
		tt.Log(logic.GameResultManager)
		tt.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney)
		logic.Clean()
	})

	t.Run("FreeGame Case2", func(tt *testing.T) {
		{
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
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(2), Order: proto.Uint32(3), Symbol: proto.String("w2"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(3), Order: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(4), Order: proto.Uint32(3), Symbol: proto.String("p6"), Proportion: proto.Uint32(1)},
				&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(502), ReelNum: proto.Uint32(5), Order: proto.Uint32(3), Symbol: proto.String("p7"), Proportion: proto.Uint32(1)},
			}
		}

		logic.FreeSpin = slot.NewFreeSpin(logic.FreeSpinTable[0], logic.SlotSymbolDistribution, 1, 1, false)

		logic.Spin(502)

		logic.GameResultManager.RowMode()
		tt.Log(logic.GameResultManager)
		tt.Log(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney)
		logic.Clean()
	})
}

func TestLogic_CalcGameAward_FreeGame(t *testing.T) {

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
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

}
