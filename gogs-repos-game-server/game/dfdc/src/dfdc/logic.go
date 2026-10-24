package dfdc

import (
	"DataCfgSlot"
	"config"
	"event"
	"fmt"
	"gamelogic"
	"msg"
	"netproto"
	"strconv"
	"strings"

	"example.invalid/golang/protobuf/proto"
)

// Logic of DFDC Game Logic
type Logic struct {
	*gamelogic.ComputeGameLogicBase

	// Config
	SlotPatternID uint32
	BetRates      []int32 // 遊戲黃金倍率符號
	BetCounts     []int32 // 遊戲押注數量
	MaxBetRate    int32   // 最大黃金倍率
	MaxBetCount   int32   // 最大押注數量
	BigWinRule    []int32 // client動畫播放階段,以Bet為主
	BigWinNotify  int32   // 當下注16, Total=6400(16*400) 得獎超過6400才廣播.
	IsSlideShow   bool    // 中獎時，是否逐條派彩線輪播(未實做)
	IsRetriggered bool    // Free Spin 過程中，是否能再觸發Free Spin(未實做)
	FreeSpinLimit int32   // Free Spin最多可累積的數量
	EndActionTime int32   // 結算動畫超時
	MinBetMoney   int64   // 最低下注金額

	// Proto Datas
	SlotPattern            *DataCfgSlot.SlotPattern
	MainGameTable          []*DataCfgSlot.MainGameTable
	FreeSpinTable          []*DataCfgSlot.FreeSpinTable
	SymbolAttributes       []*DataCfgSlot.SymbolAttributes
	SlotPayTable           []*DataCfgSlot.SlotPayTable
	SlotPayLine            []*DataCfgSlot.SlotPayLine
	JackpotTable           []*DataCfgSlot.JackpotTable
	SlotSymbolDistribution []*DataCfgSlot.SlotSymbolDistribution

	Jackpots map[JackpotLevel]*Jackpot // Jackpot資料，共有4個獎池，map[{JackpotLevel}] *Jackpot

	DefaultReelNum int32          // 預設Reel數量，以MainGameTable第一筆做基準
	DefaultRowNum  int32          // 預設Row數量，以MainGameTable第一筆做基準
	PayLineMode    PayLineMode    // 賠付線模式
	PaylineMatcher PaylineMatcher // 賠付線比對器

	GameUser *gamelogic.GameUser // 當前玩家

	GameState     GameState // 當前遊戲狀態
	FreeSpinCount int32     // 免費遊戲剩餘次數
	FreeSpinGroup int32     // 免費遊戲資料

	GameBetRate       int32  // 玩家下注倍率
	GameBetMoney      int64  // 玩家下注金額
	GameIsFree        bool   // 這次是FreeSpin
	GameIsCoupon      bool   // 這次是Coupon
	GameSymbolGroupID uint32 // 依照下注黃金符號倍率，決定使用哪個SymbolGroupID
	GameReelNum       uint32 // 該局遊戲欄數
	GameRowNum        uint32 // 該局遊戲列數

	GameResult    GameResult   // 遊戲結果
	GamePayResult []*PayResult // 遊戲中獎內容

	GamePayMoney     int64 // 花費金額
	GamePayCoupon    int64 // 花費Coupon
	GameWinMoney     int64 // 贏得金額
	FreeSpinWinMoney int64 // 免費遊戲贏得金額
	GameWinFreeSpin  bool  // 贏得免費遊戲
	GameWinBonusGame bool  // 贏得額外遊戲

	JackpotGame *JackpotGame // 當前的額外遊戲

}

