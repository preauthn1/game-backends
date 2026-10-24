package baselogic

import (
	"sort"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/prob"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Bet 判斷下注是否
func (logic *SlotLogicBase) Bet(user *gamelogic.GameUser, money int64, rate int64) (ok bool, retcode int32, betmoney int64) {

	// 檢查狀態是否可下注
	if logic.GameState != slot.GameStateBet {
		return false, slot.BetStateStepError, logic.GamePayMoney
	}

	// 檢查下注金額是否合法
	var moneyOK, rateOK bool
	for _, m := range logic.BetCounts {
		if int64(m) == money {
			moneyOK = true
			break
		}
	}

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		for _, r := range logic.BetRates {
			if int64(r) == rate {
				rateOK = true
				break
			}
		}
	case slot.PayLineModeFixed:
		rate = int64(len(logic.SlotPayLine))
		rateOK = true
	}

	if !moneyOK || !rateOK {
		mlog.Warn("玩家 %v 非法下注，金額: %d，倍率: %d", user, money, rate)
		return false, slot.BetStateIllegal, logic.GamePayMoney
	}

	logic.GameBetMoney = money
	logic.GameBetRate = rate
	logic.GamePayMoney = money * rate

	if logic.FreeSpin != nil && logic.FreeSpin.Count > 0 {
		logic.GameBetMoney = logic.FreeSpin.BetMoney
		logic.GamePayMoney = 0
		logic.FreeSpin.Count--
		logic.FreeSpin.Used++
	}

	// 檢查最後金額是否為負數
	if logic.GamePayMoney < 0 {
		mlog.Warn("玩家 %v 非法下注，付款金額小於零 %d", user.UserID, logic.GamePayMoney)
		return false, slot.BetStateIllegal, logic.GamePayMoney
	}

	// 檢查玩家金額
	if user.Score < logic.GamePayMoney {
		return false, slot.BetStateMoneyNotEnough, logic.GamePayMoney
	}

	// 檢查最低攜帶金額
	if user.Score < logic.MinCarryMoney && logic.FreeSpin == nil {
		return false, slot.BetStateMoneyLessMin, logic.GamePayMoney
	}

	return true, slot.BetStateSuccess, logic.GamePayMoney
}

// PrepareSpin 取得SymbolGroupID, BgGroupID, 累積JackpotPool
func (logic *SlotLogicBase) PrepareSpin() (symbolGroupID, bgGroupID int32) {
	if logic.FreeSpin != nil {
		symbolGroupID = logic.FreeSpin.SymbolGroupID
		bgGroupID = logic.FreeSpin.BgGroupID

		logic.GameLog.AddGameStep("玩家 %d; 底注: %d, 倍率: %d, 免費遊戲剩餘次數: %d", logic.GameUser.UserID, logic.FreeSpin.BetMoney, logic.FreeSpin.BetRate, logic.FreeSpin.Count)
	} else {
		symbolGroupID = logic.DefaultSymbolGroupID
		bgGroupID = logic.DefaultBgGroupID

		// 依照下注倍率選擇MainGameTable
		for _, table := range logic.MainGameTable {
			f, _ := strconv.ParseFloat(table.GetConditionValue(), 64)
			if int64(f) == logic.GameBetRate {
				symbolGroupID = int32(table.GetSymbolGroupId())
				bgGroupID = int32(table.GetGoldReelGroupId())
				break
			}
		}

		logic.GameLog.AddGameStep("玩家 %d; 底注: %d, 倍率: %d, 花費: %d", logic.GameUser.UserID, logic.GameBetMoney, logic.GameBetRate, logic.GamePayMoney)

		logic.IncreaseJackpotPool()

		logic.SendJackpotMessageToUser(logic.GameUser)
	}
	return
}

