package dfdc

import (
	"DataCfgSlot"
	"fmt"
	"testing"

	"example.invalid/golang/protobuf/proto"
)

func TestGameResult_String(t *testing.T) {

	result := GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p7"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p9"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p10"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p11"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p13"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p14"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p15"),
			},
		},
	}

	fmt.Println(result)

	q := result.GetSymbolString()
	fmt.Println(q)
	for _, row := range q {
		fmt.Println(row)
	}

	fmt.Println(result.Transfor().ToSymbolString())
}

func TestGameResult_Transfor(t *testing.T) {

	result := GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p7"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p9"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p10"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p11"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p13"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p14"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p15"),
			},
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
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p7"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p9"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p10"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p11"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p13"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p14"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Symbol: proto.String("p15"),
			},
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
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(1),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(2),
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(3),
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(4),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(5),
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(6),
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(7),
				Symbol: proto.String("p7"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(8),
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(9),
				Symbol: proto.String("p9"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(10),
				Symbol: proto.String("p10"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(11),
				Symbol: proto.String("p11"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(12),
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(13),
				Symbol: proto.String("p13"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(14),
				Symbol: proto.String("p14"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(15),
				Symbol: proto.String("p15"),
			},
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
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(1),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(2),
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(3),
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(4),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(5),
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(6),
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(7),
				Symbol: proto.String("p7"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(8),
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(9),
				Symbol: proto.String("p9"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(10),
				Symbol: proto.String("p10"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(11),
				Symbol: proto.String("p11"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(12),
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(13),
				Symbol: proto.String("p13"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(14),
				Symbol: proto.String("p14"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(15),
				Symbol: proto.String("p15"),
			},
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
