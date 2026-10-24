package hbsl

import (
	"math/rand"
	"sort"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/util"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/global"
	"example.invalid/ServerTeam/Servers/pkg/netproto"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
)

type HbslLogic struct {
	*gamelogic.ComputeGameLogicBase
	ary_pUser      []*User   // 所有玩家
	ary_pHB        []*HB     // 紅包注列
	pNowHB         *HBRecord // 目前發放的紅包資料
	iGameState     int       // 遊戲狀態
	sNowActionName string    // 遊戲狀態名稱
	ary_strManual  []string  // 遊戲說明
	/*config*/
	map_iTimeout map[int]int32 // timeout 時間設定
	ary_i64Chip  []int64       // 紅包金額
	i32BombRate  int32         // 踩雷倍數
	i32HBNum     int32         // 紅包數量

}

// 創建紅包掃雷邏輯
func NewHbslLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) *HbslLogic {
	glc := new(HbslLogic)
	glc.ComputeGameLogicBase = gamelogic.NewComputeGameLogicBase(table, tfManager, configs)

	glc.GetGameConfig(configs)

	glc.ary_pUser = make([]*User, 0)
	glc.ary_pHB = make([]*HB, 0)

	// 遊戲說明
	glc.ary_strManual = append(glc.ary_strManual, "庄家(发红包的玩家)根据房间限定的金币范围及红包数量发出红包，同时选定0-9任一个数字为雷号。")
	glc.ary_strManual = append(glc.ary_strManual, "红包设定好之后，进入等待列表，按照排列顺序依次发出(金额>发钱顺序)")
	glc.ary_strManual = append(glc.ary_strManual, "红包设定好包数和金额后系统会自动分配金额在红包里面")
	glc.ary_strManual = append(glc.ary_strManual, "闲家(扫雷玩家)抢到红包：")
	glc.ary_strManual = append(glc.ary_strManual, "1.未中雷，扫雷玩家本次获得红包金额尾数与庄家设置的雷号不同，避雷成功，获得所抢红包内的金额")
	glc.ary_strManual = append(glc.ary_strManual, "2.中雷，扫雷玩家本次获得红包金额尾数与庄家设置的雷号相同则中雷，须根据房间倍数及庄家红包金额赔付给庄家，如果是庄家自己抢红包的话则中雷不扣钱。")
	glc.ary_strManual = append(glc.ary_strManual, "\n• 税收：")
	glc.ary_strManual = append(glc.ary_strManual, "收取赢家金额的5%")

	// 測試流程
	//tfManager.NewTimer(5, func() {
	//	glc.GameStartFunc()
	//})

	glc.RegisterBeforeGameStartFunc(func() {
		//fmt.Printf("BeforeGameStart\n")
		glc.InitData()
	})
	glc.RegisterAfterGameStartFunc(func() {
		//fmt.Printf("AfterGameStart\n")

		for i := 0; i < len(glc.ary_pUser); i++ {
			if glc.ary_pUser[i].pTempState != nil {
				glc.ary_pUser[i].pGameUser.UserState = glc.ary_pUser[i].pTempState
			}
		}

		glc.StartGrab(false, nil)
	})
	glc.RegisterBeforeGameEndFunc(func() {
		//fmt.Printf("BeforeGameEnd\n")
	})
	glc.RegisterAfterGameEndFunc(func() {
		//fmt.Printf("AfterGameEnd\n")

		for _, ur := range glc.AllTableUser() {
			// 用户状态变为准备状态
			ur.UserState.OnUserReady(ur, glc.TableManager.Room)
		}

		if !glc.IsGaming() {
			if glc.TableUserCount() >= glc.GetServerConfig().MinPlayer {
				glc.GameStartFunc()
			}
		}
		// 測試流程
		//tfManager.NewTimer(3, glc.GameStartFunc)
	})

	return glc
}

/*interface*/

//桌子上的真實用户数
func (pGame *HbslLogic) TableRealUserCount() int {
	UserCount := pGame.TableManager.TableRealUserCount()
	if UserCount == 0 {
		UserCount++
	}
	return UserCount
}

// 用戶進入
func (glc *HbslLogic) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	//fmt.Printf("UserEnter %d\n", gameuser.UserID)
	mlog.System("UserEnter %d", gameuser.UserID)
	glc.ComputeGameLogicBase.UserEnter(gameuser, tableno, seatno)
	glc.SendGameInfo(gameuser)

	//fmt.Printf("UserEnter OK %d\n", gameuser.UserID)
}

// 用戶站立
func (glc *HbslLogic) UserFree(gameuser *gamelogic.GameUser) {
	//fmt.Printf("UserFree %d\n", gameuser.UserID)
	mlog.Debug("UserFree")
	pUser := glc.GetUserDataByID(gameuser.UserID)

	if pUser != nil {
		if pUser.pGrabHB != nil {
			mlog.Debug("用户在玩游戏还不能离开")
			return
		}

		if pUser.pFaHB != nil {
			mlog.Debug("用户在玩游戏还不能离开")
			return
		}
	}

	glc.DelUserData(gameuser.UserID)

	glc.ChangeUserStateEnd(gameuser)
	glc.ComputeGameLogicBase.UserFree(gameuser)

	//fmt.Printf("UserFree OK %d\n", gameuser.UserID)
}

// 用户斷線重連
func (glc *HbslLogic) UserRecover(gameuser *gamelogic.GameUser) {
	//fmt.Printf("UserRecover %d\n", gameuser.UserID)
	mlog.Debug("UserRecover %d", gameuser.UserID)
	glc.ComputeGameLogicBase.UserRecover(gameuser)

	glc.SendGameInfo(gameuser)

	pUser := glc.GetUserDataByID(gameuser.UserID)
	if pUser != nil && pUser.pTempState != nil {
		pUser.pTempState = nil
	}

	//fmt.Printf("UserRecover OK %d\n", gameuser.UserID)
}

