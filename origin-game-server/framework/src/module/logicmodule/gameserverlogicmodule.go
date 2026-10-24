package logicmodule

import (
	"conf"
	"event"
	"module"
	"module/heartbeat"
	"msg"
	"msgserver"
	"net/http"
	"netproto"
	"network"
	"player"
	"rpcmsg"
	"util"
)

//http服务器配置
type HttpServerConfig struct {
	ServerAddr string //服务器地址
	VCode      string //http校验码
}

//服务器逻辑模块
type GameServerLogicModule struct {
	MsgHandler       *module.HookMsgHandlerGroup
	RPCMsgHandler    *RPCHookHandlerGroup
	NotifyMsgHandler *NotifyHookHandlerGroup
	HttpMsgHandler   *HttpMsgHookHandlerGroup
	PlayerManager    *player.PlayerManager
	DBMsgQueue       *rpcmsg.RPCMsgQueue
	ModuleCtx        *module.MouduleCtx
	MsgServer        *msgserver.MsgServer
	heartBeater      *heartbeat.HeartBeatManager
	ServerStatus     ServerStatusProvider
}

//创建服务器逻辑模块
func NewGameServerLogicModule() *GameServerLogicModule {
	sm := new(GameServerLogicModule)
	//rpc 消息处理器
	sm.RPCMsgHandler = NewRPCHookHandlerGroup()
	//消息处理器
	sm.MsgHandler = module.NewHookMsgHandlerGroup()
	//通知消息处理器
	sm.NotifyMsgHandler = NewNotifyHookHandlerGroup()
	//http消息处理器
	sm.HttpMsgHandler = NewHttpMsgHookHandlerGroup()

	//服务器状态
	ssnh := NewServerStopNotifyHandler()
	sm.ServerStatus = ssnh

	return sm
}

func (sm *GameServerLogicModule) Init(ctx *module.MouduleCtx, playerManager *player.PlayerManager, dbmsgQueue *rpcmsg.RPCMsgQueue, httpCfg *HttpServerConfig) {
	sm.ModuleCtx = ctx

	sm.PlayerManager = playerManager
	sm.DBMsgQueue = dbmsgQueue
	sm.MsgServer = ctx.Server

	//注册公共消息处理器
	sm.registerCommonMsgHandler()
	//注册公共通知处理器
	sm.registerCommNotifyHandler()
	//注册公共的http消息处理器
	sm.registerCommonHttpMsgHandler(httpCfg)

	//注册消息解析器
	sm.registerMsgParser(ctx)
	//注册消息事件处理器
	sm.registerMsgEvtHandler(ctx)
	//注册RPC消息事件处理器
	sm.registerRPCMsgEventHandler(ctx)
	//注册Notify消息事件处理器
	sm.registerNotifyMsgEventHandler(ctx)
	//注册异步调用事件处理器
	sm.registerAsyncCallBackEventHandler(ctx)
	//注册http消息事件处理器
	sm.registerHttpMsgEventHandler(ctx)

	//处理RPC 消息
	go sm.rpcCallBackMsgDispatcher(ctx)

}

//注册公共的消息处理器
func (sm *GameServerLogicModule) registerCommonMsgHandler() {
	sm.MsgHandler.AddHandler(heartbeat.NewReqBeatConfigMsgHandler()) //请求心跳包配置

	sm.heartBeater = heartbeat.NewHeartBeatManager(sm.MsgServer)
	sm.MsgHandler.AddHandler(sm.heartBeater) //心跳包
}

//注册公共的通知处理器
func (sm *GameServerLogicModule) registerCommNotifyHandler() {
	sm.NotifyMsgHandler.AddHandler(sm.ServerStatus.(*ServerStopNotifyHandler))
}

//注册公共的http消息处理器
func (sm *GameServerLogicModule) registerCommonHttpMsgHandler(httpCfg *HttpServerConfig) {
	if httpCfg != nil && httpCfg.ServerAddr != "" {
		//http校验码
		sm.HttpMsgHandler.AddHandler(NewValidateHttpMsgHookHandler(httpCfg.VCode))
	}
}

func (sm *GameServerLogicModule) registerMsgParser(ctx *module.MouduleCtx) {
	ctx.RegisterMsgParser(msg.AllMsgBClassID, msg.AllMsgSClassID, msg.NewProtoMsgParser())
}

