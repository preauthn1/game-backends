package robotcomm

import (
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	ConfigSectionName_Robot = "robot"
)

//机器人配置
type RobotConfig struct {
	Enable                   bool    //是否开启机器人
	LogonInterval            int32   //机器人登录频率
	MinMoneyAmount           int64   //最小金币数
	MaxMoneyAmount           int64   //最大金币数
	QueueCheckWaitSpanMin    int32   //配桌等待时间最小值
	QueueCheckWaitSpanMax    int32   //配桌等待时间最大值
	QueueAgainWaitSpanMin    int32   //继续配桌等待时间最小值
	QueueAgainWaitSpanMax    int32   //继续配桌等待时间最大值
	TableMaxUserCount        int32   //桌子上最大人数
	TableMinUserCount        int32   //桌子上最小人数
	GameEndLeaveOdds         float32 //一局游戏结束机器人离开概率
	MaxUserGameEndLeaveOdds  float32 //桌子上人数达到上限时机器人离开概率
	GameActionWaitSpanMin    int32   //机器人游戏动作等待时间最小值
	GameActionWaitSpanMax    int32   //机器人动作等待时间最大值
	GameReadyWaitSpanMin     int32   //机器人准备等待时间最小值
	GameReadyWaitSpanMax     int32   //机器人准备等待时间最大值
	MaxCardsRndLvl           int32   //机器人换牌等级随机值
	AutoSitWaitSpanMin       int32   //机器人主动入座等待时间最小值
	AutoSitWaitSpanMax       int32   //机器人主动入座等待时间最大值
	StressTest               int32
	AgainstGameing           bool                                 //進入正在遊戲中的(觀戰) 0否 1是
	MatchTableMode           int32                                //配桌方式, 0輪詢桌子每次放一個機器人 1輪詢桌子每次放N個機率機器人
	TableMatchRobotRate      [300]int                             //index為機器人人數, value為機率%. 桌子隨機配N個機器人.
	TableMatchRobotsWiatSpan TableMatchRobotByRealUserAndWaitTime //當真人超過某數量時調整機器人配桌時間增加
}

//當真人超過某數量時調整機器人配桌時間增加
type TableMatchRobotByRealUserAndWaitTime struct {
	UserCount int32
	WaitSpan  int32
}

