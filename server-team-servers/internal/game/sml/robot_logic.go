package sml

import (
	"fmt"
	"math"
	"math/rand"
	"sort"
	"strconv"
	"strings"

	"example.invalid/golang/protobuf/proto"

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

var robots = []Robot{
	new(RobotLv1),
	new(RobotLv2),
}

// RobotLogic 機器人邏輯
type RobotLogic struct {
	*robotlogic.LogicBase

	robots   []*RobotCha
	gameChas map[int32]*RobotCha
}

func NewRobotLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, rbtConf *robotcomm.RobotConfig, confs *config.Group, glc gamelogic.GameLogic, eChannel *event.Dispatcher) *RobotLogic {

	rbtLogic := new(RobotLogic)
	rbtLogic.LogicBase = robotlogic.NewRobotLogicBase(t, tfManager, rbtConf, confs, glc, eChannel)

	// 註冊機器人個性
	rbtLogic.initRobotCha(confs)

	rbtLogic.gameChas = make(map[int32]*RobotCha)

	return rbtLogic
}

func (rbtLogic *RobotLogic) initRobotCha(confs *config.Group) {

	robotConf := confs.GetConfig("robot")

	cn := robotConf.GetConfigInt("csize", "cn")

	for i := 1; i <= cn; i++ {
		cname := fmt.Sprintf("c%d", i)

		var (
			minBetCount int64
			maxBetCount int64
			line        int64
			betTypes    []BetType
		)

		{ // 解析下注範圍
			s := robotConf.GetConfigValue(cname, "betcount")
			arr := strings.Split(s, ",")

			if len(arr) != 2 {
				mlog.Error("機器人個性 %s 設定錯誤, 下注範圍錯誤, betcount: %s", cname, s)
				continue
			}

			{ // 最小下注
				v, err := strconv.ParseInt(arr[0], 0, 64)
				if err != nil {
					mlog.Error("機器人個性 %s 設定錯誤, 最小下注範圍錯誤, %v", cname, err)
					continue
				}
				minBetCount = v
			}

			{ // 最大下注
				v, err := strconv.ParseInt(arr[1], 0, 64)
				if err != nil {
					mlog.Error("機器人個性 %s 設定錯誤, 最大下注範圍錯誤, %v", cname, err)
					continue
				}
				maxBetCount = v
			}
		}
		{ // 解析差額水線
			line = robotConf.GetConfigInt64(cname, "line")
		}
		{ // 解析下注範圍
			s := robotConf.GetConfigValue(cname, "bettype")
			arr := strings.Split(s, ",")

			if len(arr) == 0 {
				mlog.Error("機器人個性 %s 設定錯誤, 下注類型錯誤, bettype: %s", cname, s)
				continue
			}

			for _, v := range arr {
				v = strings.TrimSpace(v)
				n, err := strconv.ParseInt(v, 0, 64)
				if err != nil {
					mlog.Error("機器人個性 %s 下注類型設定錯誤, 跳過此類型, %v", cname, err)
					continue
				}

				t := BetType(n)

				if !t.AllowBet() {
					mlog.Error("機器人個性 %s 下注類型設定錯誤, 跳過此類型, 不存在於合法下注類型 %s", cname, v)
					continue
				}

				betTypes = append(betTypes, t)
			}
		}

		cha := NewRobotCha(minBetCount, maxBetCount, line, betTypes...)
		rbtLogic.robots = append(rbtLogic.robots, cha)
	}

}

// 获取机器人个性
func (rbtLogic *RobotLogic) getRobotCha(userid int32) *RobotCha {
	var cha *RobotCha
	if cha, ok := rbtLogic.gameChas[userid]; ok {
		return cha
	}

	clen := len(rbtLogic.robots)
	cn := util.RndInt(0, clen)
	cha = rbtLogic.robots[cn]

	rbtLogic.gameChas[userid] = cha

	return cha
}

