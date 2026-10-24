package ddzgamelogic

import (
	"conf"
	"mlog"
	"poker"
	"testing"
	"roomserver"
	"gamelogic"
	"event"
	"config"
	"robot/robotcomm"
	"fixtures"
	"robot/robotlogic"
)

func TestNewCombination(t *testing.T) {
	conf.CLogLevel = 3
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewDDZGameLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewDDZRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}


	glm, _ := fixtures.StartGameLogicByTest()
	glc := glm.GetGameLogic(0).(*DDZGameLogic)
	var p poker.Poker
	p = poker.NewTestFilePoker("NewCombination")
	p = poker.NewFixedFilePoker(p.(*poker.FilePoker))
	fp := p.(*poker.FixedPoker)
	userLen := fp.UserCardLen()
	for i := 0; i < userLen; i++ {
		userId := int32(i)
		up := fp.GetUserCards(userId)
		if up == nil {
			mlog.Error("系统错误robotOutCards，获取玩家牌型失败")
		}
		dc := NewDefineCard(glc)
		uc := dc.Combination(up)
		mlog.Debug("============= %v", uc)

	}
}
