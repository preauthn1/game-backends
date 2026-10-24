package wlhc

import (
	"testing"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot/mock"
	"example.invalid/ServerTeam/Servers/pkg/slot/slot_test"
)

func TestLogic_UserEnterEmpty(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		slot_test.PrintSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		userEnter(logic, user, t, safe, m)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)
}

func TestLogic_UserEnterJackpotGameOnly(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		slot_test.PrintSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		handlers1 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
			mm := mock.EmptyFreeSpinRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
			mm := mock.EmptyJackpotPoolRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {
			mm := mock.EmptySelectionRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {

			detail := mock.EmptyBonusGameDetail(logic.SlotLogicBase, user)

			// JackpotGame
			mock.AppendJackpotGame(logic.SlotLogicBase, user, detail)
			// PickGame

			mm := mock.MarshalBonusGame(detail)

			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_DeferredAwardReadID)] = func() {
			mm := mock.EmptyDeferredAwardRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)
}

func TestLogic_UserEnterPickGameOnly(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		slot_test.PrintSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		handlers1 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
			mm := mock.EmptyFreeSpinRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
			mm := mock.EmptyJackpotPoolRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {
			mm := mock.EmptySelectionRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {

			detail := mock.EmptyBonusGameDetail(logic.SlotLogicBase, user)

			// JackpotGame

			// PickGame
			mock.AppendPickGame(logic.SlotLogicBase, user, detail)

			mm := mock.MarshalBonusGame(detail)

			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_DeferredAwardReadID)] = func() {
			mm := mock.EmptyDeferredAwardRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)
}

func TestLogic_UserEnterPickGameOnly2(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		slot_test.PrintSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		handlers1 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
			mm := mock.EmptyFreeSpinRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
			mm := mock.EmptyJackpotPoolRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {
			mm := mock.EmptySelectionRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {

			detail := mock.EmptyBonusGameDetail(logic.SlotLogicBase, user)

			// JackpotGame

			// PickGame
			mock.AppendPickGame2(logic.SlotLogicBase, user, detail)

			mm := mock.MarshalBonusGame(detail)

			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_DeferredAwardReadID)] = func() {
			mm := mock.EmptyDeferredAwardRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)
}

func TestLogic_UserEnterJackpotGamePickGame(t *testing.T) {

	grs := fixtures.NewTestGameRoomServerWithConfig(cfg)

	logic := grs.GameLogicManager.GetGameLogic(0).(*Logic)

	user := fixtures.GetGameUser()[0]
	user.Score = 10000
	user.UserType = gamelogic.UserType_Normal

	logic.SendMessgeFunc = func(userID int32, m *msg.Message) {
		slot_test.PrintSendMessgeFunc(t, userID, m)
	}

	logic.SendRPCMessageFunc = func(safe bool, m *msg.Message) {
		handlers1 := make(map[int32]func())

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)] = func() {
			mm := mock.EmptyFreeSpinRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)] = func() {
			mm := mock.EmptyJackpotPoolRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)] = func() {
			mm := mock.EmptySelectionRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)] = func() {

			detail := mock.EmptyBonusGameDetail(logic.SlotLogicBase, user)

			// JackpotGame
			mock.AppendJackpotGame(logic.SlotLogicBase, user, detail)

			// PickGame
			mock.AppendPickGame(logic.SlotLogicBase, user, detail)

			mm := mock.MarshalBonusGame(detail)

			logic.OnRecvRPCMessage(mm)
		}

		handlers1[int32(netproto.Slot_GameMessageClassID_Slot_DeferredAwardReadID)] = func() {
			mm := mock.EmptyDeferredAwardRPCMessage(logic.SlotLogicBase, user)
			logic.OnRecvRPCMessage(mm)
		}

		slot_test.PrintSendRPCMessgeFunc(t, safe, m, handlers1)
	}

	user.UserState.OnUserQueue(user, grs.Room)
	logic.UserEnter(user, logic.GetTable().TableNo, logic.GetTable().GetFreeSeat().SeatNo)
	logic.UserReady(user)

	time.Sleep(100 * time.Millisecond)
}
