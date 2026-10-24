package jlbpk

import (
	"fmt"
	"strconv"
	"strings"
	"time"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	Global "example.invalid/ServerTeam/Servers/pkg/global"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/poker/pokerset"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//---------------------------------------------------------------------------
type JlbpkLogic struct {
	*gamelogic.DynamicGameLogicBase
	pPokerSet           *JlbpkPokerSet   // 系統牌組
	iSeatCount          int              // 座位數
	pDealerHandCards    *JlbpkPokerSet   // 莊家手牌
	ary_pHandCards      []*JlbpkPokerSet // 玩家手牌
	ary_pUserData       []*JlbpkUser     // 玩家資料
	pGameTicker         *time.Ticker     // 遊戲計時器
	iGameState          int              // 遊戲狀態
	i64TimeDuration     int64            // Ticker 計數值
	i64Timeout          int64            // 各階段 timeout
	i64LastMessageTime  int64            // 上一次訊息時間
	map_i64Timeout      map[int]int64    // timeout 時間設定
	ary_strJPSheet      []string         // Jackpot 賠率表
	ary_strSpecialSheet []string         // 特殊牌賠率表
	ary_strPokerManual  []string         // 遊戲說明
	ary_i32Chip         []int32          // 籌碼面額
	uiAllPlayerReady    uint             // 各玩家均已 Ready，此值設定後就不再變更
	uiPlayerReady       uint             // 標示各玩家是否 Ready
	iUserCount          int              // 遊戲中玩家人數
	map_i64JPSheet      map[int32]int64  // 特殊邊注表
	map_i64SpecialSheet map[int32]int64  // 賠注表
	iJPTest             int              // 彩金測試
	pTestDealerCards    *JlbpkPokerSet   // 測試用莊家手牌
	ary_pTestUserCards  []*JlbpkPokerSet // 測試用玩家手牌
	bDealerIsGood       bool             // 莊家不成局
}

//---------------------------------------------------------------------------
func NewJlbpkLogic(table *gamelogic.Table, tfManager *event.TimerFuncManager, configs *config.ConfigGroup) *JlbpkLogic {
	pJLB := new(JlbpkLogic)
	pJLB.DynamicGameLogicBase = gamelogic.NewDynamicGameLogicBase(table, tfManager, configs)

	pJLB.i64TimeDuration = 0
	//pJLB.pGameTicker = time.NewTicker(100 * time.Millisecond)
	//go func() {
	//	for {
	//		select {
	//		case <-pJLB.pGameTicker.C:
	//			pJLB.i64TimeDuration++
	//			pJLB.gameTickerFunc()
	//		}
	//	}
	//}()

	tfManager.NewMilliSecTicker(100, func() {
		pJLB.i64TimeDuration++
		pJLB.gameTickerFunc()
	})

	pJLB.setGameState(JLB_State_WaitingPlayer, 0)

	pJLB.map_i64Timeout = make(map[int]int64)
	pJLB.map_i64JPSheet = make(map[int32]int64)
	pJLB.map_i64SpecialSheet = make(map[int32]int64)
	pJLB.iJPTest = JLB_JPTest_Off

	pJLB.getGameConfig(configs)

	pJLB.pPokerSet = NewJlbpkPokerSet()
	pJLB.pDealerHandCards = NewJlbpkPokerSet()
	pJLB.ary_pHandCards = make([]*JlbpkPokerSet, pJLB.iSeatCount)
	pJLB.ary_pUserData = make([]*JlbpkUser, pJLB.iSeatCount)
	for i := 0; i < pJLB.iSeatCount; i++ {
		pJLB.ary_pHandCards[i] = NewJlbpkPokerSet()
		pJLB.ary_pUserData[i] = NewJlbpkUser()
		pJLB.ary_pUserData[i].Init()
	}

	// 各玩家均已 Ready，此值設定後就不再變更
	uiReady := uint(1)
	for i := 0; i < pJLB.iSeatCount; i++ {
		uiReady *= 2
	}
	pJLB.uiAllPlayerReady = uiReady - 1

	pJLB.iUserCount = 0

	//----- JP 賠率表 -----
	// 皇家同花順
	str := fmt.Sprintf("%s %d%s奖池", pokerset.PatternString(pokerset.POKER_Pattern_RoyalFlush),
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_RoyalFlush], "%")
	pJLB.ary_strJPSheet = append(pJLB.ary_strJPSheet, str)
	// 同花順
	str = fmt.Sprintf("%s %d%s奖池", pokerset.PatternString(pokerset.POKER_Pattern_StraightFlush),
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_StraightFlush], "%")
	pJLB.ary_strJPSheet = append(pJLB.ary_strJPSheet, str)
	// 四條
	str = fmt.Sprintf("%s %s元", pokerset.PatternString(pokerset.POKER_Pattern_FourOfAKind),
		Global.MoneyString(pJLB.map_i64JPSheet[pokerset.POKER_Pattern_FourOfAKind]))
	pJLB.ary_strJPSheet = append(pJLB.ary_strJPSheet, str)
	//葫蘆
	str = fmt.Sprintf("%s %s元", pokerset.PatternString(pokerset.POKER_Pattern_FullHouse),
		Global.MoneyString(pJLB.map_i64JPSheet[pokerset.POKER_Pattern_FullHouse]))
	pJLB.ary_strJPSheet = append(pJLB.ary_strJPSheet, str)
	// 同花
	str = fmt.Sprintf("%s %s元", pokerset.PatternString(pokerset.POKER_Pattern_Flush),
		Global.MoneyString(pJLB.map_i64JPSheet[pokerset.POKER_Pattern_Flush]))
	pJLB.ary_strJPSheet = append(pJLB.ary_strJPSheet, str)

	// 特殊賠率表
	for i := pokerset.POKER_Pattern_RoyalFlush; i >= pokerset.POKER_Pattern_TwoPairs-1; i-- {
		if i == pokerset.POKER_Pattern_TwoPairs-1 {
			str = fmt.Sprintf("其他 1赔%d", pJLB.map_i64SpecialSheet[i])
		} else {
			str = fmt.Sprintf("%s 1赔%d", pokerset.PatternString(i), pJLB.map_i64SpecialSheet[i])
		}
		pJLB.ary_strSpecialSheet = append(pJLB.ary_strSpecialSheet, str)
	}

	// 遊戲說明
	if g_iHDCMode == HDCModeEnable {
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "游戏使用一副普通52张牌扑克没有王牌，每轮游戏前重新洗牌。")
	} else {
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "加勒比扑克使用一副普通52张牌扑克没有王牌，每轮游戏前重新洗牌。")
	}
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "玩家先下底注。玩家、庄家各发五张牌，玩家五张牌面朝下，庄家四张牌面朝下，一张牌面朝上。然后玩家评估自己手中的牌，并据此作出以下选择。\n")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "1. 弃牌 - 玩家将输掉所下的底注。")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "2. 加注 - 玩家必须在注金区中另外再下底注两倍的赌金。")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "3. 若身上现金不足以加注(底注的两倍),则无法下彩金注。")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "庄家将翻开手中的牌与玩家的牌进行比较。但是庄家手中必须同时有A和K或者更好的牌，才有资格继续玩。如果庄家没有资格继续玩则“不成局”，玩家便可赢得与底注相等的赌金。")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "如果庄家有A和K或更好的牌，并且手里的牌面高于玩家的牌面，玩家就会输掉底注和加注赌金。")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "如果玩家的牌面高于庄家的牌面，则会赢得与底注相等的赌金以及按照赔率规则计算得出的红利赌金。")
	pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "如果客牌与庄牌点数相同，则不考虑花色，双方打成和局。\n")
	if g_iHDCMode == HDCModeEnable {
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "在开始下注的时候，您可以额外下注彩金注。")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "无论庄牌是否成局，累进大奖彩金注都可以获得彩金，若身上现金不足以加注(底注的两倍),则无法下彩金注")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "赔率如下:")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "皇家同花顺 - 累进彩池的100%")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "同花顺 - 累进彩池的 10%")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "四条 - $500 奖金")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "葫芦 - $100 奖金")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "同花 - $50 奖金")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "賠注表如下:")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "皇家同花順 1 賠 50")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "同花順 1 賠 30")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "四條 1賠20")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "葫蘆 1賠9")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "同花 1賠7")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "順子 1賠4")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "三條 1賠3")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "兩對 1賠2")
		pJLB.ary_strPokerManual = append(pJLB.ary_strPokerManual, "其他 1賠1")
	}

	pJLB.initData()

	pJLB.RegisterBeforeGameStartFunc(func() {
	})
	pJLB.RegisterAfterGameStartFunc(func() {
		pJLB.setGameState(JLB_State_StartBettingAnimation, pJLB.map_i64Timeout[JLB_Timeout_StartBettingAnimation])
		pJLB.playStartBettingAnimation()
	})
	pJLB.RegisterBeforeGameEndFunc(func() {
	})
	pJLB.RegisterAfterGameEndFunc(func() {
	})

	return pJLB
}

func (pJLB *JlbpkLogic) initData() {
	mlog.Debug("初始化遊戲資料")
	/**
	主要解決卡桌問題, 需要在用戶都離開的時候初始化遊戲資料
	若初始化過程中表示遊戲中則取消該局遊戲.
	*/
	if pJLB.IsGaming() {
		mlog.Debug("initData 遊戲初始化時遊戲狀態為遊戲中")
		pJLB.GameCancelFunc("初始化遊戲資料")
	}
	pJLB.pPokerSet.NewSet(JLBPK_PokerSetCount, 0)
	pJLB.pPokerSet.Shuffle()
	pJLB.pDealerHandCards.SetCardSet("")
	pJLB.iUserCount = 0
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pJLB.ary_pHandCards[iSeat].SetCardSet("")

		pGameUser := pJLB.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			pJLB.ary_pUserData[iSeat].Init()
		} else {
			if pJLB.ary_pUserData[iSeat].iUserState != JLB_UserState_EmptySeat {
				pJLB.ary_pUserData[iSeat].iUserState = JLB_UserState_Normal
			}
			pJLB.UserReady(pGameUser)
			pJLB.iUserCount++
		}
	}
	pJLB.bDealerIsGood = true

	if pJLB.iUserCount <= 0 {
		pJLB.setGameState(JLB_State_WaitingPlayer, 0)

		mlog.Debug("等待玩家进入")
		pJLB.GameLog.AddGameStep("等待玩家进入")
	} else {
		pJLB.setGameState(JLB_State_WaitingStart, pJLB.map_i64Timeout[JLB_Timeout_WaitingStart])
		pJLB.waitingGameBegin()
	}
	pJLB.i64LastMessageTime = pJLB.i64TimeDuration
	mlog.Debug("initData, 玩家人數：%d", pJLB.iUserCount)

	pJLB.uiPlayerReady = 0
}

func (pJLB *JlbpkLogic) GameMessage(pGameUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pGameUser == nil || pMsg == nil {
		return
	}

	if pMsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	pUser := pJLB.GetUserDataByUserID(pGameUser.UserID)
	if pUser == nil {
		return
	}

	switch pMsg.SClassID {
	case int32(netproto.JLBPK_GameMessageClassID_StartBettingAnimationOK): // C2S 開始下注動畫播放完成
		pJLB.message_StartBettingAnimationOK(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_Bet): // C2S 玩家下注，包含底注及邊注
		pJLB.message_Bet(pUser, pMsg)
	case int32(netproto.JLBPK_GameMessageClassID_StopBettingAnimationOK): // C2S 停止下注動畫播放完成
		pJLB.message_StopBettingAnimationOK(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_DealOk): // C2S 發牌動畫播放完成
		pJLB.message_DealOK(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_DoubleBet): // C2S 玩家加注
		pJLB.message_DoubleBet(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_FoldCard): // C2S 棄牌
		pJLB.message_Fold(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_ShowCardsOK): // C2S 開牌，邊注結算效果完成
		pJLB.message_ShowCardsOK(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_Compensate_BetBase_OK): // C2S 賠付，底注結算效果完成
		pJLB.message_BetBaseSettlementOK(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_Compensate_DoubleBet_OK): // C2S 賠付，加注結算效果完成
		pJLB.message_BetDoubleSettlementOK(pUser)
	case int32(netproto.JLBPK_GameMessageClassID_SettlementOK): // C2S 總結算效果完成
		pJLB.message_SettlementOK(pUser)
	}
}

func (pJLB *JlbpkLogic) PiSaiMessage(pUser *gamelogic.GameUser, pMsg *msg.Message) {
	if pUser == nil || pMsg == nil {
		return
	}
}

// 玩家進入遊戲
func (pJLB *JlbpkLogic) UserEnter(pGameUser *gamelogic.GameUser, i32TableNo int32, i32SeatNo int32) {
	if pGameUser == nil {
		return
	}

	if !g_bIsReadJackpotFromDB {
		g_bIsReadJackpotFromDB = true
		pJLB.ReadJackpotPool()
	}

	mlog.Debug("UserEnter, User[%d], Seat[%d]", pGameUser.UserID, pGameUser.SeatNo)

	//if pJLB.iGameState > JLB_State_WaitingStart {
	//	return
	//}

	pJLB.DynamicGameLogicBase.UserEnter(pGameUser, i32TableNo, i32SeatNo)

	pJLB.DynamicGameLogicBase.UserReady(pGameUser)

	pJLB.AddUser(pGameUser)
}