// 用戶離開
func (glc *HbslLogic) UserLeave(gameuser *gamelogic.GameUser) bool {
	//fmt.Printf("UserLeave %d\n", gameuser.UserID)
	mlog.Debug("UserLeave %d", gameuser.UserID)
	pUser := glc.GetUserDataByID(gameuser.UserID)

	if pUser != nil {
		if pUser.pGrabHB != nil {
			pUser.pTempState = new(gamelogic.UserOfflineState)
			mlog.Debug("用户在玩游戏还不能离开")
			return false
		}

		if pUser.pFaHB != nil {
			pUser.pTempState = new(gamelogic.UserOfflineState)
			mlog.Debug("用户在玩游戏还不能离开")
			return false
		}
	}

	glc.DelUserData(gameuser.UserID)

	glc.ChangeUserStateEnd(gameuser)

	//fmt.Printf("UserLeave OK %d\n", gameuser.UserID)
	return glc.ComputeGameLogicBase.UserLeave(gameuser)
}

// 遊戲封包
func (glc *HbslLogic) GameMessage(gameuser *gamelogic.GameUser, gmsg *msg.Message) {
	defer util.PanicErrStack()

	if gameuser == nil || gmsg == nil {
		return
	}

	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	switch gmsg.SClassID {
	case int32(netproto.HBSL_GameMessageClassID_HBSL_AllUserInfo): // C2S 在線玩家
		glc.Message_AllUserInfo(gameuser, gmsg)
	case int32(netproto.HBSL_GameMessageClassID_HBSL_Grab): // C2S 搶紅包
		glc.Message_Grab(gameuser, gmsg)
	case int32(netproto.HBSL_GameMessageClassID_HBSL_FaHB): // C2S 發紅包
		glc.Message_FaHB(gameuser, gmsg)
	case int32(netproto.HBSL_GameMessageClassID_HBSL_AllHBInfo): // C2S 紅包列表
		glc.Message_HBLList(gameuser, gmsg)
	}
}

/*Interface*/

/*GameLogic*/

/*遊戲流程*/

// 讀取遊戲設定
func (glc *HbslLogic) GetGameConfig(configs *config.ConfigGroup) {
	mlog.Debug("GetGameConfig")
	if configs == nil {
		mlog.Error("桌號[%d] 讀取 config 失敗", glc.TableNo())
		return
	}

	var str string = ""

	// 讀取 gameserver 設定值
	cfgs := configs.GetConfig("gameserver")
	if cfgs == nil {
		mlog.Error("獲取配置 [gameserver] 失敗")
	} else {
		//----- timeout -----
		glc.map_iTimeout = make(map[int]int32)
		glc.map_iTimeout[Timeout_Grab] = cfgs.GetConfigInt32(ConfigName_Game, "Timeout_Start")
		glc.map_iTimeout[Timeout_Settlement] = cfgs.GetConfigInt32(ConfigName_Game, "Timeout_End")

		// 紅包金額
		str = cfgs.GetConfigValue(ConfigName_Game, "HBValue")
		ary_strChip := strings.Split(str, ",")
		for i := 0; i < len(ary_strChip); i++ {
			i64chip, _ := strconv.ParseInt(ary_strChip[i], 10, 32)
			glc.ary_i64Chip = append(glc.ary_i64Chip, i64chip)
		}

		// 踩雷倍數
		glc.i32BombRate = cfgs.GetConfigInt32(ConfigName_Game, "BombRate")

		// 紅包數量
		glc.i32HBNum = cfgs.GetConfigInt32(ConfigName_Game, "HBNum")
	}
}

func (glc *HbslLogic) InitData() {
	mlog.Debug("InitData")
	glc.pNowHB = nil
	glc.iGameState = -1
	glc.sNowActionName = ""
}

// 開始搶紅包
func (glc *HbslLogic) StartGrab(bForUser bool, pGameUser *gamelogic.GameUser) {
	defer util.PanicErrStack()

	if !bForUser {
		mlog.Debug("StartGrab")
		glc.GameLog.AddGameStep("开始抢红包")

		glc.sNowActionName = glc.PlayerActionManager.NewAction(ActionName_Grab, glc.i32HBNum, glc.map_iTimeout[Timeout_Grab], func() {
			glc.GameResult(false, nil)
		})
		glc.iGameState = State_Grab

		glc.SetHB()
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_StartGrab)
	pData := new(netproto.HBSL_Msg_StartGrab)
	pData.TimeRemain = proto.Int32(glc.map_iTimeout[Timeout_Grab] - glc.PlayerActionManager.GetActionTimerElapsedSeconds(glc.sNowActionName))
	pData.FaceID = proto.Int32(glc.pNowHB.pHB.i32FaceID)
	pData.ZhuangID = proto.Int32(glc.pNowHB.pHB.i32UserID)
	pData.HBMoney = proto.Int64(glc.pNowHB.pHB.i64Chip)
	pData.IsReconnect = proto.Bool(bForUser)
	pMsg.MsgData = pData

	if bForUser {
		if pGameUser != nil {
			glc.SendMessageToUser(pGameUser, pMsg)
		}
	} else {
		glc.BoardcastMessage(pMsg)
	}

}