// CalcGameResult 計算遊戲結果
func (logic *SlotLogicBase) CalcGameResult(grm *slot.GameResultManager) []*slot.PayResult {

	results := make([]*slot.PayResult, 0)

	// 依照SlotPayTable檢查中了哪些
	for _, rule := range logic.SlotPayTable {
		match, result := logic.PaylineMatcher.IsMatch(grm, rule)
		if !match {
			continue
		}
		results = append(results, result)
	}

	// 檢查 Background 獎勵
	for _, rule := range logic.SlotPayTable {
		match, result := logic.BackgroundMatcher.IsMatch(grm, rule)
		if !match {
			continue
		}
		results = append(results, result)
	}

	// 所有賠線中獎規則
	lineRewareMap := make(map[slot.LinkType]map[uint32]*slot.PayResult)

	// 依照連線個數做排序，多到少
	sort.Slice(results, func(i, j int) bool {
		return results[i].PayCount > results[j].PayCount
	})

	// 過濾已經出現過的連線，並取贏多的那筆規則
	for _, result := range results {
		if !result.IsMatch || result.Rule == nil {
			continue
		}
		if result.Rule.GetPayType() == slot.PayTypeJackpot.String() {
			for _, line := range result.SymbolPayLines {
				if _, ok := lineRewareMap[slot.LinkType(result.Rule.GetLinkType())]; !ok {
					lineRewareMap[slot.LinkType(result.Rule.GetLinkType())] = make(map[uint32]*slot.PayResult)
				}
				lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()] = result
			}
		}
		if result.Rule.GetPayType() != slot.PayTypeChip.String() {
			continue
		}

		for _, line := range result.SymbolPayLines {
			if _, ok := lineRewareMap[slot.LinkType(result.Rule.GetLinkType())]; !ok {
				lineRewareMap[slot.LinkType(result.Rule.GetLinkType())] = make(map[uint32]*slot.PayResult)
			}

			if _, ok := lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()]; !ok {
				lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()] = result
				continue
			}

			orgResult := lineRewareMap[slot.LinkType(result.Rule.GetLinkType())][line.PayLine.Origin.GetLineId()]

			symbols1 := strings.Split(result.Rule.GetSymbol(), ",")
			symbols2 := strings.Split(orgResult.Rule.GetSymbol(), ",")

			if len(symbols1) != len(symbols2) {
				if len(symbols1) < len(symbols2) {
					orgResult.RemoveLine(line.PayLine.Origin.GetLineId())
				} else {
					result.RemoveLine(line.PayLine.Origin.GetLineId())
				}
				continue
			}

			f1, _ := strconv.ParseFloat(result.Rule.GetPayout(), 64)
			reware1 := int64(f1)

			f2, _ := strconv.ParseFloat(orgResult.Rule.GetPayout(), 64)
			reware2 := int64(f2)

			if reware1 > reware2 {
				orgResult.RemoveLine(line.PayLine.Origin.GetLineId())
			} else {
				result.RemoveLine(line.PayLine.Origin.GetLineId())
			}
		}
	}

	tmp := make([]*slot.PayResult, 0, len(results))
	// 整理沒有連線(被移除後)的規則
	for _, result := range results {
		if !result.IsMatch || result.Rule == nil {
			continue
		}
		if result.Rule.GetPayType() != slot.PayTypeChip.String() {
			tmp = append(tmp, result)
			continue
		}
		if result.WayCount > 0 {
			tmp = append(tmp, result)
		}
	}

	return tmp
}

// CalcGameAward 計算遊戲獎勵
func (logic *SlotLogicBase) CalcGameAward(results []*slot.PayResult, betMoney, betRate int64) int64 {

	var winMoney int64

	for _, result := range results {

		if !result.IsMatch || result.Rule == nil {
			continue
		}

		switch result.GetPayType() {
		case slot.PayTypeChip, slot.PayTypeChipXRate:
			// 計算贏得的金額
			base, win := result.CalcWinMoney(betMoney, betRate)
			result.WinMoney = win
			winMoney += win

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
				winMoney += win

				var base int64 = rate * logic.GameBetMoney
				if slot.PayType(bonus.GetRewardType()) == slot.PayTypeChipXRate {
					base *= logic.GameBetRate
				}

				logic.LogPayResultChip(result, base, win)
			}
		}
	}

	return winMoney
}

// CalcChipMoney 計算Chip or ChipXRate的錢
func (logic *SlotLogicBase) CalcChipMoney(paytype slot.PayType, value string) (win int64, rate int64) {
	switch paytype {
	default:
		return 0, 0
	case slot.PayTypeChip, slot.PayTypeChipXRate:
		f, _ := strconv.ParseFloat(value, 64)

		var betMoney int64 = logic.GameBetMoney
		var betRate int64 = 1
		if paytype == slot.PayTypeChipXRate {
			betRate = logic.GameBetRate
		}

		return betMoney * int64(f) * int64(betRate), int64(f)
	}
}

