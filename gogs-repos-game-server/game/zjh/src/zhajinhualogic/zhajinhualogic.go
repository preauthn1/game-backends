package zhajinhualogic

import (
	"config"
	"event"
	"gamelogic"
	"math/rand"
	"mlog"
	"msg"
	"netproto"
	"poker"
	"strconv"
)

import (
	"fmt"
	"math"
	"util"

	"example.invalid/golang/protobuf/proto"
)

const opttimerstr = "zjh_opt_timeout"

const (
	actionName_FaPai        = "fapai" //发牌动画
	actionName_Bet          = "bet"   //下注跟注加注动画
	actionName_Allin        = "allin" //全押动画
	actionName_AllinCompare = "allincompare"
	actionName_Compare      = "compare" //比牌动画
	actionName_End          = "end"     //结束动画
)

const (
	BAOZI       = 1  //豹子
	SHUNJIN     = 2  //順金
	JINHUA      = 3  //金花
	SHUNZI      = 4  //順子
	BIGDUIZI    = 5  //對子(9-A)
	SMALLDUIZI  = 6  //對子(2-8)
	DANPAI      = 7  //單張
	BIGJINHUA   = 11 //A/K金花
	SMALLJINHUA = 12 //非A/K金花
	DUIZI       = 13 //對子
)

// 監控用戶用參數
// 下局輸贏
const (
	vipNextWinLose_Lose = 0 // 輸
	vipNextWinLose_Win  = 1 // 贏
)

// 開始/暫停
const (
	VIP_Status_Suspend = 0 // 暫停
	VIP_Status_Start   = 1 // 開始
)

//是否完成
const (
	VIP_Finish_No  = 0 // 否
	VIP_Finish_Yes = 1 // 是
)

// 未指定
const (
	VIP_NotSpecified = 99
)

const (
	USER_SumChargeUpLimit = 50000 // 玩家所需達儲值額
)

const (
	GAME_AllowCompareTurn int32 = 3 // 可以開始比牌的輪數
)

type ZhaJinHuaLogic struct {
	*gamelogic.DynamicGameLogicBase
	opttimer             *event.TimerFunc
	poker                poker.Poker           //游戏扑克牌
	zhuangSeat           int32                 //庄家座位号
	initSeat             int32                 //遊戲開始的初始座位號, 預設是莊家
	userData             []*ZjhUserData        //用户数据
	actionName_curr      string                //当前播放的动画
	userCount            int32                 //用户人数统计
	currSeat             int32                 //当前操作的座位号
	optTime              int32                 //可操作时间
	betCurr              int32                 //当前的下注额
	betTotal             int64                 //当前下注总额
	bCompare             bool                  //是否能比牌
	bFirst               bool                  //是否是第一个操作的玩家
	bAllin               bool                  //是否能全押
	winmoney             int64                 //赢家赢钱
	bGameEnd             bool                  //是否游戏结束
	bSendGameResult      bool                  //是否发送了结算消息
	currturn             int32                 //当前进行了多少轮
	canOpt               bool                  //当前是否可操作
	isadd                bool                  //本轮是否有人加注
	disRobotExchangeCard bool                  //關閉機器人換牌
	beforeGameStartStep  []string              //前置處理的記錄
	userinthisgame       []*netproto.ZJH_Poker //此局玩家
	SuperToolService     *ZjhSuperToolService  //超端服務
	gameTimeOutTimer     *event.TimerFunc      //遊戲進行太久的控制器
	pRestCardSet         *poker.CardGroup      //發剩下的牌組
	//以下配置不会随游戏改变
	betBase           int32               //底注
	betAdd            []int32             //加注数组
	betLast           int32               //封顶注
	bFirstTurn        bool                //是否第一局
	maxplayer         int32               //最多玩家
	pokerType         int32               //1为随机牌，2为发牌器
	readyTime         int32               //准备时间
	retAction         int32               //结束动画超时
	fapaiAction       int32               //发牌动画超时
	betAction         int32               //下注跟注的动画
	compareAction     int32               //比牌动画
	maxturn           int32               //经过了多少轮以后用户掉线将不会弃牌而找人比牌
	checkturn         int32               //可以看牌的轮数
	dapairandom       []int32             //大牌隨機組數
	dapaiweight       *weightconfig       //权重配置文件
	dapaiNumber       int                 //大牌數量
	normalweight      *weightconfig       //权重配置文件
	cardType          []int               //大牌種類
	radomnickname     int32               //特殊名稱的開關
	reversepokerorder int32               //發牌順序 順時針 逆時針 設置 ，0為順時針，1為逆時針
	robotCharacter    map[int32]*RobotCha // 機器人個性
	yuanchiaInitRand  int64               //初始冤家几率, 千分比.
	yuanchiaNewRand   int64               //新动态冤家牌几率
}

//创建诈金花逻辑接口
func NewZhaJinHuaLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *ZhaJinHuaLogic {
	glc := new(ZhaJinHuaLogic)
	glc.DynamicGameLogicBase = gamelogic.NewDynamicGameLogicBase(t, tfManager, cfgs)
	glc.getzjhConfig(cfgs)
	glc.bFirstTurn = true
	glc.bGameEnd = true
	glc.SuperToolService = glc.GetTable().Room.SuperToolService.(*ZjhSuperToolService)
	glc.robotCharacter = make(map[int32]*RobotCha)
	glc.RegisterAfterGameStartFunc(func() {
		//因遊戲開始前無法添加操作記錄, 故將遊戲開始前緩存的操作記錄添加進去
		for _, log := range glc.beforeGameStartStep {
			glc.GameLog.AddGameStep(log)
		}
		//因遊戲開始前已經 setuserdata 、但是因為即時到帳在之後才更新, 只能在這裡再做一次金額的異動
		for _, ud := range glc.userData {
			if ud != nil {
				ud.userMoney = ud.User.Score
			}
		}
		//glc.GameTimeOutStopSig = make(chan bool, 1)
		glc.gameTimeOutTimer = glc.TimerFuncManager.NewNamedTimer(glc.GetServerConfig().GameTimeOut*60, func() {
			mlog.Error("遊戲超時異常 取消遊戲 tableno %d, GameTimeOut[%v]", glc.TableNo(), glc.GetServerConfig().GameTimeOut)
			glc.GameCancelFunc("游戏异常，请稍候再试")
		}, fmt.Sprintf("gametimeoutcd_%d", glc.TableNo()))
		//go glc.GameTimeOutKickUser()
		glc.betDi()
		glc.fapai() //发牌
		glc.broadCastCurrturn()
	})

	glc.RegisterBeforeGameStartFunc(func() {
		//因為機器人要進行換牌所以，牌型必需要在 beforeGameStart 定義好
		glc.clearGameData()
		glc.defineCardType()
	})

	glc.RegisterBeforeGameEndFunc(func() {
		//glc.GameTimeOutStopSig <- true
		glc.gameTimeOutTimer.Stop()
	})

	glc.RegisterAfterGameEndFunc(func() {
		for _, u := range t.GetTableUser() {
			if u != nil {
				glc.SuperToolService.GetZjhSuperToolHandler().DelUserPokerCard(u.UserID)
				glc.bGameEnd = true
				glc.UserLeave(u)
			}
		}
	})

	return glc
}

