package jlbpk

import (
	"strconv"

	Global "example.invalid/ServerTeam/Servers/pkg/global"
	"example.invalid/ServerTeam/Servers/pkg/mlog"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

func (logic *JlbpkLogic) OnRecvRPCMessage(m *msg.Message) {

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
	case int32(netproto.GameMessageClassID_JackpotLinkRetID):
		logic.recvJackpotData(m)
	}
}

// ReadJackpotPool jlbpk類型為非連線模式，每桌一池
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 66 (GameMessageClassID_JackpotNoLinkRetID)
func (logic *JlbpkLogic) ReadJackpotPool() {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_JackpotLinkID)

	data := new(netproto.JackpotData)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(logic.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))
	data.TableNo = proto.Int32(logic.TableManager.TableNo)

	//讀取資料不須填值
	data.Contribution = proto.Int64(0)
	data.TotalContribution = proto.Int64(0)
	data.AwardTotal = proto.Int64(0)
	data.Level = proto.Int32(0)
	data.JackpotPool = proto.Int64(-1)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// UpdateJackpotPool 更新獎池，所有數字都是DB幫你加好，只需要帶增加的量，獎池只有一個所以只更新Level 1
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 66 (GameMessageClassID_SlotJackpotNoLinkID)
func (logic *JlbpkLogic) UpdateJackpotPool(contribution, totalContribution, awardTotal int64, jackpotPool int64) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_JackpotLinkID)

	data := new(netproto.JackpotData)
	data.GameID = proto.Int32(logic.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(logic.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))
	data.TableNo = proto.Int32(logic.TableManager.TableNo)

	data.Contribution = proto.Int64(contribution)
	data.TotalContribution = proto.Int64(totalContribution)
	data.AwardTotal = proto.Int64(awardTotal)
	data.Level = proto.Int32(0)
	data.JackpotPool = proto.Int64(jackpotPool)

	pmsg.MsgData = data

	logic.SendRPCMessageFunc(true, pmsg)
}

// recvJackpotData 接收db server回傳的Jackpot資訊，並整理至Logic內
func (logic *JlbpkLogic) recvJackpotData(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.JackpotAllData)
	})

	data := rmm.(*netproto.JackpotAllData)

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

		g_i64Contribution = row.GetContribution()
		g_i64TotalContribution = row.GetTotalContribution()
		g_i64AwardTotal = row.GetAwardTotal()
		g_i64Jackpot = row.GetJackpotPool()
	}

	if g_i64Jackpot < g_i64JackpotBase {
		g_i64Jackpot = g_i64JackpotBase
		mlog.Debug("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
		logic.GameLog.AddGameStep("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
	}

	logic.sendUpdateJackpot()
}
