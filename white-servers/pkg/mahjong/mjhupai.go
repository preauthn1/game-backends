package mahjong

import "example.invalid/ServerTeam/Servers/pkg/mlog"

const (
	CARD_TYPE_NORMAL int32 = 0  //无
	CARD_TYPE_PH     int32 = 1  //屁胡1/1
	CARD_TYPE_DDZ    int32 = 2  //大对子1/2
	CARD_TYPE_JGD    int32 = 3  //金勾吊3
	CARD_TYPE_QDZ    int32 = 4  //七对子2/4
	CARD_TYPE_LQD    int32 = 5  //龙七对4
	CARD_TYPE_QYS    int32 = 6  //清一色3/8
	CARD_TYPE_DYJ    int32 = 7  //带幺九3/8
	CARD_TYPE_TH     int32 = 8  //天胡6/64
	CARD_TYPE_DH     int32 = 9  //地胡6/64
	CARD_TYPE_HD     int32 = 10 //海底

)

//获取牌型
func (cg *CardGroup) GetCardType(pcg, gcg []int32, isDYJ int32) map[int32]int32 {
	mapCardType := make(map[int32]int32)
	//清一色
	if cg.IsSameColor() {
		color := cg.Cards[0] / 100
		bFlag := true
		for _, pcd := range pcg {
			if color != pcd/100 {
				bFlag = false
			}
		}
		for _, gcd := range gcg {
			if color != gcd/100 {
				bFlag = false
			}
		}
		if bFlag {
			mapCardType[CARD_TYPE_QYS] = 1
		}
	}
	//七对
	if cg.IsSevenPair() {
		mapCardType[CARD_TYPE_QDZ] = 1
	}
	//大对子
	if cg.IsBigThree() {
		mapCardType[CARD_TYPE_DDZ] = 1
	}
	//金勾吊
	if cg.IsJinGouDiao() {
		mapCardType[CARD_TYPE_JGD] = 1
		//mapCardType[CARD_TYPE_DDZ] = 0	// Remarked by Joecool Wang at 2019-06-05
		mapCardType[CARD_TYPE_DDZ] = 1
	}
	//龙7对
	if cg.IsLongSevenPair() {
		mapCardType[CARD_TYPE_LQD] = 1
	}
	//幺九
	if cg.IsOneNine(pcg, gcg) && isDYJ == 1 {
		mapCardType[CARD_TYPE_DYJ] = 1
	}

	//屁胡
	if cg.CanHuBase(0) && len(mapCardType) == 0 {
		mapCardType[CARD_TYPE_PH] = 1
	}

	return mapCardType
}

//是否可以胡牌,简单胡牌（a*AAA+b*ABC+AA）
//只检测手牌，碰杠吃不列入检测范围
func (cg *CardGroup) CanHuBase(cd int32) bool {
	if cg == nil {
		return false
	}
	cgtmp := cg.CopyCardGroup() //深度拷贝
	if cgtmp == nil {
		return false
	}
	cgtmp.AddCard(cd)
	if cgtmp.Len()%3 != 2 {
		return false
	}
	// cgtmp.Cards = append(cgtmp.Cards, cd)
	// mlog.Debug("检测胡牌，%s", cgtmp.String())
	mapCard := make(map[int32]int32, 0)
	mapCardOther := make(map[int32]int32, 0)
	//对序牌和非序牌分类
	for _, card := range cgtmp.Cards {
		if card/100 <= COLOR_WAN {
			mapCard[card/10]++
		} else {
			mapCardOther[card/10]++
		}
	}
	//提取序牌可以做将牌的麻将
	pair := make([]int32, 0)
	for card, count := range mapCard {
		if count >= 2 {
			pair = append(pair, card)
		}
	}
	//序牌没有对子，检测字牌等其他牌
	nPair := 0
	for card, count := range mapCardOther {
		if count == 3 {
			continue
		} else if count == 2 {
			nPair++
			pair = append(pair, card)
		} else {
			return false //如果有单牌，直接false
		}
	}
	if nPair > 1 {
		return false
	}
	//遍历所有将牌，检查是否可以组成胡牌
	if len(pair) > 0 {
		//检测序牌
		for _, card := range pair {
			mapTmp := make(map[int32]int32, 0)
			err := DeepCopy(&mapTmp, &mapCard)
			if err != nil {
				mlog.Error("%v", err)
				return false
			}
			//剔除所有能组成顺子的牌
			if card < 40 {
				mapTmp[card] -= 2
			}
			for i := int32(10); i < 40; i++ {
				mapTmp, _ = checkShunzi(mapTmp, i)
			}
			bCanhu := true
			for _, check := range mapTmp {
				if check == 3 {
					continue
				}
				if check != 0 {
					bCanhu = false
					break
				}
			}
			if bCanhu {
				return true
			}
		}
	}
	return false
}