//获取当前配置
func (zjh *ZhaJinHuaLogic) getzjhConfig(cfgs *config.ConfigGroup) {
	var lastindex int = 0

	scfgs := cfgs.GetConfig("gameserver")
	if scfgs == nil {
		mlog.Error("獲取配置 [gameserver] 失敗")
	} else {
		zjh.betBase = int32(scfgs.GetConfigInt("gameserver", "basescore"))
		zjh.optTime = int32(scfgs.GetConfigInt("gameserver", "opttime"))
		zjh.retAction = int32(scfgs.GetConfigInt("gameserver", "retaction"))
		zjh.readyTime = int32(scfgs.GetConfigInt("gameserver", "readywaittime"))
		zjh.maxplayer = int32(scfgs.GetConfigInt("gameserver", "maxplayer"))
		zjh.fapaiAction = int32(scfgs.GetConfigInt("gameserver", "fapaiaction"))
		zjh.pokerType = int32(scfgs.GetConfigInt("gameserver", "pokertype"))
		zjh.betAction = int32(scfgs.GetConfigInt("gameserver", "betaction"))
		zjh.compareAction = int32(scfgs.GetConfigInt("gameserver", "compareaction"))
		zjh.maxturn = int32(scfgs.GetConfigInt("gameserver", "maxturn"))
		zjh.checkturn = int32(scfgs.GetConfigInt("gameserver", "checkturn"))
		zjh.radomnickname = int32(scfgs.GetConfigInt("gameserver", "radomnickname"))
		zjh.reversepokerorder = int32(scfgs.GetConfigInt("gameserver", "reversepokerorder"))
		betstr := scfgs.GetConfigValue("gameserver", "bet")
		lastindex = 0
		for index, char := range betstr {
			if char == ',' {
				substr := betstr[lastindex:index]
				lastindex = index + 1
				bet, _ := strconv.ParseInt(substr, 10, 32)
				zjh.betLast = int32(bet)
				zjh.betAdd = append(zjh.betAdd, zjh.betLast)
			}
		}
	}

	//dapai setting
	cgf := cfgs.GetConfig("logicconfig")
	if cgf == nil {
		mlog.Error("獲取配置 [logicconfig] 失敗")
	} else {
		zjh.yuanchiaInitRand = cgf.GetConfigInt64("logicsetting", "yuanchia")
		zjh.yuanchiaNewRand = zjh.yuanchiaInitRand
		dapaistr := cgf.GetConfigValue("logicsetting", "dapai")
		lastindex = 0
		for index, char := range dapaistr {
			if char == ',' {
				substr := dapaistr[lastindex:index]
				lastindex = index + 1
				dapai, _ := strconv.ParseInt(substr, 10, 32)
				zjh.dapairandom = append(zjh.dapairandom, int32(dapai))
			}
		}
		zjh.dapaiweight = newDapaiWeightConfig(cfgs)
		zjh.normalweight = newNormalWeightConfig(cfgs)
	}
}
func (zjh *ZhaJinHuaLogic) defineCardType() {
	if zjh.GetServerConfig().PokerType == gamelogic.PokerType_Random {
		provider := zjh.SuperToolService.GetZjhSuperToolHandler()
		zjh.poker = poker.NewRandomPoker(false, false, 1)
		zjh.poker.Shuffle()
		//zjh.pRestCardSet = zjh.SetNewZJHPoker(false, false, 1)	// 原發牌方式
		zjh.pRestCardSet = new(poker.CardGroup)

		var uids []int32
		var LoserUserID int32 = 0
		var WinnerUserID int32 = 0
		var assignCardType = make(map[int32]int)
		for _, user := range zjh.userData {
			if user != nil {
				uids = append(uids, user.User.UserID)
			}
		}

		for _, userid := range provider.Proiority { //按優先順序將監控用戶取出
			if ok, _ := util.InArray(userid, uids); ok { //只有存在於桌內的用戶
				example.invalid("設定牌型：VIPUserID: %d", userid)
				if stcVIPUser_d := provider.GetVipUser(userid); stcVIPUser_d != nil {
					example.invalid("Status = %d, Finish = %d, NextWinLose = %d, NextWinLoseNum = %d, FinishNextWinLoseNum = %d",
						stcVIPUser_d.GetStatus(), stcVIPUser_d.GetFinish(), stcVIPUser_d.GetNextWinLose(), stcVIPUser_d.GetNextWinLoseNum(), stcVIPUser_d.GetFinishNextWinLoseNum())
				}

				if stcVIPUser := provider.GetVipUser(userid); stcVIPUser != nil {
					// 設定要贏還是輸
					if stcVIPUser.GetStatus() == VIP_Status_Start && stcVIPUser.GetFinish() == VIP_Finish_No {

						if stcVIPUser.GetNextWinLoseNum() == VIP_NotSpecified {
							// 若 NextWinLoseNum 被設定為［未指定］則不做換牌處理
							// 並將 Finish 設定為［完成］
							WinnerUserID = 0
							LoserUserID = 0
							stcVIPUser.Finish = proto.Int64(VIP_Finish_Yes)
							zjh.SuperToolService.GetZjhSuperToolHandler().SaveVipUser(userid)
						} else if WinnerUserID == 0 && stcVIPUser.GetNextWinLoseNum() > 0 {
							WinnerUserID = userid
						} else if LoserUserID == 0 && stcVIPUser.GetNextWinLoseNum() < 0 {
							LoserUserID = userid
						}
					}

					// 設定指定牌型
					if stcVIPUser.GetNextType() != VIP_NotSpecified {
						assignCardType[userid] = int(stcVIPUser.GetNextType())
						example.invalid("assignCardType[%d] = %d", userid, assignCardType[userid])
					}
				}
			}
		}

		if WinnerUserID != 0 || LoserUserID != 0 { //如果需要指定輸贏就不指定牌型了
			zjh.disRobotExchangeCard = true
			assignCardType = make(map[int32]int)
		} else if len(assignCardType) > 0 { //如果指定牌型, 機器人不可以換牌
			zjh.disRobotExchangeCard = true
		}

		userCardType := make(map[int32]int)
		if zjh.SuperToolService.fapaiRule == 1 { //第一種發牌規則, 分配大牌打亂大牌順序分配給用戶
			userCardType = zjh.fapaiDapaiWeight()
		} else { //第二種牌型規則
			userCardType = zjh.fapaiNormalWeight()
		}
		//檢查用戶是否有被指定牌型
		for uid, ct := range userCardType {
			if cardtype, ok := assignCardType[uid]; ok {
				userCardType[uid] = cardtype
				if vipuser := provider.GetVipUser(uid); vipuser != nil {
					vipuser.Finish = proto.Int64(1)
					zjh.SuperToolService.GetZjhSuperToolHandler().SaveVipUser(uid)
					example.invalid("用戶 %d 原牌型 %d 執行指定牌型 %d", uid, ct, cardtype)
				}
				continue
			}
		}
		example.invalid("userCardType %v", userCardType)
		zjh.poker = poker.NewZJHBigFixedPoker(userCardType, zjh.poker.(*poker.RandomPoker))
		zjh.beforeGameStartStep = append(zjh.beforeGameStartStep, fmt.Sprintf("disRobotExchangeCard %v", zjh.disRobotExchangeCard))

		if WinnerUserID != 0 {
			zjh.VipExchangeCard(WinnerUserID, 1)
			example.invalid("用戶 %d 執行指定贏", WinnerUserID)
		} else if LoserUserID != 0 {
			zjh.VipExchangeCard(LoserUserID, 2)
			example.invalid("用戶 %d 執行指定輸", LoserUserID)
		}

	} else { //发牌器发牌
		zjh.poker = poker.NewFilePoker(zjh.Configs, zjh.AllTableUser())
		zjh.poker = poker.NewFixedFilePoker(zjh.poker.(*poker.FilePoker))
	}
}

//清理用户数据
func (zjh *ZhaJinHuaLogic) clearGameData() {
	zjh.zhuangSeat = -1
	zjh.initSeat = -1
	zjh.betTotal = 0
	zjh.winmoney = 0
	zjh.canOpt = false
	zjh.bCompare = false
	zjh.bFirst = true
	zjh.opttimer = nil
	zjh.bGameEnd = false
	zjh.bSendGameResult = false
	zjh.bAllin = false
	zjh.currturn = 1
	zjh.isadd = false
	zjh.cardType = nil
	zjh.userinthisgame = nil
	zjh.beforeGameStartStep = make([]string, 0)
	zjh.setUserData()
	//優化找莊方法
	i := util.RndInt(0, len(zjh.userData)) //隨機找莊
	example.invalid("初始用戶列表 %v 莊隨機 %d", zjh.userData, i)
	zjh.zhuangSeat = zjh.userData[i].User.SeatNo
	//
	//
	//
	//if zjh.zhuangSeat == -1 || zjh.getUserBySeatNo(zjh.zhuangSeat) == nil {
	//	zjh.zhuangSeat = rand.Int31n(zjh.maxplayer)
	//	for i := 0; i < 10; i++ {
	//		if zjh.getUserBySeatNo(zjh.zhuangSeat) != nil {
	//			break
	//		}
	//
	//		zjh.zhuangSeat = rand.Int31n(zjh.maxplayer)
	//	}
	//
	//	//10次都没找到庄家就用第一家作为庄家
	//	if zjh.getUserBySeatNo(zjh.zhuangSeat) == nil {
	//		zjh.zhuangSeat = zjh.userData[0].User.SeatNo
	//	}
	//}
	zjh.initSeat = zjh.zhuangSeat

	zjh.betCurr = zjh.betBase
	zjh.currSeat = zjh.zhuangSeat
}

//开始下底
func (zjh *ZhaJinHuaLogic) betDi() {
	for _, urd := range zjh.userData {
		if urd == nil {
			continue
		}

		zjh.setUserMoney(urd, false)
		zjh.GameLog.AddGameStep("用户%d投入底注%d", urd.User.UserID, zjh.betCurr)
	}

	zjh.sendbetBaseMsg()
}

//发送下底消息
func (zjh *ZhaJinHuaLogic) sendbetBaseMsg() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_BetBaseId)

	pdata := new(netproto.ZJH_BetBase)
	pdata.Betbase = proto.Int32(zjh.betBase)
	pdata.Betcount = proto.Int32(zjh.userCount)
	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)
}

//开始发牌
func (zjh *ZhaJinHuaLogic) fapai() {
	provider := zjh.SuperToolService.GetZjhSuperToolHandler()
	//確定大牌份數
	mlog.Debug("开始发牌")
	//给所有用户发牌
	// 依自然機率發牌
	iFlagID, err := strconv.Atoi(zjh.TableManager.Room.Flag)
	if err != nil {
		mlog.Error("房間編號取得錯誤: %v", err)
	}
	iUserCount := 0
	for _, pUser := range zjh.userData {
		if pUser == nil {
			continue
		}

		iUserCount++
	}

	var ary_pUserCards []*poker.CardGroup
	pPoker := poker.NewRandomPoker(false, false, 1)
	pPoker.Shuffle()
	for _, pCard := range pPoker.Cards {
		if pCard != nil {
			zjh.pRestCardSet.AddCard(pCard)
		}
	}
	ary_pUserCards, zjh.pRestCardSet = FapaiByOddsTable(int32(iFlagID), zjh.TableNo(), iUserCount, zjh.pRestCardSet)
	//TODO 冤家牌, 当桌只有一个含对子以上牌时, 再过一次概率牌给散牌其中一人
	ary_pUserCards = zjh.YuanChiaFapai(ary_pUserCards)

	iIdx := 0
	for _, ur := range zjh.userData {
		if ur == nil {
			continue
		}

		/*
			// 原發牌方式
			cards := zjh.poker.Deal(ur.User.UserID, 3)
			if cards == nil {
				mlog.Error("User[%d], 未取得發出的牌，牌組資料為空", ur.User.UserID)
				continue
			}

			zjh.setUserCards(ur, cards)
			cardtype := GetType(cards)
			//mlog.Debug("===============userid %d cardtype %d cards %v",ur.User.UserID, cardtype, cards)
			zjh.SuperToolService.GetZjhSuperToolHandler().SaveCardType(int(cardtype))
			mlog.Debug("cards=%v", cards)
			provider.AddUserPokerCard(ur.User.UserID, cards.String(), typeStringMap[int(cardtype)])
			zjh.GameLog.Fapai(ur.User.UserID, cards.String())
		*/
		zjh.setUserCards(ur, ary_pUserCards[iIdx])
		i32CardType := GetType(ary_pUserCards[iIdx])
		zjh.SuperToolService.GetZjhSuperToolHandler().SaveCardType(int(i32CardType))
		mlog.Debug("cards=%v", ary_pUserCards[iIdx])
		provider.AddUserPokerCard(ur.User.UserID, ary_pUserCards[iIdx].String(), typeStringMap[int(i32CardType)])
		zjh.GameLog.Fapai(ur.User.UserID, ary_pUserCards[iIdx].String())
		zjh.userinthisgame = append(zjh.userinthisgame, zjh.getPokerTypeMsg(ur))
		//zjh.pRestCardSet.DeleteGroup(ur.userCards)
		iIdx++
	}
	zjh.PrintUserCardsInfo("fapai()")

	//通知用户发牌
	zjh.BoardcastMessage(zjh.getFapaiMsg())
	//注册发牌动画
	zjh.actionName_curr = zjh.RegisterAnimation(actionName_FaPai, zjh.fapaiAction, zjh.waitOpt)
}

//给牌排序
func (zjh *ZhaJinHuaLogic) sortCard(cards *poker.CardGroup) {
	if cards == nil {
		return
	}

	arrlen := len(cards.Cards)
	for i := 0; i < arrlen; i++ {
		for j := 0; j+1 < arrlen-i; j++ {
			card1 := cards.Cards[j].CardNumber*10 + cards.Cards[j].Color
			card2 := cards.Cards[j+1].CardNumber*10 + cards.Cards[j+1].Color
			if card1 > card2 {
				cards.Swap(j, j+1)
			}
		}
	}
}

//将用户的牌放到用户数据里面去。
func (zjh *ZhaJinHuaLogic) setUserCards(ud *ZjhUserData, cards *poker.CardGroup) {
	if ud == nil {
		mlog.Debug("no user data")
		return
	}

	if cards == nil {
		mlog.Debug("no cards data")
		return
	}

	zjh.sortCard(cards)
	for i := 0; i < len(cards.Cards); i++ {
		mlog.Debug("排序后的第%v牌:%v", i+1, cards.Cards[i])
	}

	ud.userCards = poker.NewCardGroup(cards.Cards)
}

