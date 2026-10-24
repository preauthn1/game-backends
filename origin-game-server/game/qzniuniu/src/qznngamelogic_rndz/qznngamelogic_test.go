package qznngamelogic_rndz

import (
	"conf"
	"config"
	"event"
	"fixtures"
	"gamelog"
	"gamelogic"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"testing"
	"poker"
)


func init(){
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewQZNNGameLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewQZNNRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}
}

//五個玩家
func TestGameRound(t *testing.T) {
	endChannel := make(chan bool)
	conf.CLogLevel = 3
	conf.TimerFastMode = 100


	tgrs := fixtures.NewTestGameRoomServer()

	glc := tgrs.GameLogicManager.GetGameLogic(0).(*QZNNGameLogic)

	glc.GameLog = gamelogic.NewGameLogHelper(gamelog.NewEmptyGameLog(), glc.DynamicGameLogicBase.GameLogicBase)
	glc.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
	}

	/*
		待完善事項
		1. 目前無法使用撮合(真實用戶)或配桌(機器人)逻辑, 需要主動安排入座
	*/
	_, robots := fixtures.GetGameUsers()

	//Test Round 1 正常開始
	robotManager := tgrs.RobotService.GetRobotManager()

	if robotManager.RobotCount() != 0 {
		t.Errorf("初始化, 機器人數量應為0, RobotCount=>%d", robotManager.RobotCount())
	}

	//機器人實例化
	for i := 0; i < 5; i++ {
		robot := new(robotcomm.Robot)
		robot.UserInfo = robots[i]
		robot.RebotLevel = 6
		robotManager.AddRobot(robot)
	}
	if robotManager.RobotCount() != 5 {
		t.Errorf("加載機器人後數量應為3, RobotCount=>%d", robotManager.RobotCount())
	}

	//非機器人無法從free 到 sit, 故先統一狀態為QUEUE

	robots[0].UserState.OnUserQueue(robots[0], tgrs.Room)
	robots[1].UserState.OnUserQueue(robots[1], tgrs.Room)
	robots[2].UserState.OnUserQueue(robots[2], tgrs.Room)
	robots[3].UserState.OnUserQueue(robots[3], tgrs.Room)
	robots[4].UserState.OnUserQueue(robots[4], tgrs.Room)
	glc.UserEnter(robots[0], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[2], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[3], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[4], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)

	glc.UserReady(robots[0])
	glc.UserReady(robots[1])
	glc.UserReady(robots[2])
	glc.UserReady(robots[3])
	glc.UserReady(robots[4])

	select {
	case _ = <-endChannel:
		t.Log("game end")
	}

}

//兩個玩家3個觀戰
func TestGameRound2(t *testing.T) {
	endChannel := make(chan bool)
	conf.CLogLevel = 3
	conf.TimerFastMode = 100


	tgrs := fixtures.NewTestGameRoomServer()

	glc := tgrs.GameLogicManager.GetGameLogic(0).(*QZNNGameLogic)

	glc.GameLog = gamelogic.NewGameLogHelper(gamelog.NewEmptyGameLog(), glc.DynamicGameLogicBase.GameLogicBase)
	glc.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
	}

	/*
		待完善事項
		1. 目前無法使用撮合(真實用戶)或配桌(機器人)逻辑, 需要主動安排入座
	*/
	_, robots := fixtures.GetGameUsers()

	//Test Round 1 正常開始
	robotManager := tgrs.RobotService.GetRobotManager()

	if robotManager.RobotCount() != 0 {
		t.Errorf("初始化, 機器人數量應為0, RobotCount=>%d", robotManager.RobotCount())
	}

	//機器人實例化
	for i := 0; i < 5; i++ {
		robot := new(robotcomm.Robot)
		robot.UserInfo = robots[i]
		robot.RebotLevel = 6
		robotManager.AddRobot(robot)
	}
	if robotManager.RobotCount() != 5 {
		t.Errorf("加載機器人後數量應為3, RobotCount=>%d", robotManager.RobotCount())
	}

	//非機器人無法從free 到 sit, 故先統一狀態為QUEUE

	robots[0].UserState.OnUserQueue(robots[0], tgrs.Room)
	robots[1].UserState.OnUserQueue(robots[1], tgrs.Room)
	glc.UserEnter(robots[0], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserReady(robots[0])
	glc.UserReady(robots[1])


	robots[2].UserState.OnUserQueue(robots[2], tgrs.Room)
	robots[3].UserState.OnUserQueue(robots[3], tgrs.Room)
	robots[4].UserState.OnUserQueue(robots[4], tgrs.Room)

	glc.UserEnter(robots[2], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[3], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[4], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)


	glc.UserReady(robots[2])
	glc.UserReady(robots[3])
	glc.UserReady(robots[4])

	select {
	case _ = <-endChannel:
		t.Log("game end")
	}

}

