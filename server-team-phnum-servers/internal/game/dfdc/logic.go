package dfdc

import (
	"strconv"
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

// Logic DFDC
type Logic struct {
	*baselogic.SlotLogicBase

	JackpotGame *JackpotGame

	IsCoupon bool // 使否使用禮碼

	NetprotoFreeGameInfo []*netproto.Slot_FreeGameInfo
}

// NewLogic create dfdc logic
func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Logic {
	logic := new(Logic)
	logic.SlotLogicBase = baselogic.NewSlotLogicBase(t, tfManager, cfgs, protodatas)
	logic.SlotLogicBase.GameMessageFunc = logic.GameMessageFunc
	logic.SlotLogicBase.OnRecvRPCMessageFunc = logic.OnRecvRPCMessageFunc

	for _, row := range logic.FreeSpinTable {
		info := logic.BuildFreeGameInfo(row)
		logic.NetprotoFreeGameInfo = append(logic.NetprotoFreeGameInfo, info)
	}

	logic.RegisterBeforeGameStartFunc(func() {})
	logic.RegisterAfterGameStartFunc(func() {})
	logic.RegisterBeforeGameEndFunc(func() {})
	logic.RegisterAfterGameEndFunc(func() {})

	return logic
}

// GameMessageFunc on recived client message
func (logic *Logic) GameMessageFunc(user *gamelogic.GameUser, gmsg *msg.Message) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("GameMessageFunc 發生錯誤, %v", rec)
		}
	}()

	// 遊戲開始
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID) {
		bet := &netproto.DFDC_StartBet{}
		proto.Unmarshal(gmsg.MsgData.([]byte), bet)

		data := &netproto.DFDC_StartBet{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		ok, ret, money := logic.Bet(user, data.GetMoney(), int64(data.GetGoldenSymbol()))

		// 下注狀態
		// logic.SendBetStatusMessageToUser(user, ret, money)
		logic.SendBetRetMessage(user, ret, money)

		if !ok {
			mlog.Debug("下注失敗. %v", ret)
			return
		}

		logic.UpdateState(slot.GameStateAward)

		// 依照選擇下注倍率選擇 SymbolGroupID
		var SymbolGroupID int32

		if logic.FreeSpin != nil {
			SymbolGroupID = logic.FreeSpin.SymbolGroupID

			logic.GameLog.AddGameStep("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d, 此次為免費遊戲，剩餘次數: %d", user.UserID, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate, logic.GamePayMoney, logic.FreeSpin.Count)
			mlog.Debug("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d, 此次為免費遊戲，剩餘次數: %d", user.UserID, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate, logic.GamePayMoney, logic.FreeSpin.Count)

			if logic.FreeSpin.IsCoupon {
				logic.GameLog.AddGameStep("本次為禮碼遊戲")
				mlog.Debug("本次為禮碼遊戲")
			}

		} else {
			for _, table := range logic.MainGameTable {
				f, _ := strconv.ParseFloat(table.GetConditionValue(), 64)
				if int64(f) == logic.GameBetRate {
					SymbolGroupID = int32(table.GetSymbolGroupId())
					break
				}
			}

			logic.GameLog.AddGameStep("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d", user.UserID, logic.GameBetMoney, logic.GameBetRate, logic.GamePayMoney+logic.GamePayCoupon)
			mlog.Debug("玩家 %d; 底注: %d, 符號倍率: %d, 花費金額: %d", user.UserID, logic.GameBetMoney, logic.GameBetRate, logic.GamePayMoney+logic.GamePayCoupon)

			if logic.IsCoupon {
				logic.GameLog.AddGameStep("本次為禮碼遊戲")
				mlog.Debug("本次為禮碼遊戲")
			}

			if !logic.IsCoupon {
				// 累積Jackpot獎池
				for _, jackpot := range logic.Jackpots {
					// 手動更新獎池資訊
					jackpot.Increase(logic.GamePayMoney)
					logic.GameLog.AddGameStep("水池增加 (N/10000) N=%d , 累積獎金增加 %d, 當前最大出獎金額 %d", jackpot.TotalContribution, jackpot.AwardTotal, jackpot.GetCurrentPool())
					mlog.Debug("水池增加 (N/10000) N=%d , 累積獎金增加 %d, 當前最大出獎金額 %d", jackpot.TotalContribution, jackpot.AwardTotal, jackpot.GetCurrentPool())

					// 更新Jackpot
					logic.UpdateJackpotPoolNoLink(0, jackpot.GetIncrease(logic.GamePayMoney), 0, jackpot.Level)
				}
			}
		}

		logic.SendJackpotPoolMessage(user)

		logic.Spin(SymbolGroupID)

		if logic.FreeSpin != nil {
			logic.UpdateFreeSpin(user)
		}

		// 紀錄
		if logic.FreeSpin == nil || logic.FreeSpin.Count == int(logic.FreeSpin.Origin.GetFreeSpinCount()) {
			logic.GameResultManager.ReelMode()
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeMAIN, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
		} else {
			logic.GameResultManager.ReelMode()
			logic.AddSlotGameHistoryLog(user, slot.HistoryTypeFREE, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
		}

		logic.UpdateUserScore(user)

		// 計算流水
		if !logic.IsCoupon {
			logic.CalcFlowAmount(user)
		}

		// 傳送開獎結果
		logic.SendGameResultMessage(user)

		// 傳送FreeSpin狀態
		logic.SendFreeSpinCountMessage(user)

		// 調整狀態為等待
		logic.UpdateState(slot.GameStateWait)

		logic.GameEndLog()

		logic.GameEndFunc()

		logic.UpdateState(slot.GameStateBet)

		logic.Clean()

		return
	}

	// 玩家Bonus Game選擇
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameIconID) {

		if logic.JackpotGame == nil {
			return
		}

		detail := &netproto.DFDC_BonusGameDetail{}
		proto.Unmarshal(gmsg.MsgData.([]byte), detail)

		logic.JackpotGame.Choose(detail.GetId(), detail.GetPlace(), detail.GetIcon())
		logic.GameLog.AddGameStep("用户 %d 選擇第: %d次; 選擇位置: %d, 圖案: %d", user.UserID, detail.GetId(), detail.GetPlace(), detail.GetIcon())
		mlog.Debug("用户 %d 選擇第: %d次; 選擇位置: %d, 圖案: %d", user.UserID, detail.GetId(), detail.GetPlace(), detail.GetIcon())

		if !logic.JackpotGame.IsFinished() {
			// 更新DB
			logic.UpdateBonusGame(user)
			return
		}

		logic.SendJackpotPoolMessage(user)

		// 結束遊戲
		logic.GamePayMoney = 0
		logic.GameWinMoney = logic.JackpotGame.WinMoney
		logic.GameWinJackpot = logic.JackpotGame.WinMoney

		logic.GameLog.AddGameStep("JackpotGame圖案順序: %s", logic.JackpotGame.GetProcessData())
		logic.GameLog.AddGameStep("JackpotGame獎金: %d", logic.GameWinMoney)

		mlog.Debug("JackpotGame圖案順序: %s", logic.JackpotGame.GetProcessData())
		mlog.Debug("JackpotGame獎金: %d", logic.GameWinMoney)

		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeJPNOLINK, slot.GameTypeOPEN, logic.JackpotGame.GetProcessData())

		logic.ResetBonusGame(user)

		// 調整狀態為等待
		logic.UpdateState(slot.GameStateWait)

		if logic.FreeSpin != nil {
			logic.UpdateFreeSpin(user)
		}

		// 寫分
		logic.UpdateUserScore(user)

		// 計算流水
		if !logic.IsCoupon {
			logic.CalcFlowAmount(user)
		}

		logic.GameEndLog()

		logic.GameEndFunc()

		logic.UpdateState(slot.GameStateBet)

		logic.JackpotGame = nil

		return
	}

	// 查詢遊戲歷程紀錄
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCLoadGameHistoryID) {

		param := &netproto.DFDC_GetGameLog{}
		proto.Unmarshal(gmsg.MsgData.([]byte), param)

		logic.ReadSlotGameHistoryLog(user, param.GetPageIndex(), param.GetPageSize(), param.GetStartTime(), param.GetEndTime())

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

	logic.UpdateState(slot.GameStateWait)

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

	logic.UpdateState(slot.GameStateWait)

	go logic.WaitAndSendData(user)
}

