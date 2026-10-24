package qznngamelogic_rndz

import (
	"sort"
	"strconv"
)

func(qznn *QZNNGameLogic) getXianBetMoney() {
	qznn.userBetMoneyLimit = make([]int64, 0)
	zhuangTotal := int64(0) //庄家合計
	xianTotal := int64(0) //閒家合計
	xianMoneyList := make([]int64, 0) //閒家金額列表
	for _, ur := range qznn.AllGameUser() { //計算庄閒合計
		if qznn.zhuangInfo.userID == ur.UserID {
			zhuangTotal = ur.Score
		} else {
			xianTotal += ur.Score
			xianMoneyList = append(xianMoneyList, ur.Score)
		}
	}

	// A = 庄閒合計後, 比較小的那個值
	valueA := zhuangTotal
	if valueA > xianTotal {
		valueA = xianTotal
	}
	// B = A / 4 / 3 *權值 B
	valueB := float64(valueA) / 4 / 3

	// C = xianMinMoney / 3 *權值 C
	sort.Slice(xianMoneyList, func(i, j int) bool { return xianMoneyList[i] < xianMoneyList[j] })
	valueC := float64(xianMoneyList[0]) / 3

	// E = A|B 比較最後要使用的權值
	valueE := valueB
	if valueE > valueC {
		valueE = valueC
	}
	xbconfig := nnconfig.xianBetConfig
	basescore := int64(qznn.GetServerConfig().BaseScore)
	temp := make([]int64, 0)
	temp = append(temp, adjustBetMoneyLimit(int64(float64(basescore) * xbconfig.arg1)) )
	temp = append(temp, adjustBetMoneyLimit(int64(valueE * xbconfig.arg2)))
	temp = append(temp, adjustBetMoneyLimit(int64(valueE * xbconfig.arg3)))
	temp = append(temp, adjustBetMoneyLimit(int64(valueE * xbconfig.arg4)))

	//過濾重複的betmoney
	for _, v := range temp {
		exsit := false
		for _, t := range qznn.userBetMoneyLimit {
			if v == t {
				exsit = true
			}
		}
		if !exsit {
			qznn.userBetMoneyLimit = append(qznn.userBetMoneyLimit, v)
		}
	}

	for i, v := range qznn.userBetMoneyLimit {
		if v < xbconfig.m1 {
			qznn.userBetMoneyLimit[i] = xbconfig.m1
		}
		if v > xbconfig.m2 {
			qznn.userBetMoneyLimit[i] = xbconfig.m2
		}
	}

	return
}

//小於5則為5, 4位數的個位數捨去為0或5（ex. 2342 => 2430)
func adjustBetMoneyLimit(money int64) int64 {
	result := money
	if money < nnconfig.xianBetConfig.m1 {
		result = nnconfig.xianBetConfig.m1
	}

	if money > 500 {
		strmoney := strconv.Itoa(int(money))
		m1 := strconv.Itoa(int(money))[len(strmoney)-1:len(strmoney)]
		m, _ := strconv.Atoi(m1)
		if m < 5 {
			result = int64(money) - int64(m)
		}

		if m > 5 {
			m = m - 5
			result = int64(money) - int64(m)
		}
	}

	return result
}