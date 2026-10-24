package gamelogic

import (
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
)

//不固定人数的游戏逻辑基类
//游戏过程中，其他玩家可以入座准备。游戏结束后用户可换桌
//遍历游戏用户时应该用此基类中提供的方法AllGameUser
type DynamicGameLogicBase struct {
	*GameLogicBase
}

//创建固定用户数游戏逻辑基类对象
func NewDynamicGameLogicBase(t *Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *DynamicGameLogicBase {
	rglc := new(DynamicGameLogicBase)

	rglc.GameLogicBase = NewGameLogicBase(t, tfManager, cfgs)

	return rglc
}

//用户进入
func (glc *DynamicGameLogicBase) UserEnter(gameuser *GameUser, tableno int32, seatno int32) {
	glc.GameLogicBase.UserEnter(gameuser, tableno, seatno)
}

//用户举手
func (glc *DynamicGameLogicBase) UserReady(gameuser *GameUser) {
	glc.GameLogicBase.UserReady(gameuser)
}

//用户站立
func (glc *DynamicGameLogicBase) UserFree(gameuser *GameUser) {
	//如果是正在游戏中的用户
	if glc.isgaming && glc.IsGameUser(gameuser) {
		return
	}

	glc.GameLogicBase.UserFree(gameuser)
}

//用户离开
func (glc *DynamicGameLogicBase) UserLeave(gameuser *GameUser) bool {
	//如果是正在游戏中的用户
	if glc.isgaming && glc.IsGameUser(gameuser) {
		return false
	}

	return glc.GameLogicBase.UserLeave(gameuser)
}
