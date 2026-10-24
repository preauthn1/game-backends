package zhajinhualogic

import (
	"config"
	"event"
	"fmt"
	"gamelogic"
	"math/rand"
	"mlog"
	"msg"
	"netproto"
	"poker"
	"robot/robotcomm"
	"robot/robotlogic"
	"strconv"
	"strings"
	"time"
	"util"
)

import (
	"example.invalid/golang/protobuf/proto"
)

const (
	RobotLogicConfigFileName = "robot"
)

type gailv struct {
	check       int32 //看牌概率
	compare     int32 //比牌概率
	follow      int32 //跟注概率
	bet         int32 //下注概率
	add         int32 //加注概率
	weightindex int32 //权重索引
	addfollow   int32 //玩家加注以后机器人是否跟注
	dropbefor   int32 //弃牌之前是否比牌
	dropturn    int32 //差的牌棄牌局數
}

const (
	ODDS_Robot_Check       int32 = iota //看牌概率
	ODDS_Robot_Compare                  //比牌概率
	ODDS_Robot_Follow                   //跟注概率
	ODDS_Robot_Bet                      //下注概率
	ODDS_Robot_Add                      //加注概率
	ODDS_Robot_Weightindex              //权重索引
	ODDS_Robot_AddFollow                //玩家加注以后机器人是否跟注
	ODDS_Robot_DropBefor                //弃牌之前是否比牌
	ODDS_Robot_DropTurn                 //差的牌棄牌局數
	ODDS_Robot_Count
)

type leavelv struct {
	drop int32 //棄牌離桌概率
	lose int32 //比牌輸牌離桌概率
}

type weight struct {
	w []int32 //权重数组
}

//机器人个性
type RobotCha struct {
	baozi      gailv     //豹子处理的概率表
	shunjin    gailv     //顺金处理的概率表
	jinhua     gailv     //金花处理的概率表
	shunzi     gailv     //顺子处理的概率表
	duizi      gailv     //对子处理的概率表
	danzhang   gailv     //单张处理的概率表
	leave      leavelv   //離桌處理概率表
	drop       int32     //弃牌
	w          []*weight //所有权重数据
	rndmaxturn []int     //機器人贏牌的最大跟注局數
	iWinRate   int32     //比牌時機器人賭神換牌機率
}

// 機器人賭神換牌機率範圍
const (
	ODDS_Robot_WinRate_Min = 1
	ODDS_Robot_WinRate_Max = 10
)

//创建机器个性对象
func NewRobotCha(cf *config.Config, cname string) *RobotCha {
	if cf == nil {
		mlog.Error("創建機器個性對象失敗，無 Config 資料")
		return nil
	}

	rcha := new(RobotCha)

	s1 := cf.GetConfigValue(cname, "baozi")
	strarr := strings.Split(s1, ",")
	rcha.getGaiLv(&rcha.baozi, strarr)

	s1 = cf.GetConfigValue(cname, "shunjin")
	strarr = strings.Split(s1, ",")
	rcha.getGaiLv(&rcha.shunjin, strarr)

	s1 = cf.GetConfigValue(cname, "jinhua")
	strarr = strings.Split(s1, ",")
	rcha.getGaiLv(&rcha.jinhua, strarr)

	s1 = cf.GetConfigValue(cname, "shunzi")
	strarr = strings.Split(s1, ",")
	rcha.getGaiLv(&rcha.shunzi, strarr)

	s1 = cf.GetConfigValue(cname, "duizi")
	strarr = strings.Split(s1, ",")
	rcha.getGaiLv(&rcha.duizi, strarr)

	s1 = cf.GetConfigValue(cname, "danzhang")
	strarr = strings.Split(s1, ",")
	rcha.getGaiLv(&rcha.danzhang, strarr)

	s1 = cf.GetConfigValue(cname, "rndmaxturn")
	strarr = strings.Split(s1, ",")
	r1, _ := strconv.Atoi(strarr[0])
	rcha.rndmaxturn = append(rcha.rndmaxturn, r1)
	r1, _ = strconv.Atoi(strarr[1])
	rcha.rndmaxturn = append(rcha.rndmaxturn, r1)

	s1 = cf.GetConfigValue(cname, "leave")
	if s1 != "" {
		strarr = strings.Split(s1, ",")
		rcha.leave.drop = rcha.getIntValue(strarr[0])
		rcha.leave.lose = rcha.getIntValue(strarr[1])
	}

	rcha.drop = cf.GetConfigInt32(cname, "drop")

	rcha.w = make([]*weight, 0)
	cn := cf.GetConfigInt(cname, "cwn")
	for i := 0; i < cn; i++ {
		v := fmt.Sprintf("weight%d", i)
		s2 := cf.GetConfigValue(cname, v)
		strarr2 := strings.Split(s2, ",")
		w := new(weight)
		w.w = make([]int32, 0)
		for j := 0; j < len(strarr2); j++ {
			n, _ := strconv.ParseInt(strarr2[j], 10, 32)
			w.w = append(w.w, int32(n))
		}

		rcha.w = append(rcha.w, w)
	}

	iWinRate := cf.GetConfigInt32(cname, "winrate")
	if iWinRate < ODDS_Robot_WinRate_Min {
		iWinRate = ODDS_Robot_WinRate_Min
	} else if iWinRate > ODDS_Robot_WinRate_Max {
		iWinRate = ODDS_Robot_WinRate_Max
	}
	rcha.iWinRate = iWinRate

	return rcha
}