//是否可以胡7对
func (cg *CardGroup) IsSevenPair() bool {
	if cg == nil {
		return false
	}
	if cg.Len() != 14 {
		return false
	}
	mapCard := make(map[int32]int32, 0)
	for _, card := range cg.Cards {
		mapCard[card/10]++
	}
	for _, count := range mapCard {
		// Modified by Joecool Wang at 2019-06-05
		// 若有四張牌是相同的，應該是龍七對，而非七對
		//if count != 2 && count != 4 {
		//
		//	return false
		//}
		if count != 2 {
			return false
		}
		// Modified by Joecool Wang at 2019-06-05 end
	}
	return true
}

//龙7对
func (cg *CardGroup) IsLongSevenPair() bool {
	if cg == nil {
		return false
	}
	if cg.Len() != 14 {
		return false
	}
	mapCard := make(map[int32]int32, 0)
	for _, card := range cg.Cards {
		mapCard[card/10]++
	}
	bLQD := false
	for _, count := range mapCard {
		if count != 2 && count != 4 {
			return false
		}
		if count == 4 {
			bLQD = true
		}
	}
	return bLQD
}

//是否清一色
func (cg *CardGroup) IsSameColor() bool {
	if cg == nil {
		return false
	}
	if cg.Len() < 1 {
		return false
	}
	color := cg.Cards[0] / 100
	for _, card := range cg.Cards {
		if card/100 != color {
			return false
		}
	}
	return true
}

//是否为大对子
func (cg *CardGroup) IsBigThree() bool {
	if cg == nil {
		return false
	}
	if cg.Len() < 2 {
		return false
	}
	mapCard := make(map[int32]int32, 0)
	for _, card := range cg.Cards {
		mapCard[card/10]++
	}
	pair := 0
	for _, count := range mapCard {
		if count == 2 {
			pair++
			continue
		}
		if count != 3 {
			return false
		}
	}

	if pair != 1 {
		return false
	}
	return true
}

//金勾吊,判定手牌为两张且为对子
func (cg *CardGroup) IsJinGouDiao() bool {
	if cg.Len() == 2 {
		if cg.Cards[0]/10 == cg.Cards[1]/10 {
			return true
		}
		return false
	}
	return false
}

//幺九(包括碰pcg,杠gcg)
func (cg *CardGroup) IsOneNine(pcg, gcg []int32) bool {
	//碰
	for _, cd := range pcg {
		//if GetCardColor(cd) > COLOR_WAN {	// Modified by Joecool Wang at 2019-06-05
		if GetCardColor(cd) < COLOR_TONG || GetCardColor(cd) > COLOR_WAN {
			return false
		}
		if GetCardNumber(cd) != 1 && GetCardNumber(cd) != 9 {
			return false
		}
	}
	//杠
	for _, cd := range gcg {
		//if GetCardColor(cd) > COLOR_WAN {	// Modified by Joecool Wang at 2019-06-05
		if GetCardColor(cd) < COLOR_TONG || GetCardColor(cd) > COLOR_WAN {
			return false
		}
		if GetCardNumber(cd) != 1 && GetCardNumber(cd) != 9 {
			return false
		}
	}
	//过滤不能和九组成顺子的牌
	for _, cd := range cg.Cards {
		//if GetCardColor(cd) > COLOR_WAN {	// Modified by Joecool Wang at 2019-06-05
		if GetCardColor(cd) < COLOR_TONG || GetCardColor(cd) > COLOR_WAN {
			return false
		}
		if GetCardNumber(cd) > 3 && GetCardNumber(cd) < 7 {
			return false
		}
	}
	mapCard := make(map[int32]int32, 0)
	for _, card := range cg.Cards {
		mapCard[GetCardValue(card)]++
	}

	mapTmp := make(map[int32]int32, 0)
	err := DeepCopy(&mapTmp, &mapCard)
	if err != nil {
		mlog.Error("%v", err)
		return false
	}
	for i := int32(10); i < 40; i++ {
		mapTmp, _ = deleteShunzi(mapTmp, i)
	}
	jp := make([]int32, 0)
	for card, count := range mapTmp {
		if GetNumberByValue(card) == 1 || GetNumberByValue(card) == 9 {
			if count == 3 || count == 0 {
				continue
			} else if count == 2 {
				jp = append(jp, card)
			} else if count == 4 {
				jp = append(jp, card)
				jp = append(jp, card)
			} else {
				return false
			}
		} else {
			if count != 0 {
				return false
			}
		}
	}
	if len(jp) == 1 || len(jp) == 7 {
		return true
	}
	return false
}

