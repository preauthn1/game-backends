package serverhandler

import (
	"conf"
	"event"
	gshandler "gameserver/serverhandler"
	"mlog"
	"module/logicmodule"
	"msg"
	"netproto"
	"network"
	"util"
)

import proto "example.invalid/golang/protobuf/proto"

const (
	TimerName_ServerStop = "hallserver_serverhandler_serverstophandler_ticker"
)

//服务器状态管理器
type ServerStopHandler struct {
	statusProvider  logicmodule.ServerStatusProvider
	svrStopTicker   *event.TimerFunc
	sendMsgTimeSpan int
}

//创建服务器状态管理器
func NewServerStopHandler(statusProvider logicmodule.ServerStatusProvider, timespan int) *ServerStopHandler {
	stm := new(ServerStopHandler)
	stm.statusProvider = statusProvider
	stm.sendMsgTimeSpan = timespan

	return stm
}

func (stm *ServerStopHandler) Name() string {
	return "hallserverstop"
}

func (stm *ServerStopHandler) Stop() {
	if stm.svrStopTicker != nil {
		stm.svrStopTicker.Stop()
	}
}

func (stm *ServerStopHandler) Start(ctx *gshandler.GameServerCtx) {
	defer util.PanicErrStack()

	sendTipMsgFunc := func() {
		if !stm.statusProvider.IsServerRuning() {
			example.invalid("广播服务器停服消息,%s", stm.statusProvider.GetStopServerTipMsg())

			tipmsg := new(msg.Message)
			tipmsg.BClassID = int32(netproto.MessageBClassID_Common)
			tipmsg.SClassID = int32(netproto.PlatformCommonClassID_TipMessageID)

			tipdata := new(netproto.TipMessage)
			tipdata.MsgType = proto.Int32(3)
			tipdata.Message = proto.String(stm.statusProvider.GetStopServerTipMsg())
			tipmsg.MsgData = tipdata

			ctx.MsgServer.RangeConn(func(clt *network.ConnInfo) {
				if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) && !stm.statusProvider.IsWhiteIP(clt.IPAddress) {
					ctx.MsgServer.SendMessage(clt, tipmsg)
				}
			})
		}

		//如果服务器关闭
		if stm.statusProvider.GetServerStatus() == conf.ServerStatus_Close {
			ctx.MsgServer.RangeConn(func(clt *network.ConnInfo) {
				if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) && !stm.statusProvider.IsWhiteIP(clt.IPAddress) {
					go func() {
						ctx.MsgServer.CloseConn(clt)
					}()
				}
			})
		}
	}

	stm.svrStopTicker = ctx.TimerFuncManager.NewNamedTicker(int32(stm.sendMsgTimeSpan), sendTipMsgFunc, TimerName_ServerStop)
}
