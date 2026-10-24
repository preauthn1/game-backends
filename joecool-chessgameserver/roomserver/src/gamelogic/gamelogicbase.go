package gamelogic

import (
	"config"
	"event"
	"fmt"
	"gamelog"
	"io/ioutil"
	"mlog"
	"msg"
	"net/http"
	"netproto"
	"os"
	"strings"
	"time"
)

import (
	"conf"
	proto "example.invalid/golang/protobuf/proto"
	"math"
)

//游戏逻辑基类
type GameLogicBase struct {
	BeforeGameStartFunc       []func()                                         //游戏开始前调用函数组
	AfterGameStartFunc        []func()                                         //游戏开始后调用函数组
	GameStartFunc             func()                                           //游戏开始调用函数
	BeforeGameEndFunc         []func()                                         //游戏结束前调用函数组
	AfterGameEndFunc          []func()                                         //游戏结束后调用函数组
	GameEndFunc               func()                                           //游戏结束调用函数
	GameCancelFunc            func(str string)                                 //游戏取消调用函数
	BeforePiSaiStartFunc      []func()                                         //比賽開始前調用函數
	AfterPiSaiStartFunc       []func()                                         //比賽開始後調用函數
	PiSaiStartFunc            func()                                           //比賽開始調用函數
	BeforePiSaiEndFunc        []func()                                         //比賽結束前調用函數
	AfterPiSaiEndFunc         []func()                                         //比賽結束後調用函數
	PiSaiEndFunc              func()                                           //比賽結束調用函數
	PiSaiCancelFunc           func(str string)                                 //比賽取消調用函數
	UpdateUserScoreFunc       func(userid int32, score int64, taxamount int64) //更新分数
	UpdateExperienceScoreFunc func(userid int32, score int64, taxamount int64) //體驗金幣
	UpdateGameJackpotPoolFunc func(userid int32, amount int64)							   //更新遊戲獎金池
	KickUserFunc              func(userid int32, msg string)                   //踢人
	SendMessgeFunc            func(userid int32, m *msg.Message)               //发送消息
	SendRPCMessageFunc        func(safe bool, m *msg.Message)                  //发送RPC消息
	SendFishNotifyFunc        func(m *msg.Message)                             //發送捕魚公告
	TableManager              *Table                                           //桌子
	isgaming                  bool                                             //是否游戏中
	LastGameEndTime           time.Time                                        //上一次游戏结束时间
	GameStartTime             time.Time                                        //游戏开始时间
	Configs                   *config.ConfigGroup                              //配置信息
	TimerFuncManager          *event.TimerFuncManager                          //定时器
	PlayerActionManager       *PlayerActionManager                             //游戏动作管理器
	gameCounter               int32                                            //游戏计数器,每次游戏开始后累加
	GameLog                   *GameLogHelper                                   //牌局记录
	AsyncCall                 AsyncCallFunc                                    //异步调用函数
	UserEarlyChangeTable      func(user *GameUser, early string)               //提前離桌
	GameTimeOutStopSig        chan bool
	//VipUserProvider           *VipUserProvider //VIP用戶
	//VipUserService            *VipUserService
}

//遊戲類型
const (
	ChenChien = 0 //真錢
	PiSai     = 1 //比賽活動
	HsiuChien = 2 //休閒
)

