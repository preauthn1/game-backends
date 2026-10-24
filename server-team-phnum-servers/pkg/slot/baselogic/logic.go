package baselogic

import (
	"fmt"
	"math"
	"strconv"
	"strings"
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/slot"
	"example.invalid/ServerTeam/Servers/pkg/slot/protodata"
)

// SlotLogicBase Slot Game 專用的基礎GameLoigc
type SlotLogicBase struct {
	*gamelogic.ComputeGameLogicBase
	GameMessageFunc      func(user *gamelogic.GameUser, gmsg *msg.Message)
	OnRecvRPCMessageFunc func(m *msg.Message)

	WG *sync.WaitGroup // 等待群組, 當有需要去DB查詢東西回來才做事情可以用

	// 基本上一定有的ProtoData
	SlotPattern            *protodata.SlotPattern              // SlotPattern 遊戲主設定
	MainGameTable          []*protodata.MainGameTable          // MainGameTable 輪軸遊戲設定
	SymbolAttributes       []*protodata.SymbolAttributes       // SymbolAttributes 特殊符號設定
	SlotPayTable           []*protodata.SlotPayTable           // SlotPayTable 賠付表設定
	SlotSymbolDistribution []*protodata.SlotSymbolDistribution // SlotSymbolDistribution 符號機率設定

	// 可能會有的ProtoData
	SlotPayLine    []*protodata.SlotPayLine    // SlotPayLine 賠付線設定
	JackpotTable   []*protodata.JackpotTable   // JackpotTable 獎池設定
	RespinTable    []*protodata.RespinTable    // RespinTable 鎖軸重轉設定
	BonusTable     []*protodata.BonusTable     // BonusTable 獎勵遊戲設定
	PickGameTable  []*protodata.PickGameTable  // PickGameTable 選擇遊戲設定
	FreeSpinTable  []*protodata.FreeSpinTable  // FreeSpinTable 免費遊戲設定
	SelectionTable []*protodata.SelectionTable // SelectionTable 選擇模式設定

	SlotPatternID uint32
	BetCounts     []int32                      // 遊戲押注數量
	BetRates      []int32                      // 遊戲押注倍率
	MaxBetCount   int32                        // 最大押注數量
	MaxBetRate    int32                        // 最大押注倍率
	BigWinRule    []int32                      // client動畫播放階段,以Bet為主
	BigWinNotify  int32                        // 當下注16, Total=6400(16*400) 得獎超過6400才廣播.
	EndActionTime int32                        // 結算動畫超時
	MinCarryMoney int64                        // 最低攜帶金額
	SymbolsDesc   map[string]*slot.SymbolDesc  // 符號說明
	PayLineDesc   string                       // 派彩線說明文字
	UnitPrefix    map[string]int64             // 單位倍率對照
	Descriptions  map[string]*slot.Description // 文字訊息

	DefaultSymbolGroupID int32 // 預設SymbolGroupID，以MainGameTable第一筆做基準
	DefaultReelNum       int32 // 預設Reel數量，以MainGameTable第一筆做基準
	DefaultRowNum        int32 // 預設Row數量，以MainGameTable第一筆做基準

	PayLineMode    slot.PayLineMode    // 賠付線模式
	PaylineMatcher slot.PaylineMatcher // 賠付線比對器

	// 不會變的資訊，預先整理好準備隨時發送用
	NetprotoSlotReelsSetting   map[int32][]*netproto.Slot_ReelSetting
	NetprotoSlotReelSetting    []*netproto.Slot_ReelSetting
	NetprotoSlotSymbolPayTable []*netproto.Slot_SymbolPayTable
	NetprotoSlotPayLine        []*netproto.Slot_PayLine
	NetprotoFreeGameInfo       []*netproto.Slot_FreeGameInfo
	NetprotoGoldBallInfo       []*netproto.Slot_GoldBallInfo

	Jackpots map[slot.JackpotLevel]*slot.Jackpot // Jackpot資料，map[{JackpotLevel}] *Jackpot

	GameState slot.GameState
	GameUser  *gamelogic.GameUser

	GameBetMoney int64 // 玩家下注金額
	GameBetRate  int64 // 玩家下注倍率(黃金符號)

	GameResultManager *slot.GameResultManager // 遊戲結果
	GamePayResult     []*slot.PayResult       // 遊戲中獎內容

	GamePayMoney   int64 // 花費金額
	GamePayCoupon  int64 // 花費禮碼
	GameWinMoney   int64 // 贏得金額
	GameWinJackpot int64 // 贏得Jackpot

	// 部分遊戲會有的功能
	FreeSpin    *slot.FreeSpin    // 當前免費遊戲
	JackpotGame *slot.JackpotGame // Jackpot翻牌遊戲
}

