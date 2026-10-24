package network

import (
	"errors"
	"fmt"
	"net"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

//tcp连接
type TcpConn struct {
	conn    net.Conn
	readBuf []byte
}

func NewTcpConn(conn net.Conn, buflen int) *TcpConn {
	tcpconn := new(TcpConn)
	tcpconn.conn = conn
	tcpconn.readBuf = make([]byte, buflen)

	return tcpconn
}

func (conn *TcpConn) ReadMsg() (returnData []byte, returnErr error) {
	defer func() {
		if err := recover(); err != nil {
			mlog.Error("conn read msg error: %v", err)

			returnData, returnErr = nil, errors.New(fmt.Sprintf("%v", err))
			return
		}
	}()

	n, err := conn.conn.Read(conn.readBuf)
	if err != nil {
		returnData, returnErr = nil, err
		return
	}
	if n > conf.READBUFLEN || n <= 0 {
		mlog.Error("conn read msg error:数据包长度超出范围,%d", n)

		returnData, returnErr = nil, errors.New(fmt.Sprintf("数据包长度超出范围,%d", n))

		return
	}

	returnData, returnErr = conn.readBuf[:n], nil
	return
}

func (conn *TcpConn) WriteMsg(data []byte) (int, error) {
	if data == nil || len(data) == 0 {
		return 0, nil
	}

	return conn.conn.Write(data)
}

func (conn *TcpConn) LocalAddr() net.Addr {
	return conn.conn.LocalAddr()
}

func (conn *TcpConn) RemoteAddr() net.Addr {
	return conn.conn.RemoteAddr()
}

func (conn *TcpConn) Close() {
	conn.conn.Close()
}

func (conn *TcpConn) Destory() {
	conn.Close()
	conn.readBuf = nil
}