//解析机器人配置
func ParseRobotConfig(cfg *config.Config) *RobotConfig {
	rcfg := new(RobotConfig)

	enable := (cfg.GetConfigInt(ConfigSectionName_Robot, "enable") == 1)
	if !enable {
		return rcfg
	}

	//登录频率
	robotLogonInterval := cfg.GetConfigInt32(ConfigSectionName_Robot, "logoninterval")
	//最小金币数
	minMoneyAmount := cfg.GetConfigInt64(ConfigSectionName_Robot, "minmneyamount")
	//最大金币数
	maxMoneyAmount := cfg.GetConfigInt64(ConfigSectionName_Robot, "maxmoneyamount")

	//机器人状态
	rcfg.Enable = enable
	//登录频率
	rcfg.LogonInterval = robotLogonInterval
	//最小金币数
	rcfg.MinMoneyAmount = minMoneyAmount
	//最大金币数
	rcfg.MaxMoneyAmount = maxMoneyAmount

	//配桌
	queuecheckwaitspan := cfg.GetConfigValue(ConfigSectionName_Robot, "queuecheckwaitspan")
	queuecheckSpanArr := strings.Split(queuecheckwaitspan, ",")
	queuecheckWaitSpanMin, _ := strconv.Atoi(queuecheckSpanArr[0])
	queuecheckWaitSpanMax, _ := strconv.Atoi(queuecheckSpanArr[1])
	rcfg.QueueCheckWaitSpanMin = int32(queuecheckWaitSpanMin)
	rcfg.QueueCheckWaitSpanMax = int32(queuecheckWaitSpanMax)

	//再次配桌
	queueagainwaitspan := cfg.GetConfigValue(ConfigSectionName_Robot, "queueagainwaitspan")
	queueagainSpanArr := strings.Split(queueagainwaitspan, ",")
	queueagainWaitSpanMin, _ := strconv.Atoi(queueagainSpanArr[0])
	queueagainWaitSpanMax, _ := strconv.Atoi(queueagainSpanArr[1])
	rcfg.QueueAgainWaitSpanMin = int32(queueagainWaitSpanMin)
	rcfg.QueueAgainWaitSpanMax = int32(queueagainWaitSpanMax)

	//配桌最大人数
	rcfg.TableMaxUserCount = cfg.GetConfigInt32(ConfigSectionName_Robot, "tablemaxusercount")
	//配桌最小人数
	rcfg.TableMinUserCount = cfg.GetConfigInt32(ConfigSectionName_Robot, "tableminusercount")
	//游戏结束后离开几率
	rcfg.GameEndLeaveOdds = cfg.GetConfigFloat32(ConfigSectionName_Robot, "gameendleaveodds")
	//桌子人数达到人数上限后离开几率
	rcfg.MaxUserGameEndLeaveOdds = cfg.GetConfigFloat32(ConfigSectionName_Robot, "maxusergameendleaveodds")

	//游戏动作
	gameactionwaitspan := cfg.GetConfigValue(ConfigSectionName_Robot, "gameactionwaitspan")
	gameactionSpanArr := strings.Split(gameactionwaitspan, ",")
	gameactionWaitSpanMin, _ := strconv.Atoi(gameactionSpanArr[0])
	gameactionWaitSpanMax, _ := strconv.Atoi(gameactionSpanArr[1])

	rcfg.GameActionWaitSpanMin = int32(gameactionWaitSpanMin)
	rcfg.GameActionWaitSpanMax = int32(gameactionWaitSpanMax)

	//准备
	gamereadywaitspan := cfg.GetConfigValue(ConfigSectionName_Robot, "gamereadywaitspan")
	gamereadySpanArr := strings.Split(gamereadywaitspan, ",")
	gamereadyWaitSpanMin, _ := strconv.Atoi(gamereadySpanArr[0])
	gamereadyWaitSpanMax, _ := strconv.Atoi(gamereadySpanArr[1])

	rcfg.GameReadyWaitSpanMin = int32(gamereadyWaitSpanMin)
	rcfg.GameReadyWaitSpanMax = int32(gamereadyWaitSpanMax)

	//主动入座
	autositywaitspan := cfg.GetConfigValue(ConfigSectionName_Robot, "autositwaitspan")
	autositySpanArr := strings.Split(autositywaitspan, ",")
	autosityWaitSpanMin, _ := strconv.Atoi(autositySpanArr[0])
	autosityWaitSpanMax, _ := strconv.Atoi(autositySpanArr[1])

	rcfg.AutoSitWaitSpanMin = int32(autosityWaitSpanMin)
	rcfg.AutoSitWaitSpanMax = int32(autosityWaitSpanMax)

	//换牌概率
	rcfg.MaxCardsRndLvl = cfg.GetConfigInt32(ConfigSectionName_Robot, "maxcardsrndlvl")
	rcfg.StressTest = cfg.GetConfigInt32(ConfigSectionName_Robot, "stresstest")
	rcfg.AgainstGameing = (cfg.GetConfigInt32(ConfigSectionName_Robot, "againstgameing") == 0)
	rcfg.MatchTableMode = cfg.GetConfigInt32(ConfigSectionName_Robot, "matchtablemode")

	tableMatchRobotRate := cfg.GetConfigValue(ConfigSectionName_Robot, "tablematchrobotrate")
	if tableMatchRobotRate != "" {
		matchRobotArray := strings.Split(tableMatchRobotRate, "|")
		var sumRate int
		for _, v := range matchRobotArray {
			robotRateArr := strings.Split(v, ",")
			index, _ := strconv.Atoi(robotRateArr[0])
			rate, _ := strconv.Atoi(robotRateArr[1])
			sumRate += int(rate)
			rcfg.TableMatchRobotRate[index] = sumRate
		}
	}

	tableMatchRobotsWiatSpan := cfg.GetConfigValue(ConfigSectionName_Robot, "tablematchrobotswiatspan")
	if tableMatchRobotsWiatSpan != "" {
		matchRobotArray := strings.Split(tableMatchRobotsWiatSpan, ",")
		userCount, _ := strconv.Atoi(matchRobotArray[0])
		spanTime, _ := strconv.Atoi(matchRobotArray[1])
		rcfg.TableMatchRobotsWiatSpan.UserCount = int32(userCount)
		rcfg.TableMatchRobotsWiatSpan.WaitSpan = int32(spanTime)
	}

	return rcfg
}

//生成一个随机的游戏动作等待时间
func (rcfg *RobotConfig) GameActionRndWaitSpan() int32 {
	return util.NewIntScope(rcfg.GameActionWaitSpanMin, rcfg.GameActionWaitSpanMax+1).GetRandomValue()
}

//生成一个随机的游戏准备等待时间
func (rcfg *RobotConfig) GameReadyRndWaitSpan() int32 {
	return util.NewIntScope(rcfg.GameReadyWaitSpanMin, rcfg.GameReadyWaitSpanMax+1).GetRandomValue()
}

//生成一个再次配桌等待时间
func (rcfg *RobotConfig) QueueAgainWaitSpan() int32 {
	return util.NewIntScope(rcfg.QueueAgainWaitSpanMin, rcfg.QueueAgainWaitSpanMax+1).GetRandomValue()
}

//當真人超過某數量時調整機器人配桌時間增加
func (rcfg *RobotConfig) QueueAgainRealUserWaitSpan() int32 {
	return util.NewIntScope(rcfg.QueueAgainWaitSpanMin+rcfg.TableMatchRobotsWiatSpan.WaitSpan,
		rcfg.QueueAgainWaitSpanMax+rcfg.TableMatchRobotsWiatSpan.WaitSpan).GetRandomValue()
}