// 計時器處理程序
func (pJLB *JlbpkLogic) gameTickerFunc() {
	if pJLB.i64Timeout > 0 && pJLB.i64TimeDuration-pJLB.i64LastMessageTime > pJLB.i64Timeout {
		switch pJLB.iGameState {
		//case JLB_State_WaitingStart: // 等待遊戲開始
		//	pJLB.GameStartFunc()
		//	pJLB.setGameState(JLB_State_StartBettingAnimation, pJLB.map_i64Timeout[JLB_Timeout_StartBettingAnimation])
		//	pJLB.playStartBettingAnimation()
		case JLB_State_StartBettingAnimation: // 開始下注動畫
			pJLB.setGameState(JLB_State_StartBetting, pJLB.map_i64Timeout[JLB_Timeout_StartBetting])
			pJLB.startBetting()
		case JLB_State_StartBetting: // 開始下注
			pJLB.setGameState(JLB_State_StopBettingAnimation, pJLB.map_i64Timeout[JLB_Timeout_StopBettingAnimation])
			pJLB.playStopBettingAnimation()
		case JLB_State_StopBettingAnimation: // 停止下注動畫
			pJLB.setGameState(JLB_State_Deal, pJLB.map_i64Timeout[JLB_Timeout_DealAnimation])
			pJLB.deal()
		case JLB_State_Deal: // 發牌
			pJLB.setGameState(JLB_State_BetDouble, pJLB.map_i64Timeout[JLB_Timeout_DoubleBetting])
			pJLB.betDouble()
		case JLB_State_BetDouble: // 加注
			if g_iHDCMode == HDCModeDisable {
				pJLB.setGameState(JLB_State_BetSideSettlement, pJLB.map_i64Timeout[JLB_Timeout_BetSideSettlement])
				pJLB.showCards()
			} else if g_iHDCMode == HDCModeEnable {
				pJLB.setGameState(DHC_State_Settlement, pJLB.map_i64Timeout[DHC_Timeout_Settlement])
				pJLB.settlement()
			}
		case JLB_State_BetSideSettlement: // 邊注結算
			for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
				pJLB.ary_pUserData[iSeat].iPlayingState = JLB_UserPlayState_ShowCardsOK
			}
			pJLB.setGameState(JLB_State_BetBaseSettlement, pJLB.map_i64Timeout[JLB_Timeout_BetBaseSettlement])
			pJLB.settlement_BetBase()
		case JLB_State_BetBaseSettlement: // 底注結算
			pJLB.setGameState(JLB_State_BetDoubleSettlement, pJLB.map_i64Timeout[JLB_Timeout_BetDoubleSettlement])
			pJLB.settlement_BetDouble()
		case JLB_State_BetDoubleSettlement: // 加注結算
			fallthrough
		case DHC_State_Settlement:
			pJLB.setGameState(JLB_State_GameOver, 0)
			pJLB.gameOver()
		}
	}

	// 機器人動作
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		if pUser.iUserType == gamelogic.UserType_Rebot && pUser.iUserState == JLB_UserState_Normal {
			if pUser.i64Timeout > 0 && pJLB.i64TimeDuration-pUser.i64LastMessageTime > pUser.i64Timeout {
				switch pJLB.iGameState {
				case JLB_State_StartBetting: // 開始下注
					// 機器人隨機下邊注
					i64BetSide := g_i64DefaultBetSide
					if pJLB.ary_pUserData[iSeat].i64Money-i64BetSide >= pJLB.ary_pUserData[iSeat].i64BetBase*2 {
						pUser.i64BetSide = i64BetSide
						pUser.i64Money -= pUser.i64BetSide
						pJLB.sendBetResult(pUser, JLB_BetType_Side, pUser.i64BetSide, JLB_BetResult_Success)
					}
				case JLB_State_BetDouble: // 加注
					// 有對子就加注，無AK就有機率棄牌
					if pJLB.ary_pHandCards[iSeat].Pattern(false) >= pokerset.POKER_Pattern_Pair {
						// 有對子（含）以上的牌
						i64BetDouble := pUser.i64BetBase * 2
						if pUser.i64Money >= i64BetDouble {
							pUser.i64BetDouble = i64BetDouble
							pUser.i64Money -= pUser.i64BetDouble
							pJLB.sendDoubleBetResult(pUser, JLB_DoubleBetResult_Success)
						}
					} else if pJLB.ary_pHandCards[iSeat].Pattern(false) == pokerset.POKER_Pattern_HighCard {
						// 高牌
						if !pJLB.ary_pHandCards[iSeat].CheckIfHasAK() {
							iRnd := util.RndInt(0, 100)
							if iRnd < g_iFoldByNoAKOdds_Robot {
								// 棄牌
								pUser.iUserState = JLB_UserState_Fold
								pUser.i64MoneyChange += -pUser.i64BetBase
								pJLB.sendUserFoldCard(pUser)
							}
						}
					}
				}

				pUser.i64Timeout = 0
			}
		}
	}
}

// 讀取遊戲設定
func (pJLB *JlbpkLogic) getGameConfig(configs *config.ConfigGroup) {
	if configs == nil {
		mlog.Error("桌號[%d] 讀取 config 失敗", pJLB.TableNo())
		return
	}

	var str string = ""

	// 讀取 gameserver 設定值
	cfgs := configs.GetConfig("gameserver")
	if cfgs == nil {
		mlog.Error("獲取配置 [gameserver] 失敗")
	} else {
		pJLB.iSeatCount = cfgs.GetConfigInt("gameserver", "seatcount")

		//----- timeout -----
		pJLB.map_i64Timeout[JLB_Timeout_WaitingStart] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_WaitingStart")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_StartBettingAnimation] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_StartBettingAnimation")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_StartBetting] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_StartBetting")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_StopBettingAnimation] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_StopBettingAnimation")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_DealAnimation] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_DealAnimation")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_DoubleBetting] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_DoubleBetting")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_BetSideSettlement] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_BetSideSettlement")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_BetBaseSettlement] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_BetBaseSettlement")) * 10
		pJLB.map_i64Timeout[JLB_Timeout_BetDoubleSettlement] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_BetDoubleSettlement")) * 10
		pJLB.map_i64Timeout[DHC_Timeout_Settlement] = int64(cfgs.GetConfigInt("GameSetting", "Timeout_Settlement")) * 10

		// 籌碼面額
		str = cfgs.GetConfigValue("GameSetting", "ChipValue")
		ary_strChip := strings.Split(str, ",")
		for i := 0; i < len(ary_strChip); i++ {
			i64chip, _ := strconv.ParseInt(ary_strChip[i], 10, 32)
			pJLB.ary_i32Chip = append(pJLB.ary_i32Chip, int32(i64chip))
		}

		g_i64DefaultBetSide = int64(cfgs.GetConfigInt64("GameSetting", "BetSide"))

		// 獎池回補基準值
		g_i64JackpotBase = int64(cfgs.GetConfigInt64("GameSetting", "JackpotBase"))
		g_i64Jackpot = g_i64JackpotBase

		//----- 特殊邊注 -----
		// 皇家同花順，此值為獎池的百分比比例
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_RoyalFlush] = int64(cfgs.GetConfigInt64("GameSetting", "JP_RoyalFlush"))
		// 同花順，此值為獎池的百分比比例
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_StraightFlush] = int64(cfgs.GetConfigInt64("GameSetting", "JP_StraightFlush"))
		// 四條，中獎金額
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_FourOfAKind] = int64(cfgs.GetConfigInt64("GameSetting", "JP_FourOfKind"))
		// 葫蘆，中獎金額
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_FullHouse] = int64(cfgs.GetConfigInt64("GameSetting", "JP_FullHouse"))
		// 同花，中獎金額
		pJLB.map_i64JPSheet[pokerset.POKER_Pattern_Flush] = int64(cfgs.GetConfigInt64("GameSetting", "JP_Flush"))

		//----- 賠注表 ----
		// 皇家同花順
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_RoyalFlush] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_RoyalFlush"))
		// 同花順
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_StraightFlush] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_StraightFlush"))
		// 四條
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_FourOfAKind] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_FourOfKind"))
		// 葫蘆
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_FullHouse] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_FullHouse"))
		// 同花
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_Flush] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_Flush"))
		// 順子
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_Straight] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_Straight"))
		// 三條
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_ThreeOfAKind] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_ThreeOfKind"))
		// 兩對
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_TwoPairs] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_TwoPairs"))
		// 對子
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_Pair] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_Other"))
		// 高牌
		pJLB.map_i64SpecialSheet[pokerset.POKER_Pattern_HighCard] = int64(cfgs.GetConfigInt64("GameSetting", "Pay_Other"))

		g_iZhuangBetSide = cfgs.GetConfigInt("GameSetting", "OpenZhuangBetSide")
		g_iPluralBetSide = cfgs.GetConfigInt("GameSetting", "PluralBetSide")
		g_iPluralBetSideLimit = cfgs.GetConfigInt("GameSetting", "PluralBetSideLimit")
		g_iHDCMode = cfgs.GetConfigInt("GameSetting", "HDCMode")

		// 機器人參數
		g_iFoldByNoAKOdds_Robot = cfgs.GetConfigInt("robot", "Odds_FoldByNoAK")
		g_iBetSideOdds_Robot = cfgs.GetConfigInt("robot", "Odds_BetSide")
	}

	// 撲克牌發牌器
	pkcfgs := configs.GetConfig("poker")
	if pkcfgs == nil {
		mlog.Error("獲取配置 [poker] 失敗")
	} else {
		// 彩金測試
		iJPTest := pkcfgs.GetConfigInt("UserPoker", "JPTestEnable")
		if iJPTest < JLB_JPTest_Off || iJPTest >= JLB_JPTest_Max {
			iJPTest = JLB_JPTest_Off
		}
		pJLB.iJPTest = iJPTest

		// 測試用莊家手牌
		strCards := pkcfgs.GetConfigValue("UserPoker", "Dealer")
		pSet := new(pokerset.PokerCardSet)
		pSet.SetCardSet(strCards)
		if pSet.CardCount() < JLBPK_HandCardCount {
			pSet.SetCardSet("")
		} else if pSet.CardCount() > JLBPK_HandCardCount {
			pTemp := new(pokerset.PokerCardSet)
			for i := 0; i < JLBPK_HandCardCount; i++ {
				pCard := pSet.GetSingleCard(i, false)
				pTemp.AddSingleCard(pCard.CodeString())
			}
			pSet.SetCardSet(pTemp.CodeString())
		}

		pJLB.pTestDealerCards = NewJlbpkPokerSet()
		pJLB.pTestDealerCards.SetCardSet(pSet.CodeString())

		// 測試用玩家手牌
		pJLB.ary_pTestUserCards = make([]*JlbpkPokerSet, pJLB.iSeatCount)
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			str = fmt.Sprintf("User%d", iSeat+1)
			strCards := pkcfgs.GetConfigValue("UserPoker", str)
			pSet := new(pokerset.PokerCardSet)
			pSet.SetCardSet(strCards)
			if pSet.CardCount() < JLBPK_HandCardCount {
				pSet.SetCardSet("")
			} else if pSet.CardCount() > JLBPK_HandCardCount {
				pTemp := new(pokerset.PokerCardSet)
				for i := 0; i < JLBPK_HandCardCount; i++ {
					pCard := pSet.GetSingleCard(i, false)
					pTemp.AddSingleCard(pCard.CodeString())
				}
				pSet.SetCardSet(pTemp.CodeString())
			}

			pJLB.ary_pTestUserCards[iSeat] = NewJlbpkPokerSet()
			pJLB.ary_pTestUserCards[iSeat].SetCardSet(pSet.CodeString())
		}
	}
}

// 加入新玩家
func (pJLB *JlbpkLogic) AddUser(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}

	iSeatNo := int(pGameUser.SeatNo)
	if iSeatNo < 0 || iSeatNo >= pJLB.iSeatCount {
		return
	}

	if pJLB.ary_pUserData[iSeatNo].iUserState == JLB_UserState_EmptySeat {
		pJLB.ary_pUserData[iSeatNo].Init()
		pJLB.ary_pUserData[iSeatNo].User = pGameUser
		pJLB.ary_pUserData[iSeatNo].i32UserID = pGameUser.UserID
		pJLB.ary_pUserData[iSeatNo].i32SeatNo = pGameUser.SeatNo
		if pGameUser.IsRobot() {
			pJLB.ary_pUserData[iSeatNo].iUserType = gamelogic.UserType_Rebot
		}
		pJLB.ary_pUserData[iSeatNo].i64Money = pGameUser.Score
		pJLB.ary_pUserData[iSeatNo].ResetGameData()

		if pJLB.iGameState >= JLB_State_StartBetting {
			pJLB.ary_pUserData[iSeatNo].iUserState = JLB_UserState_Watch
		} else {
			pJLB.ary_pUserData[iSeatNo].iUserState = JLB_UserState_Normal

			switch pJLB.iGameState {
			case JLB_State_WaitingPlayer: // 等待玩家加入
				fallthrough
			case JLB_State_WaitingStart: // 等待遊戲開始
				pJLB.ary_pUserData[iSeatNo].iPlayingState = JLB_UserPlayState_WaitingStart
			case JLB_State_StartBettingAnimation: // 開始下注動畫
				fallthrough
			case JLB_State_StartBetting: // 開始下注
				pJLB.ary_pUserData[iSeatNo].iPlayingState = JLB_UserPlayState_StartBettingOK
				if pJLB.ary_pUserData[iSeatNo].i64BetBase <= 0 {
					pJLB.ary_pUserData[iSeatNo].i64BetBase += int64(pJLB.ary_i32Chip[0])
					pJLB.ary_pUserData[iSeatNo].i64TotalBet += pJLB.ary_pUserData[iSeatNo].i64BetBase
					pJLB.ary_pUserData[iSeatNo].i64Money -= pJLB.ary_pUserData[iSeatNo].i64BetBase
					pJLB.ary_pUserData[iSeatNo].i64TotalWinLose -= pJLB.ary_pUserData[iSeatNo].i64BetBase
				}
			}
		}
	}

	pJLB.iUserCount++
	mlog.Debug("Table[%d], AddUser, 玩家人數：%d", pJLB.TableNo(), pJLB.iUserCount)
	//if pJLB.iUserCount > 0 && pJLB.iGameState == JLB_State_WaitingPlayer {
	//
	//	pJLB.setGameState(JLB_State_WaitingStart, pJLB.map_i64Timeout[JLB_Timeout_WaitingStart])
	//	pJLB.waitingGameBegin()
	//}
	pJLB.sendGameInfo(pGameUser)
	pJLB.sendAllUserInfo()

	example.invalid("加入新玩家[%d], 遊戲狀態[%s], 座位[%d], 類別[%d], 玩家狀態[%s], 玩家總人數[%d]", pJLB.ary_pUserData[iSeatNo].i32UserID,
		map_strGameState_debug[pJLB.iGameState], iSeatNo, pJLB.ary_pUserData[iSeatNo].iUserType,
		map_strUserState_debug[pJLB.ary_pUserData[iSeatNo].iUserState], pJLB.iUserCount)
	pJLB.GameLog.AddGameStep("玩家[%d] 入座，状态：%s", pGameUser.UserID, map_strUserState[pJLB.ary_pUserData[iSeatNo].iUserState])
}

