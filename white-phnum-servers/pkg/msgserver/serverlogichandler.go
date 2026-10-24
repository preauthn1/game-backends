package msgserver

//服务器处逻辑理器
type ServerLogicHandler interface {
	Init(server *MsgServer) //初始化服务器
	Start()                 //启动服务器
	Stop()                  //停止服务器
	Name() string           //名称
}

//服务器逻辑处理器组
type ServerLogicGroup struct {
	innerLogicHandlers []ServerLogicHandler
}

//创建服务器组
func NewServerLogicGroup() *ServerLogicGroup {
	return new(ServerLogicGroup)
}

//注册逻辑处理器
func (slg *ServerLogicGroup) RegisterHandler(h ServerLogicHandler) *ServerLogicGroup {
	slg.innerLogicHandlers = append(slg.innerLogicHandlers, h)

	return slg
}

//初始化
func (slg *ServerLogicGroup) Init(server *MsgServer) {
	for _, h := range slg.innerLogicHandlers {
		h.Init(server)
	}
}

//启动
func (slg *ServerLogicGroup) Start() {
	for _, h := range slg.innerLogicHandlers {
		h.Start()
	}
}

//停止
func (slg *ServerLogicGroup) Stop() {
	for _, h := range slg.innerLogicHandlers {
		h.Stop()
	}
}

//名称
func (slg *ServerLogicGroup) Name() string {
	return "logic group"
}