// 處裡分紅包
func (glc *HbslLogic) SetHB() {
	glc.pNowHB = new(HBRecord)

	glc.pNowHB.pHB = new(HB)
	glc.pNowHB.ary_i64SHB = make([]int64, 0)
	glc.pNowHB.ary_pUserRecord = make([]*UserRecord, 0)
	glc.pNowHB.i64ZhuangWinLose = 0

	if len(glc.ary_pHB) > 0 {
		glc.pNowHB.pHB = glc.ary_pHB[0]
		glc.ary_pHB = append(glc.ary_pHB[:0], glc.ary_pHB[1:]...)

		mlog.Debug("庄家[%d] 红包金额[%d] 雷号[%d]", glc.pNowHB.pHB.i32UserID, glc.pNowHB.pHB.i64Chip, glc.pNowHB.pHB.i32BombNumber)
		glc.GameLog.AddGameStep("庄家[%d] 红包金额[%d] 雷号[%d]", glc.pNowHB.pHB.i32UserID, glc.pNowHB.pHB.i64Chip, glc.pNowHB.pHB.i32BombNumber)
	} else {
		iRnd := rand.Intn(len(glc.ary_i64Chip))
		glc.pNowHB.pHB.i64Chip = glc.ary_i64Chip[iRnd]

		iBombNumber := rand.Intn(10)
		glc.pNowHB.pHB.i32BombNumber = int32(iBombNumber)
		glc.pNowHB.pHB.i32UserID = -1
		glc.pNowHB.pHB.i32FaceID = 0
		glc.pNowHB.pHB.pUser = new(User)
		glc.pNowHB.pHB.pUser.pGameUser = gamelogic.NewGameUser()

		mlog.Debug("庄家[系统小妹] 红包金额[%d] 雷号[%d]", glc.pNowHB.pHB.i64Chip, glc.pNowHB.pHB.i32BombNumber)
		glc.GameLog.AddGameStep("庄家[系统小妹] 红包金额[%d] 雷号[%d]", glc.pNowHB.pHB.i64Chip, glc.pNowHB.pHB.i32BombNumber)
	}

	glc.pNowHB.ary_i64SHB = glc.DivideHB()
	// 測試踩雷
	//glc.pNowHB.pHB.i32BombNumber = int32(glc.pNowHB.ary_i64SHB[0] % 10)

	glc.pNowHB.i64ZhuangWinLose = -(glc.pNowHB.pHB.i64Chip)
}

func (glc *HbslLogic) DivideHB_old() []int64 {
	i64HBMoney := glc.pNowHB.pHB.i64Chip
	i32HBNum := glc.i32HBNum

	// 扣稅
	_, tax := glc.GetUserWinAndTax(i64HBMoney)
	i64HBMoney -= tax

	i64AllValue := make([]int64, 0)
	for i := int32(0); i < i32HBNum; i++ {
		if i == i32HBNum-1 {
			i64AllValue = append(i64AllValue, i64HBMoney)
			break
		}

		i64Valve := rand.Int63n((i64HBMoney - int64(i32HBNum-i-1))) + 1
		i64AllValue = append(i64AllValue, i64Valve)

		i64HBMoney -= i64Valve
	}

	mlog.Debug("分红包 总红包金额[%d] 税收[%d] 小红包%+v", glc.pNowHB.pHB.i64Chip-tax, tax, i64AllValue)
	glc.GameLog.AddGameStep("分红包 总红包金额[%d] 税收[%d] 小红包%+v", glc.pNowHB.pHB.i64Chip-tax, tax, i64AllValue)

	return i64AllValue
}

func (glc *HbslLogic) DivideHB() []int64 {
	i64HBMoney := glc.pNowHB.pHB.i64Chip
	i32HBNum := glc.i32HBNum

	// 扣稅
	_, tax := glc.GetUserWinAndTax(i64HBMoney)
	i64HBMoney -= tax

	i64AllValue := make([]int64, 0)
	randPosition := make([]int64, 0)
	randPosition = append(randPosition, 0)
	for i := int32(0); i < i32HBNum-1; i++ {
		for {
			// rand 1 to i64HBMoney-1
			rnd := rand.Int63n(i64HBMoney-1) + 1
			if exists, _ := util.InArray(rnd, randPosition); !exists {
				randPosition = append(randPosition, rnd)
				break
			}
		}
	}

	sort.Slice(randPosition, func(i, j int) bool {
		return randPosition[i] > randPosition[j]
	}) // 按贏錢金額大至小排序
	pos := i64HBMoney
	for _, v := range randPosition {
		i64AllValue = append(i64AllValue, pos-v)
		pos = v
	}

	mlog.Debug("分红包 总红包金额[%d] 税收[%d] 小红包%+v", glc.pNowHB.pHB.i64Chip-tax, tax, i64AllValue)
	glc.GameLog.AddGameStep("分红包 总红包金额[%d] 税收[%d] 小红包%+v", glc.pNowHB.pHB.i64Chip-tax, tax, i64AllValue)

	return i64AllValue
}

