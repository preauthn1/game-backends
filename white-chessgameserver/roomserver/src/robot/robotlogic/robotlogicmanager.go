package robotlogic

import (
	"config"
	"event"
	"gamelogic"
	"msg"
	"robot/robotcomm"
)

//机器人逻辑管理器
type RobotLogicManager struct {
	logics       map[int32]RobotLogic
	robotManager *robotcomm.RobotManager
	room         *gamelogic.Room
	tfManager    *event.TimerFuncManager
	gameLogics   *gamelogic.GameLogicManager
}

//创建机器人逻辑管理器
func NewRobotLogicManager(r *gamelogic.Room, robotCfg *robotcomm.RobotConfig, cfgs *config.ConfigGroup, tfManager *event.TimerFuncManager, rblgcf RobotLogicFunc, glcs *gamelogic.GameLogicManager, evtChannel *event.EventDispatcher, robotManager *robotcomm.RobotManager) *RobotLogicManager {
	rblgcs := new(RobotLogicManager)
	rblgcs.logics = make(map[int32]RobotLogic)
	rblgcs.robotManager = robotManager
	rblgcs.room = r
	rblgcs.tfManager = tfManager
	rblgcs.gameLogics = glcs

	var i int32
	for i = 0; i < r.TableCount; i++ {
		glc := glcs.GetGameLogic(i)
		rblgc := rblgcf(r.GetTableByNo(i), tfManager, robotCfg, cfgs, glc, evtChannel)

		rblgcs.logics[i] = rblgc
	}

	return rblgcs
}

//获取机器人逻辑
func (rblgc *RobotLogicManager) GetRobotLogic(tableno int32) RobotLogic {
	lgc, ok := rblgc.logics[tableno]

	if ok {
		return lgc
	}

	return nil
}

func (rblgc *RobotLogicManager) Range(f func(gl RobotLogic)) {
	for _, gl := range rblgc.logics {
		f(gl)
	}
}

//通知游戏消息
func (rblgc *RobotLogicManager) OnGameMessage(user *gamelogic.GameUser, m *msg.Message) {
	robotInfo := rblgc.robotManager.GetRobot(user.UserID)
	if robotInfo == nil {
		return
	}

	tableno := robotInfo.UserInfo.TableNo
	glc := rblgc.GetRobotLogic(tableno)
	if glc == nil {
		return
	}

	glc.OnGameMessage(robotInfo, m)

}
