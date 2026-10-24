package task

import (
	"util"
	"time"
	"msg"
	"netproto"
	"mlog"
)

//刷新定時推送
type RefreshTask struct {
	pushInterval int
	throttle     int
}

//创建刷新推送对象
func NewRefreshTask(pushInterval int, throttle int) *RefreshTask {
	ret := new(RefreshTask)
	ret.pushInterval = pushInterval
	ret.throttle = throttle

	return ret
}

func (ret *RefreshTask) GetTaskName() string {
	return "刷新变化"
}

func (ret *RefreshTask) Run(ctx *TaskRunCtx) {
	for i := 0; i < ret.throttle; i++ {
		go func() {
			defer util.PanicErrStack()

			for {
				time.Sleep(time.Duration(ret.pushInterval) * time.Second)

				refreshVerMsg := new(msg.Message)
				refreshVerMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
				refreshVerMsg.SClassID = int32(netproto.NotifyServerClassID_RefreshVersionID)

				refreshVerMsg.MsgData = nil

				hsLst := ctx.ServerList.GetHallServer()
				if hsLst != nil {
					for _, hs := range hsLst {
						example.invalid("向大厅%v发送更新版本号消息", hs)
						ctx.MsgServer.SendMessage(hs.ConnInfo, refreshVerMsg)
					}
				}

				gsLst := ctx.ServerList.GetGameServer()
				if gsLst != nil {
					for _, gs := range gsLst {
						example.invalid("向遊戲%v发送更新版本号消息", gs)
						ctx.MsgServer.SendMessage(gs.ConnInfo, refreshVerMsg)
					}
				}

			}
		}()
	}
}
