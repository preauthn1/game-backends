package gamelogic

import (
	"config"
	"event"
	"mlog"
	"time"
)

//人机对战类游戏基类
type ComputeGameLogicBase struct {
	*GameLogicBase
}

func NewComputeGameLogicBase(t *Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *ComputeGameLogicBase {
	rglc := new(ComputeGameLogicBase)

	rglc.GameLogicBase = NewGameLogicBase(t, tfManager, cfgs)

	return rglc
}

func (glc *ComputeGameLogicBase) UserEnter(gameuser *GameUser, tableno int32, seatno int32) {
	glc.GameLogicBase.UserEnter(gameuser, tableno, seatno)
	glc.UserReady(gameuser)
	//if !glc.isgaming {
	//gameuser.UserState.OnUserReady(gameuser, glc.TableManager.Room)
	//glc.checkGameStart()
	//} else {
	//	gameuser.UserState.OnUserGame(gameuser, glc.TableManager.Room)
	//}
}

//将玩家的游戏状态修改为游戏结束状态
func (glc *ComputeGameLogicBase) ChangeUserStateEnd(gameuser *GameUser) {
	gameuser.UserState.OnGameEnd(gameuser, glc.TableManager.Room)
}

//游戏结束
func (glc *ComputeGameLogicBase) OnGameEnd(f func(int32)) {
	glc.GameEndFunc = func() {
		//游戏结束前调用函数
		for _, regf := range glc.BeforeGameEndFunc {
			regf()
		}

		glc.isgaming = false
		glc.LastGameEndTime = time.Now()
		glc.PlayerActionManager.Clear()

		//广播游戏结束
		glc.BoardcastGameEnd()

		//通知用户状态改变
		for _, ur := range glc.AllTableUser() {
			ur.UserState.OnGameEnd(ur, glc.TableManager.Room)
			//广播用户状态改变
			glc.BoardcastUserStateChange(ur)
			//检查准备超时
			if ur.IsSit() {
				glc.checkUserReadyTimer(ur)
			}
		}

		gametime := int32(time.Now().Sub(glc.GameStartTime).Seconds())
		f(gametime)

		//游戏结束后调用函数
		for _, regf := range glc.AfterGameEndFunc {
			regf()
		}

		example.invalid("游戏结束...")
		for _, ur := range glc.AllTableUser() {
			mlog.Debug("用户ID=%v,%v", ur.UserID, ur.UserState.StateName())
			//用户状态变为准备状态
			ur.UserState.OnUserReady(ur, glc.TableManager.Room)
		}
		//游戏结束后又开始游戏
		glc.checkGameStart()
	}
}

//用户站立
func (glc *ComputeGameLogicBase) UserFree(gameuser *GameUser) {
	mlog.Debug("user free %v", gameuser)

	//广播用户离开
	glc.BoardcastUserLeave(gameuser)

	//通知用户站立
	gameuser.UserState.OnUserFree(gameuser, glc.TableManager.Room)
}

//用户离开
func (glc *ComputeGameLogicBase) UserLeave(gameuser *GameUser) bool {
	mlog.Debug("user leave %v", gameuser)

	glc.UserFree(gameuser)

	//通知用户离开
	gameuser.UserState.OnUserLeave(gameuser, glc.TableManager.Room)

	return true
}

func (glc *ComputeGameLogicBase) UserReady(gameuser *GameUser) {
	glc.GameLogicBase.UserReady(gameuser)
	//如果游戏已经开始，用户设为游戏状态
	if glc.isgaming {
		gameuser.UserState.OnUserGame(gameuser, glc.TableManager.Room)
	}
}

func (glc *ComputeGameLogicBase) UserRecover(gameuser *GameUser) {
	glc.GameLogicBase.UserRecover(gameuser)
	gameuser.UserState.OnUserGame(gameuser, glc.TableManager.Room)
}
