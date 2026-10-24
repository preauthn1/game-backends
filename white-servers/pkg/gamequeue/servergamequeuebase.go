package gamequeue

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/mylog"
)

const (
	GameEndAction_None  = 0 //游戏结束后动作,无
	GameEndAction_Ready = 1 //游戏结束后自动准备
)

//服务器游戏撮合基类
type ServerGameQueueBase struct {
	evtDispatcher       *event.EventDispatcher
	actionController    gamelogic.ActionController
	Configs             *config.ConfigGroup
	gameendaction       int  //游戏结束后行为
	againstSameIP       bool //拒绝同IP
	againstLastGameUser bool //拒绝曾经游戏过的用户
}

//创建服务器排队对象基类
func NewServerGameQueueBase(evtDispatcher *event.EventDispatcher, actionController gamelogic.ActionController, cfgs *config.ConfigGroup) *ServerGameQueueBase {
	svrqueue := new(ServerGameQueueBase)
	svrqueue.evtDispatcher = evtDispatcher
	svrqueue.actionController = actionController
	svrqueue.Configs = cfgs

	cfg := svrqueue.Configs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)

	svrqueue.gameendaction = cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "gameendaction")
	svrqueue.againstSameIP = (cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "againstsameip") == 1)
	svrqueue.againstLastGameUser = (cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "againstlastgame") == 1)

	return svrqueue
}

func (svrq *ServerGameQueueBase) Start() {
	svrq.registerMsgHandler()
}

func (svrq *ServerGameQueueBase) registerMsgHandler() {
	h := event.NewCommonHandler("gamematch", func(evt *event.Event, evtret *event.EventRet) bool {
		urs := svrq.actionController.GetQueueUser()
		if len(urs) == 0 {
			return true
		}

		for _, ur := range urs {

			//排除的IP
			exIP := ""
			//排除的桌号
			var exGameTableNo int32 = gamelogic.FreeTableNo
			if svrq.againstSameIP {
				exIP = ur.ConnInfo.IPAddress
			}

			if svrq.againstLastGameUser {
				exGameTableNo = ur.LastTableNo
			}

			tno, sno := svrq.actionController.GetFreeSeatNew(ur, exIP, exGameTableNo)
			//mlog.Debug("为用户%v分配座位.参数,%d,%s,%d,ret:%d,%d", ur, ur.LastTableNo, exIP, exGameTableNo, tno, sno)
			msglog := fmt.Sprintf("userid[%v]玩家配桌到 tableno[%v] seatno[%v]", ur.UserID, tno, sno)
			mylog.MongoLog(ur.UserID, 0, tno, msglog)

			//如果没有空闲的座位
			if tno == gamelogic.FreeTableNo || sno == gamelogic.FreeSeatNo {
				msglog := fmt.Sprintf("userid[%v]玩家配桌到 没有空闲的座位", ur.UserID, tno, sno)
				mylog.MongoLog(ur.UserID, 0, tno, msglog)
				mlog.System(msglog)
				return true
			}

			if tno != gamelogic.FreeTableNo && sno != gamelogic.FreeSeatNo {
				//坐下
				svrq.actionController.UserSit(ur, tno, sno)

				//如果10秒后还未准备,则自动准备

			}
		}

		return true
	}, EventName_GameMatch)

	svrq.evtDispatcher.PushHandler(h)
}

func (svrq *ServerGameQueueBase) publishGameMatch() {
	evt := NewGameMatchEvent()
	svrq.evtDispatcher.OnAsyncEvent(evt, nil)
}

func (svrq *ServerGameQueueBase) UserQueue(user *gamelogic.GameUser) {

}

func (svrq *ServerGameQueueBase) UserGameEnd(user *gamelogic.GameUser) {

	if svrq.gameendaction == GameEndAction_None {

	} else if svrq.gameendaction == GameEndAction_Ready {
		svrq.actionController.UserReady(user)
	}
}

func (svrq *ServerGameQueueBase) GetServerConfig() *gamelogic.GameServerConfig {
	return gamelogic.GetGameServerConfig(svrq.Configs.GetConfig(gamelogic.CONFIGFILENAME_SERVERINFO), svrq.Configs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER))
}
