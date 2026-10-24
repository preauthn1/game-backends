package lhjlogic

import (
	"config"
	"strconv"
	"strings"
	"util"
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
	scfgs := cfgs.GetConfig("gameserver")
	betcountstr := scfgs.GetConfigValue("gameserver", "betcount")
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

func (lh *LhjLogic) ReloadLogicConfig() {
	cgf := lh.cfgs.GetConfig("logicconfig")
	slotpatternstr := cgf.GetConfigValue("logicsetting", "slotpattern")
	slotpatternarr := strings.Split(slotpatternstr, ",")
	rndPpattern := util.RndInt(0, len(slotpatternarr))
	r, _ := strconv.Atoi(slotpatternarr[rndPpattern])
	lh.slotpattern = int32(r)

	lh.endactiontime = int32(cgf.GetConfigInt("logicsetting", "endactiontime"))
	lh.betrate = int32(cgf.GetConfigInt("logicsetting", "betrate"))
	lh.jprate = int32(cgf.GetConfigInt("logicsetting", "jprate"))

	bigwinrulestr := cgf.GetConfigValue("logicsetting", "bigwinrule")
	bigwinrulearr := strings.Split(bigwinrulestr, ",")
	for _, v := range bigwinrulearr {
		n, _ := strconv.Atoi(v)
		lh.bigwinrule = append(lh.bigwinrule, int32(n))
	}

	lh.bigwinnotify = int32(cgf.GetConfigInt("logicsetting", "bigwinnotify"))
}
