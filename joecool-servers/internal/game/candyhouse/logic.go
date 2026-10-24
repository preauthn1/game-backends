package candyhouse

import (
	"fmt"
	"strconv"
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// Logic CandyHouse
type Logic struct {
	*gamelogic.ComputeGameLogicBase

	SlotPattern            *protodata.SlotPattern              // SlotPattern 遊戲主設定
	MainGameTable          *protodata.MainGameTable            // MainGameTable 輪軸遊戲設定
	SymbolAttributes       []*protodata.SymbolAttributes       // SymbolAttributes 特殊符號設定
	SlotPayTable           []*protodata.SlotPayTable           // SlotPayTable 賠付表設定
	SlotPayLine            []*protodata.SlotPayLine            // SlotPayLine 賠付線設定
	JackpotTable           []*protodata.JackpotTable           // JackpotTable 獎池設定
	SlotSymbolDistribution []*protodata.SlotSymbolDistribution // SlotSymbolDistribution 符號機率設定
	BonusTable             []*protodata.BonusTable             // BonusTable 獎勵遊戲設定
	PickGameTable          []*protodata.PickGameTable          // PickGameTable 選擇遊戲設定

	SlotPatternID uint32
	BetCounts     []int32                // 遊戲押注數量
	MaxBetCount   int32                  // 最大押注數量
	BigWinRule    []int32                // client動畫播放階段,以Bet為主
	BigWinNotify  int32                  // 當下注16, Total=6400(16*400) 得獎超過6400才廣播.
	EndActionTime int32                  // 結算動畫超時
	MinCarryMoney int64                  // 最低攜帶金額
	SymbolsDesc   map[string]*SymbolDesc // 符號說明
	PayLineDesc   string                 // 派彩線說明文字
	PickGameDesc  string                 // 糖果遊戲說名字
	UnitPrefix    map[string]int64       // 單位倍率對照

	DefaultReelNum int32 // 預設Reel數量，以MainGameTable第一筆做基準
	DefaultRowNum  int32 // 預設Row數量，以MainGameTable第一筆做基準
	BonusGameSize  int32 // BonusGame共有多少選項可選擇, default: 12

	PayLineMode    slot.PayLineMode    // 賠付線模式
	PaylineMatcher slot.PaylineMatcher // 賠付線比對器

	Jackpots map[slot.JackpotLevel]*slot.Jackpot // Jackpot資料，共有4個獎池，map[{JackpotLevel}] *Jackpot
	GameUser *gamelogic.GameUser                 // 當前玩家

	GameState slot.GameState // 當前遊戲狀態

	GameBetMoney int64 // 玩家下注金額

	GameResultManager *slot.GameResultManager // 遊戲結果
	GamePayResult     []*slot.PayResult       // 遊戲中獎內容

	GamePayMoney     int64 // 花費金額
	GameWinMoney     int64 // 贏得金額
	GameWinJackpot   int64 // 贏得Jackpot金額
	GameWinBonusGame bool  // 贏得額外遊戲

	BonusGame *BonusGame // 當前額外遊戲

}

// NewLogic create CandyHouse game logic
func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Logic {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)

	logic.loadConfigs()
	logic.loadProtoDatas()

	// 設定預設值
	logic.DefaultReelNum = int32(logic.MainGameTable.GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable.GetRowNum())

	// 設定賠付線
	logic.PayLineMode = slot.PayLineMode(logic.SlotPattern.GetLineType())

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.initJackpot()

	logic.RegisterBeforeGameStartFunc(func() {
	})
	logic.RegisterAfterGameStartFunc(func() {
	})
	logic.RegisterBeforeGameEndFunc(func() {
	})
	logic.RegisterAfterGameEndFunc(func() {
		logic.Clean()
	})

	return logic
}

