package dczz

import (
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	Global "example.invalid/ServerTeam/Servers/pkg/global"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	Pokerset "example.invalid/ServerTeam/Servers/pkg/poker/pokerset"
	"example.invalid/ServerTeam/Servers/pkg/util"
	"strconv"
	"strings"
	"time"
)

//---------------------------------------------------------------------------
type Logic struct {
	*gamelogic.DynamicGameLogicBase
	pGamePoker           *Pokerset.PokerCardSet   // 系統牌盒
	iSeatCount           int                      // 座位數
	pDealerCard          *Pokerset.PokerCardSet   // 莊家手牌資料
	ary_pUserData        []*User                  // 玩家資料
	pGameTicker          *time.Ticker             // 遊戲計時器
	iGameState           int                      // 遊戲狀態
	i64TimeDuration      int64                    // Ticker 計數值
	i64Timeout           int64                    // 各階段 timeout
	i64LastMessageTime   int64                    // 上一次訊息時間
	map_i64Timeout       map[int]int64            // timeout 時間設定
	ary_strPokerManual   []string                 // 遊戲說明
	ary_strJPManual      []string                 // 彩金說明
	ary_i32Chip          []int32                  // 籌碼面額
	uiAllPlayerReady     uint                     // 各玩家均已 Ready，此值設定後就不再變更
	uiPlayerReady        uint                     // 標示各玩家是否 Ready
	iUserCount           int                      // 遊戲中玩家人數
	pRobotSetting        *RobotSetting            // 機器人設定
	i32DealerCardPoint   int32                    // 莊家點數
	bGameOver            bool                     // 遊戲結束
	iDealType            int                      // 發牌方式
	pTestDealerCards     *Pokerset.PokerCardSet   // 測試用莊家手牌
	ary_pTestUserCards   []*Pokerset.PokerCardSet // 測試用玩家手牌
	pTestDealerCardBox   *Pokerset.PokerCardSet   // 測試用莊家手牌牌盒
	ary_pTestUserCardBox []*Pokerset.PokerCardSet // 測試用玩家手牌牌盒
}

//---------------------------------------------------------------------------
func NewLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) *Logic {
	pGame := new(Logic)
	pGame.DynamicGameLogicBase = gamelogic.NewDynamicGameLogicBase(table, tfManager, configs)

	pGame.setGameState(GameState_WaitingPlayer, 0)

	pGame.map_i64Timeout = make(map[int]int64)
	pGame.pRobotSetting = NewRobotSetting()

	pGame.getGameConfig(configs)

	pGame.pGamePoker = Pokerset.NewPokerCardSet()
	pGame.pGamePoker.NewSet(PokerSetCount, 0)
	pGame.pGamePoker.Shuffle()

	pGame.pDealerCard = Pokerset.NewPokerCardSet()
	pGame.pTestDealerCardBox = Pokerset.NewPokerCardSet()

	pGame.ary_pUserData = make([]*User, pGame.iSeatCount)
	pGame.ary_pTestUserCardBox = make([]*Pokerset.PokerCardSet, pGame.iSeatCount)
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pGame.ary_pUserData[iSeat] = NewUser()
		pGame.ary_pTestUserCardBox[iSeat] = Pokerset.NewPokerCardSet()
	}

	// 各玩家均已 Ready，此值設定後就不再變更
	uiReady := uint(1)
	for i := 0; i < pGame.iSeatCount; i++ {
		uiReady *= 2
	}
	pGame.uiAllPlayerReady = uiReady - 1

	// 遊戲說明
	pGame.ary_strPokerManual = append(pGame.ary_strPokerManual, "庄家和每位下注的客人会获发一张牌，单凭比较庄家和玩家的点数大小分胜负。如果玩家的点数大于庄家，则可获相等于下注金额的赔彩。牌的花色于此游戏中无效，故当庄家和玩家的点数相同时，玩家可作以下两个选择：")
	pGame.ary_strPokerManual = append(pGame.ary_strPokerManual, "弃牌：玩家可取回下注金额的一半。")
	pGame.ary_strPokerManual = append(pGame.ary_strPokerManual, "进入赌场战争：玩家需额外再加上相等于原注的金额进入赌场战争，然后庄荷会消去三张牌，再发牌给客人，及后再消去三张牌，发牌给自己，如果庄家点数较大，玩家会输掉原注和额外加注。如玩家点数较大，则只会赢取与额外加注相等的金额。如两者点数再次相同，玩家则可赢取原注与额外加注的两份赔彩。")

	// 彩金說明
	pGame.ary_strJPManual = append(pGame.ary_strJPManual, "平手1賠10")

	pGame.i64TimeDuration = 0
	pGame.pGameTicker = time.NewTicker(100 * time.Millisecond)
	go func() {
		for {
			select {
			case <-pGame.pGameTicker.C:
				pGame.i64TimeDuration++
				pGame.gameTickerFunc()
			}
		}
	}()

	pGame.initData()

	pGame.RegisterBeforeGameStartFunc(func() {
	})
	pGame.RegisterAfterGameStartFunc(func() {
	})
	pGame.RegisterBeforeGameEndFunc(func() {
	})
	pGame.RegisterAfterGameEndFunc(func() {
	})

	return pGame
}

func (pGame *Logic) initData() {
	mlog.Debug("初始化遊戲資料")

	for i := 0; i < pGame.iSeatCount; i++ {
		pGame.ary_pUserData[i].ResetGameData()
	}
	pGame.pDealerCard.SetCardSet("")
	pGame.i32DealerCardPoint = 0
	pGame.bGameOver = false
	pGame.pTestDealerCardBox.SetCardSet(pGame.pTestDealerCards.CodeString())

	pGame.iUserCount = 0
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pGameUser := pGame.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			pGame.ary_pUserData[iSeat].Init()
		} else {
			if pGame.ary_pUserData[iSeat].iUserState != UserState_EmptySeat {
				pGame.ary_pUserData[iSeat].iUserState = UserState_Normal
			}
			pGame.UserReady(pGameUser)
			pGame.iUserCount++
		}

		pGame.ary_pTestUserCardBox[iSeat].SetCardSet(pGame.ary_pTestUserCards[iSeat].CodeString())
	}

	if pGame.iUserCount <= 0 {
		pGame.setGameState(GameState_WaitingPlayer, 0)

		mlog.Debug("等待游戏开始")
		pGame.GameLog.AddGameStep("等待游戏开始")
	} else {
		pGame.setGameState(GameState_WaitingStart, pGame.map_i64Timeout[Timeout_WaitingStart])
		pGame.waitingGameBegin()
	}
	pGame.i64LastMessageTime = pGame.i64TimeDuration

	pGame.uiPlayerReady = 0
}

func (pGame *Logic) GameMessage(pGameUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pGameUser == nil || pMsg == nil {
		return
	}

	if pMsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}
	
	pUser := pGame.GetUserDataByUserID(pGameUser.UserID)
	if pUser == nil {
		return
	}

	switch pMsg.SClassID {
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_ShuffleOK): // C2S 洗牌動畫播放完成
		pGame.message_ShuffleOK(pUser)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_Bet): // C2S 玩家下注
		pGame.message_Bet(pUser, pMsg)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_StopBettingAnimationOK): // C2S 玩家停止下注動畫播放完成
		pGame.message_StopBettingAnimationOK(pUser)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_DealAnimationOK_Normal): // C2S 一般發牌動畫播放完成
		pGame.message_DealOK_Normal(pUser)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_Settlement_Normal_OK): // C2S 一般結算動畫播放完成
		pGame.message_Settlement_Normal_OK(pUser)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_WarPhaseSelection): // C2S 戰爭階段選項
		pGame.message_WarSelection(pUser, pMsg)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_WarPhaseAnimationOK): // C2S 進入戰爭階段動畫播放完成
		pGame.message_WarPhaseAnimationOK(pUser)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_DealAnimationOK_War): // C2S 戰爭階段發牌動畫播放完成
		pGame.message_DealOK_War(pUser)
	case int32(netproto.DCZZ_GameMessageClassID_DCZZ_Settlement_War_OK): // C2S 戰爭階段結算動畫播放完成
	}
}

