package wzp

import (
	"example.invalid/ServerTeam/Servers/pkg/poker"
	"testing"
)

//是否一对
func TestIsOnePair(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isOnePair")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))

		ok := isOnePair(cards)

		if !ok {
			t.Errorf("是否一对 錯誤")
		}
	}
}

//是否两对
func TestIsTwoPair(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isTwoPair")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))
		ok := isTwoPair(cards)

		if !ok {
			t.Errorf("是否两对 錯誤")
		}
	}
}

//是否葫芦
func TestIsHulu(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isHulu")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))
		ok := isHulu(cards)

		if !ok {
			t.Errorf("是否葫芦 錯誤")
		}
	}
}

//是否三条
func TestIsSanTiao(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isSanTiao")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))

		ok := isSanTiao(cards)

		if !ok {
			t.Errorf("是否三条 錯誤")
		}
	}
}

//是否四条
func TestIsSitiao(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isSitiao")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))

		ok := isSitiao(cards)

		if !ok {
			t.Errorf("是否四条 錯誤")
		}
	}
}

//是否顺子
func TestIsShunzi(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isShunzi")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))

		ok := isShunzi(cards)

		if !ok {
			t.Errorf("是否顺子 錯誤")
		}
	}
}

//是否同花
func TestIsTongHua(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isTongHua")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))
		ok := isTongHua(cards)

		if !ok {
			t.Errorf("是否同花 錯誤")
		}
	}
}

//是否同花顺
func TestIsTonghuaShun(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isTonghuaShun")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))
		ok := isTonghuaShun(cards)

		if !ok {
			t.Errorf("是否同花顺 錯誤")
		}
	}
}

//是否同花大顺
func TestIsTonghuaDashun(t *testing.T) {
	var p poker.Poker
	p = poker.NewTestFilePoker("isTonghuaDashun")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()

	for i := 0; i < userLen; i++ {
		cards := fp.GetUserCards(int32(i))
		ok := isTonghuaDashun(cards)

		if !ok {
			t.Errorf("是否同花大顺 錯誤")
		}
	}
}
