package candyhouse

import (
	"fmt"
	"gamelogic"
	"io/ioutil"
	"netproto"
	"reflect"
	"sort"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
)

func TestLogic_Bet(t *testing.T) {
	logic := createBasicTestLogic(4)

	t.Run("Success Bet", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet
		logic.GameUser.Score = 10000

		ok, retcode, betmoney := logic.Bet(logic.GameUser, 100)

		if !ok {
			tt.Errorf("Bet 100 should be ok")
		}
		if retcode != slot.BetStateSuccess {
			tt.Errorf("Retcode want %d, but %d", slot.BetStateSuccess, retcode)
		}
		if betmoney != 2500 {
			tt.Errorf("BetMoney want %d, but %d", 2500, betmoney)
		}

		if logic.GameBetMoney != 100 {
			tt.Errorf("logic.GameBetMoney want %d, but %d", 100, logic.GameBetMoney)
		}
		if logic.GamePayMoney != 2500 {
			tt.Errorf("logic.GamePayMoney want %d, but %d", 100, logic.GamePayMoney)
		}

	})

	t.Run("Bet State Error", func(tt *testing.T) {
		logic.GameState = slot.GameStateWait

		ok, retcode, betmoney := logic.Bet(logic.GameUser, 100)

		if ok {
			tt.Errorf("Bet 100 should be false")
		}
		if retcode != slot.BetStateStepError {
			tt.Errorf("Retcode want %d, but %d", slot.BetStateSuccess, retcode)
		}
		if betmoney != 2500 {
			tt.Errorf("BetMoney want %d, but %d", 2500, betmoney)
		}
	})

	t.Run("User Money Not Enough For Bet", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet
		logic.GameUser.Score = 10

		ok, retcode, betmoney := logic.Bet(logic.GameUser, 100)

		if ok {
			tt.Errorf("Bet 100 should be false")
		}
		if retcode != slot.BetStateMoneyNotEnough {
			tt.Errorf("Retcode want %d, but %d", slot.BetStateMoneyNotEnough, retcode)
		}
		if betmoney != 2500 {
			tt.Errorf("BetMoney want %d, but %d", 2500, betmoney)
		}
	})

	t.Run("User Money Not Enough  For MinBet", func(tt *testing.T) {
		logic.GameState = slot.GameStateBet
		logic.MinCarryMoney = 10000
		logic.GameUser.Score = 5000

		ok, retcode, betmoney := logic.Bet(logic.GameUser, 100)

		if ok {
			tt.Errorf("Bet 100 should be false")
		}
		if retcode != slot.BetStateMoneyLessMin {
			tt.Errorf("Retcode want %d, but %d", slot.BetStateMoneyLessMin, retcode)
		}
		if betmoney != 2500 {
			tt.Errorf("BetMoney want %d, but %d", 2500, betmoney)
		}
	})

}

func TestLogic_CreateGameResult(t *testing.T) {
	b, err := ioutil.ReadFile("../../protodata/SlotSymbolDistribution.bytes")
	if err != nil {
		t.Error(err)
	}
	data := new(protodata.SlotSymbolDistribution_ARRAY)
	err = proto.Unmarshal(b, data)
	if err != nil {
		t.Error(err)
	}

	logic := new(Logic)
	logic.SlotSymbolDistribution = data.GetItems()
	logic.MainGameTable = &protodata.MainGameTable{SymbolGroupId: proto.Uint32(1)}
	logic.DefaultReelNum = 5
	logic.DefaultRowNum = 3

	fmt.Println(logic.GameResultManager)
	logic.CreateGameResult()
	fmt.Println(logic.GameResultManager)
}

func TestLogic_CalcGameResult_ANY(t *testing.T) {

	logic := createBasicTestLogic(2)

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(9)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(8)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(7)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(6)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(2)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("w1"), Count: proto.Int32(1)},
	}

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
	}

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	logic.CalcGameResult()
}

func TestLogic_CalcGameResult_PayLineModeAll(t *testing.T) {

	logic := new(Logic)

	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("p3,p4")},
	}

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(3)},
	}

	logic.PayLineMode = slot.PayLineModeAll
	logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("p5")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p5")},
		},
	}

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	fmt.Println(logic.GameResultManager)

	logic.CalcGameResult()

	for _, row := range logic.GamePayResult {
		fmt.Println(row)
	}
}