func (pGame *Logic) PiSaiMessage(pUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pUser == nil || pMsg == nil {
		return
	}
}

// 玩家進入遊戲
func (pGame *Logic) UserEnter(pGameUser *gamelogic.GameUser, i32TableNo int32, i32SeatNo int32) {
	if pGameUser == nil {
		return
	}

	mlog.Debug("UserEnter, User[%d], Seat[%d]", pGameUser.UserID, pGameUser.SeatNo)

	pGame.DynamicGameLogicBase.UserEnter(pGameUser, i32TableNo, i32SeatNo)
	pGame.DynamicGameLogicBase.UserReady(pGameUser)
	pGame.AddUser(pGameUser)
}

// 計時器處理程序
func (pGame *Logic) gameTickerFunc() {
	if pGame.i64Timeout > 0 && pGame.i64TimeDuration - pGame.i64LastMessageTime > pGame.i64Timeout {
		switch pGame.iGameState {
		case GameState_WaitingStart: // 等待遊戲開始
			pGame.GameStartFunc()
			pGame.setGameState(GameState_Betting, pGame.map_i64Timeout[Timeout_Betting])
			pGame.startBetting(false)
		case GameState_Betting: // 押注
			pGame.setGameState(GameState_StopBetting, pGame.map_i64Timeout[Timeout_StopBettingAnimation])
			pGame.stopBetting()
		case GameState_StopBetting: // 停止下注
			if pGame.pGamePoker.CardCount() <= ShuffleRestCardCount {
				pGame.setGameState(GameState_Shuffle, pGame.map_i64Timeout[Timeout_Shuffle])
				pGame.shuffle(false)
			} else {
				pGame.setGameState(GameState_Deal_Normal, pGame.map_i64Timeout[Timeout_Deal_Normal])
				pGame.deal_Normal()
			}
		case GameState_Shuffle: // 洗牌
			pGame.setGameState(GameState_Deal_Normal, pGame.map_i64Timeout[Timeout_Deal_Normal])
			pGame.deal_Normal()
		case GameState_Deal_Normal: // 一般發牌
			pGame.setGameState(GameState_Settlement_Normal, pGame.map_i64Timeout[Timeout_Settlement_Normal])
			pGame.settlement_Normal()
		case GameState_Settlement_Normal: // 一般結算
			if pGame.bGameOver {
				// 遊戲結束
				pGame.sendGameOver()
			} else {
				// 進入戰爭階段
				pGame.setGameState(GameState_WarSelection, pGame.map_i64Timeout[Timeout_WarSelection])
				pGame.enterWarPhase()
			}
		case GameState_WarSelection: // 選擇是否進入戰爭
			for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
				pUser := pGame.ary_pUserData[iSeat]
				if pUser.iUserState == UserState_Normal && !pUser.bOperationDone {
					if !pUser.bGoForWar {
						pGame.userAction_WarSelection(pUser, WarSelection_Fold)
					}
				}
			}
		case GameState_WarPhaseAnimation: // 進入戰爭階段動畫
			pGame.setGameState(GameState_Deal_War, pGame.map_i64Timeout[Timeout_Deal_War])
			pGame.deal_War()
		case GameState_Deal_War: // 戰爭發牌
			pGame.setGameState(GameState_Settlement_War, pGame.map_i64Timeout[Timeout_Settlement_War])
			pGame.settlement_War()
		case GameState_Settlement_War: // 戰爭結算
			pGame.setGameState(GameState_GameOver, 0)
			pGame.gameOver()
		}
	}

	// 機器人動作
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pUser := pGame.ary_pUserData[iSeat]
		if pUser.iUserType == gamelogic.UserType_Rebot && pUser.iUserState == UserState_Normal {
			if pUser.i64Timeout > 0 && pGame.i64TimeDuration - pUser.i64LastMessageTime > pUser.i64Timeout {
				switch pGame.iGameState {
				case GameState_Betting: // 押注
					// 下注類別
					iBetType := util.RndInt(0, BetType_Max)
					if pUser.pRobot.ary_pBetParam[iBetType].iBetTimes >= pUser.pRobot.ary_pBetParam[iBetType].iBetCount {
						pUser.i64Timeout = 5
						pUser.i64LastMessageTime = pGame.i64TimeDuration
					} else {
						// 可下注籌碼
						ary_i64Chip := make([]int64, 0)
						for i := 0; i < len(pGame.ary_i32Chip); i++ {
							if i < pUser.pRobot.ary_pBetParam[iBetType].iBetRange_Min - 1 || i > pUser.pRobot.ary_pBetParam[iBetType].iBetRange_Max - 1 {
								continue
							}

							if pUser.i64Money >= int64(pGame.ary_i32Chip[i]) {
								ary_i64Chip = append(ary_i64Chip, int64(pGame.ary_i32Chip[i]))
							}
						}

						if len(ary_i64Chip) > 0 {
							iBetIdx := util.RndInt(0, len(ary_i64Chip))
							iResult := pGame.userAction_Bet(pUser, iBetType, ary_i64Chip[iBetIdx])
							if iResult == BetResult_Success {
								pGame.sendBetResult(pUser, iBetType, ary_i64Chip[iBetIdx], iResult)
							}
						}

						pUser.pRobot.ary_pBetParam[iBetType].iBetTimes++

						if pUser.pRobot.ary_pBetParam[iBetType].iBetTimes >= pUser.pRobot.ary_pBetParam[iBetType].iBetCount {
							pUser.i64Timeout = 0
						} else {
							pUser.i64Timeout = 5
							pUser.i64LastMessageTime = pGame.i64TimeDuration
						}
					}
				case GameState_WarSelection: // 選擇是否進入戰爭
					if !pUser.bCanEnterWar || pUser.bIsFolded {
						continue
					}

					iRnd := util.RndInt(0,100)
					if iRnd < pUser.pRobot.iRate_FoldAtWar {
						pGame.userAction_WarSelection(pUser, WarSelection_Fold)
					} else {
						pGame.userAction_WarSelection(pUser, WarSelection_War)
					}

					pUser.i64Timeout = 0
				}
			}
		}
	}
}

