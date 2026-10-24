package zhajinhualogic

import (
	"fixtures"
	"fmt"
	"mlog"
	"poker"
	"testing"
	"util"
)

// 讀取 Config.xlsx
func TestGetNaturalOddsSetting(t *testing.T) {
	Init()
	GetNaturalOddsSetting()
}

// 依機率表發牌
func TestFapaiByOddsTable(t *testing.T) {
	Init()
	fixtures.MongoInitByTest()
	GetNaturalOddsSetting()

	pPoker := poker.NewRandomPoker(false, false, 1)

	str := ""
	i32GameFlagID := int32(RoomNo_1)
	iUserCount := 5
	var ary_pUserCards []*poker.CardGroup
	iCardCount := COUNT_HandCards
	iCardCountNeeded := COUNT_HandCards * iUserCount
	for i := 0; i < 50; i++ {
		pPoker.Shuffle()
		iCardCount = iCardCountNeeded + util.RndInt(0, len(pPoker.Cards) - iCardCountNeeded)	// 亂數設定牌組張數
		iCount := 0
		pCardSet := new(poker.CardGroup)
		for _, pCard := range pPoker.Cards {
			if pCard != nil {
				pCardSet.AddCard(pCard)
				iCount++
				if iCount >= iCardCount {
					break
				}
			}
		}
		example.invalid("發牌次數[%d], 人數[%d], 設定牌組張數[%d], 實際張數[%d]", i, iUserCount, iCardCount, pCardSet.Len())

		ary_pUserCards, pCardSet = FapaiByOddsTable(i32GameFlagID, -1, iUserCount, pCardSet)
		str = ""
		iType := SanPai
		for j := 0; j < len(ary_pUserCards); j++ {
			iType = int(GetType(ary_pUserCards[j]))
			str += fmt.Sprintf("User[%d]%s(%s) ", j, ary_pUserCards[j].String(), typeStringMap[iType])
		}
		example.invalid("%s", str)
	}
}



