package lhjlogic

import (
	"DataCfgLhj"
	"config"
	"event"
	"fixtures"
	"fmt"
	"gamelogic"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"msg"
	"netproto"
	"path"
	"robot/robotcomm"
	"robot/robotlogic"
	"roomserver"
	"runtime"
	"strconv"
	"strings"
	"testing"
	"time"
	"sync"
)

//驗證賠付線
type validate struct {
	caseName      string
	validlineid   []int32 //驗證賠付線
	vaildmoney    int32
	score         int32
	vaildfreespin int32 //驗證free spin次數
}

//測試案例由QA撰寫: 驗證賠付線
func TestPayLine(t *testing.T) {
	tcase, tverify := NewTestFileLhj("payline") //牌型定義在 hhdz/testcardtype1.dat
	for k, _ := range tcase {
		validatePayLine(t, tcase[k], tverify[k])
	}

	//select{}
}

//測試案例由QA撰寫: 驗證賠付金額
func TestPayOut(t *testing.T) {
	fileList := []string{"Reduce1"}

	//for _, fileName := range fileList {
	//	tcase, tverify := NewTestFileLhj(fileName) //牌型定義在 hhdz/testcardtype1.dat
	//
	//	for k, _ := range tcase {
	//		validatePayOut(t, tcase[k], tverify[k])
	//	}
	//}

	var wg sync.WaitGroup
	for _, fileName := range fileList {
		tcase, tverify := NewTestFileLhj(fileName) //牌型定義在 hhdz/testcardtype1.dat
		//var wg sync.WaitGroup
		for k, _ := range tcase {
			wg.Add(1)
			go func(innerwg sync.WaitGroup, k int) {
				validatePayOut(t, tcase[k], tverify[k])
				wg.Done()
			}(wg, k)
		}
		//wg.Wait()
	}
	wg.Wait()
}

//測試案例由QA撰寫: jackpot
func TestJackpot(t *testing.T) {
	tcase, tverify := NewTestFileLhj("jackpot") //牌型定義在 hhdz/testcardtype1.dat
	for k, _ := range tcase {
		validatePayOut(t, tcase[k], tverify[k])
	}
}

//驗證賠付金額
func validatePayOut(t *testing.T, tcase *LhjLogic, tverify *validate) {
	endChannel := make(chan bool, 1)
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLHJLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewLhjRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	users := fixtures.GetGameUser()

	tgrs := fixtures.NewTestGameRoomServer()

	tgrs.GameLogicManager.GetGameLogic(0)
	tgrs, glc, endChannel, _ := IntegrationTestInit(endChannel, t)

	testDone := false

	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
		//mlog.System("sclass[%v]", m.SClassID)
		if m.SClassID == int32(netproto.LHJ_GameMessageClassID_LHJGameResult) {
			payLineMsg := m.MsgData.(*netproto.LHJ_GameResultReel)

			//if payLineMsg.BetRet.GetRet() < 0 {
			//	t.Errorf("[%v] 下注失敗Ret[%v], 請確認玩家身上金額", tverify.caseName, payLineMsg.BetRet.GetRet())
			//}

			if len(payLineMsg.PayLineGroup) == 0 {
				t.Errorf("中獎線為0, [%v]", tverify.caseName)
				return
			}
			//mlog.System("[weihao] 測試驗證 payLineMsg [%+v]", payLineMsg.PayLineGroup)
			for _, v := range payLineMsg.PayLineGroup {
				fail := true
				for _, vaild := range tverify.validlineid {
					//mlog.System("[weihao] GetPayLineID[%v] 驗證資料lineid[%v]", v.GetPayLineID(), vaild)
					if v.GetPayLineID() == vaild {
						fail = false
						break
					}
				}
				if fail {
					t.Errorf("[%v] 賠付線錯誤, 程式payline[%v]找不到驗證資料payline[%v]", tverify.caseName, v.GetPayLineID(), v.GetPayLineID())
				}
			}

			for _, vaild := range tverify.validlineid {
				fail := true
				for _, v := range payLineMsg.PayLineGroup {
					if v.GetPayLineID() == vaild {
						fail = false
						break
					}
				}
				if fail {
					t.Errorf("[%v] 賠付線錯誤, 找不到驗證資料payline[%v]", tverify.caseName, vaild)
				}
			}

			if payLineMsg.GetWinMoney() != tverify.vaildmoney {
				t.Errorf("[%v] 驗證結算金額錯誤, 期望[%v] 實際[%v]", tverify.caseName, tverify.vaildmoney, payLineMsg.GetWinMoney())
			}

			if glc.freeSpins != tverify.vaildfreespin {
				t.Errorf("[%v] 驗證free spin錯誤, 期望[%v] 實際[%v]", tverify.caseName, tverify.vaildfreespin, glc.freeSpins)
			}

			testDone = true
		}
		if m.SClassID == int32(netproto.LHJ_GameMessageClassID_LHJStateID) {
			//mlog.System(" LHJ_GameMessageClassID_LHJStateID msg[%v]", m.MsgData)
			bytedata, _ := m.MsgData.(*netproto.LHJ_GameStatus)

			if bytedata.GetState() == KaiJiang {
				glc.kaiJiang = tcase.kaiJiang
				return
			}
			//resultMsg := glc.getLHJResultMsg()
		}
	}

	glc.RegisterBeforeGameStartFunc(func() {
		glc.reelRow = tcase.reelRow
		glc.kaiJiang = tcase.kaiJiang
		//glc.slotPatternTable = tcase.slotPatternTable
		glc.normalSymbolDistTable = tcase.normalSymbolDistTable

		glc.getReelSetting()
	})

	users[1].Score = int64(tverify.score)
	//robots[0].UserState.OnUserQueue(robots[0], tgrs.Room)
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)

	//安排用戶入座
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.setState(Bet)
	setJackpotPool(glc)

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.LHJ_GameMessageClassID_LHJGameStartID)
	pdata := new(netproto.LHJ_StartBet)
	pdata.Money = proto.Int32(tcase.userdata[0].userbetmoney) //TODO betmoney參數
	pmsg.MsgData, _ = proto.Marshal(pdata)
	glc.GameMessage(users[1], pmsg)

	if !testDone {
		t.Errorf("未測試完整")
	}

	glc.gameEnd()

	//select {
	//case _ = <-endChannel:
	//	mlog.NewDefaultLogger().Info("lhj game end")
	//}
}

