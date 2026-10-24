package baselogic

import (
	"math"
	"sort"
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// GameMessage 接收遊戲訊息
func (logic *SlotLogicBase) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if !logic.CheckGameUserMessage(user) {
		logic.KickUserFunc(user.UserID, "非法操作")
		return
	}

	// 空訊息，不處理
	if gmsg == nil {
		return
	}

	// 非遊戲訊息，不處理
	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	// 玩家查詢記錄
	if gmsg.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_LoadHistoryID) {

		data := &netproto.Slot_MSG_GameLogRead{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		logic.ReadSlotGameHistoryLog(user, data.GetPageIndex(), data.GetPageSize(), data.GetStartTime(), data.GetEndTime())
		return
	}

	// Client請求讀取Jackpot
	if gmsg.SClassID == int32(netproto.Slot_GameMessageClassID_Slot_JackpotPoolID) {
		logic.SendJackpotMessageToUser(user)
	}

	if logic.GameMessageFunc != nil {
		logic.GameMessageFunc(user, gmsg)
	}
}

// SendSettingMessage 發送遊戲設置內容
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20001 (Slot_GameMessageClassID_Slot_SettingID)
//   * Message = { BetCount: [10, 20, 30, 60, 100, 200, 300, 600, 1000], RowCounts: 3, TotalPayLine: 1, BigWinSetp: [300, 400, 500, 500000], PayLineMode: 1, ReelSetting: [ { ReelID: 1, SymbolGroup: [ { SymbolID: 1, SymbolName: "p1" }, ... ] }, ... ], SymbolPayTable: [ { Rate: 5, SymbolName: "p1", Count: 3 }, ... ] }
func (logic *SlotLogicBase) SendSettingMessage(user *gamelogic.GameUser) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_SettingID)
	data := new(netproto.Slot_MSG_Setting)

	data.BetMoneys = logic.BetCounts
	data.BetRates = logic.BetRates
	data.RowCounts = proto.Int32(logic.DefaultRowNum)
	data.BigWinStep = logic.BigWinRule
	data.PayLineMode = proto.Int32(logic.PayLineMode.Int())
	data.MinCarryMoney = proto.Int64(logic.MinCarryMoney)

	switch logic.PayLineMode {
	default:
	case slot.PayLineModeAll:
		data.TotalPayLine = proto.Int32(int32(math.Pow(float64(logic.DefaultRowNum), float64(logic.DefaultReelNum))))
	case slot.PayLineModeFixed:
		data.TotalPayLine = proto.Int32(int32(len(logic.SlotPayLine)))
	}

	// UnitPrefix
	if logic.UnitPrefix != nil {
		for name, multiple := range logic.UnitPrefix {
			unit := new(netproto.Slot_UnitPrefix)
			unit.Name = proto.String(name)
			unit.Multiple = proto.Int64(multiple)
			data.UnitPrefix = append(data.UnitPrefix, unit)
		}
	}

	// Description
	if logic.Descriptions != nil {
		for key, val := range logic.Descriptions {
			desc := new(netproto.Slot_Description)
			desc.Name = proto.String(key)
			desc.Desc = proto.String(val)
			data.Descriptions = append(data.Descriptions, desc)
		}
	}

	// ReelSetting
	data.ReelSetting = logic.NetprotoSlotReelSetting

	// SymbolPayTable
	data.SymbolPayTable = logic.NetprotoSlotSymbolPayTable

	// FreeGameInfo
	data.FreeGames = logic.NetprotoFreeGameInfo

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendStateMessageToUser 發送遊戲狀態資訊
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20002 (Slot_GameMessageClassID_Slot_StatusID)
//   * Message = { State: 0 }
func (logic *SlotLogicBase) SendStateMessageToUser(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_StatusID)

	data := new(netproto.Slot_MSG_Status)

	data.State = proto.Int32(int32(logic.GameState))

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendBetStatusMessageToUser 發送下注後檢查狀態
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 20003 (Slot_GameMessageClassID_Slot_BetStatusID)
//   * Message = { Ret: 0, Money: 1000 }
func (logic *SlotLogicBase) SendBetStatusMessageToUser(user *gamelogic.GameUser, ret int32, money int64) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_BetStatusID)

	data := new(netproto.Slot_MSG_BetStatus)
	data.Ret = proto.Int32(ret)
	data.Money = proto.Int64(money)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendLeftFreeGameToUser 發送免費遊戲剩於次數
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 22002 (Slot_GameMessageClassID_Slot_LeftFreeGameID)
//   * Message = { ID: 12345, Count: 8, BetMoney: 20, BetRate: 1 }
func (logic *SlotLogicBase) SendLeftFreeGameToUser(user *gamelogic.GameUser) {

	if logic.FreeSpin == nil {
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_LeftFreeGameID)

	data := new(netproto.Slot_MSG_LeftFreeGameData)
	data.ID = proto.Int32(int32(logic.FreeSpin.Origin.GetId()))
	data.Count = proto.Int32(int32(logic.FreeSpin.Count))
	data.BetMoney = proto.Int64(logic.FreeSpin.BetMoney)
	data.BetRate = proto.Int64(logic.FreeSpin.BetRate)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)

}