// 遊戲結果
func (glc *HbslLogic) GameResult(bForUser bool, pGameUser *gamelogic.GameUser) {
	defer util.PanicErrStack()

	if !bForUser {
		mlog.Debug("GameResult")
		glc.GameLog.AddGameStep("结算")

		glc.sNowActionName = glc.PlayerActionManager.NewAction(ActionName_Settlement, 0, glc.map_iTimeout[Timeout_Settlement], glc.GameOver)
		glc.iGameState = State_Settlement

		glc.Settlement()
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_Settlement)
	pData := new(netproto.HBSL_Msg_Settlement)
	pData.FaceID = proto.Int32(glc.pNowHB.pHB.i32FaceID)
	pData.ZhuangID = proto.Int32(glc.pNowHB.pHB.i32UserID)
	pData.ZhuangWinLose = proto.Int64(glc.pNowHB.i64ZhuangWinLose)
	if glc.pNowHB.pHB.pUser != nil {
		pData.ZhuangRemainMoney = proto.Int64(glc.pNowHB.pHB.pUser.i64Money)
	} else {
		pData.ZhuangRemainMoney = proto.Int64(0)
	}
	pData.LandmineNumber = proto.Int32(glc.pNowHB.pHB.i32BombNumber)

	// 已搶紅包的玩家資訊
	for i := 0; i < len(glc.pNowHB.ary_pUserRecord); i++ {
		pUserInfo := glc.UserInfoPkg(i, UserInfoKind_Grab)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	pData.HBMoney = proto.Int64(glc.pNowHB.pHB.i64Chip)

	pMsg.MsgData = pData

	if bForUser {
		if pGameUser != nil {
			glc.SendMessageToUser(pGameUser, pMsg)
		}
	} else {
		glc.BoardcastMessage(pMsg)
	}
}

// 總結輸贏
func (glc *HbslLogic) Settlement() {
	pZhuang := glc.pNowHB.pHB.pUser

	// 莊家扣紅包錢
	pZhuang.i64MoneyChange -= glc.pNowHB.pHB.i64Chip
	_, HBTax := glc.GetUserWinAndTax(glc.pNowHB.pHB.i64Chip)

	i64SurplusHB := int64(0)
	for i := 0; i < len(glc.pNowHB.ary_i64SHB); i++ {
		i64SurplusHB += glc.pNowHB.ary_i64SHB[i]
	}

	// 莊家回收紅包
	pZhuang.i64Money += i64SurplusHB
	pZhuang.i64MoneyChange += i64SurplusHB
	pZhuang.i64TotalWinLose += i64SurplusHB

	mlog.Debug("庄家[%d] 回收红包 金额[%s] 剩余金钱 [%s]", pZhuang.pGameUser.UserID,
		global.MoneyString(i64SurplusHB),
		global.MoneyString(pZhuang.i64Money))
	glc.GameLog.AddGameStep("庄家[%d] 回收红包 金额[%s] 剩余金钱 [%s]", pZhuang.pGameUser.UserID,
		global.MoneyString(i64SurplusHB),
		global.MoneyString(pZhuang.i64Money))

	i64AllMoneyChange := int64(0)
	for i := 0; i < len(glc.pNowHB.ary_pUserRecord); i++ {
		pUser := glc.pNowHB.ary_pUserRecord[i].pUser

		if pUser == nil {
			continue
		}

		// 踩雷(自己發的紅包不算)
		if pUser.pGameUser.UserID != glc.pNowHB.pHB.i32UserID {
			if glc.pNowHB.ary_pUserRecord[i].i64HBMoney%10 == int64(glc.pNowHB.pHB.i32BombNumber) {
				i64MoneyChange := glc.pNowHB.pHB.i64Chip * int64(glc.i32BombRate)

				pUser.i64Money -= i64MoneyChange
				pUser.i64MoneyChange -= i64MoneyChange
				pUser.i64TotalWinLose -= i64MoneyChange

				i64AllMoneyChange += i64MoneyChange

				glc.pNowHB.ary_pUserRecord[i].i64LoseMoney -= i64MoneyChange

				mlog.Debug("用户[%d] 踩雷 金额[%s] 剩余金钱 [%s]", pUser.pGameUser.UserID,
					global.MoneyString(i64MoneyChange),
					global.MoneyString(pUser.i64Money))
				glc.GameLog.AddGameStep("用户[%d] 踩雷 金额[%s] 剩余金钱 [%s]", pUser.pGameUser.UserID,
					global.MoneyString(i64MoneyChange),
					global.MoneyString(pUser.i64Money))
			} else {
				pUser.iWinRounds++
			}
		}

		// 牌局紀錄
		pUser.pHBRecord = append(pUser.pHBRecord, glc.pNowHB)

		// 搶紅包寫分
		if !pUser.pGameUser.IsRobot() && pUser.pGameUser.UserID != pZhuang.pGameUser.UserID {
			glc.UpdateUserScoreFunc(pUser.pGameUser.UserID, pUser.i64MoneyChange, 0)
		}
	}

	// 扣稅
	_, tax := glc.GetUserWinAndTax(i64AllMoneyChange)
	i64AllMoneyChange -= tax

	// 莊家輸贏
	glc.pNowHB.i64ZhuangWinLose = i64SurplusHB + i64AllMoneyChange - glc.pNowHB.pHB.i64Chip

	// 莊家取踩雷金額
	pZhuang.i64Money += i64AllMoneyChange
	pZhuang.i64MoneyChange += i64AllMoneyChange
	pZhuang.i64TotalWinLose += i64AllMoneyChange

	mlog.Debug("庄家[%d] 输赢 金额[%s] 税收[%s] 剩余金钱 [%s]", pZhuang.pGameUser.UserID,
		global.MoneyString(i64AllMoneyChange),
		global.MoneyString(tax),
		global.MoneyString(pZhuang.i64Money))
	glc.GameLog.AddGameStep("庄家[%d] 输赢 金额[%s] 税收[%s] 剩余金钱 [%s]", pZhuang.pGameUser.UserID,
		global.MoneyString(i64AllMoneyChange),
		global.MoneyString(tax),
		global.MoneyString(pZhuang.i64Money))

	if pZhuang.pGameUser != nil {
		// 莊家寫分
		if !pZhuang.pGameUser.IsRobot() {
			glc.UpdateUserScoreFunc(pZhuang.pGameUser.UserID, pZhuang.i64MoneyChange, tax+HBTax)
		}
	} else {
		glc.pNowHB.pHB.Init()
	}
}

// 分數紀錄
func (glc *HbslLogic) GameOver() {
	mlog.Debug("GameOver")
	// 牌局紀錄
	map_i64ScoreRet := make(map[int32]int64)
	for i := 0; i < len(glc.pNowHB.ary_pUserRecord); i++ {
		pUserRecord := glc.pNowHB.ary_pUserRecord[i]
		if pUserRecord.pUser != nil {
			if pUserRecord.pUser.i64MoneyChange >= 0 {
				pUserRecord.pUser.pGameUser.FlowAmount += pUserRecord.pUser.i64MoneyChange
			} else {
				pUserRecord.pUser.pGameUser.FlowAmount += -pUserRecord.pUser.i64MoneyChange
			}

			map_i64ScoreRet[pUserRecord.pUser.pGameUser.UserID] = pUserRecord.pUser.i64MoneyChange
			pUserRecord.pUser.Init(false)
			pUserRecord.Init()
		}
	}
	pZhuang := glc.pNowHB.pHB.pUser

	if pZhuang != nil {
		if _, ok := map_i64ScoreRet[pZhuang.pGameUser.UserID]; !ok {
			if pZhuang.i64MoneyChange >= 0 {
				pZhuang.pGameUser.FlowAmount += pZhuang.i64MoneyChange
			} else {
				pZhuang.pGameUser.FlowAmount += -pZhuang.i64MoneyChange
			}
			map_i64ScoreRet[pZhuang.pGameUser.UserID] = pZhuang.i64MoneyChange
		}
		pZhuang.Init(true)
	}

	glc.pNowHB.pHB.Init()

	glc.GameLog.GameEnd(map_i64ScoreRet)
	glc.GameEndFunc()
}

/*封包處理*/
// 訊息處理 - 線上玩家列表
func (glc *HbslLogic) Message_AllUserInfo(pGameUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pGameUser == nil || pMsg == nil {
		return
	}

	iResult := glc.UserAction_AllUserInfo()
	glc.SendAllUserInfoResult(pGameUser, iResult)
}

// 線上玩家列表處理
func (glc *HbslLogic) UserAction_AllUserInfo() []*netproto.HBSL_Msg_UserInfo {
	AllUserInfo := make([]*netproto.HBSL_Msg_UserInfo, 0)
	for i := 0; i < len(glc.ary_pUser); i++ {
		pUserInfo := glc.UserInfoPkg(i, UserInfoKind_All)
		AllUserInfo = append(AllUserInfo, pUserInfo)
	}

	if len(AllUserInfo) < AllUserInfoMax {
		for _, user := range glc.AllTableUser() {
			iIdx := Find(len(AllUserInfo), func(i int) bool {
				return AllUserInfo[i].GetUserID() == user.UserID
			})

			if iIdx != -1 {
				continue
			}

			pUserInfo := new(netproto.HBSL_Msg_UserInfo)

			pUserInfo.UserID = proto.Int32(user.UserID)
			pUserInfo.Money = proto.Int64(user.Score)
			pUserInfo.FaceID = proto.Int32(user.FaceID)
			//pUserInfo.Rounds = proto.Int32(user.WinCount + user.LoseCount + user.DrawCount)
			pUserInfo.Rounds = proto.Int32(0)
			//pUserInfo.WinRounds = proto.Int32(user.WinCount)
			pUserInfo.WinRounds = proto.Int32(0)
			//pUserInfo.WinMoney = proto.Int64(user.UpdateScore)
			pUserInfo.WinMoney = proto.Int64(0)
			pUserInfo.HBMoney = proto.Int64(0)
			pUserInfo.LoseMoney = proto.Int64(0)
			AllUserInfo = append(AllUserInfo, pUserInfo)

			if len(AllUserInfo) >= AllUserInfoMax {
				break
			}
		}
	}

	sort.Slice(AllUserInfo, func(i, j int) bool {
		return AllUserInfo[i].GetWinMoney() > AllUserInfo[j].GetWinMoney()
	}) // 按贏錢金額大至小排序

	if len(AllUserInfo) > AllUserInfoMax {
		AllUserInfo = append(AllUserInfo[0:AllUserInfoMax])
	}

	return AllUserInfo
}

// 傳送線上玩家列表結果
func (glc *HbslLogic) SendAllUserInfoResult(pGameUser *gamelogic.GameUser, iResult []*netproto.HBSL_Msg_UserInfo) {
	if pGameUser == nil {
		mlog.Debug("SendAllUserInfoResult(), 無玩家資料")
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_AllUserInfoResult)

	pData := new(netproto.HBSL_Msg_AllUserInfoResult)
	pData.AllUserInfo = iResult
	pMsg.MsgData = pData
	glc.SendMessageToUser(pGameUser, pMsg)
}

// 訊息處理 - 玩家搶紅包
func (glc *HbslLogic) Message_Grab(pGameUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pGameUser == nil || pMsg == nil {
		return
	}

	if glc.iGameState != State_Grab {
		glc.SendGrabResult(pGameUser, GrabResult_GrabingStopped)
		return
	}

	iResult := glc.UserAction_Grab(pGameUser)
	glc.SendGrabResult(pGameUser, iResult)

	if iResult == GrabResult_Success {
		glc.PlayerActionManager.OnUserAction(glc.sNowActionName, pGameUser.UserID)
	}
}

// 搶紅包處裡
func (glc *HbslLogic) UserAction_Grab(pGameUser *gamelogic.GameUser) int {
	if pGameUser == nil {
		return GrabResult_BetFailed
	}

	iResult := GrabResult_Success

	pUser := glc.GetUserDataByID(pGameUser.UserID)
	if pUser == nil {
		pUser = glc.AddUserData(pGameUser)
	}

	if pUser.pGameUser.UserID != glc.pNowHB.pHB.i32UserID && pUser.i64Money < glc.pNowHB.pHB.i64Chip*int64(glc.i32BombRate) {
		return GrabResult_NotEnoughMoney
	}

	if int32(len(glc.pNowHB.ary_pUserRecord)) >= glc.i32HBNum {
		return GrabResult_GrabingStopped
	}

	if pUser.pGrabHB != nil {
		return GrabResult_AlreadyGrab
	}

	if len(glc.pNowHB.ary_i64SHB) <= 0 {
		mlog.Error("紅包數量出錯")
		return GrabResult_GrabingStopped
	}

	HBMoney := glc.pNowHB.ary_i64SHB[0]

	glc.pNowHB.ary_i64SHB = append(glc.pNowHB.ary_i64SHB[:0], glc.pNowHB.ary_i64SHB[1:]...)
	//fmt.Printf("%+v\n", glc.pNowHB.ary_i64SHB)

	pUser.i64Money += HBMoney
	pUser.i64MoneyChange += HBMoney
	pUser.i64TotalWinLose += HBMoney
	pUser.pGrabHB = glc.pNowHB

	pUserRecord := new(UserRecord)
	pUserRecord.iUserID = pUser.pGameUser.UserID
	pUserRecord.iFaceID = pUser.pGameUser.FaceID
	pUserRecord.i64HBMoney = HBMoney
	pUserRecord.i64Money = pUser.i64Money
	pUserRecord.i64LoseMoney = 0
	pUserRecord.pUser = pUser

	glc.pNowHB.ary_pUserRecord = append(glc.pNowHB.ary_pUserRecord, pUserRecord)

	return iResult
}

// 傳送玩家搶紅包結果
func (glc *HbslLogic) SendGrabResult(pGameUser *gamelogic.GameUser, iResult int) {
	if pGameUser == nil {
		mlog.Debug("SendGrabResult(), 無玩家資料")
		return
	}

	mlog.Debug("SendGrabResult(), User[%d], iResult[%d]", pGameUser.UserID, iResult)

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_GrabResult)

	pData := new(netproto.HBSL_Msg_GrabResult)
	pData.Result = proto.Int32(int32(iResult)) // 下注結果
	pData.HBRemain = proto.Int32(int32(len(glc.pNowHB.ary_i64SHB)))
	if iResult == GrabResult_Success {
		pData.UserInfo = glc.UserInfoPkg(len(glc.pNowHB.ary_pUserRecord)-1, UserInfoKind_Grab)
	} else {
		pData.UserInfo = glc.UserInfoPkg(0, UserInfoKind_Empty)
	}

	pMsg.MsgData = pData
	if iResult == GrabResult_Success {
		glc.BoardcastMessage(pMsg)
	} else {
		glc.SendMessageToUser(pGameUser, pMsg)
	}

	if iResult == GrabResult_Success {
		mlog.Debug("用户[%d] 抢红包 金额[%s] 结果[%s] 剩余金钱 [%s]", pData.UserInfo.GetUserID(),
			global.MoneyString(pData.UserInfo.GetHBMoney()),
			g_map_strGrabResult[iResult],
			global.MoneyString(pData.UserInfo.GetMoney()))
		glc.GameLog.AddGameStep("用户[%d] 抢红包 金额[%s] 结果[%s] 剩余金钱 [%s]", pData.UserInfo.GetUserID(),
			global.MoneyString(pData.UserInfo.GetHBMoney()),
			g_map_strGrabResult[iResult],
			global.MoneyString(pData.UserInfo.GetMoney()))
	} else {
		mlog.Debug("用户[%d] 抢红包失敗 结果[%s]", pGameUser.UserID, g_map_strGrabResult[iResult])
		glc.GameLog.AddGameStep("用户[%d] 抢红包失敗 结果[%s]", pGameUser.UserID, g_map_strGrabResult[iResult])
	}

}