// NewSlotLogicBase 建立Slot專用的GameLogic
func NewSlotLogicBase(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup, protodatas *protodata.ProtoData) *SlotLogicBase {
	logic := new(SlotLogicBase)

	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)

	logic.loadConfigs()
	logic.loadProtoDatas(protodatas)

	if logic.SlotPattern == nil {
		panic(fmt.Errorf("Could not find SlotPatten"))
	}

	if len(logic.MainGameTable) == 0 {
		panic(fmt.Errorf("Could not find MainGameTable"))
	}

	// 設定預設值
	logic.DefaultSymbolGroupID = int32(logic.MainGameTable[0].GetSymbolGroupId())
	logic.DefaultReelNum = int32(logic.MainGameTable[0].GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable[0].GetRowNum())

	// 設定賠付線
	logic.PayLineMode = slot.PayLineMode(logic.SlotPattern.GetLineType())

	switch logic.PayLineMode {
	case slot.PayLineModeAll:
		logic.PaylineMatcher = slot.NewPaylineAllMatcher(logic.SymbolAttributes)
	case slot.PayLineModeFixed:
		logic.PaylineMatcher = slot.NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.GameBetRate = 1
	if len(logic.BetRates) == 0 {
		logic.MaxBetRate = 1
	}

	logic.initJackpot()

	// 建立MainGame排列帶
	logic.NetprotoSlotReelsSetting = make(map[int32][]*netproto.Slot_ReelSetting)
	for _, row := range logic.MainGameTable {
		settings := logic.BuildReelSetting(int32(row.GetSymbolGroupId()), int32(row.GetReelNum()), int32(row.GetRowNum()), logic.SlotSymbolDistribution)
		logic.NetprotoSlotReelsSetting[int32(row.GetId())] = settings
	}

	logic.NetprotoSlotReelSetting = logic.BuildReelSetting(logic.DefaultSymbolGroupID, logic.DefaultReelNum, logic.DefaultRowNum, logic.SlotSymbolDistribution)
	logic.NetprotoSlotSymbolPayTable = logic.BuildSymbolPayTable(logic.SlotPayTable, logic.SymbolsDesc)
	logic.NetprotoSlotPayLine = logic.BuildPayLines(logic.SlotPayLine)
	logic.NetprotoFreeGameInfo = logic.BuildFreeGameInfos(logic.FreeSpinTable)
	logic.NetprotoGoldBallInfo = logic.BuildGoldBallInfos(logic.SymbolAttributes)

	return logic
}

