package xyxgamelogic

import (
	"math/rand"
	"netproto"
	"time"
)

import proto "example.invalid/golang/protobuf/proto"

//牛牛牌局结果
type xyxGameRet struct {
	xyxlgc *XYXGameLogic
}

//创建小游戏结果对象
func newXYXGameRet(xyxglc *XYXGameLogic) *xyxGameRet {
	xyxret := new(xyxGameRet)
	xyxret.xyxlgc = xyxglc

	return xyxret
}

func (gameret *xyxGameRet) getGameRet() *netproto.XYX_GameRet {
	xyxUserRets := make([]*netproto.XYX_UserBetRet, 0)

	//隨機贏家
	userCount := len(gameret.xyxlgc.AllGameUser())
	s1 := rand.NewSource(time.Now().UnixNano())
	r1 := rand.New(s1)
	winIdx := r1.Intn(userCount)

	//創建結果並派彩
	for idx, ur := range gameret.xyxlgc.AllGameUser() {
		xyxRet := new(netproto.XYX_UserBetRet)
		ubtimes := gameret.xyxlgc.gameBet.getUserBetTimes(ur.UserID)
		bs := gameret.xyxlgc.GetServerConfig().BaseScore
		xyxRet.UserBetTimes = proto.Int32(ubtimes)
		xyxRet.UserID = proto.Int32(ur.UserID)

		if winIdx == idx {
			foo := int64(bs * xyxRet.GetUserBetTimes())
			xyxRet.WinLose = proto.Int64(foo)
		} else {
			foo := int64(-1 * bs * xyxRet.GetUserBetTimes())
			xyxRet.WinLose = proto.Int64(foo)
		}
		xyxUserRets = append(xyxUserRets, xyxRet)
	}
	//创建游戏结果
	xyxGameRet := new(netproto.XYX_GameRet)
	xyxGameRet.UserRet = xyxUserRets

	return xyxGameRet
}
