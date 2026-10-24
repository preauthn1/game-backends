package gamelog

import "fmt"

import "example.invalid/ServerTeam/Servers/pkg/conf"

// EmptyGameLog 空的游戏牌局记录实现
type EmptyGameLog struct {
	*gameLogBase
}

// NewEmptyGameLog 创建空的牌局记录实现
func NewEmptyGameLog() *EmptyGameLog {
	emptLog := new(EmptyGameLog)
	emptLog.gameLogBase = NewGameLogBase()

	return emptLog
}

// EndGameLog 結束
func (l *EmptyGameLog) EndGameLog(uscores map[int32]int64) {
	if !conf.AnalysisMode {
		fmt.Println(l.getGameLogDesc())
	}
}