func (rc *RobotCha) getIntValue(str string) int32 {
	v, _ := strconv.ParseInt(str, 10, 32)
	return int32(v)
}

//获取概率表
func (rc *RobotCha) getGaiLv(gl *gailv, strarr []string) {
	if gl == nil || len(strarr) < int(ODDS_Robot_Count) {
		mlog.Error("獲取概率表失敗，gl[%v], len(strarr) = %d", gl, len(strarr))
		return
	}

	gl.check = rc.getIntValue(strarr[ODDS_Robot_Check])
	gl.compare = rc.getIntValue(strarr[ODDS_Robot_Compare])
	gl.follow = rc.getIntValue(strarr[ODDS_Robot_Follow])
	gl.bet = rc.getIntValue(strarr[ODDS_Robot_Bet])
	gl.add = rc.getIntValue(strarr[ODDS_Robot_Add])
	gl.weightindex = rc.getIntValue(strarr[ODDS_Robot_Weightindex])
	gl.addfollow = rc.getIntValue(strarr[ODDS_Robot_AddFollow])
	gl.dropbefor = rc.getIntValue(strarr[ODDS_Robot_DropBefor])
	gl.dropturn = rc.getIntValue(strarr[ODDS_Robot_DropTurn]) - 1
}

//判断机器人是否看牌
func (rc *RobotCha) isCheck(t int32) bool {
	n := rand.Int31n(100)
	if t == BaoZi {
		if n < rc.baozi.check {
			return true
		}
	} else if t == SunJin {
		if n < rc.shunjin.check {
			return true
		}
	} else if t == JiaHua {
		if n < rc.jinhua.check {
			return true
		}
	} else if t == SunZi {
		if n < rc.shunzi.check {
			return true
		}
	} else if t == DuiZi {
		if n < rc.duizi.check {
			return true
		}
	} else if t == SanPai {
		if n < rc.danzhang.check {
			return true
		}
	}

	return false
}

//判断是否比牌
func (rc *RobotCha) isCompare(t int32) bool {
	n := rand.Int31n(100)
	if t == BaoZi {
		if n < rc.baozi.compare {
			return true
		}
	} else if t == SunJin {
		if n < rc.shunjin.compare {
			return true
		}
	} else if t == JiaHua {
		if n < rc.jinhua.compare {
			return true
		}
	} else if t == SunZi {
		if n < rc.shunzi.compare {
			return true
		}
	} else if t == DuiZi {
		if n < rc.duizi.compare {
			return true
		}
	} else if t == SanPai {
		if n < rc.danzhang.compare {
			return true
		}
	}

	return false
}

//判断加注以后是否跟注
func (rc *RobotCha) isAddFollow(t int32) bool {
	n := rand.Int31n(100)
	if t == BaoZi {
		if n < rc.baozi.addfollow {
			return true
		}
	} else if t == SunJin {
		if n < rc.shunjin.addfollow {
			return true
		}
	} else if t == JiaHua {
		if n < rc.jinhua.addfollow {
			return true
		}
	} else if t == SunZi {
		if n < rc.shunzi.addfollow {
			return true
		}
	} else if t == DuiZi {
		if n < rc.duizi.addfollow {
			return true
		}
	} else if t == SanPai {
		if n < rc.danzhang.addfollow {
			return true
		}
	}

	return false
}

