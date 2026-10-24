package zhajinhualogic

import (
	"poker"
	//	"sort"
	"mlog"
	"util"
)

const COUNT_HandCards		int = 3		// 手牌張數

const (
	SanPai = iota
	DuiZi
	SunZi
	JiaHua
	SunJin
	BaoZi
	S235
	CardType_Count
)

var typeStringMap = map[int]string{
	SanPai: "散牌",
	DuiZi:  "對子",
	SunZi:  "順子",
	JiaHua: "金花",
	SunJin: "順金",
	BaoZi:  "豹子",
	S235:	"235",
}

var special bool

//是否是豹子
func isBaozi(Cards *poker.CardGroup) bool {
	if Cards == nil {
		return false
	}

	if Cards.Len() != COUNT_HandCards {
		return false
	}

	if Cards.Cards[0].CardNumber == Cards.Cards[1].CardNumber && Cards.Cards[1].CardNumber == Cards.Cards[2].CardNumber {
		return true
	}

	return false
}

//是否是金花
func isJinHua(Cards *poker.CardGroup) bool {
	if Cards == nil {
		return false
	}

	if Cards.Len() != COUNT_HandCards {
		return false
	}

	if Cards.Cards[0].Color == Cards.Cards[1].Color && Cards.Cards[1].Color == Cards.Cards[2].Color {
		return true
	}
	return false
}

//是否是顺子
func isSunZi(Cards *poker.CardGroup) bool {
	if Cards == nil {
		return false
	}

	if Cards.Len() != COUNT_HandCards {
		return false
	}

	SortCardsByNo(Cards)

	if Cards.Cards[0].CardNumber+1 == Cards.Cards[1].CardNumber && Cards.Cards[1].CardNumber+1 == Cards.Cards[2].CardNumber {
		return true
	}

	//特殊123
	map_b123Check := map[int32]bool {
		1: false,
		2: false,
		3: false,
	}
	bCheckedOk := true

	for _, pCard := range Cards.Cards {
		if pCard != nil {
			if bValue, ok := map_b123Check[pCard.CardNumber]; ok {
				if !bValue {
					map_b123Check[pCard.CardNumber] = true
				}
			}
		}
	}

	for _, bValue := range map_b123Check {
		if !bValue {
			bCheckedOk = false
			break
		}
	}

	if bCheckedOk {
		special = true
		return true
	}

	//特殊AQK
	map_bAQKCheck := map[int32]bool {
		2: false,
		3: false,
		14: false,
	}
	bCheckedOk = true

	for _, pCard := range Cards.Cards {
		if pCard != nil {
			if bValue, ok := map_bAQKCheck[pCard.CardNumber]; ok {
				if !bValue {
					map_bAQKCheck[pCard.CardNumber] = true
				}
			}
		}
	}

	for _, bValue := range map_bAQKCheck {
		if !bValue {
			bCheckedOk = false
			break
		}
	}

	if bCheckedOk {
		return true
	}

	return false
}

//是否是对子
func isDuiZi(Cards *poker.CardGroup) bool {
	if Cards == nil {
		return false
	}

	if Cards.Len() != COUNT_HandCards {
		return false
	}

	if Cards.Cards[0].CardNumber == Cards.Cards[1].CardNumber ||
		Cards.Cards[1].CardNumber == Cards.Cards[2].CardNumber	||
		Cards.Cards[0].CardNumber == Cards.Cards[2].CardNumber {
		return true
	}

	return false
}

// 是否為 235
func is235(Cards *poker.CardGroup) bool {
	if Cards == nil {
		return false
	}

	if Cards.Len() != COUNT_HandCards {
		return false
	}

	map_b235Check := map[int32]bool {
		2: false,
		3: false,
		5: false,
	}
	
	for _, pCard := range Cards.Cards {
		if pCard != nil {
			if bValue, ok := map_b235Check[pCard.CardNumber]; ok {
				if !bValue {
					map_b235Check[pCard.CardNumber] = true
				}
			}
		}
	}
	
	for _, bValue := range map_b235Check {
		if !bValue {
			return false
		}
	}

	return true
}

