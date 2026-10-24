package gamelogic

import (
	"config"
	"event"
)

//固定用户数游戏逻辑基类
//用户坐满准备即开始游戏，游戏结束后用户再次排队
type FixedGameLogicBase struct {
	*GameLogicBase
}

//创建固定用户数游戏逻辑基类对象
func NewFixedGameLogicBase(t *Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *FixedGameLogicBase {
	rglc := new(FixedGameLogicBase)

	rglc.GameLogicBase = NewGameLogicBase(t, tfManager, cfgs)

	return rglc
}

//用户进入
func (glc *FixedGameLogicBase) UserEnter(gameuser *GameUser, tableno int32, seatno int32) {
	if glc.isgaming {
		return
	}

	glc.GameLogicBase.UserEnter(gameuser, tableno, seatno)
}

//用户举手
func (glc *FixedGameLogicBase) UserReady(gameuser *GameUser) {
	if glc.isgaming {
		return
	}

	glc.GameLogicBase.UserReady(gameuser)
}

//用户站立
func (glc *FixedGameLogicBase) UserFree(gameuser *GameUser) {
	if glc.isgaming {
		return
	}

	glc.GameLogicBase.UserFree(gameuser)
}

//用户离开
func (glc *FixedGameLogicBase) UserLeave(gameuser *GameUser) bool {
	if glc.isgaming {
		return false
	}

	return glc.GameLogicBase.UserLeave(gameuser)
}

func (glc *FixedGameLogicBase) GetFreeSeat() (int32, int32) {
	if glc.isgaming {
		return FreeTableNo, FreeSeatNo
	}

	return glc.GameLogicBase.GetFreeSeat()
}
