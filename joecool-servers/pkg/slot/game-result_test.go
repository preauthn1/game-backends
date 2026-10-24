package slot

import (
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
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

	t.Log(result)
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

	t.Log(result)

	result = result.Transfor()
	t.Log(result)

	result = result.Transfor()
	t.Log(result)
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

	t.Log(result)

	result = result.Transfor()
	t.Log(result)

	result = result.Reverse()
	t.Log(result)

	result = result.Transfor()
	t.Log(result)
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

	t.Log(result.GetSymbolIDByRow(0))
	t.Log(result.GetSymbolIDByRow(1))
	t.Log(result.GetSymbolIDByRow(2))
	t.Log(result.GetSymbolIDByRow(3))
	t.Log(result.GetSymbolIDByRow(4))

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

	t.Log(result)

	data := result.GetReelSymbol()

	for reelID, reel := range data {
		for rowID, row := range reel {
			t.Logf("Reel: %d, Row: %d, Symbol: %s\n", reelID, rowID, row[0].GetSymbol())
		}
	}
}
