//==================================================================
//文件名： ermjrobotlogic.go
//说明：   麻将机器人
//修改记录：
//==================================================================
package ermjlogic

import (
	"config"
	"event"
	"fmt"
	"gamelogic"
	"mahjong"
	"mlog"
	"msg"
	"netproto"
	"robot/robotcomm"
	"robot/robotlogic"
	"util"
)

import proto "example.invalid/golang/protobuf/proto"

const (
	RobotLogicConfigFileName = "robot"
)

//机器人个性
type RobotCha struct {
	changeRate  int //换牌概率
	changeTimes int //单次换牌次数
	ziMoRate    int //听牌后自摸概率
}

type rateInfo struct {
}

//机器人逻辑
type SCMJRobotLogic struct {
	*robotlogic.RobotLogicBase
	robotChas []*RobotCha
	gameChas  map[int32]*RobotCha
}

func NewSCMJRobotLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) *SCMJRobotLogic {
	lgc := new(SCMJRobotLogic)
	lgc.RobotLogicBase = robotlogic.NewRobotLogicBase(table, tfManager, robotCfg, configs, glc, evtChannel)
	lgc.gameChas = make(map[int32]*RobotCha, 0)
	lgc.parseRobotCha(configs)
	return lgc
}

//解析机器人个性
func (lgc *SCMJRobotLogic) parseRobotCha(cfgs *config.ConfigGroup) {
	if cfgs == nil {
		mlog.Error("桌號：%d, no ConfigGroup data", lgc.Table.TableNo)
		return
	}

	robotCfg := cfgs.GetConfig(RobotLogicConfigFileName)
	cn := robotCfg.GetConfigInt("csize", "cn")
	mlog.Debug("解析机器人配置文件,cn[%d]", cn)
	lgc.robotChas = make([]*RobotCha, 0)
	for i := 1; i <= cn; i++ {
		data := new(RobotCha)
		cname := fmt.Sprintf("c%d", i)
		data.changeRate = robotCfg.GetConfigInt(cname, "robotechangrate")
		data.changeTimes = robotCfg.GetConfigInt(cname, "robotechangtime")
		data.ziMoRate = robotCfg.GetConfigInt(cname, "robotezimo")
		lgc.robotChas = append(lgc.robotChas, data)
	}
}

