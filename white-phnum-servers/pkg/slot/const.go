package slot

// PayType 獎勵方式
type PayType string

// PayTypes
const (
	PayTypeChip           PayType = "chip"
	PayTypeChipXRate      PayType = "chipXrate"
	PayTypePickGame       PayType = "pickgame"
	PayTypeJackpot        PayType = "jackpot"
	PayTypeRespin         PayType = "respin"
	PayTypeFreeSpin       PayType = "free_spins"
	PayTypeMysteryJackpot PayType = "mysteryjackpot"
	PayTypeSelectGameMode PayType = "selectgamemode"
	PayTypeLightning      PayType = "lightning"
)

func (pt PayType) String() string {
	return string(pt)
}

// PayLineMode 賠付線方式
type PayLineMode string

// PayLineModes
const (
	PayLineModeAll   PayLineMode = "all"
	PayLineModeFixed PayLineMode = "fixed"
)

func (plm PayLineMode) String() string {
	return string(plm)
}

// Int get PayLineMode number
func (plm PayLineMode) Int() int32 {
	switch plm {
	default:
		return -1
	case PayLineModeAll:
		return 1
	case PayLineModeFixed:
		return 0
	}
}

// GameState 遊戲狀態
type GameState int32

// 階段狀態
const (
	GameStateBet         = iota // 下注狀態
	GameStateAward              // 開獎
	GameStateAwardAction        // 開獎動畫
	GameStateEndAction          // 結算動畫
	GameStateWait               // 等待遊戲開始
	GameStateUserAction         // 等待玩家動作(各遊戲可能有不同的事情要做)
)

// BetStates
const (
	BetStateSuccess        int32 = 0  // 下注成功
	BetStateMoneyNotEnough int32 = -1 // 玩家金額不足
	BetStateBetTypeError   int32 = -2 // 下注類型未找到
	BetStateIsBanker       int32 = -3 // 莊家不能下注
	BetStateBetTypeLimit   int32 = -4 // 此類下注已達到上限
	BetStateStepError      int32 = -5 // 非下注階段
	BetStateMoneyLessMin   int32 = -6 // 玩家金額小於下注限制
)

// 賠率計算類型
const (
	ConditionRate   = "betRate"
	ConditionAmount = "betAmount"
)

// LinkType 賠線類型
type LinkType string

// LinkTypes
const (
	LinkTypeLeft     LinkType = "left"
	LinkTypeRight    LinkType = "right"
	LinkTypeAny      LinkType = "any"
	LinkTypeStraight LinkType = "straight"
)

func (t LinkType) String() string {
	return string(t)
}

// 歷程紀錄類型
const (
	HistoryTypeMAIN      = 1 // 主遊戲
	HistoryTypeBONUS     = 2 // BonusGame
	HistoryTypeFREE      = 3 // FreeGame
	HistoryTypeJPNOLINK  = 4 // 單機 JP
	HistoryTypeJPLINK    = 5 // 連線 JP
	HistoryTypeRESPIN    = 6 // 重轉
	HistoryTypeLIGHTNING = 7 // Lightning結算
)

// 遊戲歷程紀錄類型
const (
	GameTypeREEL = 1 // 輪軸資訊
	GameTypeOPEN = 2 // 翻圖案，三個一樣結束型
)