// 訊息處理 - 玩家發紅包
func (glc *HbslLogic) Message_FaHB(pGameUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pGameUser == nil || pMsg == nil {
		return
	}

	iResult, pHB := glc.UserAction_FaHB(pGameUser, pMsg)
	glc.SendFaHBResult(pGameUser, iResult, pHB)
}

// 發紅包處裡
func (glc *HbslLogic) UserAction_FaHB(pGameUser *gamelogic.GameUser, pMsg *msg.Message) (int, *HB) {
	if pGameUser == nil || pMsg == nil {
		return FaHBResult_BetFailed, nil
	}

	iResult := FaHBResult_Success

	pUser := glc.GetUserDataByID(pGameUser.UserID)
	if pUser == nil {
		pUser = glc.AddUserData(pGameUser)
	}

	msgInfo := &netproto.HBSL_Msg_FaHB{}
	proto.Unmarshal(pMsg.MsgData.([]byte), msgInfo)

	i64HBMoney := msgInfo.GetHBMoney()
	i32BombNumber := msgInfo.GetLandmineNumber()

	bCheck := false
	for i := 0; i < len(glc.ary_i64Chip); i++ {
		if glc.ary_i64Chip[i] == i64HBMoney {
			bCheck = true
			break
		}
	}

	// 紅包金額錯誤
	if !bCheck {
		return FaHBResult_HBMoneyError, nil
	}

	// 雷號錯誤
	if i32BombNumber > 9 || i32BombNumber < 0 {
		return FaHBResult_BombNumberError, nil
	}

	// 身上金錢不夠
	if pUser.i64Money < i64HBMoney {
		return FaHBResult_NotEnoughMoney, nil
	}

	// 已經發過紅包
	if pUser.pFaHB != nil {
		return FaHBResult_AlreadyFaHB, nil
	}

	pUser.i64Money -= i64HBMoney
	pUser.i64TotalWinLose -= i64HBMoney

	pHB := new(HB)
	pHB.i64Chip = i64HBMoney
	pHB.i32BombNumber = i32BombNumber
	pHB.i32UserID = pUser.pGameUser.UserID
	pHB.i32FaceID = pUser.pGameUser.FaceID
	pHB.pUser = pUser

	pUser.pFaHB = pHB

	glc.ary_pHB = append(glc.ary_pHB, pHB)
	sort.Slice(glc.ary_pHB, func(i, j int) bool {
		return glc.ary_pHB[i].i64Chip > glc.ary_pHB[j].i64Chip
	}) // 依照紅包大小排序

	return iResult, pHB
}

