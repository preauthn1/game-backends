package scorestorage

import (
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//即时写分
type NowUserScoreStorage struct {
	room       *gamelogic.Room     //房间信息
	dbMsgQueue *rpcmsg.RPCMsgQueue //数据库远程消息队列
	rpcInfo    *netproto.RPCInfo
}

//创建即时写分对象
func NewNowUserScoreStorage(room *gamelogic.Room, dbMsgQueue *rpcmsg.RPCMsgQueue) *NowUserScoreStorage {
	uss := new(NowUserScoreStorage)
	uss.room = room
	uss.dbMsgQueue = dbMsgQueue
	uss.rpcInfo = netproto.CreateEmptyRPCInfo(uss.room.ServerID)

	return uss
}

// 寫分 : 用戶資料, 加減分數, 稅額
func (uss *NowUserScoreStorage) UpdateUserScore(user *gamelogic.GameUser, score int64, tax int64) {
	if score == 0 && tax == 0 {
		return
	}

	user.Score += score
	user.UpdateScore += score
	user.TaxAmount += tax

	updateScoreMsg := new(msg.Message)
	updateScoreMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	updateScoreMsg.SClassID = int32(netproto.DBServerClassID_UpdateUserScoreID)

	updateScoreData := new(netproto.UpdateUserScore)
	updateScoreData.UserID = proto.Int32(user.UserID)
	updateScoreData.Score = proto.Int64(score)
	updateScoreData.ServerID = proto.Int32(uss.room.ServerID)
	updateScoreData.TaxAmount = proto.Int64(tax)
	// updateScoreData.CouponAmount = proto.Int64(coupon)

	updateScoreMsg.MsgData = updateScoreData

	uss.dbMsgQueue.PushMessage(uss.rpcInfo, true, updateScoreMsg)
	//uss.afterUpdateUserScore(user)
}

func (uss *NowUserScoreStorage) UpdateExperienceScore(user *gamelogic.GameUser, score int64, tax int64) {
	updateScoreMsg := new(msg.Message)
	updateScoreMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	updateScoreMsg.SClassID = int32(netproto.DBServerClassID_UpdateExperenceScore)

	updateScoreData := new(netproto.UpdateUserScore)
	updateScoreData.UserID = proto.Int32(user.UserID)
	updateScoreData.Score = proto.Int64(score)
	updateScoreData.ServerID = proto.Int32(uss.room.ServerID)
	updateScoreData.TaxAmount = proto.Int64(tax)
	// updateScoreData.CouponAmount = proto.Int64(coupon)

	updateScoreMsg.MsgData = updateScoreData

	uss.dbMsgQueue.PushMessage(uss.rpcInfo, true, updateScoreMsg)
}

//遊戲獎金池寫分
func (uss *NowUserScoreStorage) UpdateGameJackpotPool(user *gamelogic.GameUser, amount int64) {
	if amount <= 0 {
		return
	}
	updateScoreMsg := new(msg.Message)
	updateScoreMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	updateScoreMsg.SClassID = int32(netproto.DBServerClassID_UpdateGameJackpotScore)

	updateScoreData := new(netproto.UpdateUserScore)
	updateScoreData.UserID = proto.Int32(user.UserID)
	updateScoreData.Score = proto.Int64(amount)
	updateScoreData.ServerID = proto.Int32(uss.room.ServerID)
	updateScoreData.TaxAmount = proto.Int64(0)

	updateScoreMsg.MsgData = updateScoreData

	uss.dbMsgQueue.PushMessage(uss.rpcInfo, true, updateScoreMsg)
}

func (uss *NowUserScoreStorage) OnUserLeave(user *gamelogic.GameUser) {
	logoutRoomMsg := new(msg.Message)
	logoutRoomMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	logoutRoomMsg.SClassID = int32(netproto.DBServerClassID_LogoutRoomID)

	logoutRoomData := new(netproto.LogoutGameRoomInfo)
	logoutRoomData.UserID = proto.Int32(user.UserID)
	logoutRoomData.UpdateAmount = proto.Int64(user.UpdateScore)
	logoutRoomData.TaxAmount = proto.Int64(user.TaxAmount)
	logoutRoomData.WinCount = proto.Int32(user.UpdateWinCount)
	logoutRoomData.LoseCount = proto.Int32(user.UpdateLoseCount)
	logoutRoomData.LastWinLose = proto.Int32(user.LastWinLose)
	logoutRoomData.DrawCount = proto.Int32(user.UpdateDrawCount)
	logoutRoomData.LoginTime = proto.String(util.FormatTimeStr(user.LoginTime))
	logoutRoomData.LogoutTime = proto.String(util.FormatTimeStr(time.Now()))
	logoutRoomData.FlowAmount = proto.Int64(user.FlowAmount)
	// logoutRoomData.CouponAmount = proto.Int64(user.UpdateCoupon) // 應該要寫進入到離開使用的Coupon量
	//登录成功后马上断开的情况
	if user.ConnInfo != nil {
		logoutRoomData.IPAddress = proto.String(user.ConnInfo.IPAddress)
	} else {
		logoutRoomData.IPAddress = proto.String("unknown")
	}
	logoutRoomData.HDCode = proto.String(user.HDCode)
	logoutRoomData.HDType = proto.Int32(user.HDType)
	logoutRoomData.GameTime = proto.Int32(user.GameTime)
	logoutRoomData.GameID = proto.Int32(uss.room.GameInfo.GameID)
	logoutRoomData.ServerID = proto.Int32(uss.room.ServerID)
	logoutRoomData.LoginMoney = proto.Int64(user.LoginScore)
	logoutRoomData.ScoreUpdateMode = proto.Int32(gamelogic.ScoreUpdateMode_Now)
	logoutRoomData.TradeId = proto.Int64(user.TradeID)
	logoutRoomMsg.MsgData = logoutRoomData

	uss.dbMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(uss.room.ServerID), true, logoutRoomMsg)
}

