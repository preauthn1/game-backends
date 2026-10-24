package jjbx

import (
	"fmt"
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// ActionSpin 玩家下注動作
func (logic *Logic) ActionSpin(user *gamelogic.GameUser, gmsg *msg.Message) {

	data := &netproto.Slot_MSG_Spin{}
	if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
		mlog.Error("[玩家Spin] 接收玩家封包解析錯誤. %v", err)
		return
	}

	ok, ret, money := logic.Bet(user, data.GetMoney(), int64(data.GetRate()))

	// 下注狀態
	logic.SendBetStatusMessageToUser(user, ret, money)

	if !ok {
		return
	}

	logic.GameLog.GameBegin()

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

		// 依照下注倍率選擇MainGameTable
		for _, table := range logic.MainGameTable {
			f, _ := strconv.ParseFloat(table.GetConditionValue(), 64)
			if int64(f) == logic.GameBetRate {
				symbolGroupID = int32(table.GetSymbolGroupId())
				break
			}
		}

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

	logic.SendJackpotMessageToUser(user)

	logic.Spin(symbolGroupID)

	logic.GameResultManager.RowMode()

	if logic.FreeSpin != nil {
		logic.UpdateFreeSpin(user)
		logic.SendLeftFreeGameToUser(user)
	}

	// 紀錄
	logic.GameResultManager.ReelMode()
	if logic.IsFreeSpin() {
		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeFREE, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
	} else {
		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeMAIN, slot.GameTypeREEL, logic.GameResultManager.ToSymbolString())
	}
	logic.GameResultManager.RowMode()

	if logic.FreeSpin != nil {
		if logic.FreeSpin.FreeSpinExtAward != nil {

			// 計算盤面g7數量
			var cnt int32
			symbols := make(map[slot.Position]*protodata.SlotSymbolDistribution)
			logic.GameResultManager.RowMode()
			for rowID, row := range logic.GameResultManager.GameResult {
				for reelID, symbol := range row {
					if symbol.GetSymbol() == "g7" {
						cnt++
						pos := slot.Position{Reel: int32(reelID + 1), Row: int32(rowID)}
						symbols[pos] = symbol
					}
				}
			}

			logic.FreeSpin.FreeSpinExtAward.TotalCount += cnt
			logic.FreeSpin.FreeSpinExtAward.TotalWinMoney += logic.FreeSpin.FreeSpinExtAward.EachCountMoney * int64(cnt)
			logic.FreeSpin.FreeSpinExtAward.Count = cnt
			logic.FreeSpin.FreeSpinExtAward.WinMoney = logic.FreeSpin.FreeSpinExtAward.EachCountMoney * int64(cnt)
			logic.FreeSpin.FreeSpinExtAward.Symbols = symbols

			logic.GameLog.AddGameStep("出現 %d 個『黃幣』, 額外獎勵增加 %d, 累積 %d", logic.FreeSpin.FreeSpinExtAward.Count, logic.FreeSpin.FreeSpinExtAward.WinMoney, logic.FreeSpin.FreeSpinExtAward.TotalWinMoney)
			mlog.Debug("出現 %d 個『黃幣』, 額外獎勵增加 %d, 累積 %d", logic.FreeSpin.FreeSpinExtAward.Count, logic.FreeSpin.FreeSpinExtAward.WinMoney, logic.FreeSpin.FreeSpinExtAward.TotalWinMoney)
		}
	}

	// 計算流水
	logic.CalcFlowAmount(user)

	// 寫分
	logic.UpdateUserScore(user)

	if logic.FreeSpin != nil {
		if logic.FreeSpin.FreeSpinExtAward != nil {

			// 檢查是否結束
			if logic.FreeSpin.Count == 0 {
				logic.GameEndLog()
				logic.GameLog.GameBegin()

				logic.FreeSpin.FreeSpinExtAward.IsFinished = true

				logic.GameLog.AddGameStep("免費遊戲結束, 開始結算額外獎勵, 累計出現『黃幣』%d 次, 每次獎勵 %d, 共獎勵 %d", logic.FreeSpin.FreeSpinExtAward.TotalCount, logic.FreeSpin.FreeSpinExtAward.EachCountMoney, logic.FreeSpin.FreeSpinExtAward.TotalWinMoney)
				mlog.Debug("免費遊戲結束, 開始結算額外獎勵, 累計出現『黃幣』%d 次, 每次獎勵 %d, 共獎勵 %d", logic.FreeSpin.FreeSpinExtAward.TotalCount, logic.FreeSpin.FreeSpinExtAward.EachCountMoney, logic.FreeSpin.FreeSpinExtAward.TotalWinMoney)

				tmpwin := logic.GameWinMoney

				logic.GameWinMoney = logic.FreeSpin.FreeSpinExtAward.TotalWinMoney

				logic.AddSlotGameHistoryLog(user, slot.HistoryTypeFREE, slot.GameTypeTOTALCOIN, fmt.Sprintf("%d|%d|%d", logic.FreeSpin.FreeSpinExtAward.TotalCount, logic.FreeSpin.FreeSpinExtAward.EachCountMoney, logic.FreeSpin.FreeSpinExtAward.TotalWinMoney))

				// 計算流水
				logic.CalcFlowAmount(user)

				// 寫分
				logic.UpdateUserScore(user)

				logic.GameWinMoney += tmpwin
			}
		}
	}

	// 傳送開獎結果
	logic.SendGameResultMessage(user)

	if logic.JackpotGame != nil {
		// 發送開獎內容
		logic.SendJackpotGameDetailMessage(logic.GameUser)
	}

	if logic.SelectChances != nil && len(logic.SelectChances) > 0 {
		logic.SendSelectChanceMessage(user)
	}

	logic.GameEndLog()

	// 調整狀態為等待
	logic.UpdateState(slot.GameStateWait)

	// 檢查免費遊戲，還有免費遊戲不結束
	logic.CheckFreeGame()

	logic.GameEndFunc()
}