// required int32 Count = 1;     // 剩餘次數
// required int64 BetMoney = 2;  // 下注金額
// optional int64 BetRate = 3;   // 下注倍率符號
// optional int32 ID = 4;        // 免費遊戲ID

// SendGameLogMessageToUser 傳送玩家遊戲紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 22002 (Slot_GameMessageClassID_Slot_LoadHistoryRetID)
//   * Message = { UserID: 12345, GameID: 26, PageCount: 2, SendHistoryLog: [ { RowNum: 1, CreateTime: 2019-10-10, OrderID: 1, BetMoney: 8, Profit: -8 }... ] }
func (logic *SlotLogicBase) SendGameLogMessageToUser(user *gamelogic.GameUser, userID, gameID, pageCnt int32, history []*netproto.Slot_GameLog) {
	// 收到資料後，直接轉傳給client
	// 給client的封包
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_LoadHistoryRetID)

	data := new(netproto.Slot_MSG_GameLogRecord)
	data.UserID = proto.Int32(userID)
	data.GameID = proto.Int32(gameID)
	data.Logs = history
	data.PageCount = proto.Int32(pageCnt)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// SendJackpotMessageToUser 發送Jackpot獎池資訊
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 22004 (Slot_GameMessageClassID_Slot_JackpotPoolRetID)
//   * Message = [ { Money: 500000, Name: , ID: 1 }, ]
func (logic *SlotLogicBase) SendJackpotMessageToUser(user *gamelogic.GameUser) {

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_JackpotPoolRetID)

	data := new(netproto.Slot_MSG_JackpotPool)

	for _, jackpot := range logic.Jackpots {
		row := new(netproto.Slot_JackpotInfo)
		row.Money = proto.Int64(jackpot.GetCurrentPool())
		row.Name = proto.String(jackpot.Level.String())
		row.ID = proto.Int32(jackpot.Origin.GetId())

		data.Jackpots = append(data.Jackpots, row)
	}

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// BuildReelSetting 建立傳給Client的遊戲設置
func (logic *SlotLogicBase) BuildReelSetting(groupID, reelNum, rowNum int32, symbols []*protodata.SlotSymbolDistribution) []*netproto.Slot_ReelSetting {

	var ans []*netproto.Slot_ReelSetting
	for reelNum := int32(1); reelNum <= logic.DefaultReelNum; reelNum++ {
		setting := new(netproto.Slot_ReelSetting)
		setting.ReelID = proto.Int32(int32(reelNum))

		// 只留一組，並且打亂
		sbMap := make(map[string]*netproto.Slot_Symbol)

		for rowNum := int32(1); rowNum <= logic.DefaultRowNum; rowNum++ {
			symbols := logic.GameResultManager.GetSymbolsByReelRow(int32(groupID), reelNum, rowNum, symbols)
			for _, symbol := range symbols {
				sb := new(netproto.Slot_Symbol)
				sb.SymbolID = proto.Int32(int32(symbol.GetId()))
				sb.SymbolName = proto.String(symbol.GetSymbol())
				sbMap[sb.GetSymbolName()] = sb
			}
		}

		for _, row := range sbMap {
			setting.Symbols = append(setting.Symbols, row)
		}

		ans = append(ans, setting)
	}

	sort.Slice(ans, func(i, j int) bool {
		return ans[i].GetReelID() < ans[j].GetReelID()
	})

	return ans
}

// BuildSymbolPayTable 建立傳給Client的符號賠率設置
func (logic *SlotLogicBase) BuildSymbolPayTable(paytables []*protodata.SlotPayTable, descMap map[string]*slot.SymbolDesc) []*netproto.Slot_SymbolPayTable {

	var ans []*netproto.Slot_SymbolPayTable

	for _, row := range paytables {
		pay := new(netproto.Slot_SymbolPayTable)
		i, _ := strconv.ParseFloat(row.GetPayout(), 64)
		pay.Symbol = &netproto.Slot_Symbol{SymbolID: proto.Int32(0), SymbolName: proto.String(row.GetSymbol())}
		pay.Count = proto.Int32(row.GetCount())
		pay.Type = proto.String(row.GetPayType())

		switch pay.GetType() {
		default:
			pay.Payout = proto.Int32(0)
		case slot.PayTypeChip.String(): // 一般賠錢，回傳賠率
			pay.Payout = proto.Int32(int32(i))
		case slot.PayTypeChipXRate.String(): // 一般賠錢，回傳賠率
			pay.Payout = proto.Int32(int32(i))
		}

		// 若有設定符號文案則寫上去
		if descMap != nil {
			if desc, ok := descMap[row.GetSymbol()]; ok {
				pay.Title = proto.String(desc.Title)
				pay.Desc = proto.String(desc.Description)
			}
		}

		ans = append(ans, pay)
	}

	return ans
}