// 讀取遊戲設定
func (pGame *Logic) getGameConfig(configs *config.ConfigGroup) {
	if configs == nil {
		mlog.Error("桌號[%d] 讀取 config 失敗", pGame.TableNo())
		return
	}

	var str string = ""

	// 讀取 gameserver 設定值
	cfgs := configs.GetConfig("gameserver")
	if cfgs == nil {
		mlog.Error("獲取配置 [gameserver] 失敗")
	} else {
		pGame.iSeatCount = cfgs.GetConfigInt("gameserver", "seatcount")

		//----- timeout -----
		pGame.map_i64Timeout[Timeout_WaitingStart] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_WaitingStart")) * 10
		pGame.map_i64Timeout[Timeout_Betting] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Betting")) * 10
		pGame.map_i64Timeout[Timeout_StopBettingAnimation] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_StopBettingAnimation")) * 10
		pGame.map_i64Timeout[Timeout_Shuffle] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Shuffle")) * 10
		pGame.map_i64Timeout[Timeout_Deal_Normal] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Deal_Normal")) * 10
		pGame.map_i64Timeout[Timeout_Settlement_Normal] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Settlement_Normal")) * 10
		pGame.map_i64Timeout[Timeout_WarSelection] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_WarSelection")) * 10
		pGame.map_i64Timeout[Timeout_WarPhaseBegin] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_WarPhaseBegin")) * 10
		pGame.map_i64Timeout[Timeout_Deal_War] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Deal_War")) * 10
		pGame.map_i64Timeout[Timeout_Settlement_War] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Settlement_War")) * 10

		// 籌碼面額
		str = cfgs.GetConfigValue("GameSetting", "ChipValue")
		ary_strChip := strings.Split(str, ",")
		for i := 0; i < len(ary_strChip); i++ {
			i64chip, _ := strconv.ParseInt(ary_strChip[i], 10, 32)
			pGame.ary_i32Chip = append(pGame.ary_i32Chip, int32(i64chip))
		}

		if len(pGame.ary_i32Chip) <= 0 {
			mlog.Fatal("讀取 GameSetting 失敗：沒有籌碼資料")
		}

		// 機器人參數
		pGame.pRobotSetting.iRate_FoldAtWar = cfgs.GetConfigInt("robot", "Rate_FoldAtWar")	// 進入戰爭後棄牌機率

		// 下注參數
		// 底注
		var i64Param int64
		str = cfgs.GetConfigValue("robot", "BetRange_Base")
		ary_strSetting := strings.Split(str, ",")
		if len(ary_strSetting) < 3 {
			mlog.Error("[robot] 参数 BetRange_Base 设定错误：%s", str)
		} else {
			i64Param, _ = strconv.ParseInt(ary_strSetting[0], 10, 32)
			pGame.pRobotSetting.ary_pBetParam[BetType_Base].iBetRange_Min = int(i64Param)
			i64Param, _ = strconv.ParseInt(ary_strSetting[1], 10, 32)
			pGame.pRobotSetting.ary_pBetParam[BetType_Base].iBetRange_Max = int(i64Param)
			i64Param, _ = strconv.ParseInt(ary_strSetting[2], 10, 32)
			pGame.pRobotSetting.ary_pBetParam[BetType_Base].iDefaultBetCount = int(i64Param)
		}

		// 平手注
		str = cfgs.GetConfigValue("robot", "BetRange_Side")
		ary_strSetting = strings.Split(str, ",")
		if len(ary_strSetting) < 3 {
			mlog.Error("[robot] 参数 BetRange_Side 设定错误：%s", str)
		} else {
			i64Param, _ = strconv.ParseInt(ary_strSetting[0], 10, 32)
			pGame.pRobotSetting.ary_pBetParam[BetType_Side].iBetRange_Min = int(i64Param)
			i64Param, _ = strconv.ParseInt(ary_strSetting[1], 10, 32)
			pGame.pRobotSetting.ary_pBetParam[BetType_Side].iBetRange_Max = int(i64Param)
			i64Param, _ = strconv.ParseInt(ary_strSetting[2], 10, 32)
			pGame.pRobotSetting.ary_pBetParam[BetType_Side].iDefaultBetCount = int(i64Param)
		}
	}

	// 發牌器
	pkcfgs := configs.GetConfig("dealingdevice")
	if pkcfgs == nil {
		mlog.Error("獲取配置 [dealingdevice] 失敗")
	} else {
		// 發牌方式
		iDealType := pkcfgs.GetConfigInt("poker","DealType")
		if iDealType < DealType_Natural || iDealType >= DealType_Max {
			iDealType = DealType_Natural
		}
		pGame.iDealType = iDealType

		// 測試用莊家手牌
		strCards := pkcfgs.GetConfigValue("poker", "Dealer")
		pGame.pTestDealerCards = Pokerset.NewPokerCardSet()
		pGame.pTestDealerCards.SetCardSet(strCards)
		mlog.Debug("測試用莊家手牌：%s", pGame.pTestDealerCards.CodeString())

		// 測試用玩家手牌
		pGame.ary_pTestUserCards = make([]*Pokerset.PokerCardSet, pGame.iSeatCount)
		for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
			str = fmt.Sprintf("User%d", iSeat+1)
			strCards := pkcfgs.GetConfigValue("poker", str)
			pGame.ary_pTestUserCards[iSeat] = Pokerset.NewPokerCardSet()
			pGame.ary_pTestUserCards[iSeat].SetCardSet(strCards)
			mlog.Debug("測試用玩家手牌 %d：%s", iSeat + 1, pGame.ary_pTestUserCards[iSeat].CodeString())
		}
	}
}

// 加入新玩家
func (pGame *Logic) AddUser(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}

	pUser := pGame.GetUserDataByUserID(pGameUser.UserID)
	if pUser == nil {
		return
	}

	if pUser.iUserState == UserState_EmptySeat {
		pUser.Init()
		pUser.User = pGameUser
		pUser.i32UserID = pGameUser.UserID
		pUser.i32SeatNo = pGameUser.SeatNo
		if pGameUser.IsRobot() {
			pUser.iUserType = gamelogic.UserType_Rebot
			pUser.pRobot.Copy(pGame.pRobotSetting)
		}
		pUser.i64Money = pGameUser.Score
		pUser.ResetGameData()

		if pGame.iGameState >= GameState_Betting && pGame.iGameState < GameState_GameOver {
			pUser.iUserState = UserState_Watch
		} else {
			pUser.iUserState = UserState_Normal
		}
	}
	pGame.iUserCount++

	if pGame.iUserCount > 0 && pGame.iGameState == GameState_WaitingPlayer {
		pGame.setGameState(GameState_WaitingStart, pGame.map_i64Timeout[Timeout_WaitingStart])
		pGame.waitingGameBegin()
	}
	pGame.sendGameInfo(pUser)
	pGame.sendAllUserInfo()

	example.invalid("加入新玩家[%d], 遊戲狀態[%s], 座位[%d], 類別[%d], 玩家狀態[%s], 玩家總人數[%d]", pUser.i32UserID,
		g_map_strGameState_debug[pGame.iGameState], pUser.i32SeatNo, pUser.iUserType,
		g_map_strUserState_debug[pUser.iUserState], pGame.iUserCount)
	pGame.GameLog.AddGameStep("玩家[%d] 入座，状态：%s", pGameUser.UserID, g_map_strUserState[pUser.iUserState])
}

// 遊戲狀態 log - debug 用
func (pGame *Logic) gameStateLog_Debug() {
	mlog.Debug("【state】[%s], timeout[%d], duration[%d], lastmsg[%d]", g_map_strGameState_debug[pGame.iGameState],
		pGame.i64Timeout, pGame.i64TimeDuration, pGame.i64LastMessageTime)
}

// 等待遊戲開始
func (pGame *Logic) waitingGameBegin() {
	pGame.GameLog.GameBegin()
	mlog.Debug("等待开始下注")
	pGame.GameLog.AddGameStep("等待开始下注")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_WaitingStart)
	pData := new(netproto.JDESYD_Msg_WaitingStart)
	pData.TimeRemain = proto.Int32(int32((pGame.i64Timeout - pGame.i64TimeDuration + pGame.i64LastMessageTime) / 10))	// 剩餘時間

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)
}

// 依 UserID 取得 UserData
func (pGame *Logic) GetUserDataByUserID(i32UserID int32) *User {
	iSeatNo := -1
	
	for i := 0; i < pGame.iSeatCount; i++ {
		if i32UserID == pGame.ary_pUserData[i].i32UserID {
			iSeatNo = i
			break
		}
	}
	
	if iSeatNo >= 0 && iSeatNo < pGame.iSeatCount {
		return pGame.ary_pUserData[iSeatNo]
	} else {
		return nil
	}
}

