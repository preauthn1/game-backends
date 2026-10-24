package slot

import (
	"fmt"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot/protodata"
)

func TestGameResult_String(t *testing.T) {

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}

	fmt.Println(result)
}

func TestGameResult_Transfor(t *testing.T) {

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}

	fmt.Println(result)

	result = result.Transfor()
	fmt.Println(result)

	result = result.Transfor()
	fmt.Println(result)
}

func TestGameResult_Reverse(t *testing.T) {

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}

	fmt.Println(result)

	result = result.Transfor()
	fmt.Println(result)

	result = result.Reverse()
	fmt.Println(result)

	result = result.Transfor()
	fmt.Println(result)
}

func TestGameResult_GetSymbolIDByRow(t *testing.T) {

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}

	fmt.Println(result.GetSymbolIDByRow(0))
	fmt.Println(result.GetSymbolIDByRow(1))
	fmt.Println(result.GetSymbolIDByRow(2))
	fmt.Println(result.GetSymbolIDByRow(3))
	fmt.Println(result.GetSymbolIDByRow(4))

}

func TestGameResult_GetReelSymbol(t *testing.T) {
	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p10")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p11")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p13")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p14")},
			&protodata.SlotSymbolDistribution{Symbol: proto.String("p15")},
		},
	}

	fmt.Println(result)

	data := result.GetReelSymbol()

	for reelID, reel := range data {
		for rowID, row := range reel {
			fmt.Printf("Reel: %d, Row: %d, Symbol: %s\n", reelID, rowID, row[0].GetSymbol())
		}
	}
}
