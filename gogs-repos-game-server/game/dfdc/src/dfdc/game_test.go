package dfdc

import (
	"DataCfgSlot"
	"fmt"
	"io/ioutil"
	"testing"

	"example.invalid/golang/protobuf/proto"
)

func TestLogic_GetReelSymbol(t *testing.T) {

	b, err := ioutil.ReadFile("../../protoconfig/SlotSymbolDistribution.bytes")
	if err != nil {
		t.Error(err)
	}

	data := new(DataCfgSlot.SlotSymbolDistribution_ARRAY)

	err = proto.Unmarshal(b, data)

	if err != nil {
		t.Error(err)
	}

	logic := new(Logic)
	ans := logic.GetReelSymbol(uint32(1), data.GetItems())

	fmt.Printf("Reel: %d\n", len(ans))

	for r, row := range ans {
		cnt := 0
		for i, symbols := range row {
			for _, symbol := range symbols {
				fmt.Printf("Reel: %d, Index: %d, SymbolID: %d, SymbolName: %s\n", r, i, symbol.GetId(), symbol.GetSymbol())
				cnt++
			}
		}
		fmt.Println(cnt)
	}

}

func TestLogic_RandomSymbol(t *testing.T) {

	logic := new(Logic)

	symbolData := []*DataCfgSlot.SlotSymbolDistribution{
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(1),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p1"),
			Proportion: proto.Uint32(100),
		},
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(2),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p2"),
			Proportion: proto.Uint32(100),
		},
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(3),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p3"),
			Proportion: proto.Uint32(100),
		},
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(4),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p4"),
			Proportion: proto.Uint32(100),
		},
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(5),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p5"),
			Proportion: proto.Uint32(100),
		},
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(6),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p6"),
			Proportion: proto.Uint32(100),
		},
		&DataCfgSlot.SlotSymbolDistribution{
			Id:         proto.Uint32(7),
			GroupId:    proto.Uint32(1),
			ReelNum:    proto.Uint32(1),
			Order:      proto.Uint32(1),
			Symbol:     proto.String("p7"),
			Proportion: proto.Uint32(100),
		},
	}

	for i := 0; i < 10; i++ {
		fmt.Println(logic.RandomSymbol(symbolData))
	}

}

func TestLogic_CreateGameResult(t *testing.T) {
	b, err := ioutil.ReadFile("../../protoconfig/SlotSymbolDistribution.bytes")
	if err != nil {
		t.Error(err)
	}
	data := new(DataCfgSlot.SlotSymbolDistribution_ARRAY)
	err = proto.Unmarshal(b, data)
	if err != nil {
		t.Error(err)
	}

	logic := new(Logic)
	logic.GameSymbolGroupID = uint32(1)
	logic.GameReelNum = uint32(5)
	logic.GameRowNum = uint32(3)
	logic.SlotSymbolDistribution = data.GetItems()

	fmt.Println(logic.GameResult)
	logic.CreateGameResult()
	fmt.Println(logic.GameResult)
}

func TestLogic_CalcGameResult_ANY(t *testing.T) {

	logic := new(Logic)

	logic.SlotPayTable = []*DataCfgSlot.SlotPayTable{
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(9)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(8)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(7)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(6)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(2)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(1)},
	}

	logic.GameResult = GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p3")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p3")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("p4")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("p2")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
	}

	logic.CalcGameResult()
}

func TestLogic_CalcGameResult_PayLineModeAll(t *testing.T) {

	logic := new(Logic)

	logic.SymbolAttributes = []*DataCfgSlot.SymbolAttributes{
		&DataCfgSlot.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("p3,p4")},
	}

	logic.SlotPayTable = []*DataCfgSlot.SlotPayTable{
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(3)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(5)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(4)},
		&DataCfgSlot.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(3)},
	}

	logic.PayLineMode = PayLineModeAll
	logic.PaylineMatcher = NewPaylineAllMatcher(logic.SymbolAttributes)

	logic.GameResult = GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p3")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("p3")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("p4")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("p5")},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("p1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("p2")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("w1")},
			&DataCfgSlot.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p5")},
		},
	}

	fmt.Println(logic.GameResult)

	logic.CalcGameResult()

	for _, row := range logic.GamePayResult {
		fmt.Println(row)
	}
}
