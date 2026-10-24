package aladdin

import (
	"fmt"
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestLogic_CalcGameResult(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.UserType = gamelogic.UserType_Normal
	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	t.Run("Spin", func(tt *testing.T) {

		for _, row := range logic.SlotPayLine {
			fmt.Println(row)
		}

		result := slot.GameResult{
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			},
		}

		manager, _ := slot.NewGameResultManager(5, 3)
		manager.GameResult = result

		step := NewStepResult(1, false, 6, 60, logic.SymbolAttributes, manager, nil)

		logic.CalcGameResult(step)

		fmt.Println(step.GamePayResult)
		step.ExpandResult.RowMode()
		fmt.Println(step.ExpandResult)

	})
}