// loadConfigs 讀取.dat設定檔資料
func (logic *Logic) loadConfigs() {

	if cfg == nil {
		return
	}

	// Slot Configs
	{
		slotconfig := logic.Configs.GetConfig("slotconfig")

		// slotsetting
		{
			cfg := slotconfig.GetConfigSection("slotsetting")

			// SlotPattern
			{
				i, _ := strconv.ParseInt(cfg["slotpattern"], 10, 32)
				logic.SlotPatternID = uint32(i)
			}

			// Bet Count
			{
				betcounts := strings.Split(cfg["betcount"], ",")
				logic.BetCounts = make([]int32, 0, len(betcounts))
				for _, betcount := range betcounts {
					i, err := strconv.ParseInt(betcount, 10, 32)
					if err != nil {
						continue
					}
					logic.BetCounts = append(logic.BetCounts, int32(i))

					if logic.MaxBetCount < int32(i) {
						logic.MaxBetCount = int32(i)
					}
				}
			}

			// BigWinRule
			{
				strs := strings.Split(cfg["bigwinrule"], ",")
				logic.BigWinRule = make([]int32, 0, len(strs))
				for _, str := range strs {
					i, err := strconv.ParseInt(str, 10, 32)
					if err != nil {
						continue
					}
					logic.BigWinRule = append(logic.BigWinRule, int32(i))
				}
			}

			// BigWinNotify
			{
				str := cfg["bigwinnotify"]
				i, _ := strconv.ParseInt(str, 10, 32)
				logic.BigWinNotify = int32(i)
			}

			// EndActionTime
			{
				str := cfg["endactiontime"]
				i, _ := strconv.ParseInt(str, 10, 32)
				logic.EndActionTime = int32(i)
			}
		}

		// symbols
		{
			cfg := slotconfig.GetConfigSection("symbols")

			logic.SymbolsDesc = make(map[string]*SymbolDesc)

			for key, val := range cfg {
				keys := strings.Split(key, ".")
				if len(keys) != 2 {
					continue
				}
				name := keys[0]
				field := keys[1]

				if _, ok := logic.SymbolsDesc[name]; !ok {
					logic.SymbolsDesc[name] = &SymbolDesc{Name: name}
				}

				switch field {
				case "title":
					logic.SymbolsDesc[name].Title = val
				case "desc":
					logic.SymbolsDesc[name].Description = val
				}
			}
		}

		// bonus
		{
			cfg := slotconfig.GetConfigSection("bonus")
			// use default 12
			logic.BonusGameSize = 12

			if val, ok := cfg["size"]; ok {
				f, err := strconv.ParseFloat(val, 64)
				if err == nil {
					logic.BonusGameSize = int32(f)
				}
			}
		}

		// description
		{
			cfg := slotconfig.GetConfigSection("description")

			if val, ok := cfg["payline"]; ok {
				logic.PayLineDesc = val
			}

			if val, ok := cfg["pickgame"]; ok {
				logic.PickGameDesc = val
			}
		}

		// unit prefix
		{
			cfg := slotconfig.GetConfigSection("unit-prefix")

			logic.UnitPrefix = make(map[string]int64)
			for key, val := range cfg {
				i, err := strconv.ParseInt(val, 10, 64)
				if err != nil {
					continue
				}
				logic.UnitPrefix[key] = i
			}
		}
	}

	// Game Configs
	{
		gameconfig := cfg.GetConfig("gameserver")

		cfg := gameconfig.GetConfigSection("gameserver")

		// MinBetMoney
		{
			i, _ := strconv.ParseInt(cfg["betlimitmoney"], 10, 64)
			logic.MinCarryMoney = i * 100
		}
	}

}