//创建游戏逻辑基类对象
func NewGameLogicBase(t *Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *GameLogicBase {
	glcb := new(GameLogicBase)
	glcb.TableManager = t
	glcb.Configs = cfgs
	glcb.isgaming = false
	glcb.TimerFuncManager = tfManager
	glcb.LastGameEndTime = time.Now()
	glcb.GameStartTime = time.Now()
	glcb.PlayerActionManager = NewPlayerActionManager(tfManager, glcb)
	glcb.gameCounter = 0
	glcb.GameLog = NewGameLogHelper(gamelog.NewEmptyGameLog(), glcb)
	glcb.GameTimeOutStopSig = make(chan bool, 1)
	return glcb
}

//游戏开始
func (glc *GameLogicBase) OnGameStart(f func()) {
	glc.GameStartFunc = func() {
		if glc.isgaming {
			return
		}

		//真錢
		if glc.TableManager.Room.GameType == ChenChien {
			//游戏开始前调用函数
			for _, regf := range glc.BeforeGameStartFunc {
				regf()
			}

			glc.isgaming = true
			glc.GameStartTime = time.Now()
			glc.PlayerActionManager.Clear()

			//通知用户状态改变
			for _, ur := range glc.AllTableUser() {
				ur.UserState.OnUserGame(ur, glc.TableManager.Room)
				//广播用户状态改变
				glc.BoardcastUserStateChange(ur)
			}

			//广播游戏开始
			glc.BoardcastGameStart()
			mlog.Debug("游戏开始...")

			//牌局记录
			glc.GameLog.GameBegin()

			f()

			//游戏开始后调用函数
			for _, regf := range glc.AfterGameStartFunc {
				regf()
			}

			//更新游戏逻辑计数器
			glc.gameCounter++
		}

		//是比賽
		if glc.TableManager.Room.GameType == PiSai {
			//游戏开始前调用函数
			for _, regf := range glc.BeforePiSaiStartFunc {
				regf()
			}

			glc.isgaming = true
			glc.GameStartTime = time.Now()
			glc.PlayerActionManager.Clear()

			////通知用户状态改变
			for _, ur := range glc.AllTableUser() {
				ur.UserState.OnUserGame(ur, glc.TableManager.Room)
				//广播用户状态改变
				glc.BoardcastUserStateChange(ur)
			}
			//
			////广播游戏开始
			glc.BoardcastGameStart()
			mlog.Debug("游戏开始...")

			//牌局记录
			glc.GameLog.GameBegin()

			f()

			//游戏开始后调用函数
			for _, regf := range glc.AfterPiSaiStartFunc {
				regf()
			}

			//更新游戏逻辑计数器
			glc.gameCounter++
		}

	}
}

//當遊戲開始後超過30分鐘未結束該局就踢出此張桌子所有人
func (glc *GameLogicBase) GameTimeOutKickUser() {

	for {

		select {
		case <-glc.GameTimeOutStopSig:
			mlog.Debug("遊戲正常結束, 離開超時線程")
			return
		case <-time.After(time.Duration(glc.GetServerConfig().GameTimeOut) * time.Minute):
			//case <- time.After(time.Duration(glc.GetServerConfig().GameTimeOut) * time.Second):
			mlog.Debug("xxxxxxxx")
			if glc.IsGaming() {
				mlog.Error("遊戲超時異常 取消遊戲 tableno %d", glc.TableNo())
				glc.GameCancelFunc("游戏超时异常")

				//for _, ur := range glc.AllTableUser() {
				//	mlog.System("遊戲超時異常 踢出userid:%v", ur.UserID)
				//	glc.KickUserFunc(ur.UserID, "游戏超时异常")
				//}

			}
			return
		}
	}
}

//游戏结束
func (glc *GameLogicBase) OnGameEnd(f func(int32)) {
	glc.GameEndFunc = func() {
		//游戏结束前调用函数
		for _, regf := range glc.BeforeGameEndFunc {
			regf()
		}

		glc.isgaming = false
		glc.LastGameEndTime = time.Now()
		glc.PlayerActionManager.Clear()

		//广播游戏结束
		glc.BoardcastGameEnd()

		//通知用户状态改变
		for _, ur := range glc.AllGameUser() {
			ur.UserState.OnGameEnd(ur, glc.TableManager.Room)
			//广播用户状态改变
			glc.BoardcastUserStateChange(ur)
			//检查准备超时
			if ur.IsSit() {
				glc.checkUserReadyTimer(ur)
			}
		}

		gametime := int32(time.Now().Sub(glc.GameStartTime).Seconds())

		f(gametime)

		//游戏结束后调用函数
		for _, regf := range glc.AfterGameEndFunc {
			regf()
		}

		//广播旁观用户
		glc.boardcastLookOnUser()

		mlog.Debug("游戏结束...")
	}
}

//游戏取消
func (glc *GameLogicBase) OnGameCancel(f func(str string)) {
	glc.GameCancelFunc = func(str string) {
		//游戏结束前调用函数
		for _, regf := range glc.BeforeGameEndFunc {
			regf()
		}

		glc.isgaming = false
		glc.LastGameEndTime = time.Now()
		glc.PlayerActionManager.Clear()

		//广播游戏结束
		glc.BoardcastGameEnd()

		//通知用户状态改变
		for _, ur := range glc.AllTableUser() {
			ur.UserState.OnGameEnd(ur, glc.TableManager.Room)
			//广播用户状态改变
			glc.BoardcastUserStateChange(ur)
			//检查准备超时
			if ur.IsSit() {
				glc.checkUserReadyTimer(ur)
			}
		}

		f(str)

		//游戏结束后调用函数
		for _, regf := range glc.AfterGameEndFunc {
			regf()
		}

		//广播旁观用户
		glc.boardcastLookOnUser()
		example.invalid("游戏取消...")
	}
}

//提前換桌
func (glc *GameLogicBase) OnUserEarlyChangeTable(f func(user *GameUser, early string)) {

	glc.UserEarlyChangeTable = f

}

//写分
func (glc *GameLogicBase) OnUpdateUserScore(f func(userid int32, score int64, taxamount int64)) {
	glc.UpdateUserScoreFunc = f
}

//體驗金幣寫分
func (glc *GameLogicBase) OnUpdateExperienceScore(f func(userid int32, score int64, taxamount int64)) {
	glc.UpdateExperienceScoreFunc = f
}

//遊戲獎金池寫分
func (glc *GameLogicBase) OnUpdateGameJackpotPool(f func(userid int32, amount int64)) {
	glc.UpdateGameJackpotPoolFunc = f
}

//踢人
func (glc *GameLogicBase) OnKickUser(f func(userid int32, msg string)) {
	glc.KickUserFunc = f
}

//发送消息
func (glc *GameLogicBase) OnSendMessage(f func(userid int32, m *msg.Message)) {
	glc.SendMessgeFunc = f
}

//发送RPC消息
func (glc *GameLogicBase) OnSendRPCMessage(f func(safe bool, m *msg.Message)) {
	glc.SendRPCMessageFunc = f
}

//發送捕魚公告
func (glc *GameLogicBase) OnNotifyFishMessage(f func(m *msg.Message)) {
	glc.SendFishNotifyFunc = f
}

//收到RPC消息
func (glc *GameLogicBase) OnRecvRPCMessage(m *msg.Message) {

}

//收到Http消息
func (glc *GameLogicBase) OnRecvHttpMessage(req *http.Request) proto.Message {
	return nil
}

//异步调用
func (glc *GameLogicBase) OnAsyncCall(f AsyncCallFunc) {
	glc.AsyncCall = f
}

//准备好的人数
func (glc *GameLogicBase) GetGameReadyUserCount() int32 {
	return int32(len(glc.TableManager.ReadyUser()))
}

//桌号
func (glc *GameLogicBase) TableNo() int32 {
	return glc.TableManager.TableNo
}

//是否游戏中
func (glc *GameLogicBase) IsGaming() bool {
	return glc.isgaming
}

//是否所有人都准备
func (glc *GameLogicBase) IsAllUserReady() bool {
	for _, u := range glc.TableManager.GetTableUser() {
		if !u.IsReady() {
			return false
		}
	}

	return true
}

//广播消息
func (glc *GameLogicBase) BoardcastMessage(m *msg.Message) {
	for _, u := range glc.TableManager.GetTableUser() {
		if !u.IsOffLine() {
			glc.SendMessgeFunc(u.UserID, m)
		}
	}
}

//捕魚公告
func (glc *GameLogicBase) NotifyFishMessage(m *msg.Message) {
	glc.SendFishNotifyFunc(m)
}

//向座位上的用户发送消息
func (glc *GameLogicBase) SendMessgeToSeat(seatno int32, m *msg.Message) {
	seat := glc.TableManager.GetSeatByNo(seatno)

	if seat == nil {
		return
	}

	ur := seat.SeatUser
	if ur != nil {
		glc.SendMessgeFunc(ur.UserID, m)
	}
}

//向指定用户ID发送消息
func (glc *GameLogicBase) SendMessageToUserID(userid int32, m *msg.Message) {
	glc.SendMessgeFunc(userid, m)
}

//向指定用户发送消息
func (glc *GameLogicBase) SendMessageToUser(user *GameUser, m *msg.Message) {
	glc.SendMessgeFunc(user.UserID, m)
}

//向一组指定用户ID发送消息
func (glc *GameLogicBase) SendMessageToUserIDs(userids []int32, m *msg.Message) {
	for _, uid := range userids {
		glc.SendMessageToUserID(uid, m)
	}
}

//向一组指定用户发送消息
func (glc *GameLogicBase) SendMessageToUsers(users []*GameUser, m *msg.Message) {
	for _, ur := range users {
		glc.SendMessageToUser(ur, m)
	}
}

//座位是否空闲
func (glc *GameLogicBase) CheckSeatEmpty(seatno int32) bool {
	seat := glc.TableManager.GetSeatByNo(seatno)

	if seat == nil {
		return false
	}

	return seat.SeatUser == nil
}

//当前桌子上用户数
func (glc *GameLogicBase) TableUserCount() int32 {
	return int32(len(glc.TableManager.GetTableUser()))
}

//根据用户ID获取用户信息
func (glc *GameLogicBase) GetUserByUserID(userid int32) *GameUser {
	urs := glc.TableManager.GetTableUser()

	for _, ur := range urs {
		if ur.UserID == userid {
			return ur
		}
	}

	return nil
}

//根据座位号获取用户
func (glc *GameLogicBase) GetUserBySeatNo(seatno int32) *GameUser {
	urs := glc.TableManager.GetTableUser()

	for _, ur := range urs {
		if ur.SeatNo == seatno {
			return ur
		}
	}

	return nil
}

//用户进入
func (glc *GameLogicBase) UserEnter(gameuser *GameUser, tableno int32, seatno int32) {
	st := glc.TableManager.GetSeatByNo(seatno)
	if st == nil {
		mlog.Warn("用户%v坐下,%d,%d,但座位不存在", gameuser, tableno, seatno)
		return
	}
	if st.SeatUser != nil {
		mlog.Warn("用户%v坐下,%d,%d,但座位上已经有用户%v", gameuser, tableno, seatno, st.SeatUser)
		return
	}

	mlog.Debug("user enter %v", gameuser)

	//通知用户坐下
	gameuser.UserState.OnUserSit(gameuser, tableno, seatno, glc.TableManager.Room)

	//通知用户进入
	glc.BoardcastUserEnter(gameuser)

	//检查用户准备超时
	glc.checkUserReadyTimer(gameuser)

	//广播旁观用户
	glc.boardcastLookOnUser()

	//gamelog 初始化用戶
	urs := make(map[int32]*gamelog.UserData)
	urs[gameuser.UserID] = new(gamelog.UserData)
	urs[gameuser.UserID].Score = gameuser.Score
	urs[gameuser.UserID].TradeID = gameuser.TradeID
	glc.GameLog.UpdateGameUsers(urs)
}

//检查游戏开始
func (glc *GameLogicBase) checkGameStart() {
	if !glc.isgaming {
		if glc.IsAllUserReady() && glc.TableUserCount() >= glc.GetServerConfig().MinPlayer {
			glc.GameStartFunc()
		}
	}

}

//用户举手
func (glc *GameLogicBase) UserReady(gameuser *GameUser) {

	mlog.Debug("user ready %v", gameuser)

	//通知用户举手
	gameuser.UserState.OnUserReady(gameuser, glc.TableManager.Room)

	//清除准备超时检查
	glc.clearUserReadyTimer(gameuser)

	//广播用户状态改变
	glc.BoardcastUserStateChange(gameuser)

	//检查游戏开始
	glc.checkGameStart()

	glc.checkMatchmaking(gameuser)
}

//用户站立
func (glc *GameLogicBase) UserFree(gameuser *GameUser) {
	mlog.Debug("user free %v", gameuser)

	//清除用户准备超时
	glc.clearUserReadyTimer(gameuser)

	//广播用户离开
	glc.BoardcastUserLeave(gameuser)

	//通知用户站立
	gameuser.UserState.OnUserFree(gameuser, glc.TableManager.Room)

	//检查游戏开始
	glc.checkGameStart()

	glc.checkMatchmaking(gameuser)

}

//用户离开
func (glc *GameLogicBase) UserLeave(gameuser *GameUser) bool {
	mlog.Debug("user leave %v", gameuser)

	glc.UserFree(gameuser)

	//通知用户离开
	gameuser.UserState.OnUserLeave(gameuser, glc.TableManager.Room)

	//检查游戏开始
	glc.checkGameStart()

	//glc.checkMatchmaking(gameuser)

	return true
}

//用户断线
func (glc *GameLogicBase) UserOffLine(gameuser *GameUser) {
	//如果用户已经是断线状态
	if gameuser.IsOffLine() {
		return
	}

	mlog.Debug("user offline %v", gameuser)

	//牌局记录
	glc.GameLog.AddGameStep("用户%d断线", gameuser.UserID)

	//通知用户断线
	gameuser.UserState.OnUserOffLine(gameuser, glc.TableManager.Room)

	glc.BoardcastUserStateChange(gameuser)
}

//用户断线重回
func (glc *GameLogicBase) UserRecover(gameuser *GameUser) {
	mlog.Debug("user recover %v", gameuser)

	//牌局记录
	glc.GameLog.AddGameStep("用户%d断线重连", gameuser.UserID)

	glc.notifyUserRecover(gameuser)

	//通知用户断线重连
	gameuser.UserState.OnUserGame(gameuser, glc.TableManager.Room)

	//发送自己进入桌子消息
	glc.BoardcastUserSelfEnter(gameuser)
	//发送其他用户进入桌子消息
	glc.BoardcastOhterUserOnTable(gameuser)

	//广播用户状态改变
	glc.BoardcastUserStateChange(gameuser)

	//广播旁观用户
	glc.boardcastLookOnUser()

}

//获取一个空闲的座位
func (glc *GameLogicBase) GetFreeSeat() (int32, int32) {

	st := glc.TableManager.GetFreeSeat()

	if st != nil {
		return glc.TableNo(), st.SeatNo
	}

	return FreeTableNo, FreeSeatNo
}

//通知用户断线重连
func (glc *GameLogicBase) notifyUserRecover(user *GameUser) {
	userRecoverMsg := new(msg.Message)
	userRecoverMsg.BClassID = int32(netproto.MessageBClassID_Game)
	userRecoverMsg.SClassID = int32(netproto.GameMessageClassID_UserRecoverID)

	glc.SendMessageToUser(user, userRecoverMsg)
}

//通知用户进入
func (glc *GameLogicBase) BoardcastUserEnter(user *GameUser) {
	//向桌子上的所有玩家广播用户进入
	uenterMsg := new(msg.Message)
	uenterMsg.BClassID = int32(netproto.MessageBClassID_Game)
	uenterMsg.SClassID = int32(netproto.GameMessageClassID_UserEnterID)

	uenterdata := new(netproto.UserEnter)
	uenterdata.User = getProtobufGameUser(user)
	uenterMsg.MsgData = uenterdata

	glc.BoardcastMessage(uenterMsg)
	mlog.Debug("广播用户%v进入桌子消息", user)

	//向用户自己发送其他用户进入消息
	glc.BoardcastOhterUserOnTable(user)
}

//广播用户自己进入
func (glc *GameLogicBase) BoardcastUserSelfEnter(user *GameUser) {
	tuenterMsg := new(msg.Message)
	tuenterMsg.BClassID = int32(netproto.MessageBClassID_Game)
	tuenterMsg.SClassID = int32(netproto.GameMessageClassID_UserEnterID)

	tuenterdata := new(netproto.UserEnter)
	tuenterdata.User = getProtobufGameUser(user)
	tuenterMsg.MsgData = tuenterdata
	glc.SendMessageToUser(user, tuenterMsg)

	mlog.Debug("向用户%v发送自己进入桌子消息", user)
}

//向用户广播桌子上的其他用户
func (glc *GameLogicBase) BoardcastOhterUserOnTable(user *GameUser) {
	//向用户广播所有桌子上的玩家
	for _, tur := range glc.AllTableUser() {
		tuenterMsg := new(msg.Message)
		tuenterMsg.BClassID = int32(netproto.MessageBClassID_Game)
		tuenterMsg.SClassID = int32(netproto.GameMessageClassID_UserEnterID)

		tuenterdata := new(netproto.UserEnter)
		tuenterdata.User = getProtobufGameUser(tur)
		tuenterMsg.MsgData = tuenterdata

		if user.UserID == tur.UserID {
			continue
		}

		mlog.Debug("向用户%v发送%v进入桌子消息", user, tur)
		glc.SendMessageToUser(user, tuenterMsg)
	}
}

//广播用户状态改变
func (glc *GameLogicBase) BoardcastUserStateChange(user *GameUser) {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.GameMessageClassID_UserStateChangeID)

	ust := new(netproto.UserStateChange)
	ust.User = getProtobufGameUser(user)

	m.MsgData = ust

	glc.BoardcastMessage(m)
}

