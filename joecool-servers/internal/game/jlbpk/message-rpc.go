package jlbpk

import (
	"encoding/json"
	"strconv"

	Global "example.invalid/ServerTeam/Servers/pkg/global"
	"example.invalid/ServerTeam/Servers/pkg/mlog"

	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

func (pGame *Logic) OnRecvRPCMessage(m *msg.Message) {

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
	case int32(netproto.GameMessageClassID_JackpotLinkRetID):
		// 從DB獲得的Jackpot資訊
		pGame.recvJackpotData(m)
	case int32(netproto.GameMessageClassID_SuperToolGetSysPoolWinLoseRet):
		pGame.recvSysPoolWinLose(m)
	}
}

// ReadJackpotPool jlbpk類型為非連線模式，每桌一池
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 66 (GameMessageClassID_JackpotNoLinkRetID)
func (pGame *Logic) ReadJackpotPool() {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_JackpotLinkID)

	data := new(netproto.JackpotData)
	data.GameID = proto.Int32(pGame.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(pGame.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))
	data.TableNo = proto.Int32(pGame.TableManager.TableNo)

	//讀取資料不須填值
	data.Contribution = proto.Int64(0)
	data.TotalContribution = proto.Int64(0)
	data.AwardTotal = proto.Int64(0)
	data.Level = proto.Int32(0)
	data.JackpotPool = proto.Int64(0)

	pmsg.MsgData = data

	pGame.SendRPCMessageFunc(true, pmsg)
}

// UpdateJackpotPool 更新獎池，所有數字都是DB幫你加好，只需要帶增加的量，獎池只有一個所以只更新Level 1
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 66 (GameMessageClassID_SlotJackpotNoLinkID)
func (pGame *Logic) UpdateJackpotPool(contribution, totalContribution, awardTotal int64, jackpotPool int64) {
	//DFDC類型為非連線模式，每桌一池
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_JackpotLinkID)

	data := new(netproto.JackpotData)
	data.GameID = proto.Int32(pGame.TableManager.Room.GameInfo.GameID)
	FlagID, _ := strconv.Atoi(pGame.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))
	data.TableNo = proto.Int32(pGame.TableManager.TableNo)

	data.Contribution = proto.Int64(contribution)
	data.TotalContribution = proto.Int64(totalContribution)
	data.AwardTotal = proto.Int64(awardTotal)
	data.Level = proto.Int32(0)
	data.JackpotPool = proto.Int64(jackpotPool)

	pmsg.MsgData = data

	pGame.SendRPCMessageFunc(true, pmsg)
}

