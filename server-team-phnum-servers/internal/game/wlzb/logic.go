package wlzb

import (
	"sync"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/baselogic"
)

// Logic game logic
type Logic struct {
	*baselogic.SlotLogicBase

	SelectChances []*SelectChance // 所有可選擇遊戲的機會

	NetprotoFreeGameInfo []*netproto.Slot_FreeGameInfo
}

// NewLogic create game logic
func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Logic {

	logic := new(Logic)
	logic.SlotLogicBase = baselogic.NewSlotLogicBase(t, tfManager, cfgs, protodatas)
	logic.SlotLogicBase.GameMessageFunc = logic.GameMessageFunc
	logic.SlotLogicBase.OnRecvRPCMessageFunc = logic.OnRecvRPCMessageFunc

	for _, row := range logic.FreeSpinTable {
		info := logic.BuildFreeGameInfo(row)
		logic.NetprotoFreeGameInfo = append(logic.NetprotoFreeGameInfo, info)
	}

	if len(logic.BetRates) > 0 {
		logic.GameBetRate = int64(logic.BetRates[0])
	}

	logic.RegisterBeforeGameStartFunc(func() {
		logic.Clean()
	})
	logic.RegisterAfterGameStartFunc(func() {
		// logic.UpdateState(slot.GameStateBet)
		logic.CheckGameState()
	})
	logic.RegisterBeforeGameEndFunc(func() {})
	logic.RegisterAfterGameEndFunc(func() {
		logic.CheckFreeGame()
		logic.CheckSelectChance()
	})

	return logic
}

// GameMessageFunc on recived client message
func (logic *Logic) GameMessageFunc(user *gamelogic.GameUser, gmsg *msg.Message) {
	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("GameMessageFunc 發生錯誤, %v", rec)
		}
	}()

	// 玩家Spin
	if gmsg.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_ActionSpinID) {

		data := &netproto.Slot_MSG_Spin{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("[玩家Spin] 接收玩家封包解析錯誤. %v", err)
			return
		}

		ok, ret, money := logic.Bet(user, data.GetMoney())

		// 下注狀態
		logic.SendBetStatusMessageToUser(user, ret, money)

		if !ok {
			return
		}

		logic.GameLog.AddGameStep("玩家 %d; 下注: %d", user.UserID, money)
		mlog.Debug("玩家 %d; 下注: %d", user.UserID, money)

		logic.UpdateState(slot.GameStateAward)

		var symbolGroupID int32

		if logic.FreeSpin != nil {
			symbolGroupID = logic.FreeSpin.SymbolGroupID

			logic.GameLog.AddGameStep("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d, 此次為免費遊戲，剩餘次數: %d", user.UserID, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate, logic.GamePayMoney, logic.FreeSpin.Count)
			mlog.Debug("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d, 此次為免費遊戲，剩餘次數: %d", user.UserID, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate, logic.GamePayMoney, logic.FreeSpin.Count)

		} else {
			symbolGroupID = logic.DefaultSymbolGroupID

			logic.GameLog.AddGameStep("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d", user.UserID, logic.GameBetMoney, logic.GameBetRate, logic.GamePayMoney+logic.GamePayCoupon)
			mlog.Debug("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d", user.UserID, logic.GameBetMoney, logic.GameBetRate, logic.GamePayMoney+logic.GamePayCoupon)
		}

		logic.Spin(symbolGroupID)

		if logic.FreeSpin != nil {
			logic.UpdateFreeSpin(user)
			logic.SendLeftFreeGameToUser(user)
		}

		// 計算流水
		logic.CalcFlowAmount(user)

		// 紀錄
		if logic.FreeSpin != nil {
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeFREE, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
		} else {
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeMAIN, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
		}

		// 寫分
		logic.UpdateUserScore(user)

		// 傳送開獎結果
		logic.SendGameResultMessage(user)

		// 調整狀態為等待
		logic.UpdateState(slot.GameStateWait)

		logic.GameEndLog()

		logic.GameEndFunc()

		return
	}

	// 玩家選擇遊戲
	if gmsg.SClassID == int32(netproto.WLZB_GameMessageClassID_WLZB_ChooseGameID) {

		if len(logic.SelectChances) == 0 {
			mlog.Error("[玩家選擇遊戲] 無選擇機會")
			return
		}

		if logic.FreeSpin != nil {
			mlog.Error("[玩家選擇遊戲] 還有免費遊戲沒有結束")
			return
		}

		data := &netproto.WLZB_MSG_ChooseGame{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("[玩家選擇遊戲] 接收玩家封包解析錯誤. %v", err)
			return
		}

		sc := logic.SelectChances[0]

		opt := sc.ChooseByID(data.GetID())
		if opt == nil {
			mlog.Error("[玩家選擇遊戲] 查無選項 %d", data.GetID())
			return
		}

		logic.FreeSpin = slot.NewFreeSpin(opt, logic.SlotSymbolDistribution, sc.BetMoney, sc.BetRate, false)
		logic.UpdateFreeSpin(user)

		logic.SendLeftFreeGameToUser(user)

		logic.SelectChances = logic.SelectChances[1:]
		logic.RecordSelectGameTimes(user)

		logic.GameLog.AddGameStep("玩家選擇免費遊戲, 剩餘選擇機會 %d 次。免費遊戲(%d) - 次數: %d, 下注金額: %d, 下注倍率: %d", len(logic.SelectChances), opt.GetId(), logic.FreeSpin.Count, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate)
		mlog.Debug("玩家選擇免費遊戲, 剩餘選擇機會 %d 次。免費遊戲(%d) - 次數: %d, 下注金額: %d, 下注倍率: %d", len(logic.SelectChances), opt.GetId(), logic.FreeSpin.Count, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate)

		logic.SendSelectChanceMessage(user)

		logic.CheckGameState()

		return
	}
}