// loadProtoDatas 取得該遊戲設定值所對應的proto data
//
// 由SlotPattern取得所對應的Table
func (logic *Logic) loadProtoDatas() {
	if protodatas == nil {
		return
	}

	// 整理可能會有的SymbolGroupID
	SymbolGroupIDMap := make(map[uint32]bool)

	// 整理可能會有的PickGameID
	PickGameIDMap := make(map[uint32]bool)

	// 整理可能會有的BonusGroupID
	BonusGroupIDMap := make(map[uint32]bool)

	// SlotPattern
	for _, row := range protodatas.SlotPattern {
		if row.GetId() == logic.SlotPatternID {
			logic.SlotPattern = row
			break
		}
	}

	// MainGameTable
	for _, row := range protodatas.MainGameTable {
		if row.GetGroupId() != logic.SlotPattern.GetMainGameTableGroupId() {
			continue
		}
		logic.MainGameTable = row

		SymbolGroupIDMap[row.GetSymbolGroupId()] = true
	}

	// SymbolAttributes
	for _, row := range protodatas.SymbolAttributes {
		if row.GetGroupId() != logic.SlotPattern.GetSymbolAttributesGroupId() {
			continue
		}
		logic.SymbolAttributes = append(logic.SymbolAttributes, row)
	}

	// SlotPayTable
	for _, row := range protodatas.SlotPayTable {
		if uint32(row.GetGroupId()) != logic.SlotPattern.GetPayTableGroupId() {
			continue
		}
		logic.SlotPayTable = append(logic.SlotPayTable, row)

		if row.GetPayType() == slot.PayTypePickGame.String() {
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			PickGameIDMap[uint32(id)] = true
		}
	}

	// SlotPayLine
	for _, row := range protodatas.SlotPayLine {
		if row.GetGroupId() != logic.SlotPattern.GetPayLineGroupId() {
			continue
		}
		logic.SlotPayLine = append(logic.SlotPayLine, row)
	}

	// JackpotTable
	for _, row := range protodatas.JackpotTable {
		if uint32(row.GetGroupId()) != logic.SlotPattern.GetJackpotGroup() {
			continue
		}
		logic.JackpotTable = append(logic.JackpotTable, row)
	}

	// SlotSymbolDistribution
	for _, row := range protodatas.SlotSymbolDistribution {
		if _, ok := SymbolGroupIDMap[row.GetGroupId()]; !ok {
			continue
		}
		logic.SlotSymbolDistribution = append(logic.SlotSymbolDistribution, row)
	}

	// PickGameTable
	for _, row := range protodatas.PickGameTable {
		if _, ok := PickGameIDMap[row.GetId()]; !ok {
			continue
		}

		logic.PickGameTable = append(logic.PickGameTable, row)

		BonusGroupIDMap[row.GetRewardValue()] = true
	}

	// BonusTable
	for _, row := range protodatas.BonusTable {
		if _, ok := BonusGroupIDMap[uint32(row.GetGroupId())]; !ok {
			continue
		}

		logic.BonusTable = append(logic.BonusTable, row)
	}

}

// initJackpot 依照proto設定取得Jackpot基本資料，初始時只建立共有多少Jackpot
func (logic *Logic) initJackpot() {
	logic.Jackpots = make(map[slot.JackpotLevel]*slot.Jackpot)

	for _, row := range logic.JackpotTable {
		jackpot := new(slot.Jackpot)
		jackpot.Origin = row
		if row.GetIsMultiLevel() == 0 {
			jackpot.Level = slot.JackpotSingle
		} else {
			jackpot.Level = slot.JackpotLevel(row.GetLevel())
		}
		logic.Jackpots[jackpot.Level] = jackpot
	}

}