// 傳送牌局資訊
func (pGame *Logic) sendGameInfo(pUser *User) {
	if pUser == nil {
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_GameInfo)

	pData := new(netproto.DCZZ_Msg_GameInfo)
	pData.TimeRemain = proto.Int32(int32((pGame.i64Timeout - pGame.i64TimeDuration + pGame.i64LastMessageTime) / 10))	// 剩餘時間
	// 遊戲說明
	for i := 0; i < len(pGame.ary_strPokerManual); i++ {
		pData.PokerManual = append(pData.PokerManual, pGame.ary_strPokerManual[i])
	}
	// 籌碼面額
	for i := 0; i < len(pGame.ary_i32Chip); i++ {
		pData.ChipValue = append(pData.ChipValue, pGame.ary_i32Chip[i])
	}
	pData.DealerCards = pGame.cardSetPkg(pGame.pDealerCard)	// 莊家手牌
	// 所有玩家資訊
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}
	pData.BetBaseLimit = proto.Int64(g_i64BetLimit_Base)	// 底注額上限
	pData.BetSideLimit = proto.Int64(g_i64BetLimit_Side)	// 平手額上限

	pMsg.MsgData = pData
	pGame.SendMessageToUser(pUser.User, pMsg)
}

// 手牌牌組打包資料
func (pGame *Logic) cardSetPkg(pCardSet *Pokerset.PokerCardSet) []*netproto.PokerCard {
	if pCardSet == nil {
		return nil
	}

	pRetSet := make([]*netproto.PokerCard, 0)

	// 牌組
	for iIdx := 0; iIdx < pCardSet.CardCount(); iIdx++ {
		pCard := new(netproto.PokerCard)
		pSCard := pCardSet.GetSingleCard(iIdx, true)
		if pSCard != nil {
			pCard.Color = proto.Int32(pSCard.I32Color)
			pCard.CardNumber = proto.Int32(pSCard.I32Number)
		}

		pRetSet = append(pRetSet, pCard)
	}

	return pRetSet
}

// 玩家資訊打包資料，iSeat：此座位的玩家資訊，iSendTo：要送給哪位玩家
func (pGame *Logic) userInfoPkg(iSeat int) *netproto.DCZZ_Msg_UserInfo {
	if iSeat < 0 || iSeat >= pGame.iSeatCount {
		return nil
	}

	pUser := pGame.ary_pUserData[iSeat]

	pUserInfo := new(netproto.DCZZ_Msg_UserInfo)

	pUserInfo.UserID = proto.Int32(pUser.i32UserID)	// 玩家ID，若是空位時，玩家ID = -1
	pUserInfo.SeatNo = proto.Int32(int32(iSeat + IndexBase))	// 玩家座位，client 座位號從 1 開始
	pUserInfo.State = proto.Int32(int32(pUser.iUserState))	// 玩家狀態，0:空位，1:正常，2:觀戰，3:棄牌
	pUserInfo.Money = proto.Int64(pUser.i64Money)// 玩家金錢
	pUserInfo.HandCards = pGame.cardSetPkg(pUser.pHandCard)	// 玩家手牌
	pUserInfo.Bet_Base = proto.Int64(pUser.i64Bet_Base)	// 玩家底注
	pUserInfo.Bet_Side = proto.Int64(pUser.i64Bet_Side)	// 玩家平手注
	pUserInfo.Bet_Double = proto.Int64(pUser.i64Bet_Double)	// 玩家戰爭階段加注
	pUserInfo.TotalWinLose = proto.Int64(pUser.i64TotalWinLose) // 總輸贏
	pUserInfo.IsTied = proto.Bool(pUser.bIsTied)	// 是否跟莊家平手
	pUserInfo.GetTieAward = proto.Bool(pUser.bGetTieAward)	// 玩家是否於一般階段贏得平手賠付
	pUserInfo.CanEnterWar = proto.Bool(pUser.bCanEnterWar)	// 可否進入戰爭階段
	pUserInfo.IsFolded = proto.Bool(pUser.bIsFolded)	// 玩家是否已棄牌
	pUserInfo.FoldRecover = proto.Int64(pUser.i64FoldRecover)	// 玩家於戰爭階段選擇棄牌收回的注碼

	return pUserInfo
}

// 傳送玩家資訊
func (pGame *Logic) sendAllUserInfo() {
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_AllUserInfo)

	pData := new(netproto.DCZZ_Msg_AllUserInfo)
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)
}

// 開始下注
func (pGame *Logic) startBetting(bUserRecover bool) {
	mlog.Debug("開始下注")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_StartBetting)
	pData := new(netproto.DCZZ_Msg_StartBetting)
	pData.TimeRemain = proto.Int32(int32((pGame.i64Timeout - pGame.i64TimeDuration + pGame.i64LastMessageTime) / 10))	// 剩餘時間

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)

	pGame.GameLog.AddGameStep("开始下注")

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pUser := pGame.ary_pUserData[iSeat]
		if pUser.iUserState == UserState_Normal {
			pUser.iPlayingState = UserPlayState_Betting
			// 自動幫玩家下底注
			mlog.Debug("玩家[%d] 金钱 %s", pUser.i32UserID, Global.MoneyString(pUser.i64Money))
			pGame.GameLog.AddGameStep("玩家[%d] 金钱 %s", pUser.i32UserID, Global.MoneyString(pUser.i64Money))

			if pUser.i64Bet_Base <= 0 {
				pUser.Bet(BetType_Base, int64(pGame.ary_i32Chip[0]))
				pGame.sendBetResult(pUser, BetType_Base, int64(pGame.ary_i32Chip[0]), BetResult_Auto)
			}
		}
	}

	if !bUserRecover {
		// 機器人隨機下注
		for i := 0; i < pGame.iSeatCount; i++ {
			pUser := pGame.ary_pUserData[i]
			if pUser.iUserType == gamelogic.UserType_Rebot {
				pUser.SetRobotTimeout(pGame.map_i64Timeout[Timeout_Betting])
				if pUser.i64Timeout > 0 {
					pUser.i64LastMessageTime = pGame.i64TimeDuration

					for i := 0; i < len(pUser.pRobot.ary_pBetParam); i++ {
						pUser.pRobot.ary_pBetParam[i].iBetTimes = 0
						if pUser.pRobot.ary_pBetParam[i].iDefaultBetCount > 0 {
							pUser.pRobot.ary_pBetParam[i].iBetCount = util.RndInt(0, pUser.pRobot.ary_pBetParam[i].iDefaultBetCount+1)
						}
					}
				}
			}
		}
	}
}

// 玩家下注結果
func (pGame *Logic) sendBetResult(pUser *User, iBetType int, i64Bet int64, iResult int) {
	if pUser == nil {
		return
	}

	iBetResult := iResult
	if iResult >= BetResult_Success {
		iBetResult = BetResult_Success
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_BetResult)

	pData := new(netproto.DCZZ_Msg_BetResult)
	pData.SeatNo = proto.Int32(pUser.i32SeatNo + int32(IndexBase))	// 下注玩家座位
	pData.BetType = proto.Int32(int32(iBetType))	// 下注類別
	pData.Bet = proto.Int64(i64Bet)	// 下注金額
	pData.MoneyRemain = proto.Int64(pUser.i64Money)	// 下注後玩家剩餘金錢
	pData.Result = proto.Int32(int32(iBetResult))	// 下注結果

	pMsg.MsgData = pData
	if iBetResult == BetResult_Success {
		pGame.BoardcastMessage(pMsg)
	} else {
		pGame.SendMessageToUser(pUser.User, pMsg)
	}

	str := ""
	switch iBetType {
	case BetType_Base: // 底注
		str = fmt.Sprintf("%s总额 %s", g_map_strBetType[iBetType], Global.MoneyString(pUser.i64Bet_Base))
	case BetType_Side: // 平手注
		str = fmt.Sprintf("%s总额 %s", g_map_strBetType[iBetType], Global.MoneyString(pUser.i64Bet_Side))
	}

	if iResult == BetResult_Auto {
		mlog.Debug("玩家[%d] 自动下%s %s, %s, 剩余金钱 %s", pUser.i32UserID, g_map_strBetType[iBetType],
			Global.MoneyString(i64Bet), str, Global.MoneyString(pUser.i64Money))
		pGame.GameLog.AddGameStep("玩家[%d] 自动下%s %s, %s, 剩余金钱 %s", pUser.i32UserID, g_map_strBetType[iBetType],
			Global.MoneyString(i64Bet), str, Global.MoneyString(pUser.i64Money))
	} else {
		mlog.Debug("玩家[%d] 下%s %s, 结果：%s, %s, 剩余金钱 %s", pUser.i32UserID, g_map_strBetType[iBetType],
			Global.MoneyString(i64Bet), g_map_strBetResult[iResult], str, Global.MoneyString(pUser.i64Money))
		pGame.GameLog.AddGameStep("玩家[%d] 下%s %s, 结果：%s, %s, 剩余金钱 %s", pUser.i32UserID, g_map_strBetType[iBetType],
			Global.MoneyString(i64Bet), g_map_strBetResult[iResult], str, Global.MoneyString(pUser.i64Money))
	}
}