// CalcJackpot 傳入中獎的Jackpot處理
func (logic *SlotLogicBase) CalcJackpot(jackpot *slot.Jackpot) (winMoney int64) {
	return logic.CalcJackpotByMoney(jackpot, logic.GameBetMoney*logic.GameBetRate, int64(logic.MaxBetCount)*int64(logic.MaxBetRate))
}

// CalcJackpotByMoney 傳入中獎的Jackpot處理
func (logic *SlotLogicBase) CalcJackpotByMoney(jackpot *slot.Jackpot, currentBet, maxBet int64) (winMoney int64) {
	win := jackpot.GetCurrentPoolByBet(currentBet, maxBet)

	jackpot.AwardTotal += win

	if jackpot.Origin.GetIsProgress() == 1 {
		if jackpot.IsNeedReset() {
			// logic.GameLog.AddGameStep("重置獎池 %s", jackpot.Level.String())
			mlog.Debug("重置獎池[%s]", jackpot.Level.String())

			logic.UpdateJackpotPoolNoLink(0, -jackpot.TotalContribution, -jackpot.AwardTotal+win, jackpot.Level)
			jackpot.Reset()
		} else {
			// logic.GameLog.AddGameStep("更新獎池 %s 出獎金額: %d", jackpot.Level.String(), win)
			mlog.Debug("更新獎池[%s] 出獎金額: %d", jackpot.Level.String(), win)

			logic.UpdateJackpotPoolNoLink(0, 0, win, jackpot.Level)
		}
	}

	return win
}

// WinFreeSpin 處理贏得FreeSpin
func (logic *SlotLogicBase) WinFreeSpin(rule *protodata.SlotPayTable) *slot.FreeSpin {
	// 取得贏得FreeSpin的GroupID
	f, err := strconv.ParseFloat(rule.GetPayout(), 64)
	if err != nil {
		mlog.Debug("Parse [%s] error %v", rule.GetPayType(), err)
		return nil
	}

	freeSpinGroupID := int32(f)

	var freeSpinTables []*protodata.FreeSpinTable
	var freeSpinTable *protodata.FreeSpinTable

	// 取得FreeSpinTable
	for _, table := range logic.FreeSpinTable {
		if table.GetGroupId() != freeSpinGroupID {
			continue
		}
		freeSpinTables = append(freeSpinTables, table)
	}

	if len(freeSpinTables) == 0 {
		mlog.Warn("查無FreeSpinTable, FreeSpinGroupID: %d", freeSpinGroupID)
		return nil
	}

	// 依照倍率取得正確FreeSpinTable
	for _, table := range freeSpinTables {
		f, _ := strconv.ParseFloat(table.GetConditionValue(), 64)
		if int64(f) == logic.GameBetRate {
			freeSpinTable = table
			break
		}
	}

	if freeSpinTable == nil {
		mlog.Warn("查無FreeSpinTable, FreeSpinGroupID: %d", freeSpinGroupID)
		return nil
	}

	// 檢查本身是否是FreeSpin
	if logic.FreeSpin != nil {
		if logic.FreeSpin.Origin != freeSpinTable {
			mlog.Warn("贏得的FreeSpin與原本的FreeSpin不同")
			return nil
		}

		logic.FreeSpin.Count += int(freeSpinTable.GetFreeSpinCount())

	} else {
		logic.FreeSpin = slot.NewFreeSpin(freeSpinTable, logic.SlotSymbolDistribution, logic.GameBetMoney, logic.GameBetRate)
	}

	logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 FreeSpin(%d), Count: %d, 剩餘次數: %d", rule.GetSymbol(), rule.GetLinkType(), rule.GetCount(), logic.FreeSpin.Origin.GetFreeSymbolGroupId(), logic.FreeSpin.Origin.GetFreeSpinCount(), logic.FreeSpin.Count)

	return logic.FreeSpin
}

