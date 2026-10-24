package pokerset

// 牌
type PokerCard struct {
	I32Color  int32 // 花色
	I32Number int32 // 數字
	I32State  int32 // 牌的狀態
}

//-----------------------------------------------------------------------------
func NewPokerCard(i32Color, i32Number int32) *PokerCard {
	if (i32Color < POKER_CardColor_Club || i32Color >= POKER_CardColor_Max) ||
		(i32Number <= POKER_CardNo_None || i32Number >= POKER_CardNo_Max) {
		return nil
	}

	pCard := new(PokerCard)
	pCard.I32Color = i32Color
	pCard.I32Number = i32Number

	return pCard
}

// 文字字串
func (pCard *PokerCard) String() string {
	return g_map_strCardColor[pCard.I32Color] + g_map_strCardNumber[pCard.I32Number]
}

func (pCard *PokerCard) CardID() int32 {
	i32CardID := POKER_CardID_None

	if pCard.I32Color >= POKER_CardColor_Club && pCard.I32Color <= POKER_CardColor_Spade {
		i32CardID = pCard.I32Color*POKER_OneColorCardsCount + pCard.I32Number
	} else if pCard.I32Color == POKER_CardColor_Other {
		switch pCard.I32Number {
		case POKER_CardNo_Joker1: // 小王
			i32CardID = POKER_CardID_J1
		case POKER_CardNo_Joker2: // 大王
			i32CardID = POKER_CardID_J2
		case POKER_CardNo_Hide: // 蓋牌
			i32CardID = POKER_CardID_Hide
		}
	}

	return i32CardID
}

// 代碼字串
func (pCard *PokerCard) CodeString() string {
	return g_map_strColorCode[pCard.I32Color] + g_map_strNumberCode[pCard.I32Number]
}