// 訊息處理 - 玩家下注
func (pGame *Logic) message_Bet(pUser *User, pMsg *msg.Message) {
	if pUser == nil || pMsg == nil {
		return
	}

	msgInfo := &netproto.DCZZ_Msg_Bet{}
	proto.Unmarshal(pMsg.MsgData.([]byte), msgInfo)
	iBetType := int(msgInfo.GetBetType())
	i64Bet := msgInfo.GetBet()

	if pGame.iGameState != GameState_Betting && pUser.iPlayingState != UserPlayState_Betting {
		pGame.sendBetResult(pUser, iBetType, i64Bet, BetResult_BettingStopped)
		return
	}

	iResult := pGame.userAction_Bet(pUser, iBetType, i64Bet)
	pGame.sendBetResult(pUser, iBetType, i64Bet, iResult)
}

// 玩家動作 - 下注
func (pGame *Logic) userAction_Bet(pUser *User, iBetType int, i64Bet int64) int {
	if pUser == nil {
		return BetResult_BetFailed
	}

	if iBetType < BetType_Base || iBetType >= BetType_Max {
		return BetResult_BetFailed
	}

	iResult := BetResult_Success

	// 餘額不足
	if pUser.i64Money < i64Bet {
		return BetResult_NotEnoughMoney
	}

	// 押注金額<=0
	if i64Bet <= 0 {
		return BetResult_BetSmallThanZero
	}

	// 總押注額超過上限
	switch iBetType {
	case BetType_Base: // 底注
		if pUser.i64Bet_Base + i64Bet > g_i64BetLimit_Base {
			return BetResult_OverUpperLimit
		}
	case BetType_Side: // 平手注
		if pUser.i64Bet_Side + i64Bet > g_i64BetLimit_Side {
			return BetResult_OverUpperLimit
		}
	}

	pUser.Bet(iBetType, i64Bet)

	return iResult
}

// 播放停止下注動畫
func (pGame *Logic) stopBetting() {
	mlog.Debug("停止下注")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_StopBetting)

	pGame.BoardcastMessage(pMsg)

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_StopBettingOK
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}
}

// 訊息處理 - 停止下注動畫播放完成
func (pGame *Logic) message_StopBettingAnimationOK(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_StopBetting && pUser.iPlayingState != UserPlayState_Betting {
		return
	}

	mlog.Debug("User[%d] 停止下注動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_StopBettingOK
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家停止下注動畫播放完成")
		if pGame.pGamePoker.CardCount() <= ShuffleRestCardCount {
			pGame.setGameState(GameState_Shuffle, pGame.map_i64Timeout[Timeout_Shuffle])
			pGame.shuffle(false)
		} else {
			pGame.setGameState(GameState_Deal_Normal, pGame.map_i64Timeout[Timeout_Deal_Normal])
			pGame.deal_Normal()
		}
	}
}

// 洗牌
func (pGame *Logic) shuffle(bUserRecover bool) {
	mlog.Debug("洗牌")

	// 建立新的牌組
	if !bUserRecover {
		pGame.pGamePoker.NewSet(PokerSetCount, 0)
		pGame.pGamePoker.Shuffle()
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_Shuffle)

	pGame.BoardcastMessage(pMsg)

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_ShuffleOK
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}
}

// 訊息處理 - 洗牌動畫播放完成
func (pGame *Logic) message_ShuffleOK(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_Shuffle && pUser.iPlayingState != UserPlayState_StopBettingOK {
		return
	}

	mlog.Debug("User[%d] 洗牌播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_ShuffleOK
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家洗牌動畫播放完成")
		pGame.setGameState(GameState_Deal_Normal, pGame.map_i64Timeout[Timeout_Deal_Normal])
		pGame.deal_Normal()
	}
}

// 一般發牌
func (pGame *Logic) deal_Normal() {
	// 發玩家牌
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pSet, i32Point := pGame.dealing(GamePhase_Normal, pGame.ary_pTestUserCardBox[iSeat])
		pGame.ary_pUserData[iSeat].pHandCard.AddCardSet(pSet)
		pGame.ary_pUserData[iSeat].i32Point = i32Point
	}

	// 發莊家牌
	pSet, i32Point := pGame.dealing(GamePhase_Normal, pGame.pTestDealerCardBox)
	pGame.pDealerCard.AddCardSet(pSet)
	pGame.i32DealerCardPoint = i32Point

	// 傳送訊息給 client
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_Deal_Normal)

	pData := new(netproto.DCZZ_Msg_Deal_Normal)
	pData.DealerCards = pGame.cardSetPkg(pGame.pDealerCard)	// 莊家手牌
	// 所有玩家資訊
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)

	mlog.Debug("一般发牌")
	pGame.GameLog.AddGameStep("一般发牌")
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		mlog.Debug("玩家[%d] 手牌：%s", pGame.ary_pUserData[iSeat].i32UserID, pGame.ary_pUserData[iSeat].pHandCard.String())
		pGame.GameLog.AddGameStep("玩家[%d] 手牌：%s", pGame.ary_pUserData[iSeat].i32UserID, pGame.ary_pUserData[iSeat].pHandCard.String())
	}
	mlog.Debug("庄家牌：%s", pGame.pGamePoker.String())
	pGame.GameLog.AddGameStep("庄家牌：%s", pGame.pGamePoker.String())

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_DealOK_Normal
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}
}

// 發牌，回傳［牌組］、［點數］
func (pGame *Logic) dealing(iGamePhase int, pCardBox *Pokerset.PokerCardSet) (*Pokerset.PokerCardSet, int32) {
	pRetSet := Pokerset.NewPokerCardSet()

	if iGamePhase < GamePhase_Normal || iGamePhase >= GamePhase_Max {
		return pRetSet, 0
	}

	if pCardBox == nil {
		return pRetSet, 0
	}

	iDealCount := 0
	i32Point := int32(0)

	switch iGamePhase {
	case GamePhase_Normal: // 一般階段
		iDealCount = COUNT_CardCount_Normal
		switch pGame.iDealType {
		case DealType_Assigned: // 指定發牌
			if pCardBox.CardCount() >= iDealCount {
				pSet := pCardBox.Deal(iDealCount)
				pRetSet.SetCardSet(pSet.CodeString())
			} else {
				iCount := iDealCount - pCardBox.CardCount()
				pRetSet.SetCardSet(pCardBox.CodeString())
				pSet := pGame.pGamePoker.Deal(iCount)
				pRetSet.AddCardSet(pSet)
			}
		default:
			pSet := pGame.pGamePoker.Deal(iDealCount)
			pRetSet.SetCardSet(pSet.CodeString())
		}

		pCard := pRetSet.GetSingleCard(0,true)
		i32Point = pCard.I32Number
	case GamePhase_War: // 戰爭階段
		iDealCount = COUNT_CardCount_War - COUNT_CardCount_Normal
		pSet := Pokerset.NewPokerCardSet()
		switch pGame.iDealType {
		case DealType_Assigned: // 指定發牌
			if pCardBox.CardCount() >= iDealCount {
				pTemp := pCardBox.Deal(iDealCount)
				pSet.SetCardSet(pTemp.CodeString())
			} else {
				iCount := iDealCount - pCardBox.CardCount()
				pSet.SetCardSet(pCardBox.CodeString())
				pTemp := pGame.pGamePoker.Deal(iCount)
				pSet.AddCardSet(pTemp)
			}
		default:
			pTemp := pGame.pGamePoker.Deal(iDealCount)
			pSet.SetCardSet(pTemp.CodeString())
		}

		for i := 0; i < pSet.CardCount(); i++ {
			var pCard *Pokerset.PokerCard
			if i < COUNT_HideCardCount {
				pCard = Pokerset.NewPokerCard(Pokerset.POKER_CardColor_Other, Pokerset.POKER_CardNo_Hide)
			} else {
				pCard = pSet.GetSingleCard(i,true)
				i32Point = pCard.I32Number
			}
			pRetSet.AddSingleCard(pCard.CodeString())
		}
	}

	mlog.Debug("dealing(%s), 牌組：%s, 點數[%d]", g_map_strGamePhase_debug[iGamePhase], pRetSet.String(), i32Point)

	return pRetSet, i32Point
}