//获取牌型
func GetType(Cards *poker.CardGroup) int32 {
	//默认已经排号序
	if Cards == nil || len(Cards.Cards) <= 0 {
		mlog.Debug("牌是空的")
		return SanPai
	}

	if len(Cards.Cards) != 3 {
		mlog.Debug("牌組長度不正確")
		return SanPai
	}

	if is235(Cards) {
		return S235
	}

	if isJinHua(Cards) {
		if isSunZi(Cards) {
			return SunJin
		}
		return JiaHua
	}

	if isSunZi(Cards) {
		return SunZi
	}

	if isBaozi(Cards) {
		return BaoZi
	}

	if isDuiZi(Cards) {
		return DuiZi
	}

	return SanPai
}

//比较用户牌型
func comPareCardsByNoAndColor(Cards1 *poker.CardGroup, Cards2 *poker.CardGroup) int {
	if Cards1 == nil && Cards2 == nil {
		mlog.Debug("兩副牌均為空")
		return -1
	}

	if Cards1 == nil || len(Cards1.Cards) <= 0 {
		mlog.Debug("第一組牌為空的")
		return USER2
	}

	if Cards2 == nil || len(Cards2.Cards) <= 0 {
		mlog.Debug("第二組牌為空的")
		return USER1
	}

	if Cards1.Cards[2].CardNumber == 14 && Cards2.Cards[2].CardNumber != 14 {
		return USER1
	} else if Cards1.Cards[2].CardNumber != 14 && Cards2.Cards[2].CardNumber == 14 {
		return USER2
	}
	//遍历比较点数大小
	for i := 2; i >= 0; i-- {
		if Cards1.Cards[i].CardNumber > Cards2.Cards[i].CardNumber {
			return USER1
		}

		if Cards1.Cards[i].CardNumber < Cards2.Cards[i].CardNumber {
			return USER2
		}
	}

	return comPareCardsByColor(Cards1, Cards2)
}

//最大那个牌的花色
func comPareCardsByColor(Cards1 *poker.CardGroup, Cards2 *poker.CardGroup) int {
	if Cards1 == nil && Cards2 == nil {
		mlog.Debug("兩副牌均為空")
		return -1
	}

	if Cards1 == nil || len(Cards1.Cards) <= 0 {
		mlog.Debug("第一組牌為空的")
		return USER2
	}

	if Cards2 == nil || len(Cards2.Cards) <= 0 {
		mlog.Debug("第二組牌為空的")
		return USER1
	}

	if Cards1.Cards[2].Color > Cards2.Cards[2].Color {
		return USER1
	}
	return USER2
}

