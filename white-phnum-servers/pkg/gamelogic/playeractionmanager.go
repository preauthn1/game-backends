package gamelogic

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

//玩家动作管理器
type PlayerActionManager struct {
	actions    map[string]*PlayerAction
	tfm        *event.TimerFuncManager
	actionTurn int
	gameLogic  *GameLogicBase
}

//创建动作管理器对象
func NewPlayerActionManager(tfm *event.TimerFuncManager, gamelogic *GameLogicBase) *PlayerActionManager {
	pam := new(PlayerActionManager)
	pam.tfm = tfm
	pam.actionTurn = 0
	pam.actions = make(map[string]*PlayerAction)
	pam.gameLogic = gamelogic

	return pam
}

//清理动作管理器
func (pam *PlayerActionManager) Clear() {
	pam.actionTurn = 0
	pam.actions = make(map[string]*PlayerAction)
}

//获取动作的timer
func (pam *PlayerActionManager) GetActionTimer(actname string) *event.TimerFunc {
	timername := pam.getActionTimerName(actname)

	return pam.tfm.GetTimerFunc(timername)
}

//获取动作timer消耗的秒数
func (pam *PlayerActionManager) GetActionTimerElapsedSeconds(actname string) int32 {
	tm := pam.GetActionTimer(actname)
	if tm == nil {
		return 0
	}

	return tm.ElapsedSeconds()
}

//创建动作
func (pam *PlayerActionManager) NewAction(actbasename string, usercount int32, timeout int32, finishFunc func()) string {
	pam.actionTurn++
	actname := fmt.Sprintf("%s_%d_%d_%d", actbasename, pam.gameLogic.TableNo(), pam.gameLogic.gameCounter, pam.actionTurn)

	act := NewPlayerAction(usercount, actname, finishFunc, timeout)

	pam.actions[actname] = act

	//启动超时timer
	pam.tfm.NewNamedTimer(timeout, func() {
		pam.actionFinishedCallBack(actname)
	}, pam.getActionTimerName(actname))

	//example.invalid("创建动作监听%s,%d,%d", actbasename, usercount, timeout)

	return actname
}

//取消动作
func (pam *PlayerActionManager) CancelAction(actname string) {
	timername := pam.getActionTimerName(actname)
	pam.tfm.RemoveTimer(timername)
}

//获取动作timer 名称
func (pam *PlayerActionManager) getActionTimerName(actname string) string {
	return fmt.Sprintf("acttimer_%s", actname)
}

//接收到用户动作
func (pam *PlayerActionManager) OnUserAction(actname string, userid int32) {
	if act, ok := pam.actions[actname]; ok {
		act.recvUserAction(userid)

		mlog.Debug("收到用户%d动作%s", userid, actname)

		if act.isAllUserFinished() {
			pam.actionFinishedCallBack(actname)
		}
	}
}

//获取已经提交动作的用户
func (pam *PlayerActionManager) GetFinishedActionUsers(actname string) []int32 {
	if act, ok := pam.actions[actname]; ok {
		return act.finishedUsers
	}

	return nil
}

//动作完成毁掉函数
func (pam *PlayerActionManager) actionFinishedCallBack(actname string) {
	if theact, ok := pam.actions[actname]; ok {
		mlog.Debug("执行动作%s完成回调", actname)

		//移除timer
		timername := pam.getActionTimerName(actname)
		pam.tfm.RemoveTimer(timername)

		//执行回调
		theact.finishFunc()

		//移除动作
		delete(pam.actions, actname)
	}
}

//玩家动作
type PlayerAction struct {
	finishedUsers []int32
	userCount     int32
	actionName    string
	finishFunc    func()
	timeout       int32
}

//创建用户动作对象
func NewPlayerAction(usercount int32, actname string, finishFunc func(), timeout int32) *PlayerAction {
	pa := new(PlayerAction)
	pa.userCount = usercount
	pa.actionName = actname
	pa.finishFunc = finishFunc
	pa.timeout = timeout

	return pa
}

//接收到用户动作
func (pa *PlayerAction) recvUserAction(userID int32) {
	if pa.checkUserAction(userID) {
		return
	}

	pa.finishedUsers = append(pa.finishedUsers, userID)
}

//检查用户是否已经完成了动作
func (pa *PlayerAction) checkUserAction(userID int32) bool {
	for _, uid := range pa.finishedUsers {
		if uid == userID {
			return true
		}
	}

	return false
}

//是否所有用户都完成了动作
func (pa *PlayerAction) isAllUserFinished() bool {
	if int32(len(pa.finishedUsers)) == pa.userCount {
		return true
	}

	return false
}

//已经完成动作的用户数
func (pa *PlayerAction) finishedUserCount() int32 {
	return int32(len(pa.finishedUsers))
}

//未完成动作的用户数
func (pa *PlayerAction) unFinishedUserCount() int32 {
	return pa.userCount - int32(len(pa.finishedUsers))
}
