package qz21gamelogic

import (
	"gamelogic"
	"mlog"
	"msg"
	"netproto"
	"poker"

	"example.invalid/golang/protobuf/proto"
	"math"
)

const (
	qz21ActionBegin       = "kaishi"
	qz21ActionFaPai       = "fapai"
	qz21ActionQiangZhuang = "qiangzhuang"
	qz21ActionQzAnimotion = "qzanimotion"
	qz21ActionXJBet       = "xjbet"
	qz21ActionZJBet       = "zjbet"
	qz21ActionXJComplete  = "xjcomplete"
	qz21ActionZJComplete  = "zjcomplete"
	qz21ActionShow        = "bipai"
	qz21ActionSettlement  = "jiesuan"
)

func (d *Qz21Table) regist() {
	//可以播放开始动画 发牌等
	d.RegistCallBackFnc(QZ21StepBegin, d.gameBeginTimeOut)
	//抢庄阶段
	d.RegistCallBackFnc(QZ21StepQiangZhuang, d.qz21QiangZhuangTimeOut)
	//播放抢庄动画阶段
	d.RegistCallBackFnc(QZ21StepQzAnimotion, d.qz21QiangZhuangAnimotionTimeOut)
	//闲家选择倍率阶段
	d.RegistCallBackFnc(QZ21StepXJBet, d.qz21XJBetTimeOut)
	//闲家叫牌阶段
	d.RegistCallBackFnc(QZ21StepXJComplete, d.qz21XJCompleteTimeOut)
	//庄家叫牌阶段
	d.RegistCallBackFnc(QZ21StepZJComplete, d.qz21ZJCompleteTimeOut)
	//比牌阶段
	d.RegistCallBackFnc(QZ21StepShow, d.qz21ShowTimeOut)
	//结算阶段
	d.RegistCallBackFnc(QZ21StepSettlement, d.qz21SettlementTimeOut)
	//end
	d.RegistCallBackFnc(QZ21StepEnd, d.qz21EndTimeOut)
}

func (d *Qz21Table) qz21EndTimeOut() {
	mlog.Debug("游戏结束阶段")
	d.gameover = true
	d.GameEndFunc()
}

//创建牌型消息
func (d *Qz21Table) getPokerMsg(upk *poker.UserPoker) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_UserPokerID)
	pmsg.MsgData = upk.GetNetMessageData()
	return pmsg
}

//sendtype 1表示闲家发牌 2 表示庄家发牌
func (d *Qz21Table) faPai(shuffle bool, count int, sendType int) {
	if sendType == 1 {
		for _, ur := range d.AllGameUser() {
			if ur.SeatNo == d.bankerSeat || !d.IsGameUser(ur) {
				continue
			}
			cards := d.poker.Deal(ur.UserID, count)
			mlog.Debug("闲家发牌 %v", cards)
			upk := poker.NewUserPoker(ur, cards)
			d.gamePoker.AddUserPoker(upk)
			d.GameLog.Fapai(ur.UserID, cards.String())
			//通知用户牌型
			d.BoardcastMessage(d.getPokerMsg(upk))
		}
	} else {
		for _, ur := range d.AllGameUser() {
			if ur.SeatNo == d.bankerSeat {
				cards := d.poker.Deal(ur.UserID, count)
				upk := poker.NewUserPoker(ur, cards)
				d.gamePoker.AddUserPoker(upk)
				d.GameLog.Fapai(ur.UserID, cards.String())
				mlog.Debug("庄家发牌 %v", cards)
				//通知用户牌型
				d.BoardcastMessage(d.getPokerMsg(upk))
				break
			}
		}
	}
}

func (d *Qz21Table) zupai() {
	d.nextDo()
	for i := 0; i < len(d.gamePoker.UserPokers); i++ {
		upk := d.gamePoker.UserPokers[i]
		if d.getDian(upk.Cards) == 21 {
			r := d.getRoleInfoBySeat(upk.User.SeatNo)
			if r != nil {
				r.isDo = true
			}

			r.status = 2
			mlog.Debug("玩家%v已经是21点", upk.User.UserID)
			d.broadcastUserStatus(upk.User.SeatNo, upk.User.UserID, 2)
		}
	}

	d.getBanker().isDo = true
	if d.checkAllRoleDo() {
		mlog.Debug("所有都完成")
		d.timeOutCallBack()
	}
}

