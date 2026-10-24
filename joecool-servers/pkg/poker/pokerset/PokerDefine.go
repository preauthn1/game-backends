package pokerset

const (
)

const (
	POKER_OneColorCardsCount        int32 = 13      // 一種花色有幾張牌
	POKER_ColorsCount               int32 = 4       // 幾種花色
	POKER_SetCardsCount             int32 = POKER_ColorsCount * POKER_OneColorCardsCount // 一副牌的數量(不含Joker)
	POKER_JokerCardsCount           int32 = 2       // Joker數量
	POKER_MaxSet                    int32 = 10		// 最多用幾副牌洗牌
	POKER_MinStraightCount			int = 2			// 檢查順面所需最小張數
	POKER_PatternBasicCardCount		int = 5			// 牌型基本張數
)

const (
	LEN_SingleCardCode				int = 2			// 一張牌的代碼文字長度
)

// 比牌結果
const (
	POKER_Compare_Even				int = iota		// 平手
	POKER_Compare_Dealer							// 莊家贏
	POKER_Compare_Player							// 玩家贏
)

// 牌型
const (
	POKER_Pattern_None				int32 = iota	// 無
	POKER_Pattern_HighCard							// 高牌
	POKER_Pattern_Pair								// 對子
	POKER_Pattern_TwoPairs							// 兩對
	POKER_Pattern_ThreeOfAKind						// 三條
	POKER_Pattern_Straight							// 順子
	POKER_Pattern_Flush								// 同花
	POKER_Pattern_FullHouse							// 葫蘆
	POKER_Pattern_FourOfAKind						// 四條
	POKER_Pattern_StraightFlush						// 同花順
	POKER_Pattern_RoyalFlush						// 皇家同花順
	POKER_Pattern_Dragon							// 一條龍
	POKER_Pattern_DragonFlush						// 同花一條龍
	POKER_Pattern_Max
)

// 花色
const (
	POKER_CardColor_Club			int32 = iota	// 梅花
	POKER_CardColor_Diamond							// 方塊
	POKER_CardColor_Heart							// 紅桃
	POKER_CardColor_Spade							// 黑桃
	POKER_CardColor_Other							// 其他
	POKER_CardColor_Max
)

// 數字
const (
	POKER_CardNo_None				int32 = iota
	POKER_CardNo_1
	POKER_CardNo_2
	POKER_CardNo_3
	POKER_CardNo_4
	POKER_CardNo_5
	POKER_CardNo_6
	POKER_CardNo_7
	POKER_CardNo_8
	POKER_CardNo_9
	POKER_CardNo_10
	POKER_CardNo_J
	POKER_CardNo_Q
	POKER_CardNo_K
	POKER_CardNo_A
	POKER_CardNo_Joker1								// 小王
	POKER_CardNo_Joker2								// 大王
	POKER_CardNo_Hide
	POKER_CardNo_Max
)

// 牌的資料以編號形式表達
const (
	POKER_CardID_None				int32 = iota
	// Club
	POKER_CardID_C1
	POKER_CardID_C2
	POKER_CardID_C3
	POKER_CardID_C4
	POKER_CardID_C5
	POKER_CardID_C6
	POKER_CardID_C7
	POKER_CardID_C8
	POKER_CardID_C9
	POKER_CardID_C0
	POKER_CardID_CJ
	POKER_CardID_CQ
	POKER_CardID_CK
	// Diamond
	POKER_CardID_D1
	POKER_CardID_D2
	POKER_CardID_D3
	POKER_CardID_D4
	POKER_CardID_D5
	POKER_CardID_D6
	POKER_CardID_D7
	POKER_CardID_D8
	POKER_CardID_D9
	POKER_CardID_D0
	POKER_CardID_DJ
	POKER_CardID_DQ
	POKER_CardID_DK
	// Heart
	POKER_CardID_H1
	POKER_CardID_H2
	POKER_CardID_H3
	POKER_CardID_H4
	POKER_CardID_H5
	POKER_CardID_H6
	POKER_CardID_H7
	POKER_CardID_H8
	POKER_CardID_H9
	POKER_CardID_H0
	POKER_CardID_HJ
	POKER_CardID_HQ
	POKER_CardID_HK
	// Spade
	POKER_CardID_S1
	POKER_CardID_S2
	POKER_CardID_S3
	POKER_CardID_S4
	POKER_CardID_S5
	POKER_CardID_S6
	POKER_CardID_S7
	POKER_CardID_S8
	POKER_CardID_S9
	POKER_CardID_S0
	POKER_CardID_SJ
	POKER_CardID_SQ
	POKER_CardID_SK
	// Joker
	POKER_CardID_J1
	POKER_CardID_J2
	// 蓋牌
	POKER_CardID_Hide
	POKER_CardID_Max
)

//-----------------------------------------------------------------------------
// 花色字串
var g_map_strCardColor = map[int32]string {
	POKER_CardColor_Club:		"梅花",
	POKER_CardColor_Diamond:	"方块",
	POKER_CardColor_Heart:		"红桃",
	POKER_CardColor_Spade:		"黑桃",
	POKER_CardColor_Other:		"",
}

