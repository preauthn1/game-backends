package baselogic

import (
	"encoding/json"
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// OnRecvRPCMessage 接收DB Server回傳的Message訊息
func (logic *SlotLogicBase) OnRecvRPCMessage(m *msg.Message) {

	if m == nil {
		return
	}

	rm, ok := m.MsgData.(*rpcmsg.RPCMessage)
	if !ok {
		mlog.Error("接收RPC資訊錯誤, 訊息不為*rpcmsg.RPCMessage")
		return
	}

	mmbyte := rm.MsgData.([]byte)
	if len(mmbyte) == 0 {
		mlog.Warn("接收RPC資訊錯誤, MsgData為空")
		// return
	}

	switch m.SClassID {
	// 從DB獲得的Jackpot資訊
	case int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadRetID):
		logic.recvJackpotData(m)
	// 從DB獲得的遊戲歷程紀錄
	case int32(netproto.Slot_GameMessageClassID_Slot_GameLogReadRetID):
		logic.recvSlotGameLog(m)
	// 從DB獲得的free spin資訊
	case int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadRetID):
		logic.recvFreeSpinTimes(m)
	}

	if logic.OnRecvRPCMessageFunc != nil {
		logic.OnRecvRPCMessageFunc(m)
	}
}

// AddSlotGameHistoryLog 新增歷程紀錄
func (logic *SlotLogicBase) AddSlotGameHistoryLog(user *gamelogic.GameUser, gameType int32, logType int32, gameData string) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_GameLogAddID)

	data := new(netproto.Slot_MSG_GameLogAdd)
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
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_GameLogReadID)

	data := new(netproto.Slot_MSG_GameLogRead)

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

	defer logic.CheckWaitGroup()

	if logic.GameUser == nil {
		return
	}

	// 解析出來的detail
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_GameLogRecord{}
		return mm
	})

	detail := rmm.(*netproto.Slot_MSG_GameLogRecord)

	if detail.GetUserID() != logic.GameUser.UserID || detail.GetGameID() != logic.TableManager.Room.GameInfo.GameID {
		return
	}

	logic.SendGameLogMessageToUser(logic.GameUser, detail.GetUserID(), detail.GetGameID(), detail.GetPageCount(), detail.GetLogs())
}

// ReadJackpotPoolNoLink DFDC類型為非連線模式，每桌一池
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 30021 (Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)
func (logic *SlotLogicBase) ReadJackpotPoolNoLink() {

	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)

	data := new(netproto.Slot_MSG_JackpotRead)
	example.invalid = new(netproto.Slot_JackpotData)
	example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(logic.TableManager.Room.Flag)
	example.invalid.FlagID = proto.Int32(int32(FlagID))
	example.invalid.TableNo = proto.Int32(logic.TableManager.TableNo)
	example.invalid.Level = proto.Int32(slot.JackpotSingle.Int())

	//讀取資料不須填值
	example.invalid.Contribution = proto.Int64(0)
	example.invalid.TotalContribution = proto.Int64(0)
	example.invalid.AwardTotal = proto.Int64(0)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateJackpotPoolNoLink 更新獎池，所有數字都是DB幫你加好，只需要帶增加的量，獎池只有一個所以只更新Level 1
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 30023 (Slot_GameMessageClassID_Slot_JackpotNoLinkRecordID)
func (logic *SlotLogicBase) UpdateJackpotPoolNoLink(contribution, totalContribution, awardTotal int64, level slot.JackpotLevel) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkRecordID)

	data := new(netproto.Slot_MSG_JackpotRecord)

	detail := new(netproto.Slot_JackpotData)
	detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(logic.TableManager.Room.Flag)
	detail.FlagID = proto.Int32(int32(FlagID))
	detail.TableNo = proto.Int32(logic.TableManager.TableNo)

	detail.Contribution = proto.Int64(contribution)
	detail.TotalContribution = proto.Int64(totalContribution)
	detail.AwardTotal = proto.Int64(awardTotal)
	detail.Level = proto.Int32(level.Int())

	data.Records = append(data.Records, detail)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// recvJackpotData 接收db server回傳的Jackpot資訊，並整理至Logic內
