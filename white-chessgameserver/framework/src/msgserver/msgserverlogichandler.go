package msgserver

import (
	"msg"
	"network"
)

//服务器处逻辑理器
type MsgServerLogicHandler interface {
	//客户端连接
	OnClientConnect(clt *network.ConnInfo)

	//客户端断开
	OnClientDisconnect(clt *network.ConnInfo)

	//收到数据
	OnClientData(data *network.ConnData)

	//收到消息
	OnClientMessage(clt *network.ConnInfo, msg *msg.Message)

	//断开客户端接口
	CloseClientFunc(f func(clt *network.ConnInfo))
}

//服务器逻辑处理器组
type MsgServerLogicGroup struct {
	innerLogicHandlers []MsgServerLogicHandler
	closCltFunc        func(clt *network.ConnInfo)
}

//创建服务器组
func NewMsgServerLogicGroup() *MsgServerLogicGroup {
	return new(MsgServerLogicGroup)
}

//注册逻辑处理器
func (slg *MsgServerLogicGroup) RegisterHandler(h MsgServerLogicHandler) *MsgServerLogicGroup {
	slg.innerLogicHandlers = append(slg.innerLogicHandlers, h)
	h.CloseClientFunc(slg.closCltFunc)
	return slg
}

func (slg *MsgServerLogicGroup) OnClientConnect(clt *network.ConnInfo) {
	for _, lg := range slg.innerLogicHandlers {
		lg.OnClientConnect(clt)
	}
}

func (slg *MsgServerLogicGroup) OnClientDisconnect(clt *network.ConnInfo) {
	for _, lg := range slg.innerLogicHandlers {
		lg.OnClientDisconnect(clt)
	}
}

func (slg *MsgServerLogicGroup) OnClientData(data *network.ConnData) {
	for _, lg := range slg.innerLogicHandlers {
		lg.OnClientData(data)
	}
}

func (slg *MsgServerLogicGroup) OnClientMessage(clt *network.ConnInfo, cmsg *msg.Message) {
	for _, lg := range slg.innerLogicHandlers {
		lg.OnClientMessage(clt, cmsg)
	}
}

func (slg *MsgServerLogicGroup) CloseClientFunc(f func(clt *network.ConnInfo)) {
	slg.closCltFunc = f
}
