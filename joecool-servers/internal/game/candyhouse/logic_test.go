package candyhouse

import (
	"fmt"
	"testing"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

func TestNewLogic(t *testing.T) {

	conf.CLogLevel = mlog.ErrorLevel
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	fmt.Println("SlotPatternID", logic.SlotPatternID)
	fmt.Println("BetCounts", logic.BetCounts)
	fmt.Println("MaxBetCount", logic.MaxBetCount)
	fmt.Println("BigWinRule", logic.BigWinRule)
	fmt.Println("BigWinNotify", logic.BigWinNotify)
	fmt.Println("EndActionTime", logic.EndActionTime)
	fmt.Println("MinCarryMoney", logic.MinCarryMoney)
	fmt.Println("SymbolsDesc", logic.SymbolsDesc)
	fmt.Println("PayLineDesc", logic.PayLineDesc)

	fmt.Println("DefaultReelNum", logic.DefaultReelNum)
	fmt.Println("DefaultRowNum", logic.DefaultRowNum)

	fmt.Println("PayLineMode", logic.PayLineMode)
	// fmt.Println("PaylineMatcher", logic.PaylineMatcher)

	fmt.Println("=== UnitPrefix ===", len(logic.UnitPrefix))
	for key, val := range logic.UnitPrefix {
		fmt.Println(key, val)
	}

	fmt.Println("Jackpots", logic.Jackpots)

	fmt.Println("=== SlotPattern ===")
	fmt.Println(logic.SlotPattern)

	fmt.Println("=== MainGameTable ===")
	fmt.Println(logic.MainGameTable)

	fmt.Println("=== SlotPayLine ===", len(logic.SlotPayLine))
	// for _, row := range logic.SlotPayLine {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== SlotPayTable ===", len(logic.SlotPayTable))
	for _, row := range logic.SlotPayTable {
		fmt.Println(row)
	}

	fmt.Println("=== SymbolAttributes ===", len(logic.SymbolAttributes))
	for _, row := range logic.SymbolAttributes {
		fmt.Println(row)
	}

	fmt.Println("=== SlotSymbolDistribution ===", len(logic.SlotSymbolDistribution))
	// for _, row := range logic.SlotSymbolDistribution {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== JackpotTable ===", len(logic.JackpotTable))
	// for _, row := range logic.JackpotTable {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== PickGameTable ===", len(logic.PickGameTable))
	// for _, row := range logic.PickGameTable {
	// 	fmt.Println(row)
	// }

	fmt.Println("=== BonusTable ===", len(logic.BonusTable))
	// for _, row := range logic.BonusTable {
	// 	fmt.Println(row)
	// }

}

func TestLogic_UserEnter(t *testing.T) {

	conf.CLogLevel = mlog.ErrorLevel
	conf.TimerFastMode = 1000 //40
	conf.TestMode = true

	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(t, tfManager, cfgs)
	}

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {

		// slot_test.PrintSendMessgeFunc(t, userID, m)

		printSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		// userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(2 * time.Second)

}

func printSendMessgeFunc(t *testing.T, userID int32, m *msg.Message) {

	if m.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_SettingID) {
		data := m.MsgData.(*netproto.TGW_MSG_Setting)

		t.Log("[GameSetting]")
		t.Log(" - PayLineMode:", data.GetPayLineMode())
		t.Log(" - BetCount:", data.GetBetCount())
		t.Log(" - BigWinStep:", data.GetBigWinStep())
		t.Log(" - RowCounts:", data.GetRowCounts())
		t.Log(" - MinCarryMoney:", data.GetMinCarryMoney())
		t.Log(" - PayLineDesc:", data.GetPayLineDesc())
		t.Log(" - PickGameDesc:", data.GetPickGameDesc())
		t.Log(" - TotalPayLine:", data.GetTotalPayLine())
		t.Log(" - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			t.Logf("   - %v\n", row)
		}
		t.Log(" - SymbolPayTable:")
		for _, row := range data.GetSymbolPayTable() {
			t.Logf("   - %v\n", row)
		}
		t.Log(" - PayLines:")
		for _, row := range data.GetPayLines() {
			t.Logf("   - %v\n", row)
		}
		t.Log(" - UnitPrefix:")
		for _, row := range data.GetUnitPrefix() {
			t.Logf("   - %v\n", row)
		}
	}

}