//是否可以听牌,7对单独加
func (cg *CardGroup) CanTingPai() bool {
	if cg == nil {
		return false
	}
	for i := int32(110); i < 900; {
		if i/10%10 == 0 {
			i += 10
			continue
		}
		cgcopy := new(CardGroup)
		for _, cd := range cg.Cards {
			cgcopy.AddCard(cd)
		}
		if cg.CanHuBase(i) {
			return true
		}
		i += 10
	}
	return false
}

//是否可以听7对
func (cg *CardGroup) CanTingSevenPair() bool {
	if cg == nil {
		return false
	}
	if cg.Len() < 13 {
		return false
	}
	mapCard := make(map[int32]int32, 0)
	for _, card := range cg.Cards {
		mapCard[card/10]++
	}
	Sum := 0
	for _, count := range mapCard {
		if count == 2 {
			Sum += 1
		} else if count == 2 {
			Sum += 2
		}
	}
	if Sum < 6 {
		return false
	}
	return true
}

//计算不能组成顺子的剩余牌需要几张癞子可以补齐
func NeedLaizi(mapSurplus map[int32]int32) int32 {
	//计算需要几张癞子
	sum := int32(0)
	for i := int32(1); i < 40; i++ {
		if mapSurplus[i] <= 0 {
			continue
		}
		if mapSurplus[i] == 2 {
			sum += 1
			continue
		}
		if mapSurplus[i] == 1 {
			mapSurplus[i] -= 1
			if (i+1)%10 == 0 {
				sum += 2
				continue
			}
			if mapSurplus[i+1] > 0 {
				mapSurplus[i+1] -= 1
				if (i+2)%10 == 0 {
					sum += 1
					continue
				}
				if mapSurplus[i+2] > 0 {
					mapSurplus[i+2] -= 1
				} else {
					sum += 1
				}
			} else {
				sum += 1
				if mapSurplus[i+2] > 0 {
					mapSurplus[i+2] -= 1
				} else {
					sum += 1
				}
			}
		}
	}
	return sum
}

//计算组成顺子后 ，去处一坎，剩余牌
func calSurPlusMap(mapTmp map[int32]int32) map[int32]int32 {
	mapSurplus := make(map[int32]int32, 0)
	for card, count := range mapTmp {
		if count == 3 {
			continue
		}
		if count == 4 {
			mapSurplus[card] = 1
			continue
		}
		if count != 0 {
			mapSurplus[card] = count
		}
	}
	return mapSurplus
}

//检测顺子并剔除
func checkShunzi(cd map[int32]int32, i int32) (map[int32]int32, int32) {
	if cd[i] >= 3 {
		cd[i] -= 3
	}
	if cd[i] > 0 && cd[i+1] > 0 && cd[i+2] > 0 {
		cd[i] -= 1
		cd[i+1] -= 1
		cd[i+2] -= 1
		cd, i = checkShunzi(cd, i)
		return cd, i
	}
	return cd, i
}

// 去掉顺子
func deleteShunzi(cd map[int32]int32, i int32) (map[int32]int32, int32) {
	if cd[i] > 0 && cd[i+1] > 0 && cd[i+2] > 0 {
		cd[i] -= 1
		cd[i+1] -= 1
		cd[i+2] -= 1
		cd, i = checkShunzi(cd, i)
		return cd, i
	}
	return cd, i
}