//返回1为card1用户赢
func comPareCards(Cards1 *poker.CardGroup, Cards2 *poker.CardGroup) int {
	if Cards1 == nil && Cards2 == nil {
		mlog.Debug("兩副牌均為空")
		return -1
	}

	if Cards1 == nil || len(Cards1.Cards) <= 0 {
		mlog.Debug("第一組牌為空的")
		return USER2
	}

	if Cards2 == nil || len(Cards2.Cards) <= 0 {
		mlog.Debug("第二組牌為空的")
		return USER1
	}

	type1 := GetType(Cards1)
	type2 := GetType(Cards2)

	//特殊情况235和豹子
	if type1 == BaoZi && type2 == SanPai {
		if Cards2.Cards[0].CardNumber == 2 && Cards2.Cards[1].CardNumber == 3 && Cards2.Cards[2].CardNumber == 5 {
			return USER2
		}
	}

	if type2 == BaoZi && type1 == SanPai {
		if Cards1.Cards[0].CardNumber == 2 && Cards1.Cards[1].CardNumber == 3 && Cards1.Cards[2].CardNumber == 5 {
			return USER1
		}
	}

	if type1 > type2 {
		return USER1
	}

	if type1 < type2 {
		return USER2
	}

	if (type1 == SunJin || type1 == SunZi) && special {
		if Cards1.Cards[0].CardNumber == 2 && Cards1.Cards[1].CardNumber == 3 && Cards1.Cards[2].CardNumber == 14 {
			//特殊123為最小牌型，非123的順子為贏家
			if !(Cards2.Cards[0].CardNumber == 2 && Cards2.Cards[1].CardNumber == 3 && Cards2.Cards[2].CardNumber == 14) {
				special = false
				return USER2
			}
		} else if Cards2.Cards[0].CardNumber == 2 && Cards2.Cards[1].CardNumber == 3 && Cards2.Cards[2].CardNumber == 14 {
			if !(Cards1.Cards[0].CardNumber == 2 && Cards1.Cards[1].CardNumber == 3 && Cards1.Cards[2].CardNumber == 14) {
				special = false
				return USER1
			}
		}
	}

	if type1 == DuiZi {
		card1_tmp := Cards1.Cards[0]
		if Cards1.Cards[1].CardNumber == Cards1.Cards[2].CardNumber {
			card1_tmp = Cards1.Cards[1]
		}

		card2_tmp := Cards2.Cards[0]
		if Cards2.Cards[1].CardNumber == Cards2.Cards[2].CardNumber {
			card2_tmp = Cards2.Cards[1]
		}

		if card1_tmp.CardNumber > card2_tmp.CardNumber {
			return USER1
		}

		if card1_tmp.CardNumber < card2_tmp.CardNumber {
			return USER2
		}
	}
	return comPareCardsByNoAndColor(Cards1, Cards2)
}

// 設定賭神機器人的牌，回傳順序為［機器人手牌］->［發完牌剩下的牌］
func setGodRobotCards(pRestCards *poker.CardGroup, pRobotCards *poker.CardGroup, pUserCards *poker.CardGroup) (*poker.CardGroup, *poker.CardGroup) {
	if pRestCards == nil || pRobotCards == nil || pUserCards == nil {
		mlog.Debug("無牌組資料")
		return nil, nil
	}

	iUserCardType := GetType(pUserCards)
	iGodCardType := iUserCardType + 1
	if iGodCardType >= BaoZi {
		iGodCardType = BaoZi
	}

	pCardSet := new(poker.CardGroup)
	pCardSet.AddCardGroup(pRestCards)
	pCardSet.AddCardGroup(pRobotCards)

	pTemp := new(poker.CardGroup)

	switch iGodCardType {
	case DuiZi: // 對子
		map_iCardNo := make(map[int32]int)
		iCardNo := poker.PokerNumber_Two
		for _, pCard := range pCardSet.Cards {
			if pCard != nil {
				map_iCardNo[pCard.CardNumber]++
			}
		}

		for iNo, iAmount := range map_iCardNo {
			if iAmount >= 2 {
				iCardNo = iNo
				break
			}
		}

		iCount := 0
		for _, pCard := range pCardSet.Cards {
			if pCard != nil {
				if iCount >= 2 {
					pTemp.AddCard(pCard)
					break
				} else {
					if pCard.CardNumber == iCardNo {
						pTemp.AddCard(pCard)
						iCount++
					}
				}
			}
		}
	case SunZi: // 順子
		// 為防止造成無窮迴圈，故設定最多找 10 次
		for i := 0; i < 10; i++ {
			iCardNo := int32(util.RndInt(int(poker.PokerNumber_Two), int(poker.PokerNumber_A)))
			pTemp = RobotGetShuenZi(pCardSet, iCardNo, false)
			if pTemp != nil && pTemp.Len() == 3 {
				break
			}
		}
	case JiaHua: // 金花
		var iColor int32
		iRnd := util.RndInt(int(poker.PokerColor_FangKuai), int(poker.PokerColor_HeiTao)+1)
		for i := iRnd; i < iRnd+4; i++ {
			if pTemp.Len() >= 3 {
				break
			}

			iColor = int32(i % 4)
			for _, pCard := range pCardSet.Cards {
				if pCard != nil {
					if pCard.Color == iColor {
						pTemp.AddCard(pCard)
					}

					if pTemp.Len() >= 3 {
						break
					}
				}
			}
		}
	case SunJin: // 順金
		// 為防止造成無窮迴圈，故設定最多找 10 次
		for i := 0; i < 10; i++ {
			iCardNo := int32(util.RndInt(int(poker.PokerNumber_Two), int(poker.PokerNumber_A)))
			pTemp = RobotGetShuenZi(pCardSet, iCardNo, true)
			if pTemp != nil && pTemp.Len() == 3 {
				break
			}
		}
	case BaoZi: // 豹子
		map_iCardNo := make(map[int32]int)
		for _, pCard := range pCardSet.Cards {
			if pCard != nil {
				map_iCardNo[pCard.CardNumber]++
			}
		}

		if iUserCardType == BaoZi {
			// 玩家牌型為豹子
			for iNo, iCount := range map_iCardNo {
				if pTemp.Len() >= 3 {
					break
				}

				if iCount >= 3 && iNo > pUserCards.Cards[0].CardNumber {
					for _, pCard := range pCardSet.Cards {
						if pCard != nil {
							if pCard.CardNumber == iNo {
								pTemp.AddCard(pCard)

								if pTemp.Len() >= 3 {
									break
								}
							}
						}
					}
				}
			}
		} else {
			for iNo, iCount := range map_iCardNo {
				if pTemp.Len() >= 3 {
					break
				}

				if iCount >= 3 {
					for _, pCard := range pCardSet.Cards {
						if pCard.CardNumber == iNo {
							pTemp.AddCard(pCard)

							if pTemp.Len() >= 3 {
								break
							}
						}
					}
				}
			}
		}
	}

	// 設定牌組
	if pTemp.Len() == 3 {
		pCardSet.DeleteGroup(pTemp)
		return pTemp, pCardSet
	}

	return pRobotCards, pRestCards
}

