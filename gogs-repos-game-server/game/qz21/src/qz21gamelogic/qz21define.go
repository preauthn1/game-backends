package qz21gamelogic

import (
	"config"
	"math/rand"
	"time"

	"event"
	"gamelogic"
	"mlog"
	"msg"
	"netproto"
	"poker"

	"example.invalid/golang/protobuf/proto"
)

type QZ21GameStep int32

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

type Qz21Role struct {
	roleid    int32
	seat      int32
	iszj      bool
	isDo      bool
	isTuoGuan bool
	qzodds    int32
	xjodds    int32
	point     int32
	money     int64
	status    int32
	isBaoPai  bool
	isNew     bool
}

type callBack func()

type Qz21Table struct {
	*gamelogic.DynamicGameLogicBase
	qzCfg        *qz21Config
	key          bool
	turn         int32
	zjseat       int32
	zjodds       int32
	step         QZ21GameStep
	gameover     bool
	maxroles     int32
	difen        int32
	timeOut      int32
	roles        []*Qz21Role
	bankerSeat   int32
	handlers     []callBack
	poker        poker.Poker
	gamePoker    *poker.GamePoker
	stepTimer    *event.TimerFunc
	animotionCur string
	actionover   bool
	showtimeout  int32 //比牌动画超时
}

func NewQz21GameLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Qz21Table {
	glc := new(Qz21Table)
	glc.DynamicGameLogicBase = gamelogic.NewDynamicGameLogicBase(t, tfManager, cfgs)
	//glc.poker = poker.NewRandomPoker(false, false, 1)
	//随机发牌
	scfg := cfgs.GetConfig("gameserver")
	glc.gamePoker = poker.NewGamePoker() //创建游戏牌型
	glc.qzCfg = NewConfig(cfgs.GetConfig("qz21"))
	glc.difen = int32(scfg.GetConfigInt("gameserver", "basescore"))
	glc.timeOut = int32(scfg.GetConfigInt("gameserver", "readywaittime"))
	glc.showtimeout = int32(scfg.GetConfigInt("gameserver", "showtimeout"))
	if glc.qzCfg == nil {
		mlog.Error("init qz21 configs faild,start game faild!")
		return nil
	}
	glc.initStepTimeOut(cfgs.GetConfig("qz21"))
	glc.key = false
	glc.RegisterAfterGameStartFunc(func() {
		glc.gameover = false
		glc.init(t)
		glc.gameover = false
		glc.regist()
		glc.logicEngine()
	})

	glc.RegisterBeforeGameStartFunc(func() {
		glc.fenpai()
	})

	glc.RegisterAfterGameEndFunc(func() {
		glc.clearData()
	})
	return glc
}

func (t *Qz21Table) fenpai() {
	if t.GetServerConfig().PokerType == gamelogic.PokerType_Random {
		t.poker = poker.NewRandomPoker(false, false, 1)
		t.poker = poker.NewFixedPoker(t.AllTableUser(), t.poker.(*poker.RandomPoker), 10)
	} else { //发牌器发牌
		t.poker = poker.NewFilePoker(t.Configs, t.AllTableUser())
		t.poker = poker.NewFixedFilePoker(t.poker.(*poker.FilePoker))
	}

	t.gamePoker = poker.NewGamePoker() //创建游戏牌型
}

func (t *Qz21Table) timeOutCallBack() {
	if t.gameover {
		return
	}
	if t.handlers[t.step%QZ21Max] != nil {
		t.handlers[t.step%QZ21Max]()
	}
	t.moveStep()
	t.logicEngine()
}

func (t *Qz21Table) RegistCallBackFnc(step QZ21GameStep, f callBack) {
	t.handlers[step%QZ21Max] = f
}

func (t *Qz21Table) moveStep() {
	t.step = (t.step + 1) % QZ21Max
	mlog.Debug("moveStep:%v", t.step)
}

func (t *Qz21Table) logicEngine() {
	if t.gameover {
		return
	}
	if t.stepTimer != nil {
		t.stepTimer.Stop()
	}
	t.resetRoleDo()
	timeOut := t.getStepTimeOut()
	if t.step == QZ21StepXJComplete {
		timeOut = t.getStepTimeOut() + 3
	}
	t.broadcastOptTime()
	t.stepTimer = t.TimerFuncManager.NewNamedTimer(timeOut, t.timeOutCallBack, "")
}