//广播用户离开
func (glc *GameLogicBase) BoardcastUserLeave(user *GameUser) {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.GameMessageClassID_UserLeaveID)

	ulv := new(netproto.UserLeave)
	ulv.User = getProtobufGameUser(user)
	m.MsgData = ulv

	glc.BoardcastMessage(m)
}

//广播游戏开始
func (glc *GameLogicBase) BoardcastGameStart() {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.GameMessageClassID_GameStartID)

	glc.BoardcastMessage(m)
}

//广播游戏结束

func (glc *GameLogicBase) BoardcastGameEnd() {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.GameMessageClassID_GameEndID)

	glc.BoardcastMessage(m)
}

func getProtobufGameUser(user *GameUser) *netproto.GameUser {
	return user.GetProtoBufGameUser()

}

//获取服务器配置
func (glc *GameLogicBase) GetServerConfig() *GameServerConfig {
	return GetGameServerConfig(glc.Configs.GetConfig(CONFIGFILENAME_SERVERINFO), glc.Configs.GetConfig(CONFIGFILENAME_GAMESERVER))
}

//检查用户准备超时时间
func (glc *GameLogicBase) checkUserReadyTimer(user *GameUser) {
	readyWaitTime := glc.GetServerConfig().ReadyWaitTime + 5 //增加5秒,确保客户端倒计时结束后主动退出
	timername := glc.getTimerName(user)

	//检查timer是否已经启动
	extimer := glc.TimerFuncManager.GetTimerFunc(timername)
	if extimer != nil && !extimer.IsStop() {
		extimer.Stop()
	}

	glc.TimerFuncManager.NewNamedTimer(readyWaitTime, func() {
		if user.IsSit() && user.TableNo == glc.TableNo() {
			mlog.Debug("用户%v超时未准备,被踢出", user)
			glc.KickUserFunc(user.UserID, "由于您超时未准备,请尝试重新进入房间.")
		}
	}, timername)
}

