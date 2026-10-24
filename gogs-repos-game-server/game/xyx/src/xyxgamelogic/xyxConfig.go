package xyxgamelogic

import (
	"config"
	"mlog"
	"strconv"
	"strings"
)

const (
	ConfigSectionName_GameLogic = "gamelogic"
)

//配置信息
type xyxConfig struct {
	baseScoreTimeout int32   //底分超时时间
	baseScoreTimes   []int32 //庄家倍数
}

var xyxconfig *xyxConfig

//解析牛牛配置
func parseXYXConfig(cfg *config.Config) {
	xyxconfig = new(xyxConfig)
	xyxconfig.baseScoreTimeout = int32(cfg.GetConfigInt(ConfigSectionName_GameLogic, "timeout_basescore"))

	baseScoreTimes := cfg.GetConfigValue(ConfigSectionName_GameLogic, "baseScoreTimes")
	baseScoreArr := strings.Split(baseScoreTimes, ",")
	for _, szbt := range baseScoreArr {
		zbt, err := strconv.Atoi(szbt)

		if err != nil {
			mlog.Error("解析游戏逻辑配置底注倍数发生错误,%v", err)
			return
		}

		xyxconfig.baseScoreTimes = append(xyxconfig.baseScoreTimes, int32(zbt))
	}

}

//获取牛牛配置
func getXYXConfig() *xyxConfig {
	xyxc := new(xyxConfig)
	xyxc.baseScoreTimeout = 10
	xyxc.baseScoreTimes = []int32{1, 2, 3, 4}
	return xyxc
}

//最小底注
func (xyxc *xyxConfig) minBaseScoreTimes() int32 {
	return xyxc.baseScoreTimes[0]
}

//最大底注
func (xyxc *xyxConfig) maxBaseScoreTimes() int32 {
	return xyxc.baseScoreTimes[len(xyxc.baseScoreTimes)-1]
}
