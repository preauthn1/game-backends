package network

//客户端消息通道
type ClientEventChan struct {
	DataSig         chan []byte    //数据消息通道
	MsgSig          chan []byte    //完整数据包消息通道
	ConnectedSig    chan *ConnInfo //连接成功消息通道
	DisconnectedSig chan *ConnInfo //连接断开消息通道
}

//新建客户端消息通道
func NewClientEventChan(sigSize int) *ClientEventChan {
	evt := new(ClientEventChan)

	evt.DataSig = make(chan []byte, sigSize)
	evt.MsgSig = make(chan []byte, sigSize)
	evt.ConnectedSig = make(chan *ConnInfo, sigSize)
	evt.DisconnectedSig = make(chan *ConnInfo, sigSize)

	return evt
}

//服务器网络事件通道
type ServerNetEventChan struct {
	ConnectSig    chan *ConnInfo //客户端连接
	DisConnectSig chan *ConnInfo //客户端断开
	DataSig       chan *ConnData //收到数据
	MsgSig        chan *ConnData //收到完整数据包
}

func NewNetEventChan(chansize int) *ServerNetEventChan {
	evtchan := new(ServerNetEventChan)
	evtchan.ConnectSig = make(chan *ConnInfo, chansize)
	evtchan.DisConnectSig = make(chan *ConnInfo, chansize)
	evtchan.DataSig = make(chan *ConnData, chansize)
	evtchan.MsgSig = make(chan *ConnData, chansize)

	return evtchan
}