func RobotGetShuenZi(pCardSet *poker.CardGroup, iStartNo int32, bIsShuenJin bool) *poker.CardGroup {
	if pCardSet == nil {
		return nil
	}

	if iStartNo < poker.PokerNumber_Two || iStartNo > poker.PokerNumber_A-2 {
		return nil
	}

	pTemp := new(poker.CardGroup)
	pTempSet := new(poker.CardGroup)
	pTempSet.AddCardGroup(pCardSet)
	SortCardsByNo(pTempSet)
	iCardNo := iStartNo
	for _, pCard := range pTempSet.Cards {
		if pCard != nil {
			if pCard.CardNumber == iCardNo {
				if pTemp.Len() <= 0 {
					pTemp.AddCard(pCard)
					iCardNo++
				} else {
					if !bIsShuenJin {
						pTemp.AddCard(pCard)
						iCardNo++
					} else if pCard.Color == pTemp.Cards[0].Color {
						pTemp.AddCard(pCard)
						iCardNo++
					}
				}
			}

			if pTemp.Len() == 3 || iCardNo > poker.PokerNumber_A {
				break
			}
		}
	}

	return pTemp
}

// 依數字由小到大排序牌組
func SortCardsByNo(pCardSet *poker.CardGroup) {
	if pCardSet == nil {
		return
	}

	iLen := len(pCardSet.Cards)
	for i := 0; i < iLen; i++ {
		for j := i; j < iLen; j++ {
			i32Card1 := pCardSet.Cards[i].CardNumber
			i32Card2 := pCardSet.Cards[j].CardNumber
			if i32Card1 > i32Card2 {
				pCardSet.Swap(i, j)
			}
		}
	}
}