// recvJackpotData 接收db server回傳的Jackpot資訊，並整理至Logic內
func (pGame *Logic) recvJackpotData(m *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		return new(netproto.JackpotAllData)
	})

	data := rmm.(*netproto.JackpotAllData)

	for _, row := range data.AllData {

		if row.GetGameID() != pGame.TableManager.Room.GameInfo.GameID {
			continue
		}

		if strconv.Itoa(int(row.GetFlagID())) != pGame.TableManager.Room.Flag {
			continue
		}

		if row.GetTableNo() != pGame.TableManager.TableNo {
			continue
		}

		g_i64Contribution = row.GetContribution()
		g_i64TotalContribution = row.GetTotalContribution()
		g_i64AwardTotal = row.GetAwardTotal()
		g_i64Jackpot = row.GetJackpotPool()
	}

	if g_i64Jackpot < g_i64JackpotBase {
		pGame.UpdateJackpotPool(0, 0, 0, g_i64JackpotBase-g_i64Jackpot)
		g_i64Jackpot = g_i64JackpotBase
		mlog.Debug("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
		pGame.GameLog.AddGameStep("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
	}

	pGame.sendUpdateJackpot()
}

// 要求 遊戲輸贏結果
func (pGame *Logic) GetSysPoolWinLose() {
	if !g_bEnablePoolControl {
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.GameMessageClassID_SuperToolGetSysPoolWinLose)

	data := new(netproto.SuperTool_Msg_GetSysPoolWinLose)
	FlagID, _ := strconv.Atoi(pGame.TableManager.Room.Flag)
	data.GameID = proto.Int32(pGame.TableManager.Room.GameInfo.GameID)
	data.FlagID = proto.Int32(int32(FlagID))

	pMsg.MsgData = data

	pGame.SendRPCMessageFunc(true, pMsg)
}

// 回傳 遊戲輸贏結果
func (pGame *Logic) recvSysPoolWinLose(pMsg *msg.Message) {
	_, rmm := rpcmsg.ParseRPCMessage(pMsg, func() proto.Message {
		return new(netproto.SuperTool_Msg_GetSysPoolWinLoseRet)
	})

	if !g_bEnablePoolControl || g_bGetWinLoseData {
		return
	}

	data := rmm.(*netproto.SuperTool_Msg_GetSysPoolWinLoseRet)

	if data.GetGameID() != pGame.TableManager.Room.GameInfo.GameID && strconv.Itoa(int(data.GetFlagID())) != pGame.TableManager.Room.Flag {
		return
	}

	// 水線
	i64PoolAmount := int64(data.GetPoolAmount())
	if i64PoolAmount <= 0 {
		g_pSysPoolWinLose.i64PoolAmount = g_i64WinLine
	} else {
		g_pSysPoolWinLose.i64PoolAmount = i64PoolAmount
	}
	// 遊戲輸贏結果
	pGameData := new(GameDataDB)
	err := json.Unmarshal([]byte(data.GetWinLoseData()), pGameData)
	if err == nil {
		g_pSysPoolWinLose.GameDBData.Copy(pGameData)
	}

	// 以下為 debug 用
	//mlog.Error("recvSysPoolWinLose()")
	//mlog.Warn("PoolAmount[%d]", g_pSysPoolWinLose.i64PoolAmount)
	//mlog.Warn("［GameData］WeightTableNoNow:%d, DealerNotOKCount:%d, DealerWinCount:%d, DealerLoseCount:%d, GameRound:%d",
	//	g_pSysPoolWinLose.GameDBData.WeightTableNoNow, g_pSysPoolWinLose.GameDBData.DealerNotOKCount,
	//	g_pSysPoolWinLose.GameDBData.DealerWinCount, g_pSysPoolWinLose.GameDBData.DealerLoseCount, g_pSysPoolWinLose.GameDBData.GameRound)

	g_bGetWinLoseData = true
}

// 更新 遊戲輸贏結果
func (pGame *Logic) UpdateSysPoolWinLose() {
	if !g_bEnablePoolControl {
		return
	}

	mlog.Debug("更新 WinLose DB 資料: WinLine[%d], PoolAmount[%d], GameData[%s]",
		g_i64WinLine, g_pSysPoolWinLose.i64PoolAmount, g_pSysPoolWinLose.GameDBData.LogString())

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.GameMessageClassID_SuperToolUpdateSysPoolWinLose)

	data := new(netproto.SuperTool_Msg_UpdateSysPoolWinLose)
	data.GameID = proto.Int32(pGame.TableManager.Room.GameInfo.GameID) // 遊戲編號
	FlagID, _ := strconv.Atoi(pGame.TableManager.Room.Flag)
	data.FlagID = proto.Int32(int32(FlagID))                       // 房間編號
	data.PoolAmount = proto.Int64(g_pSysPoolWinLose.i64PoolAmount) // 水線
	data.WinLine = proto.Int64(g_i64WinLine)                       // 贏錢線
	// 遊戲輸贏結果
	byGameData, _ := json.Marshal(g_pSysPoolWinLose.GameDBData)
	data.WinLoseData = proto.String(string(byGameData))

	pMsg.MsgData = data

	pGame.SendRPCMessageFunc(true, pMsg)
}