func (sm *GameServerLogicModule) registerMsgEvtHandler(ctx *module.MouduleCtx) {
	ctx.OnMessage(func(clt *network.ConnInfo, m *msg.Message) bool {
		//example.invalid("client %s msg:%d,%d", clt, m.BClassID, m.SClassID)

		//检查协议内容
		if !sm.checkClientMsgProto(m) {
			ctx.Error("客户端发送消息%v,但不在允许的通讯协议中.关闭客户端连接", m)
			ctx.Server.CloseConn(clt)
			return false
		}

		retmsgs := sm.MsgHandler.OnMessage(ctx, clt, m)

		if retmsgs != nil {
			for _, retmsg := range retmsgs {
				if retmsg != nil {
					ctx.Server.SendMessage(clt, retmsg)
				}
			}
		}

		return true
	})
	ctx.OnDisConnect(func(clt *network.ConnInfo) bool {
		//如果是大厅或者房间的客户端连接
		if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) || clt.IsServerConn(conf.TCPSERVERNAME_ROOM_CLIENT) {
			sm.heartBeater.OnClientDisconnect(clt.ConnID())
		}
		return true
	})
	ctx.OnConnect(func(clt *network.ConnInfo) bool {
		if clt.IsServerConn(conf.TCPSERVERNAME_HALL_CLIENT) || clt.IsServerConn(conf.TCPSERVERNAME_ROOM_CLIENT) {
			sm.heartBeater.OnClientConnect(clt.ConnID())
		}

		return true
	})
}

func (sm *GameServerLogicModule) registerRPCMsgEventHandler(ctx *module.MouduleCtx) {
	h := event.NewCommonHandler("rpcmsg", func(evt *event.Event, evtret *event.EventRet) bool {
		m := msgserver.GetRPCMsg(evt.Args)

		sm.RPCMsgHandler.OnRPCMessage(m, ctx)

		return true
	}, msgserver.EVT_RPCMESSAGE)

	ctx.EvtDispatcher.PushHandler(h)
}

func (sm *GameServerLogicModule) registerNotifyMsgEventHandler(ctx *module.MouduleCtx) {
	h := event.NewCommonHandler("notifymsg", func(evt *event.Event, evtret *event.EventRet) bool {
		m := msgserver.GetNotifyMsg(evt.Args)

		sm.NotifyMsgHandler.OnNotifyMessage(m, ctx, sm.PlayerManager)

		return true
	}, msgserver.EVT_NOTIFYMESSAGE)

	ctx.EvtDispatcher.PushHandler(h)
}

func (sm *GameServerLogicModule) registerAsyncCallBackEventHandler(ctx *module.MouduleCtx) {
	h := event.NewCommonHandler("asynccall", func(evt *event.Event, evtret *event.EventRet) bool {
		callbackFunc := msgserver.GetAsyncEventCallBackFunc(evt.Args)

		if callbackFunc != nil {
			util.SafeCall(callbackFunc)
		}

		return true
	}, msgserver.EVT_ASYNCCALL)

	ctx.EvtDispatcher.PushHandler(h)
}

func (sm *GameServerLogicModule) registerHttpMsgEventHandler(ctx *module.MouduleCtx) {
	h := event.NewCommonHandler("httpmsg", func(evt *event.Event, evtret *event.EventRet) bool {
		httpMsgData := msgserver.GetHttpMsgData(evt.Args)

		seamphore := httpMsgData[msgserver.HTTPMSGEVTDATA_SEMAPHORE].(util.Semaphore)
		req := httpMsgData[msgserver.HTTPMSGEVTDATA_HTTPREQUEST].(*http.Request)
		w := httpMsgData[msgserver.HTTPMSGEVTDATA_HTTPRESPONSEWRITER].(http.ResponseWriter)

		sm.HttpMsgHandler.OnHttpMsg(seamphore, w, req)

		return true
	}, msgserver.EVT_HTTPMSG)

	ctx.EvtDispatcher.PushHandler(h)
}

func (sm *GameServerLogicModule) rpcCallBackMsgDispatcher(ctx *module.MouduleCtx) {
	defer util.PanicErrStack()

	for {
		m := <-sm.DBMsgQueue.OnRPCMesasge
		ctx.Server.OnRPCMessageEvent(m)
	}
}

//检查客户端协议号
func (sm *GameServerLogicModule) checkClientMsgProto(m *msg.Message) bool {
	//允许的一级协议号
	allowBClsids := []int32{int32(netproto.MessageBClassID_Common), int32(netproto.MessageBClassID_Hall), int32(netproto.MessageBClassID_Game), int32(netproto.MessageBClassID_GameRoom), int32(netproto.MessageBClassID_ServerMgm)}
	for _, aid := range allowBClsids {
		if m.BClassID == aid {
			return true
		}
	}

	return false
}