func TestLogic_CalcGameResult_PayLineModeFixed(t *testing.T) {

	logic := new(Logic)

	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("p3,p4")},
	}

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(9)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(8)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(7)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(6)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p4"), Count: proto.Int32(3)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(5)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(4)},
		&protodata.SlotPayTable{LinkType: proto.String("right"), Symbol: proto.String("p5"), Count: proto.Int32(3)},
	}

	payline := []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_2,3_1,4_2,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(100003), GroupId: proto.Uint32(1), LineId: proto.Uint32(3), Content: proto.String("1_1,2_2,3_1,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100004), GroupId: proto.Uint32(1), LineId: proto.Uint32(4), Content: proto.String("1_2,2_2,3_2,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(100005), GroupId: proto.Uint32(1), LineId: proto.Uint32(5), Content: proto.String("1_2,2_2,3_2,4_3,5_1")},
	}

	logic.PayLineMode = slot.PayLineModeFixed
	logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, payline)

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p4")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("p2")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p5")},
		},
	}

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	fmt.Println(logic.GameResultManager)

	logic.CalcGameResult()

	for _, result := range logic.GamePayResult {
		fmt.Println(result)
		// fmt.Println(result.Rule)
		// for _, row := range result.SymbolPayLines {
		// 	fmt.Println(row.SymbolIDs)
		// 	fmt.Println(row.PayLine.Origin)
		// 	fmt.Println(row.PayLine.Positions)
		// }
	}
}

func TestLogic_CalcGameAward(t *testing.T) {

	logic := createBasicTestLogic(4)

	fmt.Println(logic.Bet(&gamelogic.GameUser{UserID: 1, Score: 10000}, 100))

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p4")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("p2")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p5")},
		},
	}

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	fmt.Println(logic.GameResultManager)

	logic.CalcGameResult()

	for _, result := range logic.GamePayResult {
		fmt.Println(result)
	}

	logic.CalcGameAward()

	fmt.Println(logic.GameBetMoney, logic.GamePayMoney, logic.GameWinMoney)
}

