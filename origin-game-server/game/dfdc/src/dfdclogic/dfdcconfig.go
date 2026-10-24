package dfdclogic

import (
	"config"
	"strconv"
	"strings"
)

type betconfig struct {
	alltype  []int32
	betcount []int32
}

//新建一个下注配置实例
func newBetconfig(cfgs *config.ConfigGroup) *betconfig {
	cf := new(betconfig)
	cf.alltype = make([]int32, 0)
	cf.getBetConfig(cfgs)

	return cf
}

//获取配置文件
func (cf *betconfig) getBetConfig(cfgs *config.ConfigGroup) {
	scfgs := cfgs.GetConfig("slotconfig")
	betcountstr := scfgs.GetConfigValue("slotsetting", "betcount")
	lastindex := 0
	for index, char := range betcountstr {
		if char == ',' {
			substr := betcountstr[lastindex:index]
			lastindex = index + 1
			count, _ := strconv.ParseInt(substr, 10, 32)
			cf.betcount = append(cf.betcount, int32(count))
		}
	}

}

func (lh *DFDCLogic) ReloadLogicConfig() {
	cgf := lh.cfgs.GetConfig("slotconfig")

	bigwinrulestr := cgf.GetConfigValue("slotsetting", "bigwinrule")
	bigwinrulearr := strings.Split(bigwinrulestr, ",")
	for _, v := range bigwinrulearr {
		n, _ := strconv.Atoi(v)
		lh.bigwinrule = append(lh.bigwinrule, int32(n))
	}

	lh.bigwinnotify = int32(cgf.GetConfigInt("slotsetting", "bigwinnotify"))

	goldenSymbolRatestr := cgf.GetConfigValue("slotsetting", "betrate")

	strarr := strings.Split(goldenSymbolRatestr, ",")
	for i := 0; i < len(strarr); i++ {
		count, _ := strconv.ParseInt(strarr[i], 10, 32)
		lh.goldenSymbolRate = append(lh.goldenSymbolRate, int32(count))
	}

}