// 傳送牌局資訊
func (pJLB *JlbpkLogic) sendGameInfo(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}

	mlog.Debug("sendGameInfo, GameState[%s], ToClientState[%s], JP[%d], Time[%d], DealerCard[%s]",
		map_strGameState_debug[pJLB.iGameState], map_strClientGameState_debug[pJLB.toClientGameState()], g_i64Jackpot,
		pJLB.i64Timeout-pJLB.i64TimeDuration+pJLB.i64LastMessageTime, pJLB.pDealerHandCards.String())

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_GameInfo)

	pData := new(netproto.JLBPK_GameInfo)
	pData.GameState = proto.Int32(int32(pJLB.toClientGameState())) // 遊戲狀態
	// Jackpot 賠率表
	for i := 0; i < len(pJLB.ary_strJPSheet); i++ {
		pData.JPSheet = append(pData.JPSheet, pJLB.ary_strJPSheet[i])
	}
	// 特殊牌賠率表
	for i := 0; i < len(pJLB.ary_strSpecialSheet); i++ {
		pData.SpecialSheet = append(pData.SpecialSheet, pJLB.ary_strSpecialSheet[i])
	}
	// 遊戲說明
	for i := 0; i < len(pJLB.ary_strPokerManual); i++ {
		pData.PokerManual = append(pData.PokerManual, pJLB.ary_strPokerManual[i])
	}
	pData.JPMoney = proto.Int64(g_i64Jackpot) // Jackpot 金額
	// 剩餘時間
	pData.TimeRemain = proto.Int32(int32((pJLB.i64Timeout - pJLB.i64TimeDuration + pJLB.i64LastMessageTime) / 10))
	// 所有玩家資訊
	for i := 0; i < pJLB.iSeatCount; i++ {
		pUserInfo := pJLB.userInfoPkg(i, int(pGameUser.SeatNo))
		pData.UserInfo = append(pData.UserInfo, pUserInfo)
	}
	// 籌碼面額
	for i := 0; i < len(pJLB.ary_i32Chip); i++ {
		pData.ChipValue = append(pData.ChipValue, pJLB.ary_i32Chip[i])
	}
	pData.Bet_Side = proto.Int64(g_i64DefaultBetSide) // 邊注金額
	// 莊家手牌
	pDealerCard := new(netproto.JLBPK_UserCards)
	for iIdx := 0; iIdx < pJLB.pDealerHandCards.CardCount(); iIdx++ {
		pCard := new(netproto.PokerCard)
		if pJLB.iGameState >= JLB_State_BetSideSettlement {
			pSCard := pJLB.pDealerHandCards.GetSingleCard(iIdx, false)
			pCard.Color = proto.Int32(pSCard.I32Color)
			pCard.CardNumber = proto.Int32(pSCard.I32Number)
		} else {
			if iIdx == 0 {
				pSCard := pJLB.pDealerHandCards.GetSingleCard(iIdx, false)
				pCard.Color = proto.Int32(pSCard.I32Color)
				pCard.CardNumber = proto.Int32(pSCard.I32Number)
			} else {
				pCard.Color = proto.Int32(pokerset.POKER_CardColor_Other)
				pCard.CardNumber = proto.Int32(pokerset.POKER_CardNo_Hide)
			}
		}
		pDealerCard.Cards = append(pDealerCard.Cards, pCard)
	}
	pDealerCard.Pattern = proto.Int32(pJLB.pDealerHandCards.Pattern(true))
	pData.BankerCards = pDealerCard

	pMsg.MsgData = pData
	pJLB.SendMessageToUser(pGameUser, pMsg)
}

// 傳送玩家資訊
func (pJLB *JlbpkLogic) sendAllUserInfo() {
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			continue
		}

		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_AllUserInfo)

		pData := new(netproto.JLBPK_AllUserInfo)
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, iSeat)
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	}
}

// 等待遊戲開始
func (pJLB *JlbpkLogic) waitingGameBegin() {
	pJLB.GameLog.GameBegin()
	mlog.Debug("等待开始下注")
	pJLB.GameLog.AddGameStep("等待开始下注")

	for i := 0; i < pJLB.iSeatCount; i++ {
		pJLB.ary_pUserData[i].ResetGameData()
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_WaitingStart)
	pData := new(netproto.JLBPK_WaitingStart)
	pData.TimeRemain = proto.Int32(int32((pJLB.i64Timeout - pJLB.i64TimeDuration + pJLB.i64LastMessageTime) / 10))

	pMsg.MsgData = pData
	pJLB.BoardcastMessage(pMsg)

	mlog.Debug("state[%s], timeout[%d], duration[%d], lastmsg[%d]", map_strGameState_debug[pJLB.iGameState],
		pJLB.i64Timeout, pJLB.i64TimeDuration, pJLB.i64LastMessageTime)
}

// 播放開始下注動畫
func (pJLB *JlbpkLogic) playStartBettingAnimation() {
	mlog.Debug("播放開始下注動畫")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_StartBettingAnimation)

	pJLB.BoardcastMessage(pMsg)

	pJLB.uiPlayerReady = 0

	// 改為播放開始下注動畫時就可以下注
	for i := 0; i < pJLB.iSeatCount; i++ {
		//if pJLB.ary_pUserData[i].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[i].iUserType == gamelogic.UserType_Rebot ||
		//	pJLB.ary_pUserData[i].bOffline {
		//	pJLB.ary_pUserData[i].iPlayingState = JLB_UserPlayState_StartBettingOK
		//	pJLB.uiPlayerReady |= 1 << uint(i)
		//}
		pJLB.ary_pUserData[i].iPlayingState = JLB_UserPlayState_StartBettingOK
	}

	pJLB.setGameState(JLB_State_StartBetting, pJLB.map_i64Timeout[JLB_Timeout_StartBetting])
	pJLB.startBetting()
}

// 訊息處理 - 開始下注動畫播放完成
func (pJLB *JlbpkLogic) message_StartBettingAnimationOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_StartBettingAnimation {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_WaitingStart {
		return
	}

	mlog.Debug("User[%d] 開始下注動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = JLB_UserPlayState_StartBettingOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家開始下注動畫播放完成")
		//pJLB.startBetting()
	}
}

// 開始下注
func (pJLB *JlbpkLogic) startBetting() {
	mlog.Debug("開始下注")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_UserBettingStart)
	pData := new(netproto.JLBPK_UserBettingStart)
	pData.TimeRemain = proto.Int32(int32((pJLB.i64Timeout - pJLB.i64TimeDuration + pJLB.i64LastMessageTime) / 10))

	pMsg.MsgData = pData
	pJLB.BoardcastMessage(pMsg)

	pJLB.GameLog.AddGameStep("开始下注")
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		pUser.iPlayingState = JLB_UserPlayState_Betting
		if pUser.iUserState == JLB_UserState_Normal {
			// 自動幫玩家下底注
			mlog.Debug("玩家[%d] 金钱 %s", pUser.i32UserID, Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d] 金钱 %s", pUser.i32UserID, Global.MoneyString(pUser.i64Money))
			pUser.i64BetBase += int64(pJLB.ary_i32Chip[0])
			pUser.i64TotalBet += pUser.i64BetBase
			pUser.i64Money -= pUser.i64BetBase
			pUser.i64TotalWinLose -= pUser.i64BetBase
			pJLB.sendBetResult(pUser, JLB_BetType_Base, pUser.i64BetBase, JLB_BetResult_Auto)

			// 隨機決定機器人是否要下邊注
			if pUser.iUserType == gamelogic.UserType_Rebot && pUser.iUserState == JLB_UserState_Normal {
				iRnd := util.RndInt(0, 100)
				if iRnd < g_iBetSideOdds_Robot {
					// 要下邊注，設定觸發時間
					pUser.SetRobotTimeout(pJLB.map_i64Timeout[JLB_Timeout_StartBetting] - 1)
					if pUser.i64Timeout > 0 {
						pUser.i64LastMessageTime = pJLB.i64TimeDuration
					}
				}
			}
		}
	}
}

// 送給 client 的遊戲狀態
func (pJLB *JlbpkLogic) toClientGameState() int {
	switch pJLB.iGameState {
	case JLB_State_WaitingPlayer: // 等待玩家加入
		fallthrough
	case JLB_State_WaitingStart: // 等待遊戲開始
		return JLB_CState_WaitingStart
	case JLB_State_StartBettingAnimation: // 開始下注動畫
		fallthrough
	case JLB_State_StartBetting: // 開始下注
		return JLB_CState_StartBetting
	case JLB_State_StopBettingAnimation: // 停止下注動畫
		fallthrough
	case JLB_State_Deal: // 發牌
		return JLB_CState_Deal
	case JLB_State_BetDouble: // 加注
		return JLB_CState_BetDouble
	case JLB_State_BetSideSettlement: // 邊注結算
		return JLB_CState_BetSideSettlement
	case JLB_State_BetDoubleSettlement: // 加注結算
		return JLB_CState_BetDoubleSettlement
	case JLB_State_BetBaseSettlement: // 底注結算
		return JLB_CState_BetBaseSettlement
	case JLB_State_GameOver: // 遊戲結束
		return JLB_CState_WaitingStart
	case DHC_State_Settlement: // 遊戲結束
		return DHC_CState_Settlement
	}

	return -1
}

// 訊息處理 - 玩家下注，包含底注及邊注
func (pJLB *JlbpkLogic) message_Bet(pUser *JlbpkUser, pMsg *msg.Message) {
	if pUser == nil || pMsg == nil {
		return
	}

	msgInfo := &netproto.JLBPK_Bet{}
	proto.Unmarshal(pMsg.MsgData.([]byte), msgInfo)
	i32BetType := msgInfo.GetBetType()
	i64Bet := msgInfo.GetBet()

	example.invalid("玩家[%d] 下%s %s", pUser.i32UserID, map_strBetType[i32BetType], Global.MoneyString(i64Bet))
	pJLB.GameLog.AddGameStep("玩家[%d] 下%s %s", pUser.i32UserID, map_strBetType[i32BetType], Global.MoneyString(i64Bet))

	if pJLB.iGameState != JLB_State_StartBetting {
		pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BettingStopped)
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_Betting {
		pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BettingStopped)
		return
	}

	if i64Bet <= 0 {
		pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSmallThanZero)
		return
	}

	// 剩餘金錢是否足以下注
	if pUser.i64Money < i64Bet {
		pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_NotEnoughMoney)
		return
	}

	switch i32BetType {
	case JLB_BetType_Base: // 底注
		// 下注是否超過上限
		if pUser.i64BetBase+i64Bet > (pUser.i64Money+pUser.i64BetBase)/3 {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BaseBetOverUpperLimit)
			return
		}

		pUser.i64BetBase += i64Bet
		pUser.i64TotalBet += i64Bet
		pUser.i64TotalWinLose -= i64Bet
	case JLB_BetType_Side: // 邊注
		// 是否已下過邊注
		if pUser.i64BetSide > 0 && g_iPluralBetSide == PluralBetSideDisable {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSideHasDone)
			return
		}

		// 邊注值是否正確
		if i64Bet != g_i64DefaultBetSide {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSideOverLowerLimit)
			return
		}

		// 邊注是否超過上限
		if pUser.i64Money-i64Bet < pUser.i64BetBase*2 {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BaseBetOverUpperLimit)
			return
		}

		// 複數邊注是否超過限制
		if g_iPluralBetSide == PluralBetSideEnable && (pUser.i64BetSide+i64Bet > int64(g_iPluralBetSideLimit) && g_iPluralBetSideLimit > 0) {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSideOverLowerLimit)
			return
		}

		pUser.i64BetSide += i64Bet
		pUser.i64TotalBet += i64Bet
		pUser.i64TotalWinLose -= i64Bet
	case JLB_BetType_ZhuangSide:
		// 是否開啟莊家邊注
		if g_iZhuangBetSide == ZhuangBetSideDisable {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_NotOpenZhuangSide)
			return
		}

		// 是否已下過邊注
		if pUser.i64ZhuangBetSide > 0 && g_iPluralBetSide == PluralBetSideDisable {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSideHasDone)
			return
		}

		// 邊注值是否正確
		if i64Bet != g_i64DefaultBetSide {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSideOverLowerLimit)
			return
		}

		// 邊注是否超過上限
		if pUser.i64Money-i64Bet < pUser.i64BetBase*2 {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BaseBetOverUpperLimit)
			return
		}

		// 複數邊注是否超過限制
		if g_iPluralBetSide == PluralBetSideEnable && (pUser.i64ZhuangBetSide+i64Bet > int64(g_iPluralBetSideLimit) && g_iPluralBetSideLimit > 0) {
			pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_BetSideOverLowerLimit)
			return
		}

		pUser.i64ZhuangBetSide += i64Bet
		pUser.i64TotalBet += i64Bet
		pUser.i64TotalWinLose -= i64Bet
	default:
		return
	}

	pUser.i64Money -= i64Bet
	pJLB.sendBetResult(pUser, i32BetType, i64Bet, JLB_BetResult_Success)
}