//是否加注
func (rc *RobotCha) isAdd(t int32) bool {
	n := rand.Int31n(100)
	if t == BaoZi {
		if n < rc.baozi.add && rc.baozi.weightindex >= 0 {
			return true
		}
	} else if t == SunJin {
		if n < rc.shunjin.add && rc.shunjin.weightindex >= 0 {
			return true
		}
	} else if t == JiaHua {
		if n < rc.jinhua.add && rc.jinhua.weightindex >= 0 {
			return true
		}
	} else if t == SunZi {
		if n < rc.shunzi.add && rc.shunzi.weightindex >= 0 {
			return true
		}
	} else if t == DuiZi {
		if n < rc.duizi.add && rc.duizi.weightindex >= 0 {
			return true
		}
	} else if t == SanPai {
		if n < rc.danzhang.add && rc.danzhang.weightindex >= 0 {
			return true
		}
	}

	return false
}

//是否比牌前棄牌
func (rc *RobotCha) isDropBefor(t int32) bool {
	n := rand.Int31n(100)
	if t == BaoZi {
		if n < rc.baozi.dropbefor {
			return true
		}
	} else if t == SunJin {
		if n < rc.shunjin.dropbefor {
			return true
		}
	} else if t == JiaHua {
		if n < rc.jinhua.dropbefor {
			return true
		}
	} else if t == SunZi {
		if n < rc.shunzi.dropbefor {
			return true
		}
	} else if t == DuiZi {
		if n < rc.duizi.dropbefor {
			return true
		}
	} else if t == SanPai {
		if n < rc.danzhang.dropbefor {
			return true
		}
	}

	return false
}

//是否差的牌棄牌局數
func (rc *RobotCha) isDropTurn(t int32, turn int32) bool {
	if t == BaoZi {
		if turn >= rc.baozi.dropturn {
			return true
		}
	} else if t == SunJin {
		if turn >= rc.shunjin.dropturn {
			return true
		}
	} else if t == JiaHua {
		if turn >= rc.jinhua.dropturn {
			return true
		}
	} else if t == SunZi {
		if turn >= rc.shunzi.dropturn {
			return true
		}
	} else if t == DuiZi {
		if turn >= rc.duizi.dropturn {
			return true
		}
	} else if t == SanPai {
		if turn >= rc.danzhang.dropturn {
			return true
		}
	}

	return false
}

//是否跟注
func (rc *RobotCha) isFollow(t int32) bool {
	n := rand.Int31n(100)
	if t == BaoZi {
		if n < rc.baozi.follow {
			return true
		}
	} else if t == SunJin {
		if n < rc.shunjin.follow {
			return true
		}
	} else if t == JiaHua {
		if n < rc.jinhua.follow {
			return true
		}
	} else if t == SunZi {
		if n < rc.shunzi.follow {
			return true
		}
	} else if t == DuiZi {
		if n < rc.duizi.follow {
			return true
		}
	} else if t == SanPai {
		if n < rc.danzhang.follow {
			return true
		}
	}

	return false
}

func (rc *RobotCha) isLeave(index int) bool {
	n := rand.Int31n(100)
	if index == 0 && n < rc.leave.drop {
		return true
	}
	if index == 1 && n < rc.leave.lose {
		return true
	}
	return false
}

