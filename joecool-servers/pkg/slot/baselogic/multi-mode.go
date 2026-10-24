package baselogic

import (
	"sort"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/prob"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// CalcMultiMode 計算加倍數
func (logic *SlotLogicBase) CalcMultiMode(mode slot.MultiMode, str string) (multi int64) {

	multi = 1

	// 先檢查盤面是否出現百搭符號，決定加倍數量
	type MultiOpt struct {
		Value  int64
		Weight int
	}
	var opts []MultiOpt

	switch mode {
	default:
	case slot.MultiModeDiceOnce:

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

			var multis []int
			for _, opt := range opts {
				multis = append(multis, int(opt.Value))
			}
			sort.Slice(multis, func(i, j int) bool {
				return multis[i] < multis[j]
			})

			r := new(prob.Random)
			for _, opt := range opts {
				r.Add(opt, opt.Weight)
			}
			t := r.Choose().(MultiOpt)

			logic.GameLog.AddGameStep("免費遊戲百搭加倍選項 %v 倍, 骰出 %v 倍", multis, t.Value)

			multi = t.Value
		}

	case slot.MultiModeDiceEachReelSum:

	case slot.MultiModeDiceEachReelMulti:

		// 決定倍率選項
		{
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
	}

	return
}

// CalcMultiLevel 計算階段型加倍
// @Param str pattern
// @Param winTimes 第n次贏
func (logic *SlotLogicBase) CalcMultiLevel(str string, winTimes int) (multi int64) {

	if winTimes <= 0 {
		return 1
	}

	strs := strings.Split(str, ",")

	var multis []int64

	for _, s := range strs {
		val, err := strconv.ParseFloat(s, 32)
		if err != nil {
			return 1
		}
		multis = append(multis, int64(val))
	}

	if winTimes > len(multis) {
		winTimes = len(multis)
	}

	return multis[winTimes-1]
}
