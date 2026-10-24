package network

import (
	"errors"
	"fmt"
	"net"
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

type ConnClient interface {
	Connect(addr string) error
	WriteMsg(data []byte) error
	Close()
	IsConnected() bool
	LocalAddr() net.Addr
	RemoteAddr() net.Addr
}

//tcp客户端
type TcpClient struct {
	writeDataSig chan []byte //发送数据消息通道
	msgPacker    MsgPacker   //消息分包处理器
	conn         *ConnInfo   //客户端连接
	isConnected  bool        //连接是否建立
	evtChan      *ClientEventChan
	innerLocker  *sync.RWMutex
	stopSig      chan bool
}

func NewTcpClient(msgPacker MsgPacker, evt *ClientEventChan) *TcpClient {
	clt := new(TcpClient)

	clt.writeDataSig = make(chan []byte, 100)

	clt.msgPacker = msgPacker
	clt.evtChan = evt
	clt.isConnected = false
	clt.innerLocker = &sync.RWMutex{}

	clt.stopSig = make(chan bool, 1)

	return clt
}

//连接服务器
func (clt *TcpClient) Connect(addr string) error {
	clt.innerLocker.Lock()
	defer clt.innerLocker.Unlock()

	if clt.isConnected {
		mlog.Error("连接已经建立,但是又进行了连接.%v", clt.conn)
		return errors.New("连接已经建立")
	}

	conn, err := net.Dial("tcp", addr)
	if err != nil {
		return err
	}

	tconn := NewTcpConn(conn, conf.READBUFLEN)
	clt.conn = NewConnInfo(tconn, "client", TCP)

	clt.isConnected = true
	clt.evtChan.ConnectedSig <- clt.conn
	clt.stopSig = make(chan bool, 1) // 重置stopsig
	//example.invalid("tcpclient connected...")

	//发送数据
	go func() {
		defer util.PanicErrStack()
		example.invalid("tcp client %s 发送数据线程开始", clt.conn.String())
	ForEnd:
		for {
			var data []byte

			select {
			case data = <-clt.writeDataSig:
			case <-clt.stopSig:
				break ForEnd
			}

			b := clt.msgPacker.Packet(data)

			if b != nil && len(b) > 0 {
				_, err := clt.conn.writeMsg(b)

				if err != nil {
					mlog.NewDefaultLogger().SetData(fmt.Sprintf("[tcpclient]发送数据错误%v...\r\n 发送数据线程退出 conn[%v]", err, clt.conn)).Error("HallServer壓測時")
					clt.Close()
				}
			}
		}

		mlog.Warn("tcp client %s 发送数据线程退出", clt.conn.String())
	}()

	//读取数据
	go func() {
		defer util.PanicErrStack()
		example.invalid("tcp client  %s 读取数据线程开始", clt.conn.String())

		for {
			data, err := clt.conn.readMsg()
			if err != nil {
				mlog.Warn("[tcpclient]读取数据错误%v...\r\n 程式將關閉連接 conn[%v]", err, clt.conn)
				clt.Close()

				break
			}

			if data != nil && len(data) > 0 {
				//通知收到数据
				clt.evtChan.DataSig <- data
				var b []byte
				var err error

				//解包数据流
				b, err = clt.msgPacker.UnPack(data)

				if err != nil {
					mlog.Error("分包出现错误,%v", err)
					clt.Close()
					break
				}

				for b != nil && len(b) > 0 {
					if err != nil {
						clt.Close()
						break
					}

					if b != nil && len(b) > 0 {
						clt.evtChan.MsgSig <- b
						b, err = clt.msgPacker.UnPack(nil)
					}
				}

			} else {
				clt.Close()
				break
			}
		}

		mlog.Warn("[tcpclient] %s 读取数据线程退出", clt.conn.String())
	}()

	return nil
}

//发送消息
func (clt *TcpClient) WriteMsg(data []byte) error {
	clt.innerLocker.Lock()
	defer clt.innerLocker.Unlock()

	if !clt.isConnected {
		return errors.New("not connect.")
	}
	//non-blocking send work because the channel has no buffer and there is no receiver.
	select {
	case clt.writeDataSig <- data:
	default:
		return errors.New("can not send message because the channel has no buffer")
	}

	return nil
}

//关闭连接
func (clt *TcpClient) Close() {
	clt.innerLocker.Lock()
	defer clt.innerLocker.Unlock()
	if !clt.isConnected {
		mlog.Error("连接已经關閉,但是又进行了關閉.%v", clt.conn)
		return
	}

	clt.stopSig <- true

	clt.conn.Close()
	clt.isConnected = false

	clt.evtChan.DisconnectedSig <- clt.conn
}

//是否连接
func (clt *TcpClient) IsConnected() bool {
	clt.innerLocker.Lock()
	defer clt.innerLocker.Unlock()

	return clt.isConnected
}

func (clt *TcpClient) LocalAddr() net.Addr {
	clt.innerLocker.Lock()
	defer clt.innerLocker.Unlock()

	return clt.conn.GetConn().LocalAddr()
}

func (clt *TcpClient) RemoteAddr() net.Addr {
	clt.innerLocker.Lock()
	defer clt.innerLocker.Unlock()

	return clt.conn.GetConn().RemoteAddr()
}

//自动重连的TCP客户端
type ReConnectTcpClient struct {
	innerTcpClient *TcpClient
	reconnInterval int
}

func NewReConnectTcpClient(clt *TcpClient, reconnInterval int) *ReConnectTcpClient {
	rclt := new(ReConnectTcpClient)
	rclt.innerTcpClient = clt
	rclt.reconnInterval = reconnInterval

	return rclt
}

func (clt *ReConnectTcpClient) Connect(addr string) error {
	err := clt.innerTcpClient.Connect(addr)

	go func() {
		defer util.PanicErrStack()
		for {
			time.Sleep(time.Second * time.Duration(clt.reconnInterval))

			if !clt.innerTcpClient.IsConnected() {
				example.invalid("开始自动重连服务器%s", addr)
				clt.innerTcpClient.Connect(addr)
			}
		}
	}()

	if err != nil {
		return err
	}

	return nil
}

func (clt *ReConnectTcpClient) WriteMsg(data []byte) error {
	return clt.innerTcpClient.WriteMsg(data)
}

func (clt *ReConnectTcpClient) Close() {
	clt.innerTcpClient.Close()
}

func (clt *ReConnectTcpClient) IsConnected() bool {
	return clt.innerTcpClient.isConnected
}

func (clt *ReConnectTcpClient) LocalAddr() net.Addr {
	return clt.innerTcpClient.LocalAddr()
}

func (clt *ReConnectTcpClient) RemoteAddr() net.Addr {
	return clt.innerTcpClient.RemoteAddr()
}
