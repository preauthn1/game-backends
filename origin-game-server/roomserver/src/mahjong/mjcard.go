package mahjong

import (
	"fmt"
)

// Card int32=100*Color+10*Number+index(第几副牌)
const (
	COLOR_TONG  int32 = 1 //筒子
	COLOR_TIAO  int32 = 2 //条子
	COLOR_WAN   int32 = 3 //万子
	COLOR_FENG  int32 = 4 //风牌
	COLOR_JIAN  int32 = 5 //箭牌
	COLOR_HUA   int32 = 6 //花牌
	COLOR_BAIDA int32 = 7 //百搭牌
	COLOR_QITA  int32 = 8 //其他
)

const (
	//风牌
	NUMBER_EAST  int32 = 1 //东
	NUMBER_WEST  int32 = 2 //西
	NUMBER_SOUTH int32 = 3 //南
	NUMBER_NORTH int32 = 4 //北
	//箭牌
	NUMBER_ZHONG int32 = 1 //中
	NUMBER_FA    int32 = 2 //发
	NUMBER_BAI   int32 = 3 //白
	//花牌
	NUMBER_CHUN int32 = 1 //春
	NUMBER_XIA  int32 = 2 //夏
	NUMBER_QIU  int32 = 3 //秋
	NUMBER_DONG int32 = 4 //冬
	NUMBER_MEI  int32 = 5 //梅
	NUMBER_LAN  int32 = 6 //兰
	NUMBER_ZHU  int32 = 7 //竹
	NUMBER_JU   int32 = 8 //菊
	//百搭
	NUMBER_CAISHEN   int32 = 1 //财神
	NUMBER_MAO       int32 = 2 //猫
	NUMBER_LAOSHU    int32 = 3 //老鼠
	NUMBER_JUBAOPENG int32 = 4 //聚宝盆
	//序牌1~9
)

func CardString(cd int32) string {
	color := ""

	if cd/100 == COLOR_TONG {
		color = "筒"
		return fmt.Sprintf("%d%s", cd%100/10, color)
	} else if cd/100 == COLOR_TIAO {
		color = "条"
		return fmt.Sprintf("%d%s", cd%100/10, color)
	} else if cd/100 == COLOR_WAN {
		color = "万"
		return fmt.Sprintf("%d%s", cd%100/10, color)
	} else if cd/100 == COLOR_FENG {
		if cd%100/10 == NUMBER_EAST {
			return "东"
		} else if cd%100/10 == NUMBER_WEST {
			return "西"
		} else if cd%100/10 == NUMBER_SOUTH {
			return "南"
		} else if cd%100/10 == NUMBER_NORTH {
			return "北"
		}
	} else if cd/100 == COLOR_JIAN {
		if cd%100/10 == NUMBER_ZHONG {
			return "红中"
		} else if cd%100/10 == NUMBER_FA {
			return "发财"
		} else if cd%100/10 == NUMBER_BAI {
			return "白板"
		}
	} else if cd/100 == COLOR_HUA {
		if cd%100/10 == NUMBER_CHUN {
			return "春"
		} else if cd%100/10 == NUMBER_XIA {
			return "夏"
		} else if cd%100/10 == NUMBER_QIU {
			return "秋"
		} else if cd%100/10 == NUMBER_DONG {
			return "冬"
		} else if cd%100/10 == NUMBER_MEI {
			return "梅"
		} else if cd%100/10 == NUMBER_LAN {
			return "兰"
		} else if cd%100/10 == NUMBER_ZHU {
			return "竹"
		} else if cd%100/10 == NUMBER_JU {
			return "菊"
		}
	} else if cd/100 == COLOR_BAIDA {
		if cd%100/10 == NUMBER_CAISHEN {
			return "财神"
		} else if cd%100/10 == NUMBER_MAO {
			return "猫"
		} else if cd%100/10 == NUMBER_LAOSHU {
			return "老鼠"
		} else if cd%100/10 == NUMBER_JUBAOPENG {
			return "聚宝盆"
		}
	}
	return "未知牌"

}

func ColorString(color int32) string {
	if color == 1 {
		return "筒子"
	} else if color == 2 {
		return "条子"
	} else if color == 3 {
		return "万子"
	} else if color == 4 {
		return "风牌"
	} else if color == 5 {
		return "箭牌"
	} else if color == 6 {
		return "花牌"
	} else if color == 7 {
		return "百搭牌"
	} else {
		return "未知颜色"
	}
	return "未知颜色"
}

// 根据牌值 获取牌索引
func GetCardIndex(cd int32) int32 {
	return cd % 100 % 10
}

// 根据牌值获取牌点数
func GetCardNumber(cd int32) int32 {
	return cd / 10 % 10
}

// 根据牌值获取牌花色
func GetCardColor(cd int32) int32 {
	return cd / 100
}

// 根据牌值获取牌有效值，点数和花色
func GetCardValue(cd int32) int32 {
	return cd / 10
}

// 根据牌有效值 获取点数
func GetNumberByValue(value int32) int32 {
	return value % 10
}

// 根据牌有效值 获取花色
func GetColorByValue(value int32) int32 {
	return value / 10
}