// BuildPayLines 建立傳給Client的賠線設置
func (logic *SlotLogicBase) BuildPayLines(lines []*protodata.SlotPayLine) []*netproto.Slot_PayLine {

	var ans []*netproto.Slot_PayLine
	// PayLines
	for _, row := range lines {
		payline := slot.NewPayLine(row)

		line := new(netproto.Slot_PayLine)
		line.PayLineID = proto.Int32(int32(row.GetLineId()))

		for _, p := range payline.Positions {
			pos := new(netproto.Slot_ReelPosition)
			pos.Reel = proto.Int32(p.Reel)
			pos.Row = proto.Int32(p.Row)
			line.Positions = append(line.Positions, pos)
		}
		ans = append(ans, line)
	}

	return ans
}

// BuildReelResult 依照GameResult建立傳給Client的開獎結果
func (logic *SlotLogicBase) BuildReelResult(result *slot.GameResultManager, payresults []*slot.PayResult) []*netproto.Slot_ReelResult {

	var results []*netproto.Slot_ReelResult

	reelMap := make(map[slot.Position]*netproto.Slot_ReelResult)
	orgReelMap := make(map[slot.Position]*protodata.SlotSymbolDistribution)

	// 輪軸結果
	result.RowMode()
	for row, reels := range result.GameResult {
		for reel, symbol := range reels {
			symbolResult := &netproto.Slot_ReelResult{
				Position: &netproto.Slot_ReelPosition{Reel: proto.Int32(int32(reel + 1)), Row: proto.Int32(int32(row + 1))},
				Symbol:   &netproto.Slot_Symbol{SymbolID: proto.Int32(int32(symbol.GetId())), SymbolName: proto.String(symbol.GetSymbol())},
				Blink:    proto.Bool(false),
			}
			reelMap[slot.Position{Reel: int32(reel + 1), Row: int32(row + 1)}] = symbolResult
			orgReelMap[slot.Position{Reel: int32(reel + 1), Row: int32(row + 1)}] = symbol
		}
	}

	switch logic.PayLineMode {
	case slot.PayLineModeFixed:
		for _, row := range payresults {
			for _, line := range row.SymbolPayLines {

				var blinkPositions []slot.Position
				tmp := append([]slot.Position{}, line.PayLine.Positions...)

				switch slot.LinkType(row.Rule.GetLinkType()) {
				case slot.LinkTypeLeft:
					sort.Slice(tmp, func(i, j int) bool {
						return tmp[i].Reel < tmp[j].Reel
					})
				case slot.LinkTypeRight:
					sort.Slice(tmp, func(i, j int) bool {
						return tmp[i].Reel > tmp[j].Reel
					})
				}

				for i := int32(0); i < row.PayCount; i++ {
					blinkPositions = append(blinkPositions, tmp[i])
				}

				for _, pos := range blinkPositions {
					if _, ok := reelMap[pos]; ok {
						reelMap[pos].Blink = proto.Bool(true)
					}
				}
			}
		}
	case slot.PayLineModeAll:
		blinkIDs := make(map[int32]bool)
		for _, row := range payresults {
			for _, id := range row.SymbolIDs {
				blinkIDs[id] = true
			}
		}

		for pos, org := range orgReelMap {
			if _, ok := blinkIDs[int32(org.GetId())]; ok {
				reelMap[pos].Blink = proto.Bool(true)
			}
		}

	}

	for _, symbol := range reelMap {
		results = append(results, symbol)
	}

	sort.Slice(results, func(i, j int) bool {
		if results[i].GetPosition().GetReel() == results[j].GetPosition().GetReel() {
			return results[i].GetPosition().GetRow() < results[j].GetPosition().GetRow()
		}

		return results[i].GetPosition().GetReel() < results[j].GetPosition().GetReel()
	})

	return results
}

