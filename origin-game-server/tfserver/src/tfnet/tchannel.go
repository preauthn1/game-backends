package tfnet

//转发通道接口定义
type TFChannel interface {
	//收到客户端消息
	OnMessage(conninfo *ConnInfo, data []byte)

	//连接建立
	OnConnected(conninfo *ConnInfo)

	//服务端连接关闭
	OnServerClose(conninfo *ConnInfo)

	//客户端连接关闭
	OnClientClose(conninfo *ConnInfo)
}

//转发通道基类
type TFChannelBase struct {
}

func (tfcb *TFChannelBase) OnMessage(conninfo *ConnInfo, data []byte) {

}

func (tfcb *TFChannelBase) OnConnected(conninfo *ConnInfo) {

}

func (tfcb *TFChannelBase) OnServerClose(conninfo *ConnInfo) {

}

func (tfcb *TFChannelBase) OnClientClose(conninfo *ConnInfo) {

}

//转发通道组
type TFChannelGroup struct {
	channels []TFChannel
}

func NewTFChannelGroup() *TFChannelGroup {
	cg := new(TFChannelGroup)
	cg.channels = make([]TFChannel, 0)

	return cg
}

func (cg *TFChannelGroup) OnMessage(conninfo *ConnInfo, data []byte) {
	for _, c := range cg.channels {
		c.OnMessage(conninfo, data)
	}
}

func (cg *TFChannelGroup) OnConnected(conninfo *ConnInfo) {
	for _, c := range cg.channels {
		c.OnConnected(conninfo)
	}
}

func (cg *TFChannelGroup) OnServerClose(conninfo *ConnInfo) {
	for _, c := range cg.channels {
		c.OnServerClose(conninfo)
	}
}

func (cg *TFChannelGroup) OnClientClose(conninfo *ConnInfo) {
	for _, c := range cg.channels {
		c.OnClientClose(conninfo)
	}
}

func (cg *TFChannelGroup) AddChannel(ch TFChannel) {
	cg.channels = append(cg.channels, ch)
}

var ChannelCtx *TFChannelGroup

func init() {
	ChannelCtx = NewTFChannelGroup()
}
