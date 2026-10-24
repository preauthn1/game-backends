package poker

import (
	"config"
	"gamelogic"
	"mlog"
	"path"
	"runtime"
	"strconv"
	"strings"
	"util"
)

const (
	POKERFILENAME    = "poker"
	POKERSECTIONNAME = "userpoker"
)

//基于文件配置的牌型
//用于特定牌型的发牌器
type FilePoker struct {
	cfg       *config.Config
	userCards map[int32]*CardGroup
}

//创建发牌器对象
func NewFilePoker(cfgs *config.ConfigGroup, urs []*gamelogic.GameUser) *FilePoker {
	pokerFileName := POKERFILENAME
	fp := innerNewFilePoker(cfgs, pokerFileName)

	return fp
}

func NewTestFilePoker(testFilePoker string) *FilePoker {
	_, filename, _, _ := runtime.Caller(1)
	configDir := path.Dir(filename) + "/../../testfixpoker"
	cfgs := new(config.ConfigGroup)
	cfgs.LoadConfig(configDir, 0)
	pokerFileName := testFilePoker
	fp := innerNewFilePoker(cfgs, pokerFileName)

	return fp
}

func innerNewFilePoker(cfgs *config.ConfigGroup, pokerFileName string) *FilePoker {
	pcfg := cfgs.Configs[pokerFileName]
	fp := new(FilePoker)
	fp.cfg = pcfg
	fp.userCards = make(map[int32]*CardGroup)
	sec := pcfg.GetConfigSection("userpoker")
	for uidxs, pstr := range sec {
		if uidxs == "" || pstr == "" {
			continue
		}

		uidx, err := strconv.Atoi(uidxs)
		if err != nil {
			mlog.Error("解析用户索引错误%v,%s", err, uidxs)
			continue
		}
		//用户转换
		var uid int32 = 0

		uid = int32(uidx)

		//创建用户牌型
		cg := NewCardGroup(nil)
		fp.userCards[uid] = cg
		//拆解牌型, 雙管線後面有特別的用途, 可以用於測試驗證牌型的結果
		tmpArr := strings.Split(pstr, "||")
		//解析牌型
		sarr := strings.Split(tmpArr[0], "|")

		for _, s := range sarr {
			if util.TrimAllSpace(s) == "" {
				continue
			}

			pk := NewPokerCardByStr(s, ",")
			if pk == nil {
				mlog.Error("解析牌型错误,字符串%s格式不正确.%s", s, pstr)
				continue
			}
			fp.userCards[uid].AddCard(pk)
		}
	}
	return fp
}

func (fp *FilePoker) GetUserCards() map[int32]*CardGroup {
	return fp.userCards
}

func (fp *FilePoker) GetConfig() *config.Config {
	return fp.cfg
}

func (fp *FilePoker) Shuffle() {
}

func (fp *FilePoker) UserCardsLen(uid int32) int {
	return fp.userCards[uid].Len()
}

func (fp *FilePoker) ShuffleMore(times int) {
	fp.Shuffle()
}

func (fp *FilePoker) Deal(userid int32, cn int) *CardGroup {
	cuid := RestCardsUserID

	if _, ok := fp.userCards[userid]; ok {
		cuid = userid
	}

	cards := fp.userCards[cuid].Cards[:cn]
	fp.userCards[userid].Cards = fp.userCards[cuid].Cards[cn:]

	return NewCardGroup(cards)
}

//换牌
func (fp *FilePoker) ExChange(userid int32, otheruserid int32) {

}

func (fp *FilePoker) String() string {
	return fp.cfg.String()
}


func (fp *FilePoker) TakeCardNumber(userid int32, cardNumber int32) (bool, PokerCard) {
	for k, v := range fp.userCards[userid].Cards {
		if v.CardNumber == cardNumber {
			fp.userCards[userid].Cards = RemoveIndex(fp.userCards[userid].Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

//指定取非存在的號碼
func (fp *FilePoker) TakeDiffCardNumber(userid int32, diffNums []int32) (bool, PokerCard) {
	for k, v := range fp.userCards[userid].Cards {
		right := true
		for _, diffnum := range diffNums {
			if v.CardNumber == diffnum {
				right = false
			}
		}
		if right {
			fp.userCards[userid].Cards = RemoveIndex(fp.userCards[userid].Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

func (fp *FilePoker) TakeCardColor(userid int32, color int32) (bool, PokerCard) {
	for k, v := range fp.userCards[userid].Cards {
		if v.Color == color {
			fp.userCards[userid].Cards = RemoveIndex(fp.userCards[userid].Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}

func (fp *FilePoker) TakeCardNumberAndColor(userid int32, cardNumber int32, color int32) (bool, PokerCard) {
	for k, v := range fp.userCards[userid].Cards {
		if v.CardNumber == cardNumber && v.Color == color {
			fp.userCards[userid].Cards = RemoveIndex(fp.userCards[userid].Cards, k)
			return true, *v
		}
	}

	return false, PokerCard{}
}


func (fp *FilePoker) AddCardGroup(userid int32, cg *CardGroup) {
	if len(cg.Cards) <= int(0) {
		return
	}

	fp.userCards[userid].Cards = append(fp.userCards[userid].Cards, cg.Cards...)
}

func (fp *FilePoker) AddCard(userid int32, card *PokerCard) {
	if card == nil {
		return
	}
	fp.userCards[userid].Cards = append(fp.userCards[userid].Cards, card)
}