//广播抢庄倍率信息
func (t *Qz21Table) broadcastOdds(oce int32) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifyOddsId)
	gsdata := new(netproto.QZ21NotifyOdds)
	gsdata.Odds = make([]int32, 0)
	if oce == 2 {
		l := len(t.qzCfg.qzOdds)
		for i := 0; i < l; i++ {
			gsdata.Odds = append(gsdata.Odds, t.qzCfg.qzOdds[i])
		}
	} else {
		l := len(t.qzCfg.xjBetOdds)
		for i := 0; i < l; i++ {
			gsdata.Odds = append(gsdata.Odds, t.qzCfg.xjBetOdds[i])
		}
	}
	gsdata.Oce = &oce
	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
}

//确定庄家
func (t *Qz21Table) getBankerSeat() ([]*Qz21Role, int32) {
	var seat int32 = -1
	var odds int32 = 0
	tmp := make([]*Qz21Role, 0)
	for _, v := range t.roles {
		if v.qzodds > odds {
			odds = v.qzodds
			seat = v.seat
		} else if v.qzodds == odds {
			if rand.Int31n(2) == 1 {
				seat = v.seat
			}
		}

		if seat == -1 {
			seat = v.seat
		}
	}
	for _, v := range t.roles {
		if v.qzodds >= 0 {
			tmp = append(tmp, v)
		}
	}
	return tmp, seat
}

//广播游戏操作超时信息
func (t *Qz21Table) broadcastOptTime() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifyTimeOutOptId)
	gsdata := new(netproto.QZ21NotifyTimeOutOpt)
	gsdata.Step = proto.Int32(int32(t.step))
	gsdata.TimeOut = proto.Int32(t.getStepTimeOut())
	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
	mlog.Debug("广播游戏阶段：%v", t.step)
}

//广播玩家选择的倍率信息
//ot 1 闲家选择倍率 2 庄家选择倍率
func (t *Qz21Table) broadcastUserChoseOdds(seat int32, userId int32, odds int32, ot int32) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifyChoseOddsId)
	gsdata := new(netproto.QZ21NotifyChoseOdds)
	gsdata.Odds = proto.Int32(odds)
	gsdata.Seat = proto.Int32(seat)
	gsdata.Ot = proto.Int32(ot)
	gsdata.UserId = proto.Int32(userId)
	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
}

//广播玩家状态信息
func (t *Qz21Table) broadcastUserStatus(seat int32, userId int32, status int32) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifyRoleStatusId)
	gsdata := new(netproto.QZ21NotifyRoleStatus)
	gsdata.Roe = &status
	gsdata.Seat = proto.Int32(seat)
	gsdata.UserId = proto.Int32(userId)
	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
}

//广播玩家亮牌信息
func (t *Qz21Table) broadcastUserShow() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifyShowId)
	gsdata := new(netproto.QZ21NotifyShowArr)
	for _, u := range t.roles {
		user := t.GetUserBySeatNo(u.seat)
		userId := user.UserID
		data := new(netproto.QZ21NotifyShow)
		data.Seat = proto.Int32(u.seat)
		data.UserId = proto.Int32(userId)

		data.Js = make([]*netproto.Qz21Js, 0)
		for _, v := range t.roles {
			s := new(netproto.Qz21Js)
			s.UserId = proto.Int32(v.roleid)
			s.Money = proto.Int64(v.money)
			data.Js = append(data.Js, s)
		}
		data.Cards = make([]*netproto.Qz21Card, 0)
		upk := t.gamePoker.GetUserPoker(userId)
		if upk != nil {
			for _, v := range upk.Cards.Cards {
				c := new(netproto.Qz21Card)
				c.Color = &v.Color
				c.Number = &v.CardNumber
				data.Cards = append(data.Cards, c)
			}
		}

		data.Point = proto.Int32(u.point)
		gsdata.ShowArr = append(gsdata.ShowArr, data)
	}

	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
}

//广播庄家信息
func (t *Qz21Table) broadcastBankerInfo(tmp []*Qz21Role) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifyBankerId)
	gsdata := new(netproto.QZ21NotifyBanker)
	u := t.getRoleInfoBySeat(t.bankerSeat)
	gsdata.UserId = proto.Int32(u.roleid)
	gsdata.Seat = proto.Int32(t.bankerSeat)
	gsdata.Odds = proto.Int32(u.qzodds)
	gsdata.Rbp = new(netproto.RobBankerPlayer)
	for _, v := range tmp {
		item := new(netproto.RobItem)
		item.Odds = proto.Int32(v.qzodds)
		item.UserId = proto.Int32(v.roleid)
		gsdata.Rbp.Rbs = append(gsdata.Rbp.Rbs, item)
	}
	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
}

