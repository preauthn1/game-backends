package jlbpk

import (
	"testing"
	"time"

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

func init() {
	conf.CLogLevel = 0
	conf.FLogLevel = 0
}

// 自動打一局
func TestOneGame(t *testing.T) {
	endChannel := make(chan bool)
	confInit()
	roomserver.GameLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) gamelogic.GameLogic {
		return NewJlbpkLogic(table, tfManager, configs)
	}

	tgrs, glc, _ := comUserReady(endChannel, t)

	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
		for _, u := range glc.AllGameUser() {
			if u.UserID == userid && u.IsRobot() {
				r := tgrs.RobotService.GetRobotManager().GetRobot(u.UserID)
				mlog.Debug("Test userid[%v] send Message, SClassID[%v]", r.UserInfo.UserID, m.SClassID)
				//rbglc.OnGameMessage(r, m)
			}
		}
	}

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Info("jlbpk game end")
	}
}

func confInit() {
	conf.CLogLevel = 0
	conf.FLogLevel = 0
	conf.TimerFastMode = 900 //40
	conf.TestMode = true
}

//流程整合測試共用初始化
func IntegrationTestInit(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *JlbpkLogic, chan bool, []*gamelogic.GameUser) {
	tgrs := fixtures.NewTestGameRoomServerWithConfig(cfg)
	glc := tgrs.GameLogicManager.GetGameLogic(0).(*JlbpkLogic)

	glc.RegisterAfterGameEndFunc(func() {
		endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64) {
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

	return tgrs, glc, endChannel, robots
}

//共用玩家準備
func comUserReady(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *JlbpkLogic, []*gamelogic.GameUser) {
	tgrs, glc, endChannel, _ := IntegrationTestInit(endChannel, t)

	time.Sleep(1 * time.Second)
	users := fixtures.GetGameUser()
	users[1].UserType = gamelogic.UserType_Normal
	users[2].UserType = gamelogic.UserType_Normal
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)
	users[2].UserState.OnUserQueue(users[1], tgrs.Room)
	//用戶入座
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserEnter(users[2], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserReady(users[1])
	glc.UserReady(users[2])

	return tgrs, glc, users
}
