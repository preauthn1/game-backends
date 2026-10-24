package gamelogic

//用户状态
type UserState interface {
	//用户站立
	OnUserFree(user *GameUser, room *Room) bool

	//用户坐下
	OnUserSit(user *GameUser, tableno int32, seatno int32, room *Room) bool

	//用户排队
	OnUserQueue(user *GameUser, room *Room) bool

	//用户准备
	OnUserReady(user *GameUser, room *Room) bool

	//用户开始游戏
	OnUserGame(user *GameUser, room *Room) bool

	//用户游戏结束
	OnGameEnd(user *GameUser, room *Room) bool

	//用户离开
	OnUserLeave(user *GameUser, room *Room) bool

	//用户断线
	OnUserOffLine(user *GameUser, room *Room) bool

	//状态名称
	StateName() string

	//状态值
	StateValue() int32
}

const (
	StateName_Free    = "free"
	StateName_Sit     = "sit"
	StateName_Queue   = "queue"
	StateName_Ready   = "ready"
	StateName_Game    = "game"
	StateName_Offline = "offline"
)

var freeState UserState
var sitState UserState
var queueState UserState
var readyState UserState
var gameState UserState
var offlineState UserState

func init() {
	freeState = new(UserFreeState)
	queueState = new(UserQueueState)
	sitState = new(UserSitState)
	readyState = new(UserReadyState)
	gameState = new(UserGameState)
	offlineState = new(UserOfflineState)
}
