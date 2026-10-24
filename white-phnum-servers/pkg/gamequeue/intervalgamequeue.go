package gamequeue

import (
	"time"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//定时撮合
type IntervalMatchGameQueue struct {
	scope util.IntScoper //随机撮合时间范围
	*ServerGameQueueBase
	stopSig chan bool
}

//创建定时撮合逻辑
func NewIntervalMatchGameQueue(evtDispatcher *event.EventDispatcher, actionController gamelogic.ActionController, cfgs *config.ConfigGroup) *IntervalMatchGameQueue {
	intervalGameQueue := new(IntervalMatchGameQueue)
	intervalGameQueue.ServerGameQueueBase = NewServerGameQueueBase(evtDispatcher, actionController, cfgs)

	cfg := intervalGameQueue.Configs.GetConfig(gamelogic.CONFIGFILENAME_GAMESERVER)

	minwaittime := int32(cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "gamematchwaitmintime"))
	maxwaittime := int32(cfg.GetConfigInt(gamelogic.CONFIGSECTIONNAME_GAMEQUEUE, "gamematchwaitmaxtime"))
	intervalGameQueue.scope = util.NewIntScope(minwaittime, maxwaittime)
	intervalGameQueue.stopSig = make(chan bool, 1)

	return intervalGameQueue
}

func (gq *IntervalMatchGameQueue) Start() {
	gq.ServerGameQueueBase.Start()

	go func() {
		defer util.PanicErrStack()

		example.invalid("定时撮合逻辑线程启动")
		for {
			sleeptime := gq.scope.GetRandomValue()
			time.Sleep(time.Duration(sleeptime) * time.Second)

			select {
			case <-gq.stopSig:
				break
			default:
			}

			gq.ServerGameQueueBase.publishGameMatch()
		}

		mlog.Error("定时撮合逻辑线程退出")
	}()
}

func (gq *IntervalMatchGameQueue) Stop() {
	gq.stopSig <- true
}