//获取加注金额
func (rc *RobotCha) Add(add []int32, t int32) int32 {
	if len(add) <= 0 {
		mlog.Debug("陣列長度為 0")
		return 0
	}

	count := len(add)
	index := int32(0)
	if t == BaoZi {
		index = rc.baozi.weightindex
	} else if t == SunJin {
		index = rc.shunjin.weightindex
	} else if t == JiaHua {
		index = rc.jinhua.weightindex
	} else if t == SunZi {
		index = rc.shunzi.weightindex
	} else if t == DuiZi {
		index = rc.duizi.weightindex
	} else if t == SanPai {
		index = rc.danzhang.weightindex
	}

	if index < 0 {
		return 0
	}

	mlog.Debug("权重索引：%v", index)
	wlen := len(rc.w[index].w)
	if count > wlen {
		return add[0]
	}

	total := int32(0)
	for i := wlen - 1; i >= wlen-count; i-- {
		total += rc.w[index].w[i]
	}

	if total > 0 {
		n := rand.Int31n(total)
		tmp := int32(0)
		for i := wlen - 1; i >= wlen-count; i-- {
			tmp += rc.w[index].w[i]
			if n < tmp {
				return add[i-wlen+count]
			}
		}
	}

	return add[0]
}

//机器人逻辑
type ZJHRobotLogic struct {
	*robotlogic.RobotLogicBase
	robotChas []*RobotCha
	gameChas  map[int32]*RobotCha
}

//新建一个机器人逻辑
func NewZJHRobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *ZJHRobotLogic {
	rblgc := new(ZJHRobotLogic)
	rblgc.RobotLogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)
	rblgc.parseRobotCha(configs)
	rblgc.gameChas = make(map[int32]*RobotCha)

	return rblgc
}

//解析机器人个性
func (lgc *ZJHRobotLogic) parseRobotCha(cfgs *config.ConfigGroup) {
	if cfgs == nil {
		mlog.Debug("無 ConfigGroup 資料")
		return
	}

	rand.Seed(time.Now().UnixNano())
	robotCfg := cfgs.GetConfig(RobotLogicConfigFileName)
	if robotCfg == nil {
		return
	}

	cn := robotCfg.GetConfigInt("csize", "cn")

	for i := 1; i <= cn; i++ {
		cname := fmt.Sprintf("c%d", i)
		cha := NewRobotCha(robotCfg, cname)
		lgc.robotChas = append(lgc.robotChas, cha)
	}
}

