package slot

import (
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestGameResultManager_String(t *testing.T) {

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

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}

	manager.GameResult = result

	t.Log(manager)
	t.Log(manager.IsReelMode(), manager.IsRowMode())

	for _, row := range manager.GameResult {
		t.Log(row)
	}

	manager.ReelMode()
	t.Log(manager.IsReelMode(), manager.IsRowMode())

	manager.ReelMode()
	t.Log(manager.IsReelMode(), manager.IsRowMode())

	t.Log(manager.Reverse())

	manager.RowMode()
	t.Log(manager.IsReelMode(), manager.IsRowMode())

	manager.RowMode()
	t.Log(manager.IsReelMode(), manager.IsRowMode())

	t.Log(manager.Reverse())

	t.Log(manager.ToSymbolString())
	manager.ReelMode()
	t.Log(manager.ToSymbolString())
}

func TestGameResultManager_Generate_GameResult(t *testing.T) {
	SymbolTable := []*protodata.SlotSymbolDistribution{
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(16), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(17), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(18), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},

		&protodata.SlotSymbolDistribution{Id: proto.Uint32(19), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(20), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(21), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(22), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(23), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(24), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(25), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(26), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(27), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(28), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(29), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(30), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(31), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(32), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(33), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(34), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(35), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(36), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},

		&protodata.SlotSymbolDistribution{Id: proto.Uint32(37), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(38), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(39), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(40), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(41), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(42), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(43), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(44), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(45), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(46), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(47), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(48), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(49), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(50), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(51), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(52), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(53), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(54), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},

		&protodata.SlotSymbolDistribution{Id: proto.Uint32(55), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(56), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(57), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(58), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(59), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(60), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(61), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(62), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(63), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(64), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(65), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(66), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(67), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(68), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(69), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(70), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(71), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(72), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},

		&protodata.SlotSymbolDistribution{Id: proto.Uint32(73), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(74), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(75), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(76), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(77), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(78), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(79), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(80), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(81), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(82), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(83), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(84), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(85), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(86), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(87), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(88), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(89), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
		&protodata.SlotSymbolDistribution{Id: proto.Uint32(90), GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}

	manager.GenerateGameResult(1, SymbolTable)

	t.Log(manager)
}

func TestGameResultManager_Generate_Background(t *testing.T) {
	dists := []*protodata.ReelBgDistribution{
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(1), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(2), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(3), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg1"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg2"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg3"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(4), BgEnum: proto.String("bg4"), Proportion: proto.Uint32(1)},
		&protodata.ReelBgDistribution{GroupId: proto.Uint32(801), ReelNum: proto.Uint32(5), BgEnum: proto.String("bg0"), Proportion: proto.Uint32(1)},
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}

	manager.GenerateBackground(801, dists)

	t.Log(manager.BackgroundResult)
}


func TestGameResultManager_GenerateGameResult_New(t *testing.T) {
	
	conf.CLogLevel = 99
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true
	conf.ClearLog = true
	conf.AnalysisMode = false

	t.Run("3 Row", func(tt *testing.T){
		manager, err := NewGameResultManager(5, 7)
		if err != nil {
			t.Error(err)
			return
		}

		SymbolTable := []*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(1), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(2), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(3), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(4), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},

			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(1)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(2)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(3)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(4)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(5)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(6)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p1"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p2"), Proportion: proto.Uint32(100), Order: proto.Uint32(7)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p3"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p4"), Proportion: proto.Uint32(100), Order: proto.Uint32(8)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("p5"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},
			&protodata.SlotSymbolDistribution{GroupId: proto.Uint32(1), ReelNum: proto.Uint32(5), Symbol: proto.String("w1"), Proportion: proto.Uint32(100), Order: proto.Uint32(9)},
		}

		manager.GenerateGameResult(1, SymbolTable)

		t.Log(manager)
	})
	
}