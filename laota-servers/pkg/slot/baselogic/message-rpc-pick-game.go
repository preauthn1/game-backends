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

// ReadJPickGame 讀取PickGame, 使用BonusGame
func (logic *SlotLogicBase) ReadJPickGame(user *gamelogic.GameUser) {
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

// RecordPickGame 紀錄PickGame, 使用BonusGame
func (logic *SlotLogicBase) RecordPickGame(user *gamelogic.GameUser) {

	if logic.PickGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BonusGameRecordID)

	record := new(netproto.Slot_MSG_BonusGameRecord)

	for _, row := range logic.PickGame.Process {
		detail := new(netproto.Slot_BonusGameDetail)
		detail.UserID = proto.Int32(user.UserID)
		detail.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
		FlagID, _ := strconv.ParseInt(logic.TableManager.Room.Flag, 10, 32)
		detail.FlagID = proto.Int32(int32(FlagID))

		detail.OriginID = proto.Int32(logic.PickGame.Origin.GetId())
		detail.TurnID = proto.Int32(row.ID)
		detail.Location = proto.Int32(row.Location)
		detail.BetMoney = proto.Int32(int32(logic.GameBetMoney))
		detail.BetRate = proto.Int32(int32(logic.GameBetRate))

		data := make(map[string]interface{})
		data["Bonus"] = row.BonusID
		data["Position"] = row.Positon

		data["TotalWin"] = logic.PickGame.WinMoney
		data["AllPosition"] = logic.PickGame.Positions

		b, _ := json.Marshal(data)

		detail.Data = proto.String(string(b))

		record.Detail = append(record.Detail, detail)
	}

	sort.Slice(record.Detail, func(i, j int) bool {
		return record.Detail[i].GetTurnID() < record.Detail[j].GetTurnID()
	})

	pmsg.MsgData = record

	logic.SendRPCMessageFunc(true, pmsg)
}

// ResetPickGame 重置, 使用BonusGame
func (logic *SlotLogicBase) ResetPickGame(user *gamelogic.GameUser) {

	if logic.PickGame == nil {
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
	data.OriginID = proto.Int32(logic.PickGame.Origin.GetId())

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// RecvPickGame 接收 Pick Game 復原
func (logic *SlotLogicBase) RecvPickGame(m *msg.Message) {

	defer logic.CheckWaitGroup()
	mlog.Debug("RecvPickGame")

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
			case slot.PayTypePickGame:
				if r.GetId() == originID {
					rules[originID] = r
					break
				}
			}
		}
	}

	for originID, rule := range rules {

		tmp := datas[originID][0]

		logic.GameBetMoney = int64(tmp.GetBetMoney())
		logic.GameBetRate = int64(tmp.GetBetRate())

		var positions []slot.Position
		data := make(map[string]interface{})
		if err := json.Unmarshal([]byte(tmp.GetData()), &data); err != nil {
			continue
		}
		if val, ok := data["AllPosition"]; ok {
			if vals, ok := val.([]interface{}); ok {
				for _, _pos := range vals {
					_pos := _pos.(map[string]interface{})

					var p slot.Position

					p.Reel = int32(_pos["Reel"].(float64))
					p.Row = int32(_pos["Row"].(float64))

					positions = append(positions, p)
				}
			}
		}

		game := logic.WinPickGame(rule, int32(len(positions)), false)
		game.Positions = positions

		// 重置Process
		game.Process = make(map[int32]*slot.PickGameDetail)
		for _, detail := range datas[originID] {

			data := make(map[string]interface{})

			if err := json.Unmarshal([]byte(detail.GetData()), &data); err != nil {
				continue
			}

			bonusID := int32((data["Bonus"].(float64)))

			win := int64(data["TotalWin"].(float64))

			var pos slot.Position

			if val, ok := data["Position"]; ok {
				_pos := val.(map[string]interface{})
				pos.Reel = int32(_pos["Reel"].(float64))
				pos.Row = int32(_pos["Row"].(float64))
			}

			game.WinMoney = win

			var bonus *protodata.BonusTable
			for _, row := range logic.BonusTable {
				if row.GetId() == bonusID {
					bonus = row
					break
				}
			}

			if bonus == nil {
				mlog.Warn("[PickGame] Recover錯誤, 查無對應Bonus(%d)", bonusID)
				continue
			}

			ps := new(slot.PickGameDetail)
			ps.Origin = bonus
			ps.BonusID = bonus.GetId()
			ps.ID = detail.GetTurnID()
			ps.Location = detail.GetLocation()
			ps.Positon = pos

			// 回推金額
			switch slot.PayType(bonus.GetRewardType()) {
			case slot.PayTypeChip:
				f, _ := strconv.ParseFloat(bonus.GetRewardValue(), 64)
				ps.Money = int64(f) * game.GameBetMoney
			case slot.PayTypeChipXRate:
				f, _ := strconv.ParseFloat(bonus.GetRewardValue(), 64)
				ps.Money = int64(f) * game.GameBetMoney * game.GameBetRate
			}

			game.Process[detail.GetTurnID()] = ps

			game.All[int(ps.ID-1)] = ps.Money
		}

		logic.PickGame = game
	}

	if logic.PickGame != nil && logic.PickGame.IsFinished() {
		logic.PickGame = nil
	}
}
