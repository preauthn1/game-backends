package xmbx

import (
	"math/rand"
	"strconv"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// LightningAward 獎勵
type LightningAward struct {
	WinMoney    int64             `json:"win_money"`
	Position    slot.Position     `json:"position"`
	Type        slot.PayType      `json:"type"`
	JackpotType slot.JackpotLevel `json:"jp_type"`
	Symbol      string            `json:"symbol"`
}

// LightningStep 每次Step相關資訊
type LightningStep struct {
	Origin            *protodata.RespinTable  `json:"-"`
	Rule              *protodata.SlotPayTable `json:"-"`
	RespinID          int32                   `json:"respin_id"`
	RuleID            int32                   `json:"rule_id"`
	GameResultManager *slot.GameResultManager `json:"game_result"`
	LastChance        int                     `json:"last_chance"`
	Awards            []*LightningAward       `json:"awards"`
	WinMoney          int64                   `json:"win_money"` // 總共贏得金額
}

// NewLightningStep create LightningStep
func NewLightningStep(table *protodata.RespinTable, rule *protodata.SlotPayTable, result *slot.GameResultManager, lastChance int) *LightningStep {
	step := new(LightningStep)

	if table != nil {
		step.Origin = table
		step.RespinID = int32(table.GetId())
	}

	if rule != nil {
		step.Rule = rule
		step.RuleID = rule.GetId()
	}

	step.GameResultManager = result
	step.LastChance = lastChance

	return step
}

func (s *LightningStep) String() string {
	return s.GameResultManager.String()
}

// LightningGame Lightning 模式遊戲
type LightningGame struct {
	Logic      *Logic           `json:"-"`
	Steps      []*LightningStep `json:"steps"`        // 每次轉出的結果
	ShowStep   int              `json:"show_step"`    // 以表演完階段
	IsFreeGame bool             `json:"is_free_game"` // 是否由FreeGame贏得

	Respined bool `json:"respined"`
	BetMoney int64
	BetRate  int64
}

// NewLightningGame create LightningGame
func NewLightningGame(logic *Logic, isFreeGame bool) *LightningGame {
	g := new(LightningGame)
	g.Logic = logic
	g.IsFreeGame = isFreeGame
	g.BetMoney = logic.GameBetMoney
	g.BetRate = logic.GameBetRate
	return g
}

// GetLastStep 取得最後一筆Step
func (g *LightningGame) GetLastStep() *LightningStep {
	if len(g.Steps) > 0 {
		return g.Steps[len(g.Steps)-1]
	}
	return nil
}

// AddStep 加入一個Step
func (g *LightningGame) AddStep(result *LightningStep) {
	if result == nil {
		return
	}
	g.Steps = append(g.Steps, result)
}

// Spin 轉出一個新輪軸結果加入
func (g *LightningGame) Spin(symbols ...string) {

	last := g.GetLastStep()
	if last == nil {
		mlog.Error("查無前一次輪軸結果")
		return
	}

	// 特別塞入的符號，找非金球空位塞
	if len(symbols) > 0 {
		last.GameResultManager.RowMode()
		for i, symbol := range symbols {
			var inserted bool
			var retryLimit int = 100
			var retry int

			for ; !inserted; retry++ {
				//  骰一個ReelID, RowID, 檢查該位置是不是金球, 不是金球就把符號塞入

				randomReelID := rand.Intn(int(last.GameResultManager.Reel))
				randomRowID := rand.Intn(int(last.GameResultManager.Row))

				mlog.Debug("randomReelID: %d, randomRowID: %d, Retry: %d", randomReelID, randomRowID, retry)

				if !g.IsGoldBall(last.GameResultManager.GameResult[randomRowID][randomReelID]) {

					smb := &protodata.SlotSymbolDistribution{Id: proto.Uint32(uint32(100000 + i)), Symbol: proto.String(symbol)}

					last.GameResultManager.GameResult[randomRowID][randomReelID] = smb

					mlog.Debug("加入符號 %s, ReelID: %d, RowID: %d, Retry: %d", symbol, randomReelID+1, randomRowID+1, retry)

					inserted = true
					break
				}

				if retry > retryLimit {
					break
				}
			}

		}
	}

	symbolGroupID := int32(last.Origin.GetRespinValue())
	last.LastChance--
	lastChance := last.LastChance

	// 建立全新的輪軸結果
	manager, _ := slot.NewGameResultManager(g.Logic.DefaultReelNum, g.Logic.DefaultRowNum)
	manager.GenerateGameResult(symbolGroupID, g.Logic.SlotSymbolDistribution)

	// 有上一局，把前一次的金球保留
	if last := g.GetLastStep(); last != nil {
		last.GameResultManager.RowMode()
		manager.RowMode()

		for rowID, row := range last.GameResultManager.GameResult {
			for reelID, symbol := range row {
				if g.IsGoldBall(symbol) {
					manager.GameResult[rowID][reelID] = symbol
				}
			}
		}
	}

	// 檢查需要重轉的盤面的規則
	extRules := g.Logic.GetExtraRuleByConditoin(slot.ExtraRuleConditionAfterSpin, slot.ExtraRuleActionRespin)
	for _, rule := range extRules {
		linkType, symbol, count, err := slot.ParseExtraRuleConditionAfterSpin(rule.GetConditionValue())
		if err != nil {
			mlog.Debug("%v", err)
			continue
		}

		payrule := &protodata.SlotPayTable{LinkType: proto.String(linkType), Symbol: proto.String(symbol), Count: proto.Int32(int32(count))}
		if match, _ := g.Logic.PaylineMatcher.IsMatch(manager, payrule); match {
			// logic.GameLog.AddGameStep("因關閉連線水池，重轉一次")

			if g.Respined {
				mlog.Debug("盤面符合重轉規則: %v, 已決定持續重轉", rule.GetConditionValue())
				g.Spin()
				return
			}

			mlog.Debug("盤面符合重轉規則: %v, 決定是否保留或重轉", rule.GetConditionValue())

			prop, denominator, err := slot.ParseExtraRuleActionRespin(rule.GetActionValue())
			if err != nil {
				mlog.Debug("%v", err)
				continue
			}
			random := rand.Intn(int(denominator))
			if int64(random) < prop {
				mlog.Debug("%d of %d, dice %d, by pass", prop, denominator, random)
				continue
			}

			mlog.Debug("%d of %d, dice %d, respin", prop, denominator, random)

			g.Respined = true

			last.LastChance++
			g.Spin()
			return
		}
	}

	var origin *protodata.RespinTable
	var rule *protodata.SlotPayTable
	var jpRule *protodata.SlotPayTable

	// 檢查是否有中獎
	results := g.Logic.SlotLogicBase.CalcGameResult(manager)

	for _, result := range results {

		switch result.Rule.GetUseType() {
		case "normal":
			if g.IsFreeGame {
				continue
			}
		case "freespin":
			if !g.IsFreeGame {
				continue
			}
		}

		if result.GetPayType() == slot.PayTypeLightning && result.Rule != last.Rule {
			rule = result.Rule
			f, _ := strconv.ParseFloat(rule.GetPayout(), 64)
			for _, respin := range g.Logic.RespinTable {
				if respin.GetGroupId() == int32(f) {
					origin = respin
				}
			}
		}

		if result.GetPayType() == slot.PayTypeJackpot {
			lastChance = 0
			rule = nil
			jpRule = result.Rule
		}
	}

	manager.RowMode()

	if rule != nil {
		lastChance = int(origin.GetRespinCount())
	} else if last.LastChance != 0 {
		rule = last.Rule
		origin = last.Origin
	}

	step := NewLightningStep(origin, rule, manager, lastChance)

	g.AddStep(step)

	if lastChance > 0 {
		g.Spin()
		return
	}

	// 計算獎勵
	// 計算每個金球金額
	step.GameResultManager.RowMode()
	for rowID, row := range step.GameResultManager.GameResult {
		for reelID, symbol := range row {
			for _, attr := range g.Logic.SymbolAttributes {
				if attr.GetIsGoldBall() == 1 && attr.GetSymbol() == symbol.GetSymbol() {
					switch slot.PayType(attr.GetGoldballType()) {
					case slot.PayTypeChip:
						f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)

						win := g.Logic.GameBetMoney * int64(f)

						award := &LightningAward{
							WinMoney: win,
							Type:     slot.PayTypeChip,
							Symbol:   symbol.GetSymbol(),
							Position: slot.Position{
								Reel: int32(reelID + 1),
								Row:  int32(rowID + 1),
							},
						}

						step.Awards = append(step.Awards, award)
						step.WinMoney += win
					case slot.PayTypeChipXRate:
						f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)

						win := g.Logic.GameBetMoney * g.Logic.GameBetRate * int64(f)

						award := &LightningAward{
							WinMoney: win,
							Type:     slot.PayTypeChipXRate,
							Symbol:   symbol.GetSymbol(),
							Position: slot.Position{
								Reel: int32(reelID + 1),
								Row:  int32(rowID + 1),
							},
						}

						step.Awards = append(step.Awards, award)
						step.WinMoney += win
					case slot.PayTypeJackpot:
						f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)

						var jackpot *slot.Jackpot
						// 找 Jackpot
						for _, jp := range g.Logic.Jackpots {
							if jp.Origin.GetId() == int32(f) {
								jackpot = jp
							}
						}

						if jackpot != nil {

							win := jackpot.GetCurrentPoolByBet(g.Logic.GameBetMoney*g.Logic.GameBetRate, int64(g.Logic.MaxBetCount)*int64(g.Logic.MaxBetRate))

							award := &LightningAward{
								WinMoney:    win,
								Type:        slot.PayTypeJackpot,
								Symbol:      symbol.GetSymbol(),
								JackpotType: jackpot.Level,
								Position: slot.Position{
									Reel: int32(reelID + 1),
									Row:  int32(rowID + 1),
								},
							}

							step.Awards = append(step.Awards, award)
							step.WinMoney += win
						}
					}
				}
			}
		}
	}

	// 有中最大Jackpot
	if jpRule != nil {
		f, _ := strconv.ParseFloat(jpRule.GetPayout(), 64)

		var jackpot *slot.Jackpot
		// 找 Jackpot
		for _, jp := range g.Logic.Jackpots {
			if jp.Origin.GetId() == int32(f) {
				jackpot = jp
			}
		}

		if jackpot != nil {

			win := jackpot.GetCurrentPoolByBet(g.Logic.GameBetMoney*g.Logic.GameBetRate, int64(g.Logic.MaxBetCount)*int64(g.Logic.MaxBetRate))

			award := &LightningAward{
				WinMoney:    win,
				Type:        slot.PayTypeJackpot,
				JackpotType: jackpot.Level,
			}

			step.Awards = append(step.Awards, award)
			step.WinMoney += win
			step.LastChance = 0
		}
	}
}

// IsGoldBall 檢查是否為金球
func (g *LightningGame) IsGoldBall(symbol *protodata.SlotSymbolDistribution) bool {

	for _, attr := range g.Logic.SymbolAttributes {
		if attr.GetIsGoldBall() == 1 && attr.GetSymbol() == symbol.GetSymbol() {
			return true
		}
	}

	return false
}
