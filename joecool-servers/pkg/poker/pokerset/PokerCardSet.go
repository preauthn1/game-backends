package pokerset

import (
	"math/rand"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

type PokerCardSet struct {
	ary_pCards []*PokerCard // 牌組
}

func NewPokerCardSet() *PokerCardSet {
	pPoker := new(PokerCardSet)
	pPoker.ary_pCards = make([]*PokerCard, 0)

	return pPoker
}

// 建立新牌組，傳入參數［幾副撲克牌］、［鬼牌張數］
func (pPoker *PokerCardSet) NewSet(i32SetCount, i32JokerCount int32) {
	if (i32SetCount <= 0 || i32SetCount > POKER_MaxSet) || (i32JokerCount < 0 || i32JokerCount > POKER_JokerCardsCount) {
		mlog.Debug("牌組設定錯誤，SetCount[%d], JokerCount[%d]", i32SetCount, i32JokerCount)
		return
	}

	pPoker.ary_pCards = nil
	pPoker.ary_pCards = make([]*PokerCard, 0)

	for i := int32(0); i < i32SetCount; i++ {
		for i32Color := POKER_CardColor_Club; i32Color <= POKER_CardColor_Spade; i32Color++ {
			for i32No := POKER_CardNo_1; i32No < POKER_CardNo_A; i32No++ {
				pCard := new(PokerCard)
				pCard.I32Color = i32Color
				pCard.I32Number = i32No

				pPoker.ary_pCards = append(pPoker.ary_pCards, pCard)
			}
		}

		// 鬼牌
		if i32JokerCount == 1 || i32JokerCount == 2 {
			pCard := new(PokerCard)
			pCard.I32Color = POKER_CardColor_Other
			pCard.I32Number = POKER_CardNo_Joker1

			pPoker.ary_pCards = append(pPoker.ary_pCards, pCard)
		}

		if i32JokerCount == 2 {
			pCard := new(PokerCard)
			pCard.I32Color = POKER_CardColor_Other
			pCard.I32Number = POKER_CardNo_Joker2

			pPoker.ary_pCards = append(pPoker.ary_pCards, pCard)
		}
	}
}

// 洗牌
func (pPoker *PokerCardSet) Shuffle() {
	rand.Seed(time.Now().UnixNano()) //设置种子

	for iCount := 0; iCount < 5; iCount++ {
		rand.Shuffle(len(pPoker.ary_pCards), func(i, j int) { //调用算法
			pPoker.ary_pCards[i], pPoker.ary_pCards[j] = pPoker.ary_pCards[j], pPoker.ary_pCards[i]
		})
	}
}

// 加入一張牌
func (pPoker *PokerCardSet) AddSingleCard(strCodeString string) {
	if len(strCodeString) != LEN_SingleCardCode {
		return
	}

	if _, ok := g_map_i32ColorByCode[strCodeString[0:1]]; !ok {
		return
	} else {
		if _, ok := g_map_i32NumberByCode[strCodeString[1:]]; !ok {
			return
		}
	}

	pCard := new(PokerCard)
	pCard.I32Color = g_map_i32ColorByCode[strCodeString[0:1]]
	pCard.I32Number = g_map_i32NumberByCode[strCodeString[1:]]

	pPoker.ary_pCards = append(pPoker.ary_pCards, pCard)
}

// 加入一副牌組
func (pPoker *PokerCardSet) AddCardSet(pCardSet *PokerCardSet) {
	if pCardSet == nil {
		return
	}

	for _, pCard := range pCardSet.ary_pCards {
		if pCard != nil {
			pPoker.AddSingleCard(pCard.CodeString())
		}
	}
}

// 尋找一張牌
func (pPoker *PokerCardSet) FindCard(pCard *PokerCard) bool {
	if pCard == nil {
		return false
	}

	bGet := false
	for _, pData := range pPoker.ary_pCards {
		if pData != nil {
			if pData.I32Color == pCard.I32Color && pData.I32Number == pCard.I32Number {
				bGet = true
				break
			}
		}
	}

	return bGet
}

// 移除一張牌
func (pPoker *PokerCardSet) RemoveSingleCard(pCard *PokerCard) {
	if pCard == nil {
		return
	}

	pTemp := make([]*PokerCard, 0)
	bGet := false
	for _, pData := range pPoker.ary_pCards {
		if pData != nil {
			if !bGet && (pData.I32Color == pCard.I32Color && pData.I32Number == pCard.I32Number) {
				bGet = true
				continue
			}

			pTemp = append(pTemp, pData)
		}
	}

	pPoker.ary_pCards = nil
	pPoker.ary_pCards = pTemp
}

// 移除牌組中的牌
func (pPoker *PokerCardSet) RemoveCardSet(pCardSet *PokerCardSet) {
	if pCardSet == nil {
		return
	}

	if pCardSet.CardCount() > pPoker.CardCount() {
		return
	}

	for _, pCard := range pCardSet.ary_pCards {
		if pCard != nil {
			pPoker.RemoveSingleCard(pCard)
		}
	}
}

// 牌組張數
func (pPoker *PokerCardSet) CardCount() int {
	return len(pPoker.ary_pCards)
}

// 牌組文字字串
func (pPoker *PokerCardSet) String() string {
	strCards := ""
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		str := ""
		if pPoker.ary_pCards[i] != nil {
			str = pPoker.ary_pCards[i].String()

			if i < len(pPoker.ary_pCards)-1 {
				str += ", "
			}
		}

		strCards += str
	}

	return strCards
}

// 牌組代碼字串
func (pPoker *PokerCardSet) CodeString() string {
	strCards := ""
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i] != nil {
			strCards += pPoker.ary_pCards[i].CodeString()
		}
	}

	return strCards
}

// 設置牌組
func (pPoker *PokerCardSet) SetCardSet(strCodeString string) {
	if strCodeString == "" {
		pPoker.ary_pCards = nil
		pPoker.ary_pCards = make([]*PokerCard, 0)
		return
	}

	pNewSet := CardCode2PokerCardSet(strCodeString)
	if pNewSet != nil {
		pPoker.ary_pCards = nil
		pPoker.ary_pCards = pNewSet.ary_pCards
	}
}

