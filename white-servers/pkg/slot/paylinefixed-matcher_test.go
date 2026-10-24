package slot

import (
	"reflect"
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

func TestPaylineFixedMatcher_ANY(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
	}

	matcher := NewPaylineFixedMatcher(nil, payline)

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

func TestPaylineFixedMatcher_IsMatch_Left_NoWild(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
	}

	matcher := NewPaylineFixedMatcher(nil, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p6")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p2")},
		},
	}

	rule := &protodata.SlotPayTable{
		LinkType: proto.String("left"),
		Symbol:   proto.String("p6"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	t.Log(manager)

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result)
		for _, row := range result.SymbolPayLines {
			t.Log(row.HasWild)
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}

}

func TestPaylineFixedMatcher_IsMatch_Left_HasWild(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p6")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p2")},
		},
	}

	rule := &protodata.SlotPayTable{
		Id:       proto.Int32(1),
		GroupId:  proto.Int32(2),
		LinkType: proto.String("left"),
		Symbol:   proto.String("p6"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
		Payout:   proto.String("100"),
		UseType:  proto.String("all"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	t.Log(manager)

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result.HasWild)
		for _, row := range result.SymbolPayLines {
			t.Log(row.HasWild)
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}
}

func TestPaylineFixedMatcher_IsMatch_Left_HasWild_TwoLine(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_2,3_1,4_2,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_2,3_1,4_1,5_1")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("w1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p6")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p2")},
		},
	}

	rule := &protodata.SlotPayTable{
		Id:       proto.Int32(1),
		GroupId:  proto.Int32(2),
		LinkType: proto.String("left"),
		Symbol:   proto.String("p6"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
		Payout:   proto.String("100"),
		UseType:  proto.String("all"),
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	t.Log(manager)

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result.HasWild)
		for _, row := range result.SymbolPayLines {
			t.Log(row.HasWild)
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}
}

func TestPaylineFixedMatcher_IsMatch_Left_HasWild_AllWild(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100003), GroupId: proto.Uint32(1), LineId: proto.Uint32(3), Content: proto.String("1_3,2_3,3_3,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100004), GroupId: proto.Uint32(1), LineId: proto.Uint32(4), Content: proto.String("1_1,2_2,3_3,4_2,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100005), GroupId: proto.Uint32(1), LineId: proto.Uint32(5), Content: proto.String("1_3,2_2,3_1,4_2,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100006), GroupId: proto.Uint32(1), LineId: proto.Uint32(6), Content: proto.String("1_2,2_1,3_1,4_1,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100007), GroupId: proto.Uint32(1), LineId: proto.Uint32(7), Content: proto.String("1_2,2_3,3_3,4_3,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100008), GroupId: proto.Uint32(1), LineId: proto.Uint32(8), Content: proto.String("1_1,2_1,3_2,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100009), GroupId: proto.Uint32(1), LineId: proto.Uint32(9), Content: proto.String("1_3,2_3,3_2,4_1,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100010), GroupId: proto.Uint32(1), LineId: proto.Uint32(10), Content: proto.String("1_2,2_1,3_2,4_3,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100011), GroupId: proto.Uint32(1), LineId: proto.Uint32(11), Content: proto.String("1_2,2_3,3_2,4_1,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100012), GroupId: proto.Uint32(1), LineId: proto.Uint32(12), Content: proto.String("1_1,2_2,3_2,4_2,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100013), GroupId: proto.Uint32(1), LineId: proto.Uint32(13), Content: proto.String("1_3,2_2,3_2,4_2,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100014), GroupId: proto.Uint32(1), LineId: proto.Uint32(14), Content: proto.String("1_1,2_2,3_1,4_2,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100015), GroupId: proto.Uint32(1), LineId: proto.Uint32(15), Content: proto.String("1_3,2_2,3_3,4_2,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100016), GroupId: proto.Uint32(1), LineId: proto.Uint32(16), Content: proto.String("1_2,2_2,3_1,4_2,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100017), GroupId: proto.Uint32(1), LineId: proto.Uint32(17), Content: proto.String("1_2,2_2,3_3,4_2,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100018), GroupId: proto.Uint32(1), LineId: proto.Uint32(18), Content: proto.String("1_1,2_1,3_3,4_1,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100019), GroupId: proto.Uint32(1), LineId: proto.Uint32(19), Content: proto.String("1_3,2_3,3_1,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100020), GroupId: proto.Uint32(1), LineId: proto.Uint32(20), Content: proto.String("1_1,2_3,3_3,4_3,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100021), GroupId: proto.Uint32(1), LineId: proto.Uint32(21), Content: proto.String("1_3,2_1,3_1,4_1,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100022), GroupId: proto.Uint32(1), LineId: proto.Uint32(22), Content: proto.String("1_1,2_1,3_2,4_1,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100023), GroupId: proto.Uint32(1), LineId: proto.Uint32(23), Content: proto.String("1_3,2_3,3_2,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100024), GroupId: proto.Uint32(1), LineId: proto.Uint32(24), Content: proto.String("1_2,2_1,3_2,4_1,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100025), GroupId: proto.Uint32(1), LineId: proto.Uint32(25), Content: proto.String("1_2,2_3,3_2,4_3,5_2")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	rule1 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule2 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule3 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule4 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5), PayType: proto.String("chip")}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p2")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("w1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p6")},
		},
	}

	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	if ok, _ := matcher.IsMatch(manager, rule1); !ok {
		t.Errorf("Rule want match. %v", rule1)
	}
	if ok, _ := matcher.IsMatch(manager, rule2); ok {
		t.Errorf("Rule want no match. %v", rule2)
	}
	if ok, _ := matcher.IsMatch(manager, rule3); ok {
		t.Errorf("Rule want no match. %v", rule3)
	}
	if ok, _ := matcher.IsMatch(manager, rule4); ok {
		t.Errorf("Rule want no match. %v", rule4)
	}
}