//清除用户准备超时timer
func (glc *GameLogicBase) clearUserReadyTimer(user *GameUser) {
	timername := glc.getTimerName(user)
	mlog.Debug("移除准备超时 timer %s", timername)
	glc.TimerFuncManager.RemoveTimer(timername)
}

//获取timer名称
func (glc *GameLogicBase) getTimerName(user *GameUser) string {
	return fmt.Sprintf("gamelogic_readywait_timer_%d_%d_%d_%d", glc.TableNo(), glc.gameCounter, user.UserID, user.SeatNo)
}

//注册游戏开始后处理函数
func (glc *GameLogicBase) RegisterAfterGameStartFunc(f func()) {
	glc.AfterGameStartFunc = append(glc.AfterGameStartFunc, f)
}

//註冊遊戲開始後處理函數
func (glc *GameLogicBase) RegisterAfterPiSaiStartFunc(f func()) {
	glc.AfterPiSaiStartFunc = append(glc.AfterPiSaiStartFunc, f)
}

//注册游戏开始前处理函数
func (glc *GameLogicBase) RegisterBeforeGameStartFunc(f func()) {
	glc.BeforeGameStartFunc = append(glc.BeforeGameStartFunc, f)
}

//註冊遊戲開始前處理函數
func (glc *GameLogicBase) RegisterBeforePiSaiStartFunc(f func()) {
	glc.BeforePiSaiStartFunc = append(glc.BeforePiSaiStartFunc, f)
}