// 發牌
func (pPoker *PokerCardSet) Deal(iCardCount int) *PokerCardSet {
	if iCardCount <= 0 || iCardCount > pPoker.CardCount() {
		return NewPokerCardSet()
	}

	strCodeString := pPoker.CodeString()[0 : LEN_SingleCardCode*iCardCount]
	pRetSet := pPoker.DrawCards(strCodeString)

	return pRetSet
}

// 將 strCodeString 中的牌從牌組中抽出
func (pPoker *PokerCardSet) DrawCards(strCodeString string) *PokerCardSet {
	if strCodeString == "" {
		return NewPokerCardSet()
	}

	pRetSet := CardCode2PokerCardSet(strCodeString)
	if pRetSet == nil {
		return NewPokerCardSet()
	}

	for _, pCard := range pRetSet.ary_pCards {
		if !pPoker.FindCard(pCard) {
			return NewPokerCardSet()
		}
	}

	pPoker.RemoveCardSet(pRetSet)

	return pRetSet
}

// 排序
// bByColor = true：依花色排序，bByColor = false：依數字排序
// bOneAsAce = true：數字 1 當作 Ace，bOneAsAce = false：數字 1 不變
// bAsc = true：由小到大排序，bAsc = false：由大到小排序
func (pPoker *PokerCardSet) Sort(bByColor, bOneAsAce, bAsc bool) {
	iLen := int(pPoker.CardCount())

	pSort := new(PokerCardSet)
	pSort.SetCardSet(pPoker.CodeString())
	if bOneAsAce {
		for i := 0; i < len(pSort.ary_pCards); i++ {
			if pSort.ary_pCards[i].I32Number == POKER_CardNo_1 {
				pSort.ary_pCards[i].I32Number = POKER_CardNo_A
			}
		}
	}

	if bByColor {
		for i := 0; i < iLen; i++ {
			for j := i; j < iLen; j++ {
				i32Card1 := pSort.ary_pCards[i].I32Color*100 + pSort.ary_pCards[i].I32Number
				i32Card2 := pSort.ary_pCards[j].I32Color*100 + pSort.ary_pCards[j].I32Number
				if bAsc {
					if i32Card1 > i32Card2 {
						pSort.ary_pCards[i], pSort.ary_pCards[j] = pSort.ary_pCards[j], pSort.ary_pCards[i]
						pPoker.ary_pCards[i], pPoker.ary_pCards[j] = pPoker.ary_pCards[j], pPoker.ary_pCards[i]
					}
				} else {
					if i32Card1 < i32Card2 {
						pSort.ary_pCards[i], pSort.ary_pCards[j] = pSort.ary_pCards[j], pSort.ary_pCards[i]
						pPoker.ary_pCards[i], pPoker.ary_pCards[j] = pPoker.ary_pCards[j], pPoker.ary_pCards[i]
					}
				}
			}
		}
	} else {
		// 依數字排序
		for i := 0; i < iLen; i++ {
			for j := i; j < iLen; j++ {
				i32Card1 := pSort.ary_pCards[i].I32Number*10 + pSort.ary_pCards[i].I32Color
				i32Card2 := pSort.ary_pCards[j].I32Number*10 + pSort.ary_pCards[j].I32Color
				if bAsc {
					if i32Card1 > i32Card2 {
						pSort.ary_pCards[i], pSort.ary_pCards[j] = pSort.ary_pCards[j], pSort.ary_pCards[i]
						pPoker.ary_pCards[i], pPoker.ary_pCards[j] = pPoker.ary_pCards[j], pPoker.ary_pCards[i]
					}
				} else {
					if i32Card1 < i32Card2 {
						pSort.ary_pCards[i], pSort.ary_pCards[j] = pSort.ary_pCards[j], pSort.ary_pCards[i]
						pPoker.ary_pCards[i], pPoker.ary_pCards[j] = pPoker.ary_pCards[j], pPoker.ary_pCards[i]
					}
				}
			}
		}
	}
}

// 牌組牌型
func (pPoker *PokerCardSet) Pattern() int32 {
	if pPoker.CardCount() <= 0 {
		return POKER_Pattern_None
	}

	//POKER_Pattern_DragonFlush						// 同花一條龍
	// todo

	//POKER_Pattern_Dragon							// 一條龍
	// todo

	// 皇家同花順
	if pPoker.isRoyalFlush() {
		return POKER_Pattern_RoyalFlush
	}

	//POKER_Pattern_StraightFlush						// 同花順
	if pPoker.isStraightFlush() {
		return POKER_Pattern_StraightFlush
	}

	// 四條
	if pPoker.isFourOfAKind() {
		return POKER_Pattern_FourOfAKind
	}

	// 葫蘆
	if pPoker.isFullHouse() {
		return POKER_Pattern_FullHouse
	}

	// 同花
	if pPoker.isFlush() {
		return POKER_Pattern_Flush
	}

	// 順子
	if pPoker.isStraight() {
		return POKER_Pattern_Straight
	}

	// 三條
	if pPoker.isThreeOfAKind() {
		return POKER_Pattern_ThreeOfAKind
	}

	// 兩對
	if pPoker.isTwoPairs() {
		return POKER_Pattern_TwoPairs
	}

	// 對子
	if pPoker.isPair() {
		return POKER_Pattern_Pair
	}

	return POKER_Pattern_HighCard
}

// 是否為對子
func (pPoker *PokerCardSet) isPair() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_Pair] {
		return false
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	// 只能有一個對子
	iPairCount := 0
	for _, iCount := range map_iCardCount {
		if iCount > g_map_iPatternCardCount[POKER_Pattern_Pair] {
			return false
		} else if iCount == g_map_iPatternCardCount[POKER_Pattern_Pair] {
			iPairCount++
		}
	}

	if iPairCount != 1 {
		return false
	}

	return true
}

// 是否為兩對
func (pPoker *PokerCardSet) isTwoPairs() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_TwoPairs] {
		return false
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	// 只能有兩個不同對子
	iPairCount := 0
	for _, iCount := range map_iCardCount {
		if iCount > g_map_iPatternCardCount[POKER_Pattern_Pair] {
			return false
		} else if iCount == g_map_iPatternCardCount[POKER_Pattern_Pair] {
			iPairCount++
		}
	}

	if iPairCount != 2 {
		return false
	}

	return true
}