//用户离开 replace dynamicleavegamelogicbase.UserLeave
func (glc *ZhaJinHuaLogic) UserLeave(gameuser *gamelogic.GameUser) bool {
	if gameuser == nil {
		mlog.Debug("no user data")
		return false
	}

	mlog.Debug("zjh user leave %v", gameuser)
	urd := glc.getUserBySeatNo(gameuser.SeatNo)
	if !glc.bGameEnd && glc.userCount > 1 {
		if urd != nil && (!urd.canEarlyChange() || !urd.isClear) {
			return false
		}
	}
	glc.SuperToolService.GetZjhSuperToolHandler().DelUserPokerCard(gameuser.UserID)
	return glc.GameLogicBase.UserLeave(gameuser)
}

//用户消息处理逻辑
func (zjh *ZhaJinHuaLogic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	if user == nil || gmsg == nil {
		return
	}

	if !zjh.CheckGameUserMessage(user) {
		zjh.KickUserFunc(user.UserID, "非法操作")
		return
	}

	//不是给游戏逻辑服务器的消息
	if gmsg.BClassID != int32(netproto.MessageBClassID_Game) {
		return
	}

	if gmsg.SClassID == int32(netproto.GameMessageClassID_UserActionCommitID) {
		zjh.CheckUserAnimationCommited(user, gmsg, zjh.actionName_curr, nil)
		return
	}

	//没有找到相关玩家数据
	urd := zjh.getUserBySeatNo(user.SeatNo)
	if urd == nil {
		mlog.Debug("用户未找到%v", user)
		return
	}

	//用戶提前換桌
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_EarlyChangeTable) {
		zjh.earlyChangeTable(urd, "change")
		return
	}

	//用戶提前離桌
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_EarlyLeaveTable) {
		zjh.earlyChangeTable(urd, "leave")
		return
	}

	if zjh.userCount <= 1 {
		mlog.Debug("游戏处于结算状态")
		return
	}

	//用户不可操作
	if !urd.canOpt() {
		mlog.Debug("用户已经放弃或比牌已输%v", urd)
		return
	}

	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_FollowEndID) {
		zjh.GameLog.AddGameStep("在第%d轮用户%d选择跟到底", zjh.currturn, user.UserID)
		zjh.setUserFollowEnd(urd)
		return
	}
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_AllinID) {
		zjh.GameLog.AddGameStep("在第%d轮用户%d选择全押", zjh.currturn, user.UserID)
		zjh.setUserAllin(urd)
		return
	}

	//用户弃牌
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_DropID) {
		mlog.Debug("收到弃牌消息")
		zjh.userDrop(user)
		return
	}

	//用戶看牌
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_CheckCardsID) {
		if urd.isCheck {
			mlog.Warn("已经看过牌了")
			return
		}

		if urd.User.Score-urd.userMoney <= 0 {
			if zjh.currturn < zjh.checkturn {
				mlog.Error("目前階段還不能看牌")
				return
			}
		}

		zjh.GameLog.AddGameStep("在第%d轮用户%d看牌", zjh.currturn, user.UserID)
		urd.isCheck = true
		zjh.SendMessageToUser(user, zjh.getPokerType(urd))
		zjh.SendMessageToUser(user, zjh.getPokerMsg(urd))
		zjh.broadCastCheckCard(user.SeatNo)
		zjh.sendOptMsg(false)
		return
	}

	if user.SeatNo != zjh.currSeat {
		mlog.Error("用户座位号非当前操作用户座位号%v", user)
		return
	}

	if urd.userMoney <= 0 {
		mlog.Debug("用户的钱都为负数了%v", user)
		return
	}

	//用户加注
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_BetAddID) {
		if ok, _ := zjh.userBetAdd(urd, gmsg.MsgData.([]byte)); !ok {
			mlog.Debug("加注不对所以弃牌")
			zjh.userDrop(user)
		} else {
			zjh.bFirst = false
			zjh.waitOpt()
		}

		return
	}

	//略过用户动画
	if gmsg.SClassID != int32(netproto.GameMessageClassID_UserActionCommitID) {
		zjh.bFirst = false
	}

	//用户比牌
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_CompareID) {
		if urd.isAllin {
			mlog.Error("用戶 %d , 目前為全押狀態，無法比牌", user.UserID)
			return
		}
		if zjh.currturn < GAME_AllowCompareTurn {
			mlog.Error("第三回合才能比牌%v", user)
			return
		}

		//操作扣钱
		zjh.compareCardsByplayer(urd, gmsg.MsgData.([]byte))
		return
	}

	//用户跟注
	if gmsg.SClassID == int32(netproto.ZJH_GameMessageClassID_FollowID) {
		zjh.userFollow(urd)
		return
	}
}

//用户提前換桌
func (zjh *ZhaJinHuaLogic) earlyChangeTable(urd *ZjhUserData, early string) {
	if urd == nil {
		mlog.Debug("no user data")
		return
	}

	if !urd.canEarlyChange() && !urd.isClear {
		return
	}

	if !urd.isClear {
		betmoney := urd.userMoney - urd.User.Score //下注的金額
		var flowAmount int64
		if betmoney > 0 {
			flowAmount = betmoney
		} else {
			flowAmount = -betmoney
		}
		win, tax := zjh.GetUserWinAndTax(betmoney)

		urd.win = win
		mlog.Debug("玩家%v 提前結算下注的金額 userMoney %d, 進房分數 %d, win %d, tax %d", urd.User.UserID, urd.userMoney, urd.User.Score, win, tax)
		//mlog.Debug("玩家%v 提前結算下注的金額 %d, %d_%d", urd.User.UserID, win, urd.userMoney, urd.User.Score)
		zjh.UpdateUserScoreFunc(urd.User.UserID, win, tax, 0)
		urd.isClear = true
		urd.User.FlowAmount += flowAmount
		zjh.GameLog.AddGameStep("在第%d轮用户%d提前換桌，提前寫分 %d, 稅 %d, 流水 %d", zjh.currturn, urd.User.UserID, win, tax, flowAmount)
	} else {
		zjh.GameLog.AddGameStep("在第%d轮用户%d提前換桌", zjh.currturn, urd.User.UserID)
	}
	//提前離桌
	zjh.UserEarlyChangeTable(urd.User, early)
}

//用户跟注
func (zjh *ZhaJinHuaLogic) userFollow(urd *ZjhUserData) {
	if urd == nil {
		mlog.Debug("no user data")
		return
	}

	if !zjh.canOpt {
		example.invalid("现在用户还不能操作")
		return
	}

	//if urd.isAllin {
	//	mlog.Debug("Allin後不能跟注")
	//	return
	//}

	if zjh.opttimer != nil {
		zjh.opttimer.Stop()
		zjh.opttimer = nil
	}
	odd := zjh.getOdds(urd, false)
	zjh.GameLog.AddGameStep("在第%d轮用户%d跟注，扣钱%d", zjh.currturn, urd.User.UserID, zjh.betCurr*odd)
	zjh.setUserMoney(urd, false)
	zjh.sendFollowMsg(urd)
	zjh.canOpt = false
	zjh.actionName_curr = zjh.RegisterAnimation(actionName_Bet, zjh.betAction, zjh.waitOpt)
	//zjh.waitOpt()
}

//用户全押
func (zjh *ZhaJinHuaLogic) userAllinAction(urd *ZjhUserData) {
	if urd == nil {
		mlog.Debug("no user data")
		return
	}

	if zjh.opttimer != nil {
		zjh.opttimer.Stop()
		zjh.opttimer = nil
	}

	//選擇身上钱較少玩家的金額進行全押()

	betCurr := urd.userMoney
	for _, u := range zjh.userData {
		if u == nil {
			continue
		}

		if u.User.UserID != urd.User.UserID && u.canOpt() {
			if u.userMoney < betCurr {
				betCurr = u.userMoney
			}
			u.isAllin = true
		}
	}
	odd := zjh.getOdds(urd, false)
	zjh.betCurr = int32(betCurr) / 2
	zjh.GameLog.AddGameStep("在第%d轮用户%d全押，扣钱%d", zjh.currturn, urd.User.UserID, zjh.betCurr*odd)
	zjh.setUserMoney(urd, false)
	zjh.sendAllinMsg(urd)
	zjh.canOpt = false
	zjh.actionName_curr = zjh.RegisterAnimation(actionName_Allin, zjh.betAction, zjh.waitOpt)
}

//设置用户跟到底
func (zjh *ZhaJinHuaLogic) setUserFollowEnd(user *ZjhUserData) {
	if user == nil {
		mlog.Debug("no user data")
		return
	}

	user.setFollowEnd()
	if zjh.currSeat == user.User.SeatNo {
		zjh.userFollowEnd()
	}
}

//设置用户全押
func (zjh *ZhaJinHuaLogic) setUserAllin(user *ZjhUserData) {
	if user == nil {
		mlog.Debug("no user data")
		return
	}

	//確認用戶是否可以全押
	if !zjh.bAllin {
		return
	}
	if !user.canOpt() {
		return
	}
	if !zjh.canOpt {
		mlog.Debug("现在用户还不能操作")
		return
	}
	user.setAllin()
	if zjh.currSeat == user.User.SeatNo {
		zjh.userAllinAction(user)
	}
}

//获取扑克消息
func (zjh *ZhaJinHuaLogic) getPokerMsg(ud *ZjhUserData) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_CardsInfo)

	if ud != nil {
		pmsg.MsgData = zjh.getCardsNetMsg(ud)
	}

	return pmsg
}

//获取发牌消息
func (zjh *ZhaJinHuaLogic) getFapaiMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_FaPaiId)
	pdata := new(netproto.ZJH_FaiPai)
	pdata.Seatno = proto.Int32(zjh.zhuangSeat)
	pdata.Bfirst = proto.Bool(zjh.bFirstTurn)
	for i := 0; i < len(zjh.betAdd); i++ {
		pdata.BetArr = append(pdata.BetArr, zjh.betAdd[i])
	}
	pmsg.MsgData = pdata
	zjh.bFirstTurn = false
	return pmsg
}

//设置用户数据
func (zjh *ZhaJinHuaLogic) setUserData() {
	zjh.userData = make([]*ZjhUserData, 0)
	zjh.userCount = 0
	for _, ur := range zjh.AllTableUser() {
		if ur == nil {
			continue
		}

		userdata := NewZjhUserData(ur)
		zjh.userData = append(zjh.userData, userdata)
		zjh.userCount++
	}
}

// 获取下一个可操作的用户
func (zjh *ZhaJinHuaLogic) waitOpt() {
	if zjh.reversepokerorder == int32(0) {
		zjh.waitoptClockwise()
	}
	if zjh.reversepokerorder == int32(1) {
		zjh.waitoptReverse()
	}
}

