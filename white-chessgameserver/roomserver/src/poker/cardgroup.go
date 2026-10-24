package poker

import (
	"bytes"
	"fmt"
	"gamelogic"
	"mlog"
	"sort"
	"strconv"
)

//牌组
type CardGroup struct {
	Cards []*PokerCard
}

//创建牌组对象
func NewCardGroup(cards []*PokerCard) *CardGroup {
	cg := new(CardGroup)
	if cards != nil {
		cg.Cards = append(cg.Cards, cards...)
	}
	return cg
}

//创建空牌组对象
func NewEmptyCardGroup(cn int) *CardGroup {
	cg := new(CardGroup)
	for i := 0; i < cn; i++ {
		cg.AddCard(NewEmptyPokerCard())
	}

	return cg
}

//洗牌
func (cg *CardGroup) ShuffleOnce() {
	mcards := make(map[int]*PokerCard)

	for idx, c := range cg.Cards {
		mcards[idx] = c
	}

	cg.Cards = make([]*PokerCard, 0)

	for _, c := range mcards {
		cg.Cards = append(cg.Cards, c)
	}
}

//添加牌
func (cg *CardGroup) AddCard(pk *PokerCard) {
	cg.Cards = append(cg.Cards, pk)
}

//添加牌组
func (cg *CardGroup) AddCardGroup(ncg *CardGroup) {
	cg.Cards = append(cg.Cards, ncg.Cards...)
}

//是否同样的牌组
func (cg *CardGroup) IsEqual(ncg *CardGroup) bool {
	if cg.Len() != ncg.Len() {
		return false
	}

	sort.Sort(cg)
	sort.Sort(ncg)

	for idx, c := range cg.Cards {
		if !c.IsEqual(ncg.Cards[idx]) {
			return false
		}
	}

	return true
}

func (cg *CardGroup) Len() int {
	return len(cg.Cards)
}

func (cg *CardGroup) Swap(i, j int) {
	cg.Cards[i], cg.Cards[j] = cg.Cards[j], cg.Cards[i]
}

func (cg *CardGroup) Less(i, j int) bool {
	cardi, _ := strconv.Atoi(fmt.Sprintf("%d%2d", cg.Cards[i].Color, cg.Cards[i].CardNumber))
	cardj, _ := strconv.Atoi(fmt.Sprintf("%d%2d", cg.Cards[j].Color, cg.Cards[j].CardNumber))

	return cardi > cardj
}

func (cg *CardGroup) String() string {
	if cg == nil {
		return ""
	}
	var buf bytes.Buffer
	for _, c := range cg.Cards {
		buf.WriteString(fmt.Sprintf("%s,", c.String()))
	}

	return buf.String()
}

func (cg *CardGroup) FixedString() string {
	if cg == nil {
		return ""
	}
	var buf bytes.Buffer
	for _, c := range cg.Cards {
		buf.WriteString(fmt.Sprintf("%d,%d|", c.Color, c.CardNumber))
	}

	return buf.String()
}

//检测给定牌组是否属于玩家手牌 检测出牌人 出的牌是否合法
func (cg *CardGroup) IsContent(cards []*PokerCard) bool {
	l := len(cards)
	var isContent bool

	for i := 0; i < l; i++ {
		isContent = false

		for _, v := range cg.Cards {
			if !v.IsEqual(cards[i]) {
				continue
			}
			isContent = true
			break
		}
		if !isContent {
			return false
		}
	}
	return true
}

func (cg *CardGroup) DeleteGroup(group *CardGroup) {
	for _, v := range group.Cards {
		cg.DeleteCard(v)
	}
}

func (cg *CardGroup) DeleteCard(card *PokerCard) {
	tmp := make([]*PokerCard, 0)
	for _, v := range cg.Cards {
		if !v.IsEqual(card) {
			tmp = append(tmp, v)
		}
	}
	cg.Cards = tmp
}

func (cg *CardGroup) DeleteCardForPokers(card *PokerCard) {
	for i := 0; i < len(cg.Cards); i++ {
		if card.IsEqual(cg.Cards[i]) {
			cg.Cards = append(cg.Cards[:i], cg.Cards[i+1:]...)
			break
		}
	}

	mlog.Debug("card:", len(cg.Cards))
}

//获取最小牌点
func (cg *CardGroup) GetMinCardNumber() int32 {
	return cg.GetMinCard().CardNumber
}

//获取最小牌型
func (cg *CardGroup) GetMinCard() *PokerCard {
	minCard := cg.Cards[0]

	for _, c := range cg.Cards {
		if c.CardNumber < minCard.CardNumber {
			minCard = c
		}
	}

	return minCard
}