// 傳送玩家下注結果
func (pJLB *JlbpkLogic) sendBetResult(pUser *JlbpkUser, i32BetType int32, i64Bet int64, iResult int) {
	if pUser == nil {
		return
	}

	iBetResult := iResult
	if iResult >= JLB_BetResult_Success {
		iBetResult = JLB_BetResult_Success
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_BetResult)

	pData := new(netproto.JLBPK_BetResult)
	pData.SeatNo = proto.Int32(pUser.i32SeatNo + int32(JLBPK_ClientStartUpSeatNo)) // 下注玩家座位
	pData.BetType = proto.Int32(i32BetType)                                        // 下注類別
	pData.Bet = proto.Int64(i64Bet)                                                // 下注金額
	pData.MoneyRemain = proto.Int64(pUser.i64Money)                                // 下注後玩家剩餘金錢
	pData.Result = proto.Int32(int32(iBetResult))                                  // 下注結果

	pMsg.MsgData = pData
	if iBetResult == JLB_BetResult_Success {
		pJLB.BoardcastMessage(pMsg)
	} else {
		pJLB.SendMessageToUser(pUser.User, pMsg)
	}

	if iResult == JLB_BetResult_Auto {
		mlog.Debug("玩家[%d] 自动下底注：%s, 剩余金钱 %s", pUser.i32UserID,
			Global.MoneyString(pUser.i64BetBase), Global.MoneyString(pUser.i64Money))
		pJLB.GameLog.AddGameStep("玩家[%d] 自动下底注：%s, 剩余金钱 %s", pUser.i32UserID,
			Global.MoneyString(pUser.i64BetBase), Global.MoneyString(pUser.i64Money))
	} else {
		example.invalid("玩家[%d] 下注结果：%s, 剩余金钱 %s", pUser.i32UserID, map_strBetResult[iResult],
			Global.MoneyString(pUser.i64Money))
		pJLB.GameLog.AddGameStep("玩家[%d] 下注结果：%s, 剩余金钱 %s", pUser.i32UserID, map_strBetResult[iResult],
			Global.MoneyString(pUser.i64Money))
	}
}

// 播放停止下注動畫
func (pJLB *JlbpkLogic) playStopBettingAnimation() {
	mlog.Debug("播放停止下注動畫")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_StopBettingAnimation)

	pJLB.BoardcastMessage(pMsg)

	pJLB.uiPlayerReady = 0

	for i := 0; i < pJLB.iSeatCount; i++ {
		if pJLB.ary_pUserData[i].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[i].iUserType == gamelogic.UserType_Rebot ||
			pJLB.ary_pUserData[i].bOffline {
			pJLB.ary_pUserData[i].iPlayingState = JLB_UserPlayState_StopBettingOK
			pJLB.uiPlayerReady |= 1 << uint(i)
		}
	}
}

// 訊息處理 - 停止下注動畫播放完成
func (pJLB *JlbpkLogic) message_StopBettingAnimationOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_StopBettingAnimation {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_Betting {
		return
	}

	mlog.Debug("User[%d] 停止下注動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = JLB_UserPlayState_StopBettingOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家停止下注動畫播放完成")
		pJLB.setGameState(JLB_State_Deal, pJLB.map_i64Timeout[JLB_Timeout_DealAnimation])
		pJLB.deal()
	}
}

// 發牌
func (pJLB *JlbpkLogic) deal() {
	mlog.Debug("發牌")

	// 發牌
	mlog.Debug("iJPTest = %d", pJLB.iJPTest)
	switch pJLB.iJPTest {
	case JLB_JPTest_Random: // 隨機發彩金牌型
		mlog.Debug("JLB_JPTest_Random")
		// 發玩家手牌
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_Normal {
				iRnd := util.RndInt(0, len(g_ary_i32JPPattern))
				i32Pattern := g_ary_i32JPPattern[iRnd]
				pSet := pJLB.pPokerSet.DealByPattern(i32Pattern, JLBPK_HandCardCount)
				pJLB.ary_pHandCards[iSeat].SetCardSet(pSet.CodeString())
			}
		}

		// 發莊家牌，莊家牌不排序
		pSet := pJLB.pPokerSet.Deal(JLBPK_HandCardCount)
		pJLB.pDealerHandCards.SetCardSet(pSet.CodeString())
	case JLB_JPTest_Assigned: // 指定玩家手牌
		mlog.Debug("JLB_JPTest_Assigned")
		// 先將指定手牌從遊戲牌組中抽出
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_Normal {
				if pJLB.ary_pTestUserCards[iSeat].CardCount() > 0 {
					pSet := new(pokerset.PokerCardSet)
					pSet.SetCardSet(pJLB.ary_pTestUserCards[iSeat].CodeString())
					pJLB.pPokerSet.RemoveCardSet(pSet)
				}
			}
		}

		if pJLB.pTestDealerCards.CardCount() > 0 {
			pSet := new(pokerset.PokerCardSet)
			pSet.SetCardSet(pJLB.pTestDealerCards.CodeString())
			pJLB.pPokerSet.RemoveCardSet(pSet)
		}

		// 發牌
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_Normal {
				if pJLB.ary_pTestUserCards[iSeat].CardCount() > 0 {
					pJLB.ary_pHandCards[iSeat].SetCardSet(pJLB.ary_pTestUserCards[iSeat].CodeString())
				} else {
					pSet := pJLB.pPokerSet.Deal(JLBPK_HandCardCount)
					pJLB.ary_pHandCards[iSeat].SetCardSet(pSet.CodeString())
				}
			}
		}

		if pJLB.pTestDealerCards.CardCount() > 0 {
			pJLB.pDealerHandCards.SetCardSet(pJLB.pTestDealerCards.CodeString())
		} else {
			pSet := pJLB.pPokerSet.Deal(JLBPK_HandCardCount)
			pJLB.pDealerHandCards.SetCardSet(pSet.CodeString())
		}
	default:
		mlog.Debug("default")
		// 發玩家手牌
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_Normal {
				pSet := pJLB.pPokerSet.Deal(JLBPK_HandCardCount)
				pJLB.ary_pHandCards[iSeat].SetCardSet(pSet.CodeString())
			}
		}

		// 發莊家牌，莊家牌不排序
		pSet := pJLB.pPokerSet.Deal(JLBPK_HandCardCount)
		pJLB.pDealerHandCards.SetCardSet(pSet.CodeString())
	}

	// 排序玩家手牌
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pJLB.ary_pHandCards[iSeat].Sort(false, true, true)
	}

	// 莊家牌是否成局
	if pJLB.pDealerHandCards.Pattern(true) == pokerset.POKER_Pattern_None {
		pJLB.bDealerIsGood = false
	} else {
		pJLB.bDealerIsGood = true
	}

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("Seat[%d], User[%d], %s[%s]", i, pJLB.ary_pUserData[i].i32UserID,
			pJLB.ary_pHandCards[i].String(), pJLB.ary_pHandCards[i].PatternString(false))
	}
	mlog.Debug("莊家: %s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))
	mlog.Debug("PoketSet left[%d]", pJLB.pPokerSet.CardCount())

	// 傳送訊息給 client
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			continue
		}

		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_Deal)

		pData := new(netproto.JLBPK_Deal)
		// 莊家手牌
		pDealerCard := new(netproto.JLBPK_UserCards)
		for iIdx := 0; iIdx < pJLB.pDealerHandCards.CardCount(); iIdx++ {
			pCard := new(netproto.PokerCard)
			if iIdx == 0 {
				pSCard := pJLB.pDealerHandCards.GetSingleCard(iIdx, false)
				pCard.Color = proto.Int32(pSCard.I32Color)
				pCard.CardNumber = proto.Int32(pSCard.I32Number)
			} else {
				pCard.Color = proto.Int32(pokerset.POKER_CardColor_Other)
				pCard.CardNumber = proto.Int32(pokerset.POKER_CardNo_Hide)
			}
			pDealerCard.Cards = append(pDealerCard.Cards, pCard)
		}
		pDealerCard.Pattern = proto.Int32(pJLB.pDealerHandCards.Pattern(true))
		pData.BankerCards = pDealerCard
		// 所有玩家資訊
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, iSeat)
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	}

	pJLB.GameLog.AddGameStep("发牌")
	for i := 0; i < pJLB.iSeatCount; i++ {
		if pJLB.ary_pUserData[i].iUserState != JLB_UserState_EmptySeat {
			pJLB.GameLog.AddGameStep("玩家[%d], 状态[%s]：%s, 牌型[%s]", pJLB.ary_pUserData[i].i32UserID,
				map_strUserState[pJLB.ary_pUserData[i].iUserState], pJLB.ary_pHandCards[i].String(),
				pJLB.ary_pHandCards[i].PatternString(false))
		}
	}
	pJLB.GameLog.AddGameStep("庄家：%s, 牌型[%s]",
		pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))

	pJLB.uiPlayerReady = 0

	for i := 0; i < pJLB.iSeatCount; i++ {
		if pJLB.ary_pUserData[i].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[i].iUserType == gamelogic.UserType_Rebot ||
			pJLB.ary_pUserData[i].bOffline {
			pJLB.ary_pUserData[i].iPlayingState = JLB_UserPlayState_DealOK
			pJLB.uiPlayerReady |= 1 << uint(i)
		}
	}
}

// 訊息處理 - 發牌動畫播放完成
func (pJLB *JlbpkLogic) message_DealOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_Deal {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_StopBettingOK {
		return
	}

	mlog.Debug("User[%d] 發牌動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = JLB_UserPlayState_DealOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家發牌動畫播放完成")
		pJLB.setGameState(JLB_State_BetDouble, pJLB.map_i64Timeout[JLB_Timeout_DoubleBetting])
		pJLB.betDouble()
	}
}

// 玩家開始加注
func (pJLB *JlbpkLogic) betDouble() {
	mlog.Debug("玩家開始加注")

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_DoubleBetStart)

	pData := new(netproto.JLBPK_DoubleBetStart)
	pData.TimeRemain = proto.Int32(int32((pJLB.i64Timeout - pJLB.i64TimeDuration + pJLB.i64LastMessageTime) / 10)) // 剩餘時間

	pMsg.MsgData = pData
	pJLB.BoardcastMessage(pMsg)

	pJLB.GameLog.AddGameStep("玩家开始加注")

	// 機器人操作：有對子就加注，無AK就有機率棄牌
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot && pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_Normal {
			// 設定觸發時間
			pJLB.ary_pUserData[iSeat].SetRobotTimeout(pJLB.map_i64Timeout[JLB_Timeout_DoubleBetting] - 1)
			if pJLB.ary_pUserData[iSeat].i64Timeout > 0 {
				pJLB.ary_pUserData[iSeat].i64LastMessageTime = pJLB.i64TimeDuration
			}
		}
	}

	pJLB.uiPlayerReady = 0

	for i := 0; i < pJLB.iSeatCount; i++ {
		pJLB.ary_pUserData[i].iPlayingState = JLB_UserPlayState_BetDouble
	}
}

// 訊息處理 - 玩家加注
func (pJLB *JlbpkLogic) message_DoubleBet(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_BetDouble {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_BetDouble {
		return
	}

	if pUser.i64BetDouble > 0 {
		pJLB.sendDoubleBetResult(pUser, JLB_DoublebetResult_BetDoubleHasDone)
		return
	}

	i64Doublebet := pUser.i64BetBase * 2

	if pUser.i64Money < i64Doublebet {
		pJLB.sendDoubleBetResult(pUser, JLB_DoubleBetResult_NotEnoughMoney)
		pJLB.sendUserFoldCard(pUser)
		return
	}

	pUser.i64BetDouble += i64Doublebet
	pUser.i64TotalBet += i64Doublebet
	pUser.i64Money -= i64Doublebet
	pUser.i64TotalWinLose -= i64Doublebet
	pJLB.sendDoubleBetResult(pUser, JLB_DoubleBetResult_Success)
}

// 傳送玩家加注結果
func (pJLB *JlbpkLogic) sendDoubleBetResult(pUser *JlbpkUser, iResult int) {
	if pUser == nil {
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_DoubleBetResult)

	pData := new(netproto.JLBPK_DoubleBetResult)
	pData.SeatNo = proto.Int32(pUser.i32SeatNo + int32(JLBPK_ClientStartUpSeatNo)) // 加注玩家座位
	pData.MoneyRemain = proto.Int64(pUser.i64Money)                                // 加注後玩家剩餘金錢
	pData.Result = proto.Int32(int32(iResult))                                     // 加注結果
	pData.Bet = proto.Int64(pUser.i64BetDouble)                                    // 加注金額

	pMsg.MsgData = pData
	if iResult == JLB_DoubleBetResult_Success {
		pJLB.BoardcastMessage(pMsg)
	} else {
		pJLB.SendMessageToUser(pUser.User, pMsg)
	}

	example.invalid("玩家[%d] 加注结果：加注 %s, %s, 剩余金钱 %s", pUser.i32UserID,
		Global.MoneyString(pUser.i64BetDouble), map_strBetResult[iResult],
		Global.MoneyString(pUser.i64Money))
	pJLB.GameLog.AddGameStep("玩家[%d] 加注结果：加注 %s, %s, 剩余金钱 %s", pUser.i32UserID,
		Global.MoneyString(pUser.i64BetDouble), map_strBetResult[iResult],
		Global.MoneyString(pUser.i64Money))
}

// 訊息處理 - 棄牌
func (pJLB *JlbpkLogic) message_Fold(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_BetDouble {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_BetDouble {
		return
	}

	pUser.iUserState = JLB_UserState_Fold
	pUser.i64MoneyChange += -pUser.i64BetBase
	pJLB.sendUserFoldCard(pUser)
}

// 傳送玩家棄牌訊息
func (pJLB *JlbpkLogic) sendUserFoldCard(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_UserFoldCard)

	pData := new(netproto.JLBPK_UserFoldCard)
	pData.SeatNo = proto.Int32(pUser.i32SeatNo + int32(JLBPK_ClientStartUpSeatNo)) // 棄牌玩家座位

	pMsg.MsgData = pData
	pJLB.BoardcastMessage(pMsg)

	mlog.Debug("玩家[%d] 弃牌", pUser.i32UserID)
	pJLB.GameLog.AddGameStep("玩家[%d] 弃牌", pUser.i32UserID)
}

// 更新 Jackpot 金額
func (pJLB *JlbpkLogic) sendUpdateJackpot() {
	pMsg := new(msg.Message)
	pMsg.BClassID = int32(netproto.MessageBClassID_Game)
	pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_UpdateJackpot)

	pData := new(netproto.JLBPK_UpdateJackpot)
	pData.JPMoney = proto.Int64(g_i64Jackpot) // Jackpot 金額

	pMsg.MsgData = pData
	pJLB.BoardcastMessage(pMsg)
}

