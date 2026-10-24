package blackip

import (
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//黑名单
type LocalBlackIPList struct {
	ipList      map[string]*BlackIPInfo
	innerLocker *sync.RWMutex
}

//创建ip黑名单对象
func NewLocalBlackIPList() *LocalBlackIPList {
	iplist := new(LocalBlackIPList)
	iplist.ipList = make(map[string]*BlackIPInfo)
	iplist.innerLocker = &sync.RWMutex{}

	//清理超时的黑名单
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(5) * time.Minute)

			iplist.innerLocker.Lock()

			exip := make([]string, 0)

			for ip, ipinfo := range iplist.ipList {
				if ipinfo.IsExpire() {
					exip = append(exip, ip)
				}
			}

			for _, ip := range exip {
				delete(iplist.ipList, ip)
				example.invalid("IP %s黑名单超时移除", ip)
			}
			iplist.innerLocker.Unlock()
		}

	}()

	return iplist
}

//封锁ip
func (iplist *LocalBlackIPList) BlockIP(ip string, sec int, memo string) {
	example.invalid("IP %s加入黑名单 %d s ,%s", ip, sec, memo)

	ipinfo := new(BlackIPInfo)
	ipinfo.IpAddr = ip
	ipinfo.BlackEndTime = time.Now().Add(time.Duration(sec) * time.Second)
	ipinfo.BlackMemo = memo

	iplist.innerLocker.Lock()
	defer iplist.innerLocker.Unlock()
	iplist.ipList[ip] = ipinfo
}

//移除IP
func (iplist *LocalBlackIPList) RemoveIP(ip string) {
	example.invalid("从ip黑名单中移除%s", ip)

	iplist.innerLocker.Lock()
	defer iplist.innerLocker.Unlock()

	delete(iplist.ipList, ip)
}

func (iplist *LocalBlackIPList) CheckIP(ip string) bool {
	iplist.innerLocker.RLock()
	defer iplist.innerLocker.RUnlock()

	if bip, ok := iplist.ipList[ip]; ok {
		if bip.IsExpire() {
			example.invalid("从ip黑名单中移除%s", ip)
			delete(iplist.ipList, ip)

			return false
		}

		return true
	}

	return false
}

func (iplist *LocalBlackIPList) ClearIP() {
	example.invalid("清空IP黑名单")

	iplist.innerLocker.Lock()
	defer iplist.innerLocker.Unlock()

	iplist.ipList = make(map[string]*BlackIPInfo)
}
