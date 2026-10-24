package qznngamelogic_rndz

import (
	"netproto"
)

import (
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
	"util"
	"msg"
)

//牛牛牌局结果
type nnGameRet struct {
	nnlgc *QZNNGameLogic
}

//创建牛牛游戏结果对象
func newNNGameRet(nnglc *QZNNGameLogic) *nnGameRet {
	nnret := new(nnGameRet)
	nnret.nnlgc = nnglc

	return nnret
}

//获取游戏结果
func (nnret *nnGameRet) getGameRet(qznn *QZNNGameLogic) *netproto.QZNN_GameRet {
	gret := new(netproto.QZNN_GameRet)

	//庄家牌型
	zjuid := nnret.nnlgc.zhuangInfo.userID

	//庄家牌型
	mlog.System("[weihao] 获取游戏结果 庄家牌型")
	zjpoker := nnret.nnlgc.gamePoker.GetUserPoker(zjuid)

	//庄家输赢
	var zhuangwinlose int64 = 0

	zjcardret := getNiuNiuRet(zjpoker.Cards)
	gret.ZhuangRet = new(netproto.QZNN_UserRet)
	gret.ZhuangRet.NiuNiuRet = proto.Int32(int32(zjcardret.niuRet))       //庄家牛牛结果
	gret.ZhuangRet.NiuNiuRetNumber = proto.Int32(int32(zjcardret.number)) //庄家牛牛点数
	gret.ZhuangRet.NiuNiuBetTimes = proto.Int32(0)                        //庄家倍数
	gret.ZhuangRet.WinLose = proto.Int64(zhuangwinlose)                   //庄家输赢
	gret.ZhuangRet.User = nnret.nnlgc.GetUserByUserID(zjuid).GetProtoBufGameUser()
	gret.ZhuangRet.CardGroup = zjcardret.getNetCardGroup()

	//闲家牌型
	for _, xjbt := range nnret.nnlgc.xianjiaBets.userBets {
		mlog.System("[weihao] 获取游戏结果 闲家牌型")
		xjpoker := nnret.nnlgc.gamePoker.GetUserPoker(xjbt.userID)

		xjcardret := getNiuNiuRet(xjpoker.Cards)
		betret := compareCards(xjcardret, zjcardret)

		//闲家输赢金币数
		//xjwinlose := int64(nnret.nnlgc.zhuangInfo.betTimes) * int64(xjbt.betTimes) * int64(betret) * int64(nnret.nnlgc.GetServerConfig().BaseScore)
		xjwinlose, bonus := nnret.getWinLoseOrJackpot(qznn.jackpotTotalPool, xjcardret, xjbt, betret)
		qznn.UpdateGameJackpotPoolFunc(xjbt.userID, -bonus)
		qznn.sendJackpotToBox(xjbt.userID ,bonus)
		//TODO xjwinlose + bonus 送到保險箱 and 郵件
		mlog.System("[weihao] 結算 userid[%v] xjwinlose[%v] JackpotBonus[%v]", xjbt.userID, xjwinlose, bonus)

		//庄家输赢金币数
		zhuangwinlose += -1 * xjwinlose

		xjuserRet := new(netproto.QZNN_UserRet)
		xjuserRet.WinLose = proto.Int64(xjwinlose)
		xjuserRet.User = nnret.nnlgc.GetUserByUserID(xjbt.userID).GetProtoBufGameUser()
		xjuserRet.NiuNiuRet = proto.Int32(int32(xjcardret.niuRet))
		xjuserRet.NiuNiuRetNumber = proto.Int32(int32(xjcardret.number))
		xjuserRet.NiuNiuBetTimes = proto.Int32(betret)

		xjuserRet.CardGroup = xjcardret.getNetCardGroup()

		gret.UserRet = append(gret.UserRet, xjuserRet)
		if xjuser := nnret.nnlgc.GetUserByUserID(xjbt.userID); xjuser != nil {
			var flowAmount int64
			if xjwinlose > 0 {
				flowAmount = xjwinlose
			} else {
				flowAmount = -(xjwinlose)
			}
			xjuser.FlowAmount += flowAmount
			retmsg := "用戶%d, 流水%d"
			if xjuser.IsRobot() {
				retmsg = "[機器人]" + retmsg
			}
			nnret.nnlgc.GameLog.AddGameStep("用戶%d, 流水%d", xjbt.userID, flowAmount)
		}

	}

	//TODO 庄贏寫入保險箱
	mlog.System("[weihao] TODO 庄贏寫入保險箱 winlose[%v]", zhuangwinlose)

	gret.ZhuangRet.WinLose = proto.Int64(zhuangwinlose) //庄家输赢
	if zjuser := nnret.nnlgc.GetUserByUserID(zjuid); zjuser != nil {
		var flowAmount int64
		if zhuangwinlose > 0 {
			flowAmount = zhuangwinlose
		} else {
			flowAmount = -zhuangwinlose
		}
		zjuser.FlowAmount += flowAmount
		retmsg := "用戶%d, 流水%d"
		if zjuser.IsRobot() {
			retmsg = "[機器人]" + retmsg
		}
		nnret.nnlgc.GameLog.AddGameStep("用戶%d, 流水%d", zjuid, flowAmount)
	}

	return gret
}