func (logic *SlotLogicBase) recvJackpotData(m *msg.Message) {

	defer logic.CheckWaitGroup()

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.Slot_MSG_JackpotRecord)
	})

	data := rmm.(*netproto.Slot_MSG_JackpotRecord)

	for _, row := range data.Records {

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

// ReadFreeSpinTimes 讀取FreeSpin紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 30011 (Slot_GameMessageClassID_Slot_FreeGameReadID)
func (logic *SlotLogicBase) ReadFreeSpinTimes(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameReadID)

	data := new(netproto.Slot_MSG_FreeGameRead)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateFreeSpin FreeSpin存入DB
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 30013 (Slot_GameMessageClassID_Slot_FreeGameRecordID)
func (logic *SlotLogicBase) UpdateFreeSpin(user *gamelogic.GameUser) {

	if logic.FreeSpin == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_FreeGameRecordID)

	data := new(netproto.Slot_MSG_FreeGameRecord)
	example.invalid = new(netproto.Slot_FreeSpinDetail)
	example.invalid.UserID = proto.Int32(user.UserID)
	example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	example.invalid.FlagID = proto.Int32(int32(FlagID))

	example.invalid.OriginID = proto.Int64(int64(logic.FreeSpin.Origin.GetId()))
	example.invalid.FreeSpinGroup = proto.Int32(logic.FreeSpin.SymbolGroupID)
	example.invalid.Times = proto.Int32(int32(logic.FreeSpin.Count))

	example.invalid.ChooseRate = proto.Int32(int32(logic.FreeSpin.BetRate))
	example.invalid.UserBet = proto.Int64(logic.FreeSpin.BetMoney)

	example.invalid.WinMoney = proto.Int64(logic.FreeSpin.WinMoney)
	example.invalid.WinTimes = proto.Int32(int32(logic.FreeSpin.WinCount))
	example.invalid.IsCoupon = proto.Bool(logic.FreeSpin.IsCoupon)

	if logic.FreeSpin.FreeSpinExtAward != nil {
		b, _ := json.Marshal(logic.FreeSpin.FreeSpinExtAward)
		example.invalid.Data = proto.String(string(b))
	} else {
		example.invalid.Data = proto.String("")
	}

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// recvFreeSpinTimes 接收db server回傳的FreeSpin資訊，並整理至Logic內
func (logic *SlotLogicBase) recvFreeSpinTimes(m *msg.Message) {

	defer logic.CheckWaitGroup()

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_FreeGameRecord{
			Info: &netproto.Slot_FreeSpinDetail{},
		}
		return mm
	})

	detail := rmm.(*netproto.Slot_MSG_FreeGameRecord)

	if logic.GameUser == nil {
		return
	}

	if logic.GameUser.UserID != example.invalid.GetUserID() {
		return
	}

	if logic.TableManager.Room.GameInfo.GameID != example.invalid.GetGameID() {
		return
	}

	if logic.TableManager.Room.Flag != strconv.Itoa(int(example.invalid.GetFlagID())) {
		return
	}

	var freeTable *protodata.FreeSpinTable
	for _, table := range logic.FreeSpinTable {
		if int64(table.GetId()) == example.invalid.GetOriginID() {
			freeTable = table
			break
		}
	}

	if freeTable == nil {
		return
	}

	if example.invalid.GetTimes() == 0 {
		return
	}

	if example.invalid.GetUserBet() == 0 || example.invalid.GetChooseRate() == 0 {
		return
	}

	logic.FreeSpin = slot.NewFreeSpin(freeTable, logic.SlotSymbolDistribution, example.invalid.GetUserBet(), int64(example.invalid.GetChooseRate()), example.invalid.GetIsCoupon())
	logic.FreeSpin.Count = int(example.invalid.GetTimes())
	logic.FreeSpin.WinMoney = example.invalid.GetWinMoney()
	logic.FreeSpin.WinCount = int(example.invalid.GetWinTimes())

	if len(example.invalid.GetData()) > 0 {
		award := new(slot.FreeSpinExtAward)
		if err := json.Unmarshal([]byte(example.invalid.GetData()), award); err != nil {
			mlog.Warn("FreeSpin 解析額外資訊錯誤, %v", err)
			return
		}
		logic.FreeSpin.FreeSpinExtAward = award
	}
}

// CheckWaitGroup 若Logic有wait group存在，則跟wg說做好了
func (logic *SlotLogicBase) CheckWaitGroup() {
	if logic.WG == nil {
		return
	}
	logic.WG.Done()
}