func (rbtLogic *RobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {
	logic, ok := rbtLogic.GameLogic.(*Logic)
	if !ok {
		return
	}

	if logic.Game.Step != WaitBet {
		return
	}

	time := int32(0)
	timer := logic.GameStepTimer[logic.Game.Step]
	if timer != nil {
		time = timer.WaitSeconds - timer.ElapsedSeconds()
	}
	//action_wait := rbtLogic.RobotConfig.GameActionRndWaitSpan()
	action_wait := int32(0) // 立刻動作

	if time-(action_wait+1) <= 0 {
		return
	}

	if (m.SClassID == int32(netproto.SML_GameMessageClassID_SMLStepID) && logic.Game.Step == WaitBet) ||
		m.SClassID == int32(netproto.SML_GameMessageClassID_SMLBetRetID) {
		example.invalid("機器人 %d 收到 %d 消息", robot.UserInfo.UserID, m.SClassID)
		rbt := rbtLogic.getRobotCha(robot.UserInfo.UserID)
		rbtLogic.DelayRun(action_wait, func() {
			rbt.Bet(rbtLogic, logic, robot)
		})
	}

}

// OnGameEnd 遊戲結束
func (rbtLogic *RobotLogic) OnGameEnd(robot *robotcomm.Robot) {
	rbtLogic.gameChas = make(map[int32]*RobotCha)
}

type Robot interface {
	Bet(rbtLogic *RobotLogic, logic *Logic, robot *robotcomm.Robot)
}

// Lv1 機器人 只要錢夠，隨機下注
type RobotLv1 struct{}

func (rbt *RobotLv1) Bet(rbtLogic *RobotLogic, logic *Logic, robot *robotcomm.Robot) {

	// 決定要下哪一邊
	options := []BetType{
		GoalLeft,
		GoalMiddle,
		GoalRight,
		SaveLeft,
		SaveMiddle,
		SaveRight,
	}
	u := logic.Game.Users[robot.UserInfo.UserID]
	if u == nil || (u.BetSummary[Goal] == 0 && u.BetSummary[Save] == 0) {
		if rand.Intn(2) == 0 {
			for i := 0; i < 60; i++ {
				options = append(options, Goal)
			}
		} else {
			for i := 0; i < 60; i++ {
				options = append(options, Save)
			}
		}
	} else if u.BetSummary[Goal] > 0 {
		for i := 0; i < 60; i++ {
			options = append(options, Goal)
		}
	} else if u.BetSummary[Save] > 0 {
		for i := 0; i < 60; i++ {
			options = append(options, Save)
		}
	}

	// 計算玩家可下注金額
	quota := robot.UserInfo.Score - logic.Game.BetMinCarray - u.BetSum()

	if quota <= 0 {
		return
	}

	opt := options[rand.Intn(len(options))]

	// 過濾剩下可以下注的籌碼
	var betOpts []int64
	for amt := range logic.Game.BetConfig.AllowAmounts {
		if quota >= amt {
			betOpts = append(betOpts, amt)
		}
	}

	if len(betOpts) == 0 {
		return
	}

	betOpt := betOpts[rand.Intn(len(betOpts))]

	// 計算區域可下注金額
	if logic.Game.BetSummary[opt]+betOpt >= logic.Game.BetConfig.MaxBet(opt) {
		return
	}

	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.SML_GameMessageClassID_SMLBetID)

	data := &netproto.SML_BetRequest{
		Bet: &netproto.SML_Bet{
			Type:  TransforBetType(opt).Enum(),
			Money: proto.Int64(betOpt),
		},
	}

	b, err := proto.Marshal(data)
	if err != nil {
		return
	}
	m.MsgData = b

	rbtLogic.SendGameMessage(robot.UserInfo, m)
}

// Lv2 機器人 只要錢夠，平均補足少的一邊
type RobotLv2 struct{}

func (rbt *RobotLv2) Bet(rbtLogic *RobotLogic, logic *Logic, robot *robotcomm.Robot) {

	var opt BetType

	// 找出要下哪一邊
	u := logic.Game.Users[robot.UserInfo.UserID]
	if u == nil || (u.BetSummary[Goal] == 0 && u.BetSummary[Save] == 0) {
		if logic.Game.BetSummary[Goal] > logic.Game.BetSummary[Save] {
			opt = Save
		} else {
			opt = Goal
		}
	} else if u.BetSummary[Goal] > 0 {
		opt = Goal
	} else if u.BetSummary[Save] > 0 {
		opt = Save
	}

	// 計算可下注金額 錢不夠就算了
	quota := robot.UserInfo.Score - logic.Game.BetMinCarray - u.BetSum()

	if quota <= 0 {
		return
	}

	// 過濾剩下可以下注的籌碼
	var betOpts []int64
	for amt := range logic.Game.BetConfig.AllowAmounts {
		if quota >= amt {
			betOpts = append(betOpts, amt)
		}
	}

	if len(betOpts) == 0 {
		return
	}

	betOpt := betOpts[rand.Intn(len(betOpts))]

	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.SML_GameMessageClassID_SMLBetID)

	data := &netproto.SML_BetRequest{
		Bet: &netproto.SML_Bet{
			Type:  TransforBetType(opt).Enum(),
			Money: proto.Int64(betOpt),
		},
	}

	b, err := proto.Marshal(data)
	if err != nil {
		return
	}
	m.MsgData = b

	rbtLogic.SendGameMessage(robot.UserInfo, m)
}

// 機器人個性
type RobotCha struct {
	minBetCount int64     // 最小下注金額
	maxBetCount int64     // 最大下注金額
	betTypes    []BetType // 可下注區域
	line        int64     // 雙邊下注差額，在此差額內，會平均下注
}