// 開牌，邊注結算
func (pJLB *JlbpkLogic) showCards() {
	mlog.Debug("開牌，邊注結算")
	pJLB.GameLog.AddGameStep("开牌，边注结算")

	// 未加注玩家棄牌
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		if pUser.iUserState == JLB_UserState_Normal &&
			pUser.i64BetDouble <= 0 {
			pUser.iUserState = JLB_UserState_Fold
			pUser.i64MoneyChange += -pUser.i64BetBase
			pJLB.sendUserFoldCard(pUser)
		}
	}

	// 計算彩金
	mlog.Debug("原彩金金额 %s", Global.MoneyString(g_i64Jackpot))
	pJLB.GameLog.AddGameStep("原彩金金额 %s", Global.MoneyString(g_i64Jackpot))

	allJPMoney := int64(0)
	baseJackpotGap := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		if pUser.iUserState == JLB_UserState_Normal || pUser.iUserState == JLB_UserState_Fold {
			mlog.Debug("玩家[%d] 原来金钱 %s", pUser.i32SeatNo, Global.MoneyString(pUser.i64Money))

			bGetJP, i64JPMoney := pJLB.checkIfGetJackpot(iSeat)
			pUser.bGetJackpot = bGetJP
			pUser.i64JPMoney = i64JPMoney
			g_i64Jackpot -= pUser.i64JPMoney
			allJPMoney += pUser.i64JPMoney
			pJLB.sendUpdateJackpot() // 更新獎池金額

			i64MoneyChange := pUser.i64JPMoney - pUser.i64BetSide
			pUser.i64WinLoseMoney = pUser.i64JPMoney - pUser.i64BetSide
			pUser.i64TotalWinLose += pUser.i64JPMoney
			pUser.i64Money += pUser.i64JPMoney
			pUser.i64MoneyChange += i64MoneyChange

			// 寫分
			if pUser.iUserType != gamelogic.UserType_Rebot {
				pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, 0)
			}

			str := ""
			switch pUser.iUserState {
			case JLB_UserState_Fold:
				str = fmt.Sprintf("玩家[%d] 弃牌, 边注 %s, %s[%s], ", pUser.i32UserID, Global.MoneyString(pUser.i64BetSide),
					pJLB.ary_pHandCards[iSeat].String(), pokerset.PatternString(pJLB.ary_pHandCards[iSeat].Pattern(false)))
			default:
				str = fmt.Sprintf("玩家[%d] 边注 %s, %s[%s], ", pUser.i32UserID, Global.MoneyString(pUser.i64BetSide),
					pJLB.ary_pHandCards[iSeat].String(), pokerset.PatternString(pJLB.ary_pHandCards[iSeat].Pattern(false)))
			}

			if !bGetJP {
				str += fmt.Sprintf("未拉中彩金, 输赢 %s, 结算后金钱 %s", Global.MoneyString(i64MoneyChange), Global.MoneyString(pUser.i64Money))
			} else {
				str += fmt.Sprintf("拉中彩金 %s, 输赢 %s, 结算后金钱 %s", Global.MoneyString(pUser.i64JPMoney),
					Global.MoneyString(pUser.i64WinLoseMoney), Global.MoneyString(pUser.i64Money))
			}

			mlog.Debug("%s", str)
			mlog.Debug("奖池剩余彩金 %s", Global.MoneyString(g_i64Jackpot))
			pJLB.GameLog.AddGameStep("%s", str)
			pJLB.GameLog.AddGameStep("奖池剩余彩金 %s", Global.MoneyString(g_i64Jackpot))

			if g_i64Jackpot < g_i64JackpotBase {
				baseJackpotGap += g_i64JackpotBase - g_i64Jackpot
				g_i64Jackpot = g_i64JackpotBase
				pJLB.sendUpdateJackpot() // 更新獎池金額
				mlog.Debug("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
				pJLB.GameLog.AddGameStep("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
			}
		}
	}

	// 收邊注
	i64BetSide := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserType != gamelogic.UserType_Rebot {
			i64BetSide += pJLB.ary_pUserData[iSeat].i64BetSide
		}
	}
	mlog.Debug("原彩金金额 %s，共计收取边注 %s", Global.MoneyString(g_i64Jackpot), Global.MoneyString(i64BetSide))
	pJLB.GameLog.AddGameStep("原彩金金额 %s，共计收取边注 %s", Global.MoneyString(g_i64Jackpot), Global.MoneyString(i64BetSide))

	g_i64Jackpot += i64BetSide

	pJLB.sendUpdateJackpot() // 更新獎池金額

	// 更新DB彩金池
	pJLB.UpdateJackpotPool(0, 0, 0, i64BetSide-allJPMoney+baseJackpotGap)

	mlog.Debug("收取边注后彩金金额 %s", Global.MoneyString(g_i64Jackpot))
	pJLB.GameLog.AddGameStep("收取边注后彩金金额 %s", Global.MoneyString(g_i64Jackpot))

	// 傳送訊息給 client
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			continue
		}

		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_ShowCards)

		pData := new(netproto.JLBPK_ShowCards)
		// 莊家手牌
		pDealerCard := new(netproto.JLBPK_UserCards)
		for iIdx := 0; iIdx < pJLB.pDealerHandCards.CardCount(); iIdx++ {
			pCard := new(netproto.PokerCard)
			pSCard := pJLB.pDealerHandCards.GetSingleCard(iIdx, false)
			pCard.Color = proto.Int32(pSCard.I32Color)
			pCard.CardNumber = proto.Int32(pSCard.I32Number)

			pDealerCard.Cards = append(pDealerCard.Cards, pCard)
		}
		pDealerCard.Pattern = proto.Int32(pJLB.pDealerHandCards.Pattern(true))
		pData.BankerCards = pDealerCard
		// 所有玩家資訊
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, iSeat)
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	}

	pJLB.uiPlayerReady = 0

	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pJLB.ary_pUserData[iSeat].bOffline {
			pJLB.ary_pUserData[iSeat].iPlayingState = JLB_UserPlayState_ShowCardsOK
			pJLB.uiPlayerReady |= 1 << uint(iSeat)
		}

		// 邊注結算完，將邊注值歸零
		//pJLB.ary_pUserData[iSeat].i64BetSide = 0
	}
}

// 訊息處理 - 開牌，邊注結算效果完成
func (pJLB *JlbpkLogic) message_ShowCardsOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_BetSideSettlement {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_BetDouble {
		return
	}

	mlog.Debug("User[%d] 邊注結算動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = JLB_UserPlayState_ShowCardsOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家邊注結算效果完成")
		pJLB.setGameState(JLB_State_BetBaseSettlement, pJLB.map_i64Timeout[JLB_Timeout_BetBaseSettlement])
		pJLB.settlement_BetBase()
	}
}

// 檢查是否獲得彩金金額，回傳值［是否中彩金］、［所中彩金金額］
func (pJLB *JlbpkLogic) checkIfGetJackpot(iSeatNo int) (bool, int64) {
	if iSeatNo < 0 || iSeatNo >= pJLB.iSeatCount {
		return false, 0
	}

	if pJLB.ary_pUserData[iSeatNo].i64BetSide <= 0 {
		return false, 0
	}

	if pJLB.ary_pUserData[iSeatNo].iUserState != JLB_UserState_Normal {
		return false, 0
	}

	bGetJP, i64JPMoney, i64Jackpot := false, int64(0), g_i64Jackpot

	i32Pattern := pJLB.ary_pHandCards[iSeatNo].Pattern(false)
	if i64Value, ok := pJLB.map_i64JPSheet[i32Pattern]; ok {
		switch i32Pattern {
		case pokerset.POKER_Pattern_RoyalFlush: // 皇家同花順
			fallthrough
		case pokerset.POKER_Pattern_StraightFlush: // 同花順
			bGetJP = true
			i64JPMoney = i64Jackpot * i64Value / 100
		default:
			bGetJP = true
			i64JPMoney = i64Value
			// 若是複數邊注
			if g_iPluralBetSide == PluralBetSideEnable {
				i64JPMoney *= pJLB.ary_pUserData[iSeatNo].i64BetSide / g_i64DefaultBetSide
			}
		}
	}

	return bGetJP, i64JPMoney
}

// 檢查是否獲得莊家彩金金額，回傳值［是否中彩金］、［所中彩金金額］
func (pJLB *JlbpkLogic) checkIfGetZhuangJackpot(iSeatNo int) (bool, int64) {
	if iSeatNo < 0 || iSeatNo >= pJLB.iSeatCount {
		return false, 0
	}

	if pJLB.ary_pUserData[iSeatNo].i64ZhuangBetSide <= 0 {
		return false, 0
	}

	if pJLB.ary_pUserData[iSeatNo].iUserState != JLB_UserState_Normal {
		return false, 0
	}

	bGetJP, i64JPMoney, i64Jackpot := false, int64(0), g_i64Jackpot

	i32Pattern := pJLB.pDealerHandCards.Pattern(false)
	if i64Value, ok := pJLB.map_i64JPSheet[i32Pattern]; ok {
		switch i32Pattern {
		case pokerset.POKER_Pattern_RoyalFlush: // 皇家同花順
			fallthrough
		case pokerset.POKER_Pattern_StraightFlush: // 同花順
			bGetJP = true
			i64JPMoney = i64Jackpot * i64Value / 100
		default:
			bGetJP = true
			i64JPMoney = i64Value
			// 若是複數邊注
			if g_iPluralBetSide == PluralBetSideEnable {
				i64JPMoney *= pJLB.ary_pUserData[iSeatNo].i64ZhuangBetSide / g_i64DefaultBetSide
			}
		}
	}

	return bGetJP, i64JPMoney
}

// 賠付，底注結算
func (pJLB *JlbpkLogic) settlement_BetBase() {
	mlog.Debug("赔付，底注结算")
	pJLB.GameLog.AddGameStep("赔付，底注结算")

	//----- 底注結算 -----
	pDealerSet := pJLB.pDealerHandCards.GetPokerCardSet()
	mlog.Debug("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))
	pJLB.GameLog.AddGameStep("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))

	i64BetBaseWinLose := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		switch pUser.iUserState {
		case JLB_UserState_Normal: // 正常
			pPlayerSet := pJLB.ary_pHandCards[iSeat].GetPokerCardSet()
			if !pJLB.bDealerIsGood {
				// 莊家不成局
				i64BetBaseWinLose = pUser.i64BetBase * int64(1+JLBPK_BetBasePayRate)
				pUser.i64WinLoseMoney = i64BetBaseWinLose - pUser.i64BetBase
				pUser.i64TotalWinLose += i64BetBaseWinLose
				pUser.i64Money += i64BetBaseWinLose
				pUser.iWinState = JLB_WinState_Win
			} else {
				iCmpResult := pJLB.pPokerSet.Compare(pDealerSet, pPlayerSet, false, false)
				switch iCmpResult {
				case pokerset.POKER_Compare_Even: // 平手
					i64BetBaseWinLose = pUser.i64BetBase
					pUser.iWinState = JLB_WinState_Even
				case pokerset.POKER_Compare_Dealer: // 莊家贏
					i64BetBaseWinLose = -pUser.i64BetBase
					pUser.iWinState = JLB_WinState_Lose
				case pokerset.POKER_Compare_Player: // 玩家贏
					i64BetBaseWinLose = pUser.i64BetBase * int64(1+JLBPK_BetBasePayRate)
					pUser.iWinState = JLB_WinState_Win
				}

				if iCmpResult == pokerset.POKER_Compare_Dealer {
					pUser.i64WinLoseMoney = i64BetBaseWinLose
					pUser.i64TotalBet -= pUser.i64BetBase
				} else {
					pUser.i64WinLoseMoney = i64BetBaseWinLose - pUser.i64BetBase
					pUser.i64Money += i64BetBaseWinLose
					pUser.i64TotalWinLose += i64BetBaseWinLose
				}
			}

			// 寫分
			i64WinLose := i64BetBaseWinLose
			if i64BetBaseWinLose > 0 {
				i64WinLose = i64BetBaseWinLose - pUser.i64BetBase
			}
			i64MoneyChange, i64Tax := pJLB.GetUserWinAndTax(i64WinLose)
			pUser.i64TotalWinLose -= i64Tax
			pUser.i64Money -= i64Tax
			pUser.i64MoneyChange += i64MoneyChange

			i64Flow := int64(0)
			if i64MoneyChange > 0 {
				i64Flow = i64MoneyChange
			} else {
				i64Flow = -i64MoneyChange
			}
			if pUser.iUserType != gamelogic.UserType_Rebot {
				pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax)
			}

			mlog.Debug("玩家[%d], %s[%s], 底注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
				Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d], %s[%s], 底注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
				Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
		case JLB_UserState_Fold: // 棄牌
			i64BetBaseWinLose = -pUser.i64BetBase
			pUser.i64WinLoseMoney = i64BetBaseWinLose
			pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64BetBaseWinLose, 0)
			pUser.iWinState = JLB_WinState_Lose

			i64Flow := int64(0)
			if i64BetBaseWinLose > 0 {
				i64Flow = i64BetBaseWinLose
			} else {
				i64Flow = -i64BetBaseWinLose
			}
			mlog.Debug("玩家[%d] 弃牌, %s[%s], 底注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64BetBaseWinLose), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d] 弃牌, %s[%s], 底注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64BetBaseWinLose), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
		}
	}

	//----- 傳送訊息給 client -----
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			continue
		}

		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_Compensate_BetBase)

		pData := new(netproto.JLBPK_Compensate_BetBase)
		// 所有玩家資訊
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, iSeat)
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	}

	pJLB.uiPlayerReady = 0

	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pJLB.ary_pUserData[iSeat].bOffline {
			pJLB.ary_pUserData[iSeat].iPlayingState = JLB_UserPlayState_Compensate_BetBaseOK
			pJLB.uiPlayerReady |= 1 << uint(iSeat)
		}

		// 底注結算完，將底注值歸零
		//pJLB.ary_pUserData[iSeat].i64BetBase = 0
	}
}