func (d *Qz21Table) nextDo() {
	d.actionover = true
	d.ResumeLoop()
}

//游戏开始阶段
func (d *Qz21Table) gameBeginTimeOut() {
	d.broadcastOdds(2)
	mlog.Debug("游戏开始发送抢庄倍率超时")
	d.stopLoop()
	d.actionover = false
	d.animotionCur = d.RegisterAnimation(qz21ActionBegin, 3, d.nextDo)
}

//抢庄阶段
func (d *Qz21Table) qz21QiangZhuangTimeOut() {
	mlog.Debug("进入抢庄逻辑超时")
	for _, v := range d.roles {
		if !v.isDo {
			v.qzodds = d.qzCfg.qzOdds[0]
			d.broadcastUserChoseOdds(v.seat, v.roleid, v.qzodds, 2)
			v.isDo = true
		}
	}
	var tmp []*Qz21Role
	tmp, d.bankerSeat = d.getBankerSeat()
	v := d.getBanker()
	if v != nil {
		v.iszj = true
	}

	d.broadcastBankerInfo(tmp)
	d.stopLoop()
	d.animotionCur = d.RegisterAnimation(qz21ActionQiangZhuang, 3, d.ResumeLoop)
}

//播放抢庄动画阶段
func (d *Qz21Table) qz21QiangZhuangAnimotionTimeOut() {
	mlog.Debug("播放抢庄动画超时!")
	d.broadcastOdds(1)
	//d.stopLoop()
	//d.animotionCur = d.RegisterAnimation(qz21ActionQzAnimotion, animotionTimeOut, d.ResumeLoop)
}

//闲家选择倍率阶段
func (d *Qz21Table) qz21XJBetTimeOut() {
	mlog.Debug("闲家选择倍率超时")
	for _, v := range d.roles {
		if !v.isDo && (!v.iszj) {
			v.xjodds = d.qzCfg.xjBetOdds[0]
			d.broadcastUserChoseOdds(v.seat, v.roleid, v.xjodds, 1)
			v.isDo = true
		}
	}
	d.faPai(true, 2, 1)
	d.stopLoop()
	d.actionover = false
	d.animotionCur = d.RegisterAnimation(qz21ActionFaPai, 2, d.zupai)
}

//闲家叫牌阶段
func (d *Qz21Table) qz21XJCompleteTimeOut() {
	mlog.Debug("闲家叫牌阶段超时")
	for _, v := range d.roles {
		if !v.isDo && (!v.iszj) {
			v.xjodds = d.qzCfg.xjBetOdds[0]
			d.broadcastUserStatus(v.seat, v.roleid, 2)
			v.isDo = true
		}
	}
}

//庄家叫牌阶段
func (d *Qz21Table) qz21ZJCompleteTimeOut() {
	mlog.Debug("庄家发牌叫牌阶段超时")
	d.faPai(false, 1, 2)
	d.sendCardToBanker()
	d.stopLoop()
	d.animotionCur = d.RegisterAnimation(qz21ActionFaPai, animotionTimeOut, d.ResumeLoop)
}

//比牌阶段
func (d *Qz21Table) qz21ShowTimeOut() {
	mlog.Debug("比牌阶段超时!")
	for _, v := range d.roles {
		d.caculatePoint(v.seat)
		d.compareWithBanker(v.seat)
	}

	uscoresRet := make(map[int32]int64)
	for _, v := range d.roles {
		a, at := d.GetUserWinAndTax(v.money)
		d.UpdateUserScoreFunc(v.roleid, a, at)
		v.money = a
		uscoresRet[v.roleid] = a
	}
	d.GameLog.GameEnd(uscoresRet)
	d.broadcastUserShow()
	d.stopLoop()
	d.animotionCur = d.RegisterAnimation(qz21ActionShow, d.showtimeout, d.ResumeLoop)
}

