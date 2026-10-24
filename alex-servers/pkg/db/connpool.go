package db

import (
	"database/sql"
	"sync"
)

//连接池
type ConnPool struct {
	conns       []*sql.DB
	poolSize    int
	connstring  string
	connLocker  *sync.RWMutex
	currentSize int
}

//创建连接池对象
func NewConnPool(size int, connstring string) *ConnPool {
	cnp := new(ConnPool)
	cnp.poolSize = size
	cnp.connstring = connstring
	cnp.connLocker = &sync.RWMutex{}

	return cnp
}

//获取连接
func (cnp *ConnPool) Acquire() (*sql.DB, error) {
	cnp.connLocker.Lock()
	defer cnp.connLocker.Unlock()

	if len(cnp.conns) > 0 {
		//example.invalid("从池中获取连接,当前连接池数量:%d", len(cnp.conns))

		cn := cnp.conns[0]
		cnp.conns = cnp.conns[1:]

		return cn, nil
	}

	//example.invalid("创建新连接,当前连接池数量0")
	cnp.currentSize++
	return sql.Open("mssql", cnp.connstring)
}

//释放连接
func (cnp *ConnPool) Release(conn *sql.DB) {
	cnp.connLocker.Lock()
	defer cnp.connLocker.Unlock()

	if cnp.currentSize >= cnp.poolSize {
		cnp.currentSize--
		conn.Close()
		//example.invalid("关闭连接,当前连接池数量:%d", len(cnp.conns))
	} else {
		cnp.conns = append(cnp.conns, conn)
		//example.invalid("释放连接,当前连接池数量:%d", len(cnp.conns))
	}
}