// 訊息處理 - 底注結算效果完成
func (pJLB *JlbpkLogic) message_BetBaseSettlementOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_BetBaseSettlement {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_ShowCardsOK {
		return
	}

	mlog.Debug("User[%d] 底注結算動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = JLB_UserPlayState_Compensate_BetBaseOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家底注結算效果完成")
		pJLB.setGameState(JLB_State_BetDoubleSettlement, pJLB.map_i64Timeout[JLB_Timeout_BetDoubleSettlement])
		pJLB.settlement_BetDouble()
	}
}

// 賠付，加注結算
func (pJLB *JlbpkLogic) settlement_BetDouble() {
	mlog.Debug("賠付，加注結算")
	pJLB.GameLog.AddGameStep("赔付，加注结算")

	//----- 加注結算 -----
	pDealerSet := pJLB.pDealerHandCards.GetPokerCardSet()
	// 莊家是否成局
	mlog.Debug("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))
	pJLB.GameLog.AddGameStep("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))

	i64DoubleWinLose := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		switch pUser.iUserState {
		case JLB_UserState_Normal: // 正常
			pPlayerSet := pJLB.ary_pHandCards[iSeat].GetPokerCardSet()
			if !pJLB.bDealerIsGood {
				// 莊家不成局
				i64DoubleWinLose = pUser.i64BetDouble
				pUser.i64WinLoseMoney = i64DoubleWinLose - pUser.i64BetDouble
				pUser.i64TotalWinLose += i64DoubleWinLose
				pUser.i64Money += i64DoubleWinLose
			} else {
				iCmpResult := pJLB.pPokerSet.Compare(pDealerSet, pPlayerSet, false, false)
				switch iCmpResult {
				case pokerset.POKER_Compare_Even: // 平手
					i64DoubleWinLose = pUser.i64BetDouble
				case pokerset.POKER_Compare_Dealer: // 第一組牌
					i64DoubleWinLose = -pUser.i64BetDouble
				case pokerset.POKER_Compare_Player: // 第二組牌
					i64DoubleWinLose = pUser.i64BetDouble * (1 + pJLB.map_i64SpecialSheet[pJLB.ary_pHandCards[iSeat].Pattern(false)])
				}

				if iCmpResult == pokerset.POKER_Compare_Dealer {
					pUser.i64WinLoseMoney = i64DoubleWinLose
					pUser.i64TotalBet -= pUser.i64BetDouble
				} else {
					pUser.i64WinLoseMoney = i64DoubleWinLose - pUser.i64BetDouble
					pUser.i64Money += i64DoubleWinLose
					pUser.i64TotalWinLose += i64DoubleWinLose
				}
			}

			// 寫分
			i64WinLose := i64DoubleWinLose
			if i64DoubleWinLose > 0 {
				i64WinLose = i64DoubleWinLose - pUser.i64BetDouble
			}
			i64MoneyChange, i64Tax := pJLB.GetUserWinAndTax(i64WinLose)
			pUser.i64TotalWinLose -= i64Tax
			pUser.i64Money -= i64Tax
			pUser.i64MoneyChange += i64MoneyChange

			i64Flow := int64(0)
			if i64MoneyChange > 0 {
				i64Flow = i64MoneyChange
			} else {
				i64Flow = -i64MoneyChange
			}

			if pUser.iUserType != gamelogic.UserType_Rebot {
				pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax)
			}

			if pJLB.bDealerIsGood {
				mlog.Debug("玩家[%d], %s[%s], 加注 %s, 赔注[1赔%d], 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), pJLB.map_i64SpecialSheet[pJLB.ary_pHandCards[iSeat].Pattern(false)],
					Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
				pJLB.GameLog.AddGameStep("玩家[%d], %s[%s], 加注 %s, 赔注[1赔%d], 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), pJLB.map_i64SpecialSheet[pJLB.ary_pHandCards[iSeat].Pattern(false)],
					Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			} else {
				mlog.Debug("玩家[%d], %s[%s], 加注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
					Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
				pJLB.GameLog.AddGameStep("玩家[%d], %s[%s], 加注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
					Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			}
		case JLB_UserState_Fold: // 棄牌
			i64DoubleWinLose = -pUser.i64BetDouble
			pUser.i64WinLoseMoney = i64DoubleWinLose
			pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64DoubleWinLose, 0)

			i64Flow := int64(0)
			if i64DoubleWinLose > 0 {
				i64Flow = i64DoubleWinLose
			} else {
				i64Flow = -i64DoubleWinLose
			}
			mlog.Debug("玩家[%d] 弃牌, %s[%s], 加注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64DoubleWinLose), Global.MoneyString(i64Flow),
				Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d] 弃牌, %s[%s], 加注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64DoubleWinLose), Global.MoneyString(i64Flow),
				Global.MoneyString(pUser.i64Money))
		}
	}

	//----- 傳送訊息給 client -----
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			continue
		}

		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_Compensate_DoubleBet)

		pData := new(netproto.JLBPK_Compensate_DoubleBet)
		// 所有玩家資訊
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, iSeat)
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	}

	pJLB.uiPlayerReady = 0

	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pJLB.ary_pUserData[iSeat].bOffline {
			pJLB.ary_pUserData[iSeat].iPlayingState = JLB_UserPlayState_Compensate_DoubleBetOK
			pJLB.uiPlayerReady |= 1 << uint(iSeat)
		}

		// 加注結算完，將加注值歸零
		//pJLB.ary_pUserData[iSeat].i64JPMoney = 0
	}
}

// 訊息處理 - 加注結算效果完成
func (pJLB *JlbpkLogic) message_BetDoubleSettlementOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != JLB_State_BetDoubleSettlement {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_Compensate_BetBaseOK {
		return
	}

	mlog.Debug("User[%d] 加注結算動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = JLB_UserPlayState_Compensate_DoubleBetOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家加注結算效果完成")
		pJLB.setGameState(JLB_State_GameOver, 0)
		pJLB.gameOver()
	}
}

// 遊戲結束
func (pJLB *JlbpkLogic) gameOver() {
	mlog.Debug("游戏结束")
	pJLB.GameLog.AddGameStep("游戏结束")

	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].User != nil {
			if pJLB.ary_pUserData[iSeat].i64MoneyChange >= 0 {
				pJLB.ary_pUserData[iSeat].User.FlowAmount = pJLB.ary_pUserData[iSeat].i64MoneyChange
			} else {
				pJLB.ary_pUserData[iSeat].User.FlowAmount = -pJLB.ary_pUserData[iSeat].i64MoneyChange
			}
		}
	}

	map_i64ScoreRet := make(map[int32]int64)
	map_i64CouponRet := make(map[int32]int64)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].User != nil {
			map_i64ScoreRet[pJLB.ary_pUserData[iSeat].i32UserID] = pJLB.ary_pUserData[iSeat].i64MoneyChange
			map_i64CouponRet[pJLB.ary_pUserData[iSeat].i32UserID] = 0
		}
	}
	pJLB.GameLog.GameEnd(map_i64ScoreRet)

	pJLB.GameEndFunc()

	iUserCount := 0
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			pJLB.ary_pUserData[iSeat].Init()
		} else {
			if pGameUser.IsOffLine() {
				pJLB.UserLeave(pGameUser)
				pJLB.ary_pUserData[iSeat].Init()
			} else {
				i32LoginMoney := pJLB.GetTable().Room.LoginMoney
				if pJLB.ary_pUserData[iSeat].i64Money < int64(i32LoginMoney) {
					pJLB.GameLogicBase.KickUserFunc(pGameUser.UserID, "您的金币不足!请补充金币后再进行游戏.")
					pJLB.ary_pUserData[iSeat].Init()
				} else {
					pJLB.ary_pUserData[iSeat].ResetGameData()
					pJLB.ary_pUserData[iSeat].iUserState = JLB_UserState_Normal
					iUserCount++
				}
			}
		}
	}
	pJLB.iUserCount = iUserCount
	mlog.Debug("gameOver, 玩家人數：%d", pJLB.iUserCount)

	pJLB.initData()
	pJLB.sendAllUserInfo()
}

