package slot

import (
	"reflect"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot/protodata"
)

func TestPaylineAllMatcher_ANY(t *testing.T) {
	matcher := NewPaylineAllMatcher(nil)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p3")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p7")},
		},
	}

	rule := &protodata.SlotPayTable{
		Symbol:   proto.String("b1"),
		Count:    proto.Int32(int32(3)),
		LinkType: proto.String("any"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	match, paylineres := matcher.IsMatch(manager, rule)

	if !match {
		t.Errorf("Match want %v, but %v", true, match)
	}

	if !paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", true, paylineres.IsMatch)
	}

	if paylineres.PayCount != 3 {
		t.Errorf("PaylineResult.PayCount want %v, but %v", 3, paylineres.PayCount)
	}

	if paylineres.WayCount != 1 {
		t.Errorf("PaylineResult.WayCount want %v, but %v", 1, paylineres.WayCount)
	}

	if paylineres.SymbolName != "b1" {
		t.Errorf("PaylineResult.SymbolName want %v, but %v", "p1", paylineres.SymbolName)
	}

	if !reflect.DeepEqual(paylineres.SymbolIDs, []int32{3, 9, 14}) {
		t.Errorf("PaylineResult.SymbolIDs want %v, but %v", []int32{3, 9, 14}, paylineres.SymbolIDs)
	}
}

func TestPaylineAllMatcher_IsMatch_Left_NoWild(t *testing.T) {

	matcher := NewPaylineAllMatcher(nil)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p3")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p7")},
		},
	}

	rule := &protodata.SlotPayTable{
		Symbol:   proto.String("p1"),
		Count:    proto.Int32(int32(4)),
		LinkType: proto.String("left"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	match, paylineres := matcher.IsMatch(manager, rule)

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

	if !reflect.DeepEqual(paylineres.SymbolIDs, []int32{1, 2, 3, 7, 9, 14}) {
		t.Errorf("PaylineResult.SymbolIDs want %v, but %v", []int32{1, 2, 3, 3, 9, 14}, paylineres.SymbolIDs)
	}
}

func TestPaylineAllMatcher_IsMatch_Left_HasWild_NoExt(t *testing.T) {

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	matcher := NewPaylineAllMatcher(wilds)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p3")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p7")},
		},
	}

	rule := &protodata.SlotPayTable{
		Symbol:   proto.String("p1"),
		Count:    proto.Int32(int32(4)),
		LinkType: proto.String("left"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	match, paylineres := matcher.IsMatch(manager, rule)

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

func TestPaylineAllMatcher_IsMatch_Left_HasWild_HasExt(t *testing.T) {

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("p1,p2,p3")},
	}

	matcher := NewPaylineAllMatcher(wilds)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p3")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p7")},
		},
	}

	rule := &protodata.SlotPayTable{
		Symbol:   proto.String("p1"),
		Count:    proto.Int32(int32(4)),
		LinkType: proto.String("left"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	match, paylineres := matcher.IsMatch(manager, rule)

	if match {
		t.Errorf("Match want %v, but %v", false, match)
	}

	if paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", false, paylineres.IsMatch)
	}

}

func TestPaylineAllMatcher_IsMatch_Right_NoWild(t *testing.T) {

	matcher := NewPaylineAllMatcher(nil)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p4")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p7")},
		},
	}

	rule := &protodata.SlotPayTable{
		Symbol:   proto.String("p4"),
		Count:    proto.Int32(int32(3)),
		LinkType: proto.String("right"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	match, paylineres := matcher.IsMatch(manager, rule)

	if !match {
		t.Errorf("Match want %v, but %v", true, match)
	}

	if !paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", true, paylineres.IsMatch)
	}

	if paylineres.PayCount != 3 {
		t.Errorf("PaylineResult.PayCount want %v, but %v", 3, paylineres.PayCount)
	}

	if paylineres.WayCount != 1 {
		t.Errorf("PaylineResult.WayCount want %v, but %v", 1, paylineres.WayCount)
	}

	if paylineres.SymbolName != "p4" {
		t.Errorf("PaylineResult.SymbolName want %v, but %v", "p4", paylineres.SymbolName)
	}

	if !reflect.DeepEqual(paylineres.SymbolIDs, []int32{4, 8, 10}) {
		t.Errorf("PaylineResult.SymbolIDs want %v, but %v", []int32{4, 8, 10}, paylineres.SymbolIDs)
	}
}

func TestPaylineAllMatcher_IsMatch_Right_HasWild_NoExt(t *testing.T) {

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	matcher := NewPaylineAllMatcher(wilds)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p3")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p12")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p7")},
		},
	}

	rule := &protodata.SlotPayTable{
		Symbol:   proto.String("p1"),
		Count:    proto.Int32(int32(4)),
		LinkType: proto.String("right"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	match, paylineres := matcher.IsMatch(manager, rule)

	if !match {
		t.Errorf("Match want %v, but %v", true, match)
	}

	if !paylineres.IsMatch {
		t.Errorf("PaylineResult.IsMatch want %v, but %v", true, paylineres.IsMatch)
	}

	if paylineres.PayCount != 4 {
		t.Errorf("PaylineResult.PayCount want %v, but %v", 4, paylineres.PayCount)
	}

	if paylineres.WayCount != 3 {
		t.Errorf("PaylineResult.WayCount want %v, but %v", 3, paylineres.WayCount)
	}

	if paylineres.SymbolName != "p1" {
		t.Errorf("PaylineResult.SymbolName want %v, but %v", "p1", paylineres.SymbolName)
	}

	if !reflect.DeepEqual(paylineres.SymbolIDs, []int32{3, 4, 5, 7, 9, 14}) {
		t.Errorf("PaylineResult.SymbolIDs want %v, but %v", []int32{3, 4, 5, 7, 9, 14}, paylineres.SymbolIDs)
	}

}
