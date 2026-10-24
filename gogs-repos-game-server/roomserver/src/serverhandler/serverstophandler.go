package serverhandler

import (
	"event"
	"gamelogic"
	gshandler "gameserver/serverhandler"
	"mlog"
	"module/logicmodule"
	"msg"
	"netproto"
	"player"
	"time"
)

import (
	"example.invalid/golang/protobuf/proto"
	"roomutil"
)

const (
	TimerName_ServerStop = "roomserver_serverhandler_serverstophandler_ticker"
)

//停服服务器处理器
type ServerStopHandler struct {
	serverStatus        logicmodule.ServerStatusProvider
	kickUserTimeSpan    int32
	cgx                 *gshandler.GameServerCtx
	svrStopNotifyTicker *event.TimerFunc
	room *gamelogic.Room
}

//创建停服处理器对象
func NewServerStopHandler(serverStatus logicmodule.ServerStatusProvider, kickUserTimeSpan int32, room *gamelogic.Room) *ServerStopHandler {
	sth := new(ServerStopHandler)
	sth.kickUserTimeSpan = kickUserTimeSpan
	sth.serverStatus = serverStatus
	sth.room = room

	return sth
}

func (stm *ServerStopHandler) Name() string {
	return "gameserverstop"
}

func (stm *ServerStopHandler) Stop() {
	if stm.svrStopNotifyTicker != nil {
		stm.svrStopNotifyTicker.Stop()
	}
}

func (stm *ServerStopHandler) Start(ctx *gshandler.GameServerCtx) {
	stm.cgx = ctx
	stm.svrStopNotifyTicker = ctx.TimerFuncManager.NewNamedTicker(stm.kickUserTimeSpan, func() {
		if stm.serverStatus.IsServerRuning() {
			return
		}

		//广播停服消息
		//stm.boardCastServerStopMsg(ctx)

		//example.invalid("停服检查空闲用户逻辑开始运行...")
		freeusers := make([]*player.Player, 0)
		ctx.PlayerManager.Range(func(pr *player.Player) {
			if pr.IsGuest() {
				freeusers = append(freeusers, pr)
			} else {
				ur := pr.UserData.(*gamelogic.GameUser)
				if ur.IsFree() || ur.IsSit() || ur.IsReady() || ur.IsQueue() {
					freeusers = append(freeusers, pr)
				}
			}
		})

		for _, pr := range freeusers {
			stm.kickUser(ctx, "", pr.Conn.ConnID())
		}

	}, TimerName_ServerStop)

}

func (stm *ServerStopHandler) kickUser(ctx *gshandler.GameServerCtx, str string, connid string) {
	mlog.Warn("停服检查逻辑,%s 断开客户端%s", str, connid)

	var m *msg.Message
	if stm.room.NewGameServerAddr != "" && stm.room.NewGameServerAddr != "-" {
		//m, _ = roomutil.GetTipMessageWithParam(stm.room.NewGameServerAddr , roomutil.MsgTypeID_RoomKick, 6)
		m, _ = roomutil.GetTipMessageWithParam("-", roomutil.MsgTypeID_RoomKick, 4)
	}else {
		m, _ = roomutil.GetTipMessageWithParam("-", roomutil.MsgTypeID_RoomKick, 4)
	}


	ctx.MsgServer.SendMessageByConnID(connid, m)

	go func() {
		time.Sleep(time.Duration(1) * time.Second)

		ctx.MsgServer.CloseConnByConnID(connid)
	}()
}

//广播停服消息
func (stm *ServerStopHandler) boardCastServerStopMsg(ctx *gshandler.GameServerCtx) {
	tipmsg := new(msg.Message)
	tipmsg.BClassID = int32(netproto.MessageBClassID_Common)
	tipmsg.SClassID = int32(netproto.PlatformCommonClassID_TipMessageID)

	tipdata := new(netproto.TipMessage)
	tipdata.MsgType = proto.Int32(3)
	tipdata.Message = proto.String(stm.serverStatus.GetStopServerTipMsg())
	tipmsg.MsgData = tipdata

	ctx.MsgServer.SendAllMessage(tipmsg)
}