//
//func TestStartGameLogicServer(t *testing.T) {
//
//	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
//		return NewQZNNGameLogic(t, tfManager, cfgs)
//	}
//	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
//		return NewQZNNRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
//	}
//
//	fixtures.StartGameLogicByTest()
//}
//
//func NewQzniuniuLogicByTest() *QZNNGameLogic {
//	_, filename, _, _ := runtime.Caller(1)
//	configDir := path.Dir(filename) + "/../../config"
//	cfgs := new(config.ConfigGroup)
//	cfgs.LoadConfig(configDir, 0)
//	cfg := cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)
//	serverInfo := cfgs.GetConfig(gamelogic.CONFIGFILENAME_SERVERINFO)
//
//	servercfg := gamelogic.GetGameServerConfig(serverInfo, cfg)
//	rumanager := gamelogic.NewRoomUserManager(new(player.PlayerManager))
//	room := gamelogic.NewRoom(servercfg, rumanager)
//	table := room.GetTableByNo(0)
//
//	evtdispatcher := event.NewAsyncEventDispatcher(1)
//	tfManager := event.NewTimerFuncManager(evtdispatcher)
//
//	glc := new(QZNNGameLogic)
//
//	glc.DynamicGameLogicBase = gamelogic.NewDynamicGameLogicBase(table, tfManager, cfgs)
//	parseNNConfig(cfgs)
//	glc.RegisterAfterGameEndFunc(func() {
//		//通知用户抢庄倍数
//		for _, ur := range glc.AllGameUser() {
//			glc.notifyUserBetTimes(ur)
//		}
//		//发牌动画
//		glc.currentWatchAnimationName = glc.RegisterAnimation(animationName_GameStart, animationTimeOut_GameStart, glc.fapai)
//	})
//	glc.RegisterBeforeGameStartFunc(func() {
//		//清除数据
//		glc.clearGameData()
//	})
//	return glc
//}
//
//func TestGameRound1(t *testing.T) {
//	glc := NewQzniuniuLogicByTest()
//	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
//	}
//	glc.OnGameStart(func() {
//	})
//	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
//	}
//	glc.GameEndFunc = func() {
//	}
//	glc.GameStartFunc()
//	users := fixtures.GetGameUser()
//	//玩家入座
//	glc.UserEnter(users[0], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
//	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
//
//	glc.UserReady(users[0])
//	glc.UserReady(users[1])
//
//	gub := new(gameUserBet)
//	gub.addUserBet(users[0].UserID, 0, true)
//	glc.zhuangInfo = gub.userBets[0]
//
//}

//指定發牌金額結算
func TestGameSettlement(t *testing.T) {
	endChannel := make(chan bool)
	conf.CLogLevel = 3
	conf.TimerFastMode = 100


	tgrs := fixtures.NewTestGameRoomServer()

	glc := tgrs.GameLogicManager.GetGameLogic(0).(*QZNNGameLogic)

	glc.GameLog = gamelogic.NewGameLogHelper(gamelog.NewEmptyGameLog(), glc.DynamicGameLogicBase.GameLogicBase)
	glc.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
	}

	//glc.GameStartFunc()
	users := fixtures.GetGameUser()
	//非機器人無法從free 到 sit, 故先統一狀態為QUEUE
	users[0].UserState.OnUserQueue(users[0], tgrs.Room)
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)
	//安排用戶入座
	if glc.GetCurrentGameUserCount() != 0 {
		t.Errorf("用戶數量有問題, should be 0 but GetCurrentGameUserCount %d", glc.GetCurrentGameUserCount())
	}

	//玩家入座
	glc.UserEnter(users[0], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)

	//glc.UserReady(users[0])
	//glc.UserReady(users[1])

	//gub := new(gameUserBet)
	//glc.gameStart()
	glc.clearGameData()
	glc.gamePoker = poker.NewGamePoker()

	glc.zhuangInfo = &userBet{users[0].UserID, 1, true}

	//發牌
	poker0, poker1, _ := getfilePoker()
	upk0 := poker.NewUserPoker(users[0], poker0)
	upk1 := poker.NewUserPoker(users[1], poker1)
	glc.gamePoker.AddUserPoker(upk0)
	glc.gamePoker.AddUserPoker(upk1)
	//glc.getXianBetMoney()
	glc.xianjiaBets.addUserBet(users[1].UserID, 1, true)
	//glc.cardGroupFinish()

	//计算牌局结果
	nnretLogic := newNNGameRet(glc)

	glc.gameRet = nnretLogic.getGameRet(glc)

	nnret := glc.gameRet

	//庄家
	zhuangwinAmount, tax := glc.GetUserWinAndTax(int64(nnret.ZhuangRet.GetWinLose()))
	if zhuangwinAmount == 1 && tax == 0 {
		//沒毛病
	} else {
		t.Errorf("莊家贏錢有問題, should be 1 but zhuangwinAmount %d", zhuangwinAmount)
	}
	//其他用户
	for _, uret := range nnret.GetUserRet() {
		userid := uret.GetUser().GetUserID()

		//如果是庄家,则不作处理
		if userid == glc.zhuangInfo.userID {
			continue
		}

		userwinAmount, tax := glc.GetUserWinAndTax(int64(uret.GetWinLose()))
		if userwinAmount == -1 && tax == 0 {
			//沒毛病
		} else {
			t.Errorf("閒家贏錢有問題, should be 1 but zhuangwinAmount %d", zhuangwinAmount)
		}
	}
}

