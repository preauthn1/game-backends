package pokerset

// 將 CardID 轉換為 PokerCard
func CardID2PokerCard(i32CardID int32) *PokerCard {
	if i32CardID <= POKER_CardID_None || i32CardID >= POKER_CardID_Hide {
		return nil
	}

	i32Color := int32(-1)
	i32Number := int32(-1)

	if i32CardID >= POKER_CardID_C1 && i32CardID <= POKER_CardID_CK {
		// 梅花
		i32Color = POKER_CardColor_Club
		i32Number = i32CardID - POKER_OneColorCardsCount*i32Color
	} else if i32CardID >= POKER_CardID_D1 && i32CardID <= POKER_CardID_DK {
		// 方塊
		i32Color = POKER_CardColor_Diamond
		i32Number = i32CardID - POKER_OneColorCardsCount*i32Color
	} else if i32CardID >= POKER_CardID_H1 && i32CardID <= POKER_CardID_HK {
		// 紅桃
		i32Color = POKER_CardColor_Heart
		i32Number = i32CardID - POKER_OneColorCardsCount*i32Color
	} else if i32CardID >= POKER_CardID_S1 && i32CardID <= POKER_CardID_SK {
		// 黑桃
		i32Color = POKER_CardColor_Spade
		i32Number = i32CardID - POKER_OneColorCardsCount*i32Color
	} else {
		i32Color = POKER_CardColor_Other
		switch i32CardID {
		case POKER_CardID_J1: // 小王
			i32Number = POKER_CardNo_Joker1
		case POKER_CardID_J2: // 大王
			i32Number = POKER_CardNo_Joker2
		}
	}

	if i32Color == int32(-1) || i32Number == int32(-1) {
		return nil
	}

	pRetCard := NewPokerCard(i32Color, i32Number)

	return pRetCard
}

// 將 CardCode 轉換為 PokerCardSet
func CardCode2PokerCardSet(strCodeString string) *PokerCardSet {
	i32Len := int32(len(strCodeString))
	if (i32Len % int32(LEN_SingleCardCode) != 0) ||
		(i32Len / int32(LEN_SingleCardCode) > POKER_SetCardsCount * POKER_MaxSet) {
		return new(PokerCardSet)
	}

	pRetCardSet := new(PokerCardSet)
	var strColor, strNumber string
	for i := int32(0); i < i32Len; i += 2 {
		strColor = strCodeString[i:i + 1]
		if _, ok := g_map_i32ColorByCode[strColor]; !ok {
			continue
		} else {
			if i == i32Len - 2 {
				strNumber = strCodeString[i + 1:]
			} else {
				strNumber = strCodeString[i + 1 : i + 2]
			}

			if _, ok := g_map_i32NumberByCode[strNumber]; !ok {
				continue
			}
		}

		pCard := new(PokerCard)
		pCard.I32Color = g_map_i32ColorByCode[strColor]
		pCard.I32Number = g_map_i32NumberByCode[strNumber]

		pRetCardSet.AddSingleCard(pCard.CodeString())
	}

	return pRetCardSet
}

// 牌型字串
func PatternString(i32Pattern int32) string {
	if i32Pattern < POKER_Pattern_None && i32Pattern >= POKER_Pattern_Max {
		return ""
	}

	return g_map_strPattern[i32Pattern]
}

// 各牌型張數
func PatternCardCount(i32Pattern int32) int {
	if i32Pattern < POKER_Pattern_None && i32Pattern >= POKER_Pattern_Max {
		return 0
	}

	return g_map_iPatternCardCount[i32Pattern]
}