// GameMessage on recived client message
func (logic *Logic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if !logic.CheckGameUserMessage(user) {
		logic.KickUserFunc(user.UserID, "非法操作")
		return
	}

	// 非遊戲訊息，不處理
	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	// Client請求讀取Jackpot
	if gmsg.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ReadJackpotPoolID) {
		logic.SendJackpotPoolMessage(user)
	}

	// 玩家Spin
	if gmsg.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionSpinID) {

		data := &netproto.TGW_MSG_Spin{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		ok, ret, money := logic.Bet(user, data.GetMoney())

		// 下注狀態
		logic.SendBetStatusMessage(user, ret, money)

		if !ok {
			return
		}

		logic.GameLog.AddGameStep("玩家 %d; 下注: %d", user.UserID, money)

		logic.GameState = slot.GameStateAward
		logic.SendStateMessage(user)

		// 累積Jackpot獎池
		var logdesc = ""
		for _, jackpot := range logic.Jackpots {
			// 手動更新獎池資訊
			jackpot.Increase(logic.GamePayMoney)
			logdesc += fmt.Sprintf("N=%d,AwardAdd=%d,jpAmount=%d|", jackpot.TotalContribution, jackpot.AwardTotal, jackpot.GetCurrentPool())
			// 更新Jackpot
			logic.UpdateJackpotPool(0, jackpot.GetIncrease(logic.GamePayMoney), 0, jackpot.Level)
		}
		logdesc = logdesc[:len(logdesc)-1]
		logic.GameLog.AddGameStep("%s", logdesc)
		mlog.Debug("%s", logdesc)

		// 把手動更新得獎池資訊給Client
		logic.SendJackpotPoolMessage(user)

		// 開始轉
		logic.Spin(user)

		logic.GameResultManager.ReelMode()

		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeMAIN, slot.LogTypeREEL, logic.GameResultManager.ToSymbolString())

		// 判斷是否贏得BonusGame
		if logic.GameWinBonusGame {
			logic.GameLog.AddGameStep("進入BonusGame, 中獎金額: %d, 中獎內容: %s", logic.BonusGame.WinMoney, logic.BonusGame.GetProcessData())

			// 儲存 DB
			for _, detail := range logic.BonusGame.Process {
				logic.RecordBonusGame(user, detail.origin.GetId(), detail.ID, detail.Location, logic.GameBetMoney)
			}

			// 發送開獎內容
			logic.SendBonusGameMessage(user)
		}

		// 傳送開獎結果
		logic.SendGameResultMessage(user)

		// 調整狀態為等待
		logic.GameState = slot.GameStateWait
		logic.SendStateMessage(user)

		// 寫分
		logic.UpdateUserScore(user)

		logic.RegisterAnimation(AnimationGameEnd, logic.EndActionTime, func() {

			logic.GameEndLog()

			logic.GameEndFunc()

			logic.GameState = slot.GameStateBet
			logic.SendStateMessage(user)

			logic.Clean()
		})
	}

	// 玩家查詢記錄
	if gmsg.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_LoadHistoryID) {

		data := &netproto.Slot_LoadHistoryData{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		logic.ReadSlotGameHistoryLog(user, data.GetPageIndex(), data.GetPageSize(), data.GetStartTime(), data.GetEndTime())
	}

	// 玩家選擇PickGame
	if gmsg.SClassID == int32(netproto.TGW_GameMessageClassID_TGW_ActionPickID) {

		data := &netproto.TGW_MSG_Pick{}
		if err := proto.Unmarshal(gmsg.MsgData.([]byte), data); err != nil {
			mlog.Error("接收玩家选择封包解析错误. %v", err)
			return
		}

		record := data.GetRecord()

		if record == nil {
			mlog.Error("User pick symbol, but client not sent TGW_BonusGameRecord")
			return
		}

		if logic.BonusGame == nil {
			mlog.Error("User pick symbol, but BonusGame is not exist")
			return
		}

		if logic.BonusGame.IsFinished() {
			mlog.Error("User pick symbol, but BonusGame is finished")
			return
		}

		if !logic.BonusGame.Exist(record.GetID()) {
			mlog.Error("User pick symbol ID %d, but not exist", record.GetID())
			return
		}

		detail, err := logic.BonusGame.Choose(record.GetID(), record.GetLocation())
		if err != nil {
			mlog.Error("%v", err)
			return
		}

		// 跟Client說我收到了
		logic.SendBonusGameRetMessage(user, record)

		logic.GameLog.AddGameStep("玩家%d; 選擇第%d次; 選擇位置: %d, 中獎金額: %d", user.UserID, detail.ID, detail.Location, detail.Money)

		// Update DB
		logic.RecordBonusGame(user, detail.origin.GetId(), detail.ID, detail.Location, detail.Money)

		// 還沒結束的話就跳開等玩家再次選擇
		if !logic.BonusGame.IsFinished() {
			return
		}

		// 結束遊戲
		logic.GamePayMoney = 0
		logic.GameWinBonusGame = false
		logic.GameWinMoney = logic.BonusGame.WinMoney

		logic.GameLog.AddGameStep("BonusGame, 中獎金額: %d, 中獎內容: %s", logic.BonusGame.WinMoney, logic.BonusGame.GetProcessData())

		logic.AddSlotGameHistoryLog(user, slot.HistoryTypeBONUS, slot.LogTypeOPEN, logic.BonusGame.GetProcessData())

		logic.ResetBonusGame(user)

		// 調整狀態為等待
		logic.GameState = slot.GameStateWait
		logic.SendStateMessage(user)

		// 寫分
		logic.UpdateUserScore(user)

		logic.BonusGame = nil

		// DB 等一秒在清空
		logic.RegisterAnimation(AnimationGameEnd, logic.EndActionTime, func() {
			logic.ResetBonusGame(user)

			logic.GameEndLog()

			logic.GameEndFunc()

			logic.GameState = slot.GameStateBet
			logic.SendStateMessage(user)

			logic.Clean()
		})

	}
}

