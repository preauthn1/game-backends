package network

import (
	"conf"
	"fmt"
	"mlog"
	"net"
	"sync"
	"util"
)

//消息分包器生成函数
type MesasgePackFun func() MsgPacker

//tcp服务器
type TCPServer struct {
	netListener   net.Listener
	msgPackerFun  MesasgePackFun
	cfg           *ServerCfg
	evtChan       *ServerNetEventChan
	conns         ConnSet
	connLocker    *sync.RWMutex
	commpacker    MsgPacker
	stopSig       chan bool
	isServerStart bool
}

//创建TCP服务器对象
func NewTcpServer(msgPacker MesasgePackFun, cfg *ServerCfg) (*TCPServer, error) {
	svr := new(TCPServer)
	svr.msgPackerFun = msgPacker           //创建消息分包方法
	svr.conns = make(map[string]*ConnInfo) //当前客户端连接集合
	svr.connLocker = new(sync.RWMutex)     //客户端连接集合同步锁

	svr.cfg = cfg
	err := cfg.CheckCfg()

	if err != nil {
		return nil, err
	}

	//默认的消息打包对象，用于发送消息的时候进行打包
	svr.commpacker = svr.msgPackerFun()

	svr.stopSig = make(chan bool, 1)

	return svr, nil
}

//启动服务器
func (server *TCPServer) Start(evtchan *ServerNetEventChan) error {
	var err error

	server.netListener, err = net.Listen("tcp", server.cfg.Addr)
	if err != nil {
		return err
	}

	server.evtChan = evtchan

	go func() {
		defer util.PanicErrStack()

		for {
			select {
			case <-server.stopSig:
				break
			default:
			}

			conn, err := server.netListener.Accept()

			if err != nil {
				continue
			}

			tconn := NewTcpConn(conn, conf.READBUFLEN) // 將net.Conn 轉換成 TcpConn
			cinfo := NewConnInfo(tconn, server.cfg.ServerID, TCP)

			go server.handleConn(cinfo)
		}

		mlog.System("服务器%s侦听%s线程退出.", server.cfg.ServerID, server.cfg.Addr)
	}()

	mlog.System("tcp服务器启动成功,侦听地址%s", server.cfg.Addr)
	//设置服务器状态
	server.isServerStart = true
	return nil
}

//处理连接
func (server *TCPServer) handleConn(cinfo *ConnInfo) {
	defer func() {
		if err := recover(); err != nil {
			server.CloseConn(cinfo)

			mlog.Error("客户端连接%s发生错误%v", cinfo, err)
		}
	}()

	//检查连接是否超出限制
	server.connLocker.Lock()
	if len(server.conns) >= server.cfg.MaxConnNum {
		server.connLocker.Unlock()

		//关闭客户端连接
		cinfo.Close()

		mlog.Error("连接数量超出限制%d,关闭连接%s", server.cfg.MaxConnNum, cinfo)

		return
	}

	//如果连接存在，先关闭
	if ecinfo, ok := server.conns[cinfo.connID]; ok {
		server.connLocker.Unlock()
		server.CloseConn(ecinfo)
		server.connLocker.Lock()
	}

	server.conns[cinfo.connID] = cinfo
	server.connLocker.Unlock()

	//创建数据流分包处理器
	connpacker := server.msgPackerFun()

	//通知客户端连接
	mlog.Debug("客户端%v连接...", cinfo)
	//mlog.NewDefaultLogger().Info()
	server.evtChan.ConnectSig <- cinfo

	for {
		data, err := cinfo.conn.ReadMsg()
		if err != nil {
			server.CloseConn(cinfo)
			if err.Error() == "EOF" {
				mlog.Warn("客户端%v读取数据出现错误,关闭连接,%v", cinfo, err)
			} else {
				mlog.Error("客户端%v读取数据出现错误,关闭连接,%v", cinfo, err)
			}

			return
		}

		if data != nil && len(data) > 0 {
			//通知收到数据
			server.evtChan.DataSig <- &ConnData{cinfo, data}
			//解包数据流
			var b []byte
			var err error
			b, err = connpacker.UnPack(data)

			if err != nil {
				mlog.Error("分包出现错误%s,%v", cinfo, err)
				server.CloseConn(cinfo)
				return
			}

			for b != nil && len(b) > 0 {
				if b != nil && len(b) > 0 {
					if err != nil {
						mlog.Error("分包出现错误%s,%v", cinfo, err)
						server.CloseConn(cinfo)
						return
					}

					server.evtChan.MsgSig <- &ConnData{cinfo, b}
					//example.invalid("收到客户端%v数据包%v", cinfo, b)
					b, err = connpacker.UnPack(nil)
				} else {
					b = nil
					err = nil
				}
			}
		} else {
			mlog.Error("接收到数据包长度为0,关闭客户端%s", cinfo)
			server.CloseConn(cinfo)
			return
		}

	}
}

//服务器标识
func (server *TCPServer) ServerID() string {
	return server.cfg.ServerID
}

//关闭客户端连接
func (server *TCPServer) CloseConn(cinfo *ConnInfo) {
	server.connLocker.Lock()
	if _, ok := server.conns[cinfo.connID]; !ok {
		server.connLocker.Unlock()
		return
	}
	delete(server.conns, cinfo.ConnID())
	server.connLocker.Unlock()

	cinfo.Close()

	server.evtChan.DisConnectSig <- cinfo
}

func (server *TCPServer) CloseConnByConnID(connid string) {
	clt := server.GetConnByID(connid)

	if clt != nil {
		server.CloseConn(clt)
	}
}

func (server *TCPServer) RangeConn(f func(cinfo *ConnInfo)) {
	server.connLocker.RLock()
	defer server.connLocker.RUnlock()

	for _, ci := range server.conns {
		f(ci)
	}
}

func (server *TCPServer) GetConnByID(connid string) *ConnInfo {
	server.connLocker.RLock()
	defer server.connLocker.RUnlock()

	return server.conns[connid]
}

func (server *TCPServer) SendMessage(cinfo *ConnInfo, data []byte) (int, error) {
	ndata := server.commpacker.Packet(data)
	n, err := cinfo.writeMsg(ndata)

	if err != nil {
		server.CloseConn(cinfo)
	}

	return n, err
}

func (server *TCPServer) SendAllMessage(data []byte) {
	ndata := server.commpacker.Packet(data)
	var errorConns []*ConnInfo

	server.connLocker.RLock()
	for _, ci := range server.conns {
		_, err := ci.writeMsg(ndata)
		if err != nil {
			errorConns = append(errorConns, ci)
		}
	}
	server.connLocker.RUnlock()

	if len(errorConns) > 0 {
		for _, ci := range errorConns {
			server.CloseConn(ci)
		}
	}
}

func (server *TCPServer) ConnNum() int {
	server.connLocker.RLock()
	defer server.connLocker.RUnlock()

	return len(server.conns)
}

func (server *TCPServer) Stop() {
	if !server.isServerStart {
		return
	}

	server.connLocker.RLock()
	for _, ci := range server.conns {
		ci.Close()
	}
	server.connLocker.RUnlock()

	server.stopSig <- true

	server.netListener.Close()
	server.isServerStart = false
}

func (server *TCPServer) ServerType() int {
	return TCP
}

func (server *TCPServer) String() string {
	return fmt.Sprintf("TCP Server listen on:%s,serverid:%s,conn:%d", server.cfg.Addr, server.cfg.ServerID, server.ConnNum())
}