func setJackpotPool(lh *LhjLogic) {
	jackpotObj := new(JackpotPool)
	jackpotObj.jackpotId = 1
	jackpotObj.spinStack = 0
	jackpotObj.syspool = 0
	jackpotObj.jpAmountOut = 0
	lh.jackpotList = append(lh.jackpotList, jackpotObj)
}

//QA測試案例
func validatePayLine(t *testing.T, tcase *LhjLogic, tverify *validate) {
	endChannel := make(chan bool)
	confInit()
	roomserver.GameLogicCreator = func(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) gamelogic.GameLogic {
		return NewLHJLogic(t, tfManager, cfgs)
	}
	roomserver.RobotLogicCreator = func(table *gamelogic.Table, tfManager *event.TimerFuncManager, robotCfg *robotcomm.RobotConfig, configs *config.ConfigGroup, glc gamelogic.GameLogic, evtChannel *event.EventDispatcher) robotlogic.RobotLogic {
		return NewLhjRobotLogic(table, tfManager, robotCfg, configs, glc, evtChannel)
	}

	users := fixtures.GetGameUser()
	tgrs := fixtures.NewTestGameRoomServer()
	tgrs.GameLogicManager.GetGameLogic(0)
	tgrs, glc, endChannel, _ := IntegrationTestInit(endChannel, t)

	glc.SendMessgeFunc = func(userid int32, m *msg.Message) {
		//mlog.System("sclass[%v]", m.SClassID)
		if m.SClassID == int32(netproto.LHJ_GameMessageClassID_LHJGameResult) {
			glc.gameEnd()
		}
		if m.SClassID == int32(netproto.LHJ_GameMessageClassID_LHJStateID) {
			ud := new(lhjuserdata)
			ud.user = users[1]
			ud.userbetmoney = 10
			glc.userdata = append(glc.userdata, ud)

			mlog.System(" LHJ_GameMessageClassID_LHJStateID msg[%v]", m.MsgData)
			resultMsg := glc.getLHJResultMsg()
			payLineMsg := resultMsg.MsgData.(*netproto.LHJ_GameResultReel)
			//mlog.System("[weihao] 測試驗證 payLineMsg [%+v]", payLineMsg.PayLineGroup)
			for _, v := range payLineMsg.PayLineGroup {
				fail := true
				for _, vaild := range tverify.validlineid {
					if v.GetPayLineID() == vaild {
						fail = false
						break
					}
				}
				if fail {
					t.Errorf("[%v] 賠付線錯誤, 程式payline[%v]找不到驗證資料payline[%v]", tverify.caseName, v.GetPayLineID(), v.GetPayLineID())
				}
			}

			for _, vaild := range tverify.validlineid {
				fail := true
				for _, v := range payLineMsg.PayLineGroup {
					if v.GetPayLineID() == vaild {
						fail = false
						break
					}
				}
				if fail {
					t.Errorf("[%v] 賠付線錯誤, 程式payline[%v]找不到驗證資料payline[%v]", tverify.caseName, vaild, vaild)
				}
			}

		}
	}

	glc.RegisterBeforeGameStartFunc(func() {
		glc.reelRow = tcase.reelRow
		glc.kaiJiang = tcase.kaiJiang
		glc.slotPatternTable = tcase.slotPatternTable
		glc.normalSymbolDistTable = tcase.normalSymbolDistTable

		glc.getReelSetting()

	})

	users[1].Score = 1000
	//robots[0].UserState.OnUserQueue(robots[0], tgrs.Room)
	users[1].UserState.OnUserQueue(users[1], tgrs.Room)

	//安排用戶入座
	glc.UserEnter(users[1], glc.GetTable().TableNo, glc.GetTable().GetFreeSeat().SeatNo)
	glc.setState(Bet)

}

