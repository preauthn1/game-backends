package jjbx

import (
	"encoding/json"
	"sort"
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

// OnRecvRPCMessageFunc 接收DB資訊
func (logic *Logic) OnRecvRPCMessageFunc(m *msg.Message) {

	switch m.SClassID {
	// 從DB獲得的LightningGame資訊
	case int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadRetID):
		logic.recvLightningData(m)
	// 從DB獲得的SelectGame資訊
	case int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadRetID):
		logic.recvSelectGameTimes(m)
	// 從DB獲得的JackpotGame資訊
	case int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadRetID):
		logic.recvJackpotGame(m)
	}

}

// RecordLightningData 更新LightningGame
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 13003 (XMBX_GameMessageClassID_XMBX_LightningRecordID)
func (logic *Logic) RecordLightningData(user *gamelogic.GameUser) {

	if logic.LightningGame == nil {
		return
	}

	b, err := json.Marshal(logic.LightningGame)
	if err != nil {
		mlog.Error("封存LightningGame json.Marshal錯誤, %v", err)
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningRecordID)

	data := new(netproto.Slot_MSG_LightningRecord)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))
	data.Data = proto.String(string(b))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ResetLightningData 更新LightningGame
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 13003 (XMBX_GameMessageClassID_XMBX_LightningRecordID)
func (logic *Logic) ResetLightningData(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningRecordID)

	data := new(netproto.Slot_MSG_LightningRecord)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))
	data.Data = proto.String("")

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadLightningData 讀取LightningGame
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 13001 (XMBX_GameMessageClassID_XMBX_LightningReadID)
func (logic *Logic) ReadLightningData(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.XMBX_GameMessageClassID_XMBX_LightningReadID)

	data := new(netproto.Slot_MSG_LightningRead)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

func (logic *Logic) recvLightningData(m *msg.Message) {

	defer logic.CheckWaitGroup()

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_LightningRecord{}
		return mm
	})

	detail := rmm.(*netproto.Slot_MSG_LightningRecord)

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

	if len(detail.GetData()) == 0 {
		example.invalid("無Lightning資訊")
		return
	}

	b := []byte(detail.GetData())
	lg := new(LightningGame)

	if err := json.Unmarshal(b, lg); err != nil {
		mlog.Error("解析LightningGame錯誤, %v", err)
		return
	}

	lg.Logic = logic
	for _, step := range lg.Steps {
		var respin *protodata.RespinTable
		var rule *protodata.SlotPayTable

		for _, row := range logic.RespinTable {
			if int32(row.GetId()) == step.RespinID {
				respin = row
			}
		}

		for _, row := range logic.SlotPayTable {
			if int32(row.GetId()) == step.RuleID {
				rule = row
			}
		}

		step.Origin = respin
		step.Rule = rule
	}

	for _, row := range logic.RespinTable {
		if int32(row.GetId()) == lg.OriginID {
			lg.Origin = row
		}
	}

	logic.LightningGame = lg
}

// RecordSelectGameTimes 更新SelectGame
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 30003 (Slot_GameMessageClassID_Slot_SelectGameRecordID)
func (logic *Logic) RecordSelectGameTimes(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameRecordID)

	data := new(netproto.Slot_MSG_SelectGameRecord)
	example.invalid = new(netproto.Slot_SelectGameInfo)
	example.invalid.UserID = proto.Int32(user.UserID)
	example.invalid.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	example.invalid.FlagID = proto.Int32(int32(FlagID))

	if len(logic.SelectChances) > 0 {
		sc := logic.SelectChances[0]
		example.invalid.OriginID = proto.Int32(sc.Rule.GetId())
		example.invalid.Times = proto.Int32(int32(len(logic.SelectChances)))

		b, _ := json.Marshal(sc)
		example.invalid.Data = proto.String(string(b))
	} else {
		example.invalid.Times = proto.Int32(0)
		example.invalid.OriginID = proto.Int32(0)
		example.invalid.Data = proto.String("")
	}

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// ReadSelectGameTimes 讀取SelectGame
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 30001 (Slot_GameMessageClassID_Slot_SelectGameReadID)
func (logic *Logic) ReadSelectGameTimes(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SelectGameReadID)

	data := new(netproto.Slot_MSG_SelectGameRead)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

func (logic *Logic) recvSelectGameTimes(m *msg.Message) {

	defer logic.CheckWaitGroup()

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_SelectGameRecord{
			Info: &netproto.Slot_SelectGameInfo{},
		}
		return mm
	})

	detail := rmm.(*netproto.Slot_MSG_SelectGameRecord)

	if logic.GameUser == nil {
		return
	}

	if example.invalid == nil {
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

	if example.invalid.GetOriginID() == 0 || example.invalid.GetTimes() == 0 {
		return
	}

	var rule *protodata.SlotPayTable
	for _, row := range logic.SlotPayTable {
		if row.GetId() == example.invalid.GetOriginID() {
			rule = row
			break
		}
	}

	if rule == nil {
		return
	}

	if len(example.invalid.GetData()) == 0 {
		return
	}

	data := new(SelectChance)

	if err := json.Unmarshal([]byte(example.invalid.GetData()), data); err != nil {
		mlog.Error("解析選擇機會資料錯誤: %v", err)
		return
	}

	sc, err := NewSelectChance(logic, data.GameResultManager, rule, data.BetMoney, data.BetRate)
	if err != nil {
		mlog.Error("建立選擇機會錯誤: %v", err)
		return
	}
	sc.Data = data.Data

	logic.SelectChances = append(logic.SelectChances, sc)
}

