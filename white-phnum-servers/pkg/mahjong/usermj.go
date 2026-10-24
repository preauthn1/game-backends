package mahjong

import "example.invalid/ServerTeam/Servers/pkg/gamelogic"

//用户牌型
type UserPoker struct {
	User  *gamelogic.GameUser //用户
	Cards *CardGroup          //牌型
}

//创建用户牌型对象
func NewUserPoker(user *gamelogic.GameUser, cards *CardGroup) *UserPoker {
	up := new(UserPoker)
	up.User = user
	up.Cards = cards

	return up
}