//获取机器人个性
func (lgc *SCMJRobotLogic) getRobotCha(userid int32) *RobotCha {
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

func (lgc *SCMJRobotLogic) getGameLogic() *MahjongSC {
	return lgc.GameLogic.(*MahjongSC)
}

//游戏消息
func (lgc *SCMJRobotLogic) OnGameMessage(robot *robotcomm.Robot, m *msg.Message) {
	// 事件操作
	if lgc.isHaveCanOp(robot, m) {
		lgc.actionEvent(robot, m)
		return
	}
	// 出牌
	if lgc.isOrdertoOut(robot, m) {
		lgc.actionOutCard(robot, m)
		return
	}
	if lgc.isOrderDingque(robot, m) {
		lgc.actionDingque(robot, m)
	}
}

func (lgc *SCMJRobotLogic) OnGameEnd(robot *robotcomm.Robot) {
	lgc.gameChas = make(map[int32]*RobotCha)
}

// 是否轮到机器人出牌
func (lgc *SCMJRobotLogic) isOrdertoOut(robot *robotcomm.Robot, m *msg.Message) bool {
	if robot == nil || m == nil {
		return false
	}

	if int32(netproto.SCMJ_GameMessageClassID_SCMJWaitId) != int32(m.SClassID) || lgc.getGameLogic().gameState != PROGRESS_FIGHT {
		return false
	}
	if lgc.getGameLogic().currOpSeatNo != robot.UserInfo.SeatNo {
		return false
	}
	if lgc.getGameLogic().isEnd {
		mlog.Debug("[weihao] stop 设置自动操作 是否轮到机器人出牌")
		return false
	}
	return true
}

// 是否轮到机器人定缺
func (lgc *SCMJRobotLogic) isOrderDingque(robot *robotcomm.Robot, m *msg.Message) bool {
	if robot == nil || m == nil {
		return false
	}

	if int32(netproto.SCMJ_GameMessageClassID_SCMJWaitId) != int32(m.SClassID) || lgc.getGameLogic().gameState != PROGRESS_CHOOSE {
		return false
	}

	return true
}

// 是否轮到机器人可选操作
func (lgc *SCMJRobotLogic) isHaveCanOp(robot *robotcomm.Robot, m *msg.Message) bool {
	if robot == nil || m == nil {
		return false
	}

	if int32(netproto.SCMJ_GameMessageClassID_SCMJCanOpEventId) != int32(m.SClassID) {
		return false
	}

	return true
}

func (lgc *SCMJRobotLogic) actionEvent(robot *robotcomm.Robot, m *msg.Message) {
	mlog.Debug("事件操作")
	if robot == nil {
		mlog.Error("桌號[%d], 机器人事件操作, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}

	if m == nil {
		mlog.Error("桌號[%d], 机器人事件操作, 無訊息資料", lgc.GameLogic.TableNo())
		return
	}

	data := m.MsgData.(*netproto.SCMJ_OpEventInfo)
	if *data.Hu {
		opValue := *data.OtherValue
		lgc.doHU(robot.UserInfo, opValue)
		return
	}
	if *data.Gang {
		opValue := data.GangValue[0]
		lgc.doGang(robot.UserInfo, opValue)
		return
	}
	if *data.Peng {
		opValue := *data.OtherValue
		lgc.doPeng(robot.UserInfo, opValue)
		return
	}
	lgc.doPass(robot.UserInfo)
}

//actionDingque
func (lgc *SCMJRobotLogic) actionDingque(robot *robotcomm.Robot, m *msg.Message) {
	//mlog.Debug("定缺")
	if robot == nil {
		mlog.Error("桌號[%d], 机器人定缺, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}

	ud := lgc.getGameLogic().getUserInfoBySeatNo(robot.UserInfo.SeatNo)
	if ud == nil {
		return
	}
	tong, tiao, wan := 0, 0, 0
	for _, cd := range ud.handCards.Cards {
		if cd/100 == mahjong.COLOR_TONG {
			tong++
			continue
		}
		if cd/100 == mahjong.COLOR_TIAO {
			tiao++
			continue
		}
		if cd/100 == mahjong.COLOR_WAN {
			wan++
			continue
		}
	}

	opValue := tong
	opType := mahjong.COLOR_TONG
	if tong >= tiao {
		opValue = tiao
		opType = mahjong.COLOR_TIAO
	}
	if opValue > wan {
		opValue = wan
		opType = mahjong.COLOR_WAN
	}
	//
	if lgc.getGameLogic().Configs.GetConfig("gameserver").GetConfigInt("gameprivate", "havechooserate") == 1 {
		nRand := util.RndInt(0, 100)
		if nRand > 50 {
			lgc.doRate(robot.UserInfo, 2)
		} else {
			lgc.doRate(robot.UserInfo, 1)
		}

	} else {
		lgc.doDingque(robot.UserInfo, opType)
	}

}

//机器人出牌
func (lgc *SCMJRobotLogic) actionOutCard(robot *robotcomm.Robot, m *msg.Message) {
	mlog.Debug("机器人出牌")
	if robot == nil {
		mlog.Error("桌號[%d], 机器人出牌, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] ===== robot, actionOutCard() =====】")

	ud := lgc.getGameLogic().getUserInfoBySeatNo(robot.UserInfo.SeatNo)
	if ud == nil {
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)]】", ud.User.UserID, ud.seatNo)
	// [Joecool] for debug
	pUser := lgc.getGameLogic().getUserInfoBySeatNo(lgc.getGameLogic().currOpSeatNo)
	if pUser != nil {
		mlog.Debug("[Joecool] User[%d(%d)], canOpEventInfo.OpInfo, len = %d", pUser.User.UserID, pUser.seatNo, len(pUser.canOpEventInfo.OpInfo))
		for _, pInfo := range pUser.canOpEventInfo.OpInfo {
			if pInfo != nil {
				mlog.Debug("[Joecool] User[%d(%d)], OpInfo, %s, value[%v]",
					pUser.User.UserID, pUser.seatNo, OpTypeString_debug(pInfo.nOpType), pInfo.nValue)
			}
		}
	}
	// [Joecool] for debug
	if ud.seatNo != lgc.getGameLogic().currOpSeatNo {
		mlog.Debug("没有轮到机器人操作,不需要出牌")
		return
	}
	// config := lgc.Configs.GetConfig("gameserver")
	// robotTimes := config.GetConfigInt("gameprivate", "robotechangtime")
	rob := lgc.getRobotCha(robot.UserInfo.UserID)
	if rob != nil {
		mlog.Debug("【[Joecool] User[%d(%d)] rob = %v, ChangeRate = %d, ChangeTimes = %d, ZiMo = %d】",
			ud.User.UserID, ud.seatNo, rob, rob.changeRate, rob.changeTimes, rob.ziMoRate)
		for i := 0; i < rob.changeTimes; i++ {
			mlog.Debug("[Joecool] User[%d(%d)], 第 %d 次換牌", ud.User.UserID, ud.seatNo, i + 1)
			lgc.ExchangeBestCardPihu(robot.UserInfo.SeatNo)
		}
	}

	if ud.canHu(ud.moPai) {
		// mlog.Debug("交换牌后可以胡牌")
		lgc.getGameLogic().curCanOpUser[ud.seatNo] = true
		lgc.doHU(robot.UserInfo, ud.moPai)
		return
	}
	opValue := ud.calBestOutCard()

	mlog.Debug("UserID[%d], 手牌：%s, 摸牌：%d(%s), 出牌：%d(%s)", ud.User.UserID,
		ud.handCards.String(), ud.moPai, mahjong.CardString(ud.moPai), opValue, mahjong.CardString(opValue))
	lgc.doOut(robot.UserInfo, opValue)
}
func (lgc *SCMJRobotLogic) doDingque(ur *gamelogic.GameUser, opValue int32) {
	mlog.Debug("机器人操作定缺")
	mlog.Debug("【[Joecool] ===== robot, doDingque() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作定缺, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)], opValue[%d]】", ur.UserID, ur.SeatNo, opValue)

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_CHOOSEDINGQUE)
	btdata.OpValue = append(btdata.OpValue, opValue)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)
}

// 选倍数
func (lgc *SCMJRobotLogic) doRate(ur *gamelogic.GameUser, opValue int32) {
	mlog.Debug("机器人操作选倍数")
	mlog.Debug("【[Joecool] ===== robot, doRate() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作选倍数, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)], opValue = %d】", ur.UserID, ur.SeatNo, opValue)

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_CHOOSERATE)
	btdata.OpValue = append(btdata.OpValue, opValue)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)
}

//
func (lgc *SCMJRobotLogic) doHU(ur *gamelogic.GameUser, opValue int32) {
	mlog.Debug("机器人操作胡")
	mlog.Debug("【[Joecool] ===== robot, doHU() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作胡, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)], opValue = %d(%s)】", ur.UserID, ur.SeatNo, opValue, mahjong.CardString(opValue))

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_HU)
	btdata.OpValue = append(btdata.OpValue, opValue)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)
}
func (lgc *SCMJRobotLogic) doGang(ur *gamelogic.GameUser, opValue int32) {
	mlog.Debug("机器人操作杠")
	mlog.Debug("【[Joecool] ===== robot, doGang() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作杠, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)], opValue = %d(%s)】", ur.UserID, ur.SeatNo, opValue, mahjong.CardString(opValue))

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_GANG)
	btdata.OpValue = append(btdata.OpValue, opValue)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)
}
func (lgc *SCMJRobotLogic) doPeng(ur *gamelogic.GameUser, opValue int32) {
	mlog.Debug("机器人操作碰")
	mlog.Debug("【[Joecool] ===== robot, doPeng() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作碰, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)], opValue = %d(%s)】", ur.UserID, ur.SeatNo, opValue, mahjong.CardString(opValue))

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_PENG)
	btdata.OpValue = append(btdata.OpValue, opValue)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)
}
func (lgc *SCMJRobotLogic) doOut(ur *gamelogic.GameUser, opValue int32) {
	mlog.Debug("机器人操作出牌")
	mlog.Debug("【[Joecool] ===== robot, doOut() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作出牌, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)], opValue = %d(%s)】", ur.UserID, ur.SeatNo, opValue, mahjong.CardString(opValue))
	// [Joecool] for debug
	pUD := lgc.getGameLogic().getUserInfoBySeatNo(ur.SeatNo)
	if pUD != nil {
		mlog.Debug("[Joecool] User[%d(%d)], canOpEventInfo.OpInfo, len = %d", pUD.User.UserID, pUD.seatNo, len(pUD.canOpEventInfo.OpInfo))
		for _, pInfo := range pUD.canOpEventInfo.OpInfo {
			if pInfo != nil {
				mlog.Debug("[Joecool] OpInfo, %s, value[%v]", OpTypeString_debug(pInfo.nOpType), pInfo.nValue)
			}
		}
	}
	// [Joecool] for debug

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_OUTCARD)
	btdata.OpValue = append(btdata.OpValue, opValue)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)

	pUser := lgc.getGameLogic().getUserInfoBySeatNo(ur.SeatNo)
	if pUser != nil {
		pUser.bSJMJActionIDMsgDone = false
		mlog.Debug("[Joecool] doOut(), User[%d(%d)], bSJMJActionIDMsgDone[%t]", pUser.User.UserID, pUser.seatNo, pUser.bSJMJActionIDMsgDone)
	}
}
func (lgc *SCMJRobotLogic) doPass(ur *gamelogic.GameUser) {
	mlog.Debug("机器人操作Pass")
	mlog.Debug("【[Joecool] ===== robot, doPass() =====】")
	if ur == nil {
		mlog.Error("桌號[%d], 机器人操作Pass, 無機器人資料", lgc.GameLogic.TableNo())
		return
	}
	mlog.Debug("【[Joecool] User[%d(%d)]】", ur.UserID, ur.SeatNo)

	btmsg := new(msg.Message)
	btmsg.BClassID = int32(netproto.MessageBClassID_Game)
	btmsg.SClassID = int32(netproto.SCMJ_GameMessageClassID_SCNJActionId)

	btdata := new(netproto.SCMJ_Action)
	btdata.OpType = proto.Int32(OP_PASS)
	mlog.Debug("[Joecool] SCNJActionId, User[%d(%d)], %s, %v",
		ur.UserID, ur.SeatNo, OpTypeString_debug(*btdata.OpType), btdata.OpValue)

	btmsg.MsgData, _ = proto.Marshal(btdata)
	lgc.SendGameMessage(ur, btmsg)
}