// 傳送玩家發紅包結果
func (glc *HbslLogic) SendFaHBResult(pGameUser *gamelogic.GameUser, iResult int, pHB *HB) {
	if pGameUser == nil {
		mlog.Debug("SendFaHBResult(), 無玩家資料")
		return
	}

	mlog.Debug("SendFaHBResult(), User[%d], iResult[%d]", pGameUser.UserID, iResult)

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_FaHBResult)

	pData := new(netproto.HBSL_Msg_FaHBResult)
	pData.Result = proto.Int32(int32(iResult)) // 下注結果
	if pHB != nil {
		pData.RemainMoney = proto.Int64(pHB.pUser.i64Money) // 玩家剩餘金錢
	} else {
		pData.RemainMoney = proto.Int64(0) // 玩家剩餘金錢
	}

	pMsg.MsgData = pData

	glc.SendMessageToUser(pGameUser, pMsg)

	if iResult == FaHBResult_Success {
		mlog.Debug("用户[%d] 发红包 金额[%s] 雷号[%d] 结果[%s] 剩余金钱 [%s] ", pHB.i32UserID,
			global.MoneyString(pHB.i64Chip),
			pHB.i32BombNumber,
			g_map_strFaHBResult[iResult],
			global.MoneyString(pHB.pUser.i64Money))
		glc.GameLog.AddGameStep("用户[%d] 发红包 金额[%s] 雷号[%d] 结果[%s] 剩余金钱 [%s] ", pHB.i32UserID,
			global.MoneyString(pHB.i64Chip),
			pHB.i32BombNumber,
			g_map_strFaHBResult[iResult],
			global.MoneyString(pHB.pUser.i64Money))
	} else {
		mlog.Debug("用户[%d] 发红包失敗 结果[%s]", pGameUser.UserID, g_map_strFaHBResult[iResult])
		glc.GameLog.AddGameStep("用户[%d] 发红包失敗 结果[%s]", pGameUser.UserID, g_map_strFaHBResult[iResult])
	}
}

