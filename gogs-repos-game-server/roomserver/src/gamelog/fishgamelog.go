package gamelog

import (
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"msg"
	"netproto"
	"rpcmsg"
	"time"
	"util"
)

//捕魚遊戲記錄
type FishGameLog struct {
	*gameLogBase
	gameid      int32
	serverid    int32
	tableno     int32
	rpcMsgQueue *rpcmsg.RPCMsgQueue
}


func NewFishGameLog(rpcQueue *rpcmsg.RPCMsgQueue, gameid int32, serverid int32, tableno int32) *FishGameLog {
	flog := new(FishGameLog)
	flog.rpcMsgQueue = rpcQueue
	flog.gameid = gameid
	flog.serverid = serverid
	flog.tableno = tableno
	flog.gameLogBase = NewGameLogBase()

	return flog
}

func (f *FishGameLog) EndGameLog(uscores map[int32]int64, ucoupon map[int32]int64) {
	defer util.PanicErrStack()

	f.gameEndTime = time.Now()

	deallogmsg := new(msg.Message)
	deallogmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	deallogmsg.SClassID = int32(netproto.DBServerClassID_DealLogID)

	deallogdata := new(netproto.DealLog)
	deallogdata.GameBeginTime = proto.String(util.FormatTimeStr(f.gameBeginTime))
	deallogdata.GameEndTime = proto.String(util.FormatTimeStr(f.gameEndTime))
	deallogdata.GameID = proto.Int32(f.gameid)
	deallogdata.ServerID = proto.Int32(f.serverid)
	deallogdata.TableNo = proto.Int32(f.tableno)
	if f.nextJiangRecord != nil {
		deallogdata.NextJiangRecord = f.nextJiangRecord
	}

	urets := ""
	for uid, ud := range f.gameUsers {
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
	deallogdata.GameData = proto.String(f.getGameLogDesc())

	var taxamount int64 = 0
	for _, amt := range uscores {
		taxamount += amt
	}
	deallogdata.TaxAmount = proto.Int64(taxamount)
	deallogmsg.MsgData = deallogdata

	f.rpcMsgQueue.PushMessage(netproto.CreateEmptyRPCInfo(f.serverid), false, deallogmsg)
	f.DeleteGameUsers(uscores)
}

func (f *FishGameLog) AddGameStep(step *GameStep) {
	f.gameSteps = append(f.gameSteps, step)
}

func (f *FishGameLog) BeginGameLog(urs map[int32]*UserData) {
	f.gameBeginTime = time.Now()
	f.clearGameStep()
}

func (f *FishGameLog) UpdateGameUsers(urs map[int32]*UserData) {
	for userid, ur := range urs {
		if f.gameUsers[userid] == nil {
			f.gameUsers[userid] = ur
		}
	}
}

func (f *FishGameLog) DeleteGameUsers(uscores map[int32]int64) {
	for uid, _ := range f.gameUsers {
		if _, ok := uscores[uid]; ok {
			delete(f.gameUsers, uid)
		}
	}
}


