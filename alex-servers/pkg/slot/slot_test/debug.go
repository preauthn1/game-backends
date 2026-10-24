package slot_test

import (
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// PrintSendMessgeFunc print information for SendMessgeFunc
func PrintSendMessgeFunc(t *testing.T, userID int32, m *msg.Message, skipIDs ...int32) {
	t.Log("SendMessgeFunc", userID, m)

	skipIDmap := make(map[int32]bool)
	for _, ID := range skipIDs {
		skipIDmap[ID] = true
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SettingID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.Slot_MSG_Setting)

		t.Log("[GameSetting]")
		t.Log(" - PayLineMode:", data.GetPayLineMode())
		t.Log(" - BetMoneys:", data.GetBetMoneys())
		t.Log(" - BetRates:", data.GetBetRates())
		t.Log(" - BigWinStep:", data.GetBigWinStep())
		t.Log(" - RowCounts:", data.GetRowCounts())
		t.Log(" - TotalPayLine:", data.GetTotalPayLine())

		t.Log(" - UnitPrefix:")
		for _, row := range data.GetUnitPrefix() {
			t.Logf("   - %v\n", row)
		}

		t.Log(" - Descriptions:")
		for _, row := range data.GetDescriptions() {
			t.Logf("   - %v\n", row)
		}

		t.Log(" - PayLines:")
		for _, row := range data.GetPayLines() {
			t.Logf("   - %v\n", row)
		}

		t.Log(" - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			t.Logf("   - %v\n", row)
		}

		t.Log(" - SymbolPayTable:")
		for _, row := range data.GetSymbolPayTable() {
			t.Logf("   - %v\n", row)
		}

		t.Log(" - FreeGames:")
		for _, row := range data.GetFreeGames() {
			t.Logf("   - ID: %d, Count: %d\n", row.GetID(), row.GetCount())
			t.Log("   - ReelSetting:")
			for _, row := range row.GetReelSetting() {
				t.Logf("     - %v\n", row)
			}
		}

		t.Log(" - GoldBallSetting:")
		for _, row := range data.GetGoldBallSetting() {
			t.Logf("   - %v\n", row)
		}

		t.Logf(" - JackpotPool\n")
		for _, row := range data.Jackpots {
			t.Log("   - Jackpot:")
			t.Log("     - ID:", row.GetID())
			t.Log("     - Name:", row.GetName())
			t.Log("     - Money:", row.GetMoney())
		}
	}

	if m.SClassID == int32(netproto.GameMessageClassID_UserStateChangeID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.UserStateChange)

		t.Logf("[UserStateChange] Score: %d\n", data.GetUser().GetScore())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BetStatusID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.Slot_MSG_BetStatus)

		t.Logf("[BetStatus] Money: %d, Ret: %d\n", data.GetMoney(), data.GetRet())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.Slot_MSG_Status)

		t.Logf("[Status] State: %d\n", data.GetState())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) && !skipIDmap[m.SClassID] {
		t.Log("[GameResult]")

		data := m.MsgData.(*netproto.Slot_MSG_GameResult)

		t.Log("  BetMoney:", data.GetBetMoney())
		t.Log("  FinalMoney:", data.GetFinalMoney())
		t.Log("  WinMoney:", data.GetWinMoney())
		t.Log("  JackpotMoney:", data.GetJackpotMoney())
		t.Log("  FreeGameWinMoney:", data.GetFreeGameWinMoney())
		t.Log("  ReelResult:")
		for _, row := range data.GetReelResult() {
			t.Logf("   - %v\n", row)
		}
		t.Log("  PayGroups:")
		for _, row := range data.GetPayGroups() {
			t.Logf("   - WinMoney: %d, Count: %d, Way: %d, LinkType: %s, Multi: %d, \n", row.GetWinMoney(), row.GetCount(), row.GetWay(), row.GetLinkType(), row.GetMulti())
			t.Log("   - PayLines:", row.GetPayLines())

			t.Log("   - BlinkPositions:")
			for _, pos := range row.GetBlinkPositions() {
				t.Logf("    - %v\n", pos)
			}
		}

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) && !skipIDmap[m.SClassID] {
		t.Log("[LaftFreeGame]")

		data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)

		t.Log(" - ID:", data.GetID())
		t.Log(" - Count:", data.GetCount())
		t.Log(" - BetMoney:", data.GetBetMoney())
		t.Log(" - BetRate:", data.GetBetRate())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_JackpotPoolRetID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.Slot_MSG_JackpotPool)

		t.Logf("[JackpotPool]\n")

		for _, row := range data.Jackpots {
			t.Log(" - Jackpot:")
			t.Log("   - ID:", row.GetID())
			t.Log("   - Name:", row.GetName())
			t.Log("   - Money:", row.GetMoney())
		}

		return
	}

	if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningPrepareID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

		t.Log("[LightningPrepare]")
		t.Log(" - LightningStep:", data.GetLightningStep())
		t.Log(" - Count:", data.GetCount())
		t.Log(" - IsFreeGame:", data.GetIsFreeGame())
		t.Log(" - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			t.Logf("   - %v\n", row)
		}
		t.Log("  ReelResult:")
		for _, row := range data.GetReelResult() {
			t.Logf("   - %v\n", row)
		}
		t.Log(" - LockSymbols:")
		for _, row := range data.GetLockSymbols() {
			t.Logf("   - %v\n", row)
		}
	}

	if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningResultID) && !skipIDmap[m.SClassID] {
		data := m.MsgData.(*netproto.Slot_MSG_LightningResult)

		t.Log("[LightningResult]")
		t.Log(" - LightningStep:", data.GetLightningStep())
		t.Log(" - Count:", data.GetCount())
		t.Log(" - BetMoney:", data.GetBetMoney())
		t.Log(" - FinalMoney:", data.GetFinalMoney())
		t.Log(" - WinMoney:", data.GetWinMoney())
		t.Log(" - JackpotMoney:", data.GetJackpotMoney())
		t.Log(" - FreeGameWinMoney:", data.GetFreeGameWinMoney())
		t.Log(" - ReelResult:")
		for _, row := range data.GetReelResult() {
			t.Logf("   - %v\n", row)
		}
		t.Log(" - PayGroups:")
		for _, row := range data.GetPayGroups() {
			t.Logf("   - %v\n", row)
		}
	}
}

