package serverhandler

//服务器处理器
type GameServerHandler interface {
	Name() string             //处理器名称
	Start(ctx *GameServerCtx) //启动处理器
	Stop()                    //停止
}

//gameserver处理器组
type GameServerHandlerGroup struct {
	innerHandlers []GameServerHandler
}

//创建服务器处理器组对象
func NewGameServerHandlerGroup() *GameServerHandlerGroup {
	gsg := new(GameServerHandlerGroup)

	return gsg
}

//添加处理器
func (gsg *GameServerHandlerGroup) AddHandler(h GameServerHandler) *GameServerHandlerGroup {
	gsg.innerHandlers = append(gsg.innerHandlers, h)

	return gsg
}

func (gsg *GameServerHandlerGroup) Name() string {
	return "handler group"
}

func (gsg *GameServerHandlerGroup) Start(ctx *GameServerCtx) {
	for _, h := range gsg.innerHandlers {
		go h.Start(ctx)
	}
}

func (gsg *GameServerHandlerGroup) Stop() {
	for _, h := range gsg.innerHandlers {
		h.Stop()
	}
}

func (gsg *GameServerHandlerGroup) Range(f func(h GameServerHandler)) {
	for _, h := range gsg.innerHandlers {
		f(h)
	}
}
