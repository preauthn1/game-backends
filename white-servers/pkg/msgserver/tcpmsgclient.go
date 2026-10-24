package msgserver

import (
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//tcp消息客户端
type TcpMsgClient struct {
	tcpclient network.ConnClient
	msgparser msg.MsgParser
	clt       *network.ConnInfo
	stopSig   chan bool
}

//消息事件通道
type MsgEvtChan struct {
	DataSig         chan []byte            //数据消息通道
	MsgSig          chan *msg.Message      //完整数据包消息通道
	ConnectedSig    chan *network.ConnInfo //连接成功消息通道
	DisconnectedSig chan *network.ConnInfo //连接断开消息通道
}

//创建消息事件通道对象
func NewMsgEvtChan(size int) *MsgEvtChan {
	evtchan := new(MsgEvtChan)
	evtchan.DataSig = make(chan []byte, size)
	evtchan.MsgSig = make(chan *msg.Message, size)
	evtchan.ConnectedSig = make(chan *network.ConnInfo, size)
	evtchan.DisconnectedSig = make(chan *network.ConnInfo, size)
	return evtchan
}

//创建tcp消息客户端
func NewTcpMsgClient(msgPacker network.MsgPacker, evt *MsgEvtChan, msgparser msg.MsgParser, autoReconnect bool) *TcpMsgClient {
	tmc := new(TcpMsgClient)
	tmc.stopSig = make(chan bool, 1)
	tcpevt := network.NewClientEventChan(conf.EVTCHANSIZE)

	//不自动连接
	if !autoReconnect {
		tmc.tcpclient = network.NewTcpClient(msgPacker, tcpevt)
	} else { //自动连接
		tcpclient := network.NewTcpClient(msgPacker, tcpevt)
		tmc.tcpclient = network.NewReConnectTcpClient(tcpclient, 3)
	}
	tmc.msgparser = msgparser

	go func() {
		util.PanicErrStack()
	ForEnd:
		for {
			select {
			case clt := <-tcpevt.ConnectedSig:
				evt.ConnectedSig <- clt
				tmc.clt = clt
				//example.invalid("tcpmsgclient connected...")
			case clt := <-tcpevt.DisconnectedSig:
				evt.DisconnectedSig <- clt
			case data := <-tcpevt.DataSig:
				evt.DataSig <- data
			case msgdata := <-tcpevt.MsgSig:
				msg, err := tmc.msgparser.UnMarshal(msgdata)
				if msg == nil {
					mlog.Error("消息解析錯誤 %v, %v", err, msgdata)
					return
				}
				mlog.Debug("收到消息%v", msg.MsgData)
				if err != nil {
					mlog.Error("连接%v出现错误%v,连接关闭", tmc.clt, err)
					tmc.Close()
					evt.DisconnectedSig <- tmc.clt
					break
				} else {
					evt.MsgSig <- msg
				}
			case <-tmc.stopSig:
				break ForEnd
			}
		}

		mlog.Debug("TCP连接%v消息处理线程退出", tmc.clt)
	}()

	return tmc
}

//连接远程地址
func (tmc *TcpMsgClient) Connect(addr string) error {
	return tmc.tcpclient.Connect(addr)
}

//关闭连接
func (tmc *TcpMsgClient) Close() {
	tmc.tcpclient.Close()
}

//結束连接 optimize by rory
func (tmc *TcpMsgClient) CloseAndStop() {
	tmc.tcpclient.Close()
	tmc.stopSig <- true
}

//是否连接
func (tmc *TcpMsgClient) IsConnected() bool {
	return tmc.tcpclient.IsConnected()
}

//发送消息
func (tmc *TcpMsgClient) SendMessage(msg *msg.Message) error {
	data, err := tmc.msgparser.Marshal(msg)

	if err != nil {
		return err
	}
	return tmc.tcpclient.WriteMsg(data)
}

//发送二进制数据
func (tmc *TcpMsgClient) SendData(data []byte) error {
	return tmc.tcpclient.WriteMsg(data)
}