// BuildPayGroups 建立傳給Client的中獎結果
func (logic *SlotLogicBase) BuildPayGroups(results []*slot.PayResult) []*netproto.Slot_PayGroup {

	var grps []*netproto.Slot_PayGroup

	switch logic.PayLineMode {
	case slot.PayLineModeFixed:
		// for 指定線
		// 中獎內容，賠付線結果(派彩線)
		for _, row := range results {
			for _, line := range row.SymbolPayLines {
				paygroup := new(netproto.Slot_PayGroup)
				payline := new(netproto.Slot_PayLine)

				if row.IsWinJackpot() {
					paygroup.WinMoney = proto.Int64(logic.GameWinJackpot)
				} else {
					paygroup.WinMoney = proto.Int64(row.CalcWinMoney(logic.GameBetMoney, logic.GameBetRate) / int64(row.WayCount))
				}
				payline.PayLineID = proto.Int32(int32(line.PayLine.Origin.GetLineId()))

				for _, orgpos := range line.PayLine.Positions {
					pos := new(netproto.Slot_ReelPosition)
					pos.Reel = proto.Int32(orgpos.Reel)
					pos.Row = proto.Int32(orgpos.Row)
					payline.Positions = append(payline.Positions, pos)
				}

				paygroup.PayLines = payline
				paygroup.LinkType = proto.String(row.Rule.GetLinkType())
				paygroup.Count = proto.Int32(row.PayCount)
				paygroup.Way = proto.Int32(row.WayCount)

				tmp := paygroup.PayLines.GetPositions()

				switch slot.LinkType(row.Rule.GetLinkType()) {
				case slot.LinkTypeLeft:
					sort.Slice(tmp, func(i, j int) bool {
						return tmp[i].GetReel() < tmp[j].GetReel()
					})
				case slot.LinkTypeRight:
					sort.Slice(tmp, func(i, j int) bool {
						return tmp[i].GetReel() > tmp[j].GetReel()
					})
				}

				for i := int32(0); i < row.PayCount; i++ {
					paygroup.BlinkPositions = append(paygroup.BlinkPositions, tmp[i])
				}

				grps = append(grps, paygroup)
			}
		}

	case slot.PayLineModeAll:
		// for 全線
		for _, row := range results {
			if row.GetPayType() != slot.PayTypeChip && row.GetPayType() != slot.PayTypeChipXRate {
				continue
			}

			paygroup := new(netproto.Slot_PayGroup)
			payline := new(netproto.Slot_PayLine)

			paygroup.PayLines = payline
			paygroup.LinkType = proto.String(row.Rule.GetLinkType())
			paygroup.Count = proto.Int32(row.PayCount)
			paygroup.Way = proto.Int32(row.WayCount)
			paygroup.WinMoney = proto.Int64(row.CalcWinMoney(logic.GameBetMoney, logic.GameBetRate))
			paygroup.Multi = proto.Int32(row.Multi)

			for _, pos := range row.BlinkPositions {
				paygroup.BlinkPositions = append(paygroup.BlinkPositions, &netproto.Slot_ReelPosition{Reel: proto.Int32(pos.Reel), Row: proto.Int32(pos.Row)})
			}

			grps = append(grps, paygroup)
		}
	}

	return grps
}

// BuildFreeGameInfos 以FreeSpinTable轉成FreeGameInfo
func (logic *SlotLogicBase) BuildFreeGameInfos(tbs []*protodata.FreeSpinTable) []*netproto.Slot_FreeGameInfo {

	infos := make([]*netproto.Slot_FreeGameInfo, 0, len(tbs))

	for _, row := range tbs {
		info := logic.BuildFreeGameInfo(row)
		infos = append(infos, info)
	}

	return infos
}

// BuildFreeGameInfo 以FreeSpinTable轉成FreeGameInfo
func (logic *SlotLogicBase) BuildFreeGameInfo(tb *protodata.FreeSpinTable) *netproto.Slot_FreeGameInfo {
	if tb == nil {
		return nil
	}

	info := new(netproto.Slot_FreeGameInfo)
	info.ID = proto.Int32(int32(tb.GetId()))
	info.Count = proto.Int32(int32(tb.GetFreeSpinCount()))
	info.ReelSetting = logic.BuildReelSetting(int32(tb.GetFreeSymbolGroupId()), int32(tb.GetFreeReelNum()), int32(tb.GetFreeRowNum()), logic.SlotSymbolDistribution)

	return info
}

// BuildGoldBallInfos 建立金球倍率設定
func (logic *SlotLogicBase) BuildGoldBallInfos(attrs []*protodata.SymbolAttributes) []*netproto.Slot_GoldBallInfo {

	var infos []*netproto.Slot_GoldBallInfo

	for _, attr := range attrs {
		if attr.GetIsGoldBall() == 1 {
			info := new(netproto.Slot_GoldBallInfo)
			info.Symbol = proto.String(attr.GetSymbol())

			f, _ := strconv.ParseFloat(attr.GetGoldballValue(), 64)

			switch slot.PayType(attr.GetGoldballType()) {
			case slot.PayTypeJackpot:
				info.Type = proto.String("jackpot")
				info.Value = proto.Int64(int64(f))
			case slot.PayTypeChipXRate:
				info.Type = proto.String("multiple")
				info.Value = proto.Int64(int64(f))
			}

			infos = append(infos, info)
		}
	}

	return infos
}
