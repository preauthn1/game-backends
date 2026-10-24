package dczz

import (
	"fmt"
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
	"os"
	"path"
	"runtime"
	"testing"
	"time"
)

var cfg *config.ConfigGroup

func init() {
	conf.CLogLevel = 0
	conf.FLogLevel = 0
	conf.TimerFastMode = 900 //40
	conf.TestMode = true

	// 檢查config位置
	if err := initCfg(); err != nil {
		fmt.Fprintf(os.Stderr, "Load config files error. %v\n", err)
		os.Exit(1)
	}
}

func initCfg() error {
	// 檢查config位置
	var data *config.ConfigGroup
	_, file, _, _ := runtime.Caller(0)
	var defaultPaths = []string{"./config", "../../config", path.Dir(file) + "/config", path.Dir(file) + "/../../config"}

	data = new(config.ConfigGroup)

	for _, path := range defaultPaths {
		fi, err := os.Stat(path)
		if err != nil {
			mlog.Debug("Find config folder %s Error", path)
			continue
		}

		if !fi.IsDir() {
			mlog.Debug("%s is not a folder", path)
			continue
		}

		data.LoadConfig(path, 0)

		if len(data.Configs) > 0 {
			mlog.Debug("Find config folder %s Success", path)
			break
		}
		mlog.Debug("Find config folder %s Error", path)
	}

	if data == nil {
		return fmt.Errorf("Can not find config folder, please put setting files on this folder")
	}

	if len(data.Configs) == 0 {
		return fmt.Errorf("Can not load configs, path: %s", data.DirName)
	}

	cfg = data

	return nil
}

func TestGameLogic(t *testing.T) {
	endChannel := make(chan bool)
	roomserver.GameLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLogic(table, tfManager, configs)
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
		mlog.NewDefaultLogger().Info("dczz game end")
	}
}

//流程整合測試共用初始化
func IntegrationTestInit(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *Logic, chan bool, []*gamelogic.GameUser) {
	tgrs := fixtures.NewTestGameRoomServerWithConfig(cfg)
	glc := tgrs.GameLogicManager.GetGameLogic(0).(*Logic)

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
func comUserReady(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *Logic, []*gamelogic.GameUser) {
	tgrs, glc, endChannel, _ := IntegrationTestInit(endChannel, t)

	time.Sleep(1 * time.Second)
	users := fixtures.GetGameUser()
	users[1].UserType = gamelogic.UserType_Normal
	//users[2].UserType = gamelogic.UserType_Rebot
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)
	//users[2].UserState.OnUserQueue(users[2], tgrs.Room)
	//用戶入座
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	//glc.UserEnter(users[2], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.UserReady(users[1])
	//glc.UserReady(users[2])

	return tgrs, glc, users
}