//玩家獲得的獎金存到保險箱
func (qznn *QZNNGameLogic) sendJackpotToBox(userid int32, bonus int64) {
	mlog.System("[weihao] 玩家獲得的獎金存到保險箱")
	//GameMessageClassID_GameUpdateUserJackpotBox
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_GameUpdateUserJackpotBox)
	ust := new(netproto.UserJackpotToBox)
	ust.UserID = proto.Int32(userid)
	ust.UpdateAmount = proto.Int64(bonus)
	pmsg.MsgData = ust

	qznn.SendRPCMessageFunc(true, pmsg)
}


//計算輸贏與jackpot得獎
func (nnret *nnGameRet) getWinLoseOrJackpot(jackpotTotalPool int64, xjcardret *niuniuCardRet, xjbt *userBet, betret int32) (int64, int64) {
	xjuser := nnret.nnlgc.GetUserByUserID(xjbt.userID)
	if xjuser.IsRobot() {
		return int64(xjbt.betTimes) * int64(betret) , 0
	}

	mlog.System("[weihao] 原始總獎金[%v]", jackpotTotalPool)
	if xjcardret.niuRet >= nnCard_Niu {
		mlog.System("[weihao] 計算輸贏與jackpot得獎 niuRet[%v] 一般獎[%v]", xjcardret.niuRet, int64(xjbt.betTimes) * int64(betret) )
		return int64(xjbt.betTimes) * int64(betret) , 0
	}

	jpKind := new(JPKind)
	rndJackpotKind := util.RndInt(0,100)	//隨機獎金種類
	sumPoolRate := int(0)
	for _, v := range nnconfig.jackpotConfig.jackpotKind {
		//mlog.System("[weihao] 獎金種類[%v] 獲獎率[%v] rnd[%v]", v.kind, v.getsyspoolrate, rndJackpotKind)
		sumPoolRate += int(v.getsyspoolrate)
		if rndJackpotKind <= sumPoolRate {
			jpKind = v
			break
		}
	}
	mlog.System("[weihao] rnd[%v] 獲得獎池類[%+v]", rndJackpotKind, jpKind)

	bonusScale := int32(0)
	rndBonus := util.RndInt(0, 100)	//隨機獎金比例
	sumBonusRate := int(0)
	for _, v := range jpKind.bonusking {
		sumBonusRate += int(v.getrate)
		if rndBonus <= sumBonusRate {
			bonusScale = v.scale
			break
		}
	}
	mlog.System("[weihao] rnd[%v] 隨機獎金比例[%+v]", rndBonus, bonusScale)

	c1 := jackpotTotalPool * int64(jpKind.syspoolrate) / 100
	c2 := c1 * int64(bonusScale) / 100
	newbonus := c2 * int64(nnconfig.jackpotConfig.roomamountrate) / 100

	mlog.System("[weihao] 計算輸贏與jackpot得獎 niuRet[%v] 一般獎[%v] jackpot得獎[%v]",
		xjcardret.niuRet, int64(xjbt.betTimes) * int64(betret), newbonus )
	return int64(xjbt.betTimes) * int64(betret), newbonus
}