//获取下一个可操作的用户 順時針
func (zjh *ZhaJinHuaLogic) waitoptClockwise() {
	//只剩一个人的时候结算
	if zjh.userCount <= 1 {
		zjh.sendGameRetMsg()
		return
	}

	//当前已经是庄家操作了,能比牌了。
	if zjh.currSeat == zjh.initSeat && !zjh.bFirst {
		zjh.currturn++
		if zjh.currturn >= GAME_AllowCompareTurn { //第三輪的時候可以比牌和Allin
			zjh.bCompare = true
			zjh.bAllin = true
		}
		zjh.isadd = false
		zjh.broadCastCurrturn()
	}

	//遊戲開始的初始座位號, 預設是莊家
	initUser := zjh.getUserBySeatNo(zjh.initSeat)
	if initUser == nil || !initUser.canOpt() {
		//新的初始座位號 -> 遞增取餘數為座位號
		for i := int32(0); i < zjh.maxplayer; i++ {
			seat := (zjh.initSeat + i) % zjh.maxplayer
			urd := zjh.getUserBySeatNo(seat)
			if urd != nil && urd.canOpt() {
				zjh.initSeat = seat
				break
			}
		}
	}

	nextSeat := zjh.userData[0].User.SeatNo
	usercount := len(zjh.userData)
	for i := 0; i < usercount; i++ {
		if zjh.userData[i].canOpt() {
			nextSeat = zjh.userData[i].User.SeatNo
			break
		}
	}

	if nextSeat == -1 {
		return
	}

	for _, urd := range zjh.userData {
		if urd == nil {
			continue
		}

		mlog.Debug("遍历座位号:%v,是否可操作%v", urd.User.SeatNo, urd.canOpt())
		if urd.User.SeatNo > zjh.currSeat && urd.canOpt() {
			nextSeat = urd.User.SeatNo
			break
		}
	}

	mlog.Debug("可操作的座位号为：%v", nextSeat)
	zjh.currSeat = nextSeat

	//當所有人都跟全押時, 輪到主動全押玩家就進入結算
	if zjh.getUserBySeatNo(nextSeat).User.UserID == zjh.getUserBySeatNo(nextSeat).AllInUserId {
		zjh.allInSettlement()
	} else {
		zjh.sendOptMsg(false)
		zjh.canOpt = true
		zjh.userFollowEnd()
	}
}

//获取下一个可操作的用户 逆時針
func (zjh *ZhaJinHuaLogic) waitoptReverse() {

	//此程式是把原本的發牌順序為座位01234修改改成04321

	// userdataReverse 為 zjh.userData 左右倒
	// userdataReverse := [10]*ZjhUserData{} // 因為GO語言規定陣列大小要先定好，只能使用以下方式
	userdataReverse := make([]*ZjhUserData, zjh.maxplayer+2) //  先保守一點，避免userdataReverse在操作時會oute of range所以設定為最多人數 zjh.maxplayer +2

	// 開始在 userdataReverse 裡塞資料
	operateTimes := (len(zjh.userData) - len(zjh.userData)%2) / 2 // operateTimes為需要對userdataReverse塞資料的操作次數
	for i := 0; i < operateTimes; i++ {
		userdataReverse[0+i] = zjh.userData[len(zjh.userData)-1-i] // 塞入資料到userdataReverse
		userdataReverse[len(zjh.userData)-1-i] = zjh.userData[0+i] // 塞入資料到userdataReverse
	}

	// 塞入中間玩家的資料
	hasMiddle := len(zjh.userData) % 2 // 但是假設玩家資料為5份，中間玩家的資料之前還沒塞入 userdataReverse，所以進行以下操作
	if hasMiddle == 1 {                // 如果有中間玩家的資料
		userdataReverse[((len(zjh.userData) - 1) / 2)] = zjh.userData[((len(zjh.userData) - 1) / 2)] // 塞入資料到 userdataReverse
	}

	//只剩一个人的时候结算
	if zjh.userCount <= 1 {
		zjh.sendGameRetMsg()
		return
	}

	//当前已经是庄家操作了,能比牌了。
	if zjh.currSeat == zjh.initSeat && !zjh.bFirst {
		zjh.currturn++
		if zjh.currturn >= GAME_AllowCompareTurn { //第三輪的時候可以比牌和Allin
			zjh.bCompare = true
			zjh.bAllin = true
		}
		zjh.isadd = false
		zjh.broadCastCurrturn()
	}

	//遊戲開始的初始座位號, 預設是莊家
	initUser := zjh.getUserBySeatNo(zjh.initSeat)
	if initUser == nil || !initUser.canOpt() {
		//新的初始座位號 -> 遞 減 取餘數為座位號，順序為 0 4 3 2 1
		seat := zjh.initSeat
		for i := int32(0); i < zjh.maxplayer; i++ {
			seat = (seat - 1)
			if seat == int32(-1) { // 這裡在處理 初始座位號 由 0 直接 跳到最大座位號
				seat = int32(len(zjh.userData) - 1) // 直接 跳到最大座位號
			} // 直接 跳到最大座位號
			urd := zjh.getUserBySeatNo(seat)
			if urd != nil && urd.canOpt() { // 到 zjh.userData (未顛倒的實實資料) 去查詢使用者可否再下注
				zjh.initSeat = seat // 如果這位玩家還可以下注，那這位玩家就設定為起始位置
				break
			}
		}
	}

	//第一次獲得nextSeat
	// 如果現在座位是0，第一次會取得最大可以 行動( 下注 棄牌 ) 的最大座位值，以座位 4 3 2 1 0 為例，第一次的nextSeat值為 4 或者是 3
	nextSeat := userdataReverse[0].User.SeatNo // 43210 起始為4
	usercount := len(zjh.userData)
	for i := 0; i < usercount; i++ {
		if userdataReverse[0+i].canOpt() {
			nextSeat = userdataReverse[0+i].User.SeatNo
			break
		}
	}

	if nextSeat == -1 { // 不能有座位小於 0
		return
	}

	//第二次獲得nextSeat
	// 如果現在座位是0，這個迴圈將沒有作用，因為沒有任何一個座位值小於0，除了 -1 ，但是在第一次就取得最大可以 行動( 下注 棄牌 ) 的最大座位值，剛好形成 0 的下一個座位為 最大座位值
	for _, urd := range userdataReverse[0:len(zjh.userData)] {
		if urd.User.SeatNo < zjh.currSeat && urd.canOpt() {
			nextSeat = urd.User.SeatNo
			break
		}
	}

	mlog.Debug("可操作的座位号为：%v", nextSeat)
	zjh.currSeat = nextSeat // 在這裡正式決定下一個座位號

	//當所有人都跟全押時, 輪到主動全押玩家就進入結算
	if zjh.getUserBySeatNo(nextSeat).User.UserID == zjh.getUserBySeatNo(nextSeat).AllInUserId {
		zjh.allInSettlement()
	} else {
		zjh.sendOptMsg(false)
		zjh.canOpt = true
		zjh.userFollowEnd()
	}
}

//當所有人都跟全押時, 輪到主動全押玩家就進入結算
func (zjh *ZhaJinHuaLogic) allInSettlement() {
	//comret := comPareCards(user.userCards, dstuser.userCards)
	comret := 0
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_AllInSettlement)
	pdata := new(netproto.ZJH_ComparePoker)

	for _, ud1 := range zjh.userData {
		if ud1 == nil {
			continue
		}

		ud1ok := !ud1.isDrop && !ud1.isFaild
		if !ud1ok {
			continue
		}

		pdata.OtherPoker = append(pdata.OtherPoker, zjh.getPokerTypeMsg(ud1))

		for _, ud2 := range zjh.userData {
			if ud2 == nil {
				continue
			}

			ud2ok := !ud2.isDrop && !ud2.isFaild && ud1.User.UserID != ud2.User.UserID
			if ud2ok {
				comret = comPareCards(ud1.userCards, ud2.userCards)
				example.invalid("比牌 ud1[%v] ud2[%v] ret[%v]", ud1.User.UserID, ud2.User.UserID, comret)

				if comret == 1 {
					ud2.isFaild = true
					//pdata.OtherPoker = append(pdata.OtherPoker, zjh.getPokerTypeMsg(ud2))
					zjh.GameLog.AddGameStep("所有全押比牌 userid[%v]card[%v] userid[%v]card[%v] ", ud1.User.UserID, ud1.userCards.Cards, ud2.User.UserID, ud2.userCards.Cards)
					example.invalid("所有全押比牌 userid[%v]card[%v] userid[%v]card[%v] ", ud1.User.UserID, ud1.userCards.Cards, ud2.User.UserID, ud2.userCards.Cards)
				}
				//儲值達500的玩家才可看牌
				if ud2.User.SumCharge >= USER_SumChargeUpLimit {
					pdata.OtherPoker = zjh.userinthisgame
				} else {
					//自己的扑克
					pdata.OtherPoker = append(pdata.OtherPoker, zjh.getPokerTypeMsg(ud2))
				}
			}
		}

	}

	for _, v := range pdata.GetOtherPoker() {
		example.invalid("全押win PokerType[%v] Userpokergroup[%v]  ", v.GetPokerType(), v.GetUserpokergroup())
	}

	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)

	zjh.userCount = 0
	zjh.canOpt = false
	zjh.actionName_curr = zjh.RegisterAnimation(actionName_AllinCompare, 1, zjh.sendGameRetMsg)
}

//处理用户跟到底
func (zjh *ZhaJinHuaLogic) userFollowEnd() {
	//第一个操作的人不能跟到底
	if zjh.bFirst {
		return
	}

	ud := zjh.getUserBySeatNo(zjh.currSeat)
	if ud == nil {
		return
	}

	if !ud.isFollowEnd {
		return
	}
	mlog.Debug("3 ud.isFollowEnd=%v, id = %v", ud.isFollowEnd, ud.User.UserID)
	//用户掉线了，那么不进行跟到底了
	if ud.User.IsOffLine() {
		ud.setFollowEnd()
		return
	}

	//看钱是否够跟到底
	betadd := (zjh.betAdd[len(zjh.betAdd)-1]) * 2   //注額最大值*看牌倍率
	usercount := (float64(zjh.userCount) - 0.5) * 2 //玩家數量*比牌倍率
	betlimit := int64(betadd) * int64(usercount)    //9/12 betlimit 移除扣底注
	if ud.userCanFollow(betlimit) {
		//够用户直接跟注
		zjh.userFollow(ud)
	} else {
		//钱不够找一个人比牌
		zjh.autoCompare()
	}
}

