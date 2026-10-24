package gamelog

import (
	"fmt"
	"sync"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

// Check Type
const (
	Check_By_GameRound   = 1 // 游戏局数
	Check_By_SettleScore = 2 // 结算分数
	// Check_By_LastUpdateTime = 3 // 上次更新时间
)

type UpdateGameLogData struct {
	UUID          string
	TradeID       int64
	UserID        int32
	CurrentAmount int64
	UpdateAmount  int64
	TaxAmount     int64
	GameRound     int
	LogDesc       string
}

//基于满足设定条件更新数据库的牌局记录
type ConditonalDBGameLog struct {
	*gameLogBase
	gameid      int32
	serverid    int32
	tableno     int32
	rpcMsgQueue *rpcmsg.RPCMsgQueue

	// key: check type
	// value: check function
	dbUpdateChecker map[int](func(data *UpdateGameLogData) bool)

	// check used data
	usrLogData sync.Map // key: tradeid value:data

	setting map[string]interface{}
}

//创建基于满足设定条件更新数据库的牌局记录对象
func NewConditonalDBGameLog(rpcQueue *rpcmsg.RPCMsgQueue, gameid int32, serverid int32, tableno int32, setting map[string]interface{}) *ConditonalDBGameLog {
	dblog := new(ConditonalDBGameLog)
	dblog.rpcMsgQueue = rpcQueue
	dblog.gameid = gameid
	dblog.serverid = serverid
	dblog.tableno = tableno
	dblog.setting = setting
	dblog.dbUpdateChecker = map[int](func(data *UpdateGameLogData) bool){
		Check_By_GameRound:   dblog.checkGameRound,
		Check_By_SettleScore: dblog.checkSettleScore,
	}
	dblog.gameLogBase = NewGameLogBase()

	return dblog
}

func (l *ConditonalDBGameLog) EndGameLog(uscores map[int32]int64) {
	defer util.PanicErrStack()

	l.gameEndTime = time.Now()

	var tradeid int64 = 0
	var currentamount int64 = 0
	var updateamount int64 = 0
	for uid, ud := range l.gameUsers {
		currentamount = ud.Score
		if amt, ok := uscores[uid]; ok {
			updateamount += amt
		}

		tradeid = ud.TradeID
		var taxamount int64 = 0
		for _, amt := range uscores {
			taxamount += amt
		}
		// 是否满足DB更新条件
		isUpdateGamelog, data := l.processGameLog(tradeid, uid, currentamount, updateamount, taxamount, l.getGameLogDesc())
		if isUpdateGamelog {
			deallogmsg := new(msg.Message)
			deallogmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
			deallogmsg.SClassID = int32(netproto.DBServerClassID_DealLogID)

			deallogdata := new(netproto.DealLog)
			deallogdata.GameBeginTime = proto.String(util.FormatTimeStr(l.gameBeginTime))
			deallogdata.GameEndTime = proto.String(util.FormatTimeStr(l.gameEndTime))
			deallogdata.GameID = proto.Int32(l.gameid)
			deallogdata.ServerID = proto.Int32(l.serverid)
			deallogdata.TableNo = proto.Int32(l.tableno)
			if l.nextJiangRecord != nil {
				deallogdata.NextJiangRecord = l.nextJiangRecord
			}
			var urets string
			for uid, ud := range l.gameUsers {
				urets += fmt.Sprintf("%d:%d:%d:%d,", uid, data.CurrentAmount, data.UpdateAmount, ud.TradeID)
			}
			deallogdata.UserGameRet = proto.String(urets)
			deallogdata.GameData = proto.String(data.LogDesc)
			deallogdata.TaxAmount = proto.Int64(data.TaxAmount)
			deallogmsg.MsgData = deallogdata
			l.rpcMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(l.serverid), false, deallogmsg)
			l.resetData(data)
		}
	}
}