func getfilePoker() (*poker.CardGroup, *poker.CardGroup, *poker.CardGroup) {
	var p poker.Poker
	p = poker.NewTestFilePoker("testsettlement")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	return fp.GetUserCards(int32(0)), fp.GetUserCards(int32(1)), fp.GetUserCards(int32(2))
}

//獎池, 玩家贏錢從稅收抽部分放入獎池
func TestGameSettlement2(t *testing.T) {
	endChannel := make(chan bool)
	conf.CLogLevel = 3
	conf.TimerFastMode = 100

	tgrs := fixtures.NewTestGameRoomServer()

	glc := tgrs.GameLogicManager.GetGameLogic(0).(*QZNNGameLogic)

	glc.GameLog = gamelogic.NewGameLogHelper(gamelog.NewEmptyGameLog(), glc.DynamicGameLogicBase.GameLogicBase)
	glc.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
	}

	//glc.GameStartFunc()
	users := fixtures.GetGameUser()
	//非機器人無法從free 到 sit, 故先統一狀態為QUEUE
	users[0].UserState.OnUserQueue(users[0], tgrs.Room)
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)
	users[2].UserState.OnUserQueue(users[2], tgrs.Room)
	//安排用戶入座
	if glc.GetCurrentGameUserCount() != 0 {
		t.Errorf("用戶數量有問題, should be 0 but GetCurrentGameUserCount %d", glc.GetCurrentGameUserCount())
	}

	//玩家入座
	glc.UserEnter(users[0], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(users[2], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)

	//glc.UserReady(users[0])
	//glc.UserReady(users[1])

	//gub := new(gameUserBet)
	//glc.gameStart()
	glc.clearGameData()
	glc.gamePoker = poker.NewGamePoker()

	glc.zhuangInfo = &userBet{users[0].UserID, 1, true}

	//發牌
	poker0, poker1, poker2 := getfilePoker()
	upk0 := poker.NewUserPoker(users[0], poker0)
	upk1 := poker.NewUserPoker(users[1], poker1)
	upk2 := poker.NewUserPoker(users[2], poker2)
	glc.gamePoker.AddUserPoker(upk0)
	glc.gamePoker.AddUserPoker(upk1)
	glc.gamePoker.AddUserPoker(upk2)
	//glc.getXianBetMoney()
	glc.xianjiaBets.addUserBet(users[0].UserID, 1000, true)
	glc.xianjiaBets.addUserBet(users[1].UserID, 1000, true)
	glc.xianjiaBets.addUserBet(users[2].UserID, 1000, true)
	//glc.cardGroupFinish()

	//计算牌局结果
	nnretLogic := newNNGameRet(glc)

	glc.jackpotTotalPool = 10000
	glc.gameRet = nnretLogic.getGameRet(glc)

	nnret := glc.gameRet

	//庄家
	zhuangwinAmount, tax := glc.GetUserWinAndTax(int64(nnret.ZhuangRet.GetWinLose()))
	tax, addjackpot := glc.GetUserTaxAddJackpotPool(tax, false)
	if zhuangwinAmount == -3000 && tax == 0 {
		//沒毛病
	} else {
		t.Errorf("莊家贏錢有問題, userid[%v] should be 1 but zhuangwinAmount %d , tax[%v] addjackpot[%v]", nnret.ZhuangRet.User.GetUserID(), zhuangwinAmount, tax, addjackpot)
	}
	//其他用户
	for _, uret := range nnret.GetUserRet() {
		userid := uret.GetUser().GetUserID()

		//如果是庄家,则不作处理
		if userid == glc.zhuangInfo.userID {
			continue
		}

		userwinAmount, tax := glc.GetUserWinAndTax(int64(uret.GetWinLose()))
		tax, addjackpot := glc.GetUserTaxAddJackpotPool(tax, false)
		if userid == 9000003 &&  addjackpot != 22 {
			t.Errorf("userid[%v] 獎金池累計計算錯誤, amount[%v]", uret.User.UserID, addjackpot)
		}
		if userid == 9000002 && userwinAmount != -1000 && tax == 0 {
			t.Errorf("閒家贏錢有問題, userid[%v] 閒輸贏[%v] 庄輸贏[%d] , tax[%v]", uret.User.GetUserID(), userwinAmount, zhuangwinAmount, tax)
		}
		if userid == 9000003 && userwinAmount != 2850 && tax == 128 {
			t.Errorf("閒家贏錢有問題, userid[%v] 閒輸贏[%v] 庄輸贏[%d] , tax[%v]", uret.User.GetUserID(), userwinAmount, zhuangwinAmount, tax)
		}
	}
}
