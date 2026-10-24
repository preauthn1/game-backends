package tfserver

import (
	"sync"
	"time"
)

//转发服务器列表
type TFServerList struct {
	innerServerLst map[string]*TFServerInfo //服务器列表
	locker         *sync.RWMutex            //锁
}

func NewTFserverList() *TFServerList {
	tfs := &TFServerList{}
	tfs.innerServerLst = make(map[string]*TFServerInfo)
	tfs.locker = &sync.RWMutex{}

	return tfs
}

//获取服务器列表
func (tfs *TFServerList) GetServerList() map[string]TFServerInfo {
	tfs.locker.RLock()
	defer tfs.locker.RUnlock()

	data := make(map[string]TFServerInfo)

	for k, v := range tfs.innerServerLst {
		data[k] = *v
	}

	return data
}

//更新服务器信息
func (tfs *TFServerList) UpdateTFServer(ip string, cn int) {
	tfs.locker.Lock()
	defer tfs.locker.Unlock()

	etf, ok := tfs.innerServerLst[ip]
	if ok {
		etf.CurrentConn = cn
		etf.IPAddress = ip
		etf.LastRegTime = time.Now()
	} else {
		tfs.innerServerLst[ip] = &TFServerInfo{IPAddress: ip, CurrentConn: cn, LastRegTime: time.Now()}
	}
}

var TFServerManager *TFServerList

func init() {
	TFServerManager = NewTFserverList()
}
