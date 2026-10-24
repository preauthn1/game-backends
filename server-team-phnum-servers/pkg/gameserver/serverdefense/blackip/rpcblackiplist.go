package blackip

import (
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/util"
)

//加载黑名单函数
type LoadBlackIPFunc func(func(map[string]string))

//rpc IP黑名单
type RPCBlackIPList struct {
	blackIPList     map[string]string
	loadBlackIPFunc LoadBlackIPFunc
	blockIPFunc     func(ip string, sec int, memo string)
	loadInterval    int

	innerLocker *sync.RWMutex
}

//创建RPC IP黑名单
func NewRPCBlackIPList(loadInterval int, loadIPFunc LoadBlackIPFunc, blockIPFunc func(ip string, sec int, memo string)) *RPCBlackIPList {
	rpciplist := new(RPCBlackIPList)
	rpciplist.loadBlackIPFunc = loadIPFunc
	rpciplist.blockIPFunc = blockIPFunc
	rpciplist.innerLocker = &sync.RWMutex{}
	rpciplist.loadInterval = loadInterval
	rpciplist.blackIPList = make(map[string]string)

	return rpciplist
}

func (rpciplst *RPCBlackIPList) StartLoadIPList() {
	go func() {
		defer util.PanicErrStack()

		for {
			rpciplst.loadBlackIPFunc(func(ips map[string]string) {
				if ips == nil {
					ips = make(map[string]string)
				}

				rpciplst.innerLocker.Lock()
				rpciplst.blackIPList = ips
				rpciplst.innerLocker.Unlock()
			})

			time.Sleep(time.Duration(rpciplst.loadInterval) * time.Second)
		}
	}()
}

func (rpciplst *RPCBlackIPList) BlockIP(ip string, sec int, memo string) {
	rpciplst.blockIPFunc(ip, sec, memo)

	rpciplst.innerLocker.Lock()
	rpciplst.blackIPList[ip] = ip
	rpciplst.innerLocker.Unlock()

}

func (rpciplst *RPCBlackIPList) CheckIP(ip string) bool {
	rpciplst.innerLocker.Lock()
	defer rpciplst.innerLocker.Unlock()

	if _, ok := rpciplst.blackIPList[ip]; ok {
		return true
	}

	return false
}

func (rpciplst *RPCBlackIPList) Clear() {
	rpciplst.innerLocker.Lock()
	defer rpciplst.innerLocker.Unlock()

	rpciplst.blackIPList = make(map[string]string)

}
