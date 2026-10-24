package poker

import (
	"bytes"
	"mlog"
	"util"
)

//扑克
type NoShufflePoker struct {
	Cards []*PokerCard
}

func NewNoShufflePoker(hasKing bool, hasPizi bool, count int, groupPoker map[int]int) *NoShufflePoker {
	poker := new(NoShufflePoker)
	rand := util.RndInt(0, len(groupPoker))
	group := 4 - groupPoker[rand] //設定幾張一組
	arrCards := []*PokerCard{}
	mcards := make(map[int]*PokerCard)
	mapGroupCard := make(map[int][]*PokerCard)

	for ip := 0; ip < count; ip++ {

		var idx int32 = 2
		for i := idx; i <= 14; i++ {
			var card *PokerCard
			var groupCard []*PokerCard
			//黑桃
			card = NewPokerCard(PokerColor_HeiTao, i)
			if group < int(1) && i != int32(2) && i != int32(14) {
				groupCard = append(groupCard, card)
			} else {
				arrCards = append(arrCards, card)
			}

			//红桃
			card = NewPokerCard(PokerColor_HongTao, i)
			if group < int(2) && i != int32(2) && i != int32(14) {
				groupCard = append(groupCard, card)
			} else {
				arrCards = append(arrCards, card)
			}

			//梅花
			card = NewPokerCard(PokerColor_MeiHua, i)
			if group < int(3) && i != int32(2) && i != int32(14) {
				groupCard = append(groupCard, card)
			} else {
				arrCards = append(arrCards, card)
			}

			//方块
			card = NewPokerCard(PokerColor_FangKuai, i)
			if group < int(4) && i != int32(2) && i != int32(14) {
				groupCard = append(groupCard, card)
			} else {
				arrCards = append(arrCards, card)
			}

			if len(groupCard) != int(0) {
				mapGroupCard[int(i)] = groupCard
			}
		}

		//是否有王
		if hasKing {
			skcard := NewPokerCard(PokerColor_SmallKing, PokerNumber_SmallKing)
			bkcard := NewPokerCard(PokerColor_BigKing, PokerNumber_BigKing)

			mapGroupCard[int(PokerNumber_SmallKing)] = []*PokerCard{skcard}
			mapGroupCard[int(PokerNumber_BigKing)] = []*PokerCard{bkcard}
		}

		//是否有皮子
		if hasPizi {
			pzcard := NewPokerCard(PokerColor_PiZi, PokerNumber_PiZi)
			mapGroupCard[int(PokerNumber_BigKing)] = []*PokerCard{pzcard}
		}

		//不在牌組的隨機發
		mapGroupLen := len(mapGroupCard) + 5
		for k, c := range arrCards {
			if _, ok := mapGroupCard[k+mapGroupLen]; ok {
				mlog.Error("洗牌位置已存在")
			}
			mapGroupCard[k+mapGroupLen] = []*PokerCard{c}
		}

		for _, c := range mcards {
			poker.Cards = append(poker.Cards, c)
		}

		for _, c := range mapGroupCard {
			poker.Cards = append(poker.Cards, c...)
		}
	}

	if len(poker.Cards) != int(54) {
		mlog.Error("初始牌張數錯誤, 非54張, 實際:%v", len(poker.Cards))
	}

	return poker
}

//洗牌
func (p *NoShufflePoker) Shuffle() {

}

func (p *NoShufflePoker) UserCardsLen(uid int32) int {
	return len(p.Cards)
}

//洗牌一次
func (p *NoShufflePoker) ShuffleOnce() {

}

//洗多次牌
func (p *NoShufflePoker) ShuffleMore(times int) {

}

//发牌
func (p *NoShufflePoker) Deal(userid int32, cn int) *CardGroup {
	if len(p.Cards) < cn {
		return nil
	}

	ret := p.Cards[:cn]
	p.Cards = p.Cards[cn:]

	return NewCardGroup(ret)
}

//加牌
func (p *NoShufflePoker) AddCardGroup(userid int32, cg *CardGroup) {

}

//加牌
func (p *NoShufflePoker) AddCard(userid int32, card *PokerCard) {

}

//指定取牌號碼
func (p *NoShufflePoker) TakeCardNumber(userid int32, cardNumber int32) (bool, PokerCard) {
	return false, PokerCard{}
}

//指定取非存在的號碼
func (p *NoShufflePoker) TakeDiffCardNumber(userid int32, diffNums []int32) (bool, PokerCard) {
	return false, PokerCard{}
}

//换牌
func (p *NoShufflePoker) ExChange(userid int32, otheruserid int32) {

}

//转换为字符串
func (p *NoShufflePoker) String() string {
	var buf bytes.Buffer
	for _, c := range p.Cards {
		buf.WriteString(c.String() + ",")
	}

	return buf.String()
}

func (p *NoShufflePoker) TakeCardColor(userid int32, color int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		if v.Color == color {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

func (p *NoShufflePoker) TakeCardNumberAndColor(userid int32, cardNumber int32, color int32) (bool, PokerCard) {
	for k, v := range p.Cards {
		if v.CardNumber == cardNumber && v.Color == color {
			p.Cards = RemoveIndex(p.Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}