//获取最大牌点
func (cg *CardGroup) GetMaxCardNumber() int32 {
	return cg.GetMaxCard().CardNumber
}

//获取最大牌型
func (cg *CardGroup) GetMaxCard() *PokerCard {
	maxCard := cg.Cards[0]

	for _, c := range cg.Cards {
		if c.CardNumber > maxCard.CardNumber {
			maxCard = c
		}
	}

	return maxCard
}

//是否有A
func (cg *CardGroup) HasA() bool {
	for _, c := range cg.Cards {
		if c.IsA() {
			return true
		}
	}

	return false
}

//牌10到K的張數
func (cg *CardGroup) HasTenToThirteen() int {
	count := int(0)
	for _, c := range cg.Cards {
		if c.CardNumber >= int32(10) && c.CardNumber <= int32(13) {
			count++
		}
	}
	return count
}

func (cg *CardGroup) HasKingToPizi() int {
	count := int(0)
	for _, c := range cg.Cards {
		if c.CardNumber >= int32(15) && c.CardNumber <= int32(17) {
			count++
		}
	}
	return count
}

//是否顺子
func (cg *CardGroup) IsShunzi() bool {
	if len(cg.Cards) == 0 {
		return false
	}

	if cg.HasA() && cg.IsShunziFromOne() {
		return true
	}

	min := cg.GetMinCardNumber()
	max := cg.GetMaxCardNumber()

	for i := min; i <= max; i++ {
		if len(cg.GetPokerCardByNumber(i)) != 1 {
			return false
		}
	}

	return true
}

//是否1点开始的顺子
func (cg *CardGroup) IsShunziFromOne() bool {
	if cg.HasA() {
		max := len(cg.Cards)
		for i := 1; i <= max; i++ {
			if len(cg.GetPokerCardByNumber(int32(i))) != 1 {
				return false
			}
		}

		return true
	}

	return false
}

//根据牌点获取牌型
func (cg *CardGroup) GetPokerCardByNumber(cn int32) []*PokerCard {
	ncg := make([]*PokerCard, 0)

	for _, c := range cg.Cards {
		if c.CardNumber == cn {
			ncg = append(ncg, c)
		}

		if cn == 1 && c.IsA() {
			ncg = append(ncg, c)
		}
	}

	return ncg
}

//是否同花
func (cg *CardGroup) IsTonghua() bool {
	if len(cg.Cards) == 0 {
		return false
	}

	//检查是否同花
	color := cg.Cards[0].Color
	for k := 1; k < len(cg.Cards); k++ {
		c := cg.Cards[k]

		if c.Color != color {
			return false
		}
	}

	return true
}

//是否同花顺
func (cg *CardGroup) IsTongHuaShun() bool {
	return cg.IsTonghua() && cg.IsShunzi()
}

//牌堆
type CardStack struct {
	Groups []*CardGroup
}

//创建牌堆对象
func NewCardStack() *CardStack {
	cst := new(CardStack)
	return cst
}

//添加牌组
func (cs *CardStack) AddCardGroup(cg *CardGroup) {
	cs.Groups = append(cs.Groups, cg)
}

//获取牌组
func (cs *CardStack) GetCardGroup() *CardGroup {
	cg := new(CardGroup)
	for _, g := range cs.Groups {
		cg.AddCardGroup(g)
	}

	return cg
}

//用户牌堆
type UserCardStack struct {
	User  *gamelogic.GameUser
	Stack *CardStack
}

//创建用户牌堆
func NewUserCardStack(user *gamelogic.GameUser, stack *CardStack) *UserCardStack {
	ustack := new(UserCardStack)
	ustack.User = user
	ustack.Stack = stack

	return ustack
}

//所有的牌
func (ucs *UserCardStack) AllCards() *CardGroup {
	cg := NewCardGroup(nil)

	for _, ug := range ucs.Stack.Groups {
		cg.AddCardGroup(ug)
	}

	return cg
}

//游戏牌堆
type GameCardStack struct {
	UserStacks []*UserCardStack
}

//创建游戏牌型
func NewGameCardStack() *GameCardStack {
	gcs := new(GameCardStack)

	return gcs
}

//检查用户牌型
func (gcs *GameCardStack) CheckUserStack(userid int32) bool {
	for _, ust := range gcs.UserStacks {
		if ust.User.UserID == userid {
			return true
		}
	}

	return false
}

//添加用户牌型
func (gcs *GameCardStack) AddUserCardStack(ucs *UserCardStack) {
	if gcs.CheckUserStack(ucs.User.UserID) {
		return
	}

	gcs.UserStacks = append(gcs.UserStacks, ucs)
}
