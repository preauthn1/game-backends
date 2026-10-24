package gamequeue

import "example.invalid/ServerTeam/Servers/pkg/event"

const (
	EventName_GameMatch = "gamequeue_GameMatch"  //事件名称,用户匹配
	argsName_MatchUsers = "gamequeue_matchusers" //事件参数,匹配的用户列表
)

//创建用户撮合事件
func NewGameMatchEvent() *event.Event {
	args := event.NewEventArgs()

	evt := event.NewEvent(EventName_GameMatch, args)

	return evt
}
