package lhd

import (
	"fmt"
	"math"
	"math/rand"
	"strconv"
	"strings"
	"time"

	proto "example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	RobotLogicConfigFileName = "robot"
)

// 机器人个性
type RobotCha struct {
	minbetcount int32
	maxbetcount int32
	bettype     []int32
	line        int32
}

// 创建机器个性对象
func NewRobotCha(cf *config.Config, cname string) *RobotCha {
	rcha := new(RobotCha)
	s1 := cf.GetConfigValue(cname, "betcount")
	strarr := strings.Split(s1, ",")
	min, _ := strconv.ParseInt(strarr[0], 10, 32)
	rcha.minbetcount = int32(min)
	max, _ := strconv.ParseInt(strarr[1], 10, 32)
	rcha.maxbetcount = int32(max)
	s1 = cf.GetConfigValue(cname, "bettype")
	rcha.bettype = rcha.getArr(s1)
	rand.Seed(time.Now().UnixNano())

	rcha.line = cf.GetConfigInt32(cname, "line")

	return rcha
}

func (rc *RobotCha) getArr(str string) []int32 {
	arr := make([]int32, 0)
	strarr := strings.Split(str, ",")
	for i := 0; i < len(strarr); i++ {
		m, _ := strconv.ParseInt(strarr[i], 10, 32)
		arr = append(arr, int32(m))
	}

	return arr
}

// cha为正时表示龙多，为负时表示虎多
func (rc *RobotCha) getBetType(cha int32) int32 {
	noBetType := int32(0)
	if int32(math.Abs(float64(cha))) > rc.line {
		if cha > 0 {
			noBetType = LONG
		} else {
			noBetType = HU
		}
	}
	betTypeSeeds := make([]int32, 0)
	if noBetType != 0 {
		for _, v := range rc.bettype {
			if v != noBetType {
				betTypeSeeds = append(betTypeSeeds, v)
			} else {
				betTypeSeeds = append(betTypeSeeds, 0)
			}
		}
	} else {
		betTypeSeeds = rc.bettype
	}
	mlog.Debug("current cha:%d, noBetType:%d, betTypeSeeds:%v", cha, noBetType, betTypeSeeds)
	index := len(betTypeSeeds)
	if index > 0 {
		n := rand.Int31n(int32(index))
		return betTypeSeeds[n]
	}
	return 0
}

func (rc *RobotCha) getBetCount() int32 {
	i := rc.maxbetcount - rc.minbetcount + 1
	n := rand.Int31n(i) + rc.minbetcount
	return n * 100
}

// 机器人逻辑
type LHDRobotLogic struct {
	*robotlogic.LogicBase
	robotChas []*RobotCha
	gameChas  map[int32]*RobotCha
}

// 新建一个机器人逻辑实例
func NewLHDRobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.Group, glc gamelogic.GameLogic, evtChannel *event.Dispatcher) *LHDRobotLogic {
	rblgc := new(LHDRobotLogic)
	rblgc.LogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)
	rblgc.parseRobotCha(configs)
	rblgc.gameChas = make(map[int32]*RobotCha)

	return rblgc
}

// 解析机器人个性
func (lgc *LHDRobotLogic) parseRobotCha(cfgs *config.Group) {
	rand.Seed(time.Now().UnixNano())
	robotCfg := cfgs.GetConfig(RobotLogicConfigFileName)

	cn := robotCfg.GetConfigInt("csize", "cn")

	for i := 1; i <= cn; i++ {
		cname := fmt.Sprintf("c%d", i)
		cha := NewRobotCha(robotCfg, cname)
		lgc.robotChas = append(lgc.robotChas, cha)
	}
}

// 获取机器人个性
func (lgc *LHDRobotLogic) getRobotCha(userid int32) *RobotCha {
	var cha *RobotCha
	if cha, ok := lgc.gameChas[userid]; ok {
		return cha
	}

	clen := len(lgc.robotChas)
	cn := util.RndInt(0, clen)
	cha = lgc.robotChas[cn]

	lgc.gameChas[userid] = cha

	return cha
}

func (lgc *LHDRobotLogic) getGameLogic() *LongHudouLogic {
	return lgc.GameLogic.(*LongHudouLogic)
}

// 游戏消息
func (lgc *LHDRobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {
	glc := lgc.getGameLogic()
	if glc.gamestate != Bet || glc.zhuangID == robot.UserInfo.UserID {
		return
	}

	if m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDStateID) ||
		m.SClassID == int32(netproto.LHD_GameMessageClassID_LHDBetRetID) {
		//action_wait := lgc.RobotConfig.GameActionRndWaitSpan()
		action_wait := int32(0) // 立刻動作
		if action_wait < glc.getBetTime() {
			lgc.DelayRun(action_wait, func() {
				lgc.robotBet(robot.UserInfo)
			})
		}

		return
	}
}

// 机器人下注
func (lgc *LHDRobotLogic) robotBet(ur *gamelogic.GameUser) {
	glc := lgc.getGameLogic()
	l := glc.getRobotBetMoneyByType(LONG)
	h := glc.getRobotBetMoneyByType(HU)
	cha := int32((l - h) / 100)
	//玩家当庄的时候不平衡
	if glc.zhuangID != 0 && !glc.GetUserByUserID(glc.zhuangID).IsRobot() {
		cha = 0
	}
	rcha := lgc.getRobotCha(ur.UserID)
	if glc.he > 0 && glc.zhuangID != 0 && !glc.GetUserByUserID(glc.zhuangID).IsRobot() {
		pre := rand.Int31n(100)
		if pre < glc.he {
			lgc.SendGameMessage(ur, lgc.getBetMsg(2, rcha.getBetCount()))
			return
		}
	}
	btype := rcha.getBetType(cha)
	if btype == 0 { //沒有下注選項
		lgc.DelayRun(1, func() { // 延遲一秒再次下注
			lgc.robotBet(ur)
		})
		return
	}
	bcount := rcha.getBetCount()

	lgc.SendGameMessage(ur, lgc.getBetMsg(btype, bcount))
}

func (lgc *LHDRobotLogic) OnGameEnd(robot *robotcomm.Robot) {
	lgc.gameChas = make(map[int32]*RobotCha)
}

// 获取下注消息
func (lgc *LHDRobotLogic) getBetMsg(bettype int32, betcount int32) *msg.Message {
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.LHD_GameMessageClassID_LHDBetID)
	btdata := new(netproto.LHD_BetArr)
	betdata := new(netproto.LHD_Bet)
	betdata.BetType = proto.Int32(bettype)
	betdata.Count = proto.Int32(betcount)
	btdata.BetArr = append(btdata.BetArr, betdata)
	btmsg.MsgData, _ = proto.Marshal(btdata)
	return btmsg
}
