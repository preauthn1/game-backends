package gamelog

import (
	"fmt"
	"msg"
	"netproto"
	"rpcmsg"
	"time"
	"util"
)

import proto "example.invalid/golang/protobuf/proto"

//基于数据库的牌局记录
type DBGameLog struct {
	*gameLogBase
	gameid      int32
	serverid    int32
	tableno     int32
	rpcMsgQueue *rpcmsg.RPCMsgQueue
}

//创建基于数据库的牌局记录对象
func NewDBGameLog(rpcQueue *rpcmsg.RPCMsgQueue, gameid int32, serverid int32, tableno int32) *DBGameLog {
	dblog := new(DBGameLog)
	dblog.rpcMsgQueue = rpcQueue
	dblog.gameid = gameid
	dblog.serverid = serverid
	dblog.tableno = tableno
	dblog.gameLogBase = NewGameLogBase()

	return dblog
}

func (l *DBGameLog) EndGameLog(uscores map[int32]int64, ucoupon map[int32]int64) {
	defer util.PanicErrStack()

	l.gameEndTime = time.Now()

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

	urets := ""
	for uid, ud := range l.gameUsers {
		var uwin int64 = 0
		var cwin int64 = 0
		if amt, ok := uscores[uid]; ok {
			uwin = amt
		}
		if cmt, ok := ucoupon[uid]; ok {
			cwin = cmt
		}
		// 分别对应UserID,CurrentAmount,UpdateAmount,TradeID,CurrentCouponAmount,CouponAmount
		urets += fmt.Sprintf("%d:%d:%d:%d:%d:%d,", uid, ud.Score, uwin, ud.TradeID, ud.Coupon, cwin)
	}

	deallogdata.UserGameRet = proto.String(urets)
	deallogdata.GameData = proto.String(l.getGameLogDesc())

	var taxamount int64 = 0
	for _, amt := range uscores {
		taxamount += amt
	}
	deallogdata.TaxAmount = proto.Int64(taxamount)
	deallogmsg.MsgData = deallogdata

	l.rpcMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(l.serverid), false, deallogmsg)

}
