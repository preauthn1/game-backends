package dfdc

import (
	"DataCfgSlot"
	"reflect"
	"testing"

	"example.invalid/golang/protobuf/proto"
)

func TestPaylineAllMatcher_IsMatch_NoWild(t *testing.T) {

	matcher := NewPaylineAllMatcher(nil)

	result := GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(1)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(2)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(3)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(4)),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(5)),
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(6)),
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(7)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(8)),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(9)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(10)),
				Symbol: proto.String("p3"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(11)),
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(12)),
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(13)),
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(14)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(15)),
				Symbol: proto.String("p7"),
			},
		},
	}

	rule := &DataCfgSlot.SlotPayTable{
		Symbol: proto.String("p1"),
		Count:  proto.Int32(int32(4)),
	}

	result = result.Transfor()
	match, paylineres := matcher.IsMatch(result, rule)

	if !match {
		t.Errorf("Match want %v, but %v", true, match)
	}

	if !paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", true, paylineres.IsMatch)
	}

	if paylineres.PayCount != 4 {
		t.Errorf("PaylineResult.PayCount want %v, but %v", 4, paylineres.PayCount)
	}

	if paylineres.WayCount != 4 {
		t.Errorf("PaylineResult.WayCount want %v, but %v", 4, paylineres.WayCount)
	}

	if paylineres.SymbolName != "p1" {
		t.Errorf("PaylineResult.SymbolName want %v, but %v", "p1", paylineres.SymbolName)
	}

	if !reflect.DeepEqual(paylineres.SymbolIDs, []int32{1, 2, 7, 3, 9, 14}) {
		t.Errorf("PaylineResult.SymbolIDs want %v, but %v", []int32{1, 2, 7, 3, 9, 14}, paylineres.SymbolIDs)
	}
}

func TestPaylineAllMatcher_IsMatch_HasWild_NoExt(t *testing.T) {

	wilds := []*DataCfgSlot.SymbolAttributes{
		&DataCfgSlot.SymbolAttributes{
			Symbol: proto.String("w1"),
			IsWild: proto.Int32(1),
			WildEx: proto.String(""),
		},
	}

	matcher := NewPaylineAllMatcher(wilds)

	result := GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(1)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(2)),
				Symbol: proto.String("w1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(3)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(4)),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(5)),
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(6)),
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(7)),
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(8)),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(9)),
				Symbol: proto.String("w1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(10)),
				Symbol: proto.String("p3"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(11)),
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(12)),
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(13)),
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(14)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(15)),
				Symbol: proto.String("p7"),
			},
		},
	}

	rule := &DataCfgSlot.SlotPayTable{
		Symbol: proto.String("p1"),
		Count:  proto.Int32(int32(4)),
	}

	result = result.Transfor()
	match, paylineres := matcher.IsMatch(result, rule)

	if !match {
		t.Errorf("Match want %v, but %v", true, match)
	}

	if !paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", true, paylineres.IsMatch)
	}

	if paylineres.PayCount != 4 {
		t.Errorf("PaylineResult.PayCount want %v, but %v", 4, paylineres.PayCount)
	}

	if paylineres.WayCount != 2 {
		t.Errorf("PaylineResult.WayCount want %v, but %v", 2, paylineres.WayCount)
	}

	if paylineres.SymbolName != "p1" {
		t.Errorf("PaylineResult.SymbolName want %v, but %v", "p1", paylineres.SymbolName)
	}

	if !reflect.DeepEqual(paylineres.SymbolIDs, []int32{1, 2, 3, 9, 14}) {
		t.Errorf("PaylineResult.SymbolIDs want %v, but %v", []int32{1, 2, 3, 9, 14}, paylineres.SymbolIDs)
	}

}

func TestPaylineAllMatcher_IsMatch_HasWild_HasExt(t *testing.T) {

	wilds := []*DataCfgSlot.SymbolAttributes{
		&DataCfgSlot.SymbolAttributes{
			Symbol: proto.String("w1"),
			IsWild: proto.Int32(1),
			WildEx: proto.String("p1,p2,p3"),
		},
	}

	matcher := NewPaylineAllMatcher(wilds)

	result := GameResult{
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(1)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(2)),
				Symbol: proto.String("w1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(3)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(4)),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(5)),
				Symbol: proto.String("p5"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(6)),
				Symbol: proto.String("p6"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(7)),
				Symbol: proto.String("p2"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(8)),
				Symbol: proto.String("p4"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(9)),
				Symbol: proto.String("w1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(10)),
				Symbol: proto.String("p3"),
			},
		},
		[]*DataCfgSlot.SlotSymbolDistribution{
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(11)),
				Symbol: proto.String("p3"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(12)),
				Symbol: proto.String("p12"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(13)),
				Symbol: proto.String("p8"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(14)),
				Symbol: proto.String("p1"),
			},
			&DataCfgSlot.SlotSymbolDistribution{
				Id:     proto.Uint32(uint32(15)),
				Symbol: proto.String("p7"),
			},
		},
	}

	rule := &DataCfgSlot.SlotPayTable{
		Symbol: proto.String("p1"),
		Count:  proto.Int32(int32(4)),
	}

	result = result.Transfor()
	match, paylineres := matcher.IsMatch(result, rule)

	if match {
		t.Errorf("Match want %v, but %v", false, match)
	}

	if paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", false, paylineres.IsMatch)
	}

}
