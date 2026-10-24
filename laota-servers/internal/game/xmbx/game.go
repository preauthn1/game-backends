package xmbx

import (
	"fmt"
	"math/rand"
	"strconv"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Spin 玩家轉輪軸
func (logic *Logic) Spin(symbolGroupID int32) {
	// 產生遊戲結果
	manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, logic.SlotSymbolDistribution)

	// 檢查需要重轉的盤面的規則
	extRules := logic.GetExtraRuleByConditoin(slot.ExtraRuleConditionAfterSpin, slot.ExtraRuleActionRespin)
	for _, rule := range extRules {
		linkType, symbol, count, err := slot.ParseExtraRuleConditionAfterSpin(rule.GetConditionValue())
		if err != nil {
			fmt.Println(err)
			mlog.Debug("%v", err)
			continue
		}

		payrule := &protodata.SlotPayTable{LinkType: proto.String(linkType), Symbol: proto.String(symbol), Count: proto.Int32(int32(count))}
		if match, _ := logic.PaylineMatcher.IsMatch(manager, payrule); match {
			// logic.GameLog.AddGameStep("因關閉連線水池，重轉一次")

			if logic.respined {
				fmt.Printf("盤面符合重轉規則: %v, 已決定持續重轉\n", rule.GetConditionValue())
				mlog.Debug("盤面符合重轉規則: %v, 已決定持續重轉", rule.GetConditionValue())
				logic.Spin(symbolGroupID)
				return
			}

			fmt.Printf("盤面符合重轉規則: %v, 決定是否保留或重轉\n", rule.GetConditionValue())
			mlog.Debug("盤面符合重轉規則: %v, 決定是否保留或重轉", rule.GetConditionValue())

			prop, denominator, err := slot.ParseExtraRuleActionRespin(rule.GetActionValue())
			if err != nil {
				mlog.Debug("%v", err)
				continue
			}
			random := rand.Intn(int(denominator))
			if int64(random) < prop {
				fmt.Printf("%d of %d, dice %d, by pass\n", prop, denominator, random)
				mlog.Debug("%d of %d, dice %d, by pass", prop, denominator, random)
				continue
			}

			logic.respined = true

			fmt.Printf("%d of %d, dice %d, respin\n", prop, denominator, random)
			mlog.Debug("%d of %d, dice %d, respin", prop, denominator, random)

			logic.Spin(symbolGroupID)
			return
		}
	}

	logic.GameResultManager = manager

	// 紀錄開獎符號
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

	extRuleMap := make(map[slot.PayType][]*protodata.ExtraRule)

	for _, rule := range logic.GetExtraRuleByConditoin(slot.ExtraRuleConditionGetAward) {
		condiType := slot.PayType(rule.GetConditionValue())
		if extRuleMap[condiType] == nil {
			extRuleMap[condiType] = make([]*protodata.ExtraRule, 0)
		}
		extRuleMap[condiType] = append(extRuleMap[condiType], rule)
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

		// 免費遊戲
		if result.GetPayType() == slot.PayTypeFreeSpin {
			// 取得贏得FreeSpin的GroupID
			f, err := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			if err != nil {
				mlog.Warn("SlotPayTable.Payout解析錯誤，%v", err)
				continue
			}
			freeSpinGroupID := int32(f)

			var freeSpinTable *protodata.FreeSpinTable

			// 取得FreeSpinTable
			for _, table := range logic.FreeSpinTable {
				if table.GetGroupId() != freeSpinGroupID {
					continue
				}
				freeSpinTable = table
			}

			if freeSpinTable == nil {
				mlog.Warn("查無FreeSpinTable, FreeSpinGroupID: %d", freeSpinGroupID)
				continue
			}

			// 檢查本身是否是FreeSpin
			if logic.FreeSpin != nil {
				if logic.FreeSpin.Origin != freeSpinTable {
					mlog.Warn("贏得的FreeSpin與原本的FreeSpin不同")
					continue
				}

				logic.FreeSpin.Count += int(freeSpinTable.GetFreeSpinCount())

			} else {
				logic.FreeSpin = slot.NewFreeSpin(freeSpinTable, logic.SlotSymbolDistribution, logic.GameBetMoney, logic.GameBetRate)
			}

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得FreeSpin(%d), Count: %d, 剩餘次數: %d", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)

			logic.UpdateFreeSpin(logic.GameUser)

			continue
		}

		// Lightning
		if result.GetPayType() == slot.PayTypeLightning {

			// 取得贏得Lightning的GroupID
			f, err := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			if err != nil {
				mlog.Warn("SlotPayTable.Payout解析錯誤，%v", err)
				continue
			}

			respinGroupID := int32(f)

			// 取得RespinTable
			var respinTable *protodata.RespinTable

			// 取得RespinTable
			for _, table := range logic.RespinTable {
				if table.GetGroupId() != respinGroupID {
					continue
				}
				respinTable = table
			}

			if respinTable == nil {
				mlog.Warn("查無RespinTable, RespinGroupID: %d", respinGroupID)
				continue
			}

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得Lightning機會(%d)", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, respinTable.GetRespinValue())

			if logic.LightningGame == nil {
				logic.LightningGame = NewLightningGame(logic, logic.IsFreeSpin())
			}

			var symbols []string

			// 處理ExtraRule
			if rules, ok := extRuleMap[slot.PayTypeLightning]; ok {
				for _, rule := range rules {
					switch slot.ExtraRuleAction(rule.GetAction()) {
					case slot.ExtraRuleActionUniGoldBall:
						symbol, prop, denominator, err := slot.ParseExtraRuleActionUniGoldBall(rule.GetActionValue())
						if err != nil {
							mlog.Debug("%v", err)
							continue
						}
						random := rand.Intn(int(denominator))
						if int64(random) > prop {
							mlog.Debug("%d of %d, dice %d, by pass", prop, denominator, random)
							continue
						}

						mlog.Debug("%d of %d, dice %d, add goldball", prop, denominator, random)
						symbols = append(symbols, symbol)
					}
				}
			}

			logic.LightningGame.AddStep(NewLightningStep(respinTable, result.Rule, logic.GameResultManager, int(respinTable.GetRespinCount())))
			logic.LightningGame.Spin(symbols...)

			logic.RecordLightningData(logic.GameUser)
		}

		// Jackpot
		if result.GetPayType() == slot.PayTypeJackpot {

			_, win := logic.WinJackpot(result.Rule)
			result.WinMoney = win

			if logic.IsFreeSpin() {
				logic.FreeSpin.WinMoney += win
			}

			logic.GameWinMoney += win
			logic.GameWinJackpot = win

			// logic.GameResultManager.ReelMode()
			// logic.AddSlotGameHistoryLog(logic.GameUser, slot.HistoryTypeJPNOLINK, slot.LogTypeREEL, logic.GameResultManager.ToSymbolString())
		}

	}

}