// 是否為三條
func (pPoker *PokerCardSet) isThreeOfAKind() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] {
		return false
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	iThreeCount := 0
	for _, iCount := range map_iCardCount {
		if iCount > g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] || iCount == g_map_iPatternCardCount[POKER_Pattern_Pair] {
			return false
		} else if iCount == g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] {
			iThreeCount++
		}
	}

	// 只能有一個三條
	if iThreeCount != 1 {
		return false
	}

	return true
}

// 是否為順子
func (pPoker *PokerCardSet) isStraight() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_Straight] {
		return false
	}

	iCheckCount := g_map_iPatternCardCount[POKER_Pattern_Straight]
	if pPoker.CardCount() >= POKER_PatternBasicCardCount {
		iCheckCount = POKER_PatternBasicCardCount
	}

	if !pPoker.HasNumberInOrder(iCheckCount) {
		return false
	}

	return true
}

// 是否為同花
func (pPoker *PokerCardSet) isFlush() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_Flush] {
		return false
	}

	i32Color := pPoker.ary_pCards[0].I32Color
	for i := 1; i < pPoker.CardCount(); i++ {
		if pPoker.ary_pCards[i].I32Color != i32Color {
			return false
		}
	}
	
	iCheckCount := g_map_iPatternCardCount[POKER_Pattern_Flush]
	if pPoker.CardCount() >= POKER_PatternBasicCardCount {
		iCheckCount = POKER_PatternBasicCardCount
	}
	
	if pPoker.HasNumberInOrder(iCheckCount) {
		return false
	}

	return true
}

// 是否為葫蘆
func (pPoker *PokerCardSet) isFullHouse() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_FullHouse] {
		return false
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	iTwoCount := 0
	iThreeCount := 0
	for _, iCount := range map_iCardCount {
		if iCount == g_map_iPatternCardCount[POKER_Pattern_Pair] {
			iTwoCount++
		} else if iCount == g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] {
			iThreeCount++
		}
	}

	if iTwoCount != 1 || iThreeCount != 1 {
		return false
	}

	return true
}

// 是否為四條
func (pPoker *PokerCardSet) isFourOfAKind() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_FourOfAKind] {
		return false
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	iFourCount := 0
	for _, iCount := range map_iCardCount {
		if iCount == g_map_iPatternCardCount[POKER_Pattern_FourOfAKind] {
			iFourCount++
		}
	}

	// 只能有一個四條
	if iFourCount != 1 {
		return false
	}

	return true
}

// 是否為同花順
func (pPoker *PokerCardSet) isStraightFlush() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_StraightFlush] {
		return false
	}
	
	if !pPoker.IsFlushType() {
		return false
	}
	
	iCheckCount := g_map_iPatternCardCount[POKER_Pattern_StraightFlush]
	if pPoker.CardCount() >= POKER_PatternBasicCardCount {
		iCheckCount = POKER_PatternBasicCardCount
	}
	
	if !pPoker.HasNumberInOrder(iCheckCount) {
		return false
	}

	return true
}

// 是否為皇家同花順
func (pPoker *PokerCardSet) isRoyalFlush() bool {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_RoyalFlush] {
		return false
	}

	ary_pCards := make([]*PokerCardSet, POKER_ColorsCount)
	for i := 0; i < len(ary_pCards); i++ {
		ary_pCards[i] = new(PokerCardSet)
	}

	pTemp := new(PokerCardSet)
	pTemp.SetCardSet(pPoker.CodeString())
	pTemp.Sort(false, true, true)

	for i := 0; i < pTemp.CardCount(); i++ {
		if pTemp.ary_pCards[i].I32Color >= POKER_CardColor_Club && pTemp.ary_pCards[i].I32Color <= POKER_CardColor_Spade {
			if pTemp.ary_pCards[i].I32Number == POKER_CardNo_1 {
				pTemp.ary_pCards[i].I32Number = POKER_CardNo_A
			}
			ary_pCards[pTemp.ary_pCards[i].I32Color].AddSingleCard(pTemp.ary_pCards[i].CodeString())
		}
	}

	for i := POKER_CardColor_Club; i <= POKER_CardColor_Spade; i++ {
		iGetCard := 0
		for j := 0; j < ary_pCards[i].CardCount(); j++ {
			if ary_pCards[i].ary_pCards[j].I32Number >= POKER_CardNo_10 && ary_pCards[i].ary_pCards[j].I32Number <= POKER_CardNo_A {
				iGetCard |= 1 << uint(ary_pCards[i].ary_pCards[j].I32Number-POKER_CardNo_10)
			}
		}

		if iGetCard == 31 {
			return true
		}
	}

	return false
}

//POKER_Pattern_Dragon							// 是否為一條龍
// todo

//POKER_Pattern_DragonFlush						// 是否為同花一條龍
// todo

// 檢查是否為順子
// iCheckCount：要檢查是否為順子的牌組張數，例如 7 張牌裡要檢查是否有 5 張的順面，則 iCheckCount = 5
func (pPoker *PokerCardSet) HasNumberInOrder(iCheckCount int) bool {
	if iCheckCount < POKER_MinStraightCount || iCheckCount > pPoker.CardCount() {
		return false
	}

	ary_bGetCard := make([]bool, POKER_OneColorCardsCount)
	bGet := true

	pTemp := new(PokerCardSet)
	pTemp.SetCardSet(pPoker.CodeString())
	// 數字1
	for i := 0; i < len(pTemp.ary_pCards); i++ {
		if pTemp.ary_pCards[i].I32Number >= POKER_CardNo_1 && pTemp.ary_pCards[i].I32Number <= POKER_CardNo_K {
			ary_bGetCard[pTemp.ary_pCards[i].I32Number-POKER_CardNo_1] = true
		}
	}

	for i := 0; i <= len(ary_bGetCard)-iCheckCount; i++ {
		bGet = true
		for j := i; j < i+iCheckCount; j++ {
			if !ary_bGetCard[j] {
				bGet = false
				break
			}
		}

		if bGet {
			break
		}
	}

	// A
	if !bGet {
		ary_bGetCard = make([]bool, POKER_OneColorCardsCount)
		for i := 0; i < len(pTemp.ary_pCards); i++ {
			if pTemp.ary_pCards[i].I32Number == POKER_CardNo_1 {
				pTemp.ary_pCards[i].I32Number = POKER_CardNo_A
			}

			if pTemp.ary_pCards[i].I32Number >= POKER_CardNo_2 && pTemp.ary_pCards[i].I32Number <= POKER_CardNo_A {
				ary_bGetCard[pTemp.ary_pCards[i].I32Number-POKER_CardNo_2] = true
			}
		}

		for i := 0; i <= len(ary_bGetCard)-iCheckCount; i++ {
			bGet = true
			for j := i; j < i+iCheckCount; j++ {
				if !ary_bGetCard[j] {
					bGet = false
					break
				}
			}

			if bGet {
				break
			}
		}
	}

	return bGet
}

