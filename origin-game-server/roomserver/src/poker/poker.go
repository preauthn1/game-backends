package poker

import (
	"bytes"
)

type Poker interface {
	Shuffle()
	ShuffleMore(times int)
	Deal(userid int32, cn int) *CardGroup
	ExChange(userid int32, otheruserid int32)
	String() string
	AddCardGroup(userid int32, cg *CardGroup)
	AddCard(userid int32, card *PokerCard)
	TakeCardNumber(userid int32, cardNumber int32) (bool, PokerCard)
	TakeCardColor(userid int32, color int32) (bool, PokerCard)
	TakeCardNumberAndColor(userid int32, cardNumber int32, color int32) (bool, PokerCard)
	TakeDiffCardNumber(userid int32, diffNums []int32) (bool, PokerCard)
	UserCardsLen(uid int32) int
}

//扑克
type RandomPoker struct {
	Cards []*PokerCard
}

//创建扑克
func NewRandomPoker(hasKing bool, hasPizi bool, count int) *RandomPoker {
	poker := new(RandomPoker)

	for ip := 0; ip < count; ip++ {
		//黑桃
		var idx int32 = 2
		for i := idx; i <= 14; i++ {
			card := NewPokerCard(PokerColor_HeiTao, i)

			poker.Cards = append(poker.Cards, card)
		}

		//红桃
		for i := idx; i <= 14; i++ {
			card := NewPokerCard(PokerColor_HongTao, i)

			poker.Cards = append(poker.Cards, card)
		}

		//梅花
		for i := idx; i <= 14; i++ {
			card := NewPokerCard(PokerColor_MeiHua, i)

			poker.Cards = append(poker.Cards, card)
		}

		//方块
		for i := idx; i <= 14; i++ {
			card := NewPokerCard(PokerColor_FangKuai, i)

			poker.Cards = append(poker.Cards, card)
		}

		//是否有王
		if hasKing {
			skcard := NewPokerCard(PokerColor_SmallKing, PokerNumber_SmallKing)
			bkcard := NewPokerCard(PokerColor_BigKing, PokerNumber_BigKing)

			poker.Cards = append(poker.Cards, skcard)
			poker.Cards = append(poker.Cards, bkcard)
		}

		//是否有皮子
		if hasPizi {
			pzcard := NewPokerCard(PokerColor_PiZi, PokerNumber_PiZi)
			poker.Cards = append(poker.Cards, pzcard)
		}
	}

	return poker
}

//洗牌
func (p *RandomPoker) Shuffle() {
	p.ShuffleMore(3)
}


func (p *RandomPoker) UserCardsLen(uid int32) int {
	return len(p.Cards)
}

//洗牌一次
func (p *RandomPoker) ShuffleOnce() {
	mcards := make(map[int]*PokerCard)

	for idx, c := range p.Cards {
		mcards[idx] = c
	}

	p.Cards = make([]*PokerCard, 0)

	for _, c := range mcards {
		p.Cards = append(p.Cards, c)
	}
}

//洗多次牌
func (p *RandomPoker) ShuffleMore(times int) {
	for i := 0; i < times; i++ {
		p.ShuffleOnce()
	}
}

//发牌
func (p *RandomPoker) Deal(userid int32, cn int) *CardGroup {
	if len(p.Cards) < cn {
		return nil
	}

	ret := p.Cards[:cn]
	p.Cards = p.Cards[cn:]

	return NewCardGroup(ret)
}

//加牌
func (p *RandomPoker) AddCardGroup(userid int32, cg *CardGroup) {
	if len(cg.Cards) <= int(0) {
		return
	}

	p.Cards = append(p.Cards, cg.Cards...)
}

func (p *RandomPoker) AddCard(userid int32, card *PokerCard) {
	if card == nil {
		return
	}
	p.Cards = append(p.Cards, card)
}

//指定取牌號碼
func (p *RandomPoker) TakeCardNumber(userid int32, cardNumber int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		if v.CardNumber == cardNumber {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}
//指定取非存在的號碼
func (p *RandomPoker) TakeDiffCardNumber(userid int32, diffNums []int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		right := true
		for _, diffnum := range diffNums {
			if v.CardNumber == diffnum {
				right = false
			}
		}
		if right {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

//指定取牌花色
func (p *RandomPoker) TakeCardColor(userid int32, color int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		if v.Color == color {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

//指定取牌點數及花色
func (p *RandomPoker) TakeCardNumberAndColor(userid int32, cardNumber int32, color int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		if v.CardNumber == cardNumber && v.Color == color {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

//指定取牌花色
func (p *RandomPoker) TakeCardColorLessThen(userid int32, lessnum int32, color int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		if v.Color == color && v.CardNumber < lessnum {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}



func RemoveIndex(s []*PokerCard, index int) []*PokerCard {
	return append(s[:index], s[index+1:]...)
}

//换牌
func (p *RandomPoker) ExChange(userid int32, otheruserid int32) {

}

//转换为字符串
func (p *RandomPoker) String() string {
	var buf bytes.Buffer
	for _, c := range p.Cards {
		buf.WriteString(c.String() + ",")
	}

	return buf.String()
}
