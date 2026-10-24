package gamelogic

import (
	"command"
	"config"
	"event"
	"gamelog"
	"msg"
	"net/http"
)

import (
	proto "example.invalid/golang/protobuf/proto"
	"rpcmsg"
)

type GameLogicFunc func(table *Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) GameLogic

type SuperToolFunc func(room *Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) ISuperToolService
//异步调用函数
type AsyncCallFunc func(asyncFunc func(), callBackFunc func())

//游戏逻辑接口
type GameLogic interface {
	//用户进入桌子
	UserEnter(gameuser *GameUser, tableno int32, seatno int32)

	//用户举手
	UserReady(gameuser *GameUser)

	//用户站立
	UserFree(gameuser *GameUser)

	//用户断线
	UserOffLine(gameuser *GameUser)

	//用户断线重连
	UserRecover(gameuser *GameUser)

	//用户离开
	UserLeave(gameuser *GameUser) bool

	//游戏消息
	GameMessage(user *GameUser, gmsg *msg.Message)
	//比賽消息
	PiSaiMessage(user *GameUser, gmsg *msg.Message)

	//是否游戏中
	IsGaming() bool

	//桌子号
	TableNo() int32

	//游戏开始
	OnGameStart(func())

	//游戏结束
	OnGameEnd(func(gametime int32))

	//游戏解散
	OnGameCancel(func(s string))

	//更新用户积分
	OnUpdateUserScore(func(userid int32, score int64, taxamount int64))

	OnUpdateExperienceScore(func(userid int32, score int64, taxamount int64))

	OnUpdateGameJackpotPool(func(userid int32, amount int64))

	//踢人
	OnKickUser(func(userid int32, msg string))

	//发送消息
	OnSendMessage(func(userid int32, m *msg.Message))

	//发送RPC消息
	OnSendRPCMessage(func(safe bool, m *msg.Message))

	//捕魚公告
	OnNotifyFishMessage(func(m *msg.Message))

	//收到RPC消息
	OnRecvRPCMessage(m *msg.Message)

	//收到Http消息
	OnRecvHttpMessage(req *http.Request) proto.Message

	//获取一个空闲的座位
	GetFreeSeat() (int32, int32)

	//设置牌局记录器
	SetGameLog(glog gamelog.GameLog)
	//
	//SetVipUserProvider(provider *VipUserProvider)
	//SetVipUserService(provider *VipUserService)

	//异步执行
	OnAsyncCall(f AsyncCallFunc)

	//交换用户最大牌型
	ExchangeMaxCard(userid int32)

	//提前換桌
	OnUserEarlyChangeTable(func(user *GameUser, early string))

	//更新設定檔
	RefreshConfigGroup(congifs *config.ConfigGroup)
}

//游戏逻辑管理器
type GameLogicManager struct {
	logics map[int32]GameLogic
}

//创建游戏逻辑管理器对象
func NewGameLogicManager(r *Room, cfgs *config.ConfigGroup, tfManager *event.TimerFuncManager, gcfunc GameLogicFunc) *GameLogicManager {
	glm := new(GameLogicManager)
	glm.logics = make(map[int32]GameLogic)

	var i int32
	for i = 0; i < r.TableCount; i++ {
		lg := gcfunc(r.GetTableByNo(i), tfManager, cfgs)

		glm.logics[i] = lg
	}

	glccmd := NewGameCmd(glm)
	command.AddCommand(glccmd)

	return glm
}

//获取游戏逻辑
func (glm *GameLogicManager) GetGameLogic(tableno int32) GameLogic {
	lg, ok := glm.logics[tableno]

	if ok {
		return lg
	}

	return nil
}

//遍历
func (glm *GameLogicManager) Range(f func(gl GameLogic)) {
	for _, g := range glm.logics {
		f(g)
	}
}
