package zhajinhualogic

import (
	"conf"
	"mlog"
	"poker"
	"testing"
	"util"
)

func Init()  {
	conf.CLogLevel = 2
	conf.FLogLevel = 2
}

// 測試取得單張
func TestGetDanZhang(t *testing.T) {
	cardTypeTest(SanPai, 10000)
}

// 測試取得對子
func TestGetDuiZi(t *testing.T) {
	cardTypeTest(DuiZi, 10000)
}

// 測試取得順子
func TestGetShuenZi(t *testing.T) {
	cardTypeTest(SunZi, 10000)
}

// 測試取得金花
func TestGetJinHua(t *testing.T) {
	cardTypeTest(JiaHua, 10000)
}

// 測試取得順金
func TestGetShuenJin(t *testing.T) {
	cardTypeTest(SunJin, 10000)
}

// 測試取得豹子
func TestGetBaoZi(t *testing.T) {
	cardTypeTest(BaoZi, 10000)
}

// 測試取得235
func TestGet235(t *testing.T) {
	cardTypeTest(S235, 10000)
}

// 測試牌型，傳入參數【牌型】【測試次數】
func cardTypeTest(iTestType, iTestCount int)  {
	Init()

	if iTestType < SanPai || iTestType >= CardType_Count {
		mlog.Error("要測試牌型錯誤: %d", iTestType)
		return
	}

	if iTestCount <= 0 {
		iTestCount = 10000
	}

	pPoker := poker.NewRandomPoker(false, false, 1)

	pUserCards := new(poker.CardGroup)
	pRest := new(poker.CardGroup)
	iCardType := SanPai
	iErrCount := 0
	iCardCount := COUNT_HandCards
	for i := 0; i < iTestCount; i++ {
		pPoker.Shuffle()
		iCardCount = COUNT_HandCards + util.RndInt(0, len(pPoker.Cards) - COUNT_HandCards)	// 亂數設定牌組張數
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
		example.invalid("設定牌組張數[%d], 實際張數[%d]", iCardCount, pCardSet.Len())

		pUserCards, iCardType, pRest = GetCardsByCardType(iTestType, pCardSet)
		SortCardsByNo(pUserCards)
		if iCardType != iTestType {
			mlog.Error("pUserCards:%v, Type[%s], pCardSet:%v", pUserCards, typeStringMap[iCardType], pRest)
			iErrCount++
		} else {
			mlog.Debug("pUserCards:%v, Type[%s], pCardSet:%v", pUserCards, typeStringMap[iCardType], pRest)
		}
	}

	if iErrCount > 0 {
		mlog.Warn("總測試數: %d, 未取得筆數: %d", iTestCount, iErrCount)
	} else {
		example.invalid("總測試數: %d, 測試完成", iTestCount)
	}
}