// 斷線重連時會呼叫此函式記錄斷線重連時的分數異動
func (uss *NowUserScoreStorage) OnUserRecover(user *gamelogic.GameUser, dbscore int64) {
	//changeScore := dbscore - user.Score
	//user.InGamingMoneyChange = changeScore
}

// 即時到帳, 有可能一局中要Add兩次以上
func (uss *NowUserScoreStorage) OnUserMoneyAdd(user *gamelogic.GameUser, changeScore int64) {
	user.InGamingMoneyChange += changeScore
}

// 在遊戲結束時，每個用戶都會做的事
func (uss *NowUserScoreStorage) OnUserReady(user *gamelogic.GameUser) {
	uss.beforeGameStart(user)
}

//通知用户状态改变
func (uss *NowUserScoreStorage) notifyUserStateChange(user *gamelogic.GameUser) {
	if user.IsOffLine() {
		return
	}
	//如果用户是机器人
	if user.IsRobot() {
		return
	}

	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Game)
	m.SClassID = int32(netproto.GameMessageClassID_UserStateChangeID)

	ust := new(netproto.UserStateChange)
	ust.User = user.GetProtoBufGameUser()

	m.MsgData = ust

	uss.room.RoomUserManager.SendMessage(user.ConnInfo, m)
}

//正在遊戲中有需要異動金額的部份等到下局開始時才生效
func (uss *NowUserScoreStorage) beforeGameStart(user *gamelogic.GameUser) {
	if user.InGamingMoneyChange != 0 {
		//msgMoney := user.InGamingMoneyChange
		user.Score += user.InGamingMoneyChange
		user.LoginScore += user.InGamingMoneyChange
		user.InGamingMoneyChange = 0
		if user.IsOffLine() {
			//用戶不在線，回寫資料表?
			return
		}
		//msg, err := roomutil.GetTipMessage(fmt.Sprintf("亲爱的玩家:\n您充值的金币:%d已到帐\n( 如有任何疑问请联系客服)", msgMoney / 100), roomutil.MsgTypeID_TIP)
		//if err != nil {
		//	mlog.Error("打包客戶端消息失敗 %v", err)
		//	return
		//}
		//uss.room.RoomUserManager.SendMessage(user.ConnInfo, msg)

		uss.notifyUserStateChange(user)
	}
}
