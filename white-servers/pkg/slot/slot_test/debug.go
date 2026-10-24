package slot_test

import (
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// PrintSendMessgeFunc print information for SendMessgeFunc
func PrintSendMessgeFunc(t *testing.T, userID int32, m *msg.Message) {
	t.Log("SendMessgeFunc", userID, m)

	if m.SClassID == int32(netproto.GameMessageClassID_UserStateChangeID) {
		data := m.MsgData.(*netproto.UserStateChange)

		t.Logf("[UserStateChange] Score: %d\n", data.GetUser().GetScore())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_BetStatusID) {
		data := m.MsgData.(*netproto.Slot_MSG_BetStatus)

		t.Logf("[BetStatus] Money: %d, Ret: %d\n", data.GetMoney(), data.GetRet())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_StatusID) {
		data := m.MsgData.(*netproto.Slot_MSG_Status)

		t.Logf("[Status] State: %d\n", data.GetState())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_GameResultID) {
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

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID) {
		t.Log("[LaftFreeGame]")

		data := m.MsgData.(*netproto.Slot_MSG_LeftFreeGameData)

		t.Log(" - ID:", data.GetID())
		t.Log(" - Count:", data.GetCount())
		t.Log(" - BetMoney:", data.GetBetMoney())
		t.Log(" - BetRate:", data.GetBetRate())

		return
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

	if m.SClassID == int32(netproto.GameMessageClassID_SlotFreeSpinRetID) {
		t.Log("[ReadFreeSpinTimes]")

		data := m.MsgData.(*netproto.Slot_FreeSpinRead)

		t.Log(" - GameID:", data.GetGameID())
		t.Log(" - FlagID:", data.GetFlagID())
		t.Log(" - UserID:", data.GetUserID())

		return
	}

	if m.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID) {
		t.Log("[UpdateFreeSpin]")

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
}