// 總結算
func (pJLB *JlbpkLogic) settlement() {
	// 未加注玩家棄牌
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		if pUser.iUserState == JLB_UserState_Normal &&
			pUser.i64BetDouble <= 0 {
			pUser.iUserState = JLB_UserState_Fold
			pUser.i64MoneyChange += -pUser.i64BetBase
			pJLB.sendUserFoldCard(pUser)
		}
	}

	//----- 邊注結算 -----
	mlog.Debug("開牌，邊注結算")
	pJLB.GameLog.AddGameStep("开牌，边注结算")

	// 計算彩金
	mlog.Debug("原彩金金额 %s", Global.MoneyString(g_i64Jackpot))
	pJLB.GameLog.AddGameStep("原彩金金额 %s", Global.MoneyString(g_i64Jackpot))

	allJPMoney := int64(0)
	baseJackpotGap := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		if pUser.iUserState == JLB_UserState_Normal || pUser.iUserState == JLB_UserState_Fold {
			mlog.Debug("玩家[%d] 原来金钱 %s", pUser.i32SeatNo, Global.MoneyString(pUser.i64Money))

			bGetJP, i64JPMoney := pJLB.checkIfGetJackpot(iSeat)
			pUser.bGetJackpot = bGetJP
			pUser.i64JPMoney = i64JPMoney
			g_i64Jackpot -= pUser.i64JPMoney
			allJPMoney += pUser.i64JPMoney
			pJLB.sendUpdateJackpot() // 更新獎池金額

			i64MoneyChange := pUser.i64JPMoney - pUser.i64BetSide
			pUser.i64WinLoseMoney = pUser.i64JPMoney - pUser.i64BetSide
			pUser.i64TotalWinLose += pUser.i64JPMoney
			pUser.i64Money += pUser.i64JPMoney
			pUser.i64MoneyChange += i64MoneyChange

			// 寫分
			if pUser.iUserType != gamelogic.UserType_Rebot {
				pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, 0)
			}

			str := ""
			switch pUser.iUserState {
			case JLB_UserState_Fold:
				str = fmt.Sprintf("玩家[%d] 弃牌, 边注 %s, %s[%s], ", pUser.i32UserID, Global.MoneyString(pUser.i64BetSide),
					pJLB.ary_pHandCards[iSeat].String(), pokerset.PatternString(pJLB.ary_pHandCards[iSeat].Pattern(false)))
			default:
				str = fmt.Sprintf("玩家[%d] 边注 %s, %s[%s], ", pUser.i32UserID, Global.MoneyString(pUser.i64BetSide),
					pJLB.ary_pHandCards[iSeat].String(), pokerset.PatternString(pJLB.ary_pHandCards[iSeat].Pattern(false)))
			}

			if !bGetJP {
				str += fmt.Sprintf("未拉中彩金, 输赢 %s, 结算后金钱 %s", Global.MoneyString(i64MoneyChange), Global.MoneyString(pUser.i64Money))
			} else {
				str += fmt.Sprintf("拉中彩金 %s, 输赢 %s, 结算后金钱 %s", Global.MoneyString(pUser.i64JPMoney),
					Global.MoneyString(pUser.i64WinLoseMoney), Global.MoneyString(pUser.i64Money))
			}

			mlog.Debug("%s", str)
			mlog.Debug("奖池剩余彩金 %s", Global.MoneyString(g_i64Jackpot))
			pJLB.GameLog.AddGameStep("%s", str)
			pJLB.GameLog.AddGameStep("奖池剩余彩金 %s", Global.MoneyString(g_i64Jackpot))

			if g_i64Jackpot < g_i64JackpotBase {
				baseJackpotGap += g_i64JackpotBase - g_i64Jackpot
				g_i64Jackpot = g_i64JackpotBase
				pJLB.sendUpdateJackpot() // 更新獎池金額
				mlog.Debug("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
				pJLB.GameLog.AddGameStep("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
			}
		}
	}

	// 收邊注
	i64BetSide := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserType != gamelogic.UserType_Rebot {
			i64BetSide += pJLB.ary_pUserData[iSeat].i64BetSide
		}
	}
	mlog.Debug("原彩金金额 %s，共计收取边注 %s", Global.MoneyString(g_i64Jackpot), Global.MoneyString(i64BetSide))
	pJLB.GameLog.AddGameStep("原彩金金额 %s，共计收取边注 %s", Global.MoneyString(g_i64Jackpot), Global.MoneyString(i64BetSide))

	g_i64Jackpot += i64BetSide

	// 莊家邊注彩池計算
	i64ZhuangBetSide := int64(0)
	if g_iZhuangBetSide == ZhuangBetSideEnable {
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			pUser := pJLB.ary_pUserData[iSeat]
			if pUser.iUserState == JLB_UserState_Normal || pUser.iUserState == JLB_UserState_Fold {
				mlog.Debug("玩家[%d] 原来金钱 %s", pUser.i32SeatNo, Global.MoneyString(pUser.i64Money))

				bGetJP, i64JPMoney := pJLB.checkIfGetZhuangJackpot(iSeat)
				pUser.bGetZhuangJackpot = bGetJP
				pUser.i64ZhuangJPMoney = i64JPMoney
				g_i64Jackpot -= pUser.i64ZhuangJPMoney
				allJPMoney += pUser.i64ZhuangJPMoney
				pJLB.sendUpdateJackpot() // 更新獎池金額

				i64MoneyChange := pUser.i64ZhuangJPMoney - pUser.i64ZhuangBetSide
				pUser.i64WinLoseMoney = pUser.i64ZhuangJPMoney - pUser.i64ZhuangBetSide
				pUser.i64TotalWinLose += pUser.i64ZhuangJPMoney
				pUser.i64Money += pUser.i64ZhuangJPMoney
				pUser.i64MoneyChange += i64MoneyChange

				// 寫分
				if pUser.iUserType != gamelogic.UserType_Rebot {
					pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, 0)
				}

				str := ""
				switch pUser.iUserState {
				case JLB_UserState_Fold:
					str = fmt.Sprintf("玩家[%d] 弃牌, 莊家边注 %s, %s[%s], ", pUser.i32UserID, Global.MoneyString(pUser.i64ZhuangBetSide),
						pJLB.ary_pHandCards[iSeat].String(), pokerset.PatternString(pJLB.ary_pHandCards[iSeat].Pattern(false)))
				default:
					str = fmt.Sprintf("玩家[%d] 莊家边注 %s, %s[%s], ", pUser.i32UserID, Global.MoneyString(pUser.i64ZhuangBetSide),
						pJLB.ary_pHandCards[iSeat].String(), pokerset.PatternString(pJLB.ary_pHandCards[iSeat].Pattern(false)))
				}

				if !bGetJP {
					str += fmt.Sprintf("未拉中莊家彩金, 输赢 %s, 结算后金钱 %s", Global.MoneyString(i64MoneyChange), Global.MoneyString(pUser.i64Money))
				} else {
					str += fmt.Sprintf("拉中莊家彩金 %s, 输赢 %s, 结算后金钱 %s", Global.MoneyString(pUser.i64ZhuangJPMoney),
						Global.MoneyString(pUser.i64WinLoseMoney), Global.MoneyString(pUser.i64Money))
				}

				mlog.Debug("%s", str)
				mlog.Debug("奖池剩余彩金 %s", Global.MoneyString(g_i64Jackpot))
				pJLB.GameLog.AddGameStep("%s", str)
				pJLB.GameLog.AddGameStep("奖池剩余彩金 %s", Global.MoneyString(g_i64Jackpot))

				if g_i64Jackpot < g_i64JackpotBase {
					baseJackpotGap += g_i64JackpotBase - g_i64Jackpot
					g_i64Jackpot = g_i64JackpotBase
					pJLB.sendUpdateJackpot() // 更新獎池金額
					mlog.Debug("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
					pJLB.GameLog.AddGameStep("彩池金额不足基准值 %s, 补足至基准值", Global.MoneyString(g_i64JackpotBase))
				}
			}
		}

		// 收邊注
		for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
			if pJLB.ary_pUserData[iSeat].iUserType != gamelogic.UserType_Rebot {
				i64ZhuangBetSide += pJLB.ary_pUserData[iSeat].i64ZhuangBetSide
			}
		}
		mlog.Debug("原彩金金额 %s，共计收取莊家边注 %s", Global.MoneyString(g_i64Jackpot), Global.MoneyString(i64ZhuangBetSide))
		pJLB.GameLog.AddGameStep("原彩金金额 %s，共计收取莊家边注 %s", Global.MoneyString(g_i64Jackpot), Global.MoneyString(i64ZhuangBetSide))

		g_i64Jackpot += i64ZhuangBetSide
	}

	pJLB.sendUpdateJackpot() // 更新獎池金額

	// 更新DB彩金池
	i64AllBetSide := i64BetSide
	if g_iZhuangBetSide == ZhuangBetSideEnable {
		i64AllBetSide += i64ZhuangBetSide
	}
	pJLB.UpdateJackpotPool(0, 0, 0, i64AllBetSide-allJPMoney+baseJackpotGap)

	mlog.Debug("收取边注后彩金金额 %s", Global.MoneyString(g_i64Jackpot))
	pJLB.GameLog.AddGameStep("收取边注后彩金金额 %s", Global.MoneyString(g_i64Jackpot))

	//----- 底注結算 -----
	mlog.Debug("赔付，底注结算")
	pJLB.GameLog.AddGameStep("赔付，底注结算")

	pDealerSet := pJLB.pDealerHandCards.GetPokerCardSet()
	// 莊家是否成局
	mlog.Debug("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))
	pJLB.GameLog.AddGameStep("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))

	i64BetBaseWinLose := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		switch pUser.iUserState {
		case JLB_UserState_Normal: // 正常
			pPlayerSet := pJLB.ary_pHandCards[iSeat].GetPokerCardSet()
			if !pJLB.bDealerIsGood {
				// 莊家不成局
				i64BetBaseWinLose = pUser.i64BetBase * int64(1+JLBPK_BetBasePayRate)
				pUser.i64WinLoseMoney = i64BetBaseWinLose - pUser.i64BetBase
				pUser.i64BetBaseWinLoseMoney = i64BetBaseWinLose - pUser.i64BetBase
				pUser.i64TotalWinLose += i64BetBaseWinLose
				pUser.i64Money += i64BetBaseWinLose
				pUser.iWinState = JLB_WinState_Win
			} else {
				iCmpResult := pJLB.pPokerSet.Compare(pDealerSet, pPlayerSet, false, false)
				switch iCmpResult {
				case pokerset.POKER_Compare_Even: // 平手
					i64BetBaseWinLose = pUser.i64BetBase
					pUser.iWinState = JLB_WinState_Even
				case pokerset.POKER_Compare_Dealer: // 莊家贏
					i64BetBaseWinLose = -pUser.i64BetBase
					pUser.iWinState = JLB_WinState_Lose
				case pokerset.POKER_Compare_Player: // 玩家贏
					i64BetBaseWinLose = pUser.i64BetBase * int64(1+JLBPK_BetBasePayRate)
					pUser.iWinState = JLB_WinState_Win
				}

				if iCmpResult == pokerset.POKER_Compare_Dealer {
					pUser.i64WinLoseMoney = i64BetBaseWinLose
					pUser.i64BetBaseWinLoseMoney = i64BetBaseWinLose
					pUser.i64TotalBet -= pUser.i64BetBase
				} else {
					pUser.i64WinLoseMoney = i64BetBaseWinLose - pUser.i64BetBase
					pUser.i64BetBaseWinLoseMoney = i64BetBaseWinLose - pUser.i64BetBase
					pUser.i64Money += i64BetBaseWinLose
					pUser.i64TotalWinLose += i64BetBaseWinLose
				}
			}

			// 寫分
			i64WinLose := i64BetBaseWinLose
			if i64BetBaseWinLose > 0 {
				i64WinLose = i64BetBaseWinLose - pUser.i64BetBase
			}
			i64MoneyChange, i64Tax := pJLB.GetUserWinAndTax(i64WinLose)
			pUser.i64TotalWinLose -= i64Tax
			pUser.i64Money -= i64Tax
			pUser.i64MoneyChange += i64MoneyChange

			i64Flow := int64(0)
			if i64MoneyChange > 0 {
				i64Flow = i64MoneyChange
			} else {
				i64Flow = -i64MoneyChange
			}
			if pUser.iUserType != gamelogic.UserType_Rebot {
				pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax)
			}

			mlog.Debug("玩家[%d], %s[%s], 底注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
				Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d], %s[%s], 底注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
				Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
		case JLB_UserState_Fold: // 棄牌
			i64BetBaseWinLose = -pUser.i64BetBase
			pUser.i64WinLoseMoney = i64BetBaseWinLose
			pUser.i64BetBaseWinLoseMoney = i64BetBaseWinLose
			pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64BetBaseWinLose, 0)
			pUser.iWinState = JLB_WinState_Lose

			i64Flow := int64(0)
			if i64BetBaseWinLose > 0 {
				i64Flow = i64BetBaseWinLose
			} else {
				i64Flow = -i64BetBaseWinLose
			}
			mlog.Debug("玩家[%d] 弃牌, %s[%s], 底注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64BetBaseWinLose), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d] 弃牌, %s[%s], 底注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetBase), Global.MoneyString(i64BetBaseWinLose), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
		}
	}

	//----- 加注結算 -----
	mlog.Debug("賠付，加注結算")
	pJLB.GameLog.AddGameStep("赔付，加注结算")

	pDealerSet = pJLB.pDealerHandCards.GetPokerCardSet()
	// 莊家是否成局
	mlog.Debug("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))
	pJLB.GameLog.AddGameStep("庄家：%s[%s]", pJLB.pDealerHandCards.String(), pJLB.pDealerHandCards.PatternString(true))

	i64DoubleWinLose := int64(0)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.ary_pUserData[iSeat]
		switch pUser.iUserState {
		case JLB_UserState_Normal: // 正常
			pPlayerSet := pJLB.ary_pHandCards[iSeat].GetPokerCardSet()
			if !pJLB.bDealerIsGood {
				// 莊家不成局
				i64DoubleWinLose = pUser.i64BetDouble
				pUser.i64WinLoseMoney = i64DoubleWinLose - pUser.i64BetDouble
				pUser.i64BetDoubleWinLoseMoney = i64DoubleWinLose - pUser.i64BetDouble
				pUser.i64TotalWinLose += i64DoubleWinLose
				pUser.i64Money += i64DoubleWinLose
			} else {
				iCmpResult := pJLB.pPokerSet.Compare(pDealerSet, pPlayerSet, false, false)
				switch iCmpResult {
				case pokerset.POKER_Compare_Even: // 平手
					i64DoubleWinLose = pUser.i64BetDouble
				case pokerset.POKER_Compare_Dealer: // 第一組牌
					i64DoubleWinLose = -pUser.i64BetDouble
				case pokerset.POKER_Compare_Player: // 第二組牌
					i64DoubleWinLose = pUser.i64BetDouble * (1 + pJLB.map_i64SpecialSheet[pJLB.ary_pHandCards[iSeat].Pattern(false)])
				}

				if iCmpResult == pokerset.POKER_Compare_Dealer {
					pUser.i64WinLoseMoney = i64DoubleWinLose
					pUser.i64BetDoubleWinLoseMoney = i64DoubleWinLose
					pUser.i64TotalBet -= pUser.i64BetDouble
				} else {
					pUser.i64WinLoseMoney = i64DoubleWinLose - pUser.i64BetDouble
					pUser.i64BetDoubleWinLoseMoney = i64DoubleWinLose - pUser.i64BetDouble
					pUser.i64Money += i64DoubleWinLose
					pUser.i64TotalWinLose += i64DoubleWinLose
				}
			}

			// 寫分
			i64WinLose := i64DoubleWinLose
			if i64DoubleWinLose > 0 {
				i64WinLose = i64DoubleWinLose - pUser.i64BetDouble
			}
			i64MoneyChange, i64Tax := pJLB.GetUserWinAndTax(i64WinLose)
			pUser.i64TotalWinLose -= i64Tax
			pUser.i64Money -= i64Tax
			pUser.i64MoneyChange += i64MoneyChange

			i64Flow := int64(0)
			if i64MoneyChange > 0 {
				i64Flow = i64MoneyChange
			} else {
				i64Flow = -i64MoneyChange
			}

			if pUser.iUserType != gamelogic.UserType_Rebot {
				pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64MoneyChange, i64Tax)
			}

			if pJLB.bDealerIsGood {
				mlog.Debug("玩家[%d], %s[%s], 加注 %s, 赔注[1赔%d], 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), pJLB.map_i64SpecialSheet[pJLB.ary_pHandCards[iSeat].Pattern(false)],
					Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
				pJLB.GameLog.AddGameStep("玩家[%d], %s[%s], 加注 %s, 赔注[1赔%d], 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), pJLB.map_i64SpecialSheet[pJLB.ary_pHandCards[iSeat].Pattern(false)],
					Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax), Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			} else {
				mlog.Debug("玩家[%d], %s[%s], 加注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
					Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
				pJLB.GameLog.AddGameStep("玩家[%d], %s[%s], 加注 %s, 输赢 %s, 税收 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
					pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
					Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64WinLose), Global.MoneyString(i64Tax),
					Global.MoneyString(i64Flow), Global.MoneyString(pUser.i64Money))
			}
		case JLB_UserState_Fold: // 棄牌
			i64DoubleWinLose = -pUser.i64BetDouble
			pUser.i64WinLoseMoney = i64DoubleWinLose
			pUser.i64BetDoubleWinLoseMoney = i64DoubleWinLose
			pJLB.UpdateUserScoreFunc(pUser.User.UserID, i64DoubleWinLose, 0)

			i64Flow := int64(0)
			if i64DoubleWinLose > 0 {
				i64Flow = i64DoubleWinLose
			} else {
				i64Flow = -i64DoubleWinLose
			}
			mlog.Debug("玩家[%d] 弃牌, %s[%s], 加注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64DoubleWinLose), Global.MoneyString(i64Flow),
				Global.MoneyString(pUser.i64Money))
			pJLB.GameLog.AddGameStep("玩家[%d] 弃牌, %s[%s], 加注 %s, 输赢 %s, 流水 %s, 结算后金钱 %s", pUser.i32UserID,
				pJLB.ary_pHandCards[iSeat].String(), pJLB.ary_pHandCards[iSeat].PatternString(false),
				Global.MoneyString(pUser.i64BetDouble), Global.MoneyString(i64DoubleWinLose), Global.MoneyString(i64Flow),
				Global.MoneyString(pUser.i64Money))
		}
	}

	// 傳送訊息給 client
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pGameUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pGameUser == nil {
			continue
		}

		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_Settlement)

		pData := new(netproto.JLBPK_Settlement)
		// 莊家手牌
		pDealerCard := new(netproto.JLBPK_UserCards)
		for iIdx := 0; iIdx < pJLB.pDealerHandCards.CardCount(); iIdx++ {
			pCard := new(netproto.PokerCard)
			pSCard := pJLB.pDealerHandCards.GetSingleCard(iIdx, false)
			pCard.Color = proto.Int32(pSCard.I32Color)
			pCard.CardNumber = proto.Int32(pSCard.I32Number)

			pDealerCard.Cards = append(pDealerCard.Cards, pCard)
		}
		pDealerCard.Pattern = proto.Int32(pJLB.pDealerHandCards.Pattern(true))
		pData.BankerCards = pDealerCard
		// 所有玩家資訊
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, iSeat)
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	}

	pJLB.uiPlayerReady = 0

	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		if pJLB.ary_pUserData[iSeat].iUserState == JLB_UserState_EmptySeat || pJLB.ary_pUserData[iSeat].iUserType == gamelogic.UserType_Rebot ||
			pJLB.ary_pUserData[iSeat].bOffline {
			pJLB.ary_pUserData[iSeat].iPlayingState = DHC_UserPlayState_SettlementOK
			pJLB.uiPlayerReady |= 1 << uint(iSeat)
		}

		// 邊注結算完，將邊注值歸零
		//pJLB.ary_pUserData[iSeat].i64BetSide = 0
	}
}

