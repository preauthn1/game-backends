package helper

import (
	"fmt"
	"io"
	"sort"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
)

// PrintLogicInfo 列印出此Logic內完整設定資訊
func PrintLogicInfo(w io.Writer, logic *baselogic.SlotLogicBase) {
	fmt.Fprintln(w, "======================================")
	fmt.Fprintln(w, "=============   Logic   ==============")
	fmt.Fprintln(w, "======================================")

	fmt.Fprintln(w, "SlotPatternID", logic.SlotPatternID, logic.SlotPattern.GetMyComment())
	fmt.Fprintln(w, "BetCounts", logic.BetCounts)
	fmt.Fprintln(w, "BetRate", logic.BetRates)
	fmt.Fprintln(w, "MaxBetCount", logic.MaxBetCount)
	fmt.Fprintln(w, "MaxBetRate", logic.MaxBetRate)
	fmt.Fprintln(w, "BigWinRule", logic.BigWinRule)
	fmt.Fprintln(w, "BigWinNotify", logic.BigWinNotify)
	fmt.Fprintln(w, "EndActionTime", logic.EndActionTime)
	fmt.Fprintln(w, "MinCarryMoney", logic.MinCarryMoney)
	fmt.Fprintln(w, "SymbolsDesc", logic.SymbolsDesc)
	fmt.Fprintln(w, "PayLineDesc", logic.PayLineDesc)
	fmt.Fprintln(w, "=== UnitPrefix ===", len(logic.UnitPrefix))
	for key, val := range logic.UnitPrefix {
		fmt.Fprintln(w, " - ", key, ":", val)
	}

	fmt.Fprintln(w, "=== Descriptions ===", len(logic.Descriptions))
	for key, val := range logic.Descriptions {
		fmt.Fprintln(w, " - ", key, ":", val)
	}

	fmt.Fprintln(w, "DefaultReelNum", logic.DefaultReelNum)
	fmt.Fprintln(w, "DefaultRowNum", logic.DefaultRowNum)
	fmt.Fprintln(w, "DefaultSymbolGroupID", logic.DefaultSymbolGroupID)

	fmt.Fprintln(w, "GameBetRate", logic.GameBetRate)

	fmt.Fprintln(w, "PayLineMode", logic.PayLineMode)
	// fmt.Fprintln(w, "PaylineMatcher", logic.PaylineMatcher)

	fmt.Fprintln(w, "=== Jackpots ===", len(logic.Jackpots))
	for level, jackpot := range logic.Jackpots {
		fmt.Fprintln(w, " - ", level, jackpot)
	}
}