// WinMysteryJPG 處理贏得MysteryJPGR該做的事
func (logic *SlotLogicBase) WinMysteryJPG(rule *protodata.SlotPayTable) *slot.JackpotGame {

	var linkprop, nolinkprop, nowinprop, denominator int64
	var err error
	switch slot.PayType(rule.GetPayType()) {
	case slot.PayTypeMysteryJPG:
		linkprop, nolinkprop, denominator, err = slot.ParsePayoutMysteryJPG(rule.GetPayout())
	case slot.PayTypeMysteryJPGR:
		linkprop, nolinkprop, denominator, err = slot.ParsePayoutMysteryJPGR(rule.GetPayout(), logic.GameBetRate)
	}

	if err != nil {
		mlog.Debug("Parse [%s] error %v", rule.GetPayType(), err)
		return nil
	}

	// 連線水池機率要乘上總下注分數
	linkprop = linkprop * logic.GameBetMoney * logic.GameBetRate
	nowinprop = denominator - nolinkprop - linkprop

	type PropType int32
	const (
		LinkType PropType = iota
		NoLinkType
		NoWinType
	)

	// linkprop need x total bet
	r := new(prob.Random)

	// 若分母為0則指骰兩種
	if denominator == 0 {
		if linkprop < 0 || nolinkprop < 0 {
			mlog.Warn("出現負數機率, LinkJP: %d, NoLinkJP: %d", linkprop, nolinkprop)
			return nil
		}
		r.Add(LinkType, int(linkprop))
		r.Add(NoLinkType, int(nolinkprop))
	} else {
		if linkprop < 0 || nolinkprop < 0 || nowinprop < 0 {
			mlog.Warn("出現負數機率, LinkJP: %d, NoLinkJP: %d, NoWin: %d", linkprop, nolinkprop, nowinprop)
			return nil
		}
		r.Add(LinkType, int(linkprop))
		r.Add(NoLinkType, int(nolinkprop))
		r.Add(NoWinType, int(nowinprop))
	}

	t := r.Choose().(PropType)

	if !conf.AnalysisMode {
		mlog.Debug("WinMysteryJPG, LinkProp(%v): %d, NoLinkProp(%v): %d, NoWinProp(%v): %d, 骰出: %v", LinkType, linkprop, NoLinkType, nolinkprop, NoWinType, nowinprop, t)
		mlog.Debug("%+v", r)
	}

	var jpg *slot.JackpotGame

	switch t {
	case NoWinType:
		mlog.Debug("JackpotGame 未中獎")
		return nil
	case LinkType:
		// 取出連線Jackpot獎池
		var jackpots, _jackpots []*slot.Jackpot
		for _, jp := range logic.Jackpots {
			if jp.Origin.GetIsMultiLevel() != 1 {
				continue
			}
			if jp.Origin.GetIsLinked() == 1 {
				jackpots = append(jackpots, jp)
			} else {
				_jackpots = append(_jackpots, jp)
			}
		}
		if len(jackpots) == 0 {
			mlog.Debug("無連線型Jackpot, 跳過 Jackpot Game")
			return nil
		}

		jpg = slot.NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, int32(logic.GameBetRate), logic.GameBetMoney)

		jpg.WinJackpot = jpg.RandomJackpot()

		jpg.Jackpots = append(jpg.Jackpots, _jackpots...)

		jpg.Process = jpg.GenerateProcess()
	case NoLinkType:
		// 取出連線Jackpot獎池
		var jackpots, _jackpots []*slot.Jackpot

		for _, jp := range logic.Jackpots {
			if jp.Origin.GetIsMultiLevel() != 1 {
				continue
			}

			if jp.Origin.GetIsLinked() == 0 {
				jackpots = append(jackpots, jp)
			} else {
				_jackpots = append(_jackpots, jp)
			}
		}

		if len(jackpots) == 0 {
			mlog.Debug("無非連線型Jackpot, 跳過 Jackpot Game")
			return nil
		}

		jpg = slot.NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, int32(logic.GameBetRate), logic.GameBetMoney)

		jpg.WinJackpot = jpg.RandomJackpot()

		jpg.Jackpots = append(jpg.Jackpots, _jackpots...)

		jpg.Process = jpg.GenerateProcess()
	}

	if jpg == nil {
		return nil
	}

	jpg.WinMoney = logic.CalcJackpot(jpg.WinJackpot)
	jpg.Origin = rule

	logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 JackpotGame[%s], 獎金: %d, 將於翻牌結束後結算", rule.GetSymbol(), rule.GetLinkType(), rule.GetCount(), jpg.WinJackpot.Level.String(), jpg.WinMoney)

	return jpg
}

// WinSelection 處理贏得Selection該做的事
func (logic *SlotLogicBase) WinSelection(rule *protodata.SlotPayTable) *slot.SelectChance {
	selection, err := slot.NewSelectChance(&protodata.ProtoData{
		SelectionTable: logic.SelectionTable,
		FreeSpinTable:  logic.FreeSpinTable,
		RespinTable:    logic.RespinTable,
	}, logic.GameResultManager, rule, logic.GameBetMoney, logic.GameBetRate)
	if err != nil {
		mlog.Error("建立選擇遊戲錯誤. %v", err)
		return nil
	}

	var opts []string
	// 依照本次下注倍率選擇該次選擇遊戲選項
	for _, row := range selection.Options {
		opts = append(opts, row.String())
	}

	logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得選擇機會(%d), 選項: %s", rule.GetSymbol(), rule.GetLinkType(), rule.GetCount(), selection.GroupID, strings.Join(opts, "|"))

	return selection
}

