package robotcomm

import (
	"gamelogic"
	"mlog"
	"sync"
)

//机器人管理器
type RobotManager struct {
	robots map[int32]*Robot
	sync.RWMutex
}

//创建机器人管理器
func NewRobotManager() *RobotManager {
	rm := new(RobotManager)
	rm.robots = make(map[int32]*Robot, 0)

	return rm
}

//添加机器人
func (rm *RobotManager) AddRobot(r *Robot) {
	rm.Lock()
	defer rm.Unlock()
	rm.robots[r.UserInfo.UserID] = r
}

//移除机器人
func (rm *RobotManager) RemoveRobot(userid int32) {
	rm.Lock()
	defer rm.Unlock()
	delete(rm.robots, userid)
}

//根据ID获取机器人
func (rm *RobotManager) GetRobot(userid int32) *Robot {
	rm.Lock()
	defer rm.Unlock()
	if r, ok := rm.robots[userid]; ok {
		return r
	}

	return nil
}

//获取所有机器人
func (rm *RobotManager) GetAllRobot() []*Robot {
	rm.Lock()
	defer rm.Unlock()
	robots := make([]*Robot, 0)
	for _, r := range rm.robots {
		robots = append(robots, r)
	}

	return robots
}

//机器人数量
func (rm *RobotManager) RobotCount() int {
	rm.Lock()
	defer rm.Unlock()
	return len(rm.robots)
}

//游戏中的机器人数量
func (rm *RobotManager) GameRobotCount() int {
	rm.Lock()
	defer rm.Unlock()
	cn := 0
	for _, r := range rm.robots {
		if r.UserInfo.IsGameing() {
			cn++
		}
	}

	return cn
}

//不在遊戲中的机器人数量
func (rm *RobotManager) GetNotGamingRobot() map[int32]*Robot {
	rm.Lock()
	defer rm.Unlock()
	bots := make(map[int32]*Robot, 0)
	for _, r := range rm.robots {
		if r.UserInfo.IsFree() || r.UserInfo.IsSit() || r.UserInfo.IsReady() {
			bots[r.UserInfo.UserID] = r
		}
	}
	return bots
}

//空闲的机器人数量
func (rm *RobotManager) FreeRobotCount() int {
	rm.Lock()
	defer rm.Unlock()
	cn := 0
	for _, r := range rm.robots {
		if r.UserInfo.IsFree() || r.UserInfo.IsSit() || r.UserInfo.IsReady() {
			cn++
		}
	}

	return cn
}

//获取一个空闲的机器人
func (rm *RobotManager) GetFreeRobot(exTableNo int32) *Robot {
	rm.Lock()
	defer rm.Unlock()
	for _, r := range rm.robots {
		if r.UserInfo.IsFree() {
			if exTableNo == gamelogic.FreeTableNo {
				return r
			}

			if r.UserInfo.LastTableNo == exTableNo {
				continue
			}

			return r
		}
	}
	mlog.Debug("獲取不到空閒機器人")
	return nil
}

//清除机器人上一次桌号
func (rm *RobotManager) ClearFreeRobotLastTable(tableNo int32) {
	rm.Lock()
	defer rm.Unlock()
	for _, r := range rm.robots {
		if r.UserInfo.IsFree() && r.UserInfo.LastTableNo == tableNo {
			r.UserInfo.LastTableNo = gamelogic.FreeTableNo
		}
	}
}
