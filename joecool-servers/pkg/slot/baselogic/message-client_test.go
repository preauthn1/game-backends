package baselogic

import (
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestBuildReelResult(t *testing.T) {

	logic := new(SlotLogicBase)
	logic.PayLineMode = slot.PayLineModeAll

	var manager *slot.GameResultManager
	{
		result := slot.GameResult{
			[]*protodata.SlotSymbolDistribution{
				nil,
				nil,
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
				nil,
				nil,
			},
			[]*protodata.SlotSymbolDistribution{
				nil,
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
				nil,
			},
			[]*protodata.SlotSymbolDistribution{
				nil,
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
				nil,
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p16")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p17")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p18")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p19")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p20")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p21")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p22")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p23")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p24")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p25")},
			},
			[]*protodata.SlotSymbolDistribution{
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p26")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p27")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p28")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p29")},
				&protodata.SlotSymbolDistribution{Symbol: proto.String("p30")},
			},
		}
		_manager, err := slot.NewGameResultManager(5, 6)
		if err != nil {
			t.Error(err)
			return
		}
		_manager.GameResult = result

		manager = _manager
	}

	ans := logic.BuildReelResult(manager, nil)
	for _, row := range ans {
		t.Log(row)
	}
}