//注册游戏结束后处理函数
func (glc *GameLogicBase) RegisterAfterGameEndFunc(f func()) {
	glc.AfterGameEndFunc = append(glc.AfterGameEndFunc, f)
}

//註冊遊戲結束後處理函數
func (glc *GameLogicBase) RegisterAfterPiSaiEndFunc(f func()) {
	glc.AfterPiSaiEndFunc = append(glc.AfterPiSaiEndFunc, f)
}

//注册游戏结束前处理函数
func (glc *GameLogicBase) RegisterBeforeGameEndFunc(f func()) {
	glc.BeforeGameEndFunc = append(glc.BeforeGameEndFunc, f)
}

//註冊遊戲結束前處理函數
func (glc *GameLogicBase) RegisterBeforePiSaiEndFunc(f func()) {
	glc.BeforePiSaiEndFunc = append(glc.BeforePiSaiEndFunc, f)
}

//注册动画播放监听
func (glc *GameLogicBase) RegisterAnimation(actname string, timeout int32, callback func()) string {
	gameusercount := int32(glc.GetGameStateRealUserCount())

	return glc.RegisterAnimationByUserCount(actname, gameusercount, timeout, callback)
}

//注册用户动画播放监听
func (glc *GameLogicBase) RegisterAnimationByUserCount(actname string, usercount int32, timeout int32, callback func()) string {
	animationCallback := func() {
		//广播通知所有用户动画都已经播放完成
		glc.BoardcastAllUserAnimationCommited(actname)

		if callback != nil {
			callback()
		}
	}
	return glc.PlayerActionManager.NewAction(actname, usercount, timeout, animationCallback)
}

