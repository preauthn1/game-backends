package candyhouse

import (
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

// OnRecvRPCMessage 接收DB Server回傳的Message訊息
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
	// BonusGame紀錄
	case int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameRetID):
		logic.recvBonusData(m)
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
	data.Level = proto.Int32(slot.JackpotSingle.Int())

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateJackpotPool 更新獎池，所有數字都是DB幫你加好，只需要帶增加的量，獎池只有一個所以只更新Level 1
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 58 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *Logic) UpdateJackpotPool(contribution, totalContribution, awardTotal int64, level slot.JackpotLevel) {
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

// ResetBonusGame BonusGame存入DB
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 13003 (TGW_GameMessageClassID_TGW_ResetBonusGameID)
func (logic *Logic) ResetBonusGame(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ResetBonusGameID)

	data := new(netproto.TGW_MSG_ResetBonusGame)
	data.UserID = proto.Int32(user.UserID)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// RecordBonusGame BonusGame存入DB
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 13004 (TGW_GameMessageClassID_TGW_RecordBonusGameID)
func (logic *Logic) RecordBonusGame(user *gamelogic.GameUser, bonusID, turnID, location int32, money int64) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_RecordBonusGameID)

	data := new(netproto.TGW_MSG_RecordBonusGame)
	data.UserID = proto.Int32(user.UserID)
	data.Records = &netproto.TGW_BonusGameRecord{
		ID:          proto.Int32(turnID),
		Location:    proto.Int32(location),
		RewareValue: proto.Int64(money),
		BonusID:     proto.Int32(bonusID),
	}

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadBonusGame 讀取BonusGame紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 13001 (TGW_GameMessageClassID_TGW_ReadBonusGameID)
func (logic *Logic) ReadBonusGame(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.TGW_GameMessageClassID_TGW_ReadBonusGameID)

	detail := new(netproto.TGW_MSG_ReadBonusGame)
	detail.UserID = proto.Int32(user.UserID)

	pmsg.MsgData = detail

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

// recvBonusData 接收db server回傳的BonusGame資訊，並整理至Logic內
func (logic *Logic) recvBonusData(m *msg.Message) {
	if logic.GameUser == nil {
		logic.BonusGame = nil
		return
	}

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.TGW_MSG_BonusGameRecord)
	})

	data := rmm.(*netproto.TGW_MSG_BonusGameRecord)

	if data.GetUserID() != logic.GameUser.UserID {
		logic.BonusGame = nil
		return
	}

	if len(data.GetRecords()) == 0 {
		logic.BonusGame = nil
		return
	}

	logic.BonusGame = new(BonusGame)
	logic.BonusGame.GameBetMoney = 0
	logic.BonusGame.GameCount = int32(len(data.GetRecords()))
	logic.BonusGame.WinMoney = 0
	logic.BonusGame.Process = make(map[int32]*BonusGameDetail)

	// 往回推算中獎的金額，下注金額，總遊戲次數
	var rate int64
	var groupID int32
	for _, record := range data.GetRecords() {
		if record.GetID() <= 0 {
			continue
		}

		detail := new(BonusGameDetail)
		detail.ID = record.GetID()
		detail.Location = record.GetLocation()

		// 找出原始BonusTable
		for _, origin := range logic.BonusTable {
			if origin.GetId() != record.GetBonusID() {
				continue
			}
			detail.origin = origin
			groupID = origin.GetGroupId()
		}

		if detail.origin == nil {
			continue
		}

		f, _ := strconv.ParseFloat(detail.origin.GetRewardValue(), 64)
		rate += int64(f)

		detail.Money = int64(f) * record.GetRewareValue()

		logic.BonusGame.Process[detail.ID] = detail
		logic.BonusGame.GameBetMoney = record.GetRewareValue()
	}

	if rate == 0 {
		logic.BonusGame = nil
		return
	}

	// 找出該bonus所屬的group，把該GroupID的BonusTable還原，以便產生後續選擇
	for _, origin := range logic.BonusTable {
		if origin.GetGroupId() != groupID {
			continue
		}
		logic.BonusGame.BonusTable = append(logic.BonusGame.BonusTable, origin)
	}

	logic.BonusGame.WinMoney = logic.BonusGame.GameBetMoney * rate
	// logic.BonusGame.GameBetMoney = total / rate
	logic.BonusGame.Size = logic.BonusGameSize
	logic.BonusGame.All = logic.BonusGame.generateLaftProcess(logic.BonusGameSize)

	if logic.BonusGame.IsFinished() {
		if logic.GameUser != nil {
			logic.ResetBonusGame(logic.GameUser)
		}
		logic.BonusGame = nil
	}
}

// recvSlotGameLog 接收db server回傳的SlotGameLog資訊，直接傳給Client
func (logic *Logic) recvSlotGameLog(m *msg.Message) {
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

	logic.SendGameLogMessage(logic.GameUser, detail.GetUserID(), detail.GetGameID(), detail.GetPageCount(), detail.GetSendHistoryLog())
}