// PrintLogicProto 印出此Logic Proto資訊
func PrintLogicProto(w io.Writer, logic *baselogic.SlotLogicBase, deep bool) {
	fmt.Fprintln(w, "======================================")
	fmt.Fprintln(w, "===========   Protodata   ============")
	fmt.Fprintln(w, "======================================")

	fmt.Fprintln(w, "=== SlotPattern ===")
	fmt.Fprintln(w, " - ", logic.SlotPattern.GetMyComment(), logic.SlotPattern)

	fmt.Fprintln(w, "=== MainGameTable ===", len(logic.MainGameTable))
	if deep {
		for _, row := range logic.MainGameTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== SlotPayLine ===", len(logic.SlotPayLine))
	if deep {
		for _, row := range logic.SlotPayLine {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== SlotPayTable ===", len(logic.SlotPayTable))
	if deep {
		for _, row := range logic.SlotPayTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== SymbolAttributes ===", len(logic.SymbolAttributes))
	if deep {
		for _, row := range logic.SymbolAttributes {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== RespinTable ===", len(logic.RespinTable))
	if deep {
		for _, row := range logic.RespinTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== SlotSymbolDistribution ===", len(logic.SlotSymbolDistribution))

	symbolGroupIDMap := make(map[int32]bool)
	for _, row := range logic.SlotSymbolDistribution {
		symbolGroupIDMap[int32(row.GetGroupId())] = true
	}
	var groupIDs []int32
	for id := range symbolGroupIDMap {
		groupIDs = append(groupIDs, id)
	}
	sort.Slice(groupIDs, func(i, j int) bool {
		return groupIDs[i] < groupIDs[j]
	})

	fmt.Fprintln(w, "=== SlotSymbolGroups ===", len(groupIDs))
	for _, row := range groupIDs {
		fmt.Fprintln(w, " -  SymbolGroupID: ", row)
	}

	fmt.Fprintln(w, "=== ReelBgDistribution ===", len(logic.ReelBgDistribution))
	if deep {
		for _, row := range logic.ReelBgDistribution {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== JackpotTable ===", len(logic.JackpotTable))
	if deep {
		for _, row := range logic.JackpotTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== PickGameTable ===", len(logic.PickGameTable))
	if deep {
		for _, row := range logic.PickGameTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== BonusTable ===", len(logic.BonusTable))
	if deep {
		for _, row := range logic.BonusTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== FreeSpinTable ===", len(logic.FreeSpinTable))
	if deep {
		for _, row := range logic.FreeSpinTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== SelectionTable ===", len(logic.SelectionTable))
	if deep {
		for _, row := range logic.SelectionTable {
			fmt.Fprintln(w, " - ", row)
		}
	}

	fmt.Fprintln(w, "=== ExtraRule ===", len(logic.ExtraRule))
	if deep {
		for _, row := range logic.ExtraRule {
			fmt.Fprintln(w, " - ", row)
		}
	}
}

// PrintMessage 印出封包訊息
func PrintMessage(w io.Writer, m *msg.Message) {
	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameReelSettingID) {
		data := m.MsgData.(*netproto.Slot_MSG_ReelSetting)
		fmt.Fprintln(w, "[ReelSetting]", m)
		fmt.Fprintln(w, " - GameReels:")
		for _, row := range data.GetGameReels() {
			fmt.Fprintf(w, "   - ID: %d, Name: %s, ConditionType: %s, ConditionValue: %s\n", row.GetID(), row.GetName(), row.GetConditionType(), row.GetConditionValue())
			fmt.Fprintln(w, "   - ReelSetting:")
			for _, row := range row.GetReelSetting() {
				fmt.Fprintf(w, "   - %v\n", row)
			}
		}
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SettingID) {
		data := m.MsgData.(*netproto.Slot_MSG_Setting)

		fmt.Fprintln(w, "[GameSetting]", m)
		fmt.Fprintln(w, " - PayLineMode:", data.GetPayLineMode())
		fmt.Fprintln(w, " - BetMoneys:", data.GetBetMoneys())
		fmt.Fprintln(w, " - BetRates:", data.GetBetRates())
		fmt.Fprintln(w, " - BigWinStep:", data.GetBigWinStep())
		fmt.Fprintln(w, " - RowCounts:", data.GetRowCounts())
		fmt.Fprintln(w, " - TotalPayLine:", data.GetTotalPayLine())

		fmt.Fprintln(w, " - UnitPrefix:")
		for _, row := range data.GetUnitPrefix() {
			fmt.Fprintln(w, "   - ", row)
		}

		fmt.Fprintln(w, " - Descriptions:")
		for _, row := range data.GetDescriptions() {
			fmt.Fprintln(w, "   - ", row)
		}

		fmt.Fprintln(w, " - PayLines:")
		for _, row := range data.GetPayLines() {
			fmt.Fprintln(w, "   - ", row)
		}

		fmt.Fprintln(w, " - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			fmt.Fprintln(w, "   - ", row)
		}

		fmt.Fprintln(w, " - SymbolPayTable:")
		for _, row := range data.GetSymbolPayTable() {
			fmt.Fprintln(w, "   - ", row)
		}

		fmt.Fprintln(w, " - FreeGames:")
		for _, row := range data.GetFreeGames() {
			fmt.Fprintf(w, "   - ID: %d, Count: %d, Data: %s\n", row.GetID(), row.GetCount(), row.GetData())
			fmt.Fprintln(w, "   - ReelSetting:")
			for _, row := range row.GetReelSetting() {
				fmt.Fprintln(w, "     - ", row)
			}
		}

		fmt.Fprintln(w, " - GoldBallSetting:")
		for _, row := range data.GetGoldBallSetting() {
			fmt.Fprintln(w, "   - ", row)
		}

		fmt.Fprintf(w, " - JackpotPool\n")
		for _, row := range data.Jackpots {
			fmt.Fprintln(w, "   - ", row)
		}

		return
	}

	if m.SClassID == int32(netproto.GameMessageClassID_UserStateChangeID) {
		data := m.MsgData.(*netproto.UserStateChange)
		fmt.Fprintln(w, "[UserStateChange]", m)
		fmt.Fprintf(w, " - Score: %d\n", data.GetUser().GetScore())
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BetStatusID) {
		data := m.MsgData.(*netproto.Slot_MSG_BetStatus)
		fmt.Fprintln(w, "[BetStatus]", m, data)
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) {
		data := m.MsgData.(*netproto.Slot_MSG_Status)
		fmt.Fprintln(w, "[GameStatus]", m, data)
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
		data := m.MsgData.(*netproto.Slot_MSG_GameResult)

		fmt.Fprintln(w, "[GameResult]", m)
		fmt.Fprintln(w, "  - BetMoney:", data.GetBetMoney())
		fmt.Fprintln(w, "  - FinalMoney:", data.GetFinalMoney())
		fmt.Fprintln(w, "  - WinMoney:", data.GetWinMoney())
		fmt.Fprintln(w, "  - JackpotMoney:", data.GetJackpotMoney())
		fmt.Fprintln(w, "  - FreeGameWinMoney:", data.GetFreeGameWinMoney())
		fmt.Fprintln(w, "  - ReelResult:")
		for _, row := range data.GetReelResult() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, "  - BackgroundInfo:")
		fmt.Fprintln(w, "    - WinMoney:", data.GetBackgroundInfo().GetWinMoney())
		fmt.Fprintln(w, "    - Result:")
		for _, row := range data.GetBackgroundInfo().GetResult() {
			fmt.Fprintf(w, "      - %v\n", row)
		}
		fmt.Fprintln(w, "    - BlinkPositions:")
		for _, row := range data.GetBackgroundInfo().GetBlinkPositions() {
			fmt.Fprintf(w, "      - %v\n", row)
		}
		fmt.Fprintln(w, "  - PayGroups:")
		for _, row := range data.GetPayGroups() {
			fmt.Fprintf(w, "     - WinMoney: %d, Count: %d, Way: %d, LinkType: %s, Multi: %d, \n", row.GetWinMoney(), row.GetCount(), row.GetWay(), row.GetLinkType(), row.GetMulti())
			fmt.Fprintln(w, "     - PayLines:", row.GetPayLines())

			fmt.Fprintln(w, "     - BlinkPositions:")
			for _, pos := range row.GetBlinkPositions() {
				fmt.Fprintf(w, "      - %v\n", pos)
			}
		}
		fmt.Fprintln(w, "  - FreeGameExtAward:", data.GetFreeGameExtAward())
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
		data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)
		fmt.Fprintln(w, "[LaftFreeGame]", m, data)
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_JackpotPoolRetID) {
		fmt.Fprintln(w, "[JackpotPool]", m)
		data := m.MsgData.(*netproto.Slot_MSG_JackpotPool)
		for _, row := range data.Jackpots {
			fmt.Fprintln(w, " - ", row)
		}
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningPrepareID) {
		data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

		fmt.Fprintln(w, "[LightningPrepare]", m)
		fmt.Fprintln(w, " - LightningStep:", data.GetLightningStep())
		fmt.Fprintln(w, " - Count:", data.GetCount())
		fmt.Fprintln(w, " - BetMoney:", data.GetBetMoney())
		fmt.Fprintln(w, " - BetRate:", data.GetBetRate())
		fmt.Fprintln(w, " - IsFreeGame:", data.GetIsFreeGame())
		fmt.Fprintln(w, " - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, "  ReelResult:")
		for _, row := range data.GetReelResult() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - LockSymbols:")
		for _, row := range data.GetLockSymbols() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - PastPayGroups:")
		for _, row := range data.GetPastPayGroups() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LightningResultID) {
		data := m.MsgData.(*netproto.Slot_MSG_LightningResult)

		fmt.Fprintln(w, "[LightningResult]", m)
		fmt.Fprintln(w, " - LightningStep:", data.GetLightningStep())
		fmt.Fprintln(w, " - Count:", data.GetCount())
		fmt.Fprintln(w, " - BetMoney:", data.GetBetMoney())
		fmt.Fprintln(w, " - FinalMoney:", data.GetFinalMoney())
		fmt.Fprintln(w, " - WinMoney:", data.GetWinMoney())
		fmt.Fprintln(w, " - JackpotMoney:", data.GetJackpotMoney())
		fmt.Fprintln(w, " - FreeGameWinMoney:", data.GetFreeGameWinMoney())
		fmt.Fprintln(w, " - ReelResult:")
		for _, row := range data.GetReelResult() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - PayGroups:")
		for _, row := range data.GetPayGroups() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		return
	}

	if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningPrepareID) {
		data := m.MsgData.(*netproto.Slot_MSG_LightningPrepare)

		fmt.Fprintln(w, "[XMBX LightningPrepare]", m)
		fmt.Fprintln(w, " - LightningStep:", data.GetLightningStep())
		fmt.Fprintln(w, " - Count:", data.GetCount())
		fmt.Fprintln(w, " - BetMoney:", data.GetBetMoney())
		fmt.Fprintln(w, " - BetRate:", data.GetBetRate())
		fmt.Fprintln(w, " - IsFreeGame:", data.GetIsFreeGame())
		fmt.Fprintln(w, " - ReelSetting:")
		for _, row := range data.GetReelSetting() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, "  ReelResult:")
		for _, row := range data.GetReelResult() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - LockSymbols:")
		for _, row := range data.GetLockSymbols() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		return
	}

	if m.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_LightningResultID) {
		data := m.MsgData.(*netproto.Slot_MSG_LightningResult)

		fmt.Fprintln(w, "[XMBX LightningResult]", m)
		fmt.Fprintln(w, " - LightningStep:", data.GetLightningStep())
		fmt.Fprintln(w, " - Count:", data.GetCount())
		fmt.Fprintln(w, " - BetMoney:", data.GetBetMoney())
		fmt.Fprintln(w, " - FinalMoney:", data.GetFinalMoney())
		fmt.Fprintln(w, " - WinMoney:", data.GetWinMoney())
		fmt.Fprintln(w, " - JackpotMoney:", data.GetJackpotMoney())
		fmt.Fprintln(w, " - FreeGameWinMoney:", data.GetFreeGameWinMoney())
		fmt.Fprintln(w, " - ReelResult:")
		for _, row := range data.GetReelResult() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - PayGroups:")
		for _, row := range data.GetPayGroups() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BonusGameID) {
		data := m.MsgData.(*netproto.Slot_MSG_BonusGame)

		fmt.Fprintln(w, "[BonusGame]", m)
		fmt.Fprintln(w, " - Type:", data.GetType())
		fmt.Fprintln(w, " - Count:", data.GetCount())
		fmt.Fprintln(w, " - WinMoney:", data.GetWinMoney())
		fmt.Fprintln(w, " - All:", data.GetAll())
		fmt.Fprintln(w, " - Record:")
		for _, row := range data.GetRecords() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - Positions:")
		for _, row := range data.GetPositions() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_SelectGameOptionsID) {
		data := m.MsgData.(*netproto.Slot_MSG_ChooseGameOptions)

		fmt.Fprintln(w, "[SelectGameOptions]", m)
		fmt.Fprintln(w, " - Times:", data.GetTimes())
		fmt.Fprintln(w, " - Options:")
		for _, row := range data.GetOptions() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		fmt.Fprintln(w, " - Data:", data.GetData())
		fmt.Fprintln(w, " - ReelResult:")
		for _, row := range data.GetReelResult() {
			fmt.Fprintf(w, "   - %v\n", row)
		}
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_PickBonusRetID) {
		data := m.MsgData.(*netproto.Slot_MSG_PickBonus)
		fmt.Fprintln(w, "[PickBonus]", m, data.Record)
		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_IncreaseGameTimesID) {
		data := m.MsgData.(*netproto.Slot_MSG_IncreaseGameTimes)
		fmt.Fprintln(w, "[IncreaseGameTimes]", m, data)
		return
	}

	fmt.Fprintln(w, m)
}