//从牌局记录清除gameuser
func (l *ConditonalDBGameLog) RemoveGameUsers(userID int32) {
	if ud, ok := l.gameUsers[userID]; ok {
		var tradeid = ud.TradeID
		var uuid = fmt.Sprintf("%d%d", tradeid, userID)
		if val, ok := l.usrLogData.Load(uuid); ok {
			var data = val.(*UpdateGameLogData)
			if data.GameRound > 0 {
				// 寫log
				deallogmsg := new(msg.Message)
				deallogmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
				deallogmsg.SClassID = int32(netproto.DBServerClassID_DealLogID)

				deallogdata := new(netproto.DealLog)
				deallogdata.GameBeginTime = proto.String(util.FormatTimeStr(l.gameBeginTime))
				deallogdata.GameEndTime = proto.String(util.FormatTimeStr(l.gameEndTime))
				deallogdata.GameID = proto.Int32(l.gameid)
				deallogdata.ServerID = proto.Int32(l.serverid)
				deallogdata.TableNo = proto.Int32(l.tableno)
				if l.nextJiangRecord != nil {
					deallogdata.NextJiangRecord = l.nextJiangRecord
				}
				var urets string
				for uid, ud := range l.gameUsers {
					urets += fmt.Sprintf("%d:%d:%d:%d,", uid, data.CurrentAmount, data.UpdateAmount, ud.TradeID)
				}
				deallogdata.UserGameRet = proto.String(urets)
				deallogdata.GameData = proto.String(data.LogDesc)
				deallogdata.TaxAmount = proto.Int64(data.TaxAmount)
				deallogmsg.MsgData = deallogdata
				l.rpcMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(l.serverid), false, deallogmsg)
				l.resetData(data)
			}
		}
	}

	l.gameLogBase.RemoveGameUsers(userID)
}

func (l *ConditonalDBGameLog) processGameLog(tradeid int64, userid int32, currentAmount int64, updateAmount int64, taxAmount int64, logDesc string) (bool, *UpdateGameLogData) {
	var logData *UpdateGameLogData
	var uuid = fmt.Sprintf("%d%d", tradeid, userid)
	if val, ok := l.usrLogData.Load(uuid); ok {
		logData = val.(*UpdateGameLogData)
		if logData.CurrentAmount == -1 {
			logData.CurrentAmount = currentAmount
		}
		logData.UpdateAmount += updateAmount
		logData.TaxAmount += taxAmount
		logData.GameRound++
		logData.LogDesc += logDesc
	} else {
		logData = new(UpdateGameLogData)
		logData.UUID = uuid
		logData.TradeID = tradeid
		logData.UserID = userid
		logData.CurrentAmount = currentAmount
		logData.UpdateAmount = updateAmount
		logData.TaxAmount = taxAmount
		logData.GameRound = 1
		logData.LogDesc = logDesc
	}
	l.usrLogData.Store(uuid, logData)

	// fmt.Println(logData)

	if logData != nil && l.isUpdateDB(logData) {
		return true, logData
	}
	return false, nil
}

func (l *ConditonalDBGameLog) isUpdateDB(data *UpdateGameLogData) bool {
	for _, checker := range l.dbUpdateChecker {
		if checker(data) {
			return true
		}
	}
	return false
}

func (l *ConditonalDBGameLog) resetData(data *UpdateGameLogData) {
	data.CurrentAmount = -1
	data.UpdateAmount = 0
	data.TaxAmount = 0
	data.GameRound = 0
	data.LogDesc = ""
	l.usrLogData.Store(data.UUID, data)
}

// checker
func (l *ConditonalDBGameLog) checkGameRound(data *UpdateGameLogData) bool {
	if data.GameRound >= l.setting["SettleRound"].(int) {
		return true
	}
	return false
}

func (l *ConditonalDBGameLog) checkSettleScore(data *UpdateGameLogData) bool {
	if data.UpdateAmount <= -l.setting["SettleScore"].(int64) || l.setting["SettleScore"].(int64) <= data.UpdateAmount {
		return true
	}
	return false
}