func TestLogic_CalcGameResult_PayLineModeFixed_UseCase_Jackpot(t *testing.T) {
	logic := new(Logic)

	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	rule1 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule2 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule3 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule4 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule5 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule6 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule7 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule8 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule9 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule10 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule11 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule12 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule13 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule14 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule15 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule16 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule17 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule18 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule19 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule20 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule21 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule22 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule23 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule24 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule25 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule26 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule27 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule28 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(5), PayType: proto.String("jackpot")}
	rule29 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule30 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(3), PayType: proto.String("chip")}

	logic.SlotPayTable = []*protodata.SlotPayTable{rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9, rule10, rule11, rule12, rule13, rule14, rule15, rule16, rule17, rule18, rule19, rule20, rule21, rule22, rule23, rule24, rule25, rule26, rule27, rule28, rule29, rule30}

	line1 := &protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")}
	line2 := &protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_1,3_1,4_1,5_1")}
	line3 := &protodata.SlotPayLine{Id: proto.Uint32(100003), GroupId: proto.Uint32(1), LineId: proto.Uint32(3), Content: proto.String("1_3,2_3,3_3,4_3,5_3")}
	line4 := &protodata.SlotPayLine{Id: proto.Uint32(100004), GroupId: proto.Uint32(1), LineId: proto.Uint32(4), Content: proto.String("1_1,2_2,3_3,4_2,5_1")}
	line5 := &protodata.SlotPayLine{Id: proto.Uint32(100005), GroupId: proto.Uint32(1), LineId: proto.Uint32(5), Content: proto.String("1_3,2_2,3_1,4_2,5_3")}
	line6 := &protodata.SlotPayLine{Id: proto.Uint32(100006), GroupId: proto.Uint32(1), LineId: proto.Uint32(6), Content: proto.String("1_2,2_1,3_1,4_1,5_2")}
	line7 := &protodata.SlotPayLine{Id: proto.Uint32(100007), GroupId: proto.Uint32(1), LineId: proto.Uint32(7), Content: proto.String("1_2,2_3,3_3,4_3,5_2")}
	line8 := &protodata.SlotPayLine{Id: proto.Uint32(100008), GroupId: proto.Uint32(1), LineId: proto.Uint32(8), Content: proto.String("1_1,2_1,3_2,4_3,5_3")}
	line9 := &protodata.SlotPayLine{Id: proto.Uint32(100009), GroupId: proto.Uint32(1), LineId: proto.Uint32(9), Content: proto.String("1_3,2_3,3_2,4_1,5_1")}
	line10 := &protodata.SlotPayLine{Id: proto.Uint32(100010), GroupId: proto.Uint32(1), LineId: proto.Uint32(10), Content: proto.String("1_2,2_1,3_2,4_3,5_2")}
	line11 := &protodata.SlotPayLine{Id: proto.Uint32(100011), GroupId: proto.Uint32(1), LineId: proto.Uint32(11), Content: proto.String("1_2,2_3,3_2,4_1,5_2")}
	line12 := &protodata.SlotPayLine{Id: proto.Uint32(100012), GroupId: proto.Uint32(1), LineId: proto.Uint32(12), Content: proto.String("1_1,2_2,3_2,4_2,5_1")}
	line13 := &protodata.SlotPayLine{Id: proto.Uint32(100013), GroupId: proto.Uint32(1), LineId: proto.Uint32(13), Content: proto.String("1_3,2_2,3_2,4_2,5_3")}
	line14 := &protodata.SlotPayLine{Id: proto.Uint32(100014), GroupId: proto.Uint32(1), LineId: proto.Uint32(14), Content: proto.String("1_1,2_2,3_1,4_2,5_1")}
	line15 := &protodata.SlotPayLine{Id: proto.Uint32(100015), GroupId: proto.Uint32(1), LineId: proto.Uint32(15), Content: proto.String("1_3,2_2,3_3,4_2,5_3")}
	line16 := &protodata.SlotPayLine{Id: proto.Uint32(100016), GroupId: proto.Uint32(1), LineId: proto.Uint32(16), Content: proto.String("1_2,2_2,3_1,4_2,5_2")}
	line17 := &protodata.SlotPayLine{Id: proto.Uint32(100017), GroupId: proto.Uint32(1), LineId: proto.Uint32(17), Content: proto.String("1_2,2_2,3_3,4_2,5_2")}
	line18 := &protodata.SlotPayLine{Id: proto.Uint32(100018), GroupId: proto.Uint32(1), LineId: proto.Uint32(18), Content: proto.String("1_1,2_1,3_3,4_1,5_1")}
	line19 := &protodata.SlotPayLine{Id: proto.Uint32(100019), GroupId: proto.Uint32(1), LineId: proto.Uint32(19), Content: proto.String("1_3,2_3,3_1,4_3,5_3")}
	line20 := &protodata.SlotPayLine{Id: proto.Uint32(100020), GroupId: proto.Uint32(1), LineId: proto.Uint32(20), Content: proto.String("1_1,2_3,3_3,4_3,5_1")}
	line21 := &protodata.SlotPayLine{Id: proto.Uint32(100021), GroupId: proto.Uint32(1), LineId: proto.Uint32(21), Content: proto.String("1_3,2_1,3_1,4_1,5_3")}
	line22 := &protodata.SlotPayLine{Id: proto.Uint32(100022), GroupId: proto.Uint32(1), LineId: proto.Uint32(22), Content: proto.String("1_1,2_1,3_2,4_1,5_1")}
	line23 := &protodata.SlotPayLine{Id: proto.Uint32(100023), GroupId: proto.Uint32(1), LineId: proto.Uint32(23), Content: proto.String("1_3,2_3,3_2,4_3,5_3")}
	line24 := &protodata.SlotPayLine{Id: proto.Uint32(100024), GroupId: proto.Uint32(1), LineId: proto.Uint32(24), Content: proto.String("1_2,2_1,3_2,4_1,5_2")}
	line25 := &protodata.SlotPayLine{Id: proto.Uint32(100025), GroupId: proto.Uint32(1), LineId: proto.Uint32(25), Content: proto.String("1_2,2_3,3_2,4_3,5_2")}

	payline := []*protodata.SlotPayLine{line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12, line13, line14, line15, line16, line17, line18, line19, line20, line21, line22, line23, line24, line25}

	logic.PayLineMode = slot.PayLineModeFixed
	logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, payline)

	result := slot.GameResult{
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

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	logic.CalcGameResult()

	fmt.Println(logic.GameResultManager)

	fmt.Println("Acual")
	for _, row := range logic.GamePayResult {
		fmt.Printf("%+v\n", row)
		for _, line := range row.SymbolPayLines {
			fmt.Printf("%5s %v\n", "", line)
		}
	}

	excepted := []*slot.PayResult{
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   5,
			WayCount:   1,
			SymbolName: "p2",
			Rule:       rule4,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line16,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 7, 3, 9, 10},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   4,
			WayCount:   2,
			SymbolName: "p2",
			Rule:       rule5,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line5,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 7, 3, 9},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line13,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 7, 8, 9},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   3,
			WayCount:   5,
			SymbolName: "p2",
			Rule:       rule6,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line9,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 1},
						},
					},
					SymbolIDs: []int32{11, 12, 8},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line11,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 12, 8},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line19,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 12, 3},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line23,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 12, 8},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line25,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 12, 8},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   5,
			WayCount:   1,
			SymbolName: "p5",
			Rule:       rule13,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line17,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 3},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 7, 13, 9, 10},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   4,
			WayCount:   2,
			SymbolName: "p5",
			Rule:       rule14,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line4,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 1},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 3},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 1},
						},
					},
					SymbolIDs: []int32{1, 7, 13, 9},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line12,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 1},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 1},
						},
					},
					SymbolIDs: []int32{1, 7, 8, 9},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   5,
			WayCount:   1,
			SymbolName: "p9",
			Rule:       rule25,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line10,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 1},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 2, 8, 14, 10},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   3,
			WayCount:   1,
			SymbolName: "p9",
			Rule:       rule27,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line24,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 1},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 2, 8},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   5,
			WayCount:   1,
			SymbolName: "w1",
			Rule:       rule28,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line1,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 7, 8, 9, 10},
				},
			},
		},
	}

	// 依照連線個數做排序，多到少
	sort.Slice(excepted, func(i, j int) bool {
		return excepted[i].PayCount > excepted[j].PayCount
	})

	// for i, l := 0, len(excepted); i < l; i++ {
	// 	if !reflect.DeepEqual(logic.GamePayResult[i], excepted[i]) {
	// 		t.Errorf("PayResult[%d] Not Equal", i)
	// 	}
	// }

	if !reflect.DeepEqual(logic.GamePayResult, excepted) {
		t.Errorf("PayResult Not Equal")
	}

	// fmt.Println()
	// fmt.Println("Expected")
	// for _, row := range excepted {
	// 	fmt.Printf("%+v\n", row)
	// 	for _, line := range row.SymbolPayLines {
	// 		fmt.Printf("%5s %v\n", "", line)
	// 		fmt.Println(line.SymbolIDs)
	// 	}
	// }
}

