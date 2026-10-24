package lhjlogic

import (
	"conf"
	"config"
	"event"
	"fixtures"
	"gamelogic"
	"mlog"
	"msg"
	"mylog"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"testing"
)

func confInit() {
	conf.CLogLevel = 3
	conf.TimerFastMode = 10 //40
	conf.TestMode = true
	mylog.InitDatabaseQueue("", 0, 0, 0, mlog.NewDefaultLogger())
	mlog.GetCLogger().Enable(false)
}

//流程整合測試共用初始化
func IntegrationTestInit(endChannel chan bool, t *testing.T) (*fixtures.TestGameRoomServer, *LhjLogic, chan bool, []*gamelogic.GameUser) {
	tgrs := fixtures.NewTestGameRoomServer()
	glc := tgrs.GameLogicManager.GetGameLogic(0).(*LhjLogic)
	glc.RegisterAfterGameEndFunc(func() {
		//mlog.Error("無法endChannel")
		//endChannel <- true
	})
	glc.UpdateUserScoreFunc = func(userid int32, score int64, taxamount int64, coupon int64) {
	}

	glc.SendFishNotifyFunc = func(m *msg.Message) {

	}

	glc.OnGameStart(func() {
	})


	return tgrs, glc, endChannel, nil
}

//获取房间配置文件CSV
func TestGetRoomTable(t *testing.T) {
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLHJLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewLhjRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	tgrs := fixtures.NewTestGameRoomServer()
	tgrs.GameLogicManager.GetGameLogic(0)

}

//TODO 設定輪軸並開獎驗證賠付線
func StopTestPlay1(t *testing.T) {
	endChannel := make(chan bool)
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLHJLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewLhjRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	tgrs := fixtures.NewTestGameRoomServer()
	tgrs.GameLogicManager.GetGameLogic(0)

	tgrs, glc, endChannel, _ := IntegrationTestInit(endChannel, t)

	users := fixtures.GetGameUser()

	glc.getReelSetting()
	glc.startSpin(users[0])

	//測試資料
	//result := make([]*netproto.LHJ_ReelSetting, 0)
	//for i := 1; i <= 5; i++ {
	//	reelSetting := new(netproto.LHJ_ReelSetting)
	//	reelSetting.RealID = proto.Int32(int32(i))
	//	for s := 1; s <= 10; s++ {
	//		symbolGroup := new(netproto.LHJ_SymbolGroup)
	//		symbolId := s + i
	//		if s+i > 6 {
	//			rand := util.RndInt(1, 5)
	//			symbolId += rand
	//		}
	//		if symbolId > 10 {
	//			symbolId = 10
	//		}
	//		symbolGroup.SymbolID = proto.Int32(int32(symbolId))
	//		symbolGroup.SynbolName = proto.String(fmt.Sprintf("p%v", symbolId))
	//		reelSetting.SymbolGroup = append(reelSetting.SymbolGroup, symbolGroup)
	//	}
	//	result = append(result, reelSetting)
	//	mlog.System("[weihao] 測試資料 reelSetting : %+v ", reelSetting)
	//}
	//
	//glc.reelsetting = result

	//kaiJiangresult := make([]int32, 0)
	//kaiJiangresult = []int32{
	//	glc.reelsetting[0].SymbolGroup[2].GetSymbolID(),
	//	glc.reelsetting[1].SymbolGroup[1].GetSymbolID(),
	//	glc.reelsetting[2].SymbolGroup[0].GetSymbolID(),
	//	glc.reelsetting[3].SymbolGroup[8].GetSymbolID(),
	//	glc.reelsetting[4].SymbolGroup[3].GetSymbolID()}
	//glc.kaiJiang = kaiJiangresult

	//msg := glc.getLHJResultMsg()
	//actual := msg.MsgData.(*netproto.LHJ_GameResultReel)

	//expected := []int32{4,4,4,10,10}
	//if !IntArrayEquals(actual.GetSymbolID(), expected) {
	//	t.Errorf("GetSymbolID : %v", actual.GetSymbolID())
	//}
	//
	//
	//if actual.GetPayLineGroup()[0].PayLineDetail[0].GetReelID() != 1 {
	//	t.Errorf("開獎賠付線ReelId錯誤 : %v", actual.GetPayLineGroup()[0].PayLineDetail[0].GetReelID())
	//}
	//if actual.GetPayLineGroup()[0].PayLineDetail[0].GetSymbolID() != 4 {
	//	t.Errorf("開獎賠付線SymbolId錯誤 : %v", actual.GetPayLineGroup()[0].PayLineDetail[0].GetReelID())
	//}
	//
	//if actual.GetPayLineGroup()[1].PayLineDetail[0].GetReelID() != 2 {
	//	t.Errorf("開獎賠付線ReelId錯誤 : %v", actual.GetPayLineGroup()[0].PayLineDetail[1].GetReelID())
	//}
	//if actual.GetPayLineGroup()[1].PayLineDetail[0].GetSymbolID() != 4 {
	//	t.Errorf("開獎賠付線SymbolId錯誤 : %v", actual.GetPayLineGroup()[0].PayLineDetail[0].GetReelID())
	//}
	//
	//if actual.GetPayLineGroup()[2].PayLineDetail[0].GetReelID() != 3 {
	//	t.Errorf("開獎賠付線ReelId錯誤 : %v", actual.GetPayLineGroup()[0].PayLineDetail[2].GetReelID())
	//}
	//if actual.GetPayLineGroup()[2].PayLineDetail[0].GetSymbolID() != 4 {
	//	t.Errorf("開獎賠付線SymbolId錯誤 : %v", actual.GetPayLineGroup()[0].PayLineDetail[0].GetReelID())
	//}

	//mlog.System("end")
	//select {
	//case _ = <-endChannel:
	//	mlog.NewDefaultLogger().Info("lhj game end")
	//}
}

//TODO 讀取測試案例
//func NewTestFilePoker(testFilePoker string) *FilePoker {
//	_, filename, _, _ := runtime.Caller(1)
//	configDir := path.Dir(filename) + "/../../testfixpoker"
//	cfgs := new(config.ConfigGroup)
//	cfgs.LoadConfig(configDir, 0)
//	pokerFileName := testFilePoker
//	fp := innerNewFileLhj(cfgs, pokerFileName)
//
//	return fp
//}

func IntArrayEquals(a []int32, b []int32) bool {
	if len(a) != len(b) {
		return false
	}
	for i, v := range a {
		if v != b[i] {
			return false
		}
	}
	return true
}
