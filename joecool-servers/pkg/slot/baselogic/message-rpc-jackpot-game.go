package baselogic

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

// ReadJackpotGame 讀取JackpotGame, 使用BonusGame
func (logic *SlotLogicBase) ReadJackpotGame(user *gamelogic.GameUser) {
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
func (logic *SlotLogicBase) RecordJackpotGame(user *gamelogic.GameUser) {

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

		data := make(map[string]interface{})
		data["Level"] = slot.NewJackpotLevel(row.Icon).String()
		data["Win"] = logic.JackpotGame.WinMoney

		b, _ := json.Marshal(data)

		// detail.Data = proto.String(slot.NewJackpotLevel(row.Icon).String())
		detail.Data = proto.String(string(b))

		record.Detail = append(record.Detail, detail)
	}

	sort.Slice(record.Detail, func(i, j int) bool {
		return record.Detail[i].GetTurnID() < record.Detail[j].GetTurnID()
	})

	pmsg.MsgData = record

	logic.SendRPCMessageFunc(true, pmsg)
}

// RestJackpotGame 重置, 使用BonusGame
func (logic *SlotLogicBase) RestJackpotGame(user *gamelogic.GameUser) {

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
	data.OriginID = proto.Int32(logic.JackpotGame.Origin.GetId())

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// RecvJackpotGame 接收 Jackpot Game 復原
func (logic *SlotLogicBase) RecvJackpotGame(m *msg.Message) {

	defer logic.CheckWaitGroup()
	mlog.Debug("RecvJackpotGame")

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
			switch slot.PayType(r.GetPayType()) {
			default:
				continue
			case slot.PayTypeMysteryJPG, slot.PayTypeMysteryJPGR, slot.PayTypeMysteryJackpot, slot.PayTypeMysteryJackpotR:
				if r.GetId() == originID {
					rules[originID] = r
					break
				}
			}
		}
	}

	for originID, rule := range rules {
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

			data := make(map[string]interface{})

			if err := json.Unmarshal([]byte(detail.GetData()), &data); err != nil {
				continue
			}

			level := slot.JackpotLevel(data["Level"].(string))
			win := int64(data["Win"].(float64))

			jpMap[level]++
			logic.JackpotGame.WinMoney = win

			ps := new(slot.JackpotGameDetail)
			ps.ID = detail.GetTurnID()
			ps.Place = detail.GetLocation()
			ps.Icon = level.Int()

			logic.JackpotGame.Process[detail.GetTurnID()] = ps
		}

		for level, cnt := range jpMap {
			if cnt == 3 {
				logic.JackpotGame.WinJackpot = logic.Jackpots[level]
			}
		}

	}

	if logic.JackpotGame != nil && logic.JackpotGame.IsFinished() {
		logic.JackpotGame = nil
	}
}
