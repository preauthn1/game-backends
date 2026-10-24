package pokerset

import (
	"fmt"
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

func Init()  {
	conf.CLogLevel = 0
	conf.FLogLevel = 0
}

func TestPokerCardSet_NewSet(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	pPokerSet.NewSet(1,2)

	mlog.Debug("建立新牌組：%d 張牌", pPokerSet.CardCount())
	mlog.Debug("牌組[%s]", pPokerSet.String())
	mlog.Debug("代碼[%s]", pPokerSet.CodeString())
}

func TestPokerCardSet_Shuffle(t *testing.T) {
	Init()
	pPokerSet := NewPokerCardSet()
	i32SetCount := int32(1)
	i32JokerCount := int32(2)
	pPokerSet.NewSet(i32SetCount,i32JokerCount)
	mlog.Debug("建立新牌組：牌組數[%d], Joker數[%d], 張牌[%d]", i32SetCount, i32JokerCount, pPokerSet.CardCount())
	mlog.Debug("洗牌後[%s]", pPokerSet.String())
	pPokerSet.Shuffle()
	mlog.Debug("洗牌後[%s]", pPokerSet.String())
}

func TestPokerCardSet_FindCard(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for i := 0; i < 10; i++ {
		pCard := new(PokerCard)
		pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
		pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
		pPokerSet.AddSingleCard(pCard.CodeString())
	}
	mlog.Debug("牌組[%s]", pPokerSet.String())

	pCard := new(PokerCard)
	pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
	pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
	mlog.Debug("尋找牌[%s], 結果[%t]", pCard.String(), pPokerSet.FindCard(pCard))

	i32Index := int32(util.RndInt(0, pPokerSet.CardCount()))
	mlog.Debug("尋找牌[%s], 結果[%t]", pPokerSet.ary_pCards[i32Index].String(), pPokerSet.FindCard(pPokerSet.ary_pCards[i32Index]))
}

func TestPokerCardSet_AddSingleCard(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for i := 0; i < 10; i++ {
		pCard := new(PokerCard)
		pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
		pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
		pPokerSet.AddSingleCard(pCard.CodeString())
		mlog.Debug("加入[%s]，牌組[%s]", pCard.String(), pPokerSet.String())
	}
}

func TestPokerCardSet_AddCardSet(t *testing.T) {
	Init()
	pMainSet := new(PokerCardSet)
	mlog.Debug("主牌組[%d張][%s]", pMainSet.CardCount(), pMainSet.String())
	pAddedSet := new(PokerCardSet)
	for i := 0; i < 10; i++ {
		pCard := new(PokerCard)
		pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
		pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
		pAddedSet.AddSingleCard(pCard.CodeString())
	}
	pMainSet.AddCardSet(pAddedSet)
	mlog.Debug("加入牌組[%d張][%s]", pAddedSet.CardCount(), pAddedSet.String())
	mlog.Debug("主牌組[%d張][%s]", pMainSet.CardCount(), pMainSet.String())
}

func TestPokerCard_CardID(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	pPokerSet.NewSet(1,2)

	pCard := NewPokerCard(POKER_CardColor_Other, POKER_CardNo_Hide)
	pPokerSet.AddSingleCard(pCard.CodeString())

	for i := 0; i < pPokerSet.CardCount(); i++ {
		mlog.Debug("[%s], CardID[%d]", pPokerSet.ary_pCards[i].String(), pPokerSet.ary_pCards[i].CardID())
	}
}

func TestPokerCardSet_RemoveSingleCard(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for i := 0; i < 15; i++ {
		pCard := new(PokerCard)
		pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
		pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
		pPokerSet.AddSingleCard(pCard.CodeString())
	}
	mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())

	map_i32CardCount := make(map[int32]int)
	for _, pCard := range pPokerSet.ary_pCards {
		if pCard != nil {
			map_i32CardCount[pCard.CardID()]++
		}
	}

	i32Index := int32(util.RndInt(0, pPokerSet.CardCount()))
	pCard := pPokerSet.ary_pCards[i32Index]
	for i32ID, iCount := range map_i32CardCount {
		if iCount > 1 {
			pCard = CardID2PokerCard(i32ID)
		}
	}
	mlog.Debug("移除[%s]", pCard.String())
	pPokerSet.RemoveSingleCard(pCard)
	mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
}

