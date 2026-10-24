package gamelog

import "netproto"

//空的游戏牌局记录实现
type EmptyGameLog struct {
}

//创建空的牌局记录实现
func NewEmptyGameLog() *EmptyGameLog {
	emptLog := new(EmptyGameLog)

	return emptLog
}

func (l *EmptyGameLog) AddGameStep(step *GameStep) {

}

func (l *EmptyGameLog) BeginGameLog(urs map[int32]*UserData) {

}

func (l *EmptyGameLog) EndGameLog(uscores map[int32]int64, ucoupon map[int32]int64) {

}

func (l *EmptyGameLog) UpdateGameUsers(urs map[int32]*UserData) {

}

func (l *EmptyGameLog) SetNextJiangRecord(record *netproto.NextJiangRecord) {

}