// 依花色及數字由小到大排序牌組
func SortCards(pCardSet *poker.CardGroup) {
	if pCardSet == nil {
		return
	}

	iLen := len(pCardSet.Cards)
	for i := 0; i < iLen; i++ {
		for j := i; j < iLen; j++ {
			i32Card1 := pCardSet.Cards[i].CardNumber * 10 + pCardSet.Cards[i].Color
			i32Card2 := pCardSet.Cards[j].CardNumber * 10 + pCardSet.Cards[j].Color
			if i32Card1 > i32Card2 {
				pCardSet.Swap(i, j)
			}
		}
	}
}

// 取得特定牌型，傳入【牌型】【發牌用牌組】，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
// 如果牌組內沒有指定的牌型，則發【單張】
func GetCardsByCardType(iCardType int, pCardSet *poker.CardGroup) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	if iCardType < SanPai || iCardType >= CardType_Count {
		mlog.Debug("無此牌型")
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	iType := SanPai

	switch iCardType {
	case SanPai: // 单牌
		pUserCards, iType, pRest = getDanZhang(pCardSet)
	case DuiZi:  // 对子
		pUserCards, iType, pRest = getDuiZi(pCardSet)
	case SunZi:  // 顺子
		pUserCards, iType, pRest = getShuenZiShuenJin(pCardSet, false)
	case JiaHua: // 金花
		pUserCards, iType, pRest = getJinHua(pCardSet)
	case SunJin: // 顺金
		pUserCards, iType, pRest = getShuenZiShuenJin(pCardSet, true)
	case BaoZi:  // 豹子
		pUserCards, iType, pRest = getBaoZi(pCardSet)
	case S235:	// 235
		pUserCards, iType, pRest = get235(pCardSet)
	}

	return pUserCards, iType, pRest
}

// 取得單張，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
func getDanZhang(pCardSet *poker.CardGroup) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iType := SanPai
	iLen := pRest.Len()
	iRnd := util.RndInt(0, iLen + 1)
	for i := iRnd; i < iRnd + iLen; i++ {
		pCard := pRest.Cards[int32(i % iLen)]
		if pCard != nil {
			if pUserCards.Len() < 1 {
				pUserCards.AddCard(pCard)
			} else {
				bCheckedOk := true
				for _, pData := range pUserCards.Cards {
					if pData != nil {
						if pCard.CardNumber == pData.CardNumber {
							bCheckedOk = false
							break
						}
					}
				}

				if bCheckedOk {
					pUserCards.AddCard(pCard)
					if pUserCards.Len() == COUNT_HandCards {
						if i == iRnd + iLen - 1 {
							break
						} else {
							if GetType(pUserCards) == SanPai {
								break
							} else {
								pUserCards.DeleteCard(pCard)
							}
						}
					}
				}
			}
		}
	}

	// 設定牌組
	if pUserCards.Len() == COUNT_HandCards {
		SortCardsByNo(pUserCards)
		pRest.DeleteGroup(pUserCards)
		iType = int(GetType(pUserCards))
		return pUserCards, iType, pRest
	}

	return pUserCards, iType, pRest
}

// 取得對子，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
func getDuiZi(pCardSet *poker.CardGroup) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iType := SanPai

	// 各數字數量
	map_iCardNoCount := make(map[int32]int)
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			map_iCardNoCount[pCard.CardNumber]++
		}
	}

	var i32CardNo int32

	// 先找出有 2 張以上的牌
	for i32No, iCount := range map_iCardNoCount {
		if iCount >= 2 {
			i32CardNo = i32No
			break
		}
	}

	// 先取出對子
	iCount := 0
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			if pCard.CardNumber == i32CardNo {
				pUserCards.AddCard(pCard)
				iCount++
			}

			if iCount >= 2 {
				break
			}
		}
	}

	// 補上剩下的一張牌
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			if pCard.CardNumber != i32CardNo {
				pUserCards.AddCard(pCard)
				break
			}
		}
	}

	// 設定牌組
	if pUserCards.Len() == COUNT_HandCards {
		SortCardsByNo(pUserCards)
		pRest.DeleteGroup(pUserCards)
		iType = int(GetType(pUserCards))
	} else {
		pUserCards, iType, pRest = getDanZhang(pCardSet)
	}

	return pUserCards, iType, pRest
}

