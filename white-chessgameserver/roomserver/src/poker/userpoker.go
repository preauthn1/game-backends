package poker

import (
	"bytes"
	"fmt"
	"gamelogic"
	"netproto"
)

import proto "example.invalid/golang/protobuf/proto"

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

func (up *UserPoker) String() string {
	var buf bytes.Buffer
	buf.WriteString(fmt.Sprintf("%v.", up.User))

	for _, c := range up.Cards.Cards {
		buf.WriteString(fmt.Sprintf("%s,", c))
	}

	return buf.String()
}

//获取用户牌型网络消息数据
func (up *UserPoker) GetNetMessageData() *netproto.UserPoker {
	pdata := new(netproto.UserPoker)
	pdata.UserID = proto.Int32(up.User.UserID)
	pdata.SeatNo = proto.Int32(up.User.SeatNo)

	for _, c := range up.Cards.Cards {
		pdata.Cards = append(pdata.Cards, c.GetNetMessageData())
	}

	return pdata
}

//牌局牌型
type GamePoker struct {
	UserPokers []*UserPoker
}

//创建游戏牌型
func NewGamePoker() *GamePoker {
	gpk := new(GamePoker)
	return gpk
}

func (gp *GamePoker) String() string {
	var buf bytes.Buffer
	for _, p := range gp.UserPokers {
		buf.WriteString(fmt.Sprintf("%s\r\n", p))
	}

	return buf.String()
}

//添加用户牌
func (gpk *GamePoker) AddUserPoker(upk *UserPoker) {
	for _, p := range gpk.UserPokers {
		if p.User.UserID == upk.User.UserID {
			p.Cards.AddCardGroup(upk.Cards)

			return
		}
	}

	gpk.UserPokers = append(gpk.UserPokers, upk)
}

//获取用户牌型
func (gpk *GamePoker) GetUserPoker(userid int32) *UserPoker {
	for _, upk := range gpk.UserPokers {
		if upk.User.UserID == userid {
			return upk
		}
	}

	return nil
}

//获取游戏牌型网络消息数据
func (gpk GamePoker) GetNetMessageData() *netproto.GamePoker {
	ngpk := new(netproto.GamePoker)
	for _, upk := range gpk.UserPokers {
		ngpk.Poker = append(ngpk.Poker, upk.GetNetMessageData())
	}

	return ngpk
}
