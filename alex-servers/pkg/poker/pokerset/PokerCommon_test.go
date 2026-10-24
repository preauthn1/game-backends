package pokerset

import (
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

// 將 CardID 轉換為 PokerCard
func TestCardID2PokerCard(t *testing.T) {
	for i := POKER_CardID_C1; i < POKER_CardID_Max; i++ {
		pCard := CardID2PokerCard(i)
		if pCard != nil {
			mlog.Debug("CardID[%d], [%s]", i, pCard.String())
		}
	}
}

// 將 CardCode 轉換為 PokerCardSet
func TestCardCode2PokerCard(t *testing.T) {
	strCodeString := ""
	iCount := util.RndInt(1, 21)
	for i := 0; i < iCount; i++ {
		i32Color := int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Max)))
		strCodeString += g_map_strColorCode[i32Color]
		if i32Color == POKER_CardColor_Other {
			i32Number := int32(util.RndInt(int(POKER_CardNo_Joker1), int(POKER_CardNo_Hide)))
			strCodeString += g_map_strNumberCode[i32Number]
		} else {
			i32Number := int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
			strCodeString += g_map_strNumberCode[i32Number]
		}
	}
	mlog.Debug("strCodeString: Count[%d][%s]", iCount, strCodeString)

	pPokerSet := CardCode2PokerCardSet(strCodeString)
	mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
}

// 牌型字串
func TestPatternString(t *testing.T) {
	for i := POKER_Pattern_None - 1; i < POKER_Pattern_Max+1; i++ {
		mlog.Debug("i = %d, 牌型[%s]", i, PatternString(i))
	}
}

// 各牌型張數
func TestPatternCardCount(t *testing.T) {
	for i := POKER_Pattern_None - 1; i < POKER_Pattern_Max+1; i++ {
		mlog.Debug("i = %d, 牌型[%s], 張數[%d]", i, PatternString(i), PatternCardCount(i))
	}
}