func TestPaylineFixedMatcher_IsMatch_Right_NoWild_MirrorLine(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_3,4_1,5_1")},
	}

	matcher := NewPaylineFixedMatcher(nil, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p1")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p6")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p2")},
		},
	}

	rule := &protodata.SlotPayTable{
		LinkType: proto.String("right"),
		Symbol:   proto.String("p1"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	t.Log(result)
	t.Log(rule)
	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}

}

func TestPaylineFixedMatcher_IsMatch_Right_NoWild_Non_MirrorLine(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_3,4_2,5_2")},
	}

	matcher := NewPaylineFixedMatcher(nil, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p3")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p3")},
		},
	}

	rule := &protodata.SlotPayTable{
		LinkType: proto.String("right"),
		Symbol:   proto.String("p1"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(4),
	}

	t.Log(result)
	t.Log(rule)
	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}

}

func TestPaylineFixedMatcher_IsMatch_WildMultiplier_All(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w2"), Multiplier: proto.Int32(1)},
		&protodata.SymbolAttributes{Symbol: proto.String("w2"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(2)},
		&protodata.SymbolAttributes{Symbol: proto.String("w3"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(3)},
		&protodata.SymbolAttributes{Symbol: proto.String("w4"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(4)},
	}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("w2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("w2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("w4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("w1")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p1")},
		},
	}

	rule := &protodata.SlotPayTable{
		LinkType: proto.String("left"),
		Symbol:   proto.String("w1,w2,w3,w4"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	t.Log(result)
	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
			t.Log("SymbolName: ", row.SymbolNames)
			t.Log("MultipleStep: ", row.MultipleStep)
		}
	}

	rule2 := &protodata.SlotPayTable{
		LinkType: proto.String("left"),
		Symbol:   proto.String("w1"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	if match, result := matcher.IsMatch(manager, rule2); match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should not be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
			t.Log("MultipleStep: ", row.MultipleStep)
		}
	}

}

func TestPaylineFixedMatcher_IsMatch_WildMultiplier_W1_All(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w2"), Multiplier: proto.Int32(1)},
		&protodata.SymbolAttributes{Symbol: proto.String("w2"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(2)},
		&protodata.SymbolAttributes{Symbol: proto.String("w3"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(3)},
		&protodata.SymbolAttributes{Symbol: proto.String("w4"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(4)},
	}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("w1")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p1")},
		},
	}

	rule := &protodata.SlotPayTable{
		LinkType: proto.String("left"),
		Symbol:   proto.String("w1,w2,w3,w4"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	t.Log(result)
	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	if match, result := matcher.IsMatch(manager, rule); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should not be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}

	rule2 := &protodata.SlotPayTable{
		LinkType: proto.String("left"),
		Symbol:   proto.String("w1"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	if match, result := matcher.IsMatch(manager, rule2); !match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}

}

func TestPaylineFixedMatcher_IsMatch_WildMultiplier_W1_All_OverRule(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w2"), Multiplier: proto.Int32(1)},
		&protodata.SymbolAttributes{Symbol: proto.String("w2"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(2)},
		&protodata.SymbolAttributes{Symbol: proto.String("w3"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(3)},
		&protodata.SymbolAttributes{Symbol: proto.String("w4"), IsWild: proto.Int32(1), WildEx: proto.String("s1,w1"), Multiplier: proto.Int32(4)},
	}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("w1")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p1")},
		},
	}

	rule := &protodata.SlotPayTable{
		LinkType: proto.String("left"),
		Symbol:   proto.String("p1"),
		PayType:  proto.String("chip"),
		Count:    proto.Int32(5),
	}

	t.Log(result)
	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result

	if match, result := matcher.IsMatch(manager, rule); match {
		t.Log(result)
		t.Log("PayLine: ", payline)
		t.Log("Rule: ", rule)
		t.Errorf("Pay rule should not be matched")
	} else {
		t.Log(result.Rule)
		for _, row := range result.SymbolPayLines {
			t.Log(row.SymbolIDs)
			t.Log(row.PayLine.Origin)
			t.Log(row.PayLine.Positions)
		}
	}

}

func TestPaylineFixedMatcher_IsMatch_BugFix_Issue8(t *testing.T) {

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_1,3_1,4_1,5_2")},
	}

	wilds := []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String(""), Multiplier: proto.Int32(1)},
	}

	matcher := NewPaylineFixedMatcher(wilds, payline)

	result := GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(1)), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(2)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(3)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(4)), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(5)), Symbol: proto.String("p6")}},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(6)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(7)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(8)), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(9)), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(10)), Symbol: proto.String("p6")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(11)), Symbol: proto.String("p6")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(12)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(13)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(14)), Symbol: proto.String("p7")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(15)), Symbol: proto.String("p2")},
		},
	}
	manager, err := NewGameResultManager(5, 3)
	if err != nil {
		t.Error(err)
		return
	}
	manager.GameResult = result
	t.Log(manager)

	rule1 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), PayType: proto.String("chip"), Count: proto.Int32(3)}
	rule2 := &protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p6"), PayType: proto.String("chip"), Count: proto.Int32(3)}

	t.Log(matcher.IsMatch(manager, rule1))
	t.Log(matcher.IsMatch(manager, rule2))

}