//发送操作消息
func (zjh *ZhaJinHuaLogic) sendOptMsg(bRecover bool) {
	ud := zjh.getUserBySeatNo(zjh.currSeat)

	if ud == nil {
		mlog.Error("玩家为空")
		return
	}

	addCount := len(zjh.betAdd)

	var arrBet = make([]int32, 0)
	for i := 0; i < addCount; i++ {
		if zjh.bFirst {
			arrBet = append(arrBet, zjh.betAdd[i])
			mlog.Debug("zjh.bFirst")
		} else if zjh.betAdd[i] > zjh.betCurr {
			arrBet = append(arrBet, zjh.betAdd[i])
		}
	}

	opttime := zjh.optTime
	//如果不是场景恢复
	if !bRecover {
		msgall := zjh.getCurrOptMsg()
		zjh.BoardcastMessage(msgall)
		//延迟一秒生效，防止网络卡顿造成弃牌的情况

		//移除操作超时Timer
		if zjh.opttimer != nil {
			zjh.opttimer.Stop()
			zjh.opttimer = nil
		}
		if zjh.currturn >= zjh.maxturn {
			zjh.opttimer = zjh.TimerFuncManager.NewNamedTimer(zjh.optTime+1, zjh.autoCompare, "")
		} else {
			zjh.opttimer = zjh.TimerFuncManager.NewNamedTimer(zjh.optTime+1, zjh.userTimeOutDrop, "")
		}

	} else {
		if zjh.opttimer != nil {
			opttime -= zjh.opttimer.ElapsedSeconds()
		}
	}
	bcheck := false
	if zjh.currturn >= zjh.checkturn {
		bcheck = true
	}
	betadd := (zjh.betAdd[len(zjh.betAdd)-1]) * 2   //注額最大值*看牌倍率
	usercount := (float64(zjh.userCount) - 0.5) * 2 //玩家數量*比牌倍率
	betlimit := int64(betadd) * int64(usercount)    //9/12 betlimit 移除扣底注
	msg := ud.GetOptMsg(arrBet, opttime, zjh.bCompare, zjh.bFirst, bcheck, betlimit, zjh.bAllin)
	zjh.SendMessageToUser(ud.User, msg)
}

//当前操作消息
func (zjh *ZhaJinHuaLogic) getCurrOptMsg() *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_CurrOptID)

	pdata := new(netproto.ZJH_WaitOptSeat)
	pdata.Seatno = proto.Int32(zjh.currSeat)
	pdata.OptTime = proto.Int32(zjh.optTime)
	pmsg.MsgData = pdata

	return pmsg
}

//获取玩家数据通过座位号
func (zjh *ZhaJinHuaLogic) getUserBySeatNo(seatno int32) *ZjhUserData {
	for _, urd := range zjh.userData {
		if urd == nil {
			continue
		}

		// 這裡很坑, TableNo 要一起檢查才行
		if zjh.TableNo() == urd.User.TableNo && urd.User.SeatNo == seatno {
			return urd
		}
	}

	return nil
}

//处理玩家的比牌消息
func (zjh *ZhaJinHuaLogic) compareCardsByplayer(user *ZjhUserData, data []byte) bool {
	if user == nil {
		mlog.Debug("無玩家資料")
		return false
	}

	if !zjh.bCompare {
		return false
	}
	ust := &netproto.ZJH_SeatNO{}

	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析用户%v用户座位号错误%v", user, err)
		return false
	}

	dstuser := zjh.getUserBySeatNo(*ust.Seatno)
	if dstuser == nil {
		mlog.Debug("解析用户%v用户座位号不存在", user)
		return false
	}
	if !dstuser.canOpt() {
		return false
	}

	if zjh.opttimer != nil {
		zjh.opttimer.Stop()
		zjh.opttimer = nil
	}

	odd := zjh.getOdds(user, true)

	zjh.GameLog.AddGameStep("在第%d轮用户%d和用户%d比牌，扣钱%d", zjh.currturn, user.User.UserID, dstuser.User.UserID, zjh.betCurr*odd)
	zjh.userCompareOpt(user, dstuser)
	return true
}

//处理玩家比牌消息
func (zjh *ZhaJinHuaLogic) userCompareOpt(user *ZjhUserData, dstuser *ZjhUserData) {
	if user == nil || dstuser == nil {
		mlog.Debug("無玩家資料")
		return
	}

	if !zjh.canOpt {
		mlog.Debug("现在用户还不能操作")
		return
	}

	//mlog.Debug("賭神：第 %d 輪，userCompareOpt(), user[%d], IsRobot[%t], dstuser[%d], IsRobot[%t]",
	//	zjh.currturn, user.User.UserID, user.User.IsRobot(), dstuser.User.UserID, dstuser.User.IsRobot())
	/*
		// 如果 dstuser 是機器人，則看機器人是否啟動賭神模式
		if !user.User.IsRobot() {
			if dstuser.User.IsRobot() {
				zjh.InitiateRobotToGod(dstuser.User.SeatNo, user.User.SeatNo)
			}
		} else {
			if !dstuser.User.IsRobot() {
				zjh.InitiateRobotToGod(user.User.SeatNo, dstuser.User.SeatNo)
			}
		}
	*/

	zjh.setUserMoney(user, true)
	zjh.sendUserCompareResult(user, dstuser)
	zjh.userCount--
	zjh.canOpt = false
	zjh.actionName_curr = zjh.RegisterAnimation(actionName_Compare, zjh.compareAction, zjh.waitOpt)
}

//发送比牌结果
func (zjh *ZhaJinHuaLogic) sendUserCompareResult(user *ZjhUserData, dstuser *ZjhUserData) {
	if user == nil || dstuser == nil {
		mlog.Debug("無玩家資料")
		return
	}

	user.setCompareUserSeatNo(dstuser.User.SeatNo)
	dstuser.setCompareUserSeatNo(user.User.SeatNo)
	comret := comPareCards(user.userCards, dstuser.userCards)

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_CompareRetID)

	pdata := new(netproto.ZJH_CompareCardsRet)

	if comret == 1 {
		mlog.Debug("玩家1赢")
		dstuser.isFaild = true
		pdata.Winseatno = proto.Int32(user.User.SeatNo)
		pdata.Faildseatno = proto.Int32(dstuser.User.SeatNo)
	} else {
		mlog.Debug("玩家2赢")
		user.isFaild = true
		pdata.Winseatno = proto.Int32(dstuser.User.SeatNo)
		pdata.Faildseatno = proto.Int32(user.User.SeatNo)
	}

	odd := zjh.getOdds(user, true)
	pdata.CompareMoney = proto.Int32(zjh.betCurr)
	pdata.Odd = proto.Int32(odd)
	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)
	//faildUser := zjh.getUserBySeatNo(pdata.GetFaildseatno())
	//zjh.earlyChangeTable(faildUser)
	if dstuser.User.IsOffLine() && dstuser.isFaild {
		zjh.earlyChangeTable(dstuser, "leave")
	}

	if user.User.IsOffLine() && user.isFaild {
		zjh.earlyChangeTable(user, "leave")
	}
}

//发送跟注消息
func (zjh *ZhaJinHuaLogic) sendFollowMsg(user *ZjhUserData) {
	if user == nil {
		mlog.Debug("no user data")
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_FollowBrodCastID)

	odd := zjh.getOdds(user, false)
	pdata := new(netproto.ZJH_Follow)
	pdata.Seatno = proto.Int32(user.User.SeatNo)
	pdata.Follow = proto.Int32(zjh.betCurr)
	pdata.Odd = proto.Int32(odd)
	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)
}

//发送全押消息
func (zjh *ZhaJinHuaLogic) sendAllinMsg(user *ZjhUserData) {
	if user == nil {
		mlog.Debug("no user data")
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_AllinRetID)

	odd := zjh.getOdds(user, false)
	pdata := new(netproto.ZJH_AllinBrodCast)
	pdata.Seatno = proto.Int32(user.User.SeatNo)
	pdata.Betadd = proto.Int32(zjh.betCurr)
	pdata.Odd = proto.Int32(odd)

	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)
	mlog.Debug("发送全押消息 userid[%v]", user.User.UserID)
}

//处理用户加注
func (zjh *ZhaJinHuaLogic) userBetAdd(user *ZjhUserData, data []byte) (bool, error) {
	if user == nil {
		mlog.Debug("no user data")
		return false, nil
	}

	ust := &netproto.ZJH_BetAdd{}

	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析用户%v用户座位号错误%v", user, err)
		return false, nil
	}
	*ust.Betadd = int32(math.Abs(float64(*ust.Betadd)))

	odd := zjh.getOdds(user, false)

	hasvalue := false
	for i := 0; i < len(zjh.betAdd); i++ {
		if zjh.betAdd[i]*odd == *ust.Betadd {
			hasvalue = true
			break
		}
	}

	if !hasvalue {
		mlog.Debug("没有找到这个值：%v", *ust.Betadd)
		return false, util.ErrIsNotBigThenZero
	}

	//加注不能比当前下注还小
	if zjh.betCurr*odd > *ust.Betadd {
		mlog.Debug("加注小于当前值：%v，%v", *ust.Betadd, zjh.betCurr*odd)
		return false, nil
	}

	if !zjh.canOpt {
		mlog.Debug("现在用户还不能操作")
		return false, nil
	}

	zjh.canOpt = false
	if *ust.Betadd > zjh.betCurr*odd {
		zjh.isadd = true
	}
	zjh.betCurr = *ust.Betadd / odd
	zjh.setUserMoney(user, false)

	zjh.GameLog.AddGameStep("在第%d轮用户%d加注%d，扣錢%d", zjh.currturn, user.User.UserID, zjh.betCurr, zjh.betCurr*odd)
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_BetAddBrodCastID)

	pdata := new(netproto.ZJH_BetAddBrodCast)
	pdata.Seatno = proto.Int32(user.User.SeatNo)
	pdata.Betadd = proto.Int32(zjh.betCurr)
	pdata.Odd = proto.Int32(odd)
	pdata.Isfirst = proto.Bool(zjh.bFirst)
	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)

	return true, nil
}

//发送游戏结果
func (zjh *ZhaJinHuaLogic) sendGameRetMsg() {
	//找出赢家和下一次的庄家
	//移除操作超时Timer
	if zjh.opttimer != nil {
		example.invalid("移除超时timer")
		zjh.opttimer.Stop()
		zjh.opttimer = nil
	}

	//分别结算
	for _, urd := range zjh.userData {
		if urd != nil {
			if urd.canOpt() {
				zjh.zhuangSeat = urd.User.SeatNo
				//urd.SetMoney(zjh.betTotal)
				break
			}
		}
	}

	for _, urd := range zjh.userData {
		if urd != nil {
			zjh.calWinMoney(urd)
		}
	}

	for _, urd := range zjh.userData {
		if urd != nil {
			if urd.isAllin {
				break
			}
			zjh.sendComparePoker(urd)
		}
	}

	uscoresRet := make(map[int32]int64)
	ucouponRet := make(map[int32]int64)
	for _, urd := range zjh.userData {
		if urd != nil {
			uscoresRet[urd.User.UserID] = urd.win
			ucouponRet[urd.User.UserID] = 0
		}
	}

	zjh.GameLog.GameEnd(uscoresRet, ucouponRet)

	zjh.broadcastGameRet()

	//zjh.actionName_curr = zjh.RegisterAnimation(actionName_End, zjh.retAction, zjh.sendReadyTime)
	zjh.TimerFuncManager.NewTimer(zjh.retAction, zjh.sendReadyTime)
}

