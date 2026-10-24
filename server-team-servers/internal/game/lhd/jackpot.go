package lhd

import (
	"sort"
	"strconv"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/prob"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/slot"
)

type JackpotLevel uint8

const (
	JackpotLevelNone JackpotLevel = iota
	JackpotLevelMini
	JackpotLevelMinor
	JackpotLevelGrand
)

func (j JackpotLevel) String() string {
	switch j {
	case JackpotLevelNone:
		return "None"
	case JackpotLevelMini:
		return "Mini"
	case JackpotLevelMinor:
		return "Minor"
	case JackpotLevelGrand:
		return "Grand"
	default:
		return "Unknown"
	}
}

type AssignType uint8

const (
	AssignTypeNone    AssignType = iota
	AssignTypeRandom             // 隨機
	AssignTypeRecycle            // 回收
)

type JackpotAssignment struct {
	Level JackpotLevel
	Type  AssignType
	UID   int32 // 事後骰出要給的玩家
}

type Jackpot struct {
	Enabled          bool
	AwardPause       bool
	BasePool         int64
	ContributeRate   int64
	Contribute       int64
	AwardLine        int64
	PhaseLevel       []int64
	AwardWeight      map[JackpotLevel]int64
	AwardAmount      map[JackpotLevel]int64
	AllowedBetAmount map[JackpotLevel]int64
	Assignment       *JackpotAssignment
}

func NewJackpot() *Jackpot {
	return &Jackpot{
		Enabled:          false,
		AwardPause:       false,
		BasePool:         500000,
		ContributeRate:   200,
		Contribute:       0,
		AwardLine:        1000000,
		PhaseLevel:       []int64{10, 30, 60},
		AwardWeight:      map[JackpotLevel]int64{JackpotLevelNone: 19993, JackpotLevelMini: 4, JackpotLevelMinor: 2, JackpotLevelGrand: 1},
		AwardAmount:      map[JackpotLevel]int64{JackpotLevelMini: 20000, JackpotLevelMinor: 100000},
		AllowedBetAmount: map[JackpotLevel]int64{JackpotLevelMini: 0, JackpotLevelMinor: 2000, JackpotLevelGrand: 10000},
	}
}

func (j *Jackpot) Reset() {
	j.Contribute = 0
}

func (j *Jackpot) AddBet(bet int64) int64 {
	n := bet * j.ContributeRate
	j.Contribute += n
	return n
}

func (j *Jackpot) Current() int64 {
	return j.BasePool + j.Contribute/10000
}

func (j *Jackpot) Awardable() bool {
	return j.Current() >= j.AwardLine
}

func (j *Jackpot) Award(bet, maxbet int64) (JackpotLevel, int64, int64) {

	// 沒有開啟獎池
	if !j.Enabled {
		return JackpotLevelNone, 0, 0
	}

	// 獎池暫停
	if j.AwardPause {
		return JackpotLevelNone, 0, 0
	}

	// 沒有達到獎勵條件
	if !j.Awardable() {
		return JackpotLevelNone, 0, 0
	}

	var lv JackpotLevel

	// 製作選項
	type options struct {
		weight int64
		level  JackpotLevel
	}

	opts := []options{
		{weight: j.AwardWeight[JackpotLevelNone] * maxbet, level: JackpotLevelNone},
		{weight: j.AwardWeight[JackpotLevelMini] * bet, level: JackpotLevelMini},
		{weight: j.AwardWeight[JackpotLevelMinor] * bet, level: JackpotLevelMinor},
		{weight: j.AwardWeight[JackpotLevelGrand] * bet, level: JackpotLevelGrand},
	}

	r := new(prob.Random)
	for _, opt := range opts {
		if bet < j.AllowedBetAmount[opt.level] {
			continue
		}
		r.Add(opt, int(opt.weight))
	}

	choose := r.Choose()
	if choose == nil {
		return JackpotLevelNone, 0, 0
	}

	lv = choose.(options).level

	return j.AwardByAssigned(lv)
}