//广播结算信息
func (t *Qz21Table) broadcastSettlement() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21NotifySettlementId)
	gsdata := new(netproto.QZ21NotifySettlement)
	gsdata.Sli = make([]*netproto.SettlementItem, 0)
	gsdata.BankerId = proto.Int32(t.getBanker().roleid)
	for _, v := range t.roles {
		u := t.GetUserByUserID(v.roleid)
		if u != nil {
			item := new(netproto.SettlementItem)
			item.Id = proto.Int32(v.roleid)
			item.Money = proto.Int64(v.money)
			item.NickName = proto.String(u.NickName)
			item.Point = proto.Int32(v.point)
			item.Seat = proto.Int32(v.seat)
			gsdata.Sli = append(gsdata.Sli, item)
			//mlog.Debug("name:%s,id:%d,point:%d,money:%d,seat:%d", u.NickName, u.UserID, v.point, v.money, v.seat)
		}
	}

	pmsg.MsgData = gsdata
	t.BoardcastMessage(pmsg)
}

//广播游戏信息 断线重连
func (t *Qz21Table) pushGameInfo(userId int32) {
	user := t.GetUserByUserID(userId)
	t.sendDeskCfg(user)

	if !t.IsGaming() {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21GameInfoPushId)
	gsdata := new(netproto.QZ21GameInfoPush)
	gsdata.BankerSeat = proto.Int32(t.bankerSeat)
	if t.getBanker() != nil {
		gsdata.BankerId = proto.Int32(t.getBanker().roleid)
	} else {
		gsdata.BankerId = proto.Int32(0)
	}

	if !t.actionover {
		gsdata.Step = proto.Int32(int32(t.step - 1))
	} else {
		gsdata.Step = proto.Int32(int32(t.step))
	}

	if t.stepTimer != nil {
		gsdata.TimeOut = proto.Int32(t.stepTimer.WaitSeconds - t.stepTimer.ElapsedSeconds() - 2)
		gsdata.TotalTimeOut = proto.Int32(t.stepTimer.WaitSeconds)
	} else {
		gsdata.TimeOut = proto.Int32(0)
		gsdata.TotalTimeOut = proto.Int32(0)
	}
	gsdata.ZjOdds = make([]int32, 0)
	gsdata.ZjOdds = append(gsdata.ZjOdds, t.qzCfg.qzOdds...)
	gsdata.XjOdds = append(gsdata.XjOdds, t.qzCfg.xjBetOdds...)
	gsdata.Ui = make([]*netproto.UserInfo, 0)
	for _, v := range t.roles {
		u := t.GetUserBySeatNo(v.seat)
		info := new(netproto.UserInfo)
		info.Money = proto.Int64(v.money)
		info.Point = proto.Int32(v.point)
		info.Status = proto.Int32(v.status)
		info.XjBet = proto.Int32(v.xjodds)
		info.ZjBet = proto.Int32(v.qzodds)
		info.Seat = proto.Int32(v.seat)
		info.UserId = proto.Int32(v.roleid)
		info.Cards = make([]*netproto.Qz21Card, 0)
		info.NickName = proto.String(u.NickName)
		upk := t.gamePoker.GetUserPoker(v.roleid)
		if upk != nil {
			for _, v := range upk.Cards.Cards {
				c := new(netproto.Qz21Card)
				c.Color = &v.Color
				c.Number = &v.CardNumber
				info.Cards = append(info.Cards, c)
			}
			mlog.Debug("user %d cards %v", v.roleid, upk.Cards.Cards)
		}
		gsdata.Ui = append(gsdata.Ui, info)
	}
	pmsg.MsgData = gsdata
	t.SendMessageToUser(user, pmsg)
}

func (t *Qz21Table) getDian(upk *poker.CardGroup) int32 {
	number := int32(0)
	count := int32(0)
	for _, v := range upk.Cards {
		if v.CardNumber == 14 {
			count++
		} else if v.CardNumber > 10 {
			number += 10
		} else {
			number += v.CardNumber
		}
	}

	if count != 0 {
		for i := int32(0); i < count; i++ {
			if (number + 11) <= 21-(count-i-1) {
				number += 11
			} else {
				number += 1
			}
		}
	}

	return number
}

