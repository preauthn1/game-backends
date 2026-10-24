package baselogic

import (
	"gamelogic"
	"mlog"
	"msg"
	"netproto"
	"sort"
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/server/pkg/slot"
	"example.invalid/server/pkg/slot/protodata"
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

		data := &netproto.Slot_LoadHistoryData{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		logic.ReadSlotGameHistoryLog(user, data.GetPageIndex(), data.GetPageSize(), data.GetStartTime(), data.GetEndTime())
		return
	}

	if logic.GameMessageFunc != nil {
		logic.GameMessageFunc(user, gmsg)
	}
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

// SendGameLogMessageToUser 傳送玩家遊戲紀錄
//
//   * BClassID = 10001 (MessageBClassID_Game)
//   * SClassID = 22002 (Slot_GameMessageClassID_Slot_LoadHistoryRetID)
//   * Message = { UserID: 12345, GameID: 26, PageCount: 2, SendHistoryLog: [ { RowNum: 1, CreateTime: 2019-10-10, OrderID: 1, BetMoney: 8, Profit: -8 }... ] }
func (logic *SlotLogicBase) SendGameLogMessageToUser(user *gamelogic.GameUser, userID, gameID, pageCnt int32, history []*netproto.Slot_SendHistoryLog) {
	// 收到資料後，直接轉傳給client
	// 給client的封包
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.Slot_GameMessageClassID_Slot_LoadHistoryRetID)

	data := new(netproto.Slot_SendHistoryData)
	data.UserID = proto.Int32(userID)
	data.GameID = proto.Int32(gameID)
	data.SendHistoryLog = history
	data.PageCount = proto.Int32(pageCnt)

	pmsg.MsgData = data

	logic.SendMessageToUser(user, pmsg)
}

// BuildReelSetting 建立傳給Client的遊戲設置
func (logic *SlotLogicBase) BuildReelSetting(groupID, reelNum, rowNum int32, symbols []*protodata.SlotSymbolDistribution) []*netproto.Slot_ReelSetting {

	var ans []*netproto.Slot_ReelSetting
	for reelNum := int32(1); reelNum <= logic.DefaultReelNum; reelNum++ {
		setting := new(netproto.Slot_ReelSetting)
		setting.RealID = proto.Int32(int32(reelNum))

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
		return ans[i].GetRealID() < ans[j].GetRealID()
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
		}
	}

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

	for _, symbol := range reelMap {
		results = append(results, symbol)
	}

	sort.Slice(results, func(i, j int) bool {
		if results[i].GetPosition().GetReel() == results[j].GetPosition().GetReel() {
			return results[i].GetPosition().GetRow() < results[j].GetPosition().GetRow()
		}

		return results[i].GetPosition().GetReel() < results[j].GetPosition().GetReel()
	})

	// sort.Slice(results, func(i, j int) bool {
	// 	if results[i].GetPosition().GetRow() == results[j].GetPosition().GetRow() {
	// 		return results[i].GetPosition().GetReel() < results[j].GetPosition().GetReel()
	// 	}

	// 	return results[i].GetPosition().GetRow() < results[j].GetPosition().GetRow()
	// })

	return results
}

// BuildPayGroups 建立傳給Client的中獎結果
func (logic *SlotLogicBase) BuildPayGroups(results []*slot.PayResult) []*netproto.Slot_PayGroup {

	var grps []*netproto.Slot_PayGroup

	// 中獎內容，賠付線結果(派彩線)
	for _, row := range results {
		for _, line := range row.SymbolPayLines {
			paygroup := new(netproto.Slot_PayGroup)
			payline := new(netproto.Slot_PayLine)

			if row.IsWinJackpot() {
				paygroup.WinMoney = proto.Int64(logic.GameWinJackpot)
			} else {
				paygroup.WinMoney = proto.Int64(row.CalcWinMoney(logic.GameBetMoney) / int64(row.WayCount))
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

	return grps
}