// PiSaiMessage on recived client message
func (logic *Logic) PiSaiMessage(user *gamelogic.GameUser, gmsg *msg.Message) {}

// UserEnter 玩家進入
func (logic *Logic) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {
	logic.ComputeGameLogicBase.UserEnter(user, tableno, seatno)

	logic.Clean()

	logic.GameLog.AddGameStep("玩家進入")
	logic.GameUser = user

	// 調整狀態為等待
	logic.GameState = slot.GameStateWait
	logic.SendStateMessage(user)

	// 讀取 Jackpot 資訊
	logic.ReadJackpotPool()
	// 讀取 BonusGame 資訊
	logic.ReadBonusGame(user)

	// 傳送遊戲設置內容
	logic.SendSettingMessage(user)

	// 等待1秒後告知Client玩家狀態, 獎池資訊
	logic.RegisterAnimation(AnimationGameStart, int32(1), func() {
		logic.SendJackpotPoolMessage(user)
		logic.SendBonusGameMessage(user)

		logic.GameState = slot.GameStateBet
		logic.SendStateMessage(user)

		logic.GameLog.GameBegin()
	})
}

// UserLeave 玩家離開
func (logic *Logic) UserLeave(user *gamelogic.GameUser) bool {
	logic.GameLog.AddGameStep("玩家離開")
	logic.GameUser = nil
	logic.Clean()

	logic.BonusGame = nil
	logic.GameWinBonusGame = false

	return logic.ComputeGameLogicBase.UserLeave(user)
}

// UserRecover 玩家回來
func (logic *Logic) UserRecover(user *gamelogic.GameUser) {
	logic.GameLog.AddGameStep("玩家回來")
	logic.GameUser = user

	logic.ComputeGameLogicBase.UserRecover(user)
}

// Clean game data
func (logic *Logic) Clean() {
	if logic.GameWinBonusGame {
		return
	}

	logic.GameBetMoney = 0

	logic.GameResultManager = nil
	logic.GamePayResult = make([]*slot.PayResult, 0)

	logic.GamePayMoney = 0
	logic.GameWinMoney = 0
	logic.GameWinJackpot = 0

	logic.GameWinBonusGame = false

	logic.BonusGame = nil
}

// GameEndLog call GameLog GameEnd()
func (logic *Logic) GameEndLog() {
	if logic.GameUser != nil {
		logic.GameLog.GameEnd(
			map[int32]int64{logic.GameUser.UserID: logic.GameWinMoney - logic.GamePayMoney},
		)
	}
}