//结算阶段
func (d *Qz21Table) qz21SettlementTimeOut() {
	mlog.Debug("结算阶段超时!")
	d.broadcastSettlement()
	d.stopLoop()
	d.actionover = false
	d.animotionCur = d.RegisterAnimation(qz21ActionSettlement, animotionTimeOut, d.nextDo)
}

//倍率选择请求
func (d *Qz21Table) OnChoseOdds(user *gamelogic.GameUser, data []byte) {
	ust := &netproto.ChoseOdds{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析玩家出牌消息错误")
		d.KickUserFunc(user.UserID, "非法操作")
		return
	}
	*ust.Odds = int32(math.Abs(float64(*ust.Odds)))
	*ust.Ot = int32(math.Abs(float64(*ust.Ot)))

	u := d.getRoleInfoBySeat(user.SeatNo)
	if u == nil {
		mlog.Error("获取玩家信息失败，座位号(%d)错误", user.SeatNo)
		d.KickUserFunc(user.UserID, "非法操作")
		return
	}

	if ust.GetOt() == 1 {
		//不是闲家选择倍率阶段
		if QZ21StepXJBet != d.step {
			return
		}
		//已经选过了
		if u.xjodds != -1 {
			return
		}

		u.xjodds = d.qzCfg.getXJOddsByIndex(ust.GetOdds())
		d.responseChoseOdds(ust.GetOt(), user, u.xjodds)
		d.broadcastUserChoseOdds(u.seat, u.roleid, u.xjodds, 1)
		d.getBanker().isDo = true
		d.GameLog.AddGameStep("闲家%d选择倍率%d", user.UserID, u.xjodds)
	} else {
		//不是抢庄阶段
		if QZ21StepQiangZhuang != d.step {
			return
		}
		//已经抢过了
		if u.qzodds != -1 {
			return
		}
		u.qzodds = d.qzCfg.getZJOddsByIndex(ust.GetOdds())
		d.broadcastUserChoseOdds(u.seat, u.roleid, u.qzodds, 2)
		d.responseChoseOdds(ust.GetOt(), user, u.qzodds)
		d.GameLog.AddGameStep("用户%d选择抢庄倍率%d", user.UserID, u.qzodds)
	}
	u.isDo = true
	if d.checkAllRoleDo() {
		d.timeOutCallBack()
	}
}

func (s *Qz21Table) stopLoop() {
	s.gameover = true
	s.stepTimer.Stop()
	s.stepTimer = nil
}

func (s *Qz21Table) restartLoop() {
	s.gameover = false
}

func (s *Qz21Table) ResumeLoop() {
	s.restartLoop()
	s.logicEngine()
}

//玩家操作请求
func (d *Qz21Table) OnRoleOpt(user *gamelogic.GameUser, data []byte) {
	u := d.getRoleInfoBySeat(user.SeatNo)
	ust := &netproto.RoleOpt{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析玩家出牌消息错误")
		d.KickUserFunc(user.UserID, "非法操作")
		return
	}
	*ust.Ot = int32(math.Abs(float64(*ust.Ot)))

	if ust.GetOt() == 1 {
		if u.isBaoPai {
			d.responseRoleOpt(1, user, 1, "玩家已经爆牌，不能继续操作")
			return
		}

		if _, number := d.checkBaoPai(user.SeatNo); number == 21 {
			d.responseRoleOpt(1, user, 2, "玩家已经是21点")
			return
		}
		if u.isDo && u.status == 2 {
			d.responseRoleOpt(1, user, 2, "玩家不能叫牌")
			return
		}

		d.responseRoleOpt(1, user, 0, "OK")
		d.getBanker().isDo = true
		d.GameLog.AddGameStep("闲家%d叫牌", user.UserID)
		mlog.Debug("闲家%d叫牌", user.UserID)
		d.sendCardToUser(user.SeatNo, 1)
		if ok, number := d.checkBaoPai(user.SeatNo); ok || number == 21 {
			u.isDo = true
			u.status = 2
			d.broadcastUserStatus(user.SeatNo, user.UserID, 2)
			if d.checkAllRoleDo() {
				d.timeOutCallBack()
			}
			if ok {
				mlog.Error("玩家牌点爆牌")
			}
			return
		}
		d.broadcastUserStatus(user.SeatNo, user.UserID, 1)
	} else {
		if u != nil {
			if u.status == 2 {
				d.responseRoleOpt(2, user, 1, "玩家已经做过完成操作")
				return
			}
			d.GameLog.AddGameStep("庄家%d叫牌", user.UserID)
			d.checkBaoPai(user.SeatNo)
			u.isDo = true
			u.status = 2
			d.getBanker().isDo = true
			d.responseRoleOpt(2, user, 0, "OK")
			d.broadcastUserStatus(user.SeatNo, user.UserID, 2)
			if d.checkAllRoleDo() {
				d.timeOutCallBack()
			}
		} else {
			mlog.Error("无法找到玩家对应的游戏信息")
		}
	}
}