// PiSaiMessage on recived client message
func (logic *Logic) PiSaiMessage(user *gamelogic.GameUser, gmsg *msg.Message) {}

// UserEnter 玩家進入
func (logic *Logic) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserEnter 發生錯誤, %v", rec)
		}
	}()

	logic.SlotLogicBase.UserEnter(user, tableno, seatno)

	go logic.WaitAndSendData(user)
}

// UserRecover 玩家斷線重連
func (logic *Logic) UserRecover(user *gamelogic.GameUser) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserRecover 發生錯誤, %v", rec)
		}
	}()

	logic.SlotLogicBase.UserRecover(user)

	go logic.WaitAndSendData(user)

}

// UserLeave 玩家離開
func (logic *Logic) UserLeave(user *gamelogic.GameUser) bool {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserLeave 發生錯誤, %v", rec)
		}
	}()

	logic.FreeSpin = nil
	logic.SelectChances = []*SelectChance(nil)

	return logic.SlotLogicBase.UserLeave(user)
}

// CheckFreeGame 檢查是否有位轉完的FreeGame
//
//   * 如果沒有FreeGame，不動作
//   * 如果有FreeGame，且Count != 0，不動作
//   * 如果有FreeGame，且Count == 0，清除FreeGame
func (logic *Logic) CheckFreeGame() {

	if logic.FreeSpin == nil || logic.FreeSpin.Count > 0 {
		return
	}

	logic.FreeSpin = nil
}

// CheckSelectChance 檢查是否可以讓玩家選擇遊戲
//
//   * 如果沒有SelectChance，不動作
//   * 如果有SelectChance，有FreeGame，不動作
//   * 如果有SelectChance，沒有FreeGame，發送封包給Client供玩家選擇
func (logic *Logic) CheckSelectChance() {

	if len(logic.SelectChances) == 0 {
		return
	}

	if logic.FreeSpin != nil && logic.FreeSpin.Count > 0 {
		return
	}

	logic.SendSelectChanceMessage(logic.GameUser)
}

// CheckGameState 檢查遊戲狀態
//
//   * 一般正常狀況: GameStateBet
//   * 有FreeSpin: GameStateBet
//   * 沒有FreeSpin, 有SelectChance: GameStateUserAction
func (logic *Logic) CheckGameState() {

	if logic.FreeSpin == nil || logic.FreeSpin.Count == 0 {
		if len(logic.SelectChances) > 0 {
			logic.UpdateState(slot.GameStateUserAction)
			return
		}
	}

	logic.UpdateState(slot.GameStateBet)
}

// WaitAndSendData 等待資料並傳送給玩家
func (logic *Logic) WaitAndSendData(user *gamelogic.GameUser) {
	// 設定等待
	logic.WG = new(sync.WaitGroup)
	defer func() { logic.WG = nil }()

	waitCh := make(chan struct{})

	go func() {
		logic.WG.Add(1)
		logic.ReadFreeSpinTimes(user)

		logic.WG.Add(1)
		logic.ReadSelectGameTimes(user)

		logic.WG.Wait()
		close(waitCh)
	}()

	select {
	case <-waitCh:
		logic.SendSettingMessage(user)

		logic.CheckFreeGame()
		logic.SendLeftFreeGameToUser(user)
		logic.CheckSelectChance()

		logic.CheckGameState()
	case <-time.After(3 * time.Second):
		mlog.Error("讀取資料等待過久, 將玩家踢出遊戲")
		logic.KickUserFunc(user.UserID, "系統忙碌中")
	}
}