// PrintSendRPCMessgeFunc print information for SendRPCMessgeFunc
func PrintSendRPCMessgeFunc(t *testing.T, safe bool, m *msg.Message, handlers map[int32]func()) {
	t.Log("SendRPCMessgeFunc", safe, m)

	if handlers != nil {
		if f, ok := handlers[m.SClassID]; ok {
			f()
		}
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID) {
		t.Log("[Read JackpotNoLink]")

		data := m.MsgData.(*netproto.Slot_MSG_JackpotRead)

		t.Log(" - GameID:", example.invalid.GetGameID())
		t.Log(" - FlagID:", example.invalid.GetFlagID())
		t.Log(" - TableNo:", example.invalid.GetTableNo())
		t.Log(" - Level:", example.invalid.GetLevel())
		t.Log(" - Contribution:", example.invalid.GetContribution())
		t.Log(" - TotalContribution:", example.invalid.GetTotalContribution())
		t.Log(" - AwardTotal:", example.invalid.GetAwardTotal())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkRecordID) {
		t.Log("[Record JackpotNoLink]")

		data := m.MsgData.(*netproto.Slot_MSG_JackpotRecord)

		t.Log(" - Records:")

		for _, row := range data.GetRecords() {
			t.Log("   - GameID:", row.GetGameID())
			t.Log("   - FlagID:", row.GetFlagID())
			t.Log("   - TableNo:", row.GetTableNo())
			t.Log("   - Level:", row.GetLevel())
			t.Log("   - Contribution:", row.GetContribution())
			t.Log("   - TotalContribution:", row.GetTotalContribution())
			t.Log("   - AwardTotal:", row.GetAwardTotal())
		}

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID) {
		t.Log("[Read FreeSpinTimes]")

		data := m.MsgData.(*netproto.Slot_MSG_FreeGameRead)

		t.Log(" - GameID:", data.GetGameID())
		t.Log(" - FlagID:", data.GetFlagID())
		t.Log(" - UserID:", data.GetUserID())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID) {
		t.Log("[Record FreeSpin]")

		data := m.MsgData.(*netproto.Slot_MSG_FreeGameRecord)

		t.Log(" - GameID:", example.invalid.GetGameID())
		t.Log(" - FlagID:", example.invalid.GetFlagID())
		t.Log(" - UserID:", example.invalid.GetUserID())
		t.Log(" - UserBet:", example.invalid.GetUserBet())
		t.Log(" - ChooseRate:", example.invalid.GetChooseRate())
		t.Log(" - FreeSpinGroup:", example.invalid.GetFreeSpinGroup())
		t.Log(" - Times:", example.invalid.GetTimes())
		t.Log(" - WinMoney:", example.invalid.GetWinMoney())
		t.Log(" - IsCoupon:", example.invalid.GetIsCoupon())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameLogReadID) {

	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameLogAddID) {
		data := m.MsgData.(*netproto.Slot_MSG_GameLogAdd)

		t.Log("[Add GameLog]")

		t.Log(" - UserID:", data.GetUserID())
		t.Log(" - GameID:", data.GetGameID())
		t.Log(" - ServerID:", data.GetServerID())
		t.Log(" - GameType:", data.GetGameType())
		t.Log(" - LogType:", data.GetLogType())
		t.Log(" - CurrentAmount:", data.GetCurrentAmount())
		t.Log(" - CurrentCoupon:", data.GetCurrentCoupon())
		t.Log(" - BetMoney:", data.GetBetMoney())
		t.Log(" - BetCoupon:", data.GetBetCoupon())
		t.Log(" - WinMoney:", data.GetWinMoney())
		t.Log(" - FinalScore:", data.GetFinalScore())
		t.Log(" - FinalCoupon:", data.GetFinalCoupon())
		t.Log(" - GameData:", data.GetGameData())
	}

	if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadID) {
		t.Log("[Read LightningGame]")

		data := m.MsgData.(*netproto.Slot_MSG_LightningRead)

		t.Log(" - GameID:", data.GetGameID())
		t.Log(" - FlagID:", data.GetFlagID())
		t.Log(" - UserID:", data.GetUserID())

		return
	}

	if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningRecordID) {
		t.Log("[Rcord LightningGame]")

		data := m.MsgData.(*netproto.Slot_MSG_LightningRecord)

		t.Log(" - GameID:", data.GetGameID())
		t.Log(" - FlagID:", data.GetFlagID())
		t.Log(" - UserID:", data.GetUserID())
		t.Log(" - Data:", data.GetData())

		return
	}
}