//响应用户提交动画完成
func (glc *GameLogicBase) CheckUserAnimationCommited(user *GameUser, m *msg.Message, currentactname string, f func(string)) {
	if m.BClassID == int32(netproto.MessageBClassID_Game) && m.SClassID == int32(netproto.GameMessageClassID_UserActionCommitID) {
		data := m.MsgData.([]byte)

		ust := &netproto.UserActionCommited{}

		err := proto.Unmarshal(data, ust)
		if err != nil {
			mlog.Error("解析用户%v动画播放完成数据结构错误%v", user, err)
			glc.KickUserFunc(user.UserID, "非法操作")
			return
		}
		actname := *ust.ActionName
		mlog.Debug("收到用户%d提交动画%s播放完成.", user.UserID, actname)

		if f != nil {
			f(actname)
		}

		if strings.Index(currentactname, actname) == 0 {
			glc.PlayerActionManager.OnUserAction(currentactname, user.UserID)
		}
	}
}

//广播所有用户动画播放完成
func (glc *GameLogicBase) BoardcastAllUserAnimationCommited(actname string) {
	amsg := new(msg.Message)
	amsg.BClassID = int32(netproto.MessageBClassID_Game)
	amsg.SClassID = int32(netproto.GameMessageClassID_AllUserActionCommitID)

	acdata := new(netproto.AllUserActionCommited)
	acdata.ActionName = proto.String(actname)

	amsg.MsgData = acdata

	glc.BoardcastMessage(amsg)
}

//当前游戏状态中的用户数，排除离线用户
func (glc *GameLogicBase) GetGameStateUserCount() int {
	return len(glc.AllOnLineGameUser())
}