func TestLogic_CalcGameResult_PayLineModeFixed_UseCase_1(t *testing.T) {
	logic := new(Logic)

	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	rule1 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule2 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule3 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule4 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule5 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule6 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule7 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule8 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule9 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule10 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule11 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule12 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule13 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule14 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule15 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule16 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule17 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule18 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule19 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule20 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule21 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule22 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule23 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule24 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule25 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(5), PayType: proto.String("chip")}
	rule26 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule27 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(3), PayType: proto.String("chip")}
	rule28 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(5), PayType: proto.String("jackpot")}
	rule29 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(4), PayType: proto.String("chip")}
	rule30 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(3), PayType: proto.String("chip")}

	logic.SlotPayTable = []*protodata.SlotPayTable{rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9, rule10, rule11, rule12, rule13, rule14, rule15, rule16, rule17, rule18, rule19, rule20, rule21, rule22, rule23, rule24, rule25, rule26, rule27, rule28, rule29, rule30}

	line1 := &protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")}
	line2 := &protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_1,3_1,4_1,5_1")}
	line3 := &protodata.SlotPayLine{Id: proto.Uint32(100003), GroupId: proto.Uint32(1), LineId: proto.Uint32(3), Content: proto.String("1_3,2_3,3_3,4_3,5_3")}
	line4 := &protodata.SlotPayLine{Id: proto.Uint32(100004), GroupId: proto.Uint32(1), LineId: proto.Uint32(4), Content: proto.String("1_1,2_2,3_3,4_2,5_1")}
	line5 := &protodata.SlotPayLine{Id: proto.Uint32(100005), GroupId: proto.Uint32(1), LineId: proto.Uint32(5), Content: proto.String("1_3,2_2,3_1,4_2,5_3")}
	line6 := &protodata.SlotPayLine{Id: proto.Uint32(100006), GroupId: proto.Uint32(1), LineId: proto.Uint32(6), Content: proto.String("1_2,2_1,3_1,4_1,5_2")}
	line7 := &protodata.SlotPayLine{Id: proto.Uint32(100007), GroupId: proto.Uint32(1), LineId: proto.Uint32(7), Content: proto.String("1_2,2_3,3_3,4_3,5_2")}
	line8 := &protodata.SlotPayLine{Id: proto.Uint32(100008), GroupId: proto.Uint32(1), LineId: proto.Uint32(8), Content: proto.String("1_1,2_1,3_2,4_3,5_3")}
	line9 := &protodata.SlotPayLine{Id: proto.Uint32(100009), GroupId: proto.Uint32(1), LineId: proto.Uint32(9), Content: proto.String("1_3,2_3,3_2,4_1,5_1")}
	line10 := &protodata.SlotPayLine{Id: proto.Uint32(100010), GroupId: proto.Uint32(1), LineId: proto.Uint32(10), Content: proto.String("1_2,2_1,3_2,4_3,5_2")}
	line11 := &protodata.SlotPayLine{Id: proto.Uint32(100011), GroupId: proto.Uint32(1), LineId: proto.Uint32(11), Content: proto.String("1_2,2_3,3_2,4_1,5_2")}
	line12 := &protodata.SlotPayLine{Id: proto.Uint32(100012), GroupId: proto.Uint32(1), LineId: proto.Uint32(12), Content: proto.String("1_1,2_2,3_2,4_2,5_1")}
	line13 := &protodata.SlotPayLine{Id: proto.Uint32(100013), GroupId: proto.Uint32(1), LineId: proto.Uint32(13), Content: proto.String("1_3,2_2,3_2,4_2,5_3")}
	line14 := &protodata.SlotPayLine{Id: proto.Uint32(100014), GroupId: proto.Uint32(1), LineId: proto.Uint32(14), Content: proto.String("1_1,2_2,3_1,4_2,5_1")}
	line15 := &protodata.SlotPayLine{Id: proto.Uint32(100015), GroupId: proto.Uint32(1), LineId: proto.Uint32(15), Content: proto.String("1_3,2_2,3_3,4_2,5_3")}
	line16 := &protodata.SlotPayLine{Id: proto.Uint32(100016), GroupId: proto.Uint32(1), LineId: proto.Uint32(16), Content: proto.String("1_2,2_2,3_1,4_2,5_2")}
	line17 := &protodata.SlotPayLine{Id: proto.Uint32(100017), GroupId: proto.Uint32(1), LineId: proto.Uint32(17), Content: proto.String("1_2,2_2,3_3,4_2,5_2")}
	line18 := &protodata.SlotPayLine{Id: proto.Uint32(100018), GroupId: proto.Uint32(1), LineId: proto.Uint32(18), Content: proto.String("1_1,2_1,3_3,4_1,5_1")}
	line19 := &protodata.SlotPayLine{Id: proto.Uint32(100019), GroupId: proto.Uint32(1), LineId: proto.Uint32(19), Content: proto.String("1_3,2_3,3_1,4_3,5_3")}
	line20 := &protodata.SlotPayLine{Id: proto.Uint32(100020), GroupId: proto.Uint32(1), LineId: proto.Uint32(20), Content: proto.String("1_1,2_3,3_3,4_3,5_1")}
	line21 := &protodata.SlotPayLine{Id: proto.Uint32(100021), GroupId: proto.Uint32(1), LineId: proto.Uint32(21), Content: proto.String("1_3,2_1,3_1,4_1,5_3")}
	line22 := &protodata.SlotPayLine{Id: proto.Uint32(100022), GroupId: proto.Uint32(1), LineId: proto.Uint32(22), Content: proto.String("1_1,2_1,3_2,4_1,5_1")}
	line23 := &protodata.SlotPayLine{Id: proto.Uint32(100023), GroupId: proto.Uint32(1), LineId: proto.Uint32(23), Content: proto.String("1_3,2_3,3_2,4_3,5_3")}
	line24 := &protodata.SlotPayLine{Id: proto.Uint32(100024), GroupId: proto.Uint32(1), LineId: proto.Uint32(24), Content: proto.String("1_2,2_1,3_2,4_1,5_2")}
	line25 := &protodata.SlotPayLine{Id: proto.Uint32(100025), GroupId: proto.Uint32(1), LineId: proto.Uint32(25), Content: proto.String("1_2,2_3,3_2,4_3,5_2")}

	payline := []*protodata.SlotPayLine{line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12, line13, line14, line15, line16, line17, line18, line19, line20, line21, line22, line23, line24, line25}

	logic.PayLineMode = slot.PayLineModeFixed
	logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, payline)

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("p8")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("p6")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("p9")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("p4")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("p1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("p2")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("p3")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("p5")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p2")},
		},
	}

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	logic.CalcGameResult()

	// excepted := []*PayResult{
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   5,
	// 		WayCount:   1,
	// 		SymbolName: "p2",
	// 		Rule:       rule4,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line16,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 1},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 7, 3, 9, 10},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   4,
	// 		WayCount:   2,
	// 		SymbolName: "p2",
	// 		Rule:       rule5,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line5,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 3},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 1},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 3},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{11, 7, 3, 9},
	// 			},
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line13,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 3},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 3},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{11, 7, 8, 9},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   3,
	// 		WayCount:   5,
	// 		SymbolName: "p2",
	// 		Rule:       rule6,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line9,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 3},
	// 						Position{Reel: 2, Row: 3},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 1},
	// 						Position{Reel: 5, Row: 1},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{11, 12, 8},
	// 			},
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line11,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 3},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 1},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 12, 8},
	// 			},
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line19,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 3},
	// 						Position{Reel: 2, Row: 3},
	// 						Position{Reel: 3, Row: 1},
	// 						Position{Reel: 4, Row: 3},
	// 						Position{Reel: 5, Row: 3},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{11, 12, 3},
	// 			},
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line23,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 3},
	// 						Position{Reel: 2, Row: 3},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 3},
	// 						Position{Reel: 5, Row: 3},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{11, 12, 8},
	// 			},
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line25,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 3},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 3},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 12, 8},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   5,
	// 		WayCount:   1,
	// 		SymbolName: "p5",
	// 		Rule:       rule13,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line17,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 3},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 7, 13, 9, 10},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   4,
	// 		WayCount:   2,
	// 		SymbolName: "p5",
	// 		Rule:       rule14,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line4,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 1},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 3},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 1},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{1, 7, 13, 9},
	// 			},
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line12,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 1},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 1},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{1, 7, 8, 9},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   5,
	// 		WayCount:   1,
	// 		SymbolName: "p9",
	// 		Rule:       rule25,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line10,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 1},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 3},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 2, 8, 14, 10},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   3,
	// 		WayCount:   1,
	// 		SymbolName: "p9",
	// 		Rule:       rule27,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line24,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 1},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 1},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 2, 8},
	// 			},
	// 		},
	// 	},
	// 	&PayResult{
	// 		IsMatch:    true,
	// 		PayCount:   5,
	// 		WayCount:   1,
	// 		SymbolName: "w1",
	// 		Rule:       rule28,
	// 		SymbolPayLines: []*SymbolPayLine{
	// 			&SymbolPayLine{
	// 				PayLine: &PayLine{
	// 					Origin: line1,
	// 					Positions: []Position{
	// 						Position{Reel: 1, Row: 2},
	// 						Position{Reel: 2, Row: 2},
	// 						Position{Reel: 3, Row: 2},
	// 						Position{Reel: 4, Row: 2},
	// 						Position{Reel: 5, Row: 2},
	// 					},
	// 				},
	// 				SymbolIDs: []int32{6, 7, 8, 9, 10},
	// 			},
	// 		},
	// 	},
	// }

	for _, row := range logic.GamePayResult {
		fmt.Println(row)
	}

	data := new(netproto.TGW_MSG_GameResult)

	// 中獎內容，賠付線結果(派彩線)
	for _, row := range logic.GamePayResult {
		for _, line := range row.SymbolPayLines {
			paygroup := new(netproto.TGW_PayGroup)
			payline := new(netproto.TGW_PayLine)

			payline.PayLineID = proto.Int32(int32(line.PayLine.Origin.GetLineId()))
			for _, orgpos := range line.PayLine.Positions {
				pos := new(netproto.TGW_ReelPosition)
				pos.Reel = proto.Int32(orgpos.Reel)
				pos.Row = proto.Int32(orgpos.Row)
				payline.Positions = append(payline.Positions, pos)
			}

			paygroup.PayLines = payline
			data.PayGroups = append(data.PayGroups, paygroup)
		}
	}

	for _, row := range data.PayGroups {
		fmt.Println(row)
	}

	// if !reflect.DeepEqual(logic.GamePayResult, excepted) {
	// 	t.Errorf("PayResult Not Equal")
	// }

}

