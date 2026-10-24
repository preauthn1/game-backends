package gamequeue

import (
	"math/rand"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

//輸贏對戰,服务器游戏撮合基类
type ServerWinLoseGameQueueBase struct {
	evtDispatcher       *event.EventDispatcher
	actionController    gamelogic.ActionController
	Configs             *config.ConfigGroup
	gameendaction       int  //游戏结束后行为
	againstSameIP       bool //拒绝同IP
	againstLastGameUser bool //拒绝曾经游戏过的用户
	randJumpTable       int32
	lessthanuser        int32
}

//輸贏對戰,创建服务器排队对象基类
func NewServerWinLoseGameQueueBase(evtDispatcher *event.EventDispatcher, actionController gamelogic.ActionController, cfgs *config.ConfigGroup) *ServerWinLoseGameQueueBase {
	svrqueue := new(ServerWinLoseGameQueueBase)
	svrqueue.evtDispatcher = evtDispatcher
	svrqueue.actionController = actionController
	svrqueue.Configs = cfgs

	cfg := svrqueue.Configs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)

	svrqueue.gameendaction = cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "gameendaction")
	svrqueue.againstSameIP = (cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "againstsameip") == 1)
	svrqueue.againstLastGameUser = (cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "againstlastgame") == 1)
	svrqueue.randJumpTable = cfg.GetConfigInt32(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "randjumptable")
	svrqueue.lessthanuser = cfg.GetConfigInt32(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "lessthanuser")

	return svrqueue
}

func (svrq *ServerWinLoseGameQueueBase) Start() {
	svrq.registerMsgHandler()
}

//輸贏對戰
func (svrq *ServerWinLoseGameQueueBase) registerMsgHandler() {
	h := event.NewCommonHandler("gamematch", func(evt *event.Event, evtret *event.EventRet) bool {
		urs := svrq.actionController.GetQueueUser()
		userCount := svrq.actionController.TotalUserCount()
		if len(urs) == 0 {
			return true
		}
		//排除的IP
		exIP := ""
		var exGameTableNo int32 = 0
		var islessthanuser bool = true
		for _, ur := range urs {
			//如果用戶有被指定進入哪一個桌子
			if tno, sno := svrq.actionController.GetNextTableSeat(ur); tno != gamelogic.FreeTableNo || sno != gamelogic.FreeSeatNo {
				svrq.actionController.UserSit(ur, tno, sno)
				continue
			}
			if userCount > svrq.lessthanuser {
				islessthanuser = false
				//隨機跳桌
				randNumber := int32(1)
				if svrq.randJumpTable != 0 {
					randNumber = svrq.randJumpTable
				}

				rand.Seed(time.Now().UnixNano())
				exGameTableNo = rand.Int31n(randNumber)
				if svrq.againstSameIP {
					exIP = ur.ConnInfo.IPAddress
				}

				if svrq.againstLastGameUser {
					exGameTableNo = ur.LastTableNo
				}
			}

			tno, sno := svrq.actionController.GetWinLoseFreeSeat(ur.LastTableNo, exIP, exGameTableNo, ur.LastWinLose, islessthanuser)
			example.invalid("为用户%v分配座位.参数,%d,%s,%d,ret:%d,%d", ur, ur.LastTableNo, exIP, exGameTableNo, tno, sno)

			//如果没有空闲的座位
			if tno == gamelogic.FreeTableNo || sno == gamelogic.FreeSeatNo {
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

func (svrq *ServerWinLoseGameQueueBase) publishGameMatch() {
	evt := NewGameMatchEvent()
	svrq.evtDispatcher.OnAsyncEvent(evt, nil)
}

func (svrq *ServerWinLoseGameQueueBase) UserQueue(user *gamelogic.GameUser) {

}

func (svrq *ServerWinLoseGameQueueBase) UserGameEnd(user *gamelogic.GameUser) {

	if svrq.gameendaction == GameEndAction_None {

	} else if svrq.gameendaction == GameEndAction_Ready {
		svrq.actionController.UserReady(user)
	}
}

func (svrq *ServerWinLoseGameQueueBase) GetServerConfig() *gamelogic.GameServerConfig {
	return gamelogic.GetGameServerConfig(svrq.Configs.GetConfig(gamelogic.CONFIGFILENAME_SERVERINFO), svrq.Configs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER))
}