// WinJackpot 處理贏得Jackpot該做的事
func (logic *SlotLogicBase) WinJackpot(rule *protodata.SlotPayTable) (jackpot *slot.Jackpot, winMoney int64) {
	f, _ := strconv.ParseFloat(rule.GetPayout(), 64)

	// 找 Jackpot
	for _, jp := range logic.Jackpots {
		if jp.Origin.GetId() == int32(f) {
			jackpot = jp
		}
	}

	if jackpot == nil {
		return nil, 0
	}

	win := logic.CalcJackpot(jackpot)

	logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 Jackpot[%s] 獎金: %d", rule.GetSymbol(), rule.GetLinkType(), rule.GetCount(), jackpot.Level.String(), win)

	return jackpot, win
}

// WinBonus 贏得Bonus選擇一筆
func (logic *SlotLogicBase) WinBonus(rule *protodata.SlotPayTable) (bonus *protodata.BonusTable) {

	f, err := strconv.ParseFloat(rule.GetPayout(), 64)
	if err != nil {
		mlog.Debug("Parse [%s] error %v", rule.GetPayType(), err)
		return nil
	}

	var bonuies []*protodata.BonusTable
	for _, row := range logic.BonusTable {
		// Filter by GroupID
		if row.GetGroupId() != int32(f) {
			continue
		}

		// Filter by Condition
		switch row.GetCondition() {
		default:
		case slot.ConditionRate:
			f, err := strconv.ParseFloat(row.GetConditionValue(), 32)
			if err != nil {
				mlog.Debug("Parse BonusTable ConditionValue error %v", err)
				continue
			}
			if logic.GameBetRate != int64(f) {
				continue
			}
		case slot.ConditionAmount:
		}

		bonuies = append(bonuies, row)
	}

	if len(bonuies) == 0 {
		mlog.Debug("No Bonus for [%s], GroupID: %d", rule.GetPayType(), int32(f))
		return nil
	}

	r := new(prob.Random)
	for _, row := range bonuies {
		r.Add(row, int(row.GetPerProb()))
	}

	bonus = r.Choose().(*protodata.BonusTable)
	return
}

// WinPickGame 贏得PickGame
func (logic *SlotLogicBase) WinPickGame(rule *protodata.SlotPayTable, size int32, log bool) (game *slot.PickGame) {

	f, err := strconv.ParseFloat(rule.GetPayout(), 64)
	if err != nil {
		mlog.Debug("Parse [%s] error %v", rule.GetPayType(), err)
		return nil
	}

	var picktb *protodata.PickGameTable
	for _, row := range logic.PickGameTable {
		if int32(row.GetId()) == int32(f) {
			picktb = row
			break
		}
	}

	if picktb == nil {
		mlog.Debug("No PickGame for [%s], ID: %d", rule.GetPayType(), int32(f))
		return nil
	}

	var bonuies []*protodata.BonusTable
	for _, row := range logic.BonusTable {
		if row.GetGroupId() != int32(picktb.GetRewardValue()) {
			continue
		}
		bonuies = append(bonuies, row)
	}

	if len(bonuies) == 0 {
		mlog.Debug("No Bonus for PickGame(%d), GroupID: %d", picktb.GetId(), picktb.GetRewardValue())
		return nil
	}

	game = slot.NewPickGame(bonuies, size, int32(picktb.GetPickCount()), logic.GameBetMoney, logic.GameBetRate)
	game.Origin = rule

	if log {
		logic.GameLog.AddGameStep("開獎符號: %s:%s:%d, 贏得 PickGame[%d] 獎金: %d", rule.GetSymbol(), rule.GetLinkType(), rule.GetCount(), picktb.GetId(), game.WinMoney)
	}

	return
}

// IsGoldBall 判斷符號是否為金球
func (logic *SlotLogicBase) IsGoldBall(symbol string) bool {
	for _, attr := range logic.SymbolAttributes {
		if attr.GetIsGoldBall() == 1 && attr.GetSymbol() == symbol {
			return true
		}
	}
	return false
}