// loadConfigs 讀取.dat設定檔資料
func (logic *SlotLogicBase) loadConfigs() {

	if logic.Configs == nil {
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

			// Bet Rate
			{
				betrates := strings.Split(cfg["betrate"], ",")
				logic.BetRates = make([]int32, 0, len(betrates))
				for _, betrate := range betrates {
					i, err := strconv.ParseInt(betrate, 10, 32)
					if err != nil {
						continue
					}
					logic.BetRates = append(logic.BetRates, int32(i))

					if logic.MaxBetRate < int32(i) {
						logic.MaxBetRate = int32(i)
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

			logic.SymbolsDesc = make(map[string]*slot.SymbolDesc)

			for key, val := range cfg {
				keys := strings.Split(key, ".")
				if len(keys) != 2 {
					continue
				}
				name := keys[0]
				field := keys[1]

				if _, ok := logic.SymbolsDesc[name]; !ok {
					logic.SymbolsDesc[name] = &slot.SymbolDesc{Name: name}
				}

				switch field {
				case "title":
					logic.SymbolsDesc[name].Title = val
				case "desc":
					logic.SymbolsDesc[name].Description = val
				}
			}
		}

		// description
		{
			cfg := slotconfig.GetConfigSection("description")

			logic.Descriptions = make(map[string]*slot.Description)
			for key, val := range cfg {
				keys := strings.Split(key, ".")
				if len(keys) != 2 {
					continue
				}
				name := keys[0]
				field := keys[1]

				if _, ok := logic.Descriptions[name]; !ok {
					logic.Descriptions[name] = &slot.Description{Name: name}
				}

				switch field {
				case "title":
					logic.Descriptions[name].Title = val
				case "desc":
					logic.Descriptions[name].Description = val
				}
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
		gameconfig := logic.Configs.GetConfig("gameserver")

		cfg := gameconfig.GetConfigSection("gameserver")

		// MinCarryMoney
		{
			i, _ := strconv.ParseInt(cfg["mincarrymoney"], 10, 64)
			logic.MinCarryMoney = i * 100
		}
	}

}

// loadProtoDatas 取得該遊戲設定值所對應的proto data
//
// 由SlotPattern取得所對應的Table
func (logic *SlotLogicBase) loadProtoDatas(protodatas *protodata.ProtoData) {
	if protodatas == nil {
		return
	}

	// 整理可能會有的 SymbolGroupID
	SymbolGroupIDMap := make(map[uint32]bool)

	// 整理可能會有的 PickGameID
	PickGameIDMap := make(map[uint32]bool)

	// 整理可能會有的 BonusGroupID
	BonusGroupIDMap := make(map[uint32]bool)

	// 整理可能會有的 RespinGroupID
	RespinGroupIDMap := make(map[int32]bool)

	// 整理可能會有的 FreeSpinGroupID
	FreeSpinGroupIDMap := make(map[int32]bool)

	// 整理可能會有的 SelectionGroupID
	SelectionGroupIDMap := make(map[int32]bool)

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
		logic.MainGameTable = append(logic.MainGameTable, row)

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

		switch slot.PayType(row.GetPayType()) {
		case slot.PayTypeRespin:
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			RespinGroupIDMap[int32(id)] = true
		case slot.PayTypePickGame:
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			PickGameIDMap[uint32(id)] = true
		case slot.PayTypeFreeSpin:
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			FreeSpinGroupIDMap[int32(id)] = true
		case slot.PayTypeSelectGameMode:
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			FreeSpinGroupIDMap[int32(id)] = true
		case slot.PayTypeLightning:
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			RespinGroupIDMap[int32(id)] = true
		case slot.PayTypeSelection:
			id, _ := strconv.ParseFloat(row.GetPayout(), 64)
			SelectionGroupIDMap[int32(id)] = true
		}
	}

	// SlotPayLine
	if logic.SlotPattern.GetLineType() == slot.PayLineModeFixed.String() {
		for _, row := range protodatas.SlotPayLine {
			if row.GetGroupId() != logic.SlotPattern.GetPayLineGroupId() {
				continue
			}
			logic.SlotPayLine = append(logic.SlotPayLine, row)
		}
	}

	// SelectionTable
	for _, row := range protodatas.SelectionTable {
		if _, ok := SelectionGroupIDMap[int32(row.GetGroupId())]; !ok {
			continue
		}

		logic.SelectionTable = append(logic.SelectionTable, row)

		switch slot.PayType(row.GetSelectionType()) {
		case slot.PayTypeRespin:
			RespinGroupIDMap[int32(row.GetSelectionValue())] = true
		case slot.PayTypeFreeSpin:
			FreeSpinGroupIDMap[int32(row.GetSelectionValue())] = true
		case slot.PayTypeLightning:
			RespinGroupIDMap[int32(row.GetSelectionValue())] = true
		}
	}

	// RespinTable
	for _, row := range protodatas.RespinTable {
		if _, ok := RespinGroupIDMap[row.GetGroupId()]; !ok {
			continue
		}
		logic.RespinTable = append(logic.RespinTable, row)

		SymbolGroupIDMap[row.GetRespinValue()] = true

		if row.GetIsLimit() == 1 {
			if row.GetLimitType() == "changeDistribution" {
				strs := strings.Split(row.GetLimitValue(), ":")
				if len(strs) != 2 {
					continue
				}
				f, _ := strconv.ParseFloat(strs[1], 64)
				SymbolGroupIDMap[uint32(f)] = true
			}
		}

	}

	// JackpotTable
	for _, row := range protodatas.JackpotTable {
		if row.GetGroupId() != int32(logic.SlotPattern.GetJackpotGroup()) {
			continue
		}
		logic.JackpotTable = append(logic.JackpotTable, row)
	}

	// PickGameTable
	for _, row := range protodatas.PickGameTable {
		if _, ok := PickGameIDMap[row.GetId()]; !ok {
			continue
		}

		logic.PickGameTable = append(logic.PickGameTable, row)

		BonusGroupIDMap[row.GetRewardValue()] = true
	}

	// FreeSpinTable
	for _, row := range protodatas.FreeSpinTable {
		if _, ok := FreeSpinGroupIDMap[row.GetGroupId()]; !ok {
			continue
		}

		logic.FreeSpinTable = append(logic.FreeSpinTable, row)

		SymbolGroupIDMap[row.GetFreeSymbolGroupId()] = true
	}

	// BonusTable
	for _, row := range protodatas.BonusTable {
		if _, ok := BonusGroupIDMap[uint32(row.GetGroupId())]; !ok {
			continue
		}

		logic.BonusTable = append(logic.BonusTable, row)
	}

	// SlotSymbolDistribution
	for _, row := range protodatas.SlotSymbolDistribution {
		if _, ok := SymbolGroupIDMap[row.GetGroupId()]; !ok {
			continue
		}
		logic.SlotSymbolDistribution = append(logic.SlotSymbolDistribution, row)
	}

}

// initJackpot 依照proto設定取得Jackpot基本資料，初始時只建立共有多少Jackpot
func (logic *SlotLogicBase) initJackpot() {
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

// Clean game data
func (logic *SlotLogicBase) Clean() {

	logic.GameBetMoney = 0
	if len(logic.BetRates) > 0 {
		logic.GameBetRate = int64(logic.BetRates[0])
	} else {
		logic.GameBetRate = 1
	}

	logic.GameResultManager = nil
	logic.GamePayResult = make([]*slot.PayResult, 0)

	logic.GamePayMoney = 0
	logic.GamePayCoupon = 0
	logic.GameWinMoney = 0
	logic.GameWinJackpot = 0

}

// UpdateState 更新遊戲狀態並傳送給玩家
func (logic *SlotLogicBase) UpdateState(state slot.GameState) {
	logic.GameState = state

	if logic.GameUser != nil {
		logic.SendStateMessageToUser(logic.GameUser)
	}
}

// UserEnter 玩家進入
func (logic *SlotLogicBase) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserEnter 發生錯誤, %v", rec)
		}
	}()

	logic.ComputeGameLogicBase.UserEnter(user, tableno, seatno)
	logic.GameStartTime = time.Now()

	logic.GameLog.GameBegin()

	logic.GameLog.AddGameStep("玩家進入")
	mlog.Debug("玩家進入")

	logic.GameUser = user

	logic.Clean()

	// 調整狀態為等待
	logic.UpdateState(slot.GameStateWait)
}

// UserRecover 玩家斷線重連
func (logic *SlotLogicBase) UserRecover(user *gamelogic.GameUser) {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserRecover 發生錯誤, %v", rec)
		}
	}()

	logic.ComputeGameLogicBase.UserRecover(user)
	logic.GameLog.AddGameStep("玩家斷線重連")
	mlog.Debug("玩家斷線重連")

	logic.GameUser = user

	logic.Clean()

	// 調整狀態為等待
	logic.UpdateState(slot.GameStateWait)
}

