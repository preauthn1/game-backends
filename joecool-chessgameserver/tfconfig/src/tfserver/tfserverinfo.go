package tfserver

import (
	"fmt"
	"time"
)

type TFServerInfo struct {
	IPAddress   string    //转发服务器IP地址
	CurrentConn int       //当前连接数
	LastRegTime time.Time //最后一次注册时间
}

func (tfs *TFServerInfo) String() string {
	return fmt.Sprintf("%s 连接数:%d %v", tfs.IPAddress, tfs.CurrentConn, tfs.LastRegTime)
}