func (t *Qz21Table) sendCardToBanker() {
	var number int32 = 0
	u := t.getRoleInfoBySeat(t.bankerSeat)
	if u != nil {
		ur := t.GetUserByUserID(u.roleid)
		for {
			number = 0
			upk := t.gamePoker.GetUserPoker(u.roleid)
			if upk != nil {
				number = t.getDian(upk.Cards)
				if number >= 17 {
					//if number > 21 {
					//number = 0
					//}
					u.point = number
					break
				} else {
					cards := t.poker.Deal(ur.UserID, 1)
					t.GameLog.Fapai(ur.UserID, cards.String())
					mlog.Debug("庄家发牌 %v", cards)
					p := poker.NewUserPoker(ur, cards)
					t.gamePoker.AddUserPoker(p)
					t.BoardcastMessage(t.getPokerMsg(p))
				}
			}
		}
	} else {
		mlog.Error("invalid banker seat!")
	}
}

func (t *Qz21Table) sendCardToUser(seat int32, count int) {
	if seat == t.bankerSeat {
		return
	}
	u := t.getRoleInfoBySeat(seat)
	if u != nil {
		ur := t.GetUserByUserID(u.roleid)
		upk := t.gamePoker.GetUserPoker(u.roleid)
		if upk != nil {
			cards := t.poker.Deal(u.roleid, count)
			mlog.Debug("闲家发牌 %v", cards)
			p := poker.NewUserPoker(ur, cards)
			t.gamePoker.AddUserPoker(p)
			t.BoardcastMessage(t.getPokerMsg(p))
			t.GameLog.Fapai(ur.UserID, cards.String())
			mlog.Debug("用户的牌为：%v", cards.Cards)
			return
		}
	} else {
		mlog.Error("invalid banker seat!")
	}
}

func (t *Qz21Table) sendDeskCfg(user *gamelogic.GameUser) {
	gsmsg := new(msg.Message)
	gsmsg.BClassID = int32(netproto.MessageBClassID_Game)
	gsmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21GameDeskCfgId)
	gsdata := new(netproto.QZ21GameDeskCfg)
	gsdata.Difen = proto.Int32(t.difen)
	gsdata.TimeOut = proto.Int32(t.timeOut)
	gsmsg.MsgData = gsdata
	t.SendMessageToUser(user, gsmsg)
}

func (t *Qz21Table) caculatePoint(seat int32) int32 {
	u := t.getRoleInfoBySeat(seat)
	upk := t.gamePoker.GetUserPoker(u.roleid)
	if upk == nil {
		mlog.Debug("get invalid cardsgroup!")
		return 0
	}
	number := t.getDian(upk.Cards)
	u.point = number
	return number
}

func (t *Qz21Table) responseChoseOdds(ot int32, user *gamelogic.GameUser, odds int32) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21ChoseOddsRetId)
	gsdata := new(netproto.ChoseOddsRet)
	gsdata.Code = proto.Int32(0)
	gsdata.Msg = proto.String("OK")
	gsdata.Ot = &ot
	gsdata.Odds = proto.Int32(odds)
	gsdata.UserId = proto.Int32(user.UserID)
	gsdata.SeatNo = proto.Int32(user.SeatNo)
	pmsg.MsgData = gsdata
	t.SendMessageToUser(user, pmsg)
}

func (t *Qz21Table) responseRoleOpt(rot int32, user *gamelogic.GameUser, err int32, mstr string) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.QZ21_GameMessageClassID_QZ21RoleOptRetId)
	gsdata := new(netproto.RoleOptRet)
	gsdata.Code = proto.Int32(err)
	gsdata.Msg = proto.String(mstr)
	gsdata.Ot = &rot
	gsdata.UserId = proto.Int32(user.UserID)
	gsdata.SeatNo = proto.Int32(user.SeatNo)
	pmsg.MsgData = gsdata
	t.SendMessageToUser(user, pmsg)
}

func (t *Qz21Table) getZjOdds() int32 {
	u := t.getBanker()
	return u.qzodds
}

func (t *Qz21Table) checkBaoPai(seat int32) (bool, int32) {
	u := t.getRoleInfoBySeat(seat)
	upk := t.gamePoker.GetUserPoker(u.roleid)
	number := t.getDian(upk.Cards)

	if number > 21 {
		u.isBaoPai = true
	} else {

	}
	u.point = number
	return u.isBaoPai, u.point
}