// 依牌型取得牌組，［i32Pattern］牌型，［iCardCount］0: 依該牌型預設張數
func (pPoker *PokerCardSet) getCardSetsByPattern(i32Pattern int32, iCardCount int) []*PokerCardSet {
	ary_pCardSet := make([]*PokerCardSet, 0)

	b3Straight := true
	if iCardCount >= POKER_PatternBasicCardCount {
		b3Straight = false
	}

	switch i32Pattern {
	case POKER_Pattern_HighCard: // 高牌
		ary_pCardSet = pPoker.getHighCard()
	case POKER_Pattern_Pair: // 對子
		ary_pCardSet = pPoker.getPair()
	case POKER_Pattern_TwoPairs: // 兩對
		ary_pCardSet = pPoker.getTwoPairs()
	case POKER_Pattern_ThreeOfAKind: // 三條
		ary_pCardSet = pPoker.getThreeOfAKind()
	case POKER_Pattern_Straight: // 順子
		ary_pCardSet = pPoker.getStraight(b3Straight)
	case POKER_Pattern_Flush: // 同花
		ary_pCardSet = pPoker.getFlush(iCardCount)
	case POKER_Pattern_FullHouse: // 葫蘆
		ary_pCardSet = pPoker.getFullHouse()
	case POKER_Pattern_FourOfAKind: // 四條
		ary_pCardSet = pPoker.getFourOfAKind()
	case POKER_Pattern_StraightFlush: // 同花順
		ary_pCardSet = pPoker.getStraightFlush(b3Straight)
	case POKER_Pattern_RoyalFlush: // 皇家同花順
		ary_pCardSet = pPoker.getRoyalFlush()
		//case POKER_Pattern_Dragon:        // 一條龍	todo
		//case POKER_Pattern_DragonFlush:   // 同花一條龍	todo
	}

	return ary_pCardSet
}

// 依所需牌型發牌，iCardCount：牌組總張數
func (pPoker *PokerCardSet) DealByPattern(i32Pattern int32, iCardCount int) *PokerCardSet {
	if i32Pattern < POKER_Pattern_HighCard || i32Pattern >= POKER_Pattern_Max {
		return NewPokerCardSet()
	}

	if iCardCount > pPoker.CardCount() {
		return NewPokerCardSet()
	}

	ary_pCardSet := pPoker.getCardSetsByPattern(i32Pattern, iCardCount)

	// 若牌盒中已取不出所要的牌型就發高牌
	if len(ary_pCardSet) <= 0 {
		ary_pCardSet = pPoker.getHighCard()
	}

	pRetSet := new(PokerCardSet)

	// 隨機取得牌組
	iIdx := util.RndInt(0, len(ary_pCardSet))
	pRetSet.SetCardSet(ary_pCardSet[iIdx].CodeString())
	if pRetSet.CardCount() <= 0 {
		return NewPokerCardSet()
	}

	// 補足張數
	pSet := pPoker.supplementCards(pRetSet, iCardCount-pRetSet.CardCount())

	pRetSet.AddCardSet(pSet)
	pPoker.RemoveCardSet(pRetSet)

	return pRetSet
}

// 補牌，［pBaseSet］原已發出的牌組，［iSupplementCount］要補的張數
// 回傳［補牌的牌組］
func (pPoker *PokerCardSet) supplementCards(pBaseSet *PokerCardSet, iSupplementCount int) *PokerCardSet {
	if pBaseSet == nil {
		return NewPokerCardSet()
	}

	if pBaseSet.CardCount() <= 0 || iSupplementCount <= 0 {
		return NewPokerCardSet()
	}

	mlog.Debug("pBaseSet(%d)%s [%s], iSupplementCount[%d]", pBaseSet.CardCount(), pBaseSet.CodeString(), PatternString(pBaseSet.Pattern()), iSupplementCount)

	pRetset := NewPokerCardSet()

	pTempSet := NewPokerCardSet()
	pCheckSet := NewPokerCardSet()
	i32BasePattern := pBaseSet.Pattern()
	for i := 0; i < pPoker.CardCount(); i++ {
		pCard := pPoker.GetSingleCard(i, false)
		pCheckSet.SetCardSet(pBaseSet.CodeString())
		pCheckSet.AddCardSet(pTempSet)

		if pCheckSet.IsNumberExisted(pCard.I32Number) {
			continue
		}

		pCheckSet.AddSingleCard(pCard.CodeString())
		if pCheckSet.Pattern() != i32BasePattern {
			continue
		}

		pTempSet.AddSingleCard(pPoker.ary_pCards[i].CodeString())
		if pTempSet.CardCount() >= iSupplementCount {
			break
		}
	}
	mlog.Debug("pTempSet(%d)%s", pTempSet.CardCount(), pTempSet.CodeString())

	pRetset.SetCardSet(pTempSet.CodeString())

	return pRetset
}