// 訊息處理 - 總結算效果完成
func (pJLB *JlbpkLogic) message_SettlementOK(pUser *JlbpkUser) {
	if pUser == nil {
		return
	}

	if pJLB.iGameState != DHC_State_Settlement {
		return
	}

	if pUser.iPlayingState != JLB_UserPlayState_BetDouble {
		return
	}

	mlog.Debug("User[%d] 總結算動畫播放完成", pUser.i32UserID)

	pUser.iPlayingState = DHC_UserPlayState_SettlementOK

	for i := 0; i < pJLB.iSeatCount; i++ {
		mlog.Debug("User[%d], seat[%d], UserState[%s], PlayingState[%s]", pJLB.ary_pUserData[i].i32UserID,
			i, map_strUserState_debug[pJLB.ary_pUserData[i].iUserState], map_strPlayingState_debug[pJLB.ary_pUserData[i].iPlayingState])
	}

	pJLB.uiPlayerReady |= 1 << uint(pUser.i32SeatNo)
	if pJLB.uiPlayerReady == pJLB.uiAllPlayerReady {
		mlog.Debug("所有玩家總結算效果完成")
		pJLB.setGameState(JLB_State_GameOver, 0)
		pJLB.gameOver()
	}
}

// 玩家斷線重連
func (pJLB *JlbpkLogic) UserRecover(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}

	mlog.Debug("Table[%d], UserRecover, User[%d], Seat[%d]", pJLB.TableNo(), pGameUser.UserID, pGameUser.SeatNo)
	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pUser != nil {
			mlog.Debug("Seat[%d], pUserData.bIsOffline[%t], pUser[%t]", iSeat, pJLB.ary_pUserData[iSeat].bOffline, pUser.IsOffLine())
		} else {
			mlog.Debug("Seat[%d], pUserData.bIsOffline[%t], pUser nil", iSeat, pJLB.ary_pUserData[iSeat].bOffline)
		}
	}

	iSeat := int(pGameUser.SeatNo)
	if iSeat < 0 || iSeat >= pJLB.iSeatCount {
		return
	}

	pJLB.DynamicGameLogicBase.UserRecover(pGameUser)

	pUser := pJLB.ary_pUserData[iSeat]
	pUser.bOffline = false

	pJLB.sendGameInfo(pGameUser)
	pJLB.sendAllUserInfo()

	switch pJLB.iGameState {
	case JLB_State_StartBettingAnimation: // 開始下注動畫
		// 玩家於此階段離線，未收到 Compensate_DoubleBet，所以重送
		pMsg := new(msg.Message)
		pMsg.BClassID = int32(netproto.MessageBClassID_Game)
		pMsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_Compensate_DoubleBet)

		pData := new(netproto.JLBPK_Compensate_DoubleBet)
		// 所有玩家資訊
		for i := 0; i < pJLB.iSeatCount; i++ {
			pUserInfo := pJLB.userInfoPkg(i, int(pGameUser.SeatNo))
			pData.UserInfo = append(pData.UserInfo, pUserInfo)
		}

		pMsg.MsgData = pData
		pJLB.SendMessageToUser(pGameUser, pMsg)
	case JLB_State_StartBetting: // 開始下注
		fallthrough
	case DHC_State_Settlement:
		fallthrough
	case JLB_State_BetDoubleSettlement: // 加注結算
		if pUser.i64BetBase <= 0 {
			pUser.i64BetBase += int64(pJLB.ary_i32Chip[0])
			pUser.i64TotalBet += pUser.i64BetBase
			pUser.i64Money -= pUser.i64BetBase
			pUser.i64TotalWinLose -= pUser.i64BetBase
			pJLB.sendBetResult(pUser, JLB_BetType_Base, pUser.i64BetBase, JLB_BetResult_Auto)
		}
	}
}

// 玩家站立
func (pJLB *JlbpkLogic) UserFree(pGameUser *gamelogic.GameUser) {
	if pGameUser == nil {
		return
	}
	iSeat := int(pGameUser.SeatNo)
	if iSeat < 0 || iSeat >= pJLB.iSeatCount {
		return
	}

	if pJLB.iGameState >= JLB_State_StartBetting && pJLB.iGameState <= JLB_State_BetDoubleSettlement {
		return
	}

	if g_iHDCMode == HDCModeEnable && pJLB.iGameState == DHC_State_Settlement {
		return
	}

	pJLB.ary_pUserData[iSeat].Init()
	pJLB.sendAllUserInfo()
	defer func() {
		// check all user leave, close game ...
		var hasuser bool
		for _, user := range pJLB.ary_pUserData {
			if user.User != nil {
				hasuser = true
				break
			}
		}

		if !hasuser {
			pJLB.iGameState = JLB_State_WaitingPlayer
		}
	}()
	pJLB.GameLogicBase.UserFree(pGameUser)
}

// 玩家離線
func (pJLB *JlbpkLogic) UserLeave(pGameUser *gamelogic.GameUser) bool {
	if pGameUser == nil {
		return false
	}

	mlog.Debug("Table[%d], UserLeave, User[%d], Seat[%d]", pJLB.TableNo(), pGameUser.UserID, pGameUser.SeatNo)

	iSeat := int(pGameUser.SeatNo)
	if iSeat < 0 || iSeat >= pJLB.iSeatCount {
		return false
	}

	pJLB.ary_pUserData[iSeat].bOffline = true

	for iSeat := 0; iSeat < pJLB.iSeatCount; iSeat++ {
		pUser := pJLB.GameLogicBase.GetUserBySeatNo(int32(iSeat))
		if pUser != nil {
			mlog.Debug("Seat[%d], pUserData.bIsOffline[%t], pUser[%t]", iSeat, pJLB.ary_pUserData[iSeat].bOffline, pUser.IsOffLine())
		} else {
			mlog.Debug("Seat[%d], pUserData.bIsOffline[%t], pUser nil", iSeat, pJLB.ary_pUserData[iSeat].bOffline)
		}
	}

	if pJLB.iGameState >= JLB_State_StartBetting && pJLB.iGameState <= JLB_State_BetDoubleSettlement && pJLB.ary_pUserData[iSeat].iUserState != JLB_UserState_Watch {
		return false
	}

	if g_iHDCMode == HDCModeEnable && pJLB.iGameState == DHC_State_Settlement && pJLB.ary_pUserData[iSeat].iUserState != JLB_UserState_Watch {
		return false
	}

	pJLB.ary_pUserData[iSeat].Init()
	pJLB.sendAllUserInfo()

	defer func() {
		// check all user leave, close game ...
		var hasuser bool
		for _, user := range pJLB.ary_pUserData {
			if user.User != nil {
				hasuser = true
				break
			}
		}

		if !hasuser {
			pJLB.initData()
		}
	}()

	return pJLB.GameLogicBase.UserLeave(pGameUser)
}

// 玩家資訊打包資料，iSeat：此座位的玩家資訊，iSendTo：要送給哪位玩家
func (pJLB *JlbpkLogic) userInfoPkg(iSeat, iSendTo int) *netproto.JLBPK_UserInfo {
	if (iSeat < 0 || iSeat >= pJLB.iSeatCount) || (iSendTo < 0 || iSendTo >= pJLB.iSeatCount) {
		return nil
	}

	pUser := pJLB.ary_pUserData[iSeat]

	pUserInfo := new(netproto.JLBPK_UserInfo)

	pUserInfo.UserID = proto.Int32(pUser.i32UserID)                          // 玩家ID，若是空位時，玩家ID = -1
	pUserInfo.SeatNo = proto.Int32(int32(iSeat + JLBPK_ClientStartUpSeatNo)) // 玩家座位，client 座位號從 1 開始
	pUserInfo.State = proto.Int32(int32(pUser.iUserState))                   // 玩家狀態，0:空位，1:正常，2:觀戰，3:棄牌
	pUserInfo.Money = proto.Int64(pUser.i64Money)                            // 玩家金錢
	pUserInfo.Bet_Base = proto.Int64(pUser.i64BetBase)                       // 玩家底注
	pUserInfo.Bet_Side = proto.Int64(pUser.i64BetSide)                       // 玩家邊注
	pUserInfo.Bet_Double = proto.Int64(pUser.i64BetDouble)                   // 玩家加注
	pUserInfo.GetJackpot = proto.Bool(pUser.bGetJackpot)                     // 玩家是否中 Jackpot
	pUserInfo.JPMoney = proto.Int64(pUser.i64JPMoney)                        // 玩家所中 Jackpot 金額
	pUserInfo.WinLoseMoney = proto.Int64(pUser.i64WinLoseMoney)              // 玩家輸贏金錢，若值為負數，則為玩家輸掉的金額
	pUserInfo.ZhuangBet_Side = proto.Int64(pUser.i64ZhuangBetSide)           // 玩家莊家邊注
	pUserInfo.ZhuangGetJackpot = proto.Bool(pUser.bGetZhuangJackpot)         // 玩家是否中 莊家Jackpot
	pUserInfo.ZhuangJPMoney = proto.Int64(pUser.i64ZhuangJPMoney)            // 玩家所中 莊家Jackpot 金額

	if pUser.User != nil {
		pUserInfo.FaceID = proto.Int32(pUser.User.FaceID % 2) // 玩家頭像ID
	} else {
		pUserInfo.FaceID = proto.Int32(0) // 玩家頭像ID
	}

	pUserInfo.BetBaseWinLoseMoney = proto.Int64(pUser.i64BetBaseWinLoseMoney)     // 玩家底注輸贏金錢，若值為負數，則為玩家輸掉的金額
	pUserInfo.BetDoubleWinLoseMoney = proto.Int64(pUser.i64BetDoubleWinLoseMoney) // 玩家加注輸贏金錢，若值為負數，則為玩家輸掉的金額

	// 玩家手牌
	pHandCard := new(netproto.JLBPK_UserCards)
	for iIdx := 0; iIdx < pJLB.ary_pHandCards[iSeat].CardCount(); iIdx++ {
		pCard := new(netproto.PokerCard)
		if pJLB.iGameState >= JLB_State_BetSideSettlement {
			// 結算階段
			if pUser.iUserState == JLB_UserState_Normal ||
				pUser.iUserState == JLB_UserState_Fold {
				pSCard := pJLB.ary_pHandCards[iSeat].GetSingleCard(iIdx, false)
				pCard.Color = proto.Int32(pSCard.I32Color)
				pCard.CardNumber = proto.Int32(pSCard.I32Number)
			} else {
				pCard.Color = proto.Int32(pokerset.POKER_CardColor_Other)
				pCard.CardNumber = proto.Int32(pokerset.POKER_CardNo_Hide)
			}
		} else {
			if iSeat != iSendTo {
				pCard.Color = proto.Int32(pokerset.POKER_CardColor_Other)
				pCard.CardNumber = proto.Int32(pokerset.POKER_CardNo_Hide)
			} else {
				pSCard := pJLB.ary_pHandCards[iSeat].GetSingleCard(iIdx, false)
				pCard.Color = proto.Int32(pSCard.I32Color)
				pCard.CardNumber = proto.Int32(pSCard.I32Number)
			}
		}
		pHandCard.Cards = append(pHandCard.Cards, pCard)
	}
	pHandCard.Pattern = proto.Int32(pJLB.ary_pHandCards[iSeat].Pattern(false))
	pUserInfo.UserCards = pHandCard
	// 玩家總輸贏金錢
	if pUser.i64TotalWinLose > 0 {
		pUserInfo.TotalWinLose = proto.Int64(pUser.i64TotalWinLose + pUser.i64TotalBet)
	} else {
		if !pJLB.bDealerIsGood {
			if pUser.iUserState == JLB_UserState_Fold {
				pUserInfo.TotalWinLose = proto.Int64(pUser.i64TotalWinLose)
			} else {
				pUserInfo.TotalWinLose = proto.Int64(pUser.i64TotalWinLose + pUser.i64TotalBet)
			}
		} else {
			pUserInfo.TotalWinLose = proto.Int64(pUser.i64TotalWinLose)
		}
	}
	pUserInfo.Winner = proto.Int32(int32(pUser.iWinState))  // 贏家
	pUserInfo.MaxBetBase = proto.Int64(pUser.i64MaxBetBase) // 可以下的最大底注值

	return pUserInfo
}

// 設定遊戲狀態
func (pJLB *JlbpkLogic) setGameState(iGameState int, i64Timeout int64) {
	if (iGameState < JLB_State_WaitingPlayer || iGameState > JLB_State_GameOver) && iGameState != DHC_State_Settlement {
		return
	}

	i64Time := int64(0)
	if i64Timeout < 0 {
		i64Time = 0
	} else {
		i64Time = i64Timeout
	}

	pJLB.iGameState = iGameState
	pJLB.i64Timeout = i64Time
	pJLB.i64LastMessageTime = pJLB.i64TimeDuration

	mlog.Debug("SetGameState[%s], timeout[%d], duration[%d], lastmsg[%d]", map_strGameState_debug[pJLB.iGameState],
		pJLB.i64Timeout, pJLB.i64TimeDuration, pJLB.i64LastMessageTime)
}

// 依 UserID 取得 UserData
func (pJLB *JlbpkLogic) GetUserDataByUserID(i32UserID int32) *JlbpkUser {
	iSeatNo := -1

	for i := 0; i < pJLB.iSeatCount; i++ {
		if i32UserID == pJLB.ary_pUserData[i].i32UserID {
			iSeatNo = i
			break
		}
	}

	if iSeatNo >= 0 && iSeatNo < pJLB.iSeatCount {
		return pJLB.ary_pUserData[iSeatNo]
	} else {
		return nil
	}
}
