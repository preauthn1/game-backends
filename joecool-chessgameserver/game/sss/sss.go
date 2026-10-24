//13水
package main

import (
	"config"
	"event"
	//"fmt"
	"gamelogic"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"sssgamelogic"
)

//牌型组合，可以全局只执行一次
/*
func CombInit() {
	for i := int(sssgamelogic.FCardTypeMin) + 1; i < int(sssgamelogic.FCardTypeMax); i++ {
		for j := int(sssgamelogic.NCardTypeMin) + int(i); j < int(sssgamelogic.NCardTypeMax); j++ {
			for k := j; k < int(sssgamelogic.NCardTypeMax); k++ {
				//ct := new(sssgamelogic.CardTemp)
				//ct.ct1 = sssgamelogic.FCardType(i)
				//ct.ct2 = sssgamelogic.NCardType(j)
				//ct.ct3 = sssgamelogic.NCardType(k)
				fmt.Printf("%d,%d,%d\r\n", i, j, k)
				//s.globalCardComb.PushBack(make([]))
			}
		}
	}
}*/

func main() {
	//CombInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return sssgamelogic.NewSSSGameLogic(t, tfManager, cfgs)
	}

	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return sssgamelogic.NewSSSRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	roomserver.StartGameLogicServer()
}
