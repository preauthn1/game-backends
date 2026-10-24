package candyhouse

import (
	"fmt"
	"gamelogic"
	"msg"
	"netproto"
	"reflect"
	"testing"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
)

func TestLogic_SendSettingMessage(t *testing.T) {

	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.PayLineMode = slot.PayLineModeFixed
	logic.BetCounts = []int32{10, 20, 30, 60, 100, 200, 300, 600, 1000}
	logic.BigWinRule = []int32{300, 400, 500, 500000}
	logic.DefaultRowNum = 3
	logic.MinCarryMoney = 5000

	logic.SlotPayLine = []*protodata.SlotPayLine{
		&protodata.SlotPayLine{Id: proto.Uint32(1), LineId: proto.Uint32(1), GroupId: proto.Uint32(1), Content: proto.String("1_2,2_2,3_2,4_2,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(2), LineId: proto.Uint32(2), GroupId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_1,4_1,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(3), LineId: proto.Uint32(3), GroupId: proto.Uint32(1), Content: proto.String("1_3,2_3,3_3,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(4), LineId: proto.Uint32(4), GroupId: proto.Uint32(1), Content: proto.String("1_1,2_2,3_3,4_2,5_1")},
		&protodata.SlotPayLine{Id: proto.Uint32(5), LineId: proto.Uint32(5), GroupId: proto.Uint32(1), Content: proto.String("1_3,2_2,3_1,4_2,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(6), LineId: proto.Uint32(6), GroupId: proto.Uint32(1), Content: proto.String("1_2,2_1,3_1,4_1,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(7), LineId: proto.Uint32(7), GroupId: proto.Uint32(1), Content: proto.String("1_2,2_3,3_3,4_3,5_2")},
		&protodata.SlotPayLine{Id: proto.Uint32(8), LineId: proto.Uint32(8), GroupId: proto.Uint32(1), Content: proto.String("1_1,2_1,3_2,4_3,5_3")},
		&protodata.SlotPayLine{Id: proto.Uint32(9), LineId: proto.Uint32(9), GroupId: proto.Uint32(1), Content: proto.String("1_3,2_3,3_2,4_1,5_1")},
	}

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(5), PayType: proto.String("jackpot"), Payout: proto.String("1")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(2), PayType: proto.String("chip"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(9), PayType: proto.String("pickgame"), Payout: proto.String("1")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(8), PayType: proto.String("pickgame"), Payout: proto.String("2")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(7), PayType: proto.String("pickgame"), Payout: proto.String("3")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(6), PayType: proto.String("pickgame"), Payout: proto.String("4")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(5), PayType: proto.String("pickgame"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(4), PayType: proto.String("pickgame"), Payout: proto.String("6")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(3), PayType: proto.String("pickgame"), Payout: proto.String("7")},
	}

	logic.PickGameTable = []*protodata.PickGameTable{
		&protodata.PickGameTable{Id: proto.Uint32(1), GroupId: proto.Int32(999), PickCount: proto.Uint32(9), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(2), GroupId: proto.Int32(999), PickCount: proto.Uint32(8), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(3), GroupId: proto.Int32(999), PickCount: proto.Uint32(7), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(4), GroupId: proto.Int32(999), PickCount: proto.Uint32(6), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(5), GroupId: proto.Int32(999), PickCount: proto.Uint32(5), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(6), GroupId: proto.Int32(999), PickCount: proto.Uint32(4), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
		&protodata.PickGameTable{Id: proto.Uint32(7), GroupId: proto.Int32(999), PickCount: proto.Uint32(3), RewardValue: proto.Uint32(1), IsGhost: proto.Uint32(0)},
	}

	logic.JackpotTable = []*protodata.JackpotTable{
		&protodata.JackpotTable{Id: proto.Int32(1), GroupId: proto.Int32(1), IsProgress: proto.Int32(1), IsLinked: proto.Int32(0), Increment: proto.Int32(100), StartUp: proto.Int64(10000000), IsMultiLevel: proto.Int32(0), IsCeling: proto.Int32(0)},
	}

	logic.initJackpot()

	logic.SymbolsDesc = map[string]*SymbolDesc{
		"w1": &SymbolDesc{Name: "w1", Title: "w1-Title", Description: "w1-Desc"},
		"b1": &SymbolDesc{Name: "b1", Title: "b1-Title", Description: "b1-Desc"},
	}

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.TGW_GameMessageClassID_TGW_SettingID) {
			t.Errorf("SClassID want %d, but %d", netproto.TGW_GameMessageClassID_TGW_SettingID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.TGW_MSG_Setting)
		if !ok {
			t.Errorf("MsgData want type *netproto.TGW_MSG_Setting")
		}

		if !reflect.DeepEqual(data.GetBetCount(), logic.BetCounts) {
			t.Errorf("BetCount want %v, but, %v", logic.BetCounts, data.GetBetCount())
		}

		if !reflect.DeepEqual(data.GetBigWinStep(), logic.BigWinRule) {
			t.Errorf("BigWinStep want %v, but, %v", logic.BigWinRule, data.GetBigWinStep())
		}

		if !reflect.DeepEqual(data.GetPayLineMode(), logic.PayLineMode.Int()) {
			t.Errorf("PayLineMode want %v, but, %v", logic.PayLineMode.Int(), data.GetPayLineMode())
		}

		if !reflect.DeepEqual(data.GetRowCounts(), logic.DefaultRowNum) {
			t.Errorf("RowCounts want %v, but, %v", logic.DefaultRowNum, data.GetRowCounts())
		}

		if data.GetTotalPayLine() != int32(len(logic.SlotPayLine)) {
			t.Errorf("PayLines length want %v, but, %v", len(logic.SlotPayLine), len(data.PayLines))
		}

		if len(data.PayLines) != len(logic.SlotPayLine) {
			t.Errorf("PayLines length want %v, but, %v", len(logic.SlotPayLine), len(data.PayLines))
		}

		if data.GetMinCarryMoney() != logic.MinCarryMoney {
			t.Errorf("MinCarryMoney want %v, but, %v", logic.MinCarryMoney, data.GetMinCarryMoney())
		}

		for _, row := range data.SymbolPayTable {
			fmt.Println(row)
		}

	}

	user := new(gamelogic.GameUser)
	user.UserID = 9936738

	logic.SendSettingMessage(user)
}

func TestLogic_SendBonusGameMessage(t *testing.T) {

	game := new(BonusGame)
	game.BonusTable = []*protodata.BonusTable{
		&protodata.BonusTable{Id: proto.Int32(1), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("2"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(2), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("3"), PerProb: proto.Int32(100000)},
		&protodata.BonusTable{Id: proto.Int32(3), GroupId: proto.Int32(1), RewardType: proto.String("chip"), RewardValue: proto.String("4"), PerProb: proto.Int32(100000)},
	}
	game.Size = 12
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

	game.All = game.generateLaftProcess(game.Size)

	game.Choose(1, 3)

	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(nil, nil, nil)
	logic.BonusGame = game
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.TGW_GameMessageClassID_TGW_BonusGameID) {
			t.Errorf("SClassID want %d, but %d", netproto.TGW_GameMessageClassID_TGW_BonusGameID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.TGW_MSG_BonusGame)
		if !ok {
			t.Errorf("MsgData want type *netproto.TGW_MSG_BonusGame")
		}

		if data.GetWinMoney() != game.WinMoney {
			t.Errorf("TGW_MSG_BonusGame.WinMoney want %d, but %d", game.WinMoney, data.GetWinMoney())
		}
		if data.GetCount() != game.GameCount {
			t.Errorf("TGW_MSG_BonusGame.Count want %d, but %d", game.GameCount, data.GetCount())
		}
		if len(data.Records) != len(game.Process) {
			t.Errorf("TGW_MSG_BonusGame.Records size want %d, but %d", len(game.Process), len(data.Records))
		}
		if len(data.All) != len(game.All) {
			t.Errorf("TGW_MSG_BonusGame.All size want %d, but %d", len(game.All), len(data.All))
		}
		if data.Records[0].GetID() != 1 {
			t.Errorf("TGW_MSG_BonusGame.Records[0].ID want %d, but %d", 1, data.Records[0].GetID())
		}
		if data.Records[0].GetLocation() != 3 {
			t.Errorf("TGW_MSG_BonusGame.Records[0].Location want %d, but %d", 3, data.Records[0].GetLocation())
		}
		if data.Records[0].GetRewareValue() != 10 {
			t.Errorf("TGW_MSG_BonusGame.Records[0].RewareValue want %d, but %d", 10, data.Records[0].GetRewareValue())
		}
		if data.Records[1].GetID() != 2 {
			t.Errorf("TGW_MSG_BonusGame.Records[1].ID want %d, but %d", 2, data.Records[1].GetID())
		}
		if data.Records[1].GetLocation() != 0 {
			t.Errorf("TGW_MSG_BonusGame.Records[1].Location want %d, but %d", 0, data.Records[1].GetLocation())
		}
		if data.Records[1].GetRewareValue() != 10 {
			t.Errorf("TGW_MSG_BonusGame.Records[1].RewareValue want %d, but %d", 10, data.Records[1].GetRewareValue())
		}
		if data.Records[2].GetID() != 3 {
			t.Errorf("TGW_MSG_BonusGame.Records[2].ID want %d, but %d", 3, data.Records[2].GetID())
		}
		if data.Records[2].GetLocation() != 0 {
			t.Errorf("TGW_MSG_BonusGame.Records[2].Location want %d, but %d", 0, data.Records[2].GetLocation())
		}
		if data.Records[2].GetRewareValue() != 10 {
			t.Errorf("TGW_MSG_BonusGame.Records[2].RewareValue want %d, but %d", 10, data.Records[2].GetRewareValue())
		}
	}

	user := new(gamelogic.GameUser)
	user.UserID = 9936738

	logic.SendBonusGameMessage(user)
}

func TestLogic_SendGameResultMessage(t *testing.T) {
	logic := createBasicTestLogic(2)

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.BClassID != int32(netproto.MessageBClassID_Game) {
			t.Errorf("BClassID want %d, but %d", netproto.MessageBClassID_Game, m.BClassID)
		}
		if m.SClassID != int32(netproto.TGW_GameMessageClassID_TGW_GameResultID) {
			t.Errorf("SClassID want %d, but %d", netproto.TGW_GameMessageClassID_TGW_GameResultID, m.SClassID)
		}

		data, ok := m.MsgData.(*netproto.TGW_MSG_GameResult)
		if !ok {
			t.Errorf("MsgData want type *netproto.TGW_MSG_GameResult")
		}

		for _, row := range data.GetReelResult() {
			fmt.Println(row)
		}

		for _, row := range data.GetPayGroups() {
			fmt.Println(row)
		}

	}

	fmt.Println(logic.Bet(logic.GameUser, 10))

	// 產生遊戲結果
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

	// 計算遊戲結果
	logic.CalcGameResult()

	for _, row := range logic.GamePayResult {
		fmt.Printf("%+v\n", row.Rule)
		for _, line := range row.SymbolPayLines {
			fmt.Printf("%5s %v\n", "", line)
		}
	}

	// 計算遊戲獎勵
	logic.CalcGameAward()

	logic.SendGameResultMessage(logic.GameUser)
}

func TestLogic_SendGameResultMessage_UseCase_2(t *testing.T) {
	logic := createBasicTestLogic(4)

	logic.SymbolAttributes = []*protodata.SymbolAttributes{
		&protodata.SymbolAttributes{Symbol: proto.String("w1"), IsWild: proto.Int32(1), WildEx: proto.String("")},
	}

	logic.SlotPayTable = []*protodata.SlotPayTable{
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(5), PayType: proto.String("jackpot"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("500")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("55")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("w1"), Count: proto.Int32(2), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("1000")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("300")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("45")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p1"), Count: proto.Int32(2), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("750")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("200")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p2"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("35")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("500")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("100")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p3"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("25")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("250")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("75")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p4"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("15")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("100")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("40")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p5"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p6"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("10")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p7"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p8"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(5), PayType: proto.String("chip"), Payout: proto.String("50")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(4), PayType: proto.String("chip"), Payout: proto.String("15")},
		&protodata.SlotPayTable{LinkType: proto.String("left"), Symbol: proto.String("p9"), Count: proto.Int32(3), PayType: proto.String("chip"), Payout: proto.String("5")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(9), PayType: proto.String("pickgame"), Payout: proto.String("209")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(8), PayType: proto.String("pickgame"), Payout: proto.String("208")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(7), PayType: proto.String("pickgame"), Payout: proto.String("207")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(6), PayType: proto.String("pickgame"), Payout: proto.String("206")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(5), PayType: proto.String("pickgame"), Payout: proto.String("205")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(4), PayType: proto.String("pickgame"), Payout: proto.String("204")},
		&protodata.SlotPayTable{LinkType: proto.String("any"), Symbol: proto.String("b1"), Count: proto.Int32(3), PayType: proto.String("pickgame"), Payout: proto.String("203")},
	}

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

	jpTable := &protodata.JackpotTable{StartUp: proto.Int64(1000000)}

	logic.GameBetMoney = 400
	logic.MaxBetCount = 400
	logic.PayLineMode = slot.PayLineModeFixed
	logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, payline)

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {}
	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_GameResultID) {
			data := m.MsgData.(*netproto.TGW_MSG_GameResult)

			fmt.Printf("BetMoney: %d, WinMoney: %d, JackpotMoney: %d\n", data.GetBetMoney(), data.GetWinMoney(), data.GetJackpotMoney())

			var linemoney int64
			for _, row := range data.PayGroups {
				fmt.Println(row)
				linemoney += row.GetWinMoney()
			}

			fmt.Printf("All Line Win Money %d\n", linemoney)
		}
	}

	logic.Jackpots = make(map[slot.JackpotLevel]*slot.Jackpot)
	logic.Jackpots[slot.JackpotSingle] = &slot.Jackpot{
		Origin: jpTable,
		Level:  slot.JackpotSingle,
	}

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

	fmt.Println(logic.GameResultManager)

	fmt.Println("Acual")
	for _, row := range logic.GamePayResult {
		fmt.Printf("%+v\n", row.Rule)
		for _, line := range row.SymbolPayLines {
			fmt.Printf("%5s %v\n", "", line)
		}
	}
	fmt.Println()

	// 計算遊戲獎勵
	logic.CalcGameAward()

	logic.SendGameResultMessage(logic.GameUser)
}
