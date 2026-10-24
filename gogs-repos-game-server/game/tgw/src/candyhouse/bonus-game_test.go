package candyhouse

import (
	"fmt"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot/protodata"
)

func TestBonusGame_RandomBonus(t *testing.T) {

	bonusTbs := []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("25"), PerProb: proto.Int32(70000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("30"), PerProb: proto.Int32(50000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("45"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("50"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("65"), PerProb: proto.Int32(7000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("70"), PerProb: proto.Int32(5000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("85"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("100"), PerProb: proto.Int32(1000)},
	}

	game := new(BonusGame)
	game.BonusTable = bonusTbs
	game.GameCount = 3

	for i := 0; i < 50; i++ {
		fmt.Println(game.randomBonus())
	}
}

func TestBonusGame_GenerateProcess(t *testing.T) {

	bonusTbs := []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("25"), PerProb: proto.Int32(70000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("30"), PerProb: proto.Int32(50000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("45"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("50"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("65"), PerProb: proto.Int32(7000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("70"), PerProb: proto.Int32(5000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("85"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("100"), PerProb: proto.Int32(1000)},
	}

	game := new(BonusGame)
	game.BonusTable = bonusTbs
	game.GameCount = 5
	game.GameBetMoney = 10

	for key, row := range game.generateProcess() {
		fmt.Println(key, row)
	}

}

func TestNewBonusGame(t *testing.T) {

	bonusTbs := []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("25"), PerProb: proto.Int32(70000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("30"), PerProb: proto.Int32(50000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("45"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("50"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("65"), PerProb: proto.Int32(7000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("70"), PerProb: proto.Int32(5000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("85"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("100"), PerProb: proto.Int32(1000)},
	}

	game := NewBonusGame(bonusTbs, 12, 5, 10)

	for key, row := range game.Process {
		fmt.Println(key, row)
	}

	fmt.Println(game)
}

func TestBonusGame_Choose(t *testing.T) {
	game := new(BonusGame)
	game.GameCount = 3
	game.GameBetMoney = 10
	game.WinMoney = 30
	game.Process = make(map[int32]*BonusGameDetail)

	origin := &protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100000)}

	for i := 0; i < 3; i++ {
		detail := new(BonusGameDetail)
		detail.origin = origin
		detail.ID = int32(i + 1)
		detail.Money = 100
		game.Process[detail.ID] = detail
	}

	game.Choose(1, 2)
	if game.Process[1].Location != 2 {
		t.Errorf("ID: %d, Location want %d, but %d", 1, 2, game.Process[1].Location)
	}
	game.Choose(2, 6)
	if game.Process[2].Location != 6 {
		t.Errorf("ID: %d, Location want %d, but %d", 2, 6, game.Process[2].Location)
	}
	game.Choose(3, 1)
	if game.Process[3].Location != 1 {
		t.Errorf("ID: %d, Location want %d, but %d", 3, 1, game.Process[3].Location)
	}
	game.Choose(3, 8)
	if game.Process[3].Location != 1 {
		t.Errorf("ID: %d, Location want %d, but %d", 3, 1, game.Process[3].Location)
	}
}

func TestBonusGame_IsFinished(t *testing.T) {
	game := new(BonusGame)
	game.GameCount = 3
	game.GameBetMoney = 10
	game.WinMoney = 30
	game.Process = make(map[int32]*BonusGameDetail)

	origin := &protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100000)}

	for i := 0; i < 3; i++ {
		detail := new(BonusGameDetail)
		detail.origin = origin
		detail.ID = int32(i + 1)
		detail.Money = 100
		game.Process[detail.ID] = detail
	}

	if game.IsFinished() {
		t.Errorf("Play 0 times, should not Finished")
	}
	game.Choose(1, 2)
	if game.IsFinished() {
		t.Errorf("Play 1 times, should not Finished")
	}
	game.Choose(2, 6)
	if game.IsFinished() {
		t.Errorf("Play 2 times, should not Finished")
	}
	game.Choose(3, 1)
	if !game.IsFinished() {
		t.Errorf("Play 3 times, should Finished")
	}

}

func TestBonusGame_GetProcessData(t *testing.T) {
	bonusTbs := []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("10"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("25"), PerProb: proto.Int32(70000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("30"), PerProb: proto.Int32(50000)},
		&protodata.BonusTable{Id: proto.Int32(4), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("45"), PerProb: proto.Int32(30000)},
		&protodata.BonusTable{Id: proto.Int32(5), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("50"), PerProb: proto.Int32(10000)},
		&protodata.BonusTable{Id: proto.Int32(6), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("65"), PerProb: proto.Int32(7000)},
		&protodata.BonusTable{Id: proto.Int32(7), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("70"), PerProb: proto.Int32(5000)},
		&protodata.BonusTable{Id: proto.Int32(8), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("85"), PerProb: proto.Int32(3000)},
		&protodata.BonusTable{Id: proto.Int32(9), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("100"), PerProb: proto.Int32(1000)},
	}

	game := NewBonusGame(bonusTbs, 12, 5, 10)

	fmt.Println(game.GetProcessData())
}

func TestBonusGame_Exist(t *testing.T) {

	game := new(BonusGame)
	game.Process = make(map[int32]*BonusGameDetail)

	fmt.Println(game.Exist(0))
	fmt.Println(game.Exist(1))
	fmt.Println(game.Exist(2))
}
