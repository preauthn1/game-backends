package scmj

import (
	"mahjong"
	"mlog"
	"fmt"
)

//获取牌型倍率
func (m *MahjongSC) getCardRate(mapCardType map[int32]int32) (int32, string) {
	mlog.Debug("【[Joecool] ----- getCardRate() -----】")
	str := ""	// [Joecool] for debug
	rate := int32(0)
	strCardType := ""
	//清一色
	if mapCardType[mahjong.CARD_TYPE_QYS] != 0 {
		rate += m.rateSettleMent.QYS
		strCardType += "清一色,"
		str += fmt.Sprintf("清一色[%d] ", m.rateSettleMent.QYS)
	}
	//龙7对
	if mapCardType[mahjong.CARD_TYPE_LQD] != 0 {
		rate += m.rateSettleMent.LQD
		strCardType += "龙七对,"
		str += fmt.Sprintf("龙七对[%d] ", m.rateSettleMent.LQD)
	} else if mapCardType[mahjong.CARD_TYPE_QDZ] != 0 {
		rate += m.rateSettleMent.QDZ
		strCardType += "七对,"
		str += fmt.Sprintf("七对[%d] ", m.rateSettleMent.QDZ)
	}
	//大对子
	if mapCardType[mahjong.CARD_TYPE_DDZ] != 0 {
		rate += m.rateSettleMent.DDZ
		strCardType += "大对子,"
		str += fmt.Sprintf("大对子[%d] ", m.rateSettleMent.DDZ)
	}
	//金勾吊
	if mapCardType[mahjong.CARD_TYPE_JGD] != 0 {
		rate += m.rateSettleMent.JGD
		strCardType += "金勾吊,"
		str += fmt.Sprintf("金勾吊[%d] ", m.rateSettleMent.JGD)
	}
	//幺九
	if mapCardType[mahjong.CARD_TYPE_DYJ] != 0 {
		rate += m.rateSettleMent.DYJ
		strCardType += "幺九,"
		str += fmt.Sprintf("幺九[%d] ", m.rateSettleMent.DYJ)
	}
	if rate == 0 {
		strCardType = "屁胡,"
		rate = m.rateSettleMent.PH
		str += fmt.Sprintf("屁胡[%d] ", m.rateSettleMent.PH)
	}
	// rate = 2 ^ rate
	mlog.Debug("【[Joecool] %s】", str)
	mlog.Debug("【[Joecool] rate[%d], strCardType[%s]】", rate, strCardType)
	mlog.Debug("【[Joecool] ----- getCardRate() -----】")
	return rate, strCardType
}
