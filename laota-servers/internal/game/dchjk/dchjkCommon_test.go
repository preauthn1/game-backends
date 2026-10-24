package dchjk

import (
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/conf"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	Pokerset "example.invalid/ServerTeam/Servers/pkg/poker/pokerset"
)

func init() {
	conf.CLogLevel = 0
	conf.FLogLevel = 0
}

// 莊家與玩家比牌
func TestCompareCards(t *testing.T) {
	for i := 0; i < 20; i++ {
		pPokerSet := Pokerset.NewPokerCardSet()
		pPokerSet.NewSet(JD21_PokerSetCount, 0)
		pPokerSet.Shuffle()

		pPlayerSet := NewCardSetInfo(JD21_IndexBase)
		pPset := pPokerSet.Deal(2)
		pPlayerSet.AddCardSet(pPset)
		if !pPlayerSet.bIsBlackJack && !pPlayerSet.bIsBusted && !pPlayerSet.IsSoftReached() {
			for {
				pCardSet := pPokerSet.Deal(1)
				pPlayerSet.AddCardSet(pCardSet)
				if pPlayerSet.IsSoftReached() {
					break
				}
			}
		}

		pDealerSet := NewCardSetInfo(JD21_IndexBase)
		pDset := pPokerSet.Deal(2)
		pDealerSet.AddCardSet(pDset)
		if !pDealerSet.bIsBlackJack && !pDealerSet.bIsBusted && !pDealerSet.IsSoftReached() {
			for {
				pCardSet := pPokerSet.Deal(1)
				pDealerSet.AddCardSet(pCardSet)
				if pDealerSet.IsSoftReached() {
					break
				}
			}
		}

		map_strResult := map[int]string{
			JD21_Compare_Even:   "平局",
			JD21_Compare_Dealer: "莊家贏",
			JD21_Compare_Player: "玩家贏",
		}

		mlog.Debug("莊家牌：%s (%d, %d), BJ[%t}, Bust[%t]", pDealerSet.pWholeCards.CodeString(), pDealerSet.i32Point_1, pDealerSet.i32Point_2,
			pDealerSet.bIsBlackJack, pDealerSet.bIsBusted)
		mlog.Debug("玩家牌：%s (%d, %d), BJ[%t}, Bust[%t]", pPlayerSet.pWholeCards.CodeString(), pPlayerSet.i32Point_1, pPlayerSet.i32Point_2,
			pPlayerSet.bIsBlackJack, pPlayerSet.bIsBusted)
		iResult := CompareCards(pDealerSet, pPlayerSet)
		mlog.Debug("比牌結果：%s", map_strResult[iResult])
		mlog.Debug("----------------------------------------------------------------------------------")
	}
}
