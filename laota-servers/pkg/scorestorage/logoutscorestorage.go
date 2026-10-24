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

//退房间写分
type LogoutUserScoreStorage struct {
	room       *gamelogic.Room     //房间信息
	dbMsgQueue *rpcmsg.RPCMsgQueue //数据库远程消息队列
}

//创建退房间写分对象
func NewLogoutUserScoreStorage(room *gamelogic.Room, dbMsgQueue *rpcmsg.RPCMsgQueue) *LogoutUserScoreStorage {
	uss := new(LogoutUserScoreStorage)
	uss.room = room
	uss.dbMsgQueue = dbMsgQueue

	return uss
}

//写分
func (uss *LogoutUserScoreStorage) UpdateUserScore(user *gamelogic.GameUser, score int64, tax int64) {
	user.Score += score
	user.UpdateScore += score
	user.TaxAmount += tax
	// user.Coupon += coupon
	// user.UpdateCoupon += coupon
}

//體驗金幣寫分
func (uss *LogoutUserScoreStorage) UpdateExperienceScore(user *gamelogic.GameUser, score int64, tax int64) {
	user.ExperienceCash += score
}

//遊戲獎金池寫分
func (uss *LogoutUserScoreStorage) UpdateGameJackpotPool(user *gamelogic.GameUser, amount int64) {
	if amount <= 0 {
		return
	}

}

//登出房间
func (uss *LogoutUserScoreStorage) OnUserLeave(user *gamelogic.GameUser) {
	logoutRoomMsg := new(msg.Message)
	logoutRoomMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	logoutRoomMsg.SClassID = int32(netproto.DBServerClassID_LogoutRoomID)

	logoutRoomData := new(netproto.LogoutGameRoomInfo)
	logoutRoomData.UserID = proto.Int32(user.UserID)
	logoutRoomData.UpdateAmount = proto.Int64(user.UpdateScore)
	logoutRoomData.TaxAmount = proto.Int64(user.TaxAmount)
	logoutRoomData.WinCount = proto.Int32(user.UpdateWinCount)
	logoutRoomData.LoseCount = proto.Int32(user.UpdateLoseCount)
	logoutRoomData.DrawCount = proto.Int32(user.UpdateDrawCount)
	logoutRoomData.LoginTime = proto.String(util.FormatTimeStr(user.LoginTime))
	logoutRoomData.LogoutTime = proto.String(util.FormatTimeStr(time.Now()))
	logoutRoomData.IPAddress = proto.String(user.ConnInfo.IPAddress)
	logoutRoomData.HDCode = proto.String(user.HDCode)
	logoutRoomData.HDType = proto.Int32(user.HDType)
	logoutRoomData.GameTime = proto.Int32(user.GameTime)
	logoutRoomData.GameID = proto.Int32(uss.room.GameInfo.GameID)
	logoutRoomData.ServerID = proto.Int32(uss.room.ServerID)
	logoutRoomData.LoginMoney = proto.Int64(user.LoginScore)
	logoutRoomData.ScoreUpdateMode = proto.Int32(gamelogic.ScoreUpdateMode_Logout)
	logoutRoomData.TradeId = proto.Int64(user.TradeID)
	// logoutRoomData.CouponAmount = proto.Int64(user.UpdateCoupon)

	logoutRoomMsg.MsgData = logoutRoomData

	uss.dbMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(uss.room.ServerID), true, logoutRoomMsg)

}

func (uss *LogoutUserScoreStorage) OnUserRecover(user *gamelogic.GameUser, dbscore int64) {
	user.Score = dbscore + user.UpdateScore
}

func (uss *LogoutUserScoreStorage) OnUserMoneyAdd(user *gamelogic.GameUser, changeScore int64) {
	user.InGamingMoneyChange += changeScore
}

// 在遊戲結束時，每個用戶都會做的事
func (uss *LogoutUserScoreStorage) OnUserReady(user *gamelogic.GameUser) {
}
