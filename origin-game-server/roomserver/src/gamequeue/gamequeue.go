package gamequeue

import (
	"gamelogic"
)

//游戏撮合接口
type GameQueue interface {

	//用户排队
	UserQueue(user *gamelogic.GameUser)

	//用户游戏结束
	UserGameEnd(user *gamelogic.GameUser)

	//开始运行
	Start()

	//停止运行
	Stop()
}
