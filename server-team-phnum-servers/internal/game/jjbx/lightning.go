package jjbx

import (
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// LightningAward 獎勵
type LightningAward struct {
	WinMoney    int64                             `json:"win_money"`
	Position    slot.Position                     `json:"position"`
	Type        slot.PayType                      `json:"type"`
	JackpotType slot.JackpotLevel                 `json:"jp_type"`
	Symbol      *protodata.SlotSymbolDistribution `json:"symbol"`
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
	Log               []string                `json:"log"`
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

// PrintAward 紀錄獎勵
func (s *LightningStep) PrintAward(logic *Logic) {
	for _, award := range s.Awards {
		switch award.Type {
		case slot.PayTypeChip:
			logic.GameLog.AddGameStep("位置: %d_%d, 符號: %s, 獎勵類型: 倍率獎金, 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.WinMoney)
			mlog.Debug("位置: %d_%d, 符號: %s, 獎勵類型: 倍率獎金, 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.WinMoney)
		case slot.PayTypeChipXRate:
			logic.GameLog.AddGameStep("位置: %d_%d, 符號: %s, 獎勵類型: 倍率獎金, 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.WinMoney)
			mlog.Debug("位置: %d_%d, 符號: %s, 獎勵類型: 倍率獎金, 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.WinMoney)
		case slot.PayTypeJackpot:
			switch award.JackpotType {
			default:
				logic.GameLog.AddGameStep("位置: %d_%d, 符號: %s, 獎勵類型: Jackpot[%s], 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.JackpotType.String(), award.WinMoney)
				mlog.Debug("位置: %d_%d, 符號: %s, 獎勵類型: Jackpot[%s], 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.JackpotType.String(), award.WinMoney)
			case slot.JackpotGrand:
				logic.GameLog.AddGameStep("贏得 Jackpot[%s], 金額: %d, 提前結束Lightning", award.JackpotType.String(), award.WinMoney)
				mlog.Debug("贏得 Jackpot[%s], 金額: %d, 提前結束Lightning", award.JackpotType.String(), award.WinMoney)
			}
		case slot.PayTypeAccuMulate:
			logic.GameLog.AddGameStep("位置: %d_%d, 符號: %s, 獎勵類型: 加總獎金, 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.WinMoney)
			mlog.Debug("位置: %d_%d, 符號: %s, 獎勵類型: 加總獎金, 金額: %d", award.Position.Reel, award.Position.Row, award.Symbol.GetSymbol(), award.WinMoney)
		}
	}
}

// LightningGame Lightning 模式遊戲
type LightningGame struct {
	Logic              *Logic                 `json:"-"`
	Steps              []*LightningStep       `json:"steps"` // 每次轉出的結果
	Origin             *protodata.RespinTable `json:"-"`
	OriginID           int32                  `json:"origin_id"`
	BetMoney           int64                  `json:"bet_money"`             // 下注金額
	BetRate            int64                  `json:"bet_rate"`              // 下注倍率
	ShowStep           int                    `json:"show_step"`             // 以表演完階段
	IsFreeGame         bool                   `json:"is_free_game"`          // 是否由FreeGame贏得
	SymbolGroupID      int32                  `json:"symbol_group_id"`       // 預設SymbolGroupID
	IsLimit            bool                   `json:"is_limit"`              // 是否有臨界值
	LimitSymbolGroupID int32                  `json:"limit_symbol_group_id"` // 當超過臨界時要轉的SymbolGroupID
	LimitPoint         int                    `json:"limit_point"`           // 重轉臨界點，超過換SymbolGroupID
	WinMoney           int64                  `json:"win_money"`             // 累計共贏金額
}

// NewLightningGame create LightningGame
func NewLightningGame(logic *Logic, table *protodata.RespinTable, betMoney, betRate int64, isFreeGame bool) *LightningGame {
	g := new(LightningGame)
	g.Logic = logic
	g.IsFreeGame = isFreeGame
	g.Origin = table
	g.OriginID = int32(table.GetId())
	g.SymbolGroupID = int32(table.GetRespinValue())
	g.BetMoney = betMoney
	g.BetRate = betRate

	if table.GetIsLimit() == 1 {
		g.IsLimit = true
		if table.GetLimitType() == "changeDistribution" {
			strs := strings.Split(table.GetLimitValue(), ":")
			if f, err := strconv.ParseFloat(strs[0], 64); err == nil {
				g.LimitPoint = int(f)
			}
			if f, err := strconv.ParseFloat(strs[1], 64); err == nil {
				g.LimitSymbolGroupID = int32(f)
			}
		}
	}

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
func (g *LightningGame) Spin() {

	last := g.GetLastStep()
	if last == nil {
		mlog.Error("查無前一次輪軸結果")
		return
	}

	var symbolGroupID int32
	if g.IsLimit && len(g.Steps) > g.LimitPoint {
		symbolGroupID = g.LimitSymbolGroupID
	} else {
		symbolGroupID = g.SymbolGroupID
	}

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

		if result.GetPayType() == slot.PayTypeAddTimes {
			rule = result.Rule
			origin = last.Origin
			f, _ := strconv.ParseFloat(rule.GetPayout(), 64)
			lastChance += int(f)
		}

		if result.GetPayType() == slot.PayTypeJackpot {
			lastChance = 0
			rule = nil
			jpRule = result.Rule
		}
	}

	manager.RowMode()

	if rule == nil {
		// rule = last.Rule
		origin = last.Origin
	}

	step := NewLightningStep(origin, rule, manager, lastChance)
	g.CalcStepAward(step)

	// 計算獎勵
	// 計算每個金球金額

	// 有中最大Jackpot
	if jpRule != nil {

		// 找出中獎的JackpotLevel
		f, _ := strconv.ParseFloat(jpRule.GetPayout(), 64)
		jpID := int32(f)

		var jackpot *slot.Jackpot
		for _, jp := range g.Logic.Jackpots {
			if jpID == jp.Origin.GetId() {
				jackpot = jp
				break
			}
		}

		if jackpot != nil {

			win := jackpot.GetCurrentPoolByBet(g.BetMoney*g.BetRate, int64(g.Logic.MaxBetCount)*int64(g.Logic.MaxBetRate))

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

	g.WinMoney += step.WinMoney
	g.AddStep(step)
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

// CalcStepAward 計算此Step的獎勵金額
func (g *LightningGame) CalcStepAward(step *LightningStep) {

	step.GameResultManager.RowMode()

	// 把AccuMulate挑出來特別處理
	type AccuMulateObj struct {
		Position  slot.Position
		Symbol    *protodata.SlotSymbolDistribution
		Attribute *protodata.SymbolAttributes
	}
	var accuMulates []*AccuMulateObj

	for rowID, row := range step.GameResultManager.GameResult {
		for reelID, symbol := range row {
			// 檢查此位置是否計算過獎勵
			var isCalc bool
			for _, _step := range g.Steps {
				for _, award := range _step.Awards {
					if award.Position.Reel == int32(reelID+1) && award.Position.Row == int32(rowID+1) {
						isCalc = true
						break
					}
				}
			}
			if isCalc {
				continue
			}
			for _, attr := range g.Logic.SymbolAttributes {
				if attr.GetIsGoldBall() == 1 && attr.GetSymbol() == symbol.GetSymbol() {
					pos := slot.Position{Reel: int32(reelID + 1), Row: int32(rowID + 1)}

					switch slot.PayType(attr.GetGoldballType()) {
					case slot.PayTypeChip:
						f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)
						win := g.BetMoney * int64(f)

						award := &LightningAward{
							WinMoney: win,
							Type:     slot.PayTypeChip,
							Symbol:   symbol,
							Position: pos,
						}

						step.Awards = append(step.Awards, award)
						step.WinMoney += win
					case slot.PayTypeChipXRate:
						f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)

						win := g.BetMoney * g.BetRate * int64(f)

						award := &LightningAward{
							WinMoney: win,
							Type:     slot.PayTypeChipXRate,
							Symbol:   symbol,
							Position: pos,
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

							win := jackpot.GetCurrentPoolByBet(g.BetMoney*g.BetRate, int64(g.Logic.MaxBetCount)*int64(g.Logic.MaxBetRate))

							award := &LightningAward{
								WinMoney:    win,
								Type:        slot.PayTypeJackpot,
								JackpotType: jackpot.Level,
								Symbol:      symbol,
								Position:    pos,
							}

							step.Awards = append(step.Awards, award)
							step.WinMoney += win
						}
					case slot.PayTypeAccuMulate:
						accuMulates = append(accuMulates, &AccuMulateObj{
							Position:  pos,
							Symbol:    symbol,
							Attribute: attr,
						})
					}
				}
			}
		}
	}

	for _, row := range accuMulates {
		if row.Symbol.GetSymbol() == "g7" {
			symbols := strings.Split(row.Attribute.GetGoldballValue(), ",")

			var win int64

			// 抓出已出現獎勵統計金額
			var _awards []*LightningAward
			for _, _step := range g.Steps {
				_awards = append(_awards, _step.Awards...)
			}
			// 本次Step的Award
			_awards = append(_awards, step.Awards...)

			for _, award := range _awards {
				for _, s := range symbols {
					if award.Symbol.GetSymbol() == s {
						win += award.WinMoney
						break
					}
				}
			}

			award := &LightningAward{
				WinMoney: win,
				Type:     slot.PayTypeAccuMulate,
				Symbol:   row.Symbol,
				Position: row.Position,
			}

			step.Awards = append(step.Awards, award)
			step.WinMoney += win
		}
	}

	for _, row := range accuMulates {
		if row.Symbol.GetSymbol() == "g8" {
			symbols := strings.Split(row.Attribute.GetGoldballValue(), ",")

			var win int64

			// 抓出已出現獎勵統計金額
			var _awards []*LightningAward
			for _, _step := range g.Steps {
				_awards = append(_awards, _step.Awards...)
			}
			// 本次Step的Award
			_awards = append(_awards, step.Awards...)

			for _, award := range _awards {
				for _, s := range symbols {
					if award.Symbol.GetSymbol() == s {
						win += award.WinMoney
						break
					}
				}
			}

			award := &LightningAward{
				WinMoney: win,
				Type:     slot.PayTypeAccuMulate,
				Symbol:   row.Symbol,
				Position: row.Position,
			}

			step.Awards = append(step.Awards, award)
			step.WinMoney += win
		}
	}
}