// 一般發牌動畫播放完成
func (pGame *Logic) message_DealOK_Normal(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_Shuffle && pUser.iPlayingState != UserPlayState_StopBettingOK && pUser.iPlayingState != UserPlayState_ShuffleOK {
		return
	}

	mlog.Debug("User[%d] 一般發牌播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_DealOK_Normal
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家一般發牌動畫播放完成")
		pGame.setGameState(GameState_Settlement_Normal, pGame.map_i64Timeout[Timeout_Settlement_Normal])
		pGame.settlement_Normal()
	}
}

// 一般結算
func (pGame *Logic) settlement_Normal() {
	mlog.Debug("一般阶段结算")
	pGame.GameLog.AddGameStep("一般阶段结算")
	mlog.Debug("庄家牌：%s", pGame.pGamePoker.String())
	pGame.GameLog.AddGameStep("庄家牌：%s", pGame.pGamePoker.String())

	strWinLose := ""
	// 一般階段結算
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pUser := pGame.ary_pUserData[iSeat]
		if pUser.iUserState == UserState_Normal && !pUser.bOperationDone {
			i64WinLose, i64MoneyChange, i64Tax := int64(0), int64(0), int64(0)
			if pUser.i32Point > pGame.i32DealerCardPoint {
				// 玩家贏
				i64MoneyChange, i64Tax = pGame.GetUserWinAndTax(pUser.i64Bet_Base * Rate_Normal)
				i64MoneyChange -= pUser.i64Bet_Side
				i64WinLose = pUser.i64Bet_Base + i64MoneyChange
				pUser.bOperationDone = true
				strWinLose = "赢"
			} else if pUser.i32Point < pGame.i32DealerCardPoint {
				// 莊家贏
				i64MoneyChange = -(pUser.i64Bet_Base + pUser.i64Bet_Side)
				i64WinLose = i64MoneyChange
				pUser.bOperationDone = true
				strWinLose = "输"
			} else {
				// 平手
				i64WinLose = pUser.i64Bet_Side + pUser.i64Bet_Side * Rate_TieAward
				i64MoneyChange = i64WinLose - pUser.i64Bet_Side
				pUser.bIsTied = true
				pUser.bGetTieAward = true
				pUser.bCanEnterWar = true
			}

			pUser.i64TotalWinLose = i64WinLose
			pUser.i64MoneyChange += i64MoneyChange
			pUser.i64Money += pUser.i64TotalWinLose

			// 寫分
			if !pUser.bOperationDone {
				// 玩家與莊家平手
				mlog.Debug("玩家[%d] 与庄家平手，得到平手赔付 %s，剩余金额 %s，可进入战争阶段", pUser.i32UserID,
					Global.MoneyString(i64WinLose - pUser.i64Bet_Side), Global.MoneyString(pUser.i64Money))
				pGame.GameLog.AddGameStep("玩家[%d]与庄家平手，得到平手赔付 %s，剩余金额 %s，可进入战争阶段", pUser.i32UserID,
					Global.MoneyString(i64WinLose - pUser.i64Bet_Side), Global.MoneyString(pUser.i64Money))
			} else {
				// 玩家輸贏已定，寫分
				i64Flow := int64(0)
				if i64MoneyChange > 0 {
					i64Flow = i64MoneyChange
				} else {
					i64Flow = -i64MoneyChange
				}
				if pUser.iUserType != gamelogic.UserType_Rebot {
					pGame.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax, 0)
				}

				mlog.Debug("玩家[%d] %s 底注 %s，平手注 %s，输赢 %s，税收 %s，流水 %s，剩余金额 %s", pUser.i32UserID, strWinLose,
					Global.MoneyString(pUser.i64Bet_Base), Global.MoneyString(pUser.i64Bet_Side), Global.MoneyString(i64WinLose),
					Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
				pGame.GameLog.AddGameStep("玩家[%d]%s 底注 %s，平手注 %s，输赢 %s，税收 %s，流水 %s，剩余金额 %s", pUser.i32UserID, strWinLose,
					Global.MoneyString(pUser.i64Bet_Base), Global.MoneyString(pUser.i64Bet_Side), Global.MoneyString(i64WinLose),
					Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			}
		}
	}

	// 傳送訊息給 client
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_Settlement_Normal)

	pData := new(netproto.DCZZ_Msg_Settlement_Normal)
	// 所有玩家資訊
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_Settlement_Normal_OK
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}

	pGame.checkIfGameOver()
}

// 一般結算動畫播放完成
func (pGame *Logic) message_Settlement_Normal_OK(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_Settlement_Normal && pUser.iPlayingState != UserPlayState_DealOK_Normal {
		return
	}

	mlog.Debug("User[%d] 一般結算播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_Settlement_Normal_OK
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家一般結算動畫播放完成")
		if pGame.bGameOver {
			// 遊戲結束
			pGame.sendGameOver()
		} else {
			// 進入戰爭階段
			pGame.setGameState(GameState_WarSelection, pGame.map_i64Timeout[Timeout_WarSelection])
			pGame.enterWarPhase()
		}
	}
}

// 檢查遊戲是否已結束
func (pGame *Logic) checkIfGameOver() {
	bGameOver := true

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_Normal {
			if !pGame.ary_pUserData[iSeat].bOperationDone {
				bGameOver = false
				break
			}
		}
	}

	pGame.bGameOver = bGameOver
}

// 本局結束
func (pGame *Logic) sendGameOver() {
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_GameOver)

	pGame.BoardcastMessage(pMsg)

	pGame.setGameState(GameState_GameOver, 0)
	pGame.gameOver()
}

// 進入戰爭階段
func (pGame *Logic) enterWarPhase() {
	mlog.Debug("進入戰爭階段")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_WarPhase)

	pData := new(netproto.DCZZ_Msg_WarPhase)
	// 所有玩家資訊
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}
	pData.TimeRemain = proto.Int32(int32((pGame.i64Timeout - pGame.i64TimeDuration + pGame.i64LastMessageTime) / 10))	// 剩餘時間

	pGame.BoardcastMessage(pMsg)

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_Normal {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_WarSelection
		}
	}
}

// 戰爭階段選項
func (pGame *Logic) message_WarSelection(pUser *User, pMsg *msg.Message) {
	if pUser == nil || pMsg == nil {
		return
	}

	if !pUser.bCanEnterWar || pUser.bIsFolded {
		return
	}

	if pGame.iGameState != GameState_WarSelection && pUser.iPlayingState != UserPlayState_WarSelection {
		return
	}

	msgInfo := &netproto.DCZZ_Msg_WarPhaseSelection{}
	proto.Unmarshal(pMsg.MsgData.([]byte), msgInfo)
	iSelection := int(msgInfo.GetSelection())

	pGame.userAction_WarSelection(pUser, iSelection)
}

