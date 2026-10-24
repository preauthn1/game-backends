package roulette

import (
	"fmt"
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

// |   | 3 | 6 | 9 | 12 | 15 | 18 | 21 | 24 | 27 | 30 | 33 | 36 | 3RD ROW |
// | 0 | 2 | 5 | 8 | 11 | 14 | 17 | 20 | 23 | 26 | 29 | 32 | 35 | 2ND ROW |
// |   | 1 | 4 | 7 | 10 | 13 | 16 | 19 | 22 | 25 | 28 | 31 | 34 | 1ST ROW |
// |   |     1ST12      |      2ND12        |       3RD12       |         |
// |   |  1-18  | EVEN  |   RED   |  BLACK  |   ODD   |  19-36  |         |
var RobotBetAreasNumbers = map[BetType][][]uint32{
	Odd:       {},
	Even:      {},
	Red:       {},
	Black:     {},
	Small:     {},
	Big:       {},
	First12:   {},
	Second12:  {},
	Third12:   {},
	FirstRow:  {},
	SecondRow: {},
	ThirdRow:  {},
	OneNumber: {{1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23}, {24}, {25}, {26}, {27}, {28}, {29}, {30}, {31}, {32}, {33}, {34}, {35}, {36}},
	TwoNumber: {
		{1, 2}, {2, 3},
		{4, 5}, {5, 6},
		{7, 8}, {8, 9},
		{10, 11}, {11, 12},
		{13, 14}, {14, 15},
		{16, 17}, {17, 18},
		{19, 20}, {20, 21},
		{22, 23}, {23, 24},
		{25, 26}, {26, 27},
		{28, 29}, {29, 30},
		{31, 32}, {32, 33},
		{34, 35}, {35, 36},
		{0, 1}, {0, 2}, {0, 3},
		{1, 4}, {2, 5}, {3, 6},
		{4, 7}, {5, 8}, {6, 9},
		{7, 10}, {8, 11}, {9, 12},
		{10, 13}, {11, 14}, {12, 15},
		{13, 16}, {14, 17}, {15, 18},
		{16, 19}, {17, 20}, {18, 21},
		{19, 22}, {20, 23}, {21, 24},
		{22, 25}, {23, 26}, {24, 27},
		{25, 28}, {26, 29}, {27, 30},
		{28, 31}, {29, 32}, {30, 33},
		{31, 34}, {32, 35}, {33, 36},
	},
	ThreeNumber: {
		{0, 1, 2}, {0, 2, 3},
		{1, 2, 3},
		{4, 5, 6},
		{7, 8, 9},
		{10, 11, 12},
		{13, 14, 15},
		{16, 17, 18},
		{19, 20, 21},
		{22, 23, 24},
		{25, 26, 27},
		{28, 29, 30},
		{31, 32, 33},
		{34, 35, 36},
	},
	FourNumber: {
		{1, 2, 4, 5}, {2, 3, 5, 6},
		{4, 5, 7, 8}, {5, 6, 8, 9},
		{7, 8, 10, 11}, {8, 9, 11, 12},
		{10, 11, 13, 14}, {11, 12, 14, 15},
		{13, 14, 16, 17}, {14, 15, 17, 18},
		{16, 17, 19, 20}, {17, 18, 20, 21},
		{19, 20, 22, 23}, {20, 21, 23, 24},
		{22, 23, 25, 26}, {23, 24, 26, 27},
		{25, 26, 28, 29}, {26, 27, 29, 30},
		{28, 29, 31, 32}, {29, 30, 32, 33},
		{31, 32, 34, 35}, {32, 33, 35, 36},
	},
	SixNumber: {
		{1, 2, 3, 4, 5, 6},
		{4, 5, 6, 7, 8, 9},
		{7, 8, 9, 10, 11, 12},
		{10, 11, 12, 13, 14, 15},
		{13, 14, 15, 16, 17, 18},
		{16, 17, 18, 19, 20, 21},
		{19, 20, 21, 22, 23, 24},
		{22, 23, 24, 25, 26, 27},
		{25, 26, 27, 28, 29, 30},
		{28, 29, 30, 31, 32, 33},
		{31, 32, 33, 34, 35, 36},
	},
	SuperZero: {},
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

		{ // 解析可下注範圍
			s := robotConf.GetConfigValue(cname, "bettype")
			s = strings.TrimSpace(s)
			arr := strings.Split(s, ",")
			if len(arr) == 0 {
				mlog.Error("機器人個性 %s 設定錯誤, 可下注範圍錯誤, bettype: %s", cname, s)
				continue
			}

			for _, v := range arr {
				v = strings.TrimSpace(v)
				if v == "" {
					continue
				}

				n, err := strconv.ParseUint(v, 0, 64)
				if err != nil {
					mlog.Error("機器人個性 %s 設定錯誤, 可下注範圍錯誤, %v", cname, err)
					continue
				}

				bt := BetType(n)
				if bt == 0 {
					mlog.Error("機器人個性 %s 設定錯誤, 可下注範圍錯誤, %v", cname, err)
					continue
				}

				// check bet type is valid
				if _, ok := RobotBetAreasNumbers[bt]; !ok {
					mlog.Error("機器人個性 %s 設定錯誤, 可下注範圍錯誤, %v", cname, err)
					continue
				}

				betTypes = append(betTypes, bt)
			}
		}

		cha := NewRobotCha(minBetCount, maxBetCount, betTypes)
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

	if (m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteStepID) && logic.Game.Step == WaitBet) ||
		m.SClassID == int32(netproto.Roulette_GameMessageClassID_RouletteBetRetID) {
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

// 機器人個性
type RobotCha struct {
	minBetCount int64     // 最小下注金額
	maxBetCount int64     // 最大下注金額
	BetTypes    []BetType // 會下注的區域
}

func NewRobotCha(minBetCount, maxBetCount int64, betTypes []BetType) *RobotCha {
	return &RobotCha{
		minBetCount: minBetCount,
		maxBetCount: maxBetCount,
		BetTypes:    betTypes,
	}
}

// 隨機選擇下注區域與數字
func (rbtc *RobotCha) GetBetType(logic *Logic) (BetType, []uint32) {

	if len(rbtc.BetTypes) == 0 {
		return 0, nil
	}
	t := rbtc.BetTypes[rand.Intn(len(rbtc.BetTypes))]
	numsOpts := RobotBetAreasNumbers[t]
	var nums []uint32
	if len(numsOpts) > 0 {
		nums = numsOpts[rand.Intn(len(numsOpts))]
	}

	return t, nums
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

	t, n := rbtc.GetBetType(logic)
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
		m.SClassID = int32(netproto.Roulette_GameMessageClassID_RouletteBetID)

		data := &netproto.Roulette_BetRequest{
			Bet: []*netproto.Roulette_Bet{
				{
					Type:    TransforBetType(t).Enum(),
					Money:   proto.Int64(money),
					Numbers: n,
				},
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
