package baselogic

import (
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// GetExtraRuleByConditoin 依照條件取得額外條件
func (logic *SlotLogicBase) GetExtraRuleByConditoin(condi slot.ExtraRuleCondition, acts ...slot.ExtraRuleAction) []*protodata.ExtraRule {

	var rules []*protodata.ExtraRule

	for _, rule := range logic.ExtraRule {
		switch slot.UseType(rule.GetUseType()) {
		default:
			continue
		case slot.UseTypeALL:

		case slot.UseTypeMainGame:
			if logic.IsFreeSpin() {
				continue
			}
		case slot.UseTypeFreeGame:
			if !logic.IsFreeSpin() {
				continue
			}
		case slot.UseTypeRespin:

		}

		if slot.ExtraRuleCondition(rule.GetCondition()) != condi {
			continue
		}

		if len(acts) == 0 {
			rules = append(rules, rule)
			continue
		}

		for _, act := range acts {
			if slot.ExtraRuleAction(rule.GetAction()) == act {
				rules = append(rules, rule)
			}
		}
	}

	return rules
}