// UserLeave 玩家離開
func (logic *Logic) UserLeave(user *gamelogic.GameUser) bool {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserLeave 發生錯誤, %v", rec)
		}
	}()

	logic.JackpotGame = nil
	logic.FreeSpin = nil

	return logic.SlotLogicBase.UserLeave(user)
}

// UpdateState 更新遊戲狀態
func (logic *Logic) UpdateState(state slot.GameState) {
	logic.GameState = state
	logic.SendStateMessage(logic.GameUser)
}

// UpdateUserScore 更新玩家分數
func (logic *Logic) UpdateUserScore(user *gamelogic.GameUser) {
	if user.IsRobot() {
		return
	}

	logic.GameLog.AddGameStep("更新玩家金幣, 下注金幣: %d, 使用金幣: %d, 使用禮碼: %d, 贏得獎金: %d", logic.GamePayMoney+logic.GamePayCoupon, logic.GamePayMoney, logic.GamePayCoupon, logic.GameWinMoney)
	mlog.Debug("更新玩家金幣, 下注金幣: %d, 使用金幣: %d, 使用禮碼: %d, 贏得獎金: %d", logic.GamePayMoney+logic.GamePayCoupon, logic.GamePayMoney, logic.GamePayCoupon, logic.GameWinMoney)

	logic.UpdateUserScoreFunc(user.UserID, logic.GameWinMoney-logic.GamePayMoney, 0, -logic.GamePayCoupon)

}

// Clean clean game data
func (logic *Logic) Clean() {
	logic.SlotLogicBase.Clean()
	logic.IsCoupon = false
}

// WaitAndSendData 等待資料並傳送給玩家
func (logic *Logic) WaitAndSendData(user *gamelogic.GameUser) {
	// 設定等待
	logic.WG = new(sync.WaitGroup)
	defer func() { logic.WG = nil }()

	waitCh := make(chan struct{})

	go func() {
		logic.WG.Add(1)
		logic.ReadJackpotPoolNoLink()

		logic.WG.Add(1)
		logic.ReadFreeSpinTimes(user)

		logic.WG.Add(1)
		logic.ReadBonusGame(user)

		logic.WG.Wait()
		close(waitCh)
	}()

	select {
	case <-waitCh:

		logic.SendGoldenSymbolMessage(user)
		logic.SendSettingMessage(user)

		logic.SendJackpotPoolMessage(user)
		logic.SendFreeSpinCountMessage(user)
		logic.SendFreeSpinBetMessage(user)
		logic.SendJackpotGameDetailMessage(user)

		logic.UpdateState(slot.GameStateBet)

	case <-time.After(3 * time.Second):
		mlog.Error("讀取資料等待過久, 將玩家踢出遊戲")
		logic.KickUserFunc(user.UserID, "系統忙碌中")
	}
}