func (t *Qz21Table) compareWithBanker(seat int32) {
	if t.bankerSeat == seat {
		return
	}
	var money int32 = 0
	banker := t.getBanker()
	u := t.getRoleInfoBySeat(seat)
	if banker == nil || u == nil {
		return
	}
	mlog.Debug("userId=%d,bankerId=%d,bankermoney=%d,umoney=%d", u.roleid, t.getBanker().roleid, t.getBanker().money, u.money)
	bpoint := banker.point
	upoint := u.point
	if bpoint > 21 {
		bpoint = 0
	}

	if upoint > 21 {
		upoint = 0
	}

	if bpoint >= upoint {
		if banker.point == 21 {
			money = (t.getZjOdds() + u.xjodds) * t.difen * 2
		} else {
			money = (t.getZjOdds() + u.xjodds) * t.difen
		}
		u.money -= int64(money)
		mlog.Debug("1 userId=%d,bankerId=%d,bankermoney=%d,umoney=%d,money=%d", u.roleid, t.getBanker().roleid, t.getBanker().money, u.money, money)
		//amount, tax := t.GetUserWinAndTax(int64(money))
		banker.money += int64(money)
		//t.UpdateUserScoreFunc(banker.roleid, amount, tax)
		//a, at := t.GetUserWinAndTax(0 - int64(money))
		//t.UpdateUserScoreFunc(u.roleid, a, at)
	} else {
		if u.point == 21 {
			money = (t.getZjOdds() + u.xjodds) * t.difen * 2
		} else {
			money = (t.getZjOdds() + u.xjodds) * t.difen
		}
		banker.money -= int64(money)
		mlog.Debug("2 userId=%d,bankerId=%d,bankermoney=%d,umoney=%d,money=%d,upoint=%d,bankerpoint=%d", u.roleid, t.getBanker().roleid, t.getBanker().money, u.money, money, u.point, banker.point)
		//amount, tax := t.GetUserWinAndTax(int64(money))
		u.money += int64(money)
		//t.UpdateUserScoreFunc(u.roleid, amount, tax)
		//a, at := t.GetUserWinAndTax(0 - int64(money))
		//t.UpdateUserScoreFunc(banker.roleid, a, at)
	}
}

func (t *Qz21Table) checkAllRoleDo() bool {
	for _, v := range t.roles {
		if !v.isDo {
			return false
		}
	}
	return true
}

func (t *Qz21Table) resetRoleDo() {
	for _, v := range t.roles {
		v.isDo = false
	}
}

func (t *Qz21Table) init(table *gamelogic.Table) {
	t.roles = make([]*Qz21Role, 0)
	t.step = QZ21StepBegin
	t.maxroles = table.Room.MaxUser
	t.handlers = make([]callBack, QZ21Max)
	t.actionover = true
	for _, v := range t.AllGameUser() {
		role := new(Qz21Role)
		role.roleid = v.UserID
		role.seat = v.SeatNo
		role.iszj = false
		role.qzodds = -1
		role.xjodds = -1
		role.isBaoPai = false
		t.roles = append(t.roles, role)
	}
	t.bankerSeat = -1
	rand.Seed(time.Now().UnixNano())
}

func (t *Qz21Table) getBanker() *Qz21Role {
	if t.bankerSeat < 0 {
		return nil
	}
	return t.getRoleInfoBySeat(t.bankerSeat)
}

func (t *Qz21Table) getRoleInfoBySeat(seat int32) *Qz21Role {
	for _, v := range t.roles {
		if v.seat == seat {
			return v
		}
	}
	return nil
}

func (t *Qz21Table) clearData() {
	//if t.GetServerConfig().PokerType == gamelogic.PokerType_Random {
	//t.poker = poker.NewRandomPoker(false, false, 1)
	//t.poker = poker.NewFixedPoker(t.AllTableUser(), t.poker.(*poker.RandomPoker), 10)
	//} else { //发牌器发牌
	//	t.poker = poker.NewFilePoker(t.Configs)
	//}
	t.step = QZ21StepBegin
	t.bankerSeat = -1
	for _, v := range t.roles {
		v.iszj = false
		v.qzodds = 0
		v.xjodds = 0
		v.point = 0
		v.money = 0
		v.status = 0
		v.isBaoPai = false
		v.isNew = false
	}
}
