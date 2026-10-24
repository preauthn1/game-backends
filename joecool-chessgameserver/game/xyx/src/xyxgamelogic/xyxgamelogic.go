package xyxgamelogic

import (
	"config"
	"event"
	"gamelogic"
	"mlog"
	"msg"
	"netproto"
)

import proto "example.invalid/golang/protobuf/proto"

const (
	gameStep_ANI_Start = iota //开始动画
	gameStep_BaseScore        //选择底注
	gameStep_ANI_Play         //遊戲动画
	gameStep_GameRet          //遊戲結果
)

const (
	actionName_BaseScore = "actionName_basescore" //选择底注
	actionName_GameRet   = "actionName_GameRet"   //遊戲結果
)

const (
	animationName_GameStart = "gamestart" //开始动画
	animationName_Play      = "play"      //发牌动画
	animationName_GameRet   = "gameret"   //遊戲結果

	animationTimeOut_GameStart = 2 //开始动画
	animationTimeout_Play      = 2 //发牌动画
	animationTimeout_GameRet   = 2 //遊戲結果
)

//小遊戲
type XYXGameLogic struct {
	*gamelogic.DynamicGameLogicBase
	gameStep int                   //游戏阶段
	gameBet  *gameBet              //用户底注
	gameRet  *netproto.XYX_GameRet //游戏结果

	currentWatchAnimationName string //当前动画名称

	actionName_BaseScore string //选择底注
}

//小遊戲對像
func NewXYXGameLogic(t *gamelogic.Table, tfManager *event.TimerFuncManager, cfgs *config.ConfigGroup) *XYXGameLogic {
	glc := new(XYXGameLogic)
	glc.DynamicGameLogicBase = gamelogic.NewDynamicGameLogicBase(t, tfManager, cfgs)

	//解析配置
	parseXYXConfig(cfgs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER))

	glc.RegisterAfterGameStartFunc(func() {

		//選擇底注
		glc.currentWatchAnimationName = glc.RegisterAnimation(animationName_GameStart, animationTimeOut_GameStart, glc.baseScoreSetting)
	})

	glc.RegisterBeforeGameStartFunc(func() {
		//清除数据
		glc.clearGameData()
	})
	return glc
}

func (xyx *XYXGameLogic) clearGameData() {

	xyx.gameStep = gameStep_ANI_Start //设置游戏阶段
	xyx.gameBet = newGameBet()        //投注對象
	xyx.gameRet = nil

	xyx.actionName_BaseScore = ""
}

//计算用户底注倍数
func (xyx *XYXGameLogic) computeUserBetTimes(ur *gamelogic.GameUser) int32 {

	//计算用户最大倍数
	userMaxBetTimes := int64(ur.Score / int64(xyx.GetServerConfig().BaseScore))

	if userMaxBetTimes >= int64(getXYXConfig().maxBaseScoreTimes()) {
		userMaxBetTimes = int64(getXYXConfig().maxBaseScoreTimes())
	}

	return int32(userMaxBetTimes)
}

//用户进入桌子
func (xyx *XYXGameLogic) UserEnter(gameuser *gamelogic.GameUser, tableno int32, seatno int32) {
	xyx.DynamicGameLogicBase.UserEnter(gameuser, tableno, seatno)

	//发送游戏设置
	xyx.sendGameSetting(gameuser)
	example.invalid("===================[%v] UserEnter", gameuser.UserID)
	//如果非游戏玩家，发送当前场景
	if xyx.IsGaming() && !xyx.IsGameUser(gameuser) {
		example.invalid("===================[%v] User IsGaming", gameuser.UserID)
		xyx.sendGameScene(gameuser)
	}
}

//用户断线重连
func (xyx *XYXGameLogic) UserRecover(gameuser *gamelogic.GameUser) {

	xyx.DynamicGameLogicBase.UserRecover(gameuser)

	//发送游戏设置
	xyx.sendGameSetting(gameuser)

	//发送游戏场景
	if xyx.IsGaming() {
		xyx.sendGameScene(gameuser)
	}
}

func (xyx *XYXGameLogic) GameMessage(user *gamelogic.GameUser, gmsg *msg.Message) {
	//检查非游戏玩家
	if !xyx.CheckGameUserMessage(user) {
		xyx.KickUserFunc(user.UserID, "非法操作")
		return
	}

	//底注
	if gmsg.BClassID == int32(netproto.MessageBClassID_Game) && gmsg.SClassID == int32(netproto.XYX_GameMessageClassID_XYX_UserBaseScoreID) {
		xyx.onUserBaseScore(user, gmsg.MsgData.([]byte))
	}

	//检查用户动画播放
	xyx.CheckUserAnimationCommited(user, gmsg, xyx.currentWatchAnimationName, nil)

}