// 玩家動作 - 戰爭階段選項
func (pGame *Logic) userAction_WarSelection(pUser *User, iSelection int) {
	if pUser == nil {
		return
	}

	if iSelection < WarSelection_Fold || iSelection >= WarSelection_Max {
		return
	}

	switch iSelection {
	case WarSelection_Fold: // 棄牌
		i64WinLose, i64MoneyChange, i64Tax := int64(0), int64(0), int64(0)
		i64WinLose = pUser.i64Bet_Base / 2
		i64MoneyChange = -i64WinLose

		pUser.bIsFolded = true
		pUser.bOperationDone = true

		pUser.i64TotalWinLose = i64WinLose
		pUser.i64MoneyChange += i64MoneyChange
		pUser.i64Money += pUser.i64TotalWinLose

		// 寫分
		// 玩家輸贏已定，寫分
		i64Flow := int64(0)
		if i64MoneyChange > 0 {
			i64Flow = i64MoneyChange
		} else {
			i64Flow = -i64MoneyChange
		}

		if pUser.iUserType != gamelogic.UserType_Rebot {
			pGame.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax, 0)
		}

		mlog.Debug("玩家[%d] 弃牌，收回 %s，流水 %s，剩余金钱 %s", pUser.i32UserID, Global.MoneyString(i64WinLose),
			Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
		pGame.GameLog.AddGameStep("玩家[%d] 弃牌，收回 %s，流水 %s，剩余金钱 %s", pUser.i32UserID, Global.MoneyString(i64WinLose),
			Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
	case WarSelection_War: // 戰爭
		pUser.bGoForWar = true
		pUser.i64Bet_Double = pUser.i64Bet_Base * 2
		pUser.i64Money -= pUser.i64Bet_Double

		mlog.Debug("玩家[%d] 加入战争，加倍 %s，剩余金钱 %s", pUser.i32UserID,
			Global.MoneyString(pUser.i64Bet_Double), Global.MoneyString(pUser.i64Money))
		pGame.GameLog.AddGameStep("玩家[%d] 加入战争，加倍 %s，剩余金钱 %s", pUser.i32UserID,
			Global.MoneyString(pUser.i64Bet_Double), Global.MoneyString(pUser.i64Money))
	}

	pGame.sendWarSelectionResult(pUser, iSelection)

	pGame.checkIfGameOver()
	if pGame.bGameOver {
		// 遊戲結束
		pGame.sendGameOver()
	} else {
		// 開始戰爭階段
		pGame.setGameState(GameState_WarPhaseAnimation, pGame.map_i64Timeout[Timeout_WarPhaseBegin])
		pGame.warPhaseBegin()
	}
}

// 戰爭階段選擇結果
func (pGame *Logic) sendWarSelectionResult(pUser *User, iSelection int) {
	if pUser == nil {
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_WarPhaseselectionResult)

	pData := new(netproto.DCZZ_Msg_WarPhaseselectionResult)
	pData.SeatNo = proto.Int32(pUser.i32SeatNo + int32(IndexBase)) // 做選擇的玩家
	pData.Selection = proto.Int32(int32(iSelection))               // 玩家選擇

	pGame.BoardcastMessage(pMsg)
}

// 開始戰爭階段
func (pGame *Logic) warPhaseBegin() {
	// 傳送訊息給 client
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_WarPhaseBegin)

	pGame.BoardcastMessage(pMsg)

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_WarSelectionOK
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}
}

// 進入戰爭階段動畫播放完成
func (pGame *Logic) message_WarPhaseAnimationOK(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_WarPhaseAnimation && pUser.iPlayingState != UserPlayState_WarSelection {
		return
	}

	mlog.Debug("User[%d] 進入戰爭階段動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_WarSelectionOK
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家進入戰爭階段動畫播放完成")
		pGame.setGameState(GameState_Deal_War, pGame.map_i64Timeout[Timeout_Deal_War])
		pGame.deal_War()
	}
}

// 戰爭發牌
func (pGame *Logic) deal_War() {
	// 發玩家牌
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pSet, i32Point := pGame.dealing(GamePhase_War, pGame.ary_pTestUserCardBox[iSeat])
		pGame.ary_pUserData[iSeat].pHandCard.AddCardSet(pSet)
		pGame.ary_pUserData[iSeat].i32Point = i32Point
	}

	// 發莊家牌
	pSet, i32Point := pGame.dealing(GamePhase_War, pGame.pTestDealerCardBox)
	pGame.pDealerCard.AddCardSet(pSet)
	pGame.i32DealerCardPoint = i32Point

	// 傳送訊息給 client
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_Deal_War)

	pData := new(netproto.DCZZ_Msg_Deal_War)
	pData.DealerCards = pGame.cardSetPkg(pGame.pDealerCard)	// 莊家手牌
	// 所有玩家資訊
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)

	mlog.Debug("战争发牌")
	pGame.GameLog.AddGameStep("战争发牌")
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		mlog.Debug("玩家[%d] 手牌：%s", pGame.ary_pUserData[iSeat].i32UserID, pGame.ary_pUserData[iSeat].pHandCard.String())
		pGame.GameLog.AddGameStep("玩家[%d] 手牌：%s", pGame.ary_pUserData[iSeat].i32UserID, pGame.ary_pUserData[iSeat].pHandCard.String())
	}
	mlog.Debug("庄家牌：%s", pGame.pGamePoker.String())
	pGame.GameLog.AddGameStep("庄家牌：%s", pGame.pGamePoker.String())

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_DealOK_War
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}
}

// 戰爭發牌動畫播放完成
func (pGame *Logic) message_DealOK_War(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_Deal_War && pUser.iPlayingState != UserPlayState_WarSelectionOK {
		return
	}

	mlog.Debug("User[%d] 戰爭發牌播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_DealOK_War
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家戰爭發牌動畫播放完成")
		//pGame.settlement_Normal()
	}
}