func NewTestFileLhj(testFilePoker string) ([]*LhjLogic, []*validate) {
	start := time.Now()
	_, filename, _, _ := runtime.Caller(1)
	configDir := path.Dir(filename) + "/../testsymbol"
	cfgs := new(config.ConfigGroup)
	cfgs.LoadConfig(configDir, 0)
	pokerFileName := testFilePoker
	lhj, verify := innerNewFileLhj(cfgs, pokerFileName)

	elapsed := time.Since(start)
	if elapsed.Seconds() > 1 {
		fmt.Printf("讀檔時間:%v pokerFileName[%v]\n", elapsed.Seconds(), pokerFileName)
	}

	return lhj, verify
}

func innerNewFileLhj(cfgs *config.ConfigGroup, pokerFileName string) ([]*LhjLogic, []*validate) {
	pcfg := cfgs.Configs[pokerFileName]
	lhjArr := make([]*LhjLogic, 0)
	validArr := make([]*validate, 0)

	totalcase := pcfg.GetConfigInt("csize", "totalcase")

	for i := 1; i <= totalcase; i++ {
		lhjObj := new(LhjLogic)
		validObj := new(validate)
		validObj.caseName = fmt.Sprintf("case%v", i)
		validObj.vaildmoney = pcfg.GetConfigInt32(fmt.Sprintf("case%v", i), "vaildmoney")
		validObj.score = pcfg.GetConfigInt32(fmt.Sprintf("case%v", i), "score")
		validObj.vaildfreespin = pcfg.GetConfigInt32(fmt.Sprintf("case%v", i), "vaildfreespin")

		lhjObj.slotPatternTable = new(DataCfgLhj.SlotPattern)
		lhjObj.normalSymbolDistTable = make([]*DataCfgLhj.SlotSymbolDistribution, 0)

		lhjObj.reelRow = pcfg.GetConfigInt32(fmt.Sprintf("case%v", i), "rownum")
		lhjObj.slotPatternTable.ReelNum = proto.Uint32(uint32(pcfg.GetConfigInt32(fmt.Sprintf("case%v", i), "reelnum")))

		lhjObj.userdata = make([]*lhjuserdata, 0)
		ud := new(lhjuserdata)
		ud.userbetmoney = pcfg.GetConfigInt32(fmt.Sprintf("case%v", i), "betmoney")
		lhjObj.userdata = append(lhjObj.userdata, ud)

		reelsettingstr := pcfg.GetConfigValue(fmt.Sprintf("case%v", i), "reelsetting")
		reelsettingarr := strings.Split(reelsettingstr, "|")
		for rk, r := range reelsettingarr {
			symbolstr := strings.Split(r, ",")
			for k, s := range symbolstr {
				normal := new(DataCfgLhj.SlotSymbolDistribution)
				normal.Id = proto.Uint32(uint32(k))
				normal.GroupId = proto.Uint32(1)
				normal.ReelNum = proto.Uint32(uint32(rk + 1))
				normal.Symbol = proto.String(s)
				normal.Proportion = proto.Uint32(1)
				normal.Order = proto.Uint32(uint32(k+1))
				lhjObj.normalSymbolDistTable = append(lhjObj.normalSymbolDistTable, normal)
			}
		}

		kaiJiangstr := pcfg.GetConfigValue(fmt.Sprintf("case%v", i), "kaiJiang")
		kaiJiangarr := strings.Split(kaiJiangstr, ",")
		for _, v := range kaiJiangarr {
			n, _ := strconv.Atoi(v)
			lhjObj.kaiJiang = append(lhjObj.kaiJiang, int32(n))
		}

		validlineidstr := pcfg.GetConfigValue(fmt.Sprintf("case%v", i), "validlineid")
		validlineidarr := strings.Split(validlineidstr, ",")
		for _, v := range validlineidarr {
			validlineid, _ := strconv.Atoi(v)
			validObj.validlineid = append(validObj.validlineid, int32(validlineid))
		}

		lhjArr = append(lhjArr, lhjObj)
		validArr = append(validArr, validObj)

	}

	return lhjArr, validArr
}

//TODO wild, w2加倍驗證, w2的2 * 數量 * 賠率
