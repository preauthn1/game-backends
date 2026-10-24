package qz21gamelogic

import (
	"config"
	"event"
	"fmt"
	"gamelogic"
	"math/rand"
	//	"time"
	//	"math/rand"
	"mlog"
	"msg"
	"netproto"
	"poker"
	"robot/robotcomm"
	"robot/robotlogic"
	"strconv"
	"strings"
	//	"sync"
	"util"
)

import proto "example.invalid/golang/protobuf/proto"

const (
	RobotLogicConfigFileName = "robot"
)

//机器人个性
type RobotCha struct {
	qiangzhuang []int32
	betodd      []int32
	dian12_18   int32 //12-18点叫牌概率
	dian19_20   int32 //19-20点叫牌概率
}

//创建机器个性对象
func NewRobotCha(cf *config.Config, cname string) *RobotCha {
	rcha := new(RobotCha)
	s1 := cf.GetConfigValue(cname, "qiangzhuang")
	rcha.qiangzhuang = rcha.getValue(s1)

	s1 = cf.GetConfigValue(cname, "bet")
	rcha.betodd = rcha.getValue(s1)
	mlog.Debug("betodd=%v", rcha.betodd)

	s1 = cf.GetConfigValue(cname, "dian12_18")
	v, _ := strconv.ParseInt(s1, 10, 32)
	rcha.dian12_18 = int32(v)

	s1 = cf.GetConfigValue(cname, "dian19_20")
	v1, _ := strconv.ParseInt(s1, 10, 32)
	rcha.dian19_20 = int32(v1)

	return rcha
}

func (rc *RobotCha) getValue(str string) []int32 {
	strarr := strings.Split(str, ",")
	arr := make([]int32, 0)
	for i := 0; i < len(strarr); i++ {
		n, _ := strconv.ParseInt(strarr[i], 10, 32)
		arr = append(arr, int32(n))
	}

	return arr
}

//闲家叫牌
func (rc *RobotCha) isJiaoPai(dian int32) bool {
	jiao := int32(100)
	pre := int32(0)
	if dian >= 12 && dian <= 18 {
		pre = rc.dian12_18
	} else if dian >= 19 && dian <= 20 {
		pre = rc.dian19_20
	}

	if pre > 0 {
		n := rand.Int31n(jiao)
		return (n < pre)
	}

	return false
}

func (rc *RobotCha) getQiangZhuangOdd() int32 {
	cn := len(rc.qiangzhuang)
	n := rand.Int31n(int32(cn))

	return rc.qiangzhuang[n]
}

func (rc *RobotCha) getBetOdd() int32 {
	cn := len(rc.betodd)
	n := rand.Int31n(int32(cn))
	return rc.betodd[n]
}

//机器人逻辑
type QZ21RobotLogic struct {
	*robotlogic.RobotLogicBase
	robotChas []*RobotCha
	gameChas  map[int32]*RobotCha
	//lock      sync.RWMutex
}

func NewQZ21RobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *QZ21RobotLogic {
	rblgc := new(QZ21RobotLogic)
	rblgc.RobotLogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)
	rblgc.parseRobotCha(configs)
	rblgc.gameChas = make(map[int32]*RobotCha)

	return rblgc
}

//解析机器人个性
func (lgc *QZ21RobotLogic) parseRobotCha(cfgs *config.ConfigGroup) {
	robotCfg := cfgs.GetConfig(RobotLogicConfigFileName)

	cn := robotCfg.GetConfigInt("csize", "cn")

	for i := 1; i <= cn; i++ {
		cname := fmt.Sprintf("c%d", i)

		cha := NewRobotCha(robotCfg, cname)
		lgc.robotChas = append(lgc.robotChas, cha)
	}
}

//获取机器人个性
func (lgc *QZ21RobotLogic) getRobotCha(userid int32) *RobotCha {
	var cha *RobotCha
	//lgc.lock.Lock()
	if cha, ok := lgc.gameChas[userid]; ok {
		//lgc.lock.Unlock()
		return cha
	}

	clen := len(lgc.robotChas)
	cn := util.RndInt(0, clen)
	cha = lgc.robotChas[cn]

	lgc.gameChas[userid] = cha
	//lgc.lock.Unlock()
	return cha
}

