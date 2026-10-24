package jdesydLogic

import (
	"example.invalid/ServerTeam/Servers/pkg/robot/robotlogic"
	"testing"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/fixtures"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/roomserver"
)

func confInit() {
	conf.CLogLevel = 0
	conf.FLogLevel = 0
	conf.TimerFastMode = 900 //40
	conf.TestMode = true
}

func TestGameLogic(t *testing.T) {
	endChannel := make(chan bool)
	confInit()

	roomserver.GameLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) gamelogic.GameLogic {
		return NewJdesydLogic(table, tfManager, configs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewJdesydRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	pTestServer, pGameLogic, pRobotLogic, _ := comUserReady(endChannel, t)

	pGameLogic.SendMessgeFunc = func(userid int32, m *msg.Message) {
		for _, pUser := range pGameLogic.AllGameUser() {
			if pUser != nil {
				if pUser.UserID == userid && pUser.IsRobot() {
					pRobot := pTestServer.RobotService.GetRobotManager().GetRobot(pUser.UserID)
					if pRobot != nil {
						pRobotLogic.OnGameMessage(pRobot, m)
					}
				}
			}
		}
	}

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Info("jdesyd game end")
	}
}

//流程整合測試共用初始化
func IntegrationTestInit(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *JdesydLogic, *JdesydRobotLogic, chan bool, []*gamelogic.GameUser) {
	pTestServer := fixtures.NewTestGameRoomServer()
	pGameLogic := pTestServer.GameLogicManager.GetGameLogic(0).(*JdesydLogic)
	pRobotLogic := pTestServer.RobotService.GetRobotLogicManager().GetRobotLogic(0).(*JdesydRobotLogic)

	pGameLogic.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	pGameLogic.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64) {
	}

	pGameLogic.SendFishNotifyFunc = func(m *msg.Message) {

	}

	pGameLogic.OnGameStart(func() {
	})

	_, robots := fixtures.GetGameUsers()
	robotManager := pTestServer.RobotService.GetRobotManager()
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

	pGameLogic.TableManager.Room.Seq = proto.Int64(0)

	return pTestServer, pGameLogic, pRobotLogic, endChannel, robots
}

//共用玩家準備
func comUserReady(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *JdesydLogic, *JdesydRobotLogic, []*gamelogic.GameUser) {
	pTestServer, pGameLogic, pRobotLogic, endChannel, ary_pRobots := IntegrationTestInit(endChannel, t)

	ary_pUsers := fixtures.GetGameUser()
	ary_pUsers[1].UserType = gamelogic.UserType_Normal
	ary_pRobots[2].UserType = gamelogic.UserType_Rebot
	ary_pUsers[1].UserState.OnUserQueue(ary_pUsers[1], pTestServer.Room)
	ary_pRobots[2].UserState.OnUserQueue(ary_pRobots[2], pTestServer.Room)
	//用戶入座
	pGameLogic.UserEnter(ary_pUsers[1], pGameLogic.GetTable().TableNo, pGameLogic.GetTable().GetFreeSeat().SeatNo)
	pGameLogic.UserEnter(ary_pRobots[2], pGameLogic.GetTable().TableNo, pGameLogic.GetTable().GetFreeSeat().SeatNo)
	pGameLogic.UserReady(ary_pUsers[1])
	pGameLogic.UserReady(ary_pRobots[2])

	return pTestServer, pGameLogic, pRobotLogic, ary_pUsers
}
