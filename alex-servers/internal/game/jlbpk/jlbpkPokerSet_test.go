package jlbpk

import (
	"testing"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/poker/pokerset"
)

// 建立新牌組，傳入參數［幾副撲克牌］、［鬼牌張數］
func TestJlbpkPokerSet_NewSet(t *testing.T) {
	pPokerSet := NewJlbpkPokerSet()
	pPokerSet.NewSet(1, 2)

	mlog.Debug("建立新牌組：%d 張牌", pPokerSet.CardCount())
	mlog.Debug("牌組[%s]", pPokerSet.String())
	mlog.Debug("代碼[%s]", pPokerSet.CodeString())
}

// 比牌，bCmpColor：是否要比花色
func TestJlbpkPokerSet_Compare(t *testing.T) {
	pPoker := NewJlbpkPokerSet()
	for i := pokerset.POKER_Pattern_HighCard; i <= pokerset.POKER_Pattern_RoyalFlush; i++ {
		for j := pokerset.POKER_Pattern_HighCard; j <= pokerset.POKER_Pattern_RoyalFlush; j++ {
			pPoker.NewSet(1, 0)
			pPoker.Shuffle()

			pSet1 := new(pokerset.PokerCardSet)
			pSet2 := new(pokerset.PokerCardSet)

			pSet1 = pPoker.DealByPattern(i, 5)
			pSet2 = pPoker.DealByPattern(j, 5)

			// 比牌
			pSet1.Sort(false, true, true)
			pSet2.Sort(false, true, true)
			if pSet1.Pattern() == pSet2.Pattern() {
				mlog.Debug("i = %d, j = %d", i, j)
				mlog.Debug("牌組 1：%s [%s]", pSet1.CodeString(), pokerset.PatternString(pSet1.Pattern()))
				mlog.Debug("牌組 2：%s [%s]", pSet2.CodeString(), pokerset.PatternString(pSet2.Pattern()))
				iWinner := pPoker.Compare(pSet1, pSet2, false)
				if iWinner == pokerset.POKER_Compare_Even {
					mlog.Debug("結果：平手")
				} else {
					mlog.Debug("結果：牌組 %d 贏", iWinner)
				}
				mlog.Debug("-------------------------------------------------")
			}
		}
	}
}
