package jjbx

import (
	"fmt"
	"strconv"

	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// SelectOption 選項
type SelectOption struct {
	Rule      *protodata.SelectionTable `json:"-"`
	RuleID    int32
	GameMode  slot.GameMode
	GameTable interface{} `json:"-"`
	GameID    int32
}

func (opt *SelectOption) String() string {
	switch opt.GameMode {
	case slot.GameModeFreeSpin:
		tb := opt.GameTable.(*protodata.FreeSpinTable)
		return fmt.Sprintf("免費遊戲(%d) - SymbolGroupID: %d, 次數: %d", tb.GetId(), tb.GetFreeSymbolGroupId(), tb.GetFreeSpinCount())
	case slot.GameModeLightning:
		tb := opt.GameTable.(*protodata.RespinTable)
		return fmt.Sprintf("Lightning遊戲(%d) - SymbolGroupID: %d, 次數: %d", tb.GetId(), tb.GetRespinValue(), tb.GetRespinCount())
	}
	return ""
}

// SelectChance 玩家可選擇遊戲模式
type SelectChance struct {
	Logic             *Logic                      `json:"-"`
	Rule              *protodata.SlotPayTable     `json:"-"`
	Selections        []*protodata.SelectionTable `json:"-"`
	GroupID           int32
	BetMoney          int64           `json:"BetMoney"`
	BetRate           int64           `json:"BetRate"`
	Options           []*SelectOption `json:"-"`
	GameResultManager *slot.GameResultManager
	Data              map[string]interface{}
}

// NewSelectChance create SelectChance
func NewSelectChance(logic *Logic, result *slot.GameResultManager, r *protodata.SlotPayTable, betMoney, betRate int64) (*SelectChance, error) {
	s := new(SelectChance)

	s.Logic = logic
	s.GameResultManager = result
	s.Rule = r
	s.Data = make(map[string]interface{})

	f, err := strconv.ParseFloat(s.Rule.GetPayout(), 64)
	if err != nil {
		return nil, err
	}

	s.GroupID = int32(f)

	for _, selection := range logic.SelectionTable {
		if selection.GetGroupId() != s.GroupID {
			continue
		}
		if selection.GetIsGoldsymbol() == 1 {
			switch selection.GetGoldCondition() {
			case slot.ConditionRate:
				f, _ := strconv.ParseFloat(selection.GetConditionValue(), 64)
				if betRate != int64(f) {
					continue
				}
			}
		}
		s.Selections = append(s.Selections, selection)

		switch slot.GameMode(selection.GetSelectionType()) {
		case slot.GameModeFreeSpin:
			for _, row := range logic.FreeSpinTable {
				if row.GetGroupId() != int32(selection.GetSelectionValue()) {
					continue
				}
				opt := new(SelectOption)
				opt.Rule = selection
				opt.RuleID = int32(selection.GetId())
				opt.GameMode = slot.GameModeFreeSpin
				opt.GameTable = row
				opt.GameID = int32(row.GetId())
				s.Options = append(s.Options, opt)
			}
		case slot.GameModeLightning:
			for _, row := range logic.RespinTable {
				if row.GetGroupId() != int32(selection.GetSelectionValue()) {
					continue
				}
				opt := new(SelectOption)
				opt.Rule = selection
				opt.RuleID = int32(selection.GetId())
				opt.GameMode = slot.GameModeLightning
				opt.GameTable = row
				opt.GameID = int32(row.GetId())
				s.Options = append(s.Options, opt)
			}
		}
	}

	s.BetMoney = betMoney
	s.BetRate = betRate

	return s, nil
}