//获取机器人个性
func (lgc *ZJHRobotLogic) getRobotCha(userid int32) *RobotCha {
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

func (lgc *ZJHRobotLogic) getGameLogic() *ZhaJinHuaLogic {
	return lgc.GameLogic.(*ZhaJinHuaLogic)
}

func (lgc *ZJHRobotLogic) getPoker() *poker.FixedPoker {
	return lgc.getGameLogic().poker.(*poker.FixedPoker)
}

//游戏消息
func (lgc *ZJHRobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {
	if robot == nil {
		mlog.Debug("無機器人資料")
		return
	} else if m == nil {
		mlog.Debug("message 為空資料")
		return
	}

	//获取机器人个性
	cha := lgc.getRobotCha(robot.UserInfo.UserID)
	glc := lgc.getGameLogic()
	if cha == nil {
		mlog.Debug("無機器人資料")
		return
	} else if glc == nil {
		mlog.Debug("遊戲邏輯資料為空")
	}

	robotdata := glc.getUserBySeatNo(robot.UserInfo.SeatNo)
	if robotdata == nil {
		mlog.Debug("机器人数据是空")
		return
	}

	glc.robotCharacter[robot.UserInfo.UserID] = cha

	if m.SClassID == int32(netproto.ZJH_GameMessageClassID_CompareRetID) {
		ust := m.MsgData.(*netproto.ZJH_CompareCardsRet)
		if robot.UserInfo.SeatNo == ust.GetFaildseatno() && cha.isLeave(1) {
			lgc.SendGameMessage(robot.UserInfo, lgc.getEarlyChangeTableMsg())
		}
	}
	if m.SClassID == int32(netproto.ZJH_GameMessageClassID_WaitOptID) {
		userCardType := GetType(robotdata.userCards)
		robotDropOrCompare := false
		userSeatNo := int32(-1)

		iCompareCardRet := int(-1)

		//機器人跟所有真人比牌, 當機器人贏所有人, 限制跟注上限. 當機器人輸其中一個真人, 限制棄牌局數
		for _, v := range glc.userData {
			if v == nil {
				continue
			}

			if !v.isDrop && !v.isFaild {
				if v.User.UserID != robotdata.User.UserID {
					ud := glc.getUserBySeatNo(v.User.SeatNo)
					if ud == nil {
						mlog.Debug("数据是空的？")
						return
					}

					//機器人輸
					iCompareCardRet = comPareCards(robotdata.userCards, ud.userCards)
					if iCompareCardRet == -1 {
						mlog.Debug("比牌結果錯誤")
						return
					} else if iCompareCardRet != USER1 && !v.User.IsRobot() {
						if cha.isDropTurn(userCardType, lgc.getGameLogic().currturn) {
							mlog.Debug("%v機器人輸 棄牌或比牌局數 turn : %v", robotdata.User.UserID, lgc.getGameLogic().currturn)
							robotDropOrCompare = true
							userSeatNo = v.User.SeatNo
							break
						}
					}

					maxTurnLimit := util.RndInt(cha.rndmaxturn[0], cha.rndmaxturn[1]+1)
					if lgc.getGameLogic().currturn >= int32(maxTurnLimit) && lgc.getGameLogic().currturn > 3 {
						mlog.Debug("%v機器人Win 比牌 userid[%v] turn[%v] maxturn[%v] ", robotdata.User.UserID, v.User.UserID, lgc.getGameLogic().currturn, maxTurnLimit)
						lgc.SendGameMessage(robot.UserInfo, lgc.getCompareMsg(ud.User.SeatNo))
						return
					}
				}
			}
		}

		//查看自己的牌是否最大
		mlog.Debug("机器人：%v", robot.UserInfo.UserID)
		ust := m.MsgData.(*netproto.ZJH_WaitOpt)
		for _, v := range glc.userData {
			if v == nil {
				continue
			}

			if !v.isDrop && !v.isFaild {
				if v.User.UserID != robotdata.User.UserID {
					userSeatNo = v.User.SeatNo
					if cha.isDropTurn(userCardType, lgc.getGameLogic().currturn) {
						robotDropOrCompare = true
					}
				}
			}
		}

		ud := glc.getUserBySeatNo(userSeatNo)
		if ud == nil {
			mlog.Debug("数据是空的？")
			return
		}

		if ud.canOpt() {

			if ud.isAllin {
				iCompareCardRet = comPareCards(robotdata.userCards, ud.userCards)
				if iCompareCardRet == -1 {
					mlog.Debug("比牌結果錯誤")
					return
				} else if iCompareCardRet == USER1 {
					mlog.Debug("%v機器人贏 比牌 ", robotdata.User.UserID)
					//lgc.SendGameMessage(robot.UserInfo, lgc.getCompareMsg(ud.User.SeatNo))
					lgc.SendGameMessage(robot.UserInfo, lgc.getFollowMsg())
				} else {
					mlog.Debug("%v機器人輸 主動棄牌 ", robotdata.User.UserID)
					lgc.sendDropMessage(robot.UserInfo, ust)
				}
				return
			}

			iCompareCardRet = comPareCards(robotdata.userCards, ud.userCards)
			if iCompareCardRet == -1 {
				mlog.Debug("比牌結果錯誤")
				return
			} else if iCompareCardRet != USER1 {
				mlog.Debug("机器人：%v的牌不是最大", robotdata.User.UserID)
				if robotDropOrCompare {
					if !cha.isDropBefor(userCardType) {
						//低於第三回合，跟注
						if lgc.getGameLogic().currturn < GAME_AllowCompareTurn {
							mlog.Debug("%v機器人輸 跟注 ", robotdata.User.UserID)
							lgc.SendGameMessage(robot.UserInfo, lgc.getFollowMsg())
						} else {
							mlog.Debug("%v機器人輸 比牌 ", robotdata.User.UserID)
							//mlog.Debug("賭神：第 %d 輪，OnGameMessage(), ud[%d], IsRobot[%t], robotdata[%d], IsRobot[%t]",
							//	glc.currturn, ud.User.UserID, ud.User.IsRobot(), robotdata.User.UserID, robotdata.User.IsRobot())
							/*
							// 原機器人比牌
							if !ud.User.IsRobot() {
								glc.InitiateRobotToGod(robotdata.User.SeatNo, ud.User.SeatNo)
							}
							 */
							lgc.SendGameMessage(robot.UserInfo, lgc.getCompareMsg(ud.User.SeatNo))
						}

					} else {
						mlog.Debug("%v機器人輸 不跟注不比牌直接看牌後棄牌 ", robotdata.User.UserID)
						if lgc.getGameLogic().currturn >= lgc.getGameLogic().checkturn {
							lgc.SendGameMessageNow(robot.UserInfo, lgc.getCheckMsg())
						}
						lgc.sendDropMessage(robot.UserInfo, ust)
					}
					return
				}
			}
		}

		if ust.GetCheck() && cha.isCheck(userCardType) {
			if lgc.getGameLogic().currturn >= lgc.getGameLogic().checkturn {
				lgc.SendGameMessage(robot.UserInfo, lgc.getCheckMsg())
			}
			return
		}

		if cha.isCompare(userCardType) && ust.GetCompare() {
			for _, ud := range glc.userData {
				if ud.User.UserID == robot.UserInfo.UserID {
					continue
				}
				if ud.canOpt() && lgc.getGameLogic().currturn >= GAME_AllowCompareTurn {
					lgc.SendGameMessage(robot.UserInfo, lgc.getCompareMsg(ud.User.SeatNo))
					return
				}
			}
		}

		if ust.GetFirst() {
			if cha.isAdd(userCardType) {
				add := cha.Add(ust.GetBetaddvalue(), userCardType)
				lgc.SendGameMessage(robot.UserInfo, lgc.getAddMsg(add))
			} else {
				lgc.SendGameMessage(robot.UserInfo, lgc.getAddMsg(ust.GetBetaddvalue()[0]))
			}

			return
		}

		if glc.isadd {
			if cha.isAddFollow(userCardType) {
				if ust.GetBetadd() && len(ust.GetBetaddvalue()) > 1 && cha.isAdd(userCardType) {
					add := cha.Add(ust.GetBetaddvalue(), userCardType)
					lgc.SendGameMessage(robot.UserInfo, lgc.getAddMsg(add))
				} else {
					lgc.SendGameMessage(robot.UserInfo, lgc.getFollowMsg())
				}
			} else {
				mlog.Debug("机器人：%v加注以后跟注弃牌", robot.UserInfo.UserID)
				lgc.sendDropMessage(robot.UserInfo, ust)
			}

			return
		}

		if cha.isFollow(userCardType) {
			if ust.GetBetadd() && len(ust.GetBetaddvalue()) > 1 && cha.isAdd(userCardType) {
				add := cha.Add(ust.GetBetaddvalue(), userCardType)
				lgc.SendGameMessage(robot.UserInfo, lgc.getAddMsg(add))
			} else {
				lgc.SendGameMessage(robot.UserInfo, lgc.getFollowMsg())
			}

			return
		}

		if ust.GetCheck() {
			if lgc.getGameLogic().currturn >= lgc.getGameLogic().checkturn {
				lgc.SendGameMessage(robot.UserInfo, lgc.getCheckMsg())
			}
		} else if ud.isCheck {
			mlog.Debug("机器人：%v无操作弃牌", robot.UserInfo.UserID)
			lgc.sendDropMessage(robot.UserInfo, ust)
		} else {
			lgc.SendGameMessage(robot.UserInfo, lgc.getFollowMsg())
		}
	}
}

func (lgc *ZJHRobotLogic) OnGameEnd(robot *robotcomm.Robot) {
	if robot == nil {
		mlog.Debug("無機器人資料")
		return
	}

	lgc.gameChas = make(map[int32]*RobotCha)
}

//是否弃牌
func (lgc *ZJHRobotLogic) isDrop(robot *robotcomm.Robot) bool {
	if robot == nil {
		mlog.Debug("無機器人資料")
		return false
	}

	glc := lgc.getGameLogic()
	cha := lgc.getRobotCha(robot.UserInfo.UserID)
	if cha == nil || glc == nil {
		mlog.Debug("資料包含 nil, cha[%v], glc[%v]", cha, glc)
		return false
	}

	drop := cha.drop * glc.currturn
	if drop >= 100 {
		return true
	}

	n := rand.Int31n(100)
	if n < drop {
		return true
	}

	return false
}

//弃牌操作
func (lgc *ZJHRobotLogic) dropAction(robot *robotcomm.Robot, ust *netproto.ZJH_WaitOpt) bool {
	if robot == nil {
		mlog.Debug("機器人資料為空")
		return false
	}

	var iCompareCardsRet int

	if !lgc.isDrop(robot) {
		return false
	}

	upk := lgc.getPoker().GetUserCards(robot.UserInfo.UserID)
	glc := lgc.getGameLogic()
	ud := glc.getUserBySeatNo(robot.UserInfo.SeatNo)
	if upk == nil || glc == nil || ud == nil {
		mlog.Debug("資料包含 nil, upk[%v], glc[%v], ud[%v]", upk, glc, ud)
		return false
	}

	if GetType(upk) > JiaHua {
		if *ust.Compare {
			if ud.canOpt() && ust.GetCompare() {
				//自己的牌不是最大
				iCompareCardsRet = comPareCards(lgc.getPoker().GetUserCards(robot.UserInfo.UserID), ud.userCards)
				if iCompareCardsRet == -1 {
					mlog.Debug("比牌結果錯誤")
					return false
				} else if iCompareCardsRet != USER1 {
					if lgc.getGameLogic().currturn > 2 {
						//发送比牌消息
						lgc.SendGameMessage(robot.UserInfo, lgc.getCompareMsg(ud.User.SeatNo))
						return true
					}
				}
			}
		}
	} else {
		if *ust.Check {
			//发送看牌消息
			if lgc.getGameLogic().currturn >= lgc.getGameLogic().checkturn {
				lgc.SendGameMessage(robot.UserInfo, lgc.getCheckMsg())
			}
		} else if ud.isCheck {
			mlog.Debug("机器人：%v自己牌小弃牌", robot.UserInfo.UserID)
			lgc.sendDropMessage(robot.UserInfo, ust)
		} else {
			return false
		}

		return true
	}

	return false
}

//看牌消息
func (lgc *ZJHRobotLogic) getCheckMsg() *msg.Message {
	mlog.Debug("机器人看牌")
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_CheckCardsID)
	return btmsg
}

//弃牌消息
func (lgc *ZJHRobotLogic) getDropMsg() *msg.Message {
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_DropID)

	return btmsg
}

