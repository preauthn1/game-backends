package xmbx

import (
	"strings"
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

	NetprotoFreeGameInfo []*netproto.Slot_FreeGameInfo

	LightningGame *LightningGame
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
		logic.CheckGameState()
	})
	logic.RegisterAfterGameStartFunc(func() {})
	logic.RegisterBeforeGameEndFunc(func() {})
	logic.RegisterAfterGameEndFunc(func() {
		logic.CheckFreeGame()
		logic.CheckLightningGame()
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

			// 累積Jackpot獎池
			for _, jackpot := range logic.Jackpots {
				if jackpot.Origin.GetIsProgress() != 1 {
					continue
				}

				// 手動更新獎池資訊
				jackpot.Increase(logic.GamePayMoney)
				logic.GameLog.AddGameStep("(%s) 水池增加 (N/10000) N=%d , 累積獎金增加 %d, 當前最大出獎金額 %d", jackpot.Level.String(), jackpot.TotalContribution, jackpot.AwardTotal, jackpot.GetCurrentPool())
				mlog.Debug("(%s) 水池增加 (N/10000) N=%d , 累積獎金增加 %d, 當前最大出獎金額 %d", jackpot.Level.String(), jackpot.TotalContribution, jackpot.AwardTotal, jackpot.GetCurrentPool())

				// 更新Jackpot
				logic.UpdateJackpotPoolNoLink(0, jackpot.GetIncrease(logic.GamePayMoney), 0, jackpot.Level)
			}
		}

		logic.Spin(symbolGroupID)

		logic.GameResultManager.RowMode()

		if logic.FreeSpin != nil {
			logic.UpdateFreeSpin(user)
			logic.SendLeftFreeGameToUser(user)
		}

		// 計算流水
		logic.CalcFlowAmount(user)

		// 紀錄
		logic.GameResultManager.ReelMode()
		if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeFREE, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
		} else {
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeMAIN, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
		}
		logic.GameResultManager.RowMode()

		// 寫分
		logic.UpdateUserScore(user)

		// 傳送開獎結果
		logic.SendGameResultMessage(user)

		logic.GameEndLog()

		if logic.LightningGame != nil {
			// 調整狀態為等待Lightning
			logic.UpdateState(slot.GameStateUserAction)

			logic.SendLightningPrepareMessage(user)
			return
		}

		// 調整狀態為等待
		logic.UpdateState(slot.GameStateWait)

		// 檢查免費遊戲，還有免費遊戲不結束
		logic.CheckFreeGame()

		if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
			// 調整狀態為等待
			logic.UpdateState(slot.GameStateBet)
		} else {
			logic.GameEndFunc()
		}

		return
	}

	// 玩家演完Lightning
	if gmsg.SClassID == int32(netproto.XMBX_GameMessageClassID_XMBX_ActionLightningSpinID) {

		data := &netproto.XMBX_MSG_LightningShow{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("[玩家演完Lightning] 接收玩家封包解析錯誤. %v", err)
			return
		}

		if logic.LightningGame == nil {
			mlog.Error("[玩家演完Lightning] 此遊戲目前無Lightning")
			return
		}

		logic.LightningGame.ShowStep = int(data.GetLightningStep())

		if logic.LightningGame.ShowStep >= len(logic.LightningGame.Steps) {
			mlog.Error("[玩家演完Lightning] 超出範圍的Step")
			return
		}

		prev := logic.LightningGame.Steps[logic.LightningGame.ShowStep-1]
		step := logic.LightningGame.Steps[logic.LightningGame.ShowStep]

		// 紀錄開獎符號
		logic.GameLog.AddGameStep("Lightngin輪軸結果, 使用SymbolGroupID: %d, 剩餘 %d 次", step.GameResultManager.SymbolGroupID, prev.LastChance)
		mlog.Debug("Lightngin輪軸結果, 使用SymbolGroupID: %d, 剩餘 %d 次", step.GameResultManager.SymbolGroupID, prev.LastChance)

		for i, row := range step.GameResultManager.GetSymbolString() {
			logic.GameLog.AddGameStep("第 %d 行: %s", i+1, strings.Join(row, ", "))
			mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
		}

		if step.Rule != nil && step.Rule != prev.Rule {
			logic.GameLog.AddGameStep("Lightngin出現新的金球, 重置次數為 %d 次", step.Origin.GetRespinCount())
			mlog.Debug("Lightngin出現新的金球, 重置次數為 %d 次", step.Origin.GetRespinCount())
		}

		// 檢查是否贏得最大Jackpot
		var isWinJackpot bool
		var jpMoney int64
		for _, award := range step.Awards {
			if award.JackpotType == slot.JackpotMajor {
				isWinJackpot = true
				jpMoney = award.WinMoney
			}
		}
		if isWinJackpot {

			if logic.Jackpots[slot.JackpotMajor].IsNeedReset() {
				logic.UpdateJackpotPoolNoLink(0, -logic.Jackpots[slot.JackpotMajor].TotalContribution, -logic.Jackpots[slot.JackpotMajor].AwardTotal+jpMoney, logic.Jackpots[slot.JackpotMajor].Level)
				logic.Jackpots[slot.JackpotMajor].Reset()
			} else {
				logic.UpdateJackpotPoolNoLink(0, 0, jpMoney, logic.Jackpots[slot.JackpotMajor].Level)
			}

			logic.GameLog.AddGameStep("贏得最大獎Jackpot: %d, 停止重轉", jpMoney)
			mlog.Debug("贏得最大獎Jackpot: %d, 停止重轉", jpMoney)
		}

		// 檢查是否表演結束
		if logic.LightningGame.ShowStep == len(logic.LightningGame.Steps)-1 {

			logic.GameWinMoney = logic.LightningGame.GetLastStep().WinMoney
			logic.GamePayMoney = 0

			// 是否在免費遊戲中
			if logic.FreeSpin != nil && logic.FreeSpin.Count != int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
				logic.FreeSpin.WinMoney += logic.LightningGame.GetLastStep().WinMoney
			}

			step.GameResultManager.ReelMode()
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeLIGHTNING, slot.GameTypeREEL, step.GameResultManager.ToSymbolString())
			step.GameResultManager.RowMode()

			// 計算流水
			logic.CalcFlowAmount(user)

			logic.UpdateUserScore(user)
		} else {
			step.GameResultManager.ReelMode()
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeRESPIN, slot.GameTypeREEL, step.GameResultManager.ToSymbolString())
			step.GameResultManager.RowMode()
		}

		logic.GameEndLog()

		logic.SendLightningResultMessage(user)

		// 檢查是否表演結束
		if logic.LightningGame.ShowStep == len(logic.LightningGame.Steps)-1 {

			logic.ResetLightningData(user)

			// 調整狀態為等待
			logic.UpdateState(slot.GameStateWait)

			logic.GameEndFunc()
		} else {

			logic.RecordLightningData(user)

			// 調整狀態為等待Lightning
			logic.UpdateState(slot.GameStateUserAction)

			logic.SendLightningPrepareMessage(user)
		}

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
	logic.LightningGame = nil

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

