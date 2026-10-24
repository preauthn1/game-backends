package network

import (
	"fmt"
	"net"
	"strings"
	"sync"
	"time"
	"util"
)

//客户端连接集合
type ConnSet map[string]*ConnInfo

//客户端连接
type Conn interface {
	ReadMsg() ([]byte, error)          //接收数据
	WriteMsg(data []byte) (int, error) //写入数据
	LocalAddr() net.Addr               //本地地址
	RemoteAddr() net.Addr              //远程地址
	Close()                            //关闭连接
	Destory()
}

//连接类型
const (
	TCP       = iota //TCP
	WebSocket        //WebSocket
	Mixed            //TCP+WebSocket
)

//客户端连接信息
type ConnInfo struct {
	IPAddress  string      //IP
	Port       int         //端口
	conn       Conn        //连接
	ServerID   string      //所对应的服务器ID
	ConnTime   time.Time   //连接建立时间
	connID     string      //链接ID
	connLocker *sync.Mutex //锁
	ClientType int         //客户端类型
	IPRouted   bool        //ip是否已经上报
	ConnToken  string      //連線口令
}

func (cinfo *ConnInfo) String() string {
	return fmt.Sprintf("IP:%s,Port:%d,ServerID:%s", cinfo.IPAddress, cinfo.Port, cinfo.ServerID)
}

//客户端ID
func (cinfo *ConnInfo) ConnID() string {
	if cinfo == nil {
		return ""
	}
	return cinfo.connID
}

func NewConnInfo(conn Conn, sid string, ctype int) *ConnInfo {
	cinfo := new(ConnInfo)
	if conn.RemoteAddr() != nil {
		cinfo.IPAddress, cinfo.Port = util.GetIPAndPort(conn.RemoteAddr())
	} else {
		cinfo.IPAddress = "unknown"
		cinfo.Port = -1
	}
	cinfo.ServerID = sid
	cinfo.conn = conn
	cinfo.ConnTime = time.Now()
	cinfo.connID = fmt.Sprintf("%s:%d_%d", cinfo.IPAddress, cinfo.Port, cinfo.ConnTime.Unix()) //客户端ID,唯一标识客户端
	cinfo.connLocker = new(sync.Mutex)
	cinfo.ClientType = ctype
	cinfo.IPRouted = false
	cinfo.ConnToken = ""

	return cinfo
}

//发送消息
func (cinfo *ConnInfo) writeMsg(data []byte) (int, error) {
	cinfo.connLocker.Lock()
	defer cinfo.connLocker.Unlock()

	return cinfo.conn.WriteMsg(data)
}

func (cinfo *ConnInfo) readMsg() ([]byte, error) {
	return cinfo.conn.ReadMsg()
}

//关闭连接
func (cinfo *ConnInfo) Close() {
	cinfo.connLocker.Lock()
	defer cinfo.connLocker.Unlock()

	cinfo.conn.Close()
}

//是否指定服务器的连接
func (cinfo *ConnInfo) IsServerConn(serverid string) bool {
	return strings.ToLower(cinfo.ServerID) == strings.ToLower(serverid)
}

//获取连接s
func (cinfo *ConnInfo) GetConn() Conn {
	return cinfo.conn
}

//客户端数据
type ConnData struct {
	Conn *ConnInfo
	Data []byte
}

//网络客户端接口定义
type NetClient interface {
	Connect(addr string) error
	WriteMsg(data []byte) error
	Close()
	IsConnected() bool
}