func (lgc *QZ21RobotLogic) getGameLogic() *Qz21Table {
	return lgc.GameLogic.(*Qz21Table)
}

func (lgc *QZ21RobotLogic) getPoker() *poker.FixedPoker {
	return lgc.getGameLogic().poker.(*poker.FixedPoker)
}

//游戏消息
func (lgc *QZ21RobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {
	//底注阶段,收到底注倍数通知时
	glc := lgc.getGameLogic()
	if glc.step == QZ21StepQiangZhuang &&
		m.SClassID == int32(netproto.QZ21_GameMessageClassID_QZ21NotifyTimeOutOptId) {
		rcha := lgc.getRobotCha(robot.UserInfo.UserID)
		lgc.SendGameMessage(robot.UserInfo, lgc.getQiangZhuangMsg(rcha.getQiangZhuangOdd()))
		return
	}

	if glc.step == QZ21StepXJBet &&
		m.SClassID == int32(netproto.QZ21_GameMessageClassID_QZ21NotifyTimeOutOptId) &&
		robot.UserInfo.SeatNo != glc.bankerSeat {
		rcha := lgc.getRobotCha(robot.UserInfo.UserID)
		lgc.SendGameMessage(robot.UserInfo, lgc.getBetMsg(rcha.getBetOdd()))
		return
	}

	if glc.step == QZ21StepXJComplete &&
		m.SClassID == int32(netproto.QZ21_GameMessageClassID_QZ21NotifyTimeOutOptId) &&
		glc.bankerSeat != robot.UserInfo.SeatNo {
		lgc.robotOpt(robot, glc)

		return
	}
	//收到回应以后再进行下次叫牌
	if glc.step == QZ21StepXJComplete &&
		m.SClassID == int32(netproto.QZ21_GameMessageClassID_QZ21NotifyRoleStatusId) {
		ust := m.MsgData.(*netproto.QZ21NotifyRoleStatus)
		if ust.GetUserId() == robot.UserInfo.UserID && ust.GetRoe() == 1 {
			lgc.robotOpt(robot, glc)
		}
	}
}

func (lgc *QZ21RobotLogic) robotOpt(robot *robotcomm.Robot, glc *Qz21Table) {
	rcha := lgc.getRobotCha(robot.UserInfo.UserID)

	cards := poker.NewCardGroup(glc.gamePoker.GetUserPoker(robot.UserInfo.UserID).Cards.Cards)
	lastdian := glc.getDian(cards)
	if lastdian >= 21 {
		return
	}

	fpoker := glc.poker.(*poker.FixedPoker)
	cg := fpoker.GetUserCards(robot.UserInfo.UserID)
	cards.AddCard(cg.Cards[0])
	dian := glc.getDian(cards)
	if dian > 21 {
		ot := int32(2)
		if rcha.isJiaoPai(lastdian) {
			mlog.Debug("闲家叫牌")
			ot = 1
		}

		lgc.SendGameMessage(robot.UserInfo, lgc.getJiaoPaiMsg(ot))
	} else {
		lgc.SendGameMessage(robot.UserInfo, lgc.getJiaoPaiMsg(1))
	}
}

func (lgc *QZ21RobotLogic) OnGameEnd(robot *robotcomm.Robot) {
	lgc.gameChas = make(map[int32]*RobotCha)
}

func (lgc *QZ21RobotLogic) getQiangZhuangMsg(odd int32) *msg.Message {
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21ChoseOddsId)

	btdata := new(netproto.ChoseOdds)
	btdata.Ot = proto.Int32(2)
	btdata.Odds = proto.Int32(odd)
	btmsg.MsgData, _ = proto.Marshal(btdata)
	return btmsg
}

func (lgc *QZ21RobotLogic) getBetMsg(odd int32) *msg.Message {
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21ChoseOddsId)
	btdata := new(netproto.ChoseOdds)
	btdata.Ot = proto.Int32(1)
	btdata.Odds = proto.Int32(odd)
	btmsg.MsgData, _ = proto.Marshal(btdata)
	return btmsg
}

func (lgc *QZ21RobotLogic) getJiaoPaiMsg(ot int32) *msg.Message {
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21RoleOptId)

	btdata := new(netproto.RoleOpt)
	btdata.Ot = proto.Int32(ot)
	btmsg.MsgData, _ = proto.Marshal(btdata)
	return btmsg
}