// ReadJackpotGame 讀取JackpotGame, 使用BonusGame
func (logic *Logic) ReadJackpotGame(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BonusGameReadID)

	data := new(netproto.Slot_MSG_BonusGameRead)
	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// RecordJackpotGame 紀錄JackpotGame, 使用BonusGame
func (logic *Logic) RecordJackpotGame(user *gamelogic.GameUser) {

	if logic.JackpotGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BonusGameRecordID)

	record := new(netproto.Slot_MSG_BonusGameRecord)

	for _, row := range logic.JackpotGame.Process {
		detail := new(netproto.Slot_BonusGameDetail)
		detail.UserID = proto.Int32(user.UserID)
		detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
		detail.FlagID = proto.Int32(int32(FlagID))

		detail.OriginID = proto.Int32(logic.JackpotGame.Origin.GetId())
		detail.TurnID = proto.Int32(row.ID)
		detail.Location = proto.Int32(row.Place)
		detail.BetMoney = proto.Int32(int32(logic.GameBetMoney))
		detail.BetRate = proto.Int32(int32(logic.GameBetRate))
		detail.Data = proto.String(slot.NewJackpotLevel(row.Icon).String())

		record.Detail = append(record.Detail, detail)
	}

	sort.Slice(record.Detail, func(i, j int) bool {
		return record.Detail[i].GetTurnID() < record.Detail[j].GetTurnID()
	})

	pmsg.MsgData = record

	logic.SendRPCMessageFunc(true, pmsg)
}

// RestJackpotGame 重置, 使用BonusGame
func (logic *Logic) RestJackpotGame(user *gamelogic.GameUser) {

	if logic.JackpotGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BonusGameResetID)

	data := new(netproto.Slot_MSG_BonusGameRead)

	data.UserID = proto.Int32(user.UserID)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
	data.FlagID = proto.Int32(int32(FlagID))

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

func (logic *Logic) recvJackpotGame(m *msg.Message) {

	defer logic.CheckWaitGroup()

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_MSG_BonusGameRecord{}
		return mm
	})

	rec := rmm.(*netproto.Slot_MSG_BonusGameRecord)

	if logic.GameUser == nil {
		return
	}

	if len(rec.Detail) == 0 {
		return
	}

	rules := make(map[int32]*protodata.SlotPayTable)
	datas := make(map[int32][]*netproto.Slot_BonusGameDetail)

	for _, detail := range rec.Detail {
		if datas[detail.GetOriginID()] == nil {
			datas[detail.GetOriginID()] = make([]*netproto.Slot_BonusGameDetail, 0)
		}

		datas[detail.GetOriginID()] = append(datas[detail.GetOriginID()], detail)
	}

	for originID := range datas {
		sort.Slice(datas[originID], func(i, j int) bool {
			return datas[originID][i].GetTurnID() < datas[originID][j].GetTurnID()
		})

		for _, r := range logic.SlotPayTable {
			if r.GetId() == originID {
				rules[originID] = r
				break
			}
		}
	}

	for originID, rule := range rules {
		switch slot.PayType(rule.GetPayType()) {
		case slot.PayTypeMysteryJackpot:
			tmp := datas[originID][0]
			jackpots := make([]*slot.Jackpot, 0, len(logic.Jackpots))
			for _, jp := range logic.Jackpots {
				jackpots = append(jackpots, jp)
			}
			logic.JackpotGame = slot.NewJackpotGame(jackpots, logic.MaxBetRate, logic.MaxBetCount, tmp.GetBetRate(), int64(tmp.GetBetMoney()))
			logic.JackpotGame.Origin = rule

			logic.JackpotGame.Process = make(map[int32]*slot.JackpotGameDetail)

			jpMap := make(map[slot.JackpotLevel]int)

			for _, detail := range datas[originID] {
				ps := new(slot.JackpotGameDetail)
				ps.ID = detail.GetTurnID()
				ps.Place = detail.GetLocation()
				ps.Icon = slot.JackpotLevel(detail.GetData()).Int()

				logic.JackpotGame.Process[detail.GetTurnID()] = ps
				jpMap[slot.JackpotLevel(detail.GetData())]++
			}

			for level, cnt := range jpMap {
				if cnt == 3 {
					logic.JackpotGame.WinJackpot = logic.Jackpots[level]
				}
			}

			if logic.JackpotGame.WinJackpot != nil {
				currentBet := int64(logic.JackpotGame.GameBetRate) * logic.JackpotGame.GameBetMoney
				maxBet := int64(logic.JackpotGame.MaxBetRate) * int64(logic.JackpotGame.MaxBetMoney)
				logic.JackpotGame.WinMoney = logic.JackpotGame.WinJackpot.GetCurrentPoolByBet(currentBet, maxBet)
			}
		}
	}

	if logic.JackpotGame != nil && logic.JackpotGame.IsFinished() {
		logic.JackpotGame = nil
	}
}
