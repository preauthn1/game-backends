package jjbx

import (
	"math/rand"
	"strconv"
	"strings"
	"time"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Spin 玩家轉輪軸
func (logic *Logic) Spin(symbolGroupID int32) {

	manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, logic.SlotSymbolDistribution)

	logic.GameResultManager = manager

	// 檢查是否關閉連線型Jackpot
	if logic.IsCloseLinkJackpot {
		// hard code
		// 找出若是全為金球就重轉
		rule := &protodata.SlotPayTable{
			LinkType: proto.String("any"),
			Symbol:   proto.String("g1"),
			Count:    proto.Int32(15),
		}
		if match, _ := logic.PaylineMatcher.IsMatch(manager, rule); match {
			// logic.GameLog.AddGameStep("因關閉連線水池，重轉一次")
			mlog.Warn("因關閉連線水池，重轉一次")
			logic.Spin(symbolGroupID)
			return
		}
	}

	logic.LogGameResult(manager)

	logic.CalcGameResult()

	logic.CalcGameAward()
}

// CalcGameResult 計算遊戲結果
func (logic *Logic) CalcGameResult() {
	logic.GamePayResult = logic.SlotLogicBase.CalcGameResult(logic.GameResultManager)
}

// CalcGameAward 計算遊戲獎勵
func (logic *Logic) CalcGameAward() {

	logic.GameWinMoney = logic.SlotLogicBase.CalcGameAward(logic.GamePayResult, logic.GameBetMoney, logic.GameBetRate)

	if logic.FreeSpin != nil && logic.GameWinMoney > 0 {

		multi := logic.FreeSpin.GetMultipleValue()
		win := logic.GameWinMoney
		total := win * int64(multi)

		logic.FreeSpin.WinCount++
		logic.FreeSpin.WinMoney += total
		logic.GameWinMoney = total

		// logic.GameLog.AddGameStep("免費遊戲中第 %d 次中獎, 加乘 %d 倍, 原中獎金額: %d, 加乘後金額: %d", logic.FreeSpin.WinCount, multi, win, total)
	}

	for _, result := range logic.GamePayResult {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		switch result.Rule.GetUseType() {
		case "normal":
			if logic.IsFreeSpin() {
				continue
			}
		case "freespin":
			if !logic.IsFreeSpin() {
				continue
			}
		}

		// Jackpot
		if result.GetPayType() == slot.PayTypeJackpot {

			_, win := logic.WinJackpot(result.Rule)

			logic.GameWinMoney += win
			logic.GameWinJackpot = win
		}

		// Jackpot Game
		if result.GetPayType() == slot.PayTypeMysteryJackpotR {

			groups := strings.Split(result.Rule.GetPayout(), ",")

			rateMap := make(map[int64]int64)
			for _, group := range groups {
				strs := strings.Split(group, ":")
				if len(strs) != 2 {
					continue
				}

				rate, _ := strconv.ParseFloat(strs[0], 64)
				prop, _ := strconv.ParseFloat(strs[1], 64)

				rateMap[int64(rate)] = int64(prop)

			}

			var prop int64

			if val, ok := rateMap[logic.GameBetRate]; !ok {
				mlog.Warn("機率表無倍率 %d 的 Jackpot Game 中獎機率.", logic.GameBetRate)
				continue
			} else {
				prop = val
			}

			mlog.Debug("倍率 %d 的 Jackpot Game, 機率: %d / (百億)", logic.GameBetRate, prop)

			// 骰出中BonusGame
			// 隨機產生該總重量
			rand.Seed(time.Now().UnixNano())
			random := rand.Intn(1e10) // 百億分之一

			if int(random) > int(prop) {
				mlog.Debug("JackpotGame 未中獎")
				continue
			}

			var jackpots []*slot.Jackpot
			for _, jackpot := range logic.Jackpots {
				jackpots = append(jackpots, jackpot)
			}

			jpg := slot.NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, int32(logic.GameBetRate), logic.GameBetMoney)
			jpg.Origin = result.Rule
			jpg.Play()

			jpg.WinMoney = logic.CalcJackpot(jpg.WinJackpot)

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 JackpotGame[%s], 獎金: %d, 將於翻牌結束後結算", result.Rule.GetSymbol(), result.Rule.GetLinkType(), result.Rule.GetCount(), jpg.WinJackpot.Level.String(), jpg.WinMoney)

			logic.JackpotGame = jpg

			logic.RecordJackpotGame(logic.GameUser)

			continue
		}

		// Jackpot Grand
		if result.GetPayType() == slot.PayTypeMysteryGrandR {
			groups := strings.Split(result.Rule.GetPayout(), ",")

			rateMap := make(map[int64]int64)
			for _, group := range groups {
				strs := strings.Split(group, ":")
				if len(strs) != 2 {
					continue
				}

				rate, _ := strconv.ParseFloat(strs[0], 64)
				prop, _ := strconv.ParseFloat(strs[1], 64)

				rateMap[int64(rate)] = int64(prop)

			}

			var prop int64

			if val, ok := rateMap[logic.GameBetRate]; !ok {
				mlog.Warn("機率表無倍率 %d 的 Jackpot Grand 中獎機率.", logic.GameBetRate)
				continue
			} else {
				prop = val
			}

			mlog.Debug("倍率 %d 的 Jackpot Grand, 機率: %d / (百億)", logic.GameBetRate, prop)

			// 骰出中BonusGame
			// 隨機產生該總重量
			rand.Seed(time.Now().UnixNano())
			random := rand.Intn(1e10) // 百億分之一

			if int(random) > int(prop) {
				mlog.Debug("Jackpot Grand 未中獎")
				continue
			}

			jp := logic.Jackpots[slot.JackpotGrand]

			win := logic.CalcJackpot(jp)

			logic.GameWinMoney += win
			logic.GameWinJackpot = win

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得Jackpot Grand獎勵: %d", result.Rule.GetSymbol(), result.Rule.GetLinkType(), result.Rule.GetCount(), win)

			continue
		}

		// Select Game
		if result.GetPayType() == slot.PayTypeSelection {

			selection := logic.WinSelection(result.Rule)

			if selection == nil {
				continue
			}

			var gMoney int64
			var gCnt int
			for _, reel := range selection.GameResultManager.GameResult {
				for _, symbol := range reel {
					symbol.GetSymbol()
					for _, attr := range logic.SymbolAttributes {
						if attr.GetIsGoldBall() == 1 && attr.GetSymbol() == symbol.GetSymbol() {
							gCnt++
							switch slot.PayType(attr.GetGoldballType()) {
							case slot.PayTypeChip:
								f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)
								win := selection.BetMoney * int64(f)
								gMoney += win
							case slot.PayTypeChipXRate:
								f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)
								win := selection.BetMoney * selection.BetRate * int64(f)
								gMoney += win
							}
						}
					}
				}
			}
			selection.Data["GMoney"] = gMoney

			logic.SelectChances = append(logic.SelectChances, selection)

			if logic.IsFreeSpin() {
				logic.GameLog.AddGameStep("因目前正於免費遊戲中, 將於免費遊戲結束後開始選擇, 剩餘 %d 次", len(logic.SelectChances))
			}

			logic.RecordSelectGameTimes(logic.GameUser)
		}

		// FreeSpin Add Count
		if result.GetPayType() == slot.PayTypeAddTimes {

			if logic.FreeSpin == nil {
				mlog.Error("增加免費遊戲次數錯誤, 沒有免費遊戲")
				continue
			}

			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			logic.FreeSpin.Count += int(f)

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得免費遊戲次數, 增加 %d 次, 剩餘 %d 次", result.Rule.GetSymbol(), result.Rule.GetLinkType(), result.Rule.GetCount(), int(f), logic.FreeSpin.Count)
		}
	}

}
