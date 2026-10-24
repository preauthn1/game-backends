package dfdc

import (
	"fmt"
	"gamelogic"
	"msg"
	"netproto"
	"rpcmsg"
	"strconv"
	"strings"

	"example.invalid/golang/protobuf/proto"
)

// OnRecvRPCMessage 接收DB Server回傳的Message訊息
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 58 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *Logic) OnRecvRPCMessage(m *msg.Message) {

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
	// 多福多財BonusGame紀錄
	case int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID):
		logic.recvBonusData(m)
	// 從DB獲得的free spin資訊
	case int32(netproto.GameMessageClassID_SlotFreeSpinDetailID):
		logic.recvFreeSpinTimes(m)
	// 從DB獲得的遊戲歷程紀錄
	case int32(netproto.GameMessageClassID_SlotSendGameLogID):
		logic.recvSlotGameLog(m)
	}

}

// ReadJackpotPool DFDC類型為非連線模式，每桌一池
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 58 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *Logic) ReadJackpotPool() {

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
	data.Level = proto.Int32(1)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateJackpotPool 更新獎池，所有數字都是DB幫你加好，只需要帶增加的量
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 58 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *Logic) UpdateJackpotPool(contribution, totalContribution, awardTotal int64, level int32) {
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
	data.Level = proto.Int32(level)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ResetBonusGame BonusGame存入DB
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10016 (DFDC_GameMessageClassID_DFDCJackpotBonusGameID)
func (logic *Logic) ResetBonusGame(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID)

	data := new(netproto.DFDC_JackpotBonusGameRecord)
	data.UserID = proto.Int32(user.UserID)
	data.ChvRecordStr = proto.String("1,0,0|2,0,0|3,0,0|4,0,0|5,0,0|6,0,0|7,0,0|8,0,0|9,0,0")
	data.Winmoney = proto.Int64(0)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateBonusGame BonusGame存入DB
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10016 (DFDC_GameMessageClassID_DFDCJackpotBonusGameID)
func (logic *Logic) UpdateBonusGame(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID)

	data := new(netproto.DFDC_JackpotBonusGameRecord)
	data.UserID = proto.Int32(user.UserID)

	// 最多9次
	strs := make([]string, 0, 9)

	for i := 0; i < 9; i++ {
		if detail, ok := logic.JackpotGame.Process[int32(i+1)]; ok {
			strs = append(strs, fmt.Sprintf("%v,%v,%v", detail.ID, detail.Place, detail.Icon))
		} else {
			strs = append(strs, fmt.Sprintf("%v,%v,%v", i+1, 0, 0))
		}
	}

	data.ChvRecordStr = proto.String(strings.Join(strs, "|"))
	data.Winmoney = proto.Int64(logic.JackpotGame.WinMoney)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadBonusGame 讀取BonusGame紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10017 (DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID)
func (logic *Logic) ReadBonusGame(user *gamelogic.GameUser) {

	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID)

	detail := new(netproto.DFDC_JackpotBonusGameRet)
	detail.UserID = proto.Int32(user.UserID)

	pmsg.MsgData = detail

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateFreeSpin FreeSpin存入DB
func (logic *Logic) UpdateFreeSpin(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotFreeSpinID)

	data := new(netproto.Slot_FreeSpinDetail)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	data.FreeSpinGroup = proto.Int32(logic.FreeSpinGroup)
	data.Times = proto.Int32(logic.FreeSpinCount)

	data.ChooseRate = proto.Int32(logic.GameBetRate)
	data.UserBet = proto.Int64(logic.GameBetMoney)

	data.WinMoney = proto.Int64(logic.FreeSpinWinMoney)
	data.IsCoupon = proto.Bool(logic.GameIsCoupon)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadFreeSpinTimes 讀取FreeSpin紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 56 (GameMessageClassID_SlotFreeSpinRetID)
func (logic *Logic) ReadFreeSpinTimes(user *gamelogic.GameUser) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SlotFreeSpinRetID)

	data := new(netproto.Slot_FreeSpinRead)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// AddSlotGameHistoryLog 新增歷程紀錄
