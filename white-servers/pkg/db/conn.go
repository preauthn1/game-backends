package db

import (
	"fmt"
)

//数据库连接
type SqlConnInfo struct {
	ServerIP string //服务器地址
	Port     int    //端口
	UserID   string //用户名
	Password string //密码
	DbName   string
}

//创建sql数据库连接
func NewSqlConnInfo(serverip string, port int, userid string, password string, dbName string) *SqlConnInfo {
	cn := new(SqlConnInfo)
	cn.ServerIP = serverip
	cn.Port = port
	cn.UserID = userid
	cn.Password = [REDACTED]

	return cn
}

//获取连接字符串
func (cn *SqlConnInfo) GetConnString() string {
	return fmt.Sprintf("server=%s;database=%s;user id=%s;password=%s;port=%d;encrypt=disable", cn.ServerIP, cn.DbName, cn.UserID, cn.Password, cn.Port, cn.DbName)
}
