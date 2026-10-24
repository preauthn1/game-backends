package dfdc

import (
	"gamelogic"
	"msg"
	"netproto"
	"sort"
	"strconv"

	"example.invalid/golang/protobuf/proto"
)

// SendSettingMessage 發送遊戲設置內容
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10001 (DFDC_GameMessageClassID_DFDCSettingID)
//   * Message = { BetCount: [10, 20, 30, 60, 100, 200, 300, 600, 1000], RowCounts: 3, TotalPayLine: 1, BigWinSetp: [300, 400, 500, 500000], PayLineMode: 1, ReelSetting: [ { ReelID: 1, SymbolGroup: [ { SymbolID: 1, SymbolName: "p1" }, ... ] }, ... ], SymbolPayTable: [ { Rate: 5, SymbolName: "p1", Count: 3 }, ... ] }
func (logic *Logic) SendSettingMessage(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCSettingID)
	data := new(netproto.DFDC_Setting)

	data.BetCount = logic.BetCounts
	data.RowCounts = proto.Int32(logic.DefaultRowNum)
	data.TotalPayLine = proto.Int32(1)
	data.BigWinStep = logic.BigWinRule
	data.PayLineMode = proto.Int32(logic.PayLineMode.Int())

	// ReelSetting
	symbolData := logic.GetReelSymbol(logic.MainGameTable[0].GetSymbolGroupId(), logic.SlotSymbolDistribution)
	for reelID, reel := range symbolData {
		setting := new(netproto.DFDC_ReelSetting)
		setting.RealID = proto.Int32(int32(reelID))
		for _, index := range reel {
			for _, symbol := range index {
				sg := new(netproto.DFDC_SymbolGroup)
				sg.SymbolID = proto.Int32(int32(symbol.GetId()))
				sg.SymbolName = proto.String(symbol.GetSymbol())
				setting.SymbolGroup = append(setting.SymbolGroup, sg)
			}
		}
		data.ReelSetting = append(data.ReelSetting, setting)
	}

	// SymbolPayTable
	for _, row := range logic.SlotPayTable {
		if row.GetPayType() != PayTypeChip.String() && row.GetPayType() != PayTypeChipXRate.String() {
			continue
		}
		pay := new(netproto.DFDC_SymbolPayTable)

		i, _ := strconv.ParseInt(row.GetPayout(), 10, 32)
		pay.Rate = proto.Int32(int32(i))
		pay.SymbolName = proto.String(row.GetSymbol())
		pay.Count = proto.Int32(row.GetCount())

		data.SymbolPayTable = append(data.SymbolPayTable, pay)
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendGoldenSymbolMessage 發送黃金符號，設定檔中的倍率BetRate列表
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10011 (DFDC_GameMessageClassID_DFDCGoldenSymbolID)
//   * Message = { GoldenDetail: [ { Symbols: 1, Credits: 8 }, { Symbols: 2, Credits: 18 }, { Symbols: 3, Credits: 38 }, { Symbols: 4, Credits: 68 }, { Symbols: 5, Credits: 88 } ] }
func (logic *Logic) SendGoldenSymbolMessage(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGoldenSymbolID)
	data := new(netproto.DFDC_GoldenSymbolList)

	data.GoldenDetail = make([]*netproto.DFDC_GoldenSymbolDetail, 0, len(logic.BetRates))

	for i, rate := range logic.BetRates {
		detail := new(netproto.DFDC_GoldenSymbolDetail)
		detail.Symbols = proto.Int32(int32(i + 1))
		detail.Credits = proto.Int32(rate)
		data.GoldenDetail = append(data.GoldenDetail, detail)
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendJackpotPoolMessage 發送Jackpot獎池資訊
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10009 (DFDC_GameMessageClassID_DFDCJackpotPoolS2CID)
//   * Message = { JackpotPoolID: [ { JackpotID: 1, Level: "Grand", Syspool: 20000000 }, { JackpotID: 2, Level: "Major", Syspool: 7500000 }, { JackpotID: 3, Level: "Minor", Syspool: 375000 }, { JackpotID: 4, Level: "Mini", Syspool: 200000 } ] }
func (logic *Logic) SendJackpotPoolMessage(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCJackpotPoolS2CID)

	data := new(netproto.DFDC_JackpotPoolS2CMsg)

	jackpots := make([]*netproto.DFDC_JackpotPoolDetail, 0, len(logic.Jackpots))

	for _, jackpot := range logic.Jackpots {
		detail := new(netproto.DFDC_JackpotPoolDetail)
		detail.JackpotID = proto.Int32(jackpot.Level.Int())
		detail.Syspool = proto.Int64(jackpot.GetCurrentPool())
		detail.Level = proto.String(jackpot.Level.String())

		jackpots = append(jackpots, detail)
	}

	data.JackpotPoolID = jackpots

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendStateMessage 發送遊戲狀態資訊
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10004 (DFDC_GameMessageClassID_DFDCStateID)
//   * Message = { State: 0 }
func (logic *Logic) SendStateMessage(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCStateID)

	data := new(netproto.DFDC_GameStatus)

	data.State = proto.Int32(int32(logic.GameState))

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendBetRetMessage 發送下注後檢查狀態
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10010 (DFDC_GameMessageClassID_DFDCBetRetID)
//   * Message = { Ret: 0, Money: 1000 }
func (logic *Logic) SendBetRetMessage(user *gamelogic.GameUser, ret int32, money int64) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCBetRetID)

	data := new(netproto.DFDC_BetRet)
	data.Ret = proto.Int32(ret)
	data.Money = proto.Int64(money)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendGameResultMessage 發送遊戲結果
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10003 (DFDC_GameMessageClassID_DFDCGameResult)
//   * Message = { Ret: 0, Money: 1000 }
func (logic *Logic) SendGameResultMessage(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCGameResult)

	data := new(netproto.DFDC_GameResultReel)

	// Symbol ID不等於開獎結果，而是透過Symbol ID骰出該order id 之結果
	data.SymbolID = logic.GameResult.GetSymbolIDByRow(1)

	// ReelSetting
	symbolData := logic.GameResult.GetReelSymbol()
	for reelID, reel := range symbolData {
		for rowID, row := range reel {
			symbol := row[0]
			setting := new(netproto.DFDC_ReelSetting)
			setting.RealID = proto.Int32(int32(reelID))
			setting.RowIndex = proto.Int32(int32(rowID))
			setting.SymbolGroup = []*netproto.DFDC_SymbolGroup{
				&netproto.DFDC_SymbolGroup{
					SymbolID:   proto.Int32(int32(symbol.GetId())),
					SymbolName: proto.String(symbol.GetSymbol()),
				},
			}
			data.ReelSetting = append(data.ReelSetting, setting)
		}
	}

	// PayAllGroup
	payAllGroup := new(netproto.DFDC_PayAllGroup)

	var SymbolIDList []int32

	var win int64

	for _, result := range logic.GamePayResult {
		SymbolIDList = append(SymbolIDList, result.SymbolIDs...)
		if result.IsWinBonusGame() {
			continue
		}
		if ok, _ := result.IsWinFreeSpin(); ok {
			continue
		}

		detail := new(netproto.DFDC_PayAllDetail)
		detail.SymbolIDList = result.SymbolIDs
		detail.SymbolName = proto.String(result.SymbolName)
		detail.PayCount = proto.Int32(result.PayCount)
		detail.WayCount = proto.Int32(result.WayCount)
		money := result.CalcWinMoney(logic.GameBetRate, logic.GameBetMoney)
		win += money
		detail.PayRate = proto.Int32(int32(money))
		detail.FeaturePays = proto.Bool(false)
		payAllGroup.PayAllDetail = append(payAllGroup.PayAllDetail, detail)
	}

	for reelID, reel := range symbolData {
		for _, row := range reel {
			symbol := row[0]
			detail := new(netproto.DFDC_PayLineDetail)
			detail.SymbolID = proto.Int32(int32(symbol.GetId()))
			detail.ReelID = proto.Int32(int32(reelID))
			detail.Line = proto.Bool(false)
			detail.Blink = proto.Bool(false)
			// 檢查是否為連線符號
			for _, line := range SymbolIDList {
				if int32(symbol.GetId()) == line {
					detail.Blink = proto.Bool(true)
					break
				}
			}
			payAllGroup.PayLineDetail = append(payAllGroup.PayLineDetail, detail)
		}
	}

	data.PayAllGroup = []*netproto.DFDC_PayAllGroup{payAllGroup}
	data.PayLineGroup = []*netproto.DFDC_PayLineGroup{}

	data.FreeGameWinMoney = proto.Int64(logic.FreeSpinWinMoney)
	data.WinMoney = proto.Int64(win)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendFreeSpinCountMessage 傳送FreeSpin數量給Client
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10005 (DFDC_GameMessageClassID_DFDCFreeOrPayID)
//   * Message = { FreeCount: 0 }
func (logic *Logic) SendFreeSpinCountMessage(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCFreeOrPayID)

	data := new(netproto.DFDC_FreeOrPayMsg)

	data.FreeCount = proto.Int32(logic.FreeSpinCount)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendJackpotGameDetailMessage 發送BonusGame內容
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10012 (DFDC_GameMessageClassID_DFDCBonusGameID)
//   * Message = { Money: 18181, BonusDetail: [ { Id: 1, Place: 0, Icon: 4 }, ... ] }
func (logic *Logic) SendJackpotGameDetailMessage(user *gamelogic.GameUser) {

	// 沒有JackpotGame不做事
	if logic.JackpotGame == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameID)

	data := new(netproto.DFDC_BonusGame)
	data.Money = proto.Int64(logic.JackpotGame.WinMoney)
	data.BonusDetail = make([]*netproto.DFDC_BonusGameDetail, 0, len(logic.JackpotGame.Process))

	for _, detail := range logic.JackpotGame.Process {
		d := new(netproto.DFDC_BonusGameDetail)
		d.Id = proto.Int32(detail.ID)
		d.Place = proto.Int32(detail.Place)
		d.Icon = proto.Int32(detail.Icon)

		data.BonusDetail = append(data.BonusDetail, d)
	}

	sort.Slice(data.BonusDetail, func(i, j int) bool {
		return data.BonusDetail[i].GetId() < data.BonusDetail[j].GetId()
	})

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendFreeSpinBetMessage 傳送玩家上局下注資訊
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10019 (DFDC_GameMessageClassID_DFDCUserRecoverID)
//   * Message = { WinMoney: 0, GoldenSymbol: 8, BetMoney: 10 }
func (logic *Logic) SendFreeSpinBetMessage(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCUserRecoverID)

	data := new(netproto.DFDC_UserRecover)
	data.WinMoey = proto.Int64(logic.FreeSpinWinMoney)
	data.BetMoney = proto.Int64(logic.GameBetMoney)
	data.GoldenSymbol = proto.Int32(logic.GameBetRate)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendGameLogMessage 傳送玩家遊戲紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 10021 (DFDC_GameMessageClassID_DFDCLoadGameHistoryRetID)
//   * Message = { WinMoney: 0, GoldenSymbol: 8, BetMoney: 10 }
func (logic *Logic) SendGameLogMessage(user *gamelogic.GameUser, userID, gameID, pageCnt int32, history []*netproto.Slot_SendHistoryLog) {
	// 收到資料後，直接轉傳給client
	// 給client的封包
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.DFDC_GameMessageClassID_DFDCLoadGameHistoryRetID)

	data := new(netproto.Slot_SendHistoryData)
	data.UserID = proto.Int32(userID)
	data.GameID = proto.Int32(gameID)
	data.SendHistoryLog = history
	data.PageCount = proto.Int32(pageCnt)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}