// 取得順子/順金，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
func getShuenZiShuenJin(pCardSet *poker.CardGroup, bIsShuenJin bool) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iType := SanPai

	map_iCardCount := make(map[int32]int)
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			map_iCardCount[pCard.CardNumber]++
		}
	}

	iDiff := int(poker.PokerNumber_A - 2) - int(poker.PokerNumber_Two - 1)
	iRnd := util.RndInt(0, iDiff)
	i32StartNo := poker.PokerNumber_Two
	bHaveShuenZi := false
	pTemp := new(poker.CardGroup)
	pTemp.AddCardGroup(pCardSet)
	if bIsShuenJin {
		// 若要順金，則依花色及數字排序
		SortCards(pTemp)
	} else {
		// 若要順子，則依數字排序
		SortCardsByNo(pTemp)
	}

	for i := iRnd; i < iRnd + iDiff; i++ {
		bHaveShuenZi = true
		// 取得開始數字
		i32StartNo = poker.PokerNumber_Two + int32(i % iDiff)
		for j := 0; j < COUNT_HandCards; j++ {
			if map_iCardCount[i32StartNo] <= 0 {
				bHaveShuenZi = false
				break
			}
		}

		if bHaveShuenZi {
			//mlog.Warn("[%d] Temp: %v", i32StartNo, pTemp)
			// 有取到順子
			bDone := true
			i32CardNo := i32StartNo
			i32Color := poker.PokerColor_FangKuai
			for _, pCard := range pTemp.Cards {
				if pCard != nil {
					if pCard.CardNumber == i32CardNo {
						if pUserCards.Len() == 1 {
							i32Color = pUserCards.Cards[0].Color
						}

						if bIsShuenJin {
							if pCard.Color == i32Color {
								pUserCards.AddCard(pCard)
								i32CardNo++
							}
						} else {
							pUserCards.AddCard(pCard)
							i32CardNo++
						}
					}

					if pUserCards.Len() == COUNT_HandCards {
						if GetType(pUserCards) == SunJin {
							if !bIsShuenJin {
								// 要順子，卻得到順金
								pUserCards.DeleteGroup(pUserCards)
								bDone = false
							}
						} else if GetType(pUserCards) == SunZi {
							if bIsShuenJin {
								// 要順金，卻得到順子
								pUserCards.DeleteGroup(pUserCards)
								bDone = false
							}
						} else {
							pUserCards.DeleteGroup(pUserCards)
							bDone = false
						}
						break
					}
				}
			}

			if bDone {
				break
			}
		}
	}

	// 設定牌組
	if pUserCards.Len() == COUNT_HandCards {
		SortCardsByNo(pUserCards)
		pRest.DeleteGroup(pUserCards)
		iType = int(GetType(pUserCards))
	} else {
		pUserCards, iType, pRest = getDanZhang(pCardSet)
	}

	return pUserCards, iType, pRest
}