func (logic *Logic) AddSlotGameHistoryLog(user *gamelogic.GameUser, gameType int32, logType int32, gameData string) {

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
	data.BetCoupon = proto.Int64(logic.GamePayCoupon)
	data.WinMoney = proto.Int64(logic.GameWinMoney)
	data.FinalScore = proto.Int64(user.Score + logic.GameWinMoney - logic.GamePayMoney)
	data.FinalCoupon = proto.Int64(user.Coupon - logic.GamePayCoupon)
	data.GameType = proto.Int32(gameType)
	data.LogType = proto.Int32(logType)
	data.GameData = proto.String(gameData)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadSlotGameHistoryLog 查詢歷程紀錄
func (logic *Logic) ReadSlotGameHistoryLog(user *gamelogic.GameUser, page, size int32, start, end string) {

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

// recvJackpotData 接收db server回傳的Jackpot資訊，並整理至Logic內
func (logic *Logic) recvJackpotData(m *msg.Message) {
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

		detail, exist := logic.Jackpots[NewJackpotLevel(row.GetLevel())]

		if !exist {
			continue
		}

		detail.Contribution = row.GetContribution()
		detail.TotalContribution = row.GetTotalContribution()
		detail.AwardTotal = row.GetAwardTotal()

		logic.Jackpots[detail.Level] = detail
	}

	// 傳送Jackpot資訊
	// logic.SendJackpotPoolMessage(logic.GameUser)
}

// recvBonusData 接收db server回傳的BonusGame資訊，並整理至Logic內
func (logic *Logic) recvBonusData(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.DFDC_JackpotBonusGame)
	})

	data := rmm.(*netproto.DFDC_JackpotBonusGame)

	if len(data.GetBonusDetail()) == 0 {
		return
	}

	var hasData bool
	for _, row := range data.GetBonusDetail() {
		if row.GetIcon() == 0 {
			continue
		}
		hasData = true
	}
	if !hasData {
		return
	}

	if logic.JackpotGame == nil {
		logic.JackpotGame = new(JackpotGame)
	}

	logic.JackpotGame.WinMoney = data.GetMoney()
	logic.JackpotGame.Process = make(map[int32]*JackpotGameDetail)

	for _, row := range data.GetBonusDetail() {
		if row.GetIcon() == 0 {
			continue
		}
		detail := &JackpotGameDetail{
			ID:    row.GetTurnID(),
			Place: row.GetPlace(),
			Icon:  row.GetIcon(),
		}

		logic.JackpotGame.Process[detail.ID] = detail
	}
}

// recvFreeSpinTimes 接收db server回傳的FreeSpin資訊，並整理至Logic內
func (logic *Logic) recvFreeSpinTimes(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_FreeSpinDetail{}
		return mm
	})

	detail := rmm.(*netproto.Slot_FreeSpinDetail)

	if logic.GameUser == nil {
		return
	}

	if logic.GameUser.UserID != detail.GetUserID() {
		return
	}

	if logic.TableManager.Room.GameInfo.GameID != detail.GetGameID() {
		return
	}

	if logic.TableManager.Room.Flag != strconv.Itoa(int(detail.GetFlagID())) {
		return
	}

	logic.FreeSpinGroup = detail.GetFreeSpinGroup()
	logic.GameBetMoney = detail.GetUserBet()
	logic.GameBetRate = detail.GetChooseRate()
	logic.FreeSpinCount = detail.GetTimes()
	logic.FreeSpinWinMoney = detail.GetWinMoney()
	logic.GameIsCoupon = detail.GetIsCoupon()

	if logic.FreeSpinCount > 0 {
		logic.UpdateFreeSpin(logic.GameUser)
	}

}

// recvSlotGameLog 接收db server回傳的SlotGameLog資訊，直接傳給Client
func (logic *Logic) recvSlotGameLog(m *msg.Message) {
	// 解析出來的detail
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_SendHistoryData{}
		return mm
	})

	if logic.GameUser == nil {
		return
	}

	detail := rmm.(*netproto.Slot_SendHistoryData)

	if detail.GetUserID() != logic.GameUser.UserID || detail.GetGameID() != logic.TableManager.Room.GameInfo.GameID {
		return
	}

	logic.SendGameLogMessage(logic.GameUser, detail.GetUserID(), detail.GetGameID(), detail.GetPageCount(), detail.GetSendHistoryLog())
}
