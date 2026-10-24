package robotcomm

import (
	"net"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/network"
)

const (
	ServerID_RobotConn = "robotconn"
)

//机器人客户端连接
type RobotConn struct {
}

func (rc *RobotConn) ReadMsg() ([]byte, error) {
	return nil, nil
}

func (rc *RobotConn) WriteMsg(data []byte) (int, error) {
	return 0, nil
}

func (rc *RobotConn) LocalAddr() net.Addr {
	return nil
}

func (rc *RobotConn) RemoteAddr() net.Addr {
	return nil
}

func (rc *RobotConn) Close() {

}

func (rc *RobotConn) Destory() {

}

func NewRebotConn() *RobotConn {
	rconn := new(RobotConn)

	return rconn
}

//创建机器人客户端信息
func NewRobotConnInfo() *network.ConnInfo {
	cinfo := network.NewConnInfo(NewRebotConn(), ServerID_RobotConn, network.TCP)
	return cinfo
}

//是否机器人客户端
func IsRobotConn(connid string) bool {
	return strings.Index(connid, "unknown:-1_") != -1
}