//比牌消息
func (lgc *ZJHRobotLogic) getCompareMsg(seat int32) *msg.Message {
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_CompareID)

	btdata := new(netproto.ZJH_SeatNO)
	btdata.Seatno = proto.Int32(seat)
	btmsg.MsgData, _ = proto.Marshal(btdata)
	return btmsg
}

//加注消息
func (lgc *ZJHRobotLogic) getAddMsg(add int32) *msg.Message {
	mlog.Debug("机器人加注")
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_BetAddID)
	btdata := new(netproto.ZJH_BetAdd)
	btdata.Betadd = proto.Int32(add)
	btmsg.MsgData, _ = proto.Marshal(btdata)
	return btmsg
}

//跟注消息
func (lgc *ZJHRobotLogic) getFollowMsg() *msg.Message {
	mlog.Debug("机器人跟注")
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_FollowID)
	return btmsg
}

//跟注消息
func (lgc *ZJHRobotLogic) getEarlyChangeTableMsg() *msg.Message {
	mlog.Debug("机器人提前換桌")
	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_EarlyChangeTable)
	return btmsg
}

//發送棄牌消息
func (lgc *ZJHRobotLogic) sendDropMessage(ur *gamelogic.GameUser, ust *netproto.ZJH_WaitOpt) {
	if ur == nil {
		mlog.Debug("無 GameUser 資料")
		return
	}

	mlog.Debug("机器人棄牌並判斷是否換桌")
	rc := lgc.getRobotCha(ur.UserID)
	checkMsg := lgc.getCheckMsg()
	dropMsg := lgc.getDropMsg()
	//drop action
	dropAction := func() {
		lgc.DelayRun(lgc.RobotConfig.GameActionRndWaitSpan()+1, func() {
			lgc.GameLogic.GameMessage(ur, dropMsg)
			if rc.isLeave(0) {
				lgc.SendGameMessage(ur, lgc.getEarlyChangeTableMsg())
			}
		})
	}
	if ust.GetCheck() {
		//check action
		lgc.DelayRun(lgc.RobotConfig.GameActionRndWaitSpan(), func() {
			lgc.GameLogic.GameMessage(ur, checkMsg)
		})
	} else {
		dropAction()
	}
}