// 數字字串
var g_map_strCardNumber = map[int32]string {
	POKER_CardNo_1:			"1",
	POKER_CardNo_2:			"2",
	POKER_CardNo_3:			"3",
	POKER_CardNo_4:			"4",
	POKER_CardNo_5:			"5",
	POKER_CardNo_6:			"6",
	POKER_CardNo_7:			"7",
	POKER_CardNo_8:			"8",
	POKER_CardNo_9:			"9",
	POKER_CardNo_10:		"10",
	POKER_CardNo_J:			"J",
	POKER_CardNo_Q:			"Q",
	POKER_CardNo_K:			"K",
	POKER_CardNo_A:			"A",
	POKER_CardNo_Joker1:	"小王",
	POKER_CardNo_Joker2:	"大王",
	POKER_CardNo_Hide:		"盖牌",
}

// 花色代碼
var g_map_strColorCode = map[int32]string {
	POKER_CardColor_Club:		"c",
	POKER_CardColor_Diamond:	"d",
	POKER_CardColor_Heart:		"h",
	POKER_CardColor_Spade:		"s",
	POKER_CardColor_Other:		"j",
}

var g_map_i32ColorByCode = map[string]int32 {
	"c":	POKER_CardColor_Club,
	"d":	POKER_CardColor_Diamond,
	"h":	POKER_CardColor_Heart,
	"s":	POKER_CardColor_Spade,
	"j":	POKER_CardColor_Other,
}

// 數字代碼
var g_map_strNumberCode = map[int32]string {
	POKER_CardNo_None:		"d",
	POKER_CardNo_1:			"1",
	POKER_CardNo_2:			"2",
	POKER_CardNo_3:			"3",
	POKER_CardNo_4:			"4",
	POKER_CardNo_5:			"5",
	POKER_CardNo_6:			"6",
	POKER_CardNo_7:			"7",
	POKER_CardNo_8:			"8",
	POKER_CardNo_9:			"9",
	POKER_CardNo_10:		"0",
	POKER_CardNo_J:			"J",
	POKER_CardNo_Q:			"Q",
	POKER_CardNo_K:			"K",
	POKER_CardNo_A:			"A",
	POKER_CardNo_Joker1:	"a",
	POKER_CardNo_Joker2:	"b",
	POKER_CardNo_Hide:		"c",
}

var g_map_i32NumberByCode = map[string]int32 {
	"d":	POKER_CardNo_None,
	"1":	POKER_CardNo_1,
	"2":	POKER_CardNo_2,
	"3":	POKER_CardNo_3,
	"4":	POKER_CardNo_4,
	"5":	POKER_CardNo_5,
	"6":	POKER_CardNo_6,
	"7":	POKER_CardNo_7,
	"8":	POKER_CardNo_8,
	"9":	POKER_CardNo_9,
	"0":	POKER_CardNo_10,
	"J":	POKER_CardNo_J,
	"Q":	POKER_CardNo_Q,
	"K":	POKER_CardNo_K,
	"A":	POKER_CardNo_A,
	"a":	POKER_CardNo_Joker1,
	"b":	POKER_CardNo_Joker2,
	"c":	POKER_CardNo_Hide,
}

//牌型字串
var g_map_strPattern = map[int32]string{
	POKER_Pattern_None:					"无",
	POKER_Pattern_HighCard:				"高牌",
	POKER_Pattern_Pair:					"对子",
	POKER_Pattern_TwoPairs:				"两对",
	POKER_Pattern_ThreeOfAKind:			"三条",
	POKER_Pattern_Straight:				"顺子",
	POKER_Pattern_Flush:				"同花",
	POKER_Pattern_FullHouse:			"葫芦",
	POKER_Pattern_FourOfAKind:			"四条",
	POKER_Pattern_StraightFlush:		"同花顺",
	POKER_Pattern_RoyalFlush:			"皇家同花顺",
	POKER_Pattern_Dragon:				"一条龙",
	POKER_Pattern_DragonFlush:			"同花一条龙",
}

//-----------------------------------------------------------------------------
// 各牌型張數
var g_map_iPatternCardCount = map[int32]int {
	POKER_Pattern_None:					0,	// 無
	POKER_Pattern_HighCard:				1,	// 高牌
	POKER_Pattern_Pair:					2,	// 對子
	POKER_Pattern_TwoPairs:				4,	// 兩對
	POKER_Pattern_ThreeOfAKind:			3,	// 三條
	POKER_Pattern_Straight:				3,	// 順子
	POKER_Pattern_Flush:				3,	// 同花
	POKER_Pattern_FullHouse:			5,	// 葫蘆
	POKER_Pattern_FourOfAKind:			4,	// 四條
	POKER_Pattern_StraightFlush:		3,	// 同花順
	POKER_Pattern_RoyalFlush:			5,	// 皇家同花順
	POKER_Pattern_Dragon:				13,	// 一條龍
	POKER_Pattern_DragonFlush:			13,	// 同花一條龍
}

//-----------------------------------------------------------------------------
