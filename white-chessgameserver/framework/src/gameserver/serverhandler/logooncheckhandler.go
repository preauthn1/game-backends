package serverhandler

import (
	"event"
	"mlog"
	"player"
	"time"
)

const (
	TimerName_LogonCheck = "gameserver_serverhandler_logoncheck"
)

//用户登录超时处理器
type LogonCheckHandler struct {
	timespan         int
	cgx              *GameServerCtx
	logonCheckTicker *event.TimerFunc
}

//创建登录超时检查处理器
func NewLogonCheckHandler(timespan int) *LogonCheckHandler {
	lh := new(LogonCheckHandler)
	lh.timespan = timespan

	return lh
}

func (lh *LogonCheckHandler) Name() string {
	return "LogonCheck"
}

func (lh *LogonCheckHandler) Start(cgx *GameServerCtx) {
	lh.cgx = cgx

	lh.logonCheckTicker = cgx.TimerFuncManager.NewNamedTicker(int32(lh.timespan), func() {
		kickguestusers := make([]*player.Player, 0)

		guestusers := cgx.PlayerManager.UnLogonPlayer()
		for _, u := range guestusers {
			if int(time.Now().Sub(u.Conn.ConnTime).Seconds()) >= lh.timespan {
				kickguestusers = append(kickguestusers, u)
			}
		}

		for _, ku := range kickguestusers {
			example.invalid("客户端%v超时未登录，断开连接", ku.Conn)
			cgx.MsgServer.CloseConn(ku.Conn)
		}

	}, TimerName_LogonCheck)

}

func (lh *LogonCheckHandler) Stop() {
	if lh.logonCheckTicker != nil {
		lh.logonCheckTicker.Stop()
	}
}
