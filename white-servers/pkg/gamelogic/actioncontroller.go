package gamelogic

//用户行为控制器
type ActionController interface {
	//用户坐下
	UserSit(user *GameUser, tableno int32, seatno int32)

	//用户站立
	UserFree(user *GameUser)

	//用户举手
	UserReady(user *GameUser)

	//获取排队用户
	GetQueueUser() []*GameUser

	//房间总用户数
	TotalUserCount() int32

	//获取空闲的座位
	GetFreeSeat(user *GameUser, exIP string, exLastGameTableNo int32) (int32, int32)
	//新的获取空闲的座位
	GetFreeSeatNew(user *GameUser, exIP string, exLastGameTableNo int32) (int32, int32)

	//輸贏分隊獲取空閒座位
	GetWinLoseFreeSeat(extableno int32, exIP string, exLastGameTableNo int32, lastWinLose int32, islessthanuser bool) (int32, int32)
	//排除同桌率取得空位
	GetFreeSeatWithUPM(user *GameUser, exIP string, exLastGameTableNo int32) (int32, int32)
	//取得指定下局桌空位
	GetNextTableSeat(user *GameUser) (int32, int32)
}
