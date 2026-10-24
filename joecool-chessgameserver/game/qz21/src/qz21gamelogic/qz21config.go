package qz21gamelogic

import (
	"config"
	//	"mlog"
	"strconv"
	"strings"
)

var animotionTimeOut int32 = 10

type qz21Config struct {
	qzOdds    []int32
	xjBetOdds []int32
}

func (qc *qz21Config) getXJOddsByIndex(index int32) int32 {
	return qc.xjBetOdds[index%int32(len(qc.xjBetOdds))]
}

func (qc *qz21Config) getZJOddsByIndex(index int32) int32 {
	return qc.qzOdds[index%int32(len(qc.qzOdds))]
}

func PraseStringArryToIntArry(strs []string) []int32 {
	l := len(strs)
	ret := make([]int32, l)
	for i := 0; i < l; i++ {
		r, _ := strconv.Atoi(strs[i])
		ret[i] = int32(r)
	}
	return ret
}

func NewConfig(cfg *config.Config) *qz21Config {
	if cfg == nil {
		return nil
	}
	qzstr := cfg.GetConfigValue("Odds", "qzodds")
	xjb := cfg.GetConfigValue("Odds", "xjb")
	qzconfig := new(qz21Config)
	arr := strings.Split(qzstr, ",")
	if arr != nil {
		qzconfig.qzOdds = PraseStringArryToIntArry(arr)
	}
	axj := strings.Split(xjb, ",")
	if axj != nil {
		qzconfig.xjBetOdds = PraseStringArryToIntArry(axj)
	}
	if arr == nil || axj == nil {
		return nil
	}
	return qzconfig
}

/*
const (
	QZ21StepNull        QZ21GameStep = iota //无状态
	QZ21StepBegin                           //游戏开始发送抢庄倍率等
	QZ21StepQiangZhuang                     //玩家选择抢庄倍率阶段
	QZ21StepQzAnimotion                     //播放抢庄动画阶段
	QZ21StepXJBet                           //闲家选择倍率阶段
	QZ21StepXJComplete                      //闲家叫牌阶段
	QZ21StepZJComplete                      //庄家叫牌阶段
	QZ21StepShow                            //比牌阶段
	QZ21StepSettlement                      //结算阶段
	QZ21StepEnd
	QZ21Max
)
*/
//阶段超时配置
var stepTimeOut []int32 //{0, 0, 20, 1, 30, 30, 1, 3, 5, 0}

func (d *Qz21Table) initStepTimeOut(cfg *config.Config) {
	arr := strings.Split(cfg.GetConfigValue("Odds", "steptimeout"), ",")
	l := len(arr)
	for i := 0; i < l; i++ {
		v, _ := strconv.Atoi(arr[i])
		stepTimeOut = append(stepTimeOut, int32(v))
	}
	//stepTimeOut[0] = d.timeOut
}

func (d *Qz21Table) getStepTimeOut() int32 {
	return stepTimeOut[d.step%QZ21Max]
}