// 取得高牌
func (pPoker *PokerCardSet) getHighCard() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_HighCard] {
		return make([]*PokerCardSet, 0)
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	for i := 0; i < pPoker.CardCount() - g_map_iPatternCardCount[POKER_Pattern_HighCard]; i++ {
		pSet := new(PokerCardSet)
		pSet.SetCardSet(pPoker.ary_pCards[i].CodeString())
		for j := i + 1; j < pPoker.CardCount(); j++ {
			if pSet.CardCount() >= g_map_iPatternCardCount[POKER_Pattern_HighCard] {
				if pSet.Pattern() == POKER_Pattern_HighCard {
					ary_pRetSet = append(ary_pRetSet, pSet)
				}
				break
			}

			if pSet.IsNumberExisted(pPoker.ary_pCards[j].I32Number) {
				continue
			}
			pSet.AddSingleCard(pPoker.ary_pCards[j].CodeString())
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得對子
func (pPoker *PokerCardSet) getPair() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_Pair] {
		return make([]*PokerCardSet, 0)
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	// 取出對子
	for i32No, iCount := range map_iCardCount {
		if iCount >= g_map_iPatternCardCount[POKER_Pattern_Pair] {
			for i := 0; i < len(pPoker.ary_pCards); i++ {
				if pPoker.ary_pCards[i].I32Number == i32No {
					pSet := new(PokerCardSet)
					for j := i; j < len(pPoker.ary_pCards); j++ {
						if pPoker.ary_pCards[j].I32Number == i32No {
							pSet.AddSingleCard(pPoker.ary_pCards[j].CodeString())
						}

						if pSet.CardCount() >= g_map_iPatternCardCount[POKER_Pattern_Pair] {
							pSet.Sort(true, false, true)
							ary_pRetSet = append(ary_pRetSet, pSet)
							break
						}
					}
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得兩對
func (pPoker *PokerCardSet) getTwoPairs() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_TwoPairs] {
		return make([]*PokerCardSet, 0)
	}

	ary_pRetSet := make([]*PokerCardSet, 0)
	ary_pPairs := pPoker.getPair()
	if len(ary_pPairs) < 2 {
		return make([]*PokerCardSet, 0)
	} else {
		for i := 0; i < len(ary_pPairs)-1; i++ {
			for j := i + 1; j < len(ary_pPairs); j++ {
				if ary_pPairs[i].ary_pCards[0].I32Number != ary_pPairs[j].ary_pCards[0].I32Number {
					pSet := new(PokerCardSet)
					pSet.AddCardSet(ary_pPairs[i])
					pSet.AddCardSet(ary_pPairs[j])
					pSet.Sort(false, false, true)
					ary_pRetSet = append(ary_pRetSet, pSet)
				}
			}
		}
	}

	return ary_pRetSet
}

// 取得三條
func (pPoker *PokerCardSet) getThreeOfAKind() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] {
		return make([]*PokerCardSet, 0)
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	// 取出三條
	for i32No, iCount := range map_iCardCount {
		if iCount >= g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] {
			pSet := new(PokerCardSet)
			for i := 0; i < len(pPoker.ary_pCards); i++ {
				if pPoker.ary_pCards[i].I32Number == i32No {
					pSet.AddSingleCard(pPoker.ary_pCards[i].CodeString())
				}

				if pSet.CardCount() >= g_map_iPatternCardCount[POKER_Pattern_ThreeOfAKind] {
					ary_pRetSet = append(ary_pRetSet, pSet)
					break
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得順子，［b3Straight］true: 3 張順子，false: 5 張順子
func (pPoker *PokerCardSet) getStraight(b3Straight bool) []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_Straight] {
		return make([]*PokerCardSet, 0)
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	iCardCount := g_map_iPatternCardCount[POKER_Pattern_Straight]
	if !b3Straight {
		iCardCount = POKER_PatternBasicCardCount
	}

	// 取得各數字的張數
	ary_pCards := make([]*PokerCardSet, POKER_OneColorCardsCount)
	for i := 0; i < len(ary_pCards); i++ {
		ary_pCards[i] = NewPokerCardSet()
	}

	for i := 0; i < pPoker.CardCount(); i++ {
		pCard := pPoker.GetSingleCard(i, false)
		if pCard.I32Number >= POKER_CardNo_1 && pCard.I32Number <= POKER_CardNo_K {
			ary_pCards[pCard.I32Number - POKER_CardNo_1].AddSingleCard(pCard.CodeString())
		}
	}

	// 取出順子
	for i := 0; i <= len(ary_pCards) - iCardCount + 1; i++ {
		bStraight := true
		for j := i; j < i + iCardCount; j++ {
			if j < len(ary_pCards) {
				if ary_pCards[j].CardCount() <= 0 {
					bStraight = false
					break
				}
			} else {
				// 如果後面的牌是 QK 或是 10JQK，則判斷 A（數字 1）的張數
				if ary_pCards[0].CardCount() <= 0 {
					bStraight = false
					break
				}
			}
		}

		if bStraight {
			for j := 0; j < int(POKER_ColorsCount); j++ {
				pSet := NewPokerCardSet()
				for k := i; k < i+iCardCount; k++ {
					iIdx := 0
					if k < len(ary_pCards) {
						if j < ary_pCards[k].CardCount() {
							iIdx = j
						} else {
							iIdx = ary_pCards[k].CardCount() - 1
						}
						pSet.AddSingleCard(ary_pCards[k].ary_pCards[iIdx].CodeString())
					} else {
						// 如果後面的牌是 QK 或是 10JQK，則把 A（數字 1） 加進來
						if j < ary_pCards[0].CardCount() {
							iIdx = j
						} else {
							iIdx = ary_pCards[0].CardCount() - 1
						}
						pSet.AddSingleCard(ary_pCards[0].ary_pCards[iIdx].CodeString())
					}
				}
				pSet.Shuffle()
				if pSet.Pattern() == POKER_Pattern_Straight {
					ary_pRetSet = append(ary_pRetSet, pSet)
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得同花
func (pPoker *PokerCardSet) getFlush(iCardCount int) []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_Flush] {
		return make([]*PokerCardSet, 0)
	}

	if iCardCount < g_map_iPatternCardCount[POKER_Pattern_Flush] || iCardCount > pPoker.CardCount() {
		return make([]*PokerCardSet, 0)
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	ary_pCards := make([]*PokerCardSet, POKER_ColorsCount)
	for i := 0; i < len(ary_pCards); i++ {
		ary_pCards[i] = new(PokerCardSet)
	}

	for i := 0; i < pPoker.CardCount(); i++ {
		if pPoker.ary_pCards[i].I32Color >= POKER_CardColor_Club && pPoker.ary_pCards[i].I32Color <= POKER_CardColor_Spade {
			ary_pCards[pPoker.ary_pCards[i].I32Color].AddSingleCard(pPoker.ary_pCards[i].CodeString())
		}
	}

	for i32Pattern := POKER_CardColor_Club; i32Pattern <= POKER_CardColor_Spade; i32Pattern++ {
		if ary_pCards[i32Pattern].CardCount() >= iCardCount {
			for i := 0; i < pPoker.CardCount() - iCardCount; i++ {
				pSet := new(PokerCardSet)
				pSet.AddSingleCard(pPoker.ary_pCards[i].CodeString())
				i32Color := pPoker.ary_pCards[i].I32Color
				for j := i + 1; j < pPoker.CardCount(); j++ {
					if pSet.CardCount() >= iCardCount {
						if pSet.Pattern() == POKER_Pattern_Flush {
							ary_pRetSet = append(ary_pRetSet, pSet)
						}
						break
					}

					pCard := pPoker.GetSingleCard(j, false)
					if pCard.I32Color != i32Color || pSet.IsNumberExisted(pCard.I32Number) {
						continue
					}

					pSet.AddSingleCard(pCard.CodeString())
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得葫蘆
func (pPoker *PokerCardSet) getFullHouse() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_FullHouse] {
		return make([]*PokerCardSet, 0)
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	ary_pSet_3 := pPoker.getThreeOfAKind()
	ary_pSet_2 := pPoker.getPair()

	if len(ary_pSet_3) <= 0 || len(ary_pSet_2) <= 0 {
		return make([]*PokerCardSet, 0)
	} else {
		for i := 0; i < len(ary_pSet_3); i++ {
			for j := 0; j < len(ary_pSet_2); j++ {
				if ary_pSet_3[i].ary_pCards[0].I32Number != ary_pSet_2[j].ary_pCards[0].I32Number {
					pSet := new(PokerCardSet)
					pSet.AddCardSet(ary_pSet_3[i])
					pSet.AddCardSet(ary_pSet_2[j])

					ary_pRetSet = append(ary_pRetSet, pSet)
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得四條
func (pPoker *PokerCardSet) getFourOfAKind() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_FourOfAKind] {
		return make([]*PokerCardSet, 0)
	}

	// 統計各數字張數
	map_iCardCount := make(map[int32]int)
	for i := 0; i < len(pPoker.ary_pCards); i++ {
		if pPoker.ary_pCards[i].I32Number >= POKER_CardNo_1 && pPoker.ary_pCards[i].I32Number <= POKER_CardNo_K {
			map_iCardCount[pPoker.ary_pCards[i].I32Number]++
		}
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	// 取出四條
	for i32No, iCount := range map_iCardCount {
		if iCount >= g_map_iPatternCardCount[POKER_Pattern_FourOfAKind] {
			pSet := new(PokerCardSet)
			for i := 0; i < len(pPoker.ary_pCards); i++ {
				if pPoker.ary_pCards[i].I32Number == i32No {
					pSet.AddSingleCard(pPoker.ary_pCards[i].CodeString())
				}

				if pSet.CardCount() >= g_map_iPatternCardCount[POKER_Pattern_FourOfAKind] {
					ary_pRetSet = append(ary_pRetSet, pSet)
					break
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得同花順
func (pPoker *PokerCardSet) getStraightFlush(b3Straight bool) []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_StraightFlush] {
		return make([]*PokerCardSet, 0)
	}

	iCardCount := g_map_iPatternCardCount[POKER_Pattern_Straight]
	if !b3Straight {
		iCardCount = POKER_PatternBasicCardCount
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	ary_pCards := make([]*PokerCardSet, POKER_ColorsCount)
	for i := 0; i < len(ary_pCards); i++ {
		ary_pCards[i] = new(PokerCardSet)
	}

	pTemp := new(PokerCardSet)
	pTemp.SetCardSet(pPoker.CodeString())

	for i := 0; i < pTemp.CardCount(); i++ {
		pCard := pTemp.ary_pCards[i]
		if pCard.I32Color >= POKER_CardColor_Club && pCard.I32Color <= POKER_CardColor_Spade {
			ary_pCards[pCard.I32Color].AddSingleCard(pCard.CodeString())
		}
	}

	for i := POKER_CardColor_Club; i <= POKER_CardColor_Spade; i++ {
		ary_pCards[i].Sort(false,false,true)

		if ary_pCards[i].CardCount() >= iCardCount {
			for a := 0; a <= ary_pCards[i].CardCount()-iCardCount; a++ {
				bStraight := true
				i32No := ary_pCards[i].ary_pCards[a].I32Number
				for b := a; b < a+iCardCount; b++ {
					if ary_pCards[i].ary_pCards[b].I32Number != i32No {
						bStraight = false
						break
					}

					i32No++
				}

				if bStraight {
					pSet := new(PokerCardSet)
					for b := a; b < a+iCardCount; b++ {
						pSet.AddSingleCard(ary_pCards[i].ary_pCards[b].CodeString())
					}
					ary_pRetSet = append(ary_pRetSet, pSet)
				}
			}
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

// 取得皇家同花順
func (pPoker *PokerCardSet) getRoyalFlush() []*PokerCardSet {
	if pPoker.CardCount() < g_map_iPatternCardCount[POKER_Pattern_RoyalFlush] {
		return make([]*PokerCardSet, 0)
	}

	ary_pRetSet := make([]*PokerCardSet, 0)

	ary_pCards := make([]*PokerCardSet, POKER_ColorsCount)
	for i := 0; i < len(ary_pCards); i++ {
		ary_pCards[i] = new(PokerCardSet)
	}

	pTemp := new(PokerCardSet)
	pTemp.SetCardSet(pPoker.CodeString())
	pTemp.Sort(false, true, true)

	for i := 0; i < pTemp.CardCount(); i++ {
		if pTemp.ary_pCards[i].I32Color >= POKER_CardColor_Club && pTemp.ary_pCards[i].I32Color <= POKER_CardColor_Spade {
			ary_pCards[pTemp.ary_pCards[i].I32Color].AddSingleCard(pTemp.ary_pCards[i].CodeString())
		}
	}

	for i := POKER_CardColor_Club; i <= POKER_CardColor_Spade; i++ {
		map_bCheck := map[int32]bool{
			POKER_CardNo_1:  false,
			POKER_CardNo_10: false,
			POKER_CardNo_J:  false,
			POKER_CardNo_Q:  false,
			POKER_CardNo_K:  false,
		}

		pSet := new(PokerCardSet)
		for j := 0; j < ary_pCards[i].CardCount(); j++ {
			if bValue, ok := map_bCheck[ary_pCards[i].ary_pCards[j].I32Number]; ok {
				if !bValue {
					pSet.AddSingleCard(ary_pCards[i].ary_pCards[j].CodeString())
					map_bCheck[ary_pCards[i].ary_pCards[j].I32Number] = true
				}
			}
		}

		if pSet.CardCount() >= g_map_iPatternCardCount[POKER_Pattern_RoyalFlush] {
			ary_pRetSet = append(ary_pRetSet, pSet)
		}
	}

	if len(ary_pRetSet) <= 0 {
		return make([]*PokerCardSet, 0)
	}

	return ary_pRetSet
}

//POKER_Pattern_Dragon							// 取得一條龍
// todo

//POKER_Pattern_DragonFlush						// 取得同花一條龍
// todo

// 比牌
func (pPoker *PokerCardSet) Compare(pCardSet1, pCardSet2 *PokerCardSet, bCmpColor bool, bCmpSingle bool) int {
	if pCardSet1 == nil || pCardSet2 == nil {
		return POKER_Compare_Even
	}

	if pCardSet1.CardCount() <= 0 || pCardSet2.CardCount() <= 0 {
		return POKER_Compare_Even
	}

	if pCardSet1.CardCount() != pCardSet2.CardCount() {
		return POKER_Compare_Even
	}

	// 先比牌型
	if pCardSet1.Pattern() > pCardSet2.Pattern() {
		return POKER_Compare_Dealer
	} else if pCardSet1.Pattern() < pCardSet2.Pattern() {
		return POKER_Compare_Player
	}

	// 牌型若相同則比數字
	ary_pSet1 := pCardSet1.getCardSetsByPattern(pCardSet1.Pattern(), pCardSet1.CardCount())
	ary_pSet2 := pCardSet2.getCardSetsByPattern(pCardSet2.Pattern(), pCardSet2.CardCount())
	if len(ary_pSet1) <= 0 || len(ary_pSet2) <= 0 {
		return POKER_Compare_Even
	}

	pTempSet1, pTempSet2 := new(PokerCardSet), new(PokerCardSet)
	pTempSet1.SetCardSet(pCardSet1.CodeString())
	pTempSet2.SetCardSet(pCardSet2.CodeString())

	switch pCardSet1.Pattern() {
	case POKER_Pattern_HighCard: // 高牌
		fallthrough
	case POKER_Pattern_Flush: // 同花
		pSet1, pSet2 := new(PokerCardSet), new(PokerCardSet)
		pSet1.SetCardSet(pCardSet1.CodeString())
		pSet1.Sort(false, true, false)
		pSet2.SetCardSet(pCardSet2.CodeString())
		pSet2.Sort(false, true, false)

		for i := 0; i < pSet1.CardCount(); i++ {
			pCard1 := pSet1.GetSingleCard(i, true)
			pCard2 := pSet2.GetSingleCard(i, true)
			if pCard1.I32Number > pCard2.I32Number {
				return POKER_Compare_Dealer
			} else if pCard1.I32Number < pCard2.I32Number {
				return POKER_Compare_Player
			}
		}
	case POKER_Pattern_Pair: // 對子
		pSet1 := pTempSet1.DrawCards(ary_pSet1[0].CodeString())
		pSet2 := pTempSet2.DrawCards(ary_pSet2[0].CodeString())

		// 先比對子數字
		pCard1 := pSet1.GetSingleCard(0, true)
		pCard2 := pSet2.GetSingleCard(0, true)
		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		} else if !bCmpSingle && pCard1.I32Number == pCard2.I32Number {
			return POKER_Compare_Even
		}

		// 對子數字相同，比剩下的牌
		pTempSet1.Sort(false, true, false)
		pTempSet2.Sort(false, true, false)

		for i := 0; i < pTempSet1.CardCount(); i++ {
			pCard1 := pTempSet1.GetSingleCard(i, true)
			pCard2 := pTempSet2.GetSingleCard(i, true)
			if pCard1.I32Number > pCard2.I32Number {
				return POKER_Compare_Dealer
			} else if pCard1.I32Number < pCard2.I32Number {
				return POKER_Compare_Player
			}
		}
	case POKER_Pattern_TwoPairs: // 兩對
		pSet1 := pTempSet1.DrawCards(ary_pSet1[0].CodeString())
		pSet2 := pTempSet2.DrawCards(ary_pSet2[0].CodeString())

		pSet1.Sort(false, true, false)
		pSet2.Sort(false, true, false)

		// 先比第一組對子數字
		pCard1 := pSet1.GetSingleCard(0, true)
		pCard2 := pSet2.GetSingleCard(0, true)
		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		}

		// 再比第二組對子數字
		pCard1 = pSet1.GetSingleCard(pSet1.CardCount()-1, true)
		pCard2 = pSet2.GetSingleCard(pSet2.CardCount()-1, true)
		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		} else if !bCmpSingle && pCard1.I32Number == pCard2.I32Number {
			return POKER_Compare_Even
		}

		// 對子數字相同，比剩下的牌
		pTempSet1.Sort(false, true, false)
		pTempSet2.Sort(false, true, false)

		for i := 0; i < pTempSet1.CardCount(); i++ {
			pCard1 := pTempSet1.GetSingleCard(i, true)
			pCard2 := pTempSet2.GetSingleCard(i, true)
			if pCard1.I32Number > pCard2.I32Number {
				return POKER_Compare_Dealer
			} else if pCard1.I32Number < pCard2.I32Number {
				return POKER_Compare_Player
			}
		}
	case POKER_Pattern_ThreeOfAKind: // 三條
		fallthrough
	case POKER_Pattern_FourOfAKind: // 四條
		pSet1 := pTempSet1.DrawCards(ary_pSet1[0].CodeString())
		pSet2 := pTempSet2.DrawCards(ary_pSet2[0].CodeString())

		// 先比三條/四條數字
		pCard1 := pSet1.GetSingleCard(0, true)
		pCard2 := pSet2.GetSingleCard(0, true)
		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		}

		// 三條/四條數字相同，比剩下的牌
		pTempSet1.Sort(false, true, false)
		pTempSet2.Sort(false, true, false)

		for i := 0; i < pTempSet1.CardCount(); i++ {
			pCard1 := pTempSet1.GetSingleCard(i, true)
			pCard2 := pTempSet2.GetSingleCard(i, true)
			if pCard1.I32Number > pCard2.I32Number {
				return POKER_Compare_Dealer
			} else if pCard1.I32Number < pCard2.I32Number {
				return POKER_Compare_Player
			}
		}
	case POKER_Pattern_Straight: // 順子
		fallthrough
	case POKER_Pattern_StraightFlush: // 同花順
		pSet1 := pTempSet1.DrawCards(ary_pSet1[0].CodeString())
		pSet2 := pTempSet2.DrawCards(ary_pSet2[0].CodeString())

		// 比最大的數字
		bHasA1, bIs15Straight1 := pPoker.CheckAStraightType(pSet1)
		pCard1, pCard2 := new(PokerCard), new(PokerCard)
		if !bHasA1 {
			pSet1.Sort(false, true, false)
			pCard1 = pSet1.GetSingleCard(0, true)
		} else {
			if bIs15Straight1 {
				pSet1.Sort(false, false, false)
				pCard1 = pSet1.GetSingleCard(0, false)
			} else {
				pSet1.Sort(false, true, false)
				pCard1 = pSet1.GetSingleCard(0, true)
			}
		}
		bHasA2, bIs15Straight2 := pPoker.CheckAStraightType(pSet2)
		if !bHasA2 {
			pSet2.Sort(false, true, false)
			pCard2 = pSet2.GetSingleCard(0, true)
		} else {
			if bIs15Straight2 {
				pSet2.Sort(false, false, false)
				pCard2 = pSet2.GetSingleCard(0, false)
			} else {
				pSet2.Sort(false, true, false)
				pCard2 = pSet2.GetSingleCard(0, true)
			}
		}

		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		}
	case POKER_Pattern_FullHouse: // 葫蘆
		// 先取出三條的部分比
		ary_pThree1 := pTempSet1.getCardSetsByPattern(POKER_Pattern_ThreeOfAKind, 0)
		ary_pThree2 := pTempSet2.getCardSetsByPattern(POKER_Pattern_ThreeOfAKind, 0)
		if len(ary_pThree1) <= 0 || len(ary_pThree2) <= 0 {
			return POKER_Compare_Even
		}

		pSet1 := pTempSet1.DrawCards(ary_pThree1[0].CodeString())
		pSet2 := pTempSet2.DrawCards(ary_pThree2[0].CodeString())

		pCard1 := pSet1.GetSingleCard(0, true)
		pCard2 := pSet2.GetSingleCard(0, true)
		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		}

		// 後比對子的部分
		pCard1 = pTempSet1.GetSingleCard(0, true)
		pCard2 = pTempSet2.GetSingleCard(0, true)
		if pCard1.I32Number > pCard2.I32Number {
			return POKER_Compare_Dealer
		} else if pCard1.I32Number < pCard2.I32Number {
			return POKER_Compare_Player
		}
	case POKER_Pattern_RoyalFlush: // 皇家同花順
		return POKER_Compare_Even
	}

	return POKER_Compare_Even
}

// 檢查是哪種順子，回傳值：［是否有 A］、［是否為 1-5 順子］
func (pPoker *PokerCardSet) CheckAStraightType(pStraightSet *PokerCardSet) (bool, bool) {
	if pStraightSet == nil {
		return false, false
	}

	if pStraightSet.CardCount() != g_map_iPatternCardCount[POKER_Pattern_Straight] &&
		pStraightSet.CardCount() != g_map_iPatternCardCount[POKER_Pattern_StraightFlush] {
		return false, false
	}

	// 檢查是否為順子
	if pStraightSet.Pattern() != POKER_Pattern_Straight && pStraightSet.Pattern() != POKER_Pattern_StraightFlush {
		return false, false
	}

	bHasA, bIs15Straight := false, false

	// 檢查是否有 A
	for i := 0; i < pStraightSet.CardCount(); i++ {
		if pStraightSet.ary_pCards[i].I32Number == POKER_CardNo_1 {
			bHasA = true
			break
		}
	}

	if !bHasA {
		return bHasA, bIs15Straight
	}

	// 檢查是否為 1-5 順子
	i15Straight := 0
	for i := 0; i < pStraightSet.CardCount(); i++ {
		if pStraightSet.ary_pCards[i].I32Number >= POKER_CardNo_1 && pStraightSet.ary_pCards[i].I32Number <= POKER_CardNo_5 {
			i15Straight |= 1 << uint(pStraightSet.ary_pCards[i].I32Number-POKER_CardNo_1)
		}

		if i15Straight == 31 {
			bIs15Straight = true
			return bHasA, bIs15Straight
		}
	}

	return bHasA, bIs15Straight
}

// 取得牌組中的一張牌
func (pPoker *PokerCardSet) GetSingleCard(iIndex int, bOneAsAce bool) *PokerCard {
	if iIndex < 0 || iIndex >= pPoker.CardCount() {
		return NewPokerCard(POKER_CardColor_Other, POKER_CardNo_Hide)
	}

	pRetCard := NewPokerCard(pPoker.ary_pCards[iIndex].I32Color, pPoker.ary_pCards[iIndex].I32Number)
	if bOneAsAce && pRetCard.I32Number == POKER_CardNo_1 {
		pRetCard.I32Number = POKER_CardNo_A
	}

	return pRetCard
}

// 取得 PokerCardSet
func (pPoker *PokerCardSet) GetPokerCardSet() *PokerCardSet {
	pRetSet := new(PokerCardSet)
	pRetSet.SetCardSet(pPoker.CodeString())

	return pRetSet
}

// 檢查是否為同花類型
func (pPoker *PokerCardSet) IsFlushType() bool {
	if pPoker.CardCount() <= 1 {
		return false
	}

	i32Color := pPoker.ary_pCards[0].I32Color
	for i := 1; i < pPoker.CardCount(); i++ {
		if pPoker.ary_pCards[i].I32Color != i32Color {
			return false
		}
	}

	return true
}

// 牌組內是否有此數字
func (pPoker *PokerCardSet) IsNumberExisted(i32CheckNo int32) bool {
	for i := 0; i < pPoker.CardCount(); i++ {
		if pPoker.ary_pCards[i].I32Number == i32CheckNo {
			return true
		}
	}

	return false
}