func TestPokerCardSet_RemoveCardSet(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for i := 0; i < 15; i++ {
		pCard := new(PokerCard)
		pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
		pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
		pPokerSet.AddSingleCard(pCard.CodeString())
	}
	mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())

	pRemoveSet := new(PokerCardSet)
	iIndex := util.RndInt(0, pPokerSet.CardCount())
	for i := iIndex; i < iIndex + 3; i++ {
		if i >= pPokerSet.CardCount() {
			break
		}

		pRemoveSet.AddSingleCard(pPokerSet.ary_pCards[i].CodeString())
	}
	mlog.Debug("移除牌組[%d張][%s]", pRemoveSet.CardCount(), pRemoveSet.String())
	pPokerSet.RemoveCardSet(pRemoveSet)
	mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
}

func TestPokerCardSet_SetCardSet(t *testing.T) {
	Init()
	strCodeString := ""
	iCount := util.RndInt(1, 21)
	if iCount % 3 == 0 {
		iCount = 0
	} else {
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
	}
	//strCodeString = "c2jbcQhJs7dKc1jakqh7zija"
	mlog.Debug("strCodeString: Count[%d][%s]", iCount, strCodeString)

	pPokerSet := new(PokerCardSet)
	//pPokerSet.NewSet(1,2)
	//mlog.Debug("原牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
	pPokerSet.SetCardSet(strCodeString)
	mlog.Debug("新牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
}

func TestPokerCardSet_Deal(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	pPokerSet.NewSet(1,0)
	pPokerSet.Shuffle()
	mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())

	pHandCard := pPokerSet.Deal(5)
	if pHandCard == nil {
		mlog.Debug("發牌失敗")
	} else {
		mlog.Debug("手牌[%d張][%s]", pHandCard.CardCount(), pHandCard.String())
		mlog.Debug("牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
	}
}

func TestPokerCardSet_DrawCards(t *testing.T) {
	Init()
	// 建立牌組
	pPokerSet := new(PokerCardSet)
	for i := 0; i < 15; i++ {
		pCard := new(PokerCard)
		pCard.I32Color = int32(util.RndInt(int(POKER_CardColor_Club), int(POKER_CardColor_Other)))
		pCard.I32Number = int32(util.RndInt(int(POKER_CardNo_1), int(POKER_CardNo_A)))
		pPokerSet.AddSingleCard(pCard.CodeString())
	}
	mlog.Debug("母牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())

	// 要抽出的牌，可能包含母牌組中沒有的牌
	mlog.Debug("----- 要抽出的牌，可能包含母牌組中沒有的牌 -----")
	strCodeString := ""
	iCount := util.RndInt(1, 11)
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
	pTemp1 := CardCode2PokerCardSet(strCodeString)
	if pTemp1 != nil {
		mlog.Debug("要抽的牌[%d張][%s]", pTemp1.CardCount(), pTemp1.String())
	}
	pDraw1 := pPokerSet.DrawCards(strCodeString)
	if pDraw1.CardCount() <= 0 {
		mlog.Debug("抽牌失敗!")
	} else {
		mlog.Debug("抽出的牌[%d張][%s]", pDraw1.CardCount(), pDraw1.String())
	}
	mlog.Debug("抽牌後母牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())

	// 要抽出的牌，從母牌組中隨機取得
	mlog.Debug("----- 要抽出的牌，從母牌組中隨機取得 -----")
	pTemp2 := new(PokerCardSet)
	for i := 0; i < pPokerSet.CardCount(); i++ {
		iRnd := util.RndInt(0, 2)
		if iRnd != 0 {
			pTemp2.AddSingleCard(pPokerSet.ary_pCards[i].CodeString())
		}
	}
	mlog.Debug("要抽的牌[%d張][%s]", pTemp2.CardCount(), pTemp2.String())
	pDraw2 := pPokerSet.DrawCards(pTemp2.CodeString())
	if pDraw2.CardCount() <= 0 {
		mlog.Debug("抽牌失敗!")
	} else {
		mlog.Debug("抽出的牌[%d張][%s]", pDraw2.CardCount(), pDraw2.String())
	}
	mlog.Debug("抽牌後母牌組[%d張][%s]", pPokerSet.CardCount(), pPokerSet.String())
}

func TestPokerCardSet_Sort(t *testing.T) {
	Init()
	mlog.Debug("========== 數字 1 ==========")
	pPokerSet1 := new(PokerCardSet)
	pPokerSet1.NewSet(1,2)

	pPokerSet1.Shuffle()
	mlog.Debug("----- 依花色排序，由小到大 -----")
	mlog.Debug("排序前[%s]", pPokerSet1.String())
	pPokerSet1.Sort(true, false, true)
	mlog.Debug("排序後[%s]", pPokerSet1.String())

	pPokerSet1.Shuffle()
	mlog.Debug("----- 依花色排序，由大到小 -----")
	mlog.Debug("排序前[%s]", pPokerSet1.String())
	pPokerSet1.Sort(true, false, false)
	mlog.Debug("排序後[%s]", pPokerSet1.String())

	pPokerSet1.Shuffle()
	mlog.Debug("----- 依數字排序，由小到大 -----")
	mlog.Debug("排序前[%s]", pPokerSet1.String())
	pPokerSet1.Sort(false, false, true)
	mlog.Debug("排序後[%s]", pPokerSet1.String())

	pPokerSet1.Shuffle()
	mlog.Debug("----- 依數字排序，由大到小 -----")
	mlog.Debug("排序前[%s]", pPokerSet1.String())
	pPokerSet1.Sort(false, false, false)
	mlog.Debug("排序後[%s]", pPokerSet1.String())

	mlog.Debug("========== Ace ==========")
	pPokerSetA := new(PokerCardSet)
	pPokerSetA.NewSet(1,2)

	pPokerSetA.Shuffle()
	mlog.Debug("----- 依花色排序，由小到大 -----")
	mlog.Debug("排序前[%s]", pPokerSetA.String())
	pPokerSetA.Sort(true,true, true)
	mlog.Debug("排序後[%s]", pPokerSetA.String())

	pPokerSetA.Shuffle()
	mlog.Debug("----- 依花色排序，由大到小 -----")
	mlog.Debug("排序前[%s]", pPokerSetA.String())
	pPokerSetA.Sort(true,true,false)
	mlog.Debug("排序後[%s]", pPokerSetA.String())

	pPokerSetA.Shuffle()
	mlog.Debug("----- 依數字排序，由小到大 -----")
	mlog.Debug("排序前[%s]", pPokerSetA.String())
	pPokerSetA.Sort(false,true, true)
	mlog.Debug("排序後[%s]", pPokerSetA.String())

	pPokerSetA.Shuffle()
	mlog.Debug("----- 依數字排序，由大到小 -----")
	mlog.Debug("排序前[%s]", pPokerSetA.String())
	pPokerSetA.Sort(false,true,false)
	mlog.Debug("排序後[%s]", pPokerSetA.String())
}

// 是否為對子
func TestIsPair(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"s7",
		"h2c7s1s0d8",
		"h2s8c2d1s0",
		"h3c3d3s8h4",
		"c5s5h5d5s9",
		"c5s6h6d5s9",
		"d6d7cJcKsK",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false,false,true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為兩對
func TestIsTwoPairs(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"d1s7c5",
		"d1s1c1",
		"c5s5",
		"h2c7s1s0d8",
		"h2s2c8d1s0",
		"h3c3d3s8h4",
		"c5s5h5d5s9",
		"c5s6h6d5s9",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false,false,true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為三條
func TestIsThreeOfKind(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"s1c9",
		"c5s5",
		"h2c7s1s0d8",
		"h2s2c8d1s0",
		"h3c3d3s8h4",
		"h3c3d3s4h4",
		"c5s5h5d5s9",
		"c5s6h6d5s9",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false,false,true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為順子
func TestIsStraight(t *testing.T) {
	Init()
	mlog.Debug("----- 5 張牌 -----")
	ary_strCodeString := []string{
		"h2h7h1h0d8",
		"h2s3c4d5s6",
		"h3c3d3s8h4",
		"c0sJhQdKs1",
		"c5s6h6d5s9",
		"d6d2dKd0d8",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, true, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}

	mlog.Debug("----- 7 張牌 -----")
	ary_strCodeString = []string{
		"h2h7h1h0d8s6d9",
		"h2d2s3c4d5s6s8",
		"h3c3d3s8h4s4c5",
		"c0sJhQdKs1s7d3",
		"c5s6h6d5s9c1dK",
		"d6d2dKd0d8cJs7",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, true, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為同花
func TestIsFlush(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"s1s9",
		"c5s5",
		"h2h7h1h0d8",
		"h2s2c8d1s0",
		"h3c3d3s8h4",
		"c5s5h5d5s9",
		"c5s6h6d5s9",
		"d6d2dKd0d8",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, false, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為葫蘆
func TestIsFullHouse(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"s1c9",
		"c5s5",
		"h2c7s1s0d8",
		"h2s2c8d1s0",
		"h3c3d3s8h4",
		"h3c3d3s4h4",
		"c5s5h5d5s9",
		"c5s6h6d5s9",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, false, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為四條
func TestIsFourOfKind(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"s1c9",
		"c5s5",
		"h2c7s1s0d8",
		"h2s2c8d1s0",
		"h3c3d3s8h4",
		"h3c3d3s4h4",
		"c5s5h5d5s9",
		"c5s6h6d5s9",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, false, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為同花順
func TestIsStraightFlush(t *testing.T) {
	Init()
	mlog.Debug("----- 5 張牌 -----")
	ary_strCodeString := []string{
		"h2h7h1h0d8",
		"c2c3c4c5c6",
		"h3c3d3s8h4",
		"c0sJhQdKs1",
		"c5s6h6d5s9",
		"d6d2dKd0d8",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, true, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}

	mlog.Debug("----- 7 張牌 -----")
	ary_strCodeString = []string{
		"h2h7h1h0d8s6d9",
		"h1d2d3d4d5d6s7",
		"h3c3d3s8h4s4c5",
		"c0sJhQdKs1s7d3",
		"c5s6h6d5s9c1dK",
		"d6d2dKd0d8cJs7",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, true, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為皇家同花順
func TestIsRoyalFlush(t *testing.T) {
	Init()
	ary_strCodeString := []string{
		"h1h2h3h4d5",
		"c2c3c4c5c6",
		"c0sJhQdKs1",
		"d0dJdQdKd1",
		"c0cJcQcKc1",
	}

	for i := 0; i < len(ary_strCodeString); i++ {
		pPoker := new(PokerCardSet)
		pPoker.SetCardSet(ary_strCodeString[i])
		pPoker.Sort(false, true, true)
		mlog.Debug("牌組[%s][%s]", pPoker.String(), g_map_strPattern[pPoker.Pattern()])
	}
}

// 是否為一條龍	todo

// 是否為同花一條龍	todo

// 檢查是否有順子
func TestIsNumberInOrder(t *testing.T) {
	Init()
	pPoker := NewPokerCardSet()
	for i := 0; i < 20; i++ {
		pPoker.NewSet(1,0)
		pPoker.Shuffle()
		pTemp := pPoker.Deal(7)
		iCount := util.RndInt(3,6)
		bAsAce := false
		iRnd := util.RndInt(0,2)
		if iRnd == 0 {
			bAsAce = false
		} else {
			bAsAce = true
		}
		pTemp.Sort(false, bAsAce, true)
		bOrder := pTemp.isNumberInOrder(iCount)
		mlog.Debug("牌組[%s], 順子張數[%d], 順子[%t], bAsAce[%t]", pTemp.CodeString(), iCount, bOrder, bAsAce)
	}
}

// 取得高牌
func TestGetHighCard(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_Pair, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得對子
func TestPokerCardSet_GetPair(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_Pair, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得兩對
func TestPokerCardSet_GetTwoPairs(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_TwoPairs, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得三條
func TestPokerCardSet_GetThreeOfAKind(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_ThreeOfAKind, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得順子
func TestPokerCardSet_GetStraight(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_Straight, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得同花
func TestPokerCardSet_GetFlush(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_Flush, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得葫蘆
func TestPokerCardSet_GetFullHouse(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_FullHouse, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得四條
func TestPokerCardSet_GetFourOfAKind(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_FourOfAKind, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得同花順
func TestPokerCardSet_GetStraightFlush(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0,2)
		iCount := 5
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_StraightFlush, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得皇家同花順
func TestPokerCardSet_GetRoyalFlush(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		iRnd := util.RndInt(0, 2)
		iCount := 0
		if iRnd == 0 {
			iCount = 5
		} else {
			iCount = 7
		}

		pSet := pPoker.DealByPattern(POKER_Pattern_RoyalFlush, iCount)
		pSet.Sort(false, true, true)
		mlog.Debug("張數[%d], 牌型[%s], 牌組[%s]", iCount, g_map_strPattern[pSet.Pattern()], pSet.CodeString())
	}
}

// 取得一條龍	todo

// 取得同花一條龍	todo

// 依牌型取得牌組
func TestPokerCardSet_GetCardSetsByPattern(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for i := POKER_Pattern_HighCard; i <= POKER_Pattern_RoyalFlush; i++ {
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		ary_pCardSet := pPoker.GetCardSetsByPattern(i)

		str := fmt.Sprintf("牌型[%s]：", g_map_strPattern[i])
		for i := 0; i < len(ary_pCardSet); i++ {
			str += fmt.Sprintf("[%s] ", ary_pCardSet[i].CodeString())
		}
		mlog.Debug("%s", str)
	}
}

// 依所需牌型發牌
func TestPokerCardSet_DealByPattern(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for i := POKER_Pattern_HighCard; i <= POKER_Pattern_RoyalFlush; i++ {
		pPoker.NewSet(1,0)
		pPoker.Shuffle()

		iCardCount := 0
		iRnd := util.RndInt(0,2)
		if iRnd == 0 {
			iCardCount = 5
		} else {
			iCardCount = 7
		}

		pCards := pPoker.DealByPattern(i, iCardCount)
		mlog.Debug("牌型[%s], 張數[%d], 牌組[%s]", g_map_strPattern[i], iCardCount, pCards.CodeString())
	}
}

// 比牌
func TestPokerCardSet_Compare(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for i := POKER_Pattern_HighCard; i <= POKER_Pattern_RoyalFlush; i++ {
		for j := POKER_Pattern_HighCard; j <= POKER_Pattern_RoyalFlush; j++ {
			pPoker.NewSet(1, 0)
			pPoker.Shuffle()

			pSet1 := new(PokerCardSet)
			pSet2 := new(PokerCardSet)

			pSet1 = pPoker.DealByPattern(i, 5)
			pSet2 = pPoker.DealByPattern(j, 5)

			// 比牌
			pSet1.Sort(false, true, true)
			pSet2.Sort(false, true, true)
			if pSet1.Pattern() == pSet2.Pattern() {
				mlog.Debug("i = %d, j = %d", i, j)
				mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), g_map_strPattern[pSet1.Pattern()])
				mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), g_map_strPattern[pSet2.Pattern()])
				iWinner := pPoker.Compare(pSet1, pSet2, false)
				if iWinner == POKER_Compare_Even {
					mlog.Debug("結果：平手")
				} else {
					mlog.Debug("結果：牌組 %d 贏", iWinner)
				}
				mlog.Debug("-------------------------------------------------")
			}
		}
	}
}

// 檢查是哪種順子，回傳值：［是否為順子］、［是否有 A］、［是否為 1-5 順子］
func TestPokerCardSet_CheckAStraightType(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	for i := 0; i < 50; i++ {
		pPoker.NewSet(1, 0)
		pPoker.Shuffle()

		i32Pattern := POKER_Pattern_None
		iRnd := util.RndInt(0, 2)
		switch iRnd {
		case 0:
			i32Pattern = POKER_Pattern_Straight
		case 1:
			i32Pattern = POKER_Pattern_StraightFlush
		default:
			i32Pattern = POKER_Pattern_Straight
		}

		pSet := pPoker.DealByPattern(i32Pattern, 5)
		bHasA, bIs15Straight := pPoker.CheckAStraightType(pSet)
		mlog.Debug("牌組[%s], 牌型[%s], 有A[%t], 15順[%t]", pSet.CodeString(), g_map_strPattern[pSet.Pattern()], bHasA, bIs15Straight)
	}
}

// 取得牌組中的一張牌
func TestPokerCardSet_GetSingleCard(t *testing.T) {
	Init()
	pPoker := new(PokerCardSet)
	pPoker.NewSet(1, 0)
	pPoker.Shuffle()

	pHandCard := pPoker.Deal(10)
	mlog.Debug("手牌[%s]", pHandCard.String())
	for i := 0; i < pHandCard.CardCount(); i++ {
		pCard := pHandCard.GetSingleCard(i, true)
		mlog.Debug("i = [%d]: %s", i, pCard.String())
	}
}

// 比牌 - 對子
func TestCompare_Pair(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		switch k {
		case 0:
			pSet1.SetCardSet("h3s3d6h8dJ")
			pSet2.SetCardSet("c3d3s6s8hJ")
		case 1:
			pSet1.SetCardSet("h3s3d6h7dJ")
			pSet2.SetCardSet("c3d3s6s8hJ")
		case 2:
			pSet1.SetCardSet("h3s3d6h8dK")
			pSet2.SetCardSet("c3d3s6s8hJ")
		default:
			pSet1 = pPokerSet.DealByPattern(POKER_Pattern_Pair, 5)
			pSet2 = pPokerSet.DealByPattern(POKER_Pattern_Pair, 5)
		}

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}


// 比牌 - 兩對
func TestCompare_TwoPairs(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		switch k {
		case 0:
			pSet1.SetCardSet("h3s3d6h6dJ")
			pSet2.SetCardSet("c3d3c6s6hJ")
		case 1:
			pSet1.SetCardSet("h3s3d6h6dJ")
			pSet2.SetCardSet("c3d3c6s6hK")
		case 2:
			pSet1.SetCardSet("h3s3d6h6dK")
			pSet2.SetCardSet("c3d3c6s6hJ")
		default:
			pSet1 = pPokerSet.DealByPattern(POKER_Pattern_TwoPairs, 5)
			pSet2 = pPokerSet.DealByPattern(POKER_Pattern_TwoPairs, 5)
		}

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}

// 比牌 - 三條
func TestCompare_ThreeOfAKind(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		pSet1 = pPokerSet.DealByPattern(POKER_Pattern_ThreeOfAKind, 5)
		pSet2 = pPokerSet.DealByPattern(POKER_Pattern_ThreeOfAKind, 5)

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}

// 比牌 - 順子
func TestCompare_Straight(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		pSet1 = pPokerSet.DealByPattern(POKER_Pattern_Straight, 5)
		pSet2 = pPokerSet.DealByPattern(POKER_Pattern_Straight, 5)

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}

// 比牌 - 同花
func TestCompare_Flush(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		pSet1 = pPokerSet.DealByPattern(POKER_Pattern_Flush, 5)
		pSet2 = pPokerSet.DealByPattern(POKER_Pattern_Flush, 5)

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}

// 比牌 - 葫蘆
func TestCompare_FullHouse(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		pSet1 = pPokerSet.DealByPattern(POKER_Pattern_FullHouse, 5)
		pSet2 = pPokerSet.DealByPattern(POKER_Pattern_FullHouse, 5)

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}

// 比牌 - 四條
func TestCompare_FourOfAKind(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		pSet1 = pPokerSet.DealByPattern(POKER_Pattern_FourOfAKind, 5)
		pSet2 = pPokerSet.DealByPattern(POKER_Pattern_FourOfAKind, 5)

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}

// 比牌 - 同花順
func TestCompare_StraightFlush(t *testing.T) {
	Init()
	pPokerSet := new(PokerCardSet)
	for k := 0; k < 10; k++ {
		mlog.Debug("----- 第 [%d] 組牌 -----", k+1)
		pPokerSet.NewSet(1, 0)
		pPokerSet.Shuffle()

		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)

		pSet1 = pPokerSet.DealByPattern(POKER_Pattern_StraightFlush, 5)
		pSet2 = pPokerSet.DealByPattern(POKER_Pattern_StraightFlush, 5)

		mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), PatternString(pSet1.Pattern()))
		mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), PatternString(pSet2.Pattern()))
		iWinner := pPokerSet.Compare(pSet1, pSet2, false)
		if iWinner == POKER_Compare_Even {
			mlog.Debug("結果：平手")
		} else {
			mlog.Debug("結果：牌組 %d 贏", iWinner)
		}
	}
}