// 取得金花，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
func getJinHua(pCardSet *poker.CardGroup) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iType := SanPai

	// 各花色數量
	map_iCardColorCount := make(map[int32]int)
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			map_iCardColorCount[pCard.Color]++
		}
	}
	var i32Color int32

	// 隨機選取花色
	iRnd := util.RndInt(int(poker.PokerColor_FangKuai), int(poker.PokerColor_HeiTao) + 1)
	for i := iRnd; i < iRnd + 4; i++ {
		i32Color = int32(i % 4)
		if map_iCardColorCount[i32Color] >= COUNT_HandCards {
			break
		}
	}

	for _, pCard := range pRest.Cards {
		if pCard != nil {
			if pCard.Color == i32Color {
				pUserCards.AddCard(pCard)
				if pUserCards.Len() == COUNT_HandCards {
					SortCardsByNo(pUserCards)
					if GetType(pUserCards) == SunJin || GetType(pUserCards) == S235 {
						// 排除順金或235
						pUserCards.DeleteCard(pCard)
					}
				}
			}

			if pUserCards.Len() == COUNT_HandCards {
				break
			}
		}
	}

	// 設定牌組
	if pUserCards.Len() == COUNT_HandCards {
		SortCardsByNo(pUserCards)
		pRest.DeleteGroup(pUserCards)
		iType = int(GetType(pUserCards))
	} else {
		pUserCards, iType, pRest = getDanZhang(pCardSet)
	}

	return pUserCards, iType, pRest
}

// 取得豹子，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
func getBaoZi(pCardSet *poker.CardGroup) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iType := SanPai

	// 各數字數量
	map_iCardNoCount := make(map[int32]int)
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			map_iCardNoCount[pCard.CardNumber]++
		}
	}

	var i32CardNo int32

	bGet := false
	iDiff := int(poker.PokerNumber_A - 1) - int(poker.PokerNumber_Two)
	iRnd := util.RndInt(0, iDiff)
	for i := iRnd; i <= iRnd + iDiff; i++ {
		i32CardNo = poker.PokerNumber_Two + int32(i % iDiff)
		if map_iCardNoCount[i32CardNo] >= 3 {
			bGet = true
			break
		}
	}

	if bGet {
		for _, pCard := range pRest.Cards {
			if pCard != nil {
				if pCard.CardNumber == i32CardNo {
					pUserCards.AddCard(pCard)
				}

				if pUserCards.Len() == COUNT_HandCards {
					break
				}
			}
		}
	}

	// 設定牌組
	if pUserCards.Len() == COUNT_HandCards {
		SortCardsByNo(pUserCards)
		pRest.DeleteGroup(pUserCards)
		iType = int(GetType(pUserCards))
	} else {
		pUserCards, iType, pRest = getDanZhang(pCardSet)
	}

	return pUserCards, iType, pRest
}

// 取得235，回傳【玩家手牌】【牌型】【發完牌剩下的牌組】
func get235(pCardSet *poker.CardGroup) (*poker.CardGroup, int, *poker.CardGroup) {
	if pCardSet == nil {
		mlog.Debug("無發牌用牌組資料")
		return nil, SanPai, nil
	}

	if pCardSet.Len() < COUNT_HandCards {
		mlog.Debug("牌組張數不足 %d 張", COUNT_HandCards)
		return nil, SanPai, pCardSet
	}

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	pRest.AddCardGroup(pCardSet)
	iType := SanPai

	map_iCardCount := make(map[int32]int)
	for _, pCard := range pRest.Cards {
		if pCard != nil {
			map_iCardCount[pCard.CardNumber]++
		}
	}

	map_b235Check := map[int32]bool {
		2: false,
		3: false,
		5: false,
	}

	if map_iCardCount[int32(2)] > 0 && map_iCardCount[int32(3)] > 0 && map_iCardCount[int32(5)] > 0 {
		for _, pCard := range pRest.Cards {
			if pCard != nil {
				if bValue, ok := map_b235Check[pCard.CardNumber]; ok {
					if !bValue {
						pUserCards.AddCard(pCard)
						map_b235Check[pCard.CardNumber] = true
					}
				}

				if pUserCards.Len() == COUNT_HandCards {
					break
				}
			}
		}
	}

	// 設定牌組
	if pUserCards.Len() == COUNT_HandCards {
		SortCardsByNo(pUserCards)
		pRest.DeleteGroup(pUserCards)
		iType = int(GetType(pUserCards))
	} else {
		pUserCards, iType, pRest = getDanZhang(pCardSet)
	}

	return pUserCards, iType, pRest
}