//扑克信息
func (zjh *ZhaJinHuaLogic) getPokerTypeMsg(ud *ZjhUserData) *netproto.ZJH_Poker {
	zjhpoker := new(netproto.ZJH_Poker)

	if ud != nil {
		pokermsg := zjh.getCardsNetMsg(ud)

		zjhpoker.PokerType = proto.Int32(GetType(ud.userCards))
		zjhpoker.Userpokergroup = pokermsg
	}

	return zjhpoker
}

//广播结算消息
func (zjh *ZhaJinHuaLogic) broadcastGameRet() {
	zjh.BoardcastMessage(zjh.getGameRetMsg())
}

//获取游戏结果消息
func (zjh *ZhaJinHuaLogic) getGameRetMsg() *msg.Message {
	zjh.bSendGameResult = true
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_GameResultID)
	pdata := new(netproto.ZJH_GameRet)

	pdata.Money = proto.Int64(zjh.winmoney)
	pdata.WinSeatNO = proto.Int32(zjh.zhuangSeat)
	pmsg.MsgData = pdata
	mlog.Debug("WinSeatNO[%v] winmoney[%v]", zjh.zhuangSeat, zjh.winmoney)
	return pmsg
}

//結算金額
func (zjh *ZhaJinHuaLogic) calWinMoney(ud *ZjhUserData) {
	if ud == nil {
		mlog.Debug("no user data")
		return
	}

	//己提前結算過
	if ud.isClear {
		return
	}
	winmoney := ud.userMoney - ud.User.Score //下注的金額
	userbet := winmoney

	mlog.Debug("游戏开始的钱：%v，游戏结束的钱：%v", ud.User.Score, ud.userMoney)
	if ud.User.SeatNo == zjh.zhuangSeat { //最後的贏家贏得牌桌上的錢
		winmoney = zjh.betTotal
	}
	var flowAmount int64
	if winmoney > 0 { //贏家減去自已的下注, userbet基本上是負數
		flowAmountAfterTax, _ := zjh.GetUserWinAndTax(winmoney)
		flowAmountTotal := flowAmountAfterTax + userbet
		flowAmount += flowAmountTotal
	} else {
		flowAmount = -winmoney
	}
	ud.User.FlowAmount += flowAmount
	money, tax := zjh.GetUserWinAndTax(winmoney) //牌桌上的錢需要抽稅
	win := money
	if win > 0 {
		win += ud.userMoney - ud.User.Score
	}
	zjh.GameLog.AddGameStep("用戶%d 結算金額, win %d, 稅收 %d, 流水 %d", ud.User.UserID, win, tax, flowAmount)
	mlog.Debug("用戶%d 結算金額 userMoney %d, Score %d, win %d, tax %d", ud.User.UserID, ud.userMoney, ud.User.Score, winmoney, tax)
	//zjh.GameLog.AddGameStep("用戶%d 寫分, win %d, 稅收 %d", ud.User.UserID, win, tax)
	zjh.UpdateUserScoreFunc(ud.User.UserID, win, tax, 0)
	ud.isClear = true
	if ud.User.SeatNo == zjh.zhuangSeat {
		zjh.winmoney = money
	}

	ud.win = win

	// 若遊戲結果符合［下局輸贏］設定條件，則計完成次數一次並存檔
	stcVipProvider := zjh.SuperToolService.GetZjhSuperToolHandler()
	var iNextWinLoseCount int64 = 0

	if ok, _ := util.InArray(ud.User.UserID, stcVipProvider.Proiority); ok { // 檢查 User 是否為監控用戶
		if stcVIPUser := stcVipProvider.GetVipUser(ud.User.UserID); stcVIPUser != nil {
			if stcVIPUser.GetFinish() == VIP_Finish_No { // 監控尚未完成
				// 取輸贏局數的絕對值
				iNextWinLoseCount = stcVIPUser.GetNextWinLoseNum()
				if iNextWinLoseCount < 0 {
					iNextWinLoseCount = -iNextWinLoseCount
				}

				if stcVIPUser.GetNextWinLoseNum() < 0 {
					// 指定為輸
					if ud.win < 0 {
						stcVIPUser.FinishNextWinLoseNum = proto.Int64(stcVIPUser.GetFinishNextWinLoseNum() + 1)
						if stcVIPUser.GetFinishNextWinLoseNum() >= iNextWinLoseCount {
							stcVIPUser.Finish = proto.Int64(VIP_Finish_Yes)
						}
						zjh.SuperToolService.GetZjhSuperToolHandler().SaveVipUser(ud.User.UserID)
					}
				} else if stcVIPUser.GetNextWinLoseNum() > 0 {
					// 指定為贏
					if ud.win > 0 {
						stcVIPUser.FinishNextWinLoseNum = proto.Int64(stcVIPUser.GetFinishNextWinLoseNum() + 1)
						if stcVIPUser.GetFinishNextWinLoseNum() >= iNextWinLoseCount {
							stcVIPUser.Finish = proto.Int64(VIP_Finish_Yes)
						}
						zjh.SuperToolService.GetZjhSuperToolHandler().SaveVipUser(ud.User.UserID)
					}
				}
			}
		}
	}
}

//分别发送和玩家比牌的消息
func (zjh *ZhaJinHuaLogic) sendComparePoker(ud *ZjhUserData) {
	if ud == nil {
		mlog.Debug("no user data")
		return
	}

	////己提前結算過
	//if ud.isClear {
	//	return
	//}
	//winmoney := ud.userMoney - ud.User.Score //下注的金額
	////mlog.Debug("游戏开始的钱：%v，游戏结束的钱：%v", ud.User.Score, ud.userMoney)
	//if ud.User.SeatNo == zjh.zhuangSeat { //最後的贏家贏得牌桌上的錢
	//	winmoney = zjh.betTotal
	//}
	//
	//money, tax := zjh.GetUserWinAndTax(winmoney) //牌桌上的錢需要抽稅
	//win := money
	//if win > 0 {
	//	win += ud.userMoney - ud.User.Score
	//}
	//zjh.UpdateUserScoreFunc(ud.User.UserID, win, tax)
	//ud.isClear = true
	//if ud.User.SeatNo == zjh.zhuangSeat {
	//	zjh.winmoney = money
	//}
	//
	//ud.win = win

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_ComparePokerID)
	pdata := new(netproto.ZJH_ComparePoker)

	//儲值達500的玩家才可看牌
	if ud.User.SumCharge >= USER_SumChargeUpLimit {
		pdata.OtherPoker = zjh.userinthisgame
	} else {
		//自己的扑克
		//pdata.OtherPoker = append(pdata.OtherPoker, zjh.getPokerTypeMsg(ud))
		//和我比牌人的扑克
		for _, i := range ud.compareUserSeatNo {
			comud := zjh.getUserBySeatNo(i)
			if comud == nil {
				mlog.Debug("和我比牌的人, 己經離開桌子了")
				continue
			}
			pdata.OtherPoker = append(pdata.OtherPoker, zjh.getPokerTypeMsg(comud))
		}
		myself := zjh.getUserBySeatNo(ud.User.SeatNo)
		if myself != nil {
			pdata.OtherPoker = append(pdata.OtherPoker, zjh.getPokerTypeMsg(myself))
		}
	}

	pmsg.MsgData = pdata
	zjh.SendMessageToUser(ud.User, pmsg)
}

//自动找人比牌
func (zjh *ZhaJinHuaLogic) autoCompare() {
	ud := zjh.getUserBySeatNo(zjh.currSeat)
	dstuser := zjh.userData[0]
	if ud == nil || dstuser == nil {
		mlog.Debug("no user data: ud[%v], dstuser[%v]", ud, dstuser)
		return
	}

	for i := 0; i < len(zjh.userData); i++ {
		dstuser = zjh.userData[i]
		if dstuser.User.SeatNo != ud.User.SeatNo && dstuser.canOpt() {
			break
		}
	}

	odd := zjh.getOdds(ud, true)
	zjh.GameLog.AddGameStep("在第%d轮用户%d和用户%d自动比牌，扣钱%d", zjh.currturn, ud.User.UserID, dstuser.User.UserID, zjh.betCurr*odd)
	zjh.userCompareOpt(ud, dstuser)
}

//用户弃牌
func (zjh *ZhaJinHuaLogic) userDrop(user *gamelogic.GameUser) {
	if user == nil {
		mlog.Error("無玩家資料")
		return
	}

	urd := zjh.getUserBySeatNo(user.SeatNo)
	if urd == nil {
		mlog.Error("用户弃牌 Error ")
		return
	}

	if urd.AllInUserId == urd.User.UserID {
		mlog.Error("主動全押的玩家不能棄牌")
		return
	}

	urd.SetDrop()

	zjh.GameLog.AddGameStep("在第%d轮用户%d弃牌", zjh.currturn, urd.User.UserID)
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_DropBroadCastID)
	pdata := new(netproto.ZJH_SeatNO)
	pdata.Seatno = proto.Int32(user.SeatNo)
	pmsg.MsgData = pdata

	zjh.BoardcastMessage(pmsg)

	zjh.userCount--
	if zjh.currSeat == user.SeatNo || zjh.userCount <= 1 { //當前操作不是自已，且用戶只剩一個人
		zjh.waitOpt()
	}
	//zjh.earlyChangeTable(urd)

	if urd.User.IsOffLine() {
		zjh.earlyChangeTable(urd, "leave")
	}
}

//处理用户进入
func (zjh *ZhaJinHuaLogic) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	if gameuser == nil {
		mlog.Debug("no user data")
		return
	}

	//隨機名字
	if zjh.radomnickname == 0 {
		gameuser.NickName = fmt.Sprintf("%d", gameuser.UserID)
	} else {
		gameuser.NickName = zjh.getNickName()
	}
	//隨機頭像

	zjh.DynamicGameLogicBase.UserEnter(gameuser, tableno, seatno)

	//没有庄家游戏还没开始
	if zjh.zhuangSeat == -1 {
		return
	}
	//如果非游戏玩家，发送当前场景
	if !zjh.IsGameUser(gameuser) {
		zjh.sendGameScene(gameuser)
	}
}

//用户断线重连
func (zjh *ZhaJinHuaLogic) UserRecover(gameuser *gamelogic.GameUser) {
	if gameuser == nil {
		mlog.Debug("no user data")
		return
	}

	zjh.DynamicGameLogicBase.UserRecover(gameuser)

	//发送游戏场景
	zjh.sendGameScene(gameuser)
}

//发送当前游戏场景
func (zjh *ZhaJinHuaLogic) sendGameScene(user *gamelogic.GameUser) {
	if user == nil {
		mlog.Debug("無玩家資料")
		return
	}

	if zjh.bGameEnd {
		mlog.Debug("游戏结束了，不发送场景数据了")
		return
	}

	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_ZJHGameSceneID)
	pdata := new(netproto.ZJH_UserInfoArr)
	pdata.CurrOptSeatno = proto.Int32(zjh.currSeat)
	opt := int32(0)
	if zjh.opttimer != nil {
		opt = zjh.opttimer.WaitSeconds - zjh.opttimer.ElapsedSeconds() - 1
	}

	ud := zjh.getUserBySeatNo(user.SeatNo)
	if ud != nil {
		pdata.FollowEnd = proto.Bool(ud.isFollowEnd)
		pdata.Allin = proto.Bool(ud.isAllin)
	} else {
		pdata.FollowEnd = proto.Bool(false)
		pdata.Allin = proto.Bool(false)
	}

	pdata.OptTime = proto.Int32(opt)
	pdata.OptTimeTotal = proto.Int32(zjh.optTime)
	pdata.ZhuangSeatNO = proto.Int32(zjh.zhuangSeat)
	pdata.BetTotal = proto.Int64(zjh.betTotal)
	pdata.Currturn = proto.Int32(zjh.currturn)
	//pdata.AllinStatus = proto.Bool()

	for i := 0; i < len(zjh.betAdd); i++ {
		pdata.BetArr = append(pdata.BetArr, zjh.betAdd[i])
	}

	for _, ud := range zjh.userData {
		if ud == nil {
			continue
		}

		userinfo := new(netproto.ZJH_UserInfo)
		if ud.isClear {
			userinfo.Seatno = proto.Int32(-1)
		} else {
			userinfo.Seatno = proto.Int32(ud.User.SeatNo)
		}

		userinfo.Check = proto.Bool(ud.isCheck)
		userinfo.Drop = proto.Bool(ud.isDrop)
		userinfo.Comparefailed = proto.Bool(ud.isFaild)
		betmoney := ud.userMoney - ud.User.Score //下注的金額
		userinfo.BetMoney = proto.Int64(-betmoney)
		pdata.UserInfo = append(pdata.UserInfo, userinfo)
	}

	pmsg.MsgData = pdata
	zjh.SendMessageToUser(user, pmsg)

	if ud != nil {
		if ud.isCheck && ud.canOpt() {
			zjh.SendMessageToUser(user, zjh.getPokerMsg(ud))
		}

		if zjh.userCount <= 1 && zjh.bSendGameResult == true {
			zjh.calWinMoney(ud)
			zjh.sendComparePoker(ud)
			zjh.SendMessageToUser(user, zjh.getGameRetMsg())
		}
	}

	if user.SeatNo == zjh.currSeat && zjh.userCount > 1 {
		zjh.sendOptMsg(true)
	}
}

//广播用户看牌
func (zjh *ZhaJinHuaLogic) broadCastCheckCard(seatno int32) {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_CheckCardsBroadCastID)
	pdata := new(netproto.ZJH_SeatNO)
	pdata.Seatno = proto.Int32(seatno)
	pmsg.MsgData = pdata

	zjh.BoardcastMessage(pmsg)
}

//扣用户操作钱
func (zjh *ZhaJinHuaLogic) setUserMoney(urd *ZjhUserData, bCompare bool) {
	if urd == nil {
		return
	}

	odd := zjh.getOdds(urd, bCompare)
	money := zjh.betCurr * odd
	zjh.betTotal += int64(money)
	urd.SetMoney(int64(-money))
}

//获取牌数据消息
func (zjh *ZhaJinHuaLogic) getCardsNetMsg(urd *ZjhUserData) *netproto.UserPoker {
	pokermsg := new(netproto.UserPoker)

	if urd != nil {
		pokermsg.UserID = proto.Int32(urd.User.UserID)
		pokermsg.SeatNo = proto.Int32(urd.User.SeatNo)
		cardslen := len(urd.userCards.Cards)
		for i := 0; i < cardslen; i++ {
			npk := new(netproto.PokerCard)
			npk.Color = proto.Int32(urd.userCards.Cards[i].Color)
			npk.CardNumber = proto.Int32(urd.userCards.Cards[i].CardNumber)
			pokermsg.Cards = append(pokermsg.Cards, npk)
		}
	}

	return pokermsg
}

//发送准备消息
func (zjh *ZhaJinHuaLogic) sendReadyTime() {
	zjh.bGameEnd = true
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_ReadyTime)

	pdata := new(netproto.ZJH_ReadyTime)
	pdata.ReadyTime = proto.Int32(zjh.readyTime)
	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)
	mlog.Debug("GameEndFunc 发送倒计时 readyTime[%v]", zjh.readyTime)
	zjh.GameEndFunc()
}

//换牌
func (zjh *ZhaJinHuaLogic) ExchangeMaxCard(userid int32) {
	if zjh.disRobotExchangeCard {
		return
	}
	zjh.GameLogicBase.ExchangeMaxCard(userid)
	var maxRetUserID int32

	var maxcg *poker.CardGroup
	if zjh.poker.(*poker.FixedPoker) == nil {
		example.invalid("*poker.FixedPoker:%v", zjh.poker.(*poker.FixedPoker))
	}

	fpoker := zjh.poker.(*poker.FixedPoker)
	for _, ur := range zjh.AllTableUser() {
		if ur == nil {
			continue
		}

		cg := fpoker.GetUserCards(ur.UserID)
		zjh.sortCard(cg)
		if maxcg == nil {
			maxcg = cg
			maxRetUserID = ur.UserID

			continue
		}

		//比较牌型

		if comPareCards(maxcg, cg) != 1 {
			maxcg = cg
			maxRetUserID = ur.UserID
		}
	}

	if maxRetUserID == 0 {
		return
	}

	mlog.Debug("maxcg=%v", maxcg.Cards)
	example.invalid("交换用户%d和用户%d牌型", userid, maxRetUserID)
	zjh.GameLog.AddGameStep("交换用户%d和用户%d牌型", userid, maxRetUserID)
	fpoker.ExChange(userid, maxRetUserID)
}

func (zjh *ZhaJinHuaLogic) VipExchangeCard(userid int32, max int) { //1是最大, 2是最小
	zjh.GameLog.AddGameStep("交換%d交換用戶牌型 %v, (1 最大, 2 最小)", userid, max)
	var retUserID int32

	var retcg *poker.CardGroup
	if zjh.poker.(*poker.FixedPoker) == nil {
		example.invalid("*poker.FixedPoker:%v", zjh.poker.(*poker.FixedPoker))
	}

	fpoker := zjh.poker.(*poker.FixedPoker)
	for _, ur := range zjh.AllTableUser() {
		cg := fpoker.GetUserCards(ur.UserID)
		if cg == nil {
			continue
		}

		zjh.sortCard(cg)
		if retcg == nil {
			retcg = cg
			retUserID = ur.UserID
			continue
		}

		//比较牌型

		if comPareCards(retcg, cg) != max {
			retcg = cg
			retUserID = ur.UserID
		}
	}

	if retUserID == 0 {
		return
	}

	example.invalid("retcg=%v", retcg.Cards)
	example.invalid("交换用户%d和用户%d牌型", userid, retUserID)
	zjh.GameLog.AddGameStep("交换用户%d和用户%d牌型", userid, retUserID)
	fpoker.ExChange(userid, retUserID)
}

func (zjh *ZhaJinHuaLogic) getOdds(ud *ZjhUserData, bCompare bool) int32 {
	odd := int32(1)

	if ud != nil {
		if ud.isCheck {
			odd = odd * 2
		}
	}
	if bCompare {
		odd = odd * 2
	}

	return odd
}

func (zjh *ZhaJinHuaLogic) broadCastCurrturn() {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_GameCurrturn)
	pdata := new(netproto.ZJH_Currturn)
	pdata.Currturn = proto.Int32(zjh.currturn)
	//如果當下沒有玩家 回傳空值
	pmsg.MsgData = pdata
	zjh.BoardcastMessage(pmsg)
}

func (zjh *ZhaJinHuaLogic) userTimeOutDrop() {
	urd := zjh.getUserBySeatNo(zjh.currSeat)
	if urd == nil {
		mlog.Error("用户弃牌 Error ")
		return
	}

	urd.SetDrop()

	zjh.GameLog.AddGameStep("在第%d轮用户%d弃牌", zjh.currturn, urd.User.UserID)
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_DropBroadCastID)
	pdata := new(netproto.ZJH_SeatNO)
	pdata.Seatno = proto.Int32(zjh.currSeat)
	pmsg.MsgData = pdata

	zjh.BoardcastMessage(pmsg)

	zjh.userCount--
	zjh.waitOpt()
	//zjh.earlyChangeTable(urd)

	if urd.User.IsOffLine() {
		zjh.earlyChangeTable(urd, "leave")
	}
}

//获取扑克消息
func (zjh *ZhaJinHuaLogic) getPokerType(ud *ZjhUserData) *msg.Message {
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.ZJH_GameMessageClassID_UserCardType)

	if ud != nil {
		pmsg.MsgData = zjh.getPokerTypeMsg(ud)
	}

	return pmsg
}

func (zjh *ZhaJinHuaLogic) getNickName() string {
	nameString := []string{"大欣becky", "Flomama", "一楠_jyn", "Uni颖儿", "深海贝类的围脖", "雨歌阿", "主持人-王不凡", "周大静_", "卢伟阿", "Mx沫浠", "郑迦文", "杰西酷克", "张大麦和马包包", "王者荣耀神吐槽", "一米小厨娘", "张猫要练嘴皮子", "KatAndSid思凯传媒", "光灿", "达人说", "婉锐", "快美妆BeautyQ", "青藤文化", "papitube", "洋葱视频", "歪果仁研究协会", "橘子娱乐灵药LoveLing ", "黄文煜小盆友", "nG家的猫", "李明霖 ", "-王小强- ", "马克Malik", "铁馆教练", "kanahoooo", "造型师小邱Elvan微小微是你李公子啊", "许墨Zzz", "小威廉的情书", "吕浩男", "林森男神经", "陈吃吃好饿", "智博君李蒲赫", "永远阳光的小宝", "张哲睿bikinir", "la子豪俊平大魔王", "杨霞-Sunny", "王岳鹏Niko", "陈子蜜", "潘雨润PanYR_", "小猪姐姐zz", "扇", "NO_FAN_NO_FUN", "Rika0_0", "Iam方美丽同学", "余潇潇JessicaLinForeverGirl ", "delicious大金", "美美de夏夏啊", "小刘小粒赵大喜", "梦姣--于MOMO", "谢梦Tse", "阿希哥VCRUAN", "滕雨佳Amiu", "Fancystyle", "魏妮妮Yanni大胃王密子君", "美食家大雄", "中华小鸣仔", "锅包肉博士", "香喷喷的小烤鸡", "王大厨", "Amanda", "小厨房", "李子柒", "野食小哥", "办公室小野回忆专用小马甲", "国民老岳父公", "6岛岛", "大锅只是喝醉", "LEO它爹", "大胖大胖", "王白菜-", "酥饼大人", "小H的成长日记", "野老多周玥", "昼夜", "1022女声", "安子与九妹", "沈虫虫", "_胖胖胖_", "李延亮", "阿细-CHEUNG", "奇然liya", "上海彩虹室内合唱团Miss", "Misaya若风lol", "小苍cany", "嗨氏", "dsm大司马解说", "ixm解说小漠", "Esports海涛", "骚男丶注意马叉虫", "靠脸吃饭的徐大王", "起小点同道大叔", "软软其实不太硬", "董新尧", "我的女友嘴很贱", "英国报姐", "王尼玛", "杀马特强子", "小野妹子学吐槽", "追风少年刘全有", "妖妖小精苏小白Alex", "盖达巍", "活力女毒姜允儿", "yero鱼哥", "赵一婷1358", "萌小美Monkey", "冯朗朗", "十三Ace", "一只乔玉鼓山文化", "楼氏传播", "如涵", "宸帆", "蜂群文化", "魔力TV", "大禹网络", "壹加娱乐", "忘忧传媒", "缇苏张沫凡MOMO", "悉尼Cherie", "papi酱", "张大奕eve", "回忆专用小马甲", "同道大叔", "林珊珊_Sunny", "吴大伟DvWooooo", "冯提莫", "陈一发", "周二珂", "章泽天", "莉哥o3o", "摩登兄弟", "一禅小和尚", "会说话的刘二豆", "虎神", "阿哲"}
	//nameString := []string{"资阳市"}
	name := nameString[rand.Intn(len(nameString))]
	return name
}

// 設定新的牌組，用於處理發牌後剩下的牌
func (zjh *ZhaJinHuaLogic) SetNewZJHPoker(bHasKing bool, bHasPiZi bool, iCount int) *poker.CardGroup {
	pNewCardSet := new(poker.CardGroup)

	for i := 0; i < iCount; i++ {
		for iColor := poker.PokerColor_FangKuai; iColor <= poker.PokerColor_HeiTao; iColor++ {
			for iNo := poker.PokerNumber_Two; iNo <= poker.PokerNumber_A; iNo++ {
				pCard := poker.NewPokerCard(iColor, iNo)
				pNewCardSet.AddCard(pCard)
			}
		}
	}

	return pNewCardSet
}

// 機器人是否發動賭神模式
func (zjh *ZhaJinHuaLogic) InitiateRobotToGod(i32RobotSeatNo, i32UserSeatNo int32) bool {
	if zjh.currturn < GAME_AllowCompareTurn {
		return false
	}

	// 獲取機器人及玩家資料
	pRobot := zjh.getUserBySeatNo(i32RobotSeatNo)
	pUser := zjh.getUserBySeatNo(i32UserSeatNo)
	if pRobot == nil {
		mlog.Error("無機器人SeatNo[%d]資料", i32RobotSeatNo)
		return false
	} else if pUser == nil {
		mlog.Error("無玩家SeatNo[%d]資料", i32UserSeatNo)
		return false
	}

	if !pRobot.User.IsRobot() || pUser.User.IsRobot() {
		return false
	}

	if zjh.robotCharacter[pRobot.User.UserID] == nil {
		mlog.Error("機器人[%d]無個性資料", pRobot.User.UserID)
		return false
	}

	iCompareCardRet := int(-1)
	bRobotIsGod := false

	iRnd := int32(util.RndInt(1, 100))
	if iRnd < zjh.robotCharacter[pRobot.User.UserID].iWinRate*pRobot.User.RobotLevel {
		bRobotIsGod = true
	}
	mlog.Debug("賭神模式：Robot[%d], iRnd[%d], iWinRate[%d], RobotLevel[%d], bRobotIsGod[%t]",
		pRobot.User.UserID, iRnd, zjh.robotCharacter[pRobot.User.UserID].iWinRate, pRobot.User.RobotLevel, bRobotIsGod)

	// 如果機器人變為賭神模式，則跟所有真人比牌，換成必勝的牌
	if bRobotIsGod {
		// 換牌
		bChangeCard := true
		iCompareCardRet = comPareCards(pRobot.userCards, pUser.userCards)
		if iCompareCardRet == -1 {
			mlog.Debug("比牌結果錯誤")
			return false
		} else if iCompareCardRet == USER1 {
			bChangeCard = false
		}

		//if !bChangeCard {
		//	mlog.Debug("Robot[%d][%v], User[%d][%v], Robot wins", pRobot.User.UserID, pRobot.userCards, pUser.User.UserID, pUser.userCards)
		//} else {
		//	mlog.Debug("Robot[%d][%v], User[%d][%v], User wins", pRobot.User.UserID, pRobot.userCards, pUser.User.UserID, pUser.userCards)
		//}

		if bChangeCard {
			mlog.Debug("機器人[%d]與玩家[%d]比牌，開啟賭神模式, 玩家手牌：%v",
				pRobot.User.UserID, pUser.User.UserID, pUser.userCards)
			mlog.Debug("機器人[%d], Before SetGodCards: %v", pRobot.User.UserID, pRobot.userCards)
			zjh.GameLog.AddGameStep("機器人[%d]與玩家[%d]比牌，開啟賭神模式, 玩家手牌：%v",
				pRobot.User.UserID, pUser.User.UserID, pUser.userCards)
			zjh.GameLog.AddGameStep("機器人[%d]換牌前手牌：%v", pRobot.User.UserID, pRobot.userCards)

			pRobot.userCards, zjh.pRestCardSet = setGodRobotCards(zjh.pRestCardSet, pRobot.userCards, pUser.userCards)

			mlog.Debug("機器人[%d], After SetGodCards: %v", pRobot.User.UserID, pRobot.userCards)
			zjh.GameLog.AddGameStep("機器人[%d]換牌後手牌：%v", pRobot.User.UserID, pRobot.userCards)

			// 變更儲值達標可看牌玩家各家牌組資料
			zjh.userinthisgame = nil
			for _, pUser := range zjh.userData {
				if pUser != nil {
					zjh.userinthisgame = append(zjh.userinthisgame, zjh.getPokerTypeMsg(pUser))
				}
			}
		}
	}

	return true
}

// ★★★先保留日後可做 debug 用 begin
// 印出玩家手牌資料
func (zjh *ZhaJinHuaLogic) PrintUserCardsInfo(strNote string) {
	str := ""
	iType := int32(SanPai)
	for _, pUser := range zjh.userData {
		if pUser != nil {
			iType = GetType(pUser.userCards)
			str += fmt.Sprintf("User[%d(%d)][%v][%s(%d)], ",
				pUser.User.UserID, pUser.User.SeatNo, pUser.userCards, typeStringMap[int(iType)], iType)
		}
	}
	mlog.Debug("%s, 玩家手牌：%s", strNote, str)

}

// 印出 userinthisgame 資料用
func (zjh *ZhaJinHuaLogic) PrintUserInThisGame() {
	str := ""
	for _, pData := range zjh.userinthisgame {
		if pData != nil {
			pCardSet := new(poker.CardGroup)
			for _, pCard := range pData.Userpokergroup.Cards {
				if pCard != nil {
					pCardSet.AddCard(&poker.PokerCard{int32(*pCard.Color), int32(*pCard.CardNumber)})
				}
			}
			str += fmt.Sprintf("[User(%v), Type:%v(%s), Cards:%v]", *pData.Userpokergroup.UserID,
				*pData.PokerType, typeStringMap[int(*pData.PokerType)], pCardSet)
		}
	}
	mlog.Debug("userinthisgame, %s", str)
}

//冤家牌, 当桌只有一个含对子以上牌时, 再过一次概率牌给散牌其中一人
func (zjh *ZhaJinHuaLogic) YuanChiaFapai(ary_pUserCards []*poker.CardGroup) []*poker.CardGroup {
	bigthenDuizi := 0
	for _, v := range ary_pUserCards {
		iCardType := GetType(v)
		if iCardType >= DuiZi {
			bigthenDuizi++
		}
	}

	if bigthenDuizi != 1 {
		mlog.System("[weihao] 超过一个玩家有对子, 或都是散牌")
		return ary_pUserCards
	}

	rnd := util.RndInt(1, 1001)
	mlog.System("[weihao] 冤家牌, rnd[%v] 冤家新机率[%v]", rnd, zjh.yuanchiaNewRand)
	if int64(rnd) <= zjh.yuanchiaNewRand {
		mlog.Error("[weihao] 启动冤家牌")
		pUserCards := new(poker.CardGroup)
		iFlagID, _ := strconv.Atoi(zjh.TableManager.Room.Flag)
		// 亂數取牌型
		iRnd := util.RndInt(1, int(map_pBasicSetting[int32(iFlagID)].I32WeightSum + 1))
		iCardType := SanPai
		for iType := SanPai; iType < CardType_Count; iType++ {
			mlog.Warn("[weihao] type[%v] oddRange[%v]", iType, map_pCardTypeOddsRange[int32(iFlagID)].OddsRange[iType])
			if iRnd - int(map_pCardTypeOddsRange[int32(iFlagID)].OddsRange[iType]) <= 0 {
				iCardType = iType
				break
			}
		}
		mlog.System(" Rnd[%d], iCardType[%d]%s -----", iRnd, iCardType, typeStringMap[iCardType])
		pUserCards, iCardType, zjh.pRestCardSet = GetCardsByCardType(iCardType, zjh.pRestCardSet)
		mlog.Debug("[Joecool] UserCards: %s, Type[%s]", pUserCards.String(), typeStringMap[iCardType])
		mlog.Debug("[Joecool] RestCard: %s", zjh.pRestCardSet.String())

		//替换散牌
		if iCardType >= DuiZi {
			for _, v := range ary_pUserCards {
				vCardType := GetType(v)
				if vCardType == SanPai {
					mlog.System("[weihao] before 冤家替换[%v] ", v.String())
					v.Cards = pUserCards.Cards
					mlog.System("[weihao] 冤家替换 new[%v] ", v.String())
					break
				}
			}
		}

		for _, v := range ary_pUserCards {
			mlog.System("[weihao] 确定牌[%v] ", v.String())
		}

		return ary_pUserCards
	} else {
		return ary_pUserCards
	}
	return ary_pUserCards
}

// ★★★先保留日後可做 debug 用 end