func NewRobotCha(minBetCount, maxBetCount, line int64, betTypes ...BetType) *RobotCha {
	return &RobotCha{
		minBetCount: minBetCount,
		maxBetCount: maxBetCount,
		betTypes:    betTypes,
		line:        line,
	}
}

// 若達到補差條件，則平均補足少的一邊
// 若未達到補差條件，則隨機下注
func (rbtc *RobotCha) GetBetType(logic *Logic) BetType {
	var betTypeContains = func(s []BetType, e BetType) bool {
		for _, a := range s {
			if a == e {
				return true
			}
		}
		return false
	}
	var noBetTypes []BetType
	// 找出要下哪一邊
	goalGroup := logic.Game.RobotBetSummary[Goal] + logic.Game.RobotBetSummary[GoalLeft] + logic.Game.RobotBetSummary[GoalMiddle] + logic.Game.RobotBetSummary[GoalRight]
	saveGroup := logic.Game.RobotBetSummary[Save] + logic.Game.RobotBetSummary[SaveLeft] + logic.Game.RobotBetSummary[SaveMiddle] + logic.Game.RobotBetSummary[SaveRight]
	diff := goalGroup - saveGroup
	if int64(math.Abs(float64(diff))) >= rbtc.line {
		if diff > 0 {
			noBetTypes = []BetType{Goal, GoalLeft, GoalMiddle, GoalRight}
		} else {
			noBetTypes = []BetType{Save, SaveLeft, SaveMiddle, SaveRight}
		}
	}
	betTypeSeeds := make([]BetType, 0)
	if len(noBetTypes) > 0 {
		for _, v := range rbtc.betTypes {
			if !betTypeContains(noBetTypes, v) {
				betTypeSeeds = append(betTypeSeeds, v)
			} else {
				betTypeSeeds = append(betTypeSeeds, 0)
			}
		}
	} else {
		betTypeSeeds = rbtc.betTypes
	}
	example.invalid("current diff:%d, noBetType:%v, betTypeSeeds:%v", diff, noBetTypes, betTypeSeeds)
	if len(betTypeSeeds) > 0 {
		return betTypeSeeds[rand.Intn(len(betTypeSeeds))]
	}
	return 0
}

// 隨機下注金額	骰出後利用可下注金額湊出最接近骰出數的金額，不可超過
func (rbtc *RobotCha) RandomBetMoney(allowAmounts map[int64]bool) []int64 {

	// 產生一個隨機金額 minBetCount < amt < maxBetCount
	var amt int64
	diff := rbtc.maxBetCount - rbtc.minBetCount
	amt = rbtc.minBetCount
	if diff > 0 {
		amt = rand.Int63n(rbtc.maxBetCount-rbtc.minBetCount) + rbtc.minBetCount
	}

	// 整理可下注的籌碼 由大到小
	var options []int64
	for v := range allowAmounts {
		options = append(options, v)
	}
	sort.Slice(options, func(i, j int) bool { return options[i] > options[j] })

	findBetableAmount := func(v int64) int64 {
		for _, n := range options {
			if n <= v {
				return n
			}
		}
		return 0
	}

	// 使用可下注的數字湊出接近 amt 的籌碼 最大的先
	var bets []int64

	for {
		n := findBetableAmount(amt)
		if n == 0 {
			break
		}

		bets = append(bets, n)
		amt -= n
	}

	return bets
}

// 下注 最終成功與否由 Game Logic 判斷
func (rbtc *RobotCha) Bet(rbtLogic *RobotLogic, logic *Logic, robot *robotcomm.Robot) {

	t := rbtc.GetBetType(logic)
	if t == 0 { // 沒有下注區域
		rbtLogic.DelayRun(1, func() { // 延遲一秒再次下注
			rbtc.Bet(rbtLogic, logic, robot)
		})
		return
	}
	moneys := rbtc.RandomBetMoney(logic.Game.BetConfig.AllowAmounts)
	if len(moneys) > 0 {
		moneys = moneys[0:1] // 只下最大那一個
	} else {
		moneys = []int64{0}
	}

	for _, money := range moneys {
		m := new(msg.Message)
		m.BClassID = int32(netproto.MessageBClassID_Game)
		m.SClassID = int32(netproto.SML_GameMessageClassID_SMLBetID)

		data := &netproto.SML_BetRequest{
			Bet: &netproto.SML_Bet{
				Type:  TransforBetType(t).Enum(),
				Money: proto.Int64(money),
			},
		}

		b, err := proto.Marshal(data)
		if err != nil {
			return
		}
		m.MsgData = b

		rbtLogic.SendGameMessage(robot.UserInfo, m)
	}

}
