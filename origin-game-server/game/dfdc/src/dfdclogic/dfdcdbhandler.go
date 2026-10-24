package dfdclogic

import (
	"msg"
	"netproto"
	"rpcmsg"
	"strconv"

	"example.invalid/golang/protobuf/proto"
)

//收到RPC消息解析處理
func (lh *DFDCLogic) OnRecvRPCMessage(m *msg.Message) {
	rm, ok := m.MsgData.(*rpcmsg.RPCMessage)
	if !ok {
		return
	}
	mmbyte := rm.MsgData.([]byte)
	if len(mmbyte) == 0 {
		return
	}

	//從DB獲得的Jackpot資訊
	if m.SClassID == int32(netproto.GameMessageClassID_SlotJackpotNoLinkRetID) {
		lh.getJackpotData(m)
	}

	//多福多財BonusGame紀錄
	if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID) {
		lh.getBonusGameRecord(m)
	}

	//從DB獲得的free spin資訊
	if m.SClassID == int32(netproto.GameMessageClassID_SlotFreeSpinDetailID) {
		lh.getFreeSpinTimes(m)
	}

	//從DB獲得的遊戲歷程紀錄
	if m.SClassID == int32(netproto.GameMessageClassID_SlotSendGameLogID) {
		lh.getHistoryLogFromDB(m)
	}
}

//多福多財BonusGame紀錄
func (lh *DFDCLogic) getJackpotData(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_JackpotAllData{}
		return mm
	})

	detail := rmm.(*netproto.Slot_JackpotAllData)

	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)

	for _, jpdetail := range detail.AllData {
		if jpdetail.GetTableNo() != lh.TableManager.TableNo ||
			jpdetail.GetGameID() != lh.TableManager.Room.GameInfo.GameID ||
			jpdetail.GetFlagID() != int32(FlagID) {
			continue
		}

		for _, jackpotDetail := range lh.jackpotdataDetail {
			if jackpotDetail.level != jpdetail.GetLevel() {
				continue
			}

			jackpotDetail.contribution = jpdetail.GetContribution()
			jackpotDetail.totalContribution = jpdetail.GetTotalContribution()
			jackpotDetail.awardTotal = jpdetail.GetAwardTotal()
		}
	}

}

//多福多財BonusGame紀錄
func (lh *DFDCLogic) getBonusGameRecord(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.DFDC_JackpotBonusGame{}
		return mm
	})
	detail := rmm.(*netproto.DFDC_JackpotBonusGame)
	bonusDt := false
	for _, bsdetail := range detail.BonusDetail {
		if bsdetail.GetIcon() != 0 {
			bonusDt = true
		}
	}
	if len(lh.userdata) > 0 && bonusDt {
		lh.bonusGameRecord = make([]*BonusRecord, 0)
		lh.bonusProcess = make([]int32, 0)
		for _, jpdetail := range detail.BonusDetail {
			if jpdetail.GetIcon() != 0 && jpdetail.GetUserID() == lh.userdata[0].user.UserID {
				for i := 1; i <= 9; i++ {
					if i == int(jpdetail.GetTurnID()) {
						lh.jpFlag = true
						bsRecord := new(BonusRecord)
						bsRecord.id = jpdetail.GetTurnID()
						bsRecord.place = jpdetail.GetPlace()
						bsRecord.icon = jpdetail.GetIcon()
						lh.bonusProcess = append(lh.bonusProcess, jpdetail.GetIcon())
						lh.bonusGameRecord = append(lh.bonusGameRecord, bsRecord)
						lh.jackpotAward = detail.GetMoney()
					}
				}
			}
		}
	}
}

//從DB獲得的free spin資訊
func (lh *DFDCLogic) getFreeSpinTimes(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_FreeSpinDetail{}
		return mm
	})
	detail := rmm.(*netproto.Slot_FreeSpinDetail)

	if len(lh.TableManager.GetTableUser()) > 0 {
		if detail.GetUserID() == lh.TableManager.GetTableUser()[0].UserID && detail.GetGameID() == lh.TableManager.Room.GameInfo.GameID && strconv.Itoa(int(detail.GetFlagID())) == lh.TableManager.Room.Flag {
			lh.freespinRecord.userBet = detail.GetUserBet()
			lh.freespinRecord.freeSpinGroup = detail.GetFreeSpinGroup()
			lh.freespinRecord.times = detail.GetTimes()
			lh.freespinRecord.chooseRate = detail.GetChooseRate()
			lh.freespinRecord.winMoney = detail.GetWinMoney()
			lh.isCoupon = detail.GetIsCoupon()
		}
	}
}

//從DB獲得的free spin資訊
func (lh *DFDCLogic) getHistoryLogFromDB(m *msg.Message) {
	//解析出來的detail
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_SendHistoryData{}
		return mm
	})
	detail := rmm.(*netproto.Slot_SendHistoryData)
	userID := detail.GetUserID()
	gameID := detail.GetGameID()
	pageCount := int32(0)
	historyLogMsg := make([]*netproto.Slot_SendHistoryLog, 0)
	if userID == lh.userdata[0].user.UserID && gameID == lh.TableManager.Room.GameInfo.GameID {
		for _, historyLog := range detail.SendHistoryLog {
			if historyLog.GetRowNum() == 0 {
				continue
			}
			pageCount = detail.GetPageCount()
			gameLogdt := new(netproto.Slot_SendHistoryLog)
			gameLogdt.RowNum = proto.Int32(historyLog.GetRowNum())
			gameLogdt.CreateTime = proto.String(historyLog.GetCreateTime())
			gameLogdt.OrderID = proto.Int64(historyLog.GetOrderID())
			gameLogdt.BetMoney = proto.Int64(historyLog.GetBetMoney())
			gameLogdt.Profit = proto.Int64(historyLog.GetProfit())
			historyLogMsg = append(historyLogMsg, gameLogdt)
		}
	}

	//收到資料後，直接轉傳給client
	//給client的封包
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCLoadGameHistoryRetID)
	pdata := new(netproto.Slot_SendHistoryData)
	pdata.UserID = proto.Int32(userID)
	pdata.GameID = proto.Int32(gameID)
	pdata.SendHistoryLog = historyLogMsg
	pdata.PageCount = proto.Int32(pageCount)
	pmsg.MsgData = pdata
	lh.BoardcastGameUserMessage(pmsg)
}