// NewLogic create logic for DFDC
func NewLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *Logic {
	logic := new(Logic)
	logic.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(t, tfManager, cfgs)

	logic.loadConfigs()

	logic.initProtoDatas()

	logic.DefaultReelNum = int32(logic.MainGameTable[0].GetReelNum())
	logic.DefaultRowNum = int32(logic.MainGameTable[0].GetRowNum())
	logic.PayLineMode = PayLineMode(logic.SlotPattern.GetLineType())
	switch logic.PayLineMode {
	case PayLineModeAll:
		logic.PaylineMatcher = NewPaylineAllMatcher(logic.SymbolAttributes)
	case PayLineModeFixed:
		logic.PaylineMatcher = NewPaylineFixedMatcher(logic.SymbolAttributes, logic.SlotPayLine)
	}

	logic.initJackpot()

	logic.RegisterBeforeGameStartFunc(func() {
	})
	logic.RegisterAfterGameStartFunc(func() {
		logic.GameLog.GameBegin()
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

	// Slot Configs
	{
		slotconfig := logic.Configs.GetConfig("slotconfig")

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

		// IsSlideShow
		{
			str := cfg["isSlideShow"]
			i, _ := strconv.ParseInt(str, 10, 32)
			if i == 1 {
				logic.IsSlideShow = true
			}
		}

		// IsRetriggered
		{
			{
				str := cfg["isRetriggered"]
				i, _ := strconv.ParseInt(str, 10, 32)
				if i == 1 {
					logic.IsRetriggered = true
				}
			}
		}

		// FreeSpinLimit
		{
			str := cfg["freeSpinLimit"]
			i, _ := strconv.ParseInt(str, 10, 32)
			logic.FreeSpinLimit = int32(i)
		}

		// EndActionTime
		{
			str := cfg["endactiontime"]
			i, _ := strconv.ParseInt(str, 10, 32)
			logic.EndActionTime = int32(i)
		}
	}

	// Game Configs
	{
		gameconfig := logic.Configs.GetConfig("gameserver")

		cfg := gameconfig.GetConfigSection("gameserver")

		// MinBetMoney
		{
			i, _ := strconv.ParseInt(cfg["betlimitmoney"], 10, 64)
			logic.MinBetMoney = i * 100
		}
	}

}

// initProtoDatas 取得該遊戲設定值所對應的proto data
//
// 由SlotPattern取得所對應的Table
func (logic *Logic) initProtoDatas() {
	// 整理可能會有的SymbolGroupID
	SymbolGroupIDMap := make(map[uint32]bool)
	// 整理可能會有的FreeSpinGroupID
	FreeSpinGroupIDMap := make(map[int32]bool)

	// SlotPattern
	for _, row := range protodata.SlotPattern {
		if row.GetId() == logic.SlotPatternID {
			logic.SlotPattern = row
			break
		}
	}

	// MainGameTable
	for _, row := range protodata.MainGameTable {
		if row.GetGroupId() != logic.SlotPattern.GetMainGameTableGroupId() {
			continue
		}
		logic.MainGameTable = append(logic.MainGameTable, row)

		SymbolGroupIDMap[row.GetSymbolGroupId()] = true
	}

	// SymbolAttributes
	for _, row := range protodata.SymbolAttributes {
		if row.GetGroupId() != logic.SlotPattern.GetSymbolAttributesGroupId() {
			continue
		}
		logic.SymbolAttributes = append(logic.SymbolAttributes, row)
	}

	// SlotPayTable
	for _, row := range protodata.SlotPayTable {
		if uint32(row.GetGroupId()) != logic.SlotPattern.GetPayTableGroupId() {
			continue
		}
		logic.SlotPayTable = append(logic.SlotPayTable, row)

		if row.GetPayType() == PayTypeFreeSpin.String() {
			gid, _ := strconv.ParseInt(row.GetPayout(), 10, 32)
			FreeSpinGroupIDMap[int32(gid)] = true
		}
	}

	// SlotPayLine
	for _, row := range protodata.SlotPayLine {
		if row.GetGroupId() != logic.SlotPattern.GetPayLineGroupId() {
			continue
		}
		logic.SlotPayLine = append(logic.SlotPayLine, row)
	}

	// JackpotTable
	for _, row := range protodata.JackpotTable {
		if uint32(row.GetGroupId()) != logic.SlotPattern.GetJackpotGroup() {
			continue
		}
		logic.JackpotTable = append(logic.JackpotTable, row)
	}

	// FreeSpinTable
	for _, row := range protodata.FreeSpinTable {
		if _, ok := FreeSpinGroupIDMap[row.GetGroupId()]; !ok {
			continue
		}
		logic.FreeSpinTable = append(logic.FreeSpinTable, row)

		SymbolGroupIDMap[row.GetFreeSymbolGroupId()] = true
	}

	// SlotSymbolDistribution
	for _, row := range protodata.SlotSymbolDistribution {
		if _, ok := SymbolGroupIDMap[row.GetGroupId()]; !ok {
			continue
		}
		logic.SlotSymbolDistribution = append(logic.SlotSymbolDistribution, row)
	}

}

// initJackpot 依照proto設定取得Jackpot基本資料，初始時只建立共有多少Jackpot
func (logic *Logic) initJackpot() {
	logic.Jackpots = make(map[JackpotLevel]*Jackpot)

	// 由設定的SlotPattern取得JackpotGroupID
	for _, row := range protodata.JackpotTable {
		if uint32(row.GetGroupId()) != logic.SlotPattern.GetJackpotGroup() {
			continue
		}
		jackpot := new(Jackpot)
		jackpot.origin = row
		jackpot.Level = JackpotLevel(row.GetLevel())
		logic.Jackpots[jackpot.Level] = jackpot
	}
}

// GameMessage 遊戲消息
func (logic *Logic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {

	if !logic.CheckGameUserMessage(user) {
		logic.KickUserFunc(user.UserID, "非法操作")
	}

	// 非遊戲訊息，不處理
	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	// 遊戲開始
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCGameStartID) {
		bet := &netproto.DFDC_StartBet{}
		proto.Unmarshal(gmsg.MsgData.([]byte), bet)

		if logic.Bet(user, bet.GetGoldenSymbol(), bet.GetMoney()) {
			logic.Spin(user)
		}

		return
	}

	// 玩家Bonus Game選擇
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCBonusGameIconID) {

		if logic.JackpotGame == nil {
			return
		}

		detail := &netproto.DFDC_BonusGameDetail{}
		proto.Unmarshal(gmsg.MsgData.([]byte), detail)

		logic.JackpotGame.Choose(detail.GetId(), detail.GetPlace(), detail.GetIcon())
		logic.GameLog.AddGameStep("用户 %d 選擇第: %d次; 選擇位置: %d, 圖案: %d", user.UserID, detail.GetId(), detail.GetPlace(), detail.GetIcon())

		if !logic.JackpotGame.IsFinished() {
			// 更新DB
			logic.UpdateBonusGame(user)
			return
		}

		logic.SendJackpotPoolMessage(user)

		// 結束遊戲
		logic.GamePayMoney = 0
		logic.GameWinBonusGame = false
		logic.GameWinMoney = logic.JackpotGame.WinMoney

		// 如果是FreeSpin中得獎要加到FreeSpinWinMoney
		if logic.FreeSpinCount > 0 && logic.JackpotGame.WinMoney > 0 {
			logic.FreeSpinWinMoney += logic.JackpotGame.WinMoney
			logic.UpdateFreeSpin(user)
		}

		logic.GameLog.AddGameStep("JackpotGame圖案順序: %s", logic.JackpotGame.GetProcessData())
		logic.GameLog.AddGameStep("JackpotGame獎金: %d", logic.GameWinMoney)

		logic.AddSlotGameHistoryLog(user, HistoryTypeJPNOLINK, GameTypeOPEN, logic.JackpotGame.GetProcessData())

		logic.ResetBonusGame(user)

		// 調整狀態為等待
		logic.GameState = GameStateWait
		logic.SendStateMessage(user)

		// 寫分
		logic.UpdateUserScore(user)

		logic.GameEndFunc()

		logic.GameState = GameStateBet
		logic.SendStateMessage(user)

		logic.JackpotGame = nil

		return
	}

	// 查詢遊戲歷程紀錄
	if gmsg.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCLoadGameHistoryID) {

		param := &netproto.DFDC_GetGameLog{}
		proto.Unmarshal(gmsg.MsgData.([]byte), param)

		logic.ReadSlotGameHistoryLog(user, param.GetPageIndex(), param.GetPageSize(), param.GetStartTime(), param.GetEndTime())

		return
	}

}