//当前游戏状态中的真实用户数，排除机器人,离线用户
func (glc *GameLogicBase) GetGameStateRealUserCount() int {
	urs := glc.AllOnLineGameUser()

	cn := 0
	for _, ur := range urs {
		if !ur.IsRobot() {
			cn++
		}
	}

	return cn
}

//获取参与当前牌局游戏的用户数,包括离线用户
func (glc *GameLogicBase) GetCurrentGameUserCount() int {
	return len(glc.AllGameUser())
}

//获取所有游戏用户
func (glc *GameLogicBase) AllGameUser() []*GameUser {
	urs := make([]*GameUser, 0)
	for _, ur := range glc.AllTableUser() {
		if ur.IsGameing() || ur.IsOffLine() {
			urs = append(urs, ur)
		}
	}

	return urs
}

//获取所有旁观用户
func (glc *GameLogicBase) AllLookOnUser() []*GameUser {
	urs := make([]*GameUser, 0)
	for _, ur := range glc.AllTableUser() {
		if !ur.IsGameing() && !ur.IsOffLine() {
			urs = append(urs, ur)
		}
	}

	return urs
}

//获取在线游戏的用户
func (glc *GameLogicBase) AllOnLineGameUser() []*GameUser {
	urs := make([]*GameUser, 0)
	for _, ur := range glc.AllTableUser() {
		if ur.IsGameing() {
			urs = append(urs, ur)
		}
	}

	return urs
}

//桌子上的所有玩家
func (glc *GameLogicBase) AllTableUser() []*GameUser {
	return glc.TableManager.GetTableUser()
}

//桌子的所有在线玩家
func (glc *GameLogicBase) AllOnLineTableUser() []*GameUser {
	urs := glc.AllTableUser()

	onurs := make([]*GameUser, 0)
	for _, ur := range urs {
		if ur.IsOffLine() {
			continue
		}

		onurs = append(onurs, ur)
	}

	return onurs
}

//是否游戏中的用户
func (glc *GameLogicBase) IsGameUser(user *GameUser) bool {
	return user.IsGameing() || user.IsOffLine()
}

//向桌子上的游戏玩家广播消息
func (glc *GameLogicBase) BoardcastGameUserMessage(m *msg.Message) {
	for _, u := range glc.AllOnLineGameUser() {
		if !u.IsOffLine() {
			glc.SendMessgeFunc(u.UserID, m)
		}
	}
}

//检查用户消息
func (glc *GameLogicBase) CheckGameUserMessage(user *GameUser) bool {
	if !glc.IsGameUser(user) {
		glc.KickUserFunc(user.UserID, "非法操作")
		return false
	}

	return true
}

//新增成贏家抽稅

//获取用户扣除税收后的输赢和税收金额
func (glc *GameLogicBase) GetUserWinAndTax(amount int64) (int64, int64) {
	if amount <= 0 {
		return amount, 0
	}

	taxscale := glc.GetServerConfig().TaxScale

	taxamount := int64(amount * int64(taxscale) / 100)
	return amount - taxamount, taxamount
}

//將玩家稅收抽一部分放入獎金池
func (glc *GameLogicBase) GetUserTaxAddJackpotPool(taxamount int64, isrobot bool) (int64, int64) {
	if taxamount <= 0 || isrobot{
		return 0, 0
	}

	taxscale := glc.GetServerConfig().TaxAddPool
	addjackpottax := int64(taxamount * int64(taxscale) / 100)
	newtaxamount := taxamount - addjackpottax

	return newtaxamount, addjackpottax
}

func (glc *GameLogicBase) GetTax() float64 {
	return (float64(100) - float64(glc.GetServerConfig().TaxScale)) / float64(100)
}

//注水的錢要包含計算收稅比例後無條件捨去
func (glc *GameLogicBase) GetZhushuimoneyTax(zhushuimoney int64) int64 {
	taxscale := glc.GetServerConfig().TaxScale
	t1 := float32(1) - float32(taxscale)/float32(100)
	t2 := math.Ceil(float64(float32(zhushuimoney) * t1))
	return int64(t2)
}

//获取桌子
func (glc *GameLogicBase) GetTable() *Table {
	return glc.TableManager
}

//广播游戏旁观用户
func (glc *GameLogicBase) boardcastLookOnUser() {
	lookonMsg := new(msg.Message)
	lookonMsg.BClassID = int32(netproto.MessageBClassID_Game)
	lookonMsg.SClassID = int32(netproto.GameMessageClassID_LookOnUserNotifyID)

	lookOnData := new(netproto.LookOnUserNotify)
	lookonMsg.MsgData = lookOnData

	urs := glc.AllTableUser()
	for _, ur := range urs {
		if glc.isgaming && !ur.IsGameing() && !ur.IsOffLine() {
			lookOnData.UserID = append(lookOnData.UserID, ur.UserID)
		}
	}

	mlog.Debug("发送旁观用户,%v", lookOnData)
	glc.BoardcastMessage(lookonMsg)

}

