package bjl

import (
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
)

var BJLUserHistoryStore = make(map[int32][]*baijialeuserdata)

type bettype struct {
	baijialetype int32 //下注类型
	money        int64 //下注的钱
}

type baijialeuserdata struct {
	user        *gamelogic.GameUser
	userbet     []*bettype
	money       int64 //用户开始的钱
	moneylast   int64 //用户最开始的钱，用来更新用户中途加钱
	win         int64 //用户赢的钱
	updatemoney int64 //用户更新的钱
	clearcount  int32 //清理次数
}

//新建一个玩家数据实例
func newBaiJiaLeUserData(user *gamelogic.GameUser) *baijialeuserdata {
	ud := new(baijialeuserdata)
	ud.user = user
	ud.userbet = make([]*bettype, 0)
	ud.money = user.Score
	ud.moneylast = user.Score
	ud.win = 0
	ud.clearcount = 0
	ud.updatemoney = 0
	return ud
}

//清除玩家数据
func (ud *baijialeuserdata) clearData() {
	ud.userbet = make([]*bettype, 0)
	ud.money = ud.user.Score
	ud.moneylast = ud.user.Score
	ud.win = 0
}

//设置玩家下注
func (ud *baijialeuserdata) setUserBet(typevalue int32, money int32) {
	for _, v := range ud.userbet {
		if v.baijialetype == typevalue {
			v.money += int64(money)
			ud.money -= int64(money)
			return
		}
	}

	v := new(bettype)
	v.baijialetype = typevalue
	v.money = int64(money)
	ud.userbet = append(ud.userbet, v)
	ud.money -= int64(money)
}

//获取玩家下注数组
func (ud *baijialeuserdata) getUserBet() []*bettype {
	return ud.userbet
}

func (ud *baijialeuserdata) getUserBetByType(t int32) int64 {
	for _, bet := range ud.userbet {
		if bet.baijialetype == t {
			return bet.money
		}
	}
	return 0
}

func (ud *baijialeuserdata) getUserBetTotal() int64{
	sumbet := int64(0)
	for _, b := range ud.userbet {
		sumbet += b.money
	}
	return sumbet
}

func (ud *baijialeuserdata) calUserWinRate(size int32) int64{
	if userhis, ok := BJLUserHistoryStore[ud.user.UserID]; ok {
		wincount := int32(0)
		arr := userhis
		if len(userhis) > int(size) {
			arr = userhis[:size]
		} else {
			size = int32(len(arr))
		}

		for _, his := range arr {
			if his.win > 0 {
				wincount++
			}
		}
		return int64((float32(wincount) / float32(size)) * 100)
	}
	return 0
}

func (ud *baijialeuserdata) getShowWinLose() int64 {
	if ud.win == 0 {
		return -ud.getUserBetTotal()
	} else {
		return ud.win
	}
}