//选择底注
func (xyx *XYXGameLogic) baseScoreSetting() {
	//设置游戏阶段
	xyx.gameStep = gameStep_BaseScore

	xyx.actionName_BaseScore = xyx.PlayerActionManager.NewAction(actionName_BaseScore, int32(xyx.GetCurrentGameUserCount()), getXYXConfig().baseScoreTimeout, xyx.baseScoreConfirm)

	//广播游戏阶段
	xyx.boardcastGameStep()
}

//用户底注
func (xyx *XYXGameLogic) onUserBaseScore(user *gamelogic.GameUser, data []byte) {
	if xyx.gameStep != gameStep_BaseScore {
		example.invalid("用户%v发送底注消息,但当前游戏阶段是%d", user, xyx.gameStep)
		//xyx.KickUserFunc(user.UserID, "非法操作")
		return
	}

	ust := &netproto.XYX_UserBaseScore{}

	err := proto.Unmarshal(data, ust)
	if err != nil {
		mlog.Error("解析用户%v底注数据结构错误%v", user, err)
		xyx.KickUserFunc(user.UserID, "非法操作")
		return
	}

	//检查用户底注倍数
	ubtimes := xyx.computeUserBetTimes(user)
	if ubtimes < ust.GetBaseScore() {
		mlog.Error("用户%v提交了非法的底注%d。不能超过%d", user, ust.GetBaseScore(), ubtimes)
		xyx.KickUserFunc(user.UserID, "非法操作")
		return
	}

	if !xyx.gameBet.checkBetTimes(ust.GetBaseScore()) {
		mlog.Error("用户%v提交了非法的底注%d", user, ust.GetBaseScore())
		xyx.KickUserFunc(user.UserID, "非法操作")
		return
	}

	//添加用户底注
	if xyx.gameBet.addUserBet(user.UserID, ust.GetBaseScore()) {
		xyx.boardCastUserBaseScore(user.UserID, ust.GetBaseScore())
	}

	//通知用户动作
	xyx.PlayerActionManager.OnUserAction(xyx.actionName_BaseScore, user.UserID)

	//牌局记录
	xyx.GameLog.AddGameStep("用户%d选择底注%d", user.UserID, ust.GetBaseScore())

}

//底注确定
func (xyx *XYXGameLogic) baseScoreConfirm() {
	for _, ur := range xyx.AllGameUser() {
		xyx.gameBet.ensureUserBet(ur.UserID)
	}

	//广播游戏底注
	xyx.boardCastGameBaseScore()

	//设置游戏阶段
	xyx.gameStep = gameStep_ANI_Play

	//广播游戏阶段
	xyx.boardcastGameStep()

	//发牌
	xyx.play()
}

//小遊戲
func (xyx *XYXGameLogic) play() {
	//设置游戏阶段
	xyx.gameStep = gameStep_GameRet

	xyx.currentWatchAnimationName = xyx.RegisterAnimation(animationName_Play, animationTimeout_Play, xyx.playFinish)

	//广播游戏阶段
	xyx.boardcastGameStep()
}

//遊戲結束
func (xyx *XYXGameLogic) playFinish() {
	//亮牌阶段
	xyx.gameStep = gameStep_GameRet
	//广播游戏阶段
	xyx.boardcastGameStep()

	//结算
	gameret := newXYXGameRet(xyx).getGameRet()

	//牌局结果
	uscores := make(map[int32]int64)
	for _, ret := range gameret.UserRet {
		//计算税收
		amount, tax := xyx.GetUserWinAndTax(ret.GetWinLose())

		//写分
		xyx.UpdateUserScoreFunc(ret.GetUserID(), int64(amount), int64(tax))

		ret.WinLose = proto.Int64(amount)

		//牌局结果
		uscores[ret.GetUserID()] = amount

	}

	//设置游戏结果
	xyx.gameRet = gameret

	//广播游戏结果
	xyx.boardCastGameRet()

	//结算动画
	xyx.currentWatchAnimationName = xyx.RegisterAnimation(animationName_GameRet, animationTimeout_GameRet, func() {
		xyx.GameEndFunc()
		xyx.clearGameData()
	})

	//牌局记录
	xyx.GameLog.GameEnd(uscores)
}

//广播游戏结果
func (xyx *XYXGameLogic) boardCastGameRet() {
	gmsg := new(msg.Message)
	gmsg.BClassID = int32(netproto.MessageBClassID_Game)
	gmsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_GameRetID)

	gmsg.MsgData = xyx.gameRet
	xyx.BoardcastMessage(gmsg)
}

//广播游戏阶段
func (xyx *XYXGameLogic) boardcastGameStep() {
	xyx.BoardcastMessage(xyx.createGameStepMsg())
}

//构造游戏阶段消息
func (xyx *XYXGameLogic) createGameStepMsg() *msg.Message {
	gsmsg := new(msg.Message)
	gsmsg.BClassID = int32(netproto.MessageBClassID_Game)
	gsmsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_GameStepID)

	gsdata := new(netproto.XYX_GameStep)
	gsdata.Step = proto.Int32(int32(xyx.gameStep))
	gsmsg.MsgData = gsdata

	return gsmsg
}

//广播用户底注信息
func (xyx *XYXGameLogic) boardCastUserBaseScore(userid int32, baseScore int32) {
	gmsg := new(msg.Message)
	gmsg.BClassID = int32(netproto.MessageBClassID_Game)
	gmsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_NotifyUserBaseScoreID)

	ubtData := new(netproto.XYX_NotifyUserBaseScore)
	ubtData.UserID = proto.Int32(userid)
	ubtData.BaseScore = proto.Int32(baseScore)
	gmsg.MsgData = ubtData

	xyx.BoardcastMessage(gmsg)
}

//广播游戏底注信息
func (xyx *XYXGameLogic) boardCastGameBaseScore() {
	gmsg := new(msg.Message)
	gmsg.BClassID = int32(netproto.MessageBClassID_Game)
	gmsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_NotifyGameBaseScoreID)
	gmsg.MsgData = xyx.getGameBaseScoreData()

	xyx.BoardcastMessage(gmsg)
}

//构造游戏底注数据
func (xyx *XYXGameLogic) getGameBaseScoreData() *netproto.XYX_NotifyGameBaseScore {
	gbtData := new(netproto.XYX_NotifyGameBaseScore)

	for _, ubt := range xyx.gameBet.userbets {
		ubtData := new(netproto.XYX_NotifyUserBaseScore)
		ubtData.UserID = proto.Int32(ubt.userID)
		ubtData.BaseScore = proto.Int32(ubt.betTimes)

		gbtData.GameBet = append(gbtData.GameBet, ubtData)
	}

	return gbtData
}

//发送游戏配置
func (xyx *XYXGameLogic) sendGameSetting(ur *gamelogic.GameUser) {
	gmsg := new(msg.Message)
	gmsg.BClassID = int32(netproto.MessageBClassID_Game)
	gmsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_GameSettingID)

	stData := new(netproto.XYX_GameSetting)
	stData.BaseScoreTimeout = proto.Int32(getXYXConfig().baseScoreTimeout)
	stData.BaseScore = proto.Int32(xyx.GetServerConfig().BaseScore)
	stData.TableNo = proto.Int32(xyx.TableNo())
	stData.GameID = proto.Int32(xyx.GetServerConfig().GameID)
	stData.ReadyTimeout = proto.Int32(xyx.GetServerConfig().ReadyWaitTime)
	stData.BaseScoreTimes = getXYXConfig().baseScoreTimes

	gmsg.MsgData = stData

	xyx.BoardcastMessage(gmsg)
}

//发送当前游戏场景
func (xyx *XYXGameLogic) sendGameScene(user *gamelogic.GameUser) {
	sceneMsg := new(msg.Message)
	sceneMsg.BClassID = int32(netproto.MessageBClassID_Game)
	sceneMsg.SClassID = int32(netproto.XYX_GameMessageClassID_XYX_GameSceneID)

	sceneData := new(netproto.XYX_GameScene)
	sceneMsg.MsgData = sceneData
	example.invalid("===============================[%v] sceneData %v", user.UserID, sceneData)
	//游戏阶段
	sceneData.GameStep = proto.Int32(int32(xyx.gameStep))

	//设置倒计时数字
	var elapsedSeconds int32 = 0
	//选择底注阶段
	if xyx.gameStep == gameStep_BaseScore {
		elapsedSeconds = getXYXConfig().baseScoreTimeout - xyx.PlayerActionManager.GetActionTimerElapsedSeconds(xyx.actionName_BaseScore)

	}

	sceneData.ElapsedSeconds = proto.Int32(elapsedSeconds)

	//设置用户底注倍数
	userMaxBetTimes := xyx.computeUserBetTimes(user)
	for i := 0; i < len(getXYXConfig().baseScoreTimes); i++ {
		bt := getXYXConfig().baseScoreTimes[i]

		if bt <= userMaxBetTimes {
			sceneData.BetTimes = append(sceneData.BetTimes, bt)
		}
	}

	//底注
	if xyx.gameStep >= gameStep_BaseScore {
		sceneData.GameBaseScore = xyx.getGameBaseScoreData()
	}

	//亮牌阶段
	if xyx.gameStep == gameStep_GameRet {
		sceneData.GameRet = xyx.gameRet
	}

	xyx.SendMessageToUser(user, sceneMsg)
}