func TestLogic_CalcGameResult_PayLineModeFixed_UseCase_2(t *testing.T) {
	logic := new(Logic)

	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	rule1 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(5), PayType: proto.String("jackpot"), Payout: proto.String("5")}
	rule2 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("500")}
	rule3 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("55")}
	rule4 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(2), PayType: proto.String("chip"), Payout: proto.String("10")}
	rule5 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("1000")}
	rule6 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("300")}
	rule7 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("45")}
	rule8 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(2), PayType: proto.String("chip"), Payout: proto.String("5")}
	rule9 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("750")}
	rule10 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("200")}
	rule11 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("35")}
	rule12 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("500")}
	rule13 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("100")}
	rule14 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("25")}
	rule15 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("250")}
	rule16 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("75")}
	rule17 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("15")}
	rule18 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("100")}
	rule19 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("40")}
	rule20 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("10")}
	rule21 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")}
	rule22 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")}
	rule23 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("10")}
	rule24 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")}
	rule25 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")}
	rule26 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")}
	rule27 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")}
	rule28 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")}
	rule29 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")}
	rule30 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")}
	rule31 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")}
	rule32 := &protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")}
	rule33 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(9), PayType: proto.String("pickgame"), Payout: proto.String("209")}
	rule34 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(8), PayType: proto.String("pickgame"), Payout: proto.String("208")}
	rule35 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(7), PayType: proto.String("pickgame"), Payout: proto.String("207")}
	rule36 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(6), PayType: proto.String("pickgame"), Payout: proto.String("206")}
	rule37 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(5), PayType: proto.String("pickgame"), Payout: proto.String("205")}
	rule38 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(4), PayType: proto.String("pickgame"), Payout: proto.String("204")}
	rule39 := &protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(3), PayType: proto.String("pickgame"), Payout: proto.String("203")}

	logic.SlotPayTable = []*protodata.SlotPayTable{rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9, rule10, rule11, rule12, rule13, rule14, rule15, rule16, rule17, rule18, rule19, rule20, rule21, rule22, rule23, rule24, rule25, rule26, rule27, rule28, rule29, rule30, rule31, rule32, rule33, rule34, rule35, rule36, rule37, rule38, rule39}

	line1 := &protodata.SlotPayLine{Id: proto.Uint32(100001), GroupId: proto.Uint32(1), LineId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")}
	line2 := &protodata.SlotPayLine{Id: proto.Uint32(100002), GroupId: proto.Uint32(1), LineId: proto.Uint32(2), Content: proto.String("1_1,2_1,3_1,4_1,5_1")}
	line3 := &protodata.SlotPayLine{Id: proto.Uint32(100003), GroupId: proto.Uint32(1), LineId: proto.Uint32(3), Content: proto.String("1_3,2_3,3_3,4_3,5_3")}
	line4 := &protodata.SlotPayLine{Id: proto.Uint32(100004), GroupId: proto.Uint32(1), LineId: proto.Uint32(4), Content: proto.String("1_1,2_2,3_3,4_2,5_1")}
	line5 := &protodata.SlotPayLine{Id: proto.Uint32(100005), GroupId: proto.Uint32(1), LineId: proto.Uint32(5), Content: proto.String("1_3,2_2,3_1,4_2,5_3")}
	line6 := &protodata.SlotPayLine{Id: proto.Uint32(100006), GroupId: proto.Uint32(1), LineId: proto.Uint32(6), Content: proto.String("1_2,2_1,3_1,4_1,5_2")}
	line7 := &protodata.SlotPayLine{Id: proto.Uint32(100007), GroupId: proto.Uint32(1), LineId: proto.Uint32(7), Content: proto.String("1_2,2_3,3_3,4_3,5_2")}
	line8 := &protodata.SlotPayLine{Id: proto.Uint32(100008), GroupId: proto.Uint32(1), LineId: proto.Uint32(8), Content: proto.String("1_1,2_1,3_2,4_3,5_3")}
	line9 := &protodata.SlotPayLine{Id: proto.Uint32(100009), GroupId: proto.Uint32(1), LineId: proto.Uint32(9), Content: proto.String("1_3,2_3,3_2,4_1,5_1")}
	line10 := &protodata.SlotPayLine{Id: proto.Uint32(100010), GroupId: proto.Uint32(1), LineId: proto.Uint32(10), Content: proto.String("1_2,2_1,3_2,4_3,5_2")}
	line11 := &protodata.SlotPayLine{Id: proto.Uint32(100011), GroupId: proto.Uint32(1), LineId: proto.Uint32(11), Content: proto.String("1_2,2_3,3_2,4_1,5_2")}
	line12 := &protodata.SlotPayLine{Id: proto.Uint32(100012), GroupId: proto.Uint32(1), LineId: proto.Uint32(12), Content: proto.String("1_1,2_2,3_2,4_2,5_1")}
	line13 := &protodata.SlotPayLine{Id: proto.Uint32(100013), GroupId: proto.Uint32(1), LineId: proto.Uint32(13), Content: proto.String("1_3,2_2,3_2,4_2,5_3")}
	line14 := &protodata.SlotPayLine{Id: proto.Uint32(100014), GroupId: proto.Uint32(1), LineId: proto.Uint32(14), Content: proto.String("1_1,2_2,3_1,4_2,5_1")}
	line15 := &protodata.SlotPayLine{Id: proto.Uint32(100015), GroupId: proto.Uint32(1), LineId: proto.Uint32(15), Content: proto.String("1_3,2_2,3_3,4_2,5_3")}
	line16 := &protodata.SlotPayLine{Id: proto.Uint32(100016), GroupId: proto.Uint32(1), LineId: proto.Uint32(16), Content: proto.String("1_2,2_2,3_1,4_2,5_2")}
	line17 := &protodata.SlotPayLine{Id: proto.Uint32(100017), GroupId: proto.Uint32(1), LineId: proto.Uint32(17), Content: proto.String("1_2,2_2,3_3,4_2,5_2")}
	line18 := &protodata.SlotPayLine{Id: proto.Uint32(100018), GroupId: proto.Uint32(1), LineId: proto.Uint32(18), Content: proto.String("1_1,2_1,3_3,4_1,5_1")}
	line19 := &protodata.SlotPayLine{Id: proto.Uint32(100019), GroupId: proto.Uint32(1), LineId: proto.Uint32(19), Content: proto.String("1_3,2_3,3_1,4_3,5_3")}
	line20 := &protodata.SlotPayLine{Id: proto.Uint32(100020), GroupId: proto.Uint32(1), LineId: proto.Uint32(20), Content: proto.String("1_1,2_3,3_3,4_3,5_1")}
	line21 := &protodata.SlotPayLine{Id: proto.Uint32(100021), GroupId: proto.Uint32(1), LineId: proto.Uint32(21), Content: proto.String("1_3,2_1,3_1,4_1,5_3")}
	line22 := &protodata.SlotPayLine{Id: proto.Uint32(100022), GroupId: proto.Uint32(1), LineId: proto.Uint32(22), Content: proto.String("1_1,2_1,3_2,4_1,5_1")}
	line23 := &protodata.SlotPayLine{Id: proto.Uint32(100023), GroupId: proto.Uint32(1), LineId: proto.Uint32(23), Content: proto.String("1_3,2_3,3_2,4_3,5_3")}
	line24 := &protodata.SlotPayLine{Id: proto.Uint32(100024), GroupId: proto.Uint32(1), LineId: proto.Uint32(24), Content: proto.String("1_2,2_1,3_2,4_1,5_2")}
	line25 := &protodata.SlotPayLine{Id: proto.Uint32(100025), GroupId: proto.Uint32(1), LineId: proto.Uint32(25), Content: proto.String("1_2,2_3,3_2,4_3,5_2")}

	payline := []*protodata.SlotPayLine{line1, line2, line3, line4, line5, line6, line7, line8, line9, line10, line11, line12, line13, line14, line15, line16, line17, line18, line19, line20, line21, line22, line23, line24, line25}

	logic.PayLineMode = slot.PayLineModeFixed
	logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, payline)

	result := slot.GameResult{
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(1), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(2), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(3), Symbol: proto.String("b1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(4), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(5), Symbol: proto.String("b1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(6), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(7), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(8), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(9), Symbol: proto.String("w1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(10), Symbol: proto.String("w1")},
		},
		[]*protodata.SlotSymbolDistribution{
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(11), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(12), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(13), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(14), Symbol: proto.String("p1")},
			&protodata.SlotSymbolDistribution{Id: proto.Uint32(15), Symbol: proto.String("p1")},
		},
	}

	logic.GameResultManager, _ = slot.NewGameResultManager(5, 3)
	logic.GameResultManager.GameResult = result

	logic.CalcGameResult()

	logic.GameResultManager.RowMode()
	fmt.Println(logic.GameResultManager)

	fmt.Println("Acual")
	for _, row := range logic.GamePayResult {
		fmt.Printf("%+v\n", row.Rule)
		for _, line := range row.SymbolPayLines {
			fmt.Printf("%5s %v\n", "", line)
		}
	}

	data := new(netproto.TGW_MSG_GameResult)

	// 中獎內容，賠付線結果(派彩線)
	for _, row := range logic.GamePayResult {
		for _, line := range row.SymbolPayLines {
			paygroup := new(netproto.TGW_PayGroup)
			payline := new(netproto.TGW_PayLine)

			payline.PayLineID = proto.Int32(int32(line.PayLine.Origin.GetLineId()))
			for _, orgpos := range line.PayLine.Positions {
				pos := new(netproto.TGW_ReelPosition)
				pos.Reel = proto.Int32(orgpos.Reel)
				pos.Row = proto.Int32(orgpos.Row)
				payline.Positions = append(payline.Positions, pos)
			}

			paygroup.PayLines = payline
			data.PayGroups = append(data.PayGroups, paygroup)
		}
	}

	excepted := []*slot.PayResult{
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   5,
			WayCount:   1,
			SymbolName: "w1",
			Rule:       rule1,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line1,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 7, 8, 9, 10},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   5,
			WayCount:   10,
			SymbolName: "p1",
			Rule:       rule5,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line3,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 3},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 12, 13, 14, 15},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line7,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 3},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 12, 13, 14, 10},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line10,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 1},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 2, 8, 14, 10},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line11,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 12, 8, 4, 10},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line13,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 7, 8, 9, 15},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line15,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 3},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 7, 13, 9, 15},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line17,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 3},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 7, 13, 9, 10},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line23,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 12, 8, 14, 15},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line24,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 1},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 2, 8, 4, 10},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line25,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 12, 8, 14, 10},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   4,
			WayCount:   1,
			SymbolName: "p1",
			Rule:       rule6,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line9,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 2},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 1},
						},
					},
					SymbolIDs: []int32{11, 12, 8, 4},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   2,
			WayCount:   1,
			SymbolName: "w1",
			Rule:       rule4,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line16,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 7},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			PayCount:   2,
			WayCount:   4,
			SymbolName: "p1",
			Rule:       rule8,
			SymbolPayLines: []*slot.SymbolPayLine{
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line5,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 2},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 2},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 7},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line6,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 2},
							slot.Position{Reel: 2, Row: 1},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 2},
						},
					},
					SymbolIDs: []int32{6, 2},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line19,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 3},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 3},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 12},
				},
				&slot.SymbolPayLine{
					PayLine: &slot.PayLine{
						Origin: line21,
						Positions: []slot.Position{
							slot.Position{Reel: 1, Row: 3},
							slot.Position{Reel: 2, Row: 1},
							slot.Position{Reel: 3, Row: 1},
							slot.Position{Reel: 4, Row: 1},
							slot.Position{Reel: 5, Row: 3},
						},
					},
					SymbolIDs: []int32{11, 2},
				},
			},
		},
		&slot.PayResult{
			IsMatch:    true,
			SymbolName: "b1",
			Rule:       rule39,
			WayCount:   1,
			PayCount:   3,
			SymbolIDs:  []int32{1, 3, 5},
		},
	}

	sort.Slice(excepted, func(i, j int) bool {
		return excepted[i].PayCount > excepted[j].PayCount
	})

	if !reflect.DeepEqual(logic.GamePayResult, excepted) {
		t.Errorf("PayResult Not Equal want %v, but %v", excepted, logic.GamePayResult)
	}

	fmt.Println()
	fmt.Println("Expected")
	for _, row := range excepted {
		fmt.Printf("%+v\n", row.Rule)
		for _, line := range row.SymbolPayLines {
			fmt.Printf("%5s %v\n", "", line)
		}
	}
}