//换牌胡 屁胡
func (lgc *SCMJRobotLogic) ExchangeBestCardPihu(SeatNo int32) {
	// config := lgc.Configs.GetConfig("gameserver")
	// robotRate := config.GetConfigInt("gameprivate", "robotechangrate")
	mlog.Debug("【[Joecool] ===== robot, ExchangeBestCardPihu() SeatNo[%d] =====】", SeatNo)

	ud := lgc.getGameLogic().getUserInfoBySeatNo(SeatNo)
	if ud == nil {
		return
	}

	map_iOpValue := make(map[int32]int, 0)
	pUser := lgc.getGameLogic().getUserInfoBySeatNo(lgc.getGameLogic().currOpSeatNo)
	if pUser != nil {
		mlog.Debug("[Joecool] User[%d(%d)], canOpEventInfo.OpInfo, len = %d", pUser.User.UserID, pUser.seatNo, len(pUser.canOpEventInfo.OpInfo))
		for _, pInfo := range pUser.canOpEventInfo.OpInfo {
			if pInfo != nil {
				mlog.Debug("[Joecool] (before)OpInfo, %s, value[%v]", OpTypeString_debug(pInfo.nOpType), pInfo.nValue)
				for _, i32Value := range pInfo.nValue {
					map_iOpValue[i32Value]++
				}
			}
		}
	}

	rob := lgc.getRobotCha(ud.User.UserID)
	mlog.Debug("【[Joecool] User[%d(%d)] getRobotCha[%d] = %v】", ud.User.UserID, ud.seatNo, ud.User.UserID, rob)
	if rob == nil {
		return
	}
	nRand := util.RndInt(0, 100)
	if nRand > rob.changeRate {
		mlog.Debug("【[Joecool] User[%d], SeatNo[%d], robotechangrate = %d, 亂數值 = %d, robotechangtime = %d, 不換牌】",
			ud.User.UserID, ud.seatNo, rob.changeRate, nRand, rob.changeTimes)
		lgc.getGameLogic().GameLog.AddGameStep("User[%d], SeatNo[%d], robotechangrate = %d, 亂數值 = %d, robotechangtime = %d, 不換牌",
			ud.User.UserID, ud.seatNo, rob.changeRate, nRand, rob.changeTimes)
		return
	}
	mlog.Debug("【[Joecool] User[%d], SeatNo[%d], robotechangrate = %d, 亂數值 = %d, robotechangtime = %d, 換牌】",
		ud.User.UserID, ud.seatNo, rob.changeRate, nRand, rob.changeTimes)
	lgc.getGameLogic().GameLog.AddGameStep("User[%d], SeatNo[%d], robotechangrate = %d, 亂數值 = %d, robotechangtime = %d, 換牌",
		ud.User.UserID, ud.seatNo, rob.changeRate, nRand, rob.changeTimes)
	// mlog.Debug("****************************************************")
	// defer func() {
	// 	mlog.Debug("手牌数量[%d],牌池数量[%d]", ud.handCards.Len(), lgc.getGameLogic().randGroup.GetCardGroup().Len())
	// 	mlog.Debug("****************************************************")
	// }()
	// mlog.Debug("换牌前的牌[%s]", ud.handCards.String())
	// mlog.Debug("换牌前的牌数量[%d]", ud.handCards.Len())
	tmp := new(mahjong.CardGroup)
	for _, cd := range ud.handCards.Cards {
		tmp.AddCard(cd)
	}
	//获取用户手牌
	mapTmp := make(map[int32]int32, 0)
	for _, cd := range tmp.Cards {
		if cd/100 == ud.color {
			continue
		}

		// 接著要操作用的牌避掉
		if map_iOpValue[cd] > 0 {
			continue
		}

		mapTmp[cd/10]++
	}
	//出去一坎的
	for i := int32(10); i < 40; i++ {
		if i/10 == ud.color {
			continue
		}
		mapTmp = lgc.checkShunzi(mapTmp, i)
	}
	sum := int32(0)
	for _, count := range mapTmp {
		// mlog.Debug("count[%d]", count)
		sum += count
	}

	//获取牌池
	mapTmpCg := make(map[int32]int32, 0)
	cg := lgc.getGameLogic().randGroup.GetCardGroup()
	// mlog.Debug("换牌前的牌池数量[%d]", cg.Len())
	for _, cdcg := range cg.Cards {
		if cdcg/100 == ud.color {
			continue
		}
		mapTmpCg[cdcg/10]++
	}
	for _, cdcg := range ud.handCards.Cards {
		if cdcg/100 == ud.color {
			sum += 1
		}
	}
	//大于等于4换三张,小于4换胡
	mlog.Debug("【[Joecool] User[%d], 換牌前：手牌[%s], 摸牌[%s]】",
		ud.User.UserID, ud.handCards.String(), mahjong.CardString(ud.moPai))
	lgc.getGameLogic().GameLog.AddGameStep("User[%d], 換牌前：手牌[%s], 摸牌[%s]",
		ud.User.UserID, ud.handCards.String(), mahjong.CardString(ud.moPai))
	if sum >= 4 {
		//mlog.Debug("【[Joecool] User[%d(%d)] sum = %d, 大于等于4换三张】", ud.User.UserID, ud.seatNo, sum)
		//在牌池中找一坎
		result := lgc.findThreeCard(mapTmpCg)
		//	ud.User.UserID, ud.seatNo, len(result), result)
		if len(result) < 3 {
			//如果没有一坎 找对子
			result = lgc.findTwoCard(mapTmpCg)
		}
		if len(result) <= 0 {
			mlog.Debug("牌库无对应牌,换牌失败")
			return
		}

		cgNewResult := lgc.list2cardGroup(cg, result)

		siceOld := make([]int32, 0)
		for _, hd := range ud.handCards.Cards {
			if hd/100 == ud.color {
				siceOld = append(siceOld, hd/10)
			}
		}
		for cdOld, count := range mapTmp {
			if count == 1 {
				siceOld = append(siceOld, cdOld)
			} else if count == 2 {
				siceOld = append(siceOld, cdOld)
				siceOld = append(siceOld, cdOld)
			}
		}
		size := len(result)
		if len(siceOld) > size {
			siceOld = siceOld[:size]
		}
		cgOld := lgc.list2cardGroup(ud.handCards, siceOld)
		if cgNewResult.Len() != cgOld.Len() {
			mlog.Debug("交换牌数量不等1,换牌失败,NewSize[%d],OldSize[%d]", size, len(siceOld))
			mlog.Debug("交换牌数量不等1,换牌失败,cgNewResult[%d],cgOld[%d]", cgNewResult.Len(), cgOld.Len())
			return
		}

		cg.AddCardGroup(cgOld)
		cg.DeleteGroup(cgNewResult)
		//重新设置牌库
		if lgc.getGameLogic().randGroup.SetCardGroup(cg) {
			//交换牌
			ud.handCards.DeleteGroup(cgOld)
			ud.handCards.AddCardGroup(cgNewResult)
		} else {
			mlog.Debug("交换牌数量不等2,换牌失败")
			return
		}

	} else {
		//mlog.Debug("【[Joecool] User[%d(%d)] sum = %d, 小于4换胡】", ud.User.UserID, ud.seatNo, sum)
		huRand := util.RndInt(0, 100)
		if huRand > rob.ziMoRate {
			lgc.getGameLogic().randGroup.Shuffle()
			return
		}
		// 查找有用的牌siceHuCard
		siceHuCard := make([]int32, 0)
		mapTmpCg := make(map[int32]int32, 0)
		for _, cdcg := range cg.Cards {
			if cdcg/100 == ud.color {
				continue
			}
			mapTmpCg[cdcg/10]++
		}
		for mapcd, count := range mapTmpCg {
			if count >= 2 {
				siceHuCard = append(siceHuCard, mapcd)
				break
			}
		}
		if len(siceHuCard) == 1 && ud.moPai > 0 {
			// 获取没有用的牌siceOld
			siceOld := make([]int32, 0)
			for cdOld, count := range mapTmp {
				if count == 1 {
					siceOld = append(siceOld, cdOld)
					break
				}
			}
			// 转化为手牌
			cgOld := lgc.list2cardGroup(ud.handCards, siceOld)
			if cgOld.Len() != 1 {
				mlog.Debug("没有找到无用手牌 手牌[%s]，摸牌[%d]", ud.handCards.String(), ud.moPai)
				return
			}
			cgNew := lgc.list2cardGroup(cg, siceHuCard)
			if cgNew.Len() != 1 {
				mlog.Debug("没有找到有用牌池")
				return
			}
			cg.AddCardGroup(cgOld)
			cg.AddCard(ud.moPai)
			cg.DeleteGroup(cgNew)
			cg.DeleteGroup(cgNew)
			if lgc.getGameLogic().randGroup.SetCardGroup(cg) {
				//交换牌
				ud.handCards.DeleteGroup(cgOld)
				ud.handCards.AddCardGroup(cgNew)
				ud.moPai = cgNew.Cards[0]
			} else {
				mlog.Debug("交换牌数量不等3,换牌失败")
				return
			}
		}
	}
	lgc.getGameLogic().randGroup.Shuffle()
	mlog.Debug("【[Joecool] User[%d], 換牌後：手牌[%s], 摸牌[%s]】",
		ud.User.UserID, ud.handCards.String(), mahjong.CardString(ud.moPai))
	lgc.getGameLogic().GameLog.AddGameStep("User[%d], 換牌後：手牌[%s], 摸牌[%s]",
		ud.User.UserID, ud.handCards.String(), mahjong.CardString(ud.moPai))
	// mlog.Debug("换牌后的牌[%s]", ud.handCards.String())
	// mlog.Debug("换牌成功 屁胡")
	// [Joecool] for debug
	for _, pInfo := range ud.canOpEventInfo.OpInfo {
		if pInfo != nil {
			mlog.Debug("[Joecool] (after)OpInfo, %s, value[%v]", OpTypeString_debug(pInfo.nOpType), pInfo.nValue)
		}
	}
	// [Joecool] for debug
	return
}