func (j *Jackpot) AwardByAssigned(lv JackpotLevel) (JackpotLevel, int64, int64) {
	contribute := j.Contribute

	switch lv {
	default:
		return JackpotLevelNone, 0, 0
	case JackpotLevelMini, JackpotLevelMinor:
		award := j.AwardAmount[lv]
		j.Contribute -= award * 10000
		if j.Contribute < 0 {
			j.Reset()
		}
		return lv, award, j.Contribute - contribute
	case JackpotLevelGrand:
		award := j.Current()
		j.Contribute -= award * 10000
		if j.Contribute < 0 {
			j.Reset()
		}
		return lv, award, j.Contribute - contribute
	}
}

// 當前為第幾階段，百分比多少
func (j *Jackpot) Phase() (int, int) {
	if !j.Enabled {
		return 0, 0
	}

	// 計算現在是多少百分比
	r := int64(float64(j.Current()-j.BasePool) / float64(j.AwardLine-j.BasePool) * 100)

	sort.Slice(j.PhaseLevel, func(a, b int) bool { return j.PhaseLevel[a] < j.PhaseLevel[b] })

	for i, v := range j.PhaseLevel {
		if r < v {
			return i + 1, int(r)
		}
	}

	return len(j.PhaseLevel) + 1, int(r)
}

func (lh *LongHudouLogic) ReadJackpotPoolNoLink() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkReadID)

	data := new(netproto.Slot_MSG_JackpotRead)
	example.invalid = new(netproto.Slot_JackpotData)
	example.invalid.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)
	example.invalid.FlagID = proto.Int32(int32(FlagID))
	example.invalid.TableNo = proto.Int32(lh.TableManager.TableNo)
	example.invalid.Level = proto.Int32(slot.JackpotSingle.Int())

	//讀取資料不須填值
	example.invalid.Contribution = proto.Int64(0)
	example.invalid.TotalContribution = proto.Int64(0)
	example.invalid.AwardTotal = proto.Int64(0)

	pmsg.MsgData = data

	lh.SendRPCMessageFunc(true, pmsg)
}

func (lh *LongHudouLogic) UpdateJackpotPoolNoLink(contribution int64) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotNoLinkRecordID)

	data := new(netproto.Slot_MSG_JackpotRecord)

	detail := new(netproto.Slot_JackpotData)
	detail.GameID = proto.Int32(lh.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(lh.TableManager.Room.Flag)
	detail.FlagID = proto.Int32(int32(FlagID))
	detail.TableNo = proto.Int32(lh.TableManager.TableNo)

	detail.Contribution = proto.Int64(contribution)
	detail.TotalContribution = proto.Int64(0)
	detail.AwardTotal = proto.Int64(0)
	detail.Level = proto.Int32(0)

	data.Records = append(data.Records, detail)

	example.invalid("更新 Jackpot Contribution: %v", contribution)
	lh.GameLog.AddGameStep("Jackpot Contribution: GameId[%d] FlagID[%d] TableNo[%d] Update[%d] After[%d]",
		lh.TableManager.Room.GameInfo.GameID, FlagID, lh.TableManager.TableNo, contribution, lh.Jackpot.Contribute)
	pmsg.MsgData = data

	lh.SendRPCMessageFunc(true, pmsg)
}

// RecvJackpotData 接收db server回傳的Jackpot資訊，並整理至Logic內
func (lh *LongHudouLogic) RecvJackpotData(m *msg.Message) {

	mlog.Debug("Table %d, RecvJackpotData", lh.TableNo())

	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.Slot_MSG_JackpotRecord)
	})

	data := rmm.(*netproto.Slot_MSG_JackpotRecord)

	for _, row := range data.Records {

		if row.GetGameID() != lh.TableManager.Room.GameInfo.GameID {
			continue
		}

		if strconv.Itoa(int(row.GetFlagID())) != lh.TableManager.Room.Flag {
			continue
		}

		if row.GetTableNo() != lh.TableManager.TableNo {
			continue
		}

		lh.GameLog.AddGameStep("GameId %d, FlagID %s, TableNo %d, RecvContribution: %v", lh.TableManager.Room.GameInfo.GameID, lh.TableManager.Room.Flag, lh.TableManager.TableNo, row.GetContribution())

		example.invalid("Table %d, RecvJackpotData, JackpotLevel: %d, Contribution: %d, TotalContribution: %d, AwardTotal: %d", lh.TableNo(), row.GetLevel(), row.GetContribution(), row.GetTotalContribution(), row.GetAwardTotal())

		lh.Jackpot.Contribute = row.GetContribution()
	}
}