// 戰爭結算
func (pGame *Logic) settlement_War() {
	mlog.Debug("战争阶段结算")
	pGame.GameLog.AddGameStep("战争阶段结算")
	mlog.Debug("庄家牌：%s", pGame.pGamePoker.String())
	pGame.GameLog.AddGameStep("庄家牌：%s", pGame.pGamePoker.String())

	strWinLose := ""
	// 戰爭階段結算
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pUser := pGame.ary_pUserData[iSeat]
		if pUser.iUserState == UserState_Normal && !pUser.bOperationDone {
			i64WinLose, i64MoneyChange, i64Tax := int64(0), int64(0), int64(0)
			if pUser.i32Point > pGame.i32DealerCardPoint {
				// 玩家贏
				i64MoneyChange, i64Tax = pGame.GetUserWinAndTax(pUser.i64Bet_Double * Rate_War)
				i64WinLose = pUser.i64Bet_Base + pUser.i64Bet_Double + i64MoneyChange
				pUser.bOperationDone = true
				strWinLose = "赢"
			} else if pUser.i32Point < pGame.i32DealerCardPoint {
				// 莊家贏
				i64MoneyChange = -(pUser.i64Bet_Base + pUser.i64Bet_Double)
				i64WinLose = i64MoneyChange
				pUser.bOperationDone = true
				strWinLose = "输"
			} else {
				// 平手
				i64MoneyChange, i64Tax = pGame.GetUserWinAndTax((pUser.i64Bet_Double + pUser.i64Bet_Base) * Rate_War)
				i64WinLose = pUser.i64Bet_Double + pUser.i64Bet_Base + i64MoneyChange
				pUser.bOperationDone = true
			}

			pUser.i64TotalWinLose = i64WinLose
			pUser.i64MoneyChange += i64MoneyChange
			pUser.i64Money += pUser.i64TotalWinLose

			// 寫分
			if !pUser.bOperationDone {
				// 玩家與莊家平手
				mlog.Debug("玩家[%d] 与庄家平手，得到平手赔付 %s，剩余金额 %s，可进入战争阶段", pUser.i32UserID,
					Global.MoneyString(i64WinLose - pUser.i64Bet_Side), Global.MoneyString(pUser.i64Money))
				pGame.GameLog.AddGameStep("玩家[%d]与庄家平手，得到平手赔付 %s，剩余金额 %s，可进入战争阶段", pUser.i32UserID,
					Global.MoneyString(i64WinLose - pUser.i64Bet_Side), Global.MoneyString(pUser.i64Money))
			} else {
				// 玩家輸贏已定，寫分
				i64Flow := int64(0)
				if i64MoneyChange > 0 {
					i64Flow = i64MoneyChange
				} else {
					i64Flow = -i64MoneyChange
				}
				if pUser.iUserType != gamelogic.UserType_Rebot {
					pGame.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax, 0)
				}

				mlog.Debug("玩家[%d] %s 底注 %s，平手注 %s，输赢 %s，税收 %s，流水 %s，剩余金额 %s", pUser.i32UserID, strWinLose,
					Global.MoneyString(pUser.i64Bet_Base), Global.MoneyString(pUser.i64Bet_Side), Global.MoneyString(i64WinLose),
					Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
				pGame.GameLog.AddGameStep("玩家[%d]%s 底注 %s，平手注 %s，输赢 %s，税收 %s，流水 %s，剩余金额 %s", pUser.i32UserID, strWinLose,
					Global.MoneyString(pUser.i64Bet_Base), Global.MoneyString(pUser.i64Bet_Side), Global.MoneyString(i64WinLose),
					Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			}
		}
	}

	// 傳送訊息給 client
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.DCZZ_GameMessageClassID_DCZZ_Settlement_War)

	pData := new(netproto.DCZZ_Msg_Settlement_War)
	// 所有玩家資訊
	for i := 0; i < pGame.iSeatCount; i++ {
		pUserInfo := pGame.userInfoPkg(i)
		pData.AllUserInfo = append(pData.AllUserInfo, pUserInfo)
	}

	pMsg.MsgData = pData
	pGame.BoardcastMessage(pMsg)

	pGame.uiPlayerReady = 0

	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		if pGame.ary_pUserData[iSeat].iUserState == UserState_EmptySeat || pGame.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pGame.ary_pUserData[iSeat].bOffline {
			pGame.ary_pUserData[iSeat].iPlayingState = UserPlayState_Settlement_War_OK
			pGame.uiPlayerReady |= 1 << uint(iSeat)
		}
	}
}

// 戰爭結算動畫播放完成
func (pGame *Logic) message_Settlement_War_OK(pUser *User) {
	if pUser == nil {
		return
	}

	if pGame.iGameState != GameState_Settlement_War && pUser.iPlayingState != UserPlayState_DealOK_War {
		return
	}

	mlog.Debug("User[%d] 戰爭結算播放完成", pUser.i32UserID)

	pUser.iPlayingState = UserPlayState_Settlement_War_OK
	pGame.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pGame.uiPlayerReady == pGame.uiAllPlayerReady {
		mlog.Debug("所有玩家戰爭結算動畫播放完成")
		pGame.sendGameOver()
	}
}

// 遊戲結束
func (pGame *Logic) gameOver() {
	mlog.Debug("游戏结束")
	pGame.GameLog.AddGameStep("游戏结束")

	map_i64ScoreRet := make(map[int32]int64)
	map_i64CouponRet := make(map[int32]int64)
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pUser := pGame.ary_pUserData[iSeat]
		if pUser.User != nil {
			if pUser.i64MoneyChange >= 0 {
				pUser.User.FlowAmount += pUser.i64MoneyChange
			} else {
				pUser.User.FlowAmount += -pUser.i64MoneyChange
			}

			map_i64ScoreRet[pUser.i32UserID] = pUser.i64MoneyChange
			map_i64CouponRet[pUser.i32UserID] = 0
		}
	}
	pGame.GameLog.GameEnd(map_i64ScoreRet, map_i64CouponRet)

	pGame.GameEndFunc()

	iUserCount := 0
	for iSeat := 0; iSeat < pGame.iSeatCount; iSeat++ {
		pGameUser := pGame.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			pGame.ary_pUserData[iSeat].Init()
		} else {
			if pGameUser.IsOffLine() {
				pGame.UserLeave(pGameUser)
				pGame.ary_pUserData[iSeat].Init()
			} else {
				i32LoginMoney := pGame.GetTable().Room.LoginMoney
				if pGame.ary_pUserData[iSeat].i64Money < int64(i32LoginMoney) {
					pGame.GameLogicBase.KickUserFunc(pGameUser.UserID, "您的金币不足!请补充金币后再进行游戏.")
					pGame.ary_pUserData[iSeat].Init()
				} else {
					pGame.ary_pUserData[iSeat].ResetGameData()
					pGame.ary_pUserData[iSeat].iUserState = UserState_Normal
					iUserCount++
				}
			}
		}
	}
	pGame.iUserCount = iUserCount
	mlog.Debug("gameOver, 玩家人數：%d", pGame.iUserCount)

	pGame.initData()
	pGame.sendAllUserInfo()
}

// 玩家離線
func (pGame *Logic) UserLeave(pGameUser *gamelogic.GameUser) bool {
	if pGameUser == nil {
		return false
	}

	mlog.Debug("Table[%d], UserLeave, User[%d], Seat[%d], IsOffLine()[%t]", pGame.TableNo(), pGameUser.UserID, pGameUser.SeatNo, pGameUser.IsOffLine())

	iSeatNo := int(pGameUser.SeatNo)
	if iSeatNo < 0 || iSeatNo >= pGame.iSeatCount {
		return false
	}

	pUser := pGame.ary_pUserData[iSeatNo]
	pUser.bOffline = true

	if pUser.iUserState == UserState_Normal {
		if pGame.iGameState >= GameState_Betting && pGame.iGameState <= GameState_Settlement_War {
			return false
		}
	}

	pUser.Init()
	pGame.sendAllUserInfo()

	defer func() {
		// check all user leave, close game ...
		bHasUser := false
		for _, pUser := range pGame.ary_pUserData {
			if pUser.User != nil {
				bHasUser = true
				break
			}
		}

		if !bHasUser {
			pGame.iGameState = GameState_WaitingPlayer
		}
	}()

	return pGame.GameLogicBase.UserLeave(pGameUser)
}

// 玩家斷線重連
func (pGame *Logic) UserRecover(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}

	mlog.Debug("Table[%d], UserRecover, User[%d], Seat[%d], IsOffLine()[%t]", pGame.TableNo(), pGameUser.UserID, pGameUser.SeatNo, pGameUser.IsOffLine())

	pGame.DynamicGameLogicBase.UserRecover(pGameUser)

	iSeatNo := int(pGameUser.SeatNo)
	if iSeatNo < 0 || iSeatNo >= pGame.iSeatCount {
		return
	}

	pUser := pGame.ary_pUserData[iSeatNo]
	pUser.bOffline = false

	pGame.sendGameInfo(pUser)
	pGame.sendAllUserInfo()

	switch pGame.iGameState {
	case GameState_Betting: // 押注
		pGame.startBetting(true)
	case GameState_Shuffle: // 洗牌
		pGame.shuffle(true)
	case GameState_WarSelection: // 選擇是否進入戰爭
		pGame.enterWarPhase()
	}
}

// 設定遊戲狀態
func (pGame *Logic) setGameState(iGameState int, i64Timeout int64) {
	if iGameState < GameState_WaitingPlayer || iGameState > GameState_GameOver {
		return
	}

	i64Time := int64(0)
	if i64Timeout < 0 {
		i64Time = 0
	}

	pGame.iGameState = iGameState
	pGame.i64Timeout = i64Time
	pGame.i64LastMessageTime = pGame.i64TimeDuration

	pGame.gameStateLog_Debug()
}