// PiSaiMessage 比賽消息
func (logic *Logic) PiSaiMessage(user *gamelogic.GameUser, gmsg *msg.Message) {

}

// UserEnter 玩家進入
func (logic *Logic) UserEnter(user *gamelogic.GameUser, tableno, seatno int32) {
	logic.GameLog.AddGameStep("玩家進入")
	logic.GameUser = user

	logic.ComputeGameLogicBase.UserEnter(user, tableno, seatno)

	// 讀取 Jackpot 資訊
	logic.ReadJackpotPool()
	// 讀取 BonusGame 資訊
	logic.ReadBonusGame(user)
	// 讀取 FreeSpin 資訊
	logic.ReadFreeSpinTimes(user)

	// 傳送黃金符號倍率
	logic.SendGoldenSymbolMessage(user)
	// 傳送遊戲設置內容
	logic.SendSettingMessage(user)

	// 過一秒在傳送獎池資訊
	logic.TimerFuncManager.NewNamedTimer(int32(1), func() {
		logic.SendJackpotPoolMessage(user)
		logic.SendJackpotGameDetailMessage(user)
		if logic.FreeSpinCount > 0 {
			logic.SendFreeSpinBetMessage(user)
			logic.SendFreeSpinCountMessage(user)
			logic.SendStateMessage(user)
		}
	}, fmt.Sprintf("gametimeoutcd_%d", logic.TableNo()))

	// 等待1秒後告知Client玩家狀態
	logic.RegisterAnimation(AnimationGameStart, int32(1), func() {
		logic.SendStateMessage(user)
	})
}

// UserLeave 玩家離開
func (logic *Logic) UserLeave(user *gamelogic.GameUser) bool {
	logic.GameLog.AddGameStep("玩家離開")
	logic.GameUser = nil

	return logic.ComputeGameLogicBase.UserLeave(user)
}

// UserRecover 玩家回來
func (logic *Logic) UserRecover(user *gamelogic.GameUser) {
	logic.GameLog.AddGameStep("玩家回來")
	logic.GameUser = user

	logic.ComputeGameLogicBase.UserRecover(user)
}

// Clean 清除遊戲資料
func (logic *Logic) Clean() {
	if logic.GameWinBonusGame {
		return
	}

	logic.GameBetRate = 0
	logic.GameBetMoney = 0
	logic.GameSymbolGroupID = 0
	logic.GameReelNum = 0
	logic.GameRowNum = 0

	logic.GameResult = nil
	logic.GamePayResult = make([]*PayResult, 0)

	logic.GamePayMoney = 0
	logic.GamePayCoupon = 0
	logic.GameWinMoney = 0
	logic.FreeSpinWinMoney = 0

	if logic.GameIsFree && logic.FreeSpinCount == 0 && logic.GameUser != nil {
		logic.ReadFreeSpinTimes(logic.GameUser)
	}

	logic.GameIsFree = false
	logic.GameWinFreeSpin = false
	logic.GameWinBonusGame = false

	logic.JackpotGame = nil

}