// CheckLightningGame 檢查是否有位轉完的LightningGame
//
//   * 如果沒有LightningGame，不動作
//   * 如果有LightningGame，且還沒表演完，不動作
//   * 如果有LightningGame，且已經表演完，清除LightningGame
func (logic *Logic) CheckLightningGame() {

	if logic.LightningGame == nil || int(logic.LightningGame.ShowStep) < len(logic.LightningGame.Steps)-1 {
		return
	}

	logic.LightningGame = nil
}

// CheckGameState 檢查遊戲狀態
//
//   * 一般正常狀況: GameStateBet
//   * 有FreeSpin: GameStateBet
//   * 有Lightning: GameStateUserAction
func (logic *Logic) CheckGameState() {

	if logic.LightningGame != nil {
		logic.UpdateState(slot.GameStateUserAction)
		return
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
		logic.ReadJackpotPoolNoLink()

		logic.WG.Add(1)
		logic.ReadLightningData(user)

		logic.WG.Wait()
		close(waitCh)
	}()

	select {
	case <-waitCh:
		logic.SendSettingMessage(user)

		logic.CheckFreeGame()
		logic.CheckLightningGame()
		logic.SendLeftFreeGameToUser(user)
		logic.SendJackpotMessageToUser(user)
		logic.SendLightningPrepareMessage(user)

		logic.CheckGameState()
	case <-time.After(3 * time.Second):
		mlog.Error("讀取資料等待過久, 將玩家踢出遊戲")
		logic.KickUserFunc(user.UserID, "系統忙碌中")
	}
}