//断线重连
func (d *Qz21Table) UserRecover(gameuser *gamelogic.GameUser) {
	d.DynamicGameLogicBase.UserRecover(gameuser)
	d.pushGameInfo(gameuser.UserID)
}

func (d *Qz21Table) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	d.DynamicGameLogicBase.UserEnter(gameuser, tableno, seatno)
	d.sendDeskCfg(gameuser)
	if d.IsGaming() && !d.IsGameUser(gameuser) {
		d.pushGameInfo(gameuser.UserID)
	}
}

func (d *Qz21Table) OnWaitOpt(user *gamelogic.GameUser, data []byte) {
	ust := &netproto.UserActionCommited{}
	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析玩家出牌消息错误")
		d.KickUserFunc(user.UserID, "非法操作")
		return
	}
	d.PlayerActionManager.OnUserAction(ust.GetActionName(), user.UserID)
}

func (d *Qz21Table) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}
	//选择倍率
	if gmsg.SClassID == int32(netproto.QZ21_GameMessageClassID_QZ21ChoseOddsId) {
		d.OnChoseOdds(user, gmsg.MsgData.([]byte))
	}
	//玩家操作请求
	if gmsg.SClassID == int32(netproto.QZ21_GameMessageClassID_QZ21RoleOptId) {
		d.OnRoleOpt(user, gmsg.MsgData.([]byte))
	}
	//动画等待
	d.CheckUserAnimationCommited(user, gmsg, d.animotionCur, nil)
}

func (d *Qz21Table) ExchangeMaxCard(userid int32) {
	d.GameLogicBase.ExchangeMaxCard(userid)
	var maxRetUserID int32

	var maxcg *poker.CardGroup
	maxdian := int32(0)
	fpoker := d.poker.(*poker.FixedPoker)
	for _, ur := range d.AllTableUser() {
		if ur.IsRobot() && ur.UserID != userid {
			continue
		}

		cg := fpoker.GetUserCards(ur.UserID)
		mlog.Debug("cg=%v", cg)
		if maxcg == nil {
			maxcg = cg
			maxRetUserID = ur.UserID
			cards := poker.NewEmptyCardGroup(0)
			for i := 0; i < cg.Len(); i++ {
				cards.AddCard(cg.Cards[i])
				tmp := d.getDian(cards)
				if tmp > 21 {
					break
				}

				maxdian = tmp
				//d.getDian(upk)
			}
			continue
		}

		dian := int32(0)
		cards := poker.NewEmptyCardGroup(0)
		for i := 0; i < cg.Len(); i++ {
			cards.AddCard(cg.Cards[i])
			tmp := d.getDian(cards)
			if tmp > 21 {
				break
			}

			dian = tmp
		}

		if maxdian < dian {
			maxcg = cg
			maxRetUserID = ur.UserID
		}
	}

	mlog.Debug("maxcg=%v", maxcg.Cards)
	mlog.Debug("交换用户%d和用户%d牌型", userid, maxRetUserID)
	d.GameLog.AddGameStep("交换用户%d和用户%d牌型", userid, maxRetUserID)
	fpoker.ExChange(userid, maxRetUserID)
}