// UserLeave 玩家離開
func (logic *SlotLogicBase) UserLeave(user *gamelogic.GameUser) bool {

	defer func() {
		if rec := recover(); rec != nil {
			mlog.Error("UserLeave 發生錯誤, %v", rec)
		}
	}()

	logic.GameLog.AddGameStep("玩家離開")
	mlog.Debug("玩家離開")
	logic.GameUser = nil

	logic.Clean()

	return logic.ComputeGameLogicBase.UserLeave(user)
}

// GameEndLog call GameLog GameEnd()
func (logic *SlotLogicBase) GameEndLog() {
	if logic.GameUser != nil {
		logic.GameLog.GameEnd(
			map[int32]int64{logic.GameUser.UserID: logic.GameWinMoney - logic.GamePayMoney},
			map[int32]int64{logic.GameUser.UserID: -logic.GamePayCoupon},
		)
	}
}

// CalcFlowAmount 計算流水，增加本次異動值(贏的錢-花的錢)，使用禮碼不累計
func (logic *SlotLogicBase) CalcFlowAmount(user *gamelogic.GameUser) {

	var diff float64

	diff = float64(logic.GameWinMoney) - float64(logic.GamePayMoney)

	flowAmt := int64(math.Abs(diff))

	logic.GameLog.AddGameStep("流水增加 %d", flowAmt)
	mlog.Debug("流水增加 %d", flowAmt)

	user.FlowAmount += flowAmt
}

// UpdateUserScore 更新玩家分數
func (logic *SlotLogicBase) UpdateUserScore(user *gamelogic.GameUser) {
	if user.IsRobot() {
		return
	}

	logic.GameLog.AddGameStep("更新玩家金幣, 下注金幣: %d, 使用金幣: %d, 使用禮碼: %d, 贏得獎金: %d", logic.GamePayMoney+logic.GamePayCoupon, logic.GamePayMoney, logic.GamePayCoupon, logic.GameWinMoney)
	mlog.Debug("更新玩家金幣, 下注金幣: %d, 使用金幣: %d, 使用禮碼: %d, 贏得獎金: %d", logic.GamePayMoney+logic.GamePayCoupon, logic.GamePayMoney, logic.GamePayCoupon, logic.GameWinMoney)

	logic.UpdateUserScoreFunc(user.UserID, logic.GameWinMoney-logic.GamePayMoney, 0, -logic.GamePayCoupon)
}

// IsFreeSpin 檢查目前是否為FreeSpin狀態
func (logic *SlotLogicBase) IsFreeSpin() bool {
	if logic.FreeSpin != nil && logic.FreeSpin.Count >= 0 && logic.FreeSpin.Used > 0 {
		return true
	}

	return false
}