// 訊息處理 - 紅包列表
func (glc *HbslLogic) Message_HBLList(pGameUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pGameUser == nil || pMsg == nil {
		return
	}

	iResult := glc.UserAction_HBLList()
	glc.SendHBLListResult(pGameUser, iResult)
}

// 紅包列表處裡
func (glc *HbslLogic) UserAction_HBLList() []*netproto.HBSL_Msg_HBInfo {
	AllHBInfo := glc.HBInfoPkg()
	return AllHBInfo
}

// 傳送紅包列表結果
func (glc *HbslLogic) SendHBLListResult(pGameUser *gamelogic.GameUser, iResult []*netproto.HBSL_Msg_HBInfo) {
	if pGameUser == nil {
		mlog.Debug("SendHBLListResult(), 無玩家資料")
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_AllHBInfoResult)

	pData := new(netproto.HBSL_Msg_AllHBInfoResult)
	pData.AllHBInfo = iResult
	pMsg.MsgData = pData
	glc.SendMessageToUser(pGameUser, pMsg)
}

/*其它邏輯*/

// 打包玩家資訊
func (glc *HbslLogic) UserInfoPkg(iSeat, iKind int) *netproto.HBSL_Msg_UserInfo {
	switch iKind {
	case UserInfoKind_Grab:
		if iSeat < 0 || iSeat >= len(glc.pNowHB.ary_pUserRecord) {
			return nil
		}

		if glc.pNowHB == nil {
			return nil
		}

		pUser := glc.pNowHB.ary_pUserRecord[iSeat]

		pUserInfo := new(netproto.HBSL_Msg_UserInfo)

		pUserInfo.UserID = proto.Int32(pUser.iUserID)
		pUserInfo.Money = proto.Int64(pUser.i64Money)
		pUserInfo.FaceID = proto.Int32(pUser.iFaceID)
		pUserInfo.Rounds = proto.Int32(0)
		pUserInfo.WinRounds = proto.Int32(0)
		pUserInfo.WinMoney = proto.Int64(0)
		pUserInfo.HBMoney = proto.Int64(pUser.i64HBMoney)
		pUserInfo.LoseMoney = proto.Int64(pUser.i64LoseMoney)

		return pUserInfo
	case UserInfoKind_All:
		if iSeat < 0 || iSeat >= len(glc.ary_pUser) {
			return nil
		}

		pUser := glc.ary_pUser[iSeat]

		pUserInfo := new(netproto.HBSL_Msg_UserInfo)

		pUserInfo.UserID = proto.Int32(pUser.pGameUser.UserID)
		pUserInfo.Money = proto.Int64(pUser.i64Money)
		pUserInfo.FaceID = proto.Int32(pUser.pGameUser.FaceID)
		pUserInfo.Rounds = proto.Int32(int32(len(pUser.pHBRecord)))
		pUserInfo.WinRounds = proto.Int32(int32(pUser.iWinRounds))
		pUserInfo.WinMoney = proto.Int64(pUser.i64TotalWinLose)
		pUserInfo.HBMoney = proto.Int64(0)
		pUserInfo.LoseMoney = proto.Int64(0)

		return pUserInfo
	case UserInfoKind_Empty:
		pUserInfo := new(netproto.HBSL_Msg_UserInfo)
		pUserInfo.UserID = proto.Int32(0)
		pUserInfo.Money = proto.Int64(0)
		pUserInfo.FaceID = proto.Int32(0)
		pUserInfo.Rounds = proto.Int32(0)
		pUserInfo.WinRounds = proto.Int32(0)
		pUserInfo.WinMoney = proto.Int64(0)
		pUserInfo.HBMoney = proto.Int64(0)
		pUserInfo.LoseMoney = proto.Int64(0)

		return pUserInfo
	default:
		return nil
	}

	return nil
}

