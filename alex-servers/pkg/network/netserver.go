package network

import (
	"bytes"
	"errors"
	"fmt"
)

//网络服务器
type NetServer interface {
	Start(evtchar *ServerNetEventChan) error                  //启动服务器
	Stop()                                                    //停止服务器
	ServerID() string                                         //服务器标识
	RangeConn(f func(conn *ConnInfo))                         //遍历所有连接
	GetConnByID(connid string) *ConnInfo                      //获取连接
	CloseConn(conninfo *ConnInfo)                             //关闭客户端连接
	CloseConnByConnID(connid string)                          //根据连接ID关闭客户端连接
	SendMessage(conninfo *ConnInfo, data []byte) (int, error) //发送消息
	SendAllMessage(data []byte)                               //广播消息
	ConnNum() int                                             //当前连接数
	ServerType() int                                          //服务器类型
	String() string                                           //服务器描述信息

}

//服务器配置
type ServerCfg struct {
	Addr       string //侦听地址
	MaxConnNum int    //最大连接数
	ServerID   string //服务器标识

	OtherCfg interface{}
}

//检查服务器参数
func (cfg *ServerCfg) CheckCfg() error {
	if cfg.Addr == "" {
		return errors.New("服务器侦听地址不能为空")
	}
	if cfg.MaxConnNum <= 0 {
		return errors.New("最大连接数必须为大于0的数字")
	}

	if cfg.ServerID == "" {
		return errors.New("服务器标识不能为空")
	}

	return nil
}

func NewServerCfg(addr string, maxconnNum int, serverid string) *ServerCfg {
	cfg := new(ServerCfg)
	cfg.Addr = addr
	cfg.MaxConnNum = maxconnNum
	cfg.ServerID = serverid

	return cfg
}

//服务器组
type NetServerGroup struct {
	innerServers map[string]NetServer
	serverID     string
}

func NewNetServerGroup(sid string) *NetServerGroup {
	svr := new(NetServerGroup)
	svr.serverID = sid
	svr.innerServers = make(map[string]NetServer)

	return svr
}

func (svr *NetServerGroup) AddServer(s NetServer) error {
	sid := s.ServerID()

	if _, ok := svr.innerServers[sid]; ok {
		return errors.New("服务器已经存在")
	}

	svr.innerServers[sid] = s

	return nil
}

func (svr *NetServerGroup) Start(evtchan *ServerNetEventChan) error {
	errstr := ""

	for _, s := range svr.innerServers {
		err := s.Start(evtchan)

		if err != nil {
			errstr += err.Error()
			errstr += "\r\n"
		}
	}

	if errstr == "" {
		return nil
	}

	return errors.New(errstr)
}

func (svr *NetServerGroup) Stop() {
	for _, s := range svr.innerServers {
		s.Stop()
	}
}

func (svr *NetServerGroup) ServerID() string {
	return svr.serverID
}

func (svr *NetServerGroup) RangeConn(f func(conn *ConnInfo)) {
	for _, s := range svr.innerServers {
		s.RangeConn(f)
	}
}

func (svr *NetServerGroup) GetConnByID(connid string) *ConnInfo {
	var ci *ConnInfo

	for _, s := range svr.innerServers {
		ci = s.GetConnByID(connid)

		if ci != nil {
			return ci
		}
	}

	return nil
}

func (svr *NetServerGroup) CloseConn(conninfo *ConnInfo) {
	sid := conninfo.ServerID

	s := svr.innerServers[sid]
	if s == nil {
		return
	}

	if conninfo != nil {
		s.CloseConn(conninfo)
	}

	return
}

func (svr *NetServerGroup) CloseConnByConnID(connid string) {
	clt := svr.GetConnByID(connid)

	if clt != nil {
		svr.CloseConn(clt)
	}
}

func (svr *NetServerGroup) SendMessage(conninfo *ConnInfo, data []byte) (int, error) {
	sid := conninfo.ServerID

	s := svr.innerServers[sid]
	if s == nil {
		return 0, errors.New("serverid不存在")
	}

	return s.SendMessage(conninfo, data)
}

func (svr *NetServerGroup) SendAllMessage(data []byte) {
	for _, s := range svr.innerServers {
		s.SendAllMessage(data)
	}
}

func (svr *NetServerGroup) ConnNum() int {
	num := 0

	for _, s := range svr.innerServers {
		num += s.ConnNum()
	}

	return num
}

func (svr *NetServerGroup) RangeServer(f func(s NetServer)) {
	for _, s := range svr.innerServers {
		f(s)
	}
}

func (svr *NetServerGroup) GetServerByID(id string) NetServer {
	netsvr, ok := svr.innerServers[id]

	if !ok {
		return nil
	}

	return netsvr
}

func (server *NetServerGroup) ServerType() int {
	return Mixed
}

func (server *NetServerGroup) ClearServer() {
	server.innerServers = make(map[string]NetServer)
}

func (server *NetServerGroup) String() string {
	var buf bytes.Buffer
	buf.WriteString(fmt.Sprintf("server count:%d,conn count:%d\r\n", len(server.innerServers), server.ConnNum()))

	//写服务器组信息
	for _, s := range server.innerServers {
		buf.WriteString(s.String() + "\r\n")
	}

	return buf.String()
}
