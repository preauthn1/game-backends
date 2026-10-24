package ermjlogic

import (
	"conf"
	"config"
	"event"
	"fixtures"
	"gamelogic"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"msg"
	"mylog"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"testing"
)

func TestLoop(t *testing.T) {
	for i:=0; i<500; i++ {
		TestRound03(t)
	}
}

//自動打完一局, 普通託管
func TestRound01(t *testing.T) {
	endChannel := make(chan bool)
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {

		return NewMahjongGame(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		robotCfg.Enable = false	//手動產生測試機器人
		return NewSCMJRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	tgrs, glc, rbglc, _ := comUserReady(endChannel, t)
	glc.bIsIntelligentTuoGuan = false	//普通託管

	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
		//mlog.Debug("[weihao] s[%v]", m.SClassID)
		for _, u := range glc.AllGameUser() {
			if u.UserID == userid && u.IsRobot() {
				r := tgrs.RobotService.GetRobotManager().GetRobot(u.UserID)
				mlog.Warn("[weihao] Test userid[%v] send Message", r.UserInfo.UserID)
				rbglc.OnGameMessage(r, m)
			}
		}
	}

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Info("scmj game end")
	}
}

//TODO 智慧託管
func TestRound02(t *testing.T) {
	endChannel := make(chan bool)
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {

		return NewMahjongGame(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		robotCfg.Enable = false	//手動產生測試機器人
		return NewSCMJRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	tgrs, glc, rbglc, _ := comUserReady(endChannel, t)
	glc.bIsIntelligentTuoGuan = true	//普通託管

	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
		//mlog.Debug("[weihao] s[%v]", m.SClassID)
		for _, u := range glc.AllGameUser() {
			if u.UserID == userid && u.IsRobot() {
				r := tgrs.RobotService.GetRobotManager().GetRobot(u.UserID)
				mlog.Warn("[weihao] Test userid[%v] send Message", r.UserInfo.UserID)
				rbglc.OnGameMessage(r, m)
			}
		}
	}

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Info("scmj game end")
	}
}

//TODO 剩余多少张牌结束游戏
func TestRound03(t *testing.T) {
	endChannel := make(chan bool)
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {

		return NewMahjongGame(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		robotCfg.Enable = false	//手動產生測試機器人
		return NewSCMJRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	tgrs, glc, rbglc, _ := comUserReady(endChannel, t)
	glc.bIsIntelligentTuoGuan = false	//普通託管
	//glc.endCardSize = 65	//剩余多少张牌结束游戏

	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
		//mlog.Debug("[weihao] s[%v]", m.SClassID)
		for _, u := range glc.AllGameUser() {
			if u.UserID == userid && u.IsRobot() {
				r := tgrs.RobotService.GetRobotManager().GetRobot(u.UserID)
				mlog.Warn("[weihao] Test userid[%v] send Message", r.UserInfo.UserID)
				rbglc.OnGameMessage(r, m)
			}
		}
	}

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Info("scmj game end")
	}
}

func confInit() {
	conf.CLogLevel = 0
	conf.TimerFastMode = 900 //40
	conf.TestMode = true
	mylog.InitDatabaseQueue("", 0, 0, 0, mlog.NewDefaultLogger())
}

//流程整合測試共用初始化
func IntegrationTestInit(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *MahjongSC, *SCMJRobotLogic, chan bool, []*gamelogic.GameUser) {
	tgrs := fixtures.NewTestGameRoomServer()
	glc := tgrs.GameLogicManager.GetGameLogic(0).(*MahjongSC)
	rbglc := tgrs.RobotService.GetRobotLogicManager().GetRobotLogic(0).(*SCMJRobotLogic)

	glc.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount, coupon int64) {
	}

	glc.SendFishNotifyFunc = func(m *msg.Message) {

	}

	glc.OnGameStart(func() {
	})

	_, robots := fixtures.GetGameUsers()
	robotManager := tgrs.RobotService.GetRobotManager()
	if robotManager.RobotCount() != 0 {
		t.Errorf("初始化, 機器人數量應為0, RobotCount=>%d", robotManager.RobotCount())
	}
	for i := 0; i < 5; i++ {
		robot := new(robotcomm.Robot)
		robot.UserInfo = robots[i]
		robot.RebotLevel = 10
		robot.UserInfo.UserType = gamelogic.UserType_Rebot
		robotManager.AddRobot(robot)
	}

	glc.TableManager.Room.Seq = proto.Int64(0)

	return tgrs, glc, rbglc, endChannel, robots
}

//共用玩家準備
func comUserReady(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *MahjongSC, *SCMJRobotLogic, []*gamelogic.GameUser) {
	tgrs, glc, rbglc, endChannel, robots := IntegrationTestInit(endChannel, t)

	users := fixtures.GetGameUser()
	users[1].UserType = gamelogic.UserType_Normal
	robots[2].UserType = gamelogic.UserType_Rebot
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)
	robots[2].UserState.OnUserQueue(robots[2], tgrs.Room)
	//用戶入座
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(robots[2], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserReady(users[1])
	glc.UserReady(robots[2])
	return tgrs, glc, rbglc, users
}