// 打包自己資訊
func (glc *HbslLogic) SelfInfoPkg(pGameUser *gamelogic.GameUser) *netproto.HBSL_Msg_UserInfo {

	SelfUserInfo := new(netproto.HBSL_Msg_UserInfo)

	pUser := glc.GetUserDataByID(pGameUser.UserID)

	if pUser != nil {
		SelfUserInfo.UserID = proto.Int32(pUser.pGameUser.UserID)
		SelfUserInfo.Money = proto.Int64(pUser.i64Money)
		SelfUserInfo.FaceID = proto.Int32(pUser.pGameUser.FaceID)
		SelfUserInfo.Rounds = proto.Int32(int32(len(pUser.pHBRecord)))
		SelfUserInfo.WinRounds = proto.Int32(int32(pUser.iWinRounds))
		SelfUserInfo.WinMoney = proto.Int64(pUser.i64TotalWinLose)

		if pUser.pGrabHB != nil {
			pUserRecord := pUser.pGrabHB.GetUserRecordByID(pUser.pGameUser.UserID)
			if pUserRecord != nil {
				SelfUserInfo.HBMoney = proto.Int64(pUserRecord.i64HBMoney)
				SelfUserInfo.LoseMoney = proto.Int64(pUserRecord.i64LoseMoney)
			} else {
				SelfUserInfo.HBMoney = proto.Int64(0)
				SelfUserInfo.LoseMoney = proto.Int64(0)
			}
		} else {
			SelfUserInfo.HBMoney = proto.Int64(0)
			SelfUserInfo.LoseMoney = proto.Int64(0)
		}

	} else {
		SelfUserInfo.UserID = proto.Int32(pGameUser.UserID)
		SelfUserInfo.Money = proto.Int64(pGameUser.Score)
		SelfUserInfo.FaceID = proto.Int32(pGameUser.FaceID)
		SelfUserInfo.Rounds = proto.Int32(0)
		SelfUserInfo.WinRounds = proto.Int32(0)
		SelfUserInfo.WinMoney = proto.Int64(0)
		SelfUserInfo.HBMoney = proto.Int64(0)
		SelfUserInfo.LoseMoney = proto.Int64(0)
	}

	return SelfUserInfo
}

// 打包所有紅包資訊
func (glc *HbslLogic) HBInfoPkg() []*netproto.HBSL_Msg_HBInfo {
	ary_pAllHBInfo := make([]*netproto.HBSL_Msg_HBInfo, 0)
	for i := 0; i < len(glc.ary_pHB); i++ {
		pHBInfo := new(netproto.HBSL_Msg_HBInfo)
		pHBInfo.UserID = proto.Int32(glc.ary_pHB[i].i32UserID)
		pHBInfo.HBMoney = proto.Int64(glc.ary_pHB[i].i64Chip)
		pHBInfo.FaceID = proto.Int32(glc.ary_pHB[i].i32FaceID)
		ary_pAllHBInfo = append(ary_pAllHBInfo, pHBInfo)
	}

	return ary_pAllHBInfo
}

// 傳送牌局資訊
func (glc *HbslLogic) SendGameInfo(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.HBSL_GameMessageClassID_HBSL_GameInfo)

	pData := new(netproto.HBSL_Msg_GameInfo)

	// 分紅包數量
	pData.HBCount = proto.Int32(glc.i32HBNum)
	// 踩雷倍率
	pData.LandmineRate = proto.Int32(glc.i32BombRate)

	// 紅包面額
	for i := 0; i < len(glc.ary_i64Chip); i++ {
		pData.HBValue = append(pData.HBValue, glc.ary_i64Chip[i])
	}

	// 已搶紅包的玩家資訊
	for i := 0; i < len(glc.pNowHB.ary_pUserRecord); i++ {
		pUserInfo := glc.UserInfoPkg(i, UserInfoKind_Grab)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	// 自己的資訊
	pData.SelfUserInfo = glc.SelfInfoPkg(pGameUser)

	// 遊戲說明
	for i := 0; i < len(glc.ary_strManual); i++ {
		pData.PokerManual = append(pData.PokerManual, glc.ary_strManual[i])
	}

	pMsg.MsgData = pData
	glc.SendMessageToUser(pGameUser, pMsg)

	switch glc.iGameState {
	case State_Grab: // 開始搶紅包
		glc.StartGrab(true, pGameUser)
	case State_Settlement: // 結算
		glc.GameResult(true, pGameUser)
	}
}

// 取得User資料
func (glc *HbslLogic) GetUserDataByID(i32UserID int32) *User {
	for _, pUser := range glc.ary_pUser {
		if pUser.pGameUser.UserID == i32UserID {
			return pUser
		}
	}

	return nil
}

// 加入玩家資訊
func (glc *HbslLogic) AddUserData(pGameUser *gamelogic.GameUser) *User {
	pUser := NewUser(pGameUser)
	glc.ary_pUser = append(glc.ary_pUser, pUser)
	return pUser
}

// 刪除玩家資訊
func (glc *HbslLogic) DelUserData(i32UserID int32) {
	for i := 0; i < len(glc.ary_pUser); i++ {
		if glc.ary_pUser[i].pGameUser.UserID == i32UserID {
			glc.ary_pUser = append(glc.ary_pUser[:i], glc.ary_pUser[i+1:]...)
			break
		}
	}
}

/*GameLogic*/

/*Public*/

func Find(n int, f func(int) bool) int {
	for i := 0; i < n; i++ {
		if f(i) {
			return i
		}
	}
	return -1
}

/*Public*/
