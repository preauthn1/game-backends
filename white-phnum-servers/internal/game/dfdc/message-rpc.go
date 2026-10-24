package dfdc

import (
	"fmt"
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// OnRecvRPCMessageFunc 接收DB Server回傳的Message訊息
func (logic *Logic) OnRecvRPCMessageFunc(m *msg.Message) {

	switch m.SClassID {
	// 多福多財BonusGame紀錄
	case int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID):
		logic.recvBonusData(m)
	// 從DB獲得的遊戲歷程紀錄
	case int32(netproto.GameMessageClassID_SlotSendGameLogID):
		logic.recvSlotGameLog(m)
	}

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

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID)

	detail := new(netproto.DFDC_JackpotBonusGameRet)
	detail.UserID = proto.Int32(user.UserID)

	pmsg.MsgData = detail

	logic.SendRPCMessageFunc(true, pmsg)
}

// recvBonusData 接收db server回傳的BonusGame資訊，並整理至Logic內
func (logic *Logic) recvBonusData(m *msg.Message) {

	defer logic.CheckWaitGroup()

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

// recvSlotGameLog 接收db server回傳的SlotGameLog資訊，直接傳給Client
func (logic *Logic) recvSlotGameLog(m *msg.Message) {

	defer logic.CheckWaitGroup()

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
