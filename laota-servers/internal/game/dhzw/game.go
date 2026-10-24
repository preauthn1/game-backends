package dhzw

import (
	"sort"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/prob"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// Spin 玩家轉輪軸
func (logic *Logic) Spin(symbolGroupID, bgGroupID int32) {
	// 產生遊戲結果
	manager, _ := slot.NewGameResultManager(logic.DefaultReelNum, logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, logic.SlotSymbolDistribution)
	manager.GenerateBackground(bgGroupID, logic.ReelBgDistribution)

	logic.GameResultManager = manager

	// 檢查背景是否需要整理
	{
		mlog.Debug("檢查換前背景結果, %p, %v", manager.BackgroundResult, manager.BackgroundResult)
		extRules := logic.GetExtraRuleByConditoin(slot.ExtraRuleConditionAfterBackground)
		for _, rule := range extRules {
			if !slot.MatchAfterBackground(logic.GameResultManager, rule, logic.BackgroundMatcher) {
				mlog.Debug("AfterBackground 檢查背景不符合條件, %v", rule.GetConditionValue())
				continue
			}

			switch slot.ExtraRuleAction(rule.GetAction()) {
			case slot.ExtraRuleActionReplaceBG, slot.ExtraRuleActionReplaceBGT, slot.ExtraRuleActionReplaceBGR, slot.ExtraRuleActionReplaceBGTR:
				var ror, symbol string
				var target int
				var prop, denominator int64
				var err error

				switch slot.ExtraRuleAction(rule.GetAction()) {
				case slot.ExtraRuleActionReplaceBG, slot.ExtraRuleActionReplaceBGT:
					ror, target, symbol, prop, denominator, err = slot.ParseExtraRuleActionReplaceBG(rule.GetActionValue())
				case slot.ExtraRuleActionReplaceBGR, slot.ExtraRuleActionReplaceBGTR:
					ror, target, symbol, prop, denominator, err = slot.ParseExtraRuleActionReplaceBGR(rule.GetActionValue(), logic.GameBetRate)
				}

				if err != nil {
					mlog.Warn("%s 解析錯誤. %v", rule.GetAction(), err)
					continue
				}

				if slot.ExtraRuleAction(rule.GetAction()) == slot.ExtraRuleActionReplaceBGT || slot.ExtraRuleAction(rule.GetAction()) == slot.ExtraRuleActionReplaceBGTR {
					prop = prop * logic.GameBetRate * logic.GameBetRate
				}

				mlog.Debug("%s, %s, %d, %s, %d, %d", rule.GetAction(), ror, target, symbol, prop, denominator)
				mlog.Debug("替換前背景結果, %v", manager.BackgroundResult)
				for i := 0; i < int(manager.Row); i++ {
					var bg []string
					for j := 0; j < int(manager.Reel); j++ {
						bg = append(bg, string(manager.BackgroundResult.Result[slot.Position{Reel: int32(j + 1), Row: int32(i + 1)}]))
					}
					mlog.Debug("第 %d 行: %s", i+1, strings.Join(bg, ", "))
				}

				slot.HandleReplaceBG(logic.GameResultManager, ror, target, symbol, prop, denominator)
			}
		}
	}

	// 紀錄開獎符號
	logic.LogBackgroundResult(logic.GameResultManager)

	// 檢查盤面是否需要整理
	{
		extRules := logic.GetExtraRuleByConditoin(slot.ExtraRuleConditionAppearSymbol)
		for _, rule := range extRules {

			ok, appears := slot.MatchAppearSymbol(logic.GameResultManager, rule)
			if !ok {
				mlog.Debug("AppearSymbol 不符合條件, %v", rule.GetConditionValue())
				continue
			}

			switch slot.ExtraRuleAction(rule.GetAction()) {
			case slot.ExtraRuleActionExpandReel:

				mlog.Debug("%s, %v", rule.GetAction(), appears)
				mlog.Debug("替換前輪軸結果")
				manager.RowMode()
				for i, row := range manager.GetSymbolString() {
					mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
				}

				slot.HandleExpandReel(logic.GameResultManager, rule, appears)
			case slot.ExtraRuleActionSwitchReelRandom:

				mlog.Debug("%s, %v", rule.GetAction(), appears)
				mlog.Debug("替換前輪軸結果")
				manager.RowMode()
				for i, row := range manager.GetSymbolString() {
					mlog.Debug("第 %d 行: %s", i+1, strings.Join(row, ", "))
				}

				slot.HandleSwitchReelRandom(logic.GameResultManager, rule, appears)
			}
		}
	}

	logic.LogGameResult(logic.GameResultManager)

	logic.CalcGameResult()

	logic.CalcGameAward()
}

// CalcGameResult 計算遊戲結果
func (logic *Logic) CalcGameResult() {
	logic.GamePayResult = logic.SlotLogicBase.CalcGameResult(logic.GameResultManager)
}

// CalcGameAward 計算遊戲獎勵
func (logic *Logic) CalcGameAward() {

	// 免費遊戲檢查是否有龍加倍
	if logic.FreeSpin != nil {

		var multi int64 = 1

		// 先檢查盤面是否出現百搭符號，決定加倍數量
		type MultiOpt struct {
			Value  int64
			Weight int
		}
		var opts []MultiOpt

		// 決定倍率選項
		{
			str := logic.FreeSpin.Origin.GetMultiplierValue()
			multis := strings.Split(str, ",")
			for _, row := range multis {
				vals := strings.Split(row, ":")

				m, _ := strconv.ParseFloat(vals[0], 64)
				w, _ := strconv.ParseFloat(vals[1], 64)

				multi := MultiOpt{Value: int64(m), Weight: int(w)}
				opts = append(opts, multi)
			}

			if len(opts) > 0 {
				// 排序由大到小
				sort.Slice(opts, func(i, j int) bool {
					return opts[i].Value > opts[j].Value
				})
			}
		}

		// 判斷每個Reel的加倍數量
		logic.GameResultManager.ReelMode()

		var multis []string
		for _, reel := range logic.GameResultManager.GameResult {
			var cnt int

			for _, symbol := range reel {
				// 檢查是否為百搭
				for _, attr := range logic.SymbolAttributes {
					if attr.GetIsWild() == 1 && symbol.GetSymbol() == attr.GetSymbol() {
						cnt++
					}
				}
			}

			// 若整條Reel都是百搭則使用最大倍數
			if cnt == int(logic.GameResultManager.Row) {
				multi = multi * opts[0].Value
				multis = append(multis, strconv.Itoa(int(opts[0].Value)))

				for _, symbol := range reel {
					for _, attr := range logic.SymbolAttributes {
						if attr.GetIsWild() == 1 && symbol.GetSymbol() == attr.GetSymbol() {
							logic.GameResultManager.MultiSymbol[int32(symbol.GetId())] = opts[0].Value
						}
					}
				}
			} else if cnt > 0 {
				r := new(prob.Random)
				for _, opt := range opts {
					r.Add(opt, opt.Weight)
				}
				t := r.Choose().(MultiOpt)

				multi = multi * t.Value
				multis = append(multis, strconv.Itoa(int(t.Value)))

				for _, symbol := range reel {
					for _, attr := range logic.SymbolAttributes {
						if attr.GetIsWild() == 1 && symbol.GetSymbol() == attr.GetSymbol() {
							logic.GameResultManager.MultiSymbol[int32(symbol.GetId())] = t.Value
						}
					}
				}
			} else {
				multis = append(multis, "1")
			}
		}
		var _multis []int
		for _, opt := range opts {
			_multis = append(_multis, int(opt.Value))
		}
		sort.Slice(_multis, func(i, j int) bool {
			return _multis[i] < _multis[j]
		})
		logic.GameLog.AddGameStep("百搭加倍選項 %v 倍, 總加倍: %s = %d 倍", _multis, strings.Join(multis, " * "), multi)

		// 結算
		for _, result := range logic.GamePayResult {

			if !result.IsMatch || result.Rule == nil {
				continue
			}

			switch result.GetPayType() {
			case slot.PayTypeChip, slot.PayTypeChipXRate:
				if result.FromBackground {
					// 計算贏得的金額
					base, win := result.CalcWinMoney(logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate)

					logic.GameWinMoney += win
					logic.FreeSpin.WinMoney += win

					logic.LogPayResultChip(result, base, win)

					continue
				}

				// 計算贏得的金額
				result.Multi = int32(multi)
				result.MultiType = slot.MultiTypeApearWild
				base, win := result.CalcWinMoney(logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate)

				logic.GameWinMoney += win
				logic.FreeSpin.WinMoney += win

				logic.LogPayResultChip(result, base, win)

			case slot.PayTypeBonus:

				bonus := logic.WinBonus(result.Rule)
				if bonus == nil {
					mlog.Debug("No Bonus for [%s]", result.Rule.GetPayType())
					continue
				}

				switch slot.PayType(bonus.GetRewardType()) {
				default:
					continue
				case slot.PayTypeChip, slot.PayTypeChipXRate:
					win, rate := logic.CalcChipMoney(slot.PayType(bonus.GetRewardType()), bonus.GetRewardValue())
					result.WinMoney = win
					if win == 0 {
						continue
					}
					logic.GameWinMoney += win
					logic.FreeSpin.WinMoney += win

					var base int64 = rate * logic.GameBetMoney
					if slot.PayType(bonus.GetRewardType()) == slot.PayTypeChipXRate {
						base *= logic.GameBetRate
					}

					logic.LogPayResultChip(result, base, win)
				}
			}
		}
	} else {
		// 一般遊戲錢照算
		logic.GameWinMoney = logic.SlotLogicBase.CalcGameAward(logic.GamePayResult, logic.GameBetMoney, logic.GameBetRate)
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

		switch result.GetPayType() {
		case slot.PayTypeMysteryJPG, slot.PayTypeMysteryJPGR:

			// 在免費遊戲中僅給予機會
			if logic.IsFreeSpin() {

				c, err := slot.NewJackpotGameChance(logic.GameResultManager, result.Rule, logic.GameBetMoney, logic.GameBetRate)
				if err != nil {
					mlog.Warn("建立 Jackpot Game 機會錯誤 %v", err)
					continue
				}

				logic.JackpotGameChances = append(logic.JackpotGameChances, c)

				logic.SendIncreaseGameTimesMessage(logic.GameUser, string(slot.GameModeJackpotGame), 0, 1, "")

				logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 JackpotGame, 正在免費遊戲中, 結束後將決定獎項, 剩餘 %d 次", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, len(logic.JackpotGameChances))

				logic.RecordJackpotGameChance(logic.GameUser, c)

				continue
			}

			jpg := logic.WinMysteryJPG(result.Rule)

			if jpg == nil {
				continue
			}

			logic.JackpotGame = jpg

			logic.RecordJackpotGame(logic.GameUser)

			logic.SendIncreaseGameTimesMessage(logic.GameUser, string(slot.GameModeJackpotGame), 0, 1, "")

		case slot.PayTypeSelection:

			selection := logic.WinSelection(result.Rule)

			if selection == nil {
				continue
			}

			logic.SelectChances = append(logic.SelectChances, selection)

			logic.RecordSelectGameTimes(logic.GameUser)

		case slot.PayTypeAddTimes:

			if logic.FreeSpin == nil {
				mlog.Error("增加免費遊戲次數錯誤, 沒有免費遊戲")
				continue
			}

			f, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)

			logic.FreeSpin.Count += int(f)

			logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得免費遊戲次數, 增加 %d 次, 剩餘 %d 次", result.SymbolName, result.Rule.GetLinkType(), result.PayCount, int(f), logic.FreeSpin.Count)

			logic.SendIncreaseGameTimesMessage(logic.GameUser, string(slot.GameModeFreeSpin), int32(logic.FreeSpin.Origin.GetId()), int32(f), "")

		case slot.PayTypeJackpot:

			_, win := logic.WinJackpot(result.Rule)
			result.WinMoney = win

			if logic.IsFreeSpin() {
				logic.FreeSpin.WinMoney += win
			}

			logic.GameWinMoney += win
			logic.GameWinJackpot = win
		}
	}
}