//检测顺子并剔除
func (lgc *SCMJRobotLogic) checkShunzi(cd map[int32]int32, i int32) map[int32]int32 {
	if cd[i] >= 3 {
		cd[i] -= 3
	}
	if cd[i] > 0 && cd[i+1] > 0 && cd[i+2] > 0 {
		cd[i] -= 1
		cd[i+1] -= 1
		cd[i+2] -= 1
		cd = lgc.checkShunzi(cd, i)
		return cd
	}
	return cd
}

func (lgc *SCMJRobotLogic) findThreeCard(cd map[int32]int32) []int32 {
	r := make([]int32, 0)
	// 随机一个数换牌
	nRand := util.RndInt(10, 35)
	for i := int32(nRand); i < 40; i++ {
		if cd[i] >= 3 {
			r = append(r, i)
			r = append(r, i)
			r = append(r, i)
			return r
		}
		if cd[i] > 0 && cd[i+1] > 0 && cd[i+2] > 0 {
			r = append(r, i)
			r = append(r, i+1)
			r = append(r, i+2)
			return r
		}
	}
	// 如果随机没有找到 重新找一次
	for i := int32(10); i < 40; i++ {
		if cd[i] >= 3 {
			r = append(r, i)
			r = append(r, i)
			r = append(r, i)
			return r
		}
		if cd[i] > 0 && cd[i+1] > 0 && cd[i+2] > 0 {
			r = append(r, i)
			r = append(r, i+1)
			r = append(r, i+2)
			return r
		}
	}
	return r
}

func (lgc *SCMJRobotLogic) findTwoCard(mapCd map[int32]int32) []int32 {
	r := make([]int32, 0)
	for cd, count := range mapCd {
		if count >= 2 {
			r = append(r, cd)
			r = append(r, cd)
			break
		}
	}
	return r
}

//转化list为牌组
func (lgc *SCMJRobotLogic) list2cardGroup(cg *mahjong.CardGroup, cdlist []int32) *mahjong.CardGroup {
	if cg == nil {
		mlog.Error("桌號：%d, 無牌組資料", lgc.Table.TableNo)
		return nil
	}

	cg2 := new(mahjong.CardGroup)
	cgcal := new(mahjong.CardGroup)
	for _, cgcd := range cg.Cards {
		cgcal.Cards = append(cgcal.Cards, cgcd)
	}
	for _, cd := range cdlist {
		for _, cgcd := range cgcal.Cards {
			if cd == cgcd/10 {
				cg2.AddCard(cgcd)
				cgcal.DeleteCard(cgcd, 1)
				break
			}
		}
	}
	if len(cdlist) != cg2.Len() {
		mlog.Debug("转化list出错")
	}
	return cg2
}

// 寻找一个和cd相同的牌
func (lgc *SCMJRobotLogic) findSameCard(cg *mahjong.CardGroup, cd int32) bool {
	for _, cgcd := range cg.Cards {
		if cgcd == cd {
			return true
		}
	}
	return false
}
