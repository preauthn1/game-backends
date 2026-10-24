package poker

import (
	"fmt"
	"strconv"
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	PokerColor_HeiTao    int32 = 3 //黑桃
	PokerColor_HongTao   int32 = 2 //红桃
	PokerColor_MeiHua    int32 = 1 //梅花
	PokerColor_FangKuai  int32 = 0 //方块
	PokerColor_SmallKing int32 = 4 //小王
	PokerColor_BigKing   int32 = 5 //大王
	PokerColor_PiZi      int32 = 6 //皮子
	PokerColor_UnKnown   int32 = 7 //未知
)

const (
	PokerNumber_Two       int32 = 2  //2點
	PokerNumber_A         int32 = 14 //A点数
	PokerNumber_SmallKing int32 = 15 //小王点数
	PokerNumber_BigKing   int32 = 16 //大王点数
	PokerNumber_PiZi      int32 = 17 //皮子点数
)

//扑克牌
type PokerCard struct {
	Color      int32 //花色
	CardNumber int32 //点数
}

//创建扑克牌对象
func NewPokerCard(color int32, number int32) *PokerCard {
	pc := new(PokerCard)
	pc.Color = color
	pc.CardNumber = number

	return pc
}

//创建空的扑克牌
func NewEmptyPokerCard() *PokerCard {
	pc := new(PokerCard)
	pc.Color = PokerColor_UnKnown
	pc.CardNumber = 0

	return pc
}

//根据字符串创建扑克牌
func NewPokerCardByStr(str string, sep string) *PokerCard {
	str = util.TrimAllSpace(str)
	sep = util.TrimAllSpace(sep)

	sarr := strings.Split(str, sep)

	if len(sarr) != 2 {
		return nil
	}

	icolor, err := strconv.Atoi(sarr[0])
	if err != nil {
		return nil
	}

	inum, err := strconv.Atoi(sarr[1])
	if err != nil {
		return nil
	}

	return NewPokerCard(int32(icolor), int32(inum))

}

//根据网络数据创建扑克牌对象
func NewPokerCardFromNet(npk *netproto.PokerCard) *PokerCard {
	pk := new(PokerCard)
	pk.SetNetMessageData(npk)

	return pk
}

//是否大小王
func (pc *PokerCard) IsKing() bool {
	return pc.IsSmallKing() || pc.IsBigKing()
}

//是否小王
func (pc *PokerCard) IsSmallKing() bool {
	return pc.Color == PokerColor_SmallKing && pc.CardNumber == PokerNumber_SmallKing
}

//是否大王
func (pc *PokerCard) IsBigKing() bool {
	return pc.Color == PokerColor_BigKing && pc.CardNumber == PokerNumber_BigKing
}

//是否黑桃
func (pc *PokerCard) IsHeiTao() bool {
	return pc.Color == PokerColor_HeiTao
}

//是否红桃
func (pc *PokerCard) IsHongTao() bool {
	return pc.Color == PokerColor_HongTao
}

//是否梅花
func (pc *PokerCard) IsMeiHua() bool {
	return pc.Color == PokerColor_MeiHua
}

//是否方块
func (pc *PokerCard) IsFangKuai() bool {
	return pc.Color == PokerColor_FangKuai
}

//是否皮子
func (pc *PokerCard) IsPiZi() bool {
	return pc.Color == PokerColor_PiZi && pc.CardNumber == PokerNumber_PiZi
}

func (pc *PokerCard) IsA() bool {
	return pc.CardNumber == PokerNumber_A
}

//是否同一个牌
func (pc *PokerCard) IsEqual(other *PokerCard) bool {
	return pc.CardNumber == other.CardNumber && pc.Color == other.Color
}

//是否同一个牌点
func (pc *PokerCard) IsSameCardNumber(other *PokerCard) bool {
	return pc.CardNumber == other.CardNumber && !pc.IsKing() && !pc.IsPiZi()
}

//是否正确的牌
func (pc *PokerCard) IsValid() bool {
	if pc.Color < 0 || pc.Color > 6 {
		return false
	}

	if pc.CardNumber < 2 || pc.CardNumber > PokerNumber_PiZi {
		return false
	}

	return true
}

func (pc *PokerCard) String() string {
	color := ""
	//♠ ♥ ♣ ♦ ♤ ♡ ♧ ♢
	if pc.IsHeiTao() {
		color = "黑桃"
	} else if pc.IsHongTao() {
		color = "红桃"
	} else if pc.IsMeiHua() {
		color = "梅花"
	} else if pc.IsFangKuai() {
		color = "方块"
	} else if pc.IsSmallKing() {
		return "小王"
	} else if pc.IsBigKing() {
		return "大王"
	} else if pc.IsPiZi() {
		return "皮子"
	}

	if pc.CardNumber <= 10 {
		return fmt.Sprintf("%s%d", color, pc.CardNumber)
	} else {
		if pc.CardNumber == 11 {
			return fmt.Sprintf("%sJ", color)
		} else if pc.CardNumber == 12 {
			return fmt.Sprintf("%sQ", color)
		} else if pc.CardNumber == 13 {
			return fmt.Sprintf("%sK", color)
		} else if pc.CardNumber == 14 {
			return fmt.Sprintf("%sA", color)
		} else {
			return "invalid card"
		}
	}

}

//获取牌型网络数据对象
func (pk *PokerCard) GetNetMessageData() *netproto.PokerCard {
	npk := new(netproto.PokerCard)
	npk.Color = proto.Int32(pk.Color)
	npk.CardNumber = proto.Int32(pk.CardNumber)

	return npk
}

//设置网络牌型数据对象
func (pk *PokerCard) SetNetMessageData(npk *netproto.PokerCard) {
	pk.Color = npk.GetColor()
	pk.CardNumber = npk.GetCardNumber()
}
