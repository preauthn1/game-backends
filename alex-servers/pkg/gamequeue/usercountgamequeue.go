package gamequeue

import (
	"time"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//基于排队人数的撮合逻辑
type UserCountGameQueue struct {
	queueUserCount int //排队人数
	queueTimeout   int
	*ServerGameQueueBase
	stopSig chan bool
}

//创建用户人数排队撮合逻辑
func NewUserCountGameQueue(evtDispatcher *event.EventDispatcher, actionController gamelogic.ActionController, cfgs *config.ConfigGroup) *UserCountGameQueue {
	ugGameQueue := new(UserCountGameQueue)
	ugGameQueue.ServerGameQueueBase = NewServerGameQueueBase(evtDispatcher, actionController, cfgs)

	cfg := ugGameQueue.Configs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)
	queueUrCount := cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "queueusercount")
	queueTimeout := cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "queuetimeout")

	ugGameQueue.queueUserCount = queueUrCount
	ugGameQueue.queueTimeout = queueTimeout

	ugGameQueue.stopSig = make(chan bool, 1)

	return ugGameQueue
}

func (gq *UserCountGameQueue) Start() {
	gq.ServerGameQueueBase.Start()

	go func() {
		defer util.PanicErrStack()

		example.invalid("人数撮合逻辑超时线程启动,人数%d,超时时间%d,同ip:%v,同游戏:%v", gq.queueUserCount, gq.queueTimeout, gq.againstSameIP, gq.againstLastGameUser)
		for {
			time.Sleep(time.Duration(gq.queueTimeout) * time.Second)

			select {
			case <-gq.stopSig:
				break
			default:
			}

			//example.invalid("固定人数撮合逻辑超时触发")
			gq.ServerGameQueueBase.publishGameMatch()

		}

		mlog.Error("人数撮合逻辑超时线程退出")
	}()
}

func (gq *UserCountGameQueue) Stop() {
	gq.stopSig <- true
}

func (gq *UserCountGameQueue) UserQueue(user *gamelogic.GameUser) {
	//如果排队人数达到设定值
	currentQueueUserCount := len(gq.actionController.GetQueueUser())

	if currentQueueUserCount >= gq.queueUserCount {
		//example.invalid("固定人数撮合逻辑排队人数达到%d触发", currentQueueUserCount)
		gq.ServerGameQueueBase.publishGameMatch()
	}
}