//设置牌局记录器
func (glc *GameLogicBase) SetGameLog(glog gamelog.GameLog) {
	glc.GameLog = NewGameLogHelper(glog, glc)
}

func (glc *GameLogicBase) GetSuperToolService() ISuperToolService {
	return glc.GetTable().Room.SuperToolService
}

//func (glc *GameLogicBase) SetVipUserProvider(provider *VipUserProvider) {
//	//glc.VipUserProvider = provider
//}
//
//func (glc *GameLogicBase) SetVipUserService(service *VipUserService) {
//	//glc.VipUserService = service
//}

//交换最大牌型
func (glc *GameLogicBase) ExchangeMaxCard(userid int32) {
	mlog.Debug("用户%d交换最大牌型", userid)
	//牌局记录
	glc.GameLog.AddGameStep("用户%d交换最大牌型", userid)
}

//读取proto配置文件
func (glc *GameLogicBase) ReadProtoConfig(fName string, unmarshFunc func(data []byte) (proto.Message, error)) (proto.Message, error) {
	protoConfigFolderName := "protoconfig"

	if conf.TestMode {
		protoConfigFolderName = "../../protoconfig"
	}

	fpath := fmt.Sprintf("%s/%s", protoConfigFolderName, fName)
	fi, err := os.Open(fpath)

	if err != nil {
		return nil, err
	}
	defer fi.Close()

	fd, err := ioutil.ReadAll(fi)
	if err != nil {
		return nil, err
	}

	return unmarshFunc(fd)
}

func (glc *GameLogicBase) RefreshConfigGroup(congifs *config.ConfigGroup) {
	glc.Configs = congifs
	for _, c := range glc.Configs.Configs {
		for _, s := range c.Sections {
			for k, v := range s {
				mlog.Debug("reload k[%v] v[%v]", k, v)
			}
		}
	}
}

func (glc *GameLogicBase) checkMatchmaking(gameuser *GameUser) {
	if glc.GetServerConfig().MatchMakingNumber == 0 {
		return
	}
	var gameStart = func() {
		if glc.IsAllUserReady() && glc.TableUserCount() >= glc.GetServerConfig().MatchMakingNumber {
			glc.GameStartFunc()
		}
	}

	//X秒配對，到達Y人開始倒數，低於Y人重設計時器
	if !glc.isgaming {
		timername := fmt.Sprintf("CountDown_%d", glc.TableNo())
		if glc.IsAllUserReady() && glc.TableUserCount() >= glc.GetServerConfig().MatchMakingNumber {
			//檢查是否有正在倒數的計時器
			if tf := glc.TimerFuncManager.GetTimerFunc(timername); tf == nil {
				//開始倒數時間，倒數完成開始遊戲
				//mlog.System("開始倒數")
				glc.TimerFuncManager.NewNamedTimer(glc.GetServerConfig().MatchMakingTime, gameStart, timername)
				//glc.TimerFuncManager.NewNamedTimer(glc.GetServerConfig().MatchMakingTime, glc.GameStartFunc, timername)
				pmsg := new(msg.Message)
				pmsg.BClassID = int32(netproto.MessageBClassID_Game)
				pmsg.SClassID = int32(netproto.GameMessageClassID_GameBeginCountDownID)
				pdata := new(netproto.GameBeginCountDown)
				pdata.Time = proto.Int32(glc.GetServerConfig().MatchMakingTime)
				pmsg.MsgData = pdata

				glc.BoardcastMessage(pmsg)
			} else {
				time := glc.GetServerConfig().MatchMakingTime - tf.ElapsedSeconds()
				//mlog.System("玩家進入")
				pmsg := new(msg.Message)
				pmsg.BClassID = int32(netproto.MessageBClassID_Game)
				pmsg.SClassID = int32(netproto.GameMessageClassID_GameBeginCountDownID)
				pdata := new(netproto.GameBeginCountDown)
				pdata.Time = proto.Int32(time)
				pmsg.MsgData = pdata

				glc.SendMessageToUser(gameuser, pmsg)
			}

		} else {
			if tf := glc.TimerFuncManager.GetTimerFunc(timername); tf != nil {
				//低於兩個人 暫停計時器
				//mlog.System("計時器關閉")
				glc.TimerFuncManager.RemoveTimer(timername)
				pmsg := new(msg.Message)
				pmsg.BClassID = int32(netproto.MessageBClassID_Game)
				pmsg.SClassID = int32(netproto.GameMessageClassID_GameBeginCountDownStopID)
				glc.BoardcastMessage(pmsg)
			}
		}
	}
}
