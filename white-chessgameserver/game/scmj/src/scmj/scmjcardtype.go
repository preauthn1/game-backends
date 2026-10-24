package scmj

import (
	"mahjong"
)

//获取牌型倍率
func (m *MahjongSC) getCardRate(mapCardType map[int32]int32) (int32, string) {
	rate := int32(0)
	strCardType := ""
	//清一色
	if mapCardType[mahjong.CARD_TYPE_QYS] != 0 {
		rate += m.rateSettleMent.QYS
		strCardType += "清一色,"
	}
	//龙7对
	if mapCardType[mahjong.CARD_TYPE_LQD] != 0 {
		rate += m.rateSettleMent.LQD
		strCardType += "龙七对,"
	} else if mapCardType[mahjong.CARD_TYPE_QDZ] != 0 {
		rate += m.rateSettleMent.QDZ
		strCardType += "七对,"
	}
	//大对子
	if mapCardType[mahjong.CARD_TYPE_DDZ] != 0 {
		rate += m.rateSettleMent.DDZ
		strCardType += "大对子,"
	}
	//金勾吊
	if mapCardType[mahjong.CARD_TYPE_JGD] != 0 {
		rate += m.rateSettleMent.JGD
		strCardType += "金勾吊,"
	}
	//幺九
	if mapCardType[mahjong.CARD_TYPE_DYJ] != 0 {
		rate += m.rateSettleMent.DYJ
		strCardType += "幺九,"
	}
	if rate == 0 {
		strCardType = "屁胡,"
		rate = m.rateSettleMent.PH
	}
	// rate = 2 ^ rate
	return rate, strCardType
}
