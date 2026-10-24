package baselogic

import (
	"gamelogic"
	"msg"
	"netproto"
	"rpcmsg"
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/server/pkg/slot"
)

// OnRecvRPCMessage 接收DB Server回傳的Message訊息
func (logic *SlotLogicBase) OnRecvRPCMessage(m *msg.Message) {

	if m == nil {
		return
	}

	rm, ok := m.MsgData.(*rpcmsg.RPCMessage)
	if !ok {
		return
	}

	mmbyte := rm.MsgData.([]byte)
	if len(mmbyte) == 0 {
		return
	}

	switch m.SClassID {
	// 從DB獲得的Jackpot資訊
	case int32(netproto.GameMessageClassID_SlotJackpotNoLinkRetID):
		logic.recvJackpotData(m)
	// 從DB獲得的遊戲歷程紀錄
	case int32(netproto.GameMessageClassID_SlotSendGameLogID):
		logic.recvSlotGameLog(m)
	}

}

// AddSlotGameHistoryLog 新增歷程紀錄
func (logic *SlotLogicBase) AddSlotGameHistoryLog(user *gamelogic.GameUser, gameType int32, logType int32, gameData string) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotAddGameLogID)

	data := new(netproto.Slot_HistoryLogData)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	data.ServerID = proto.Int32(logic.TableManager.Room.ServerID)

	data.CurrentAmount = proto.Int64(user.Score)
	data.CurrentCoupon = proto.Int64(user.Coupon)
	data.BetMoney = proto.Int64(logic.GamePayMoney)
	data.BetCoupon = proto.Int64(0)
	data.WinMoney = proto.Int64(logic.GameWinMoney)
	data.FinalScore = proto.Int64(user.Score + logic.GameWinMoney - logic.GamePayMoney)
	data.FinalCoupon = proto.Int64(user.Coupon)
	data.GameType = proto.Int32(gameType)
	data.LogType = proto.Int32(logType)
	data.GameData = proto.String(gameData)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadSlotGameHistoryLog 查詢歷程紀錄
func (logic *SlotLogicBase) ReadSlotGameHistoryLog(user *gamelogic.GameUser, page, size int32, start, end string) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotGetGameLogID)

	data := new(netproto.Slot_LoadHistoryData)

	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	data.StartTime = proto.String(start)
	data.EndTime = proto.String(end)
	data.PageIndex = proto.Int32(page)
	data.PageSize = proto.Int32(size)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// recvSlotGameLog 接收db server回傳的SlotGameLog資訊，直接傳給Client
func (logic *SlotLogicBase) recvSlotGameLog(m *msg.Message) {
	if logic.GameUser == nil {
		return
	}

	// 解析出來的detail
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_SendHistoryData{}
		return mm
	})

	detail := rmm.(*netproto.Slot_SendHistoryData)

	if detail.GetUserID() != logic.GameUser.UserID || detail.GetGameID() != logic.TableManager.Room.GameInfo.GameID {
		return
	}

	logic.SendGameLogMessageToUser(logic.GameUser, detail.GetUserID(), detail.GetGameID(), detail.GetPageCount(), detail.GetSendHistoryLog())
}

// ReadJackpotPoolNoLink DFDC類型為非連線模式，每桌一池
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 58 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *SlotLogicBase) ReadJackpotPoolNoLink() {

	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotJackpotNoLinkID)

	data := new(netproto.Slot_JackpotData)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(logic.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))
	data.TableNo = proto.Int32(logic.TableManager.TableNo)

	//讀取資料不須填值
	data.Contribution = proto.Int64(0)
	data.TotalContribution = proto.Int64(0)
	data.AwardTotal = proto.Int64(0)
	data.Level = proto.Int32(slot.JackpotSingle.Int())

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateJackpotPoolNoLink 更新獎池，所有數字都是DB幫你加好，只需要帶增加的量，獎池只有一個所以只更新Level 1
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 58 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *SlotLogicBase) UpdateJackpotPoolNoLink(contribution, totalContribution, awardTotal int64, level slot.JackpotLevel) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotJackpotNoLinkID)

	data := new(netproto.Slot_JackpotData)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(logic.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))
	data.TableNo = proto.Int32(logic.TableManager.TableNo)

	data.Contribution = proto.Int64(contribution)
	data.TotalContribution = proto.Int64(totalContribution)
	data.AwardTotal = proto.Int64(awardTotal)
	data.Level = proto.Int32(level.Int())

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// recvJackpotData 接收db server回傳的Jackpot資訊，並整理至Logic內
func (logic *SlotLogicBase) recvJackpotData(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.Slot_JackpotAllData)
	})

	data := rmm.(*netproto.Slot_JackpotAllData)

	for _, row := range data.AllData {

		if row.GetGameID() != logic.TableManager.Room.GameInfo.GameID {
			continue
		}

		if strconv.Itoa(int(row.GetFlagID())) != logic.TableManager.Room.Flag {
			continue
		}

		if row.GetTableNo() != logic.TableManager.TableNo {
			continue
		}

		detail, exist := logic.Jackpots[slot.NewJackpotLevel(row.GetLevel())]

		if !exist {
			continue
		}

		detail.Contribution = row.GetContribution()
		detail.TotalContribution = row.GetTotalContribution()
		detail.AwardTotal = row.GetAwardTotal()

		logic.Jackpots[detail.Level] = detail
	}
}
