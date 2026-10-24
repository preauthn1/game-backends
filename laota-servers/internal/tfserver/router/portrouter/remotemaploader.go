package portrouter

import (
	"io/ioutil"
	"net/http"
	"strconv"
	"strings"
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"

	"example.invalid/ServerTeam/Servers/internal/tfserver/router"
)

// RemoteMapLoader 远程端口映射加载器
type RemoteMapLoader struct {
	remoteAddr string
	pmData     PortMap
}

// NewRemoteMapLoader 创建远程端口映射加载器对象
func NewRemoteMapLoader(remoteAddr string) *RemoteMapLoader {
	rml := new(RemoteMapLoader)
	rml.remoteAddr = remoteAddr
	rml.pmData = rml.loadMapData()

	return rml
}

func (rml *RemoteMapLoader) loadMapData() PortMap {

	resp, err := http.Get(rml.remoteAddr)
	defer func() {
		if resp != nil && resp.Body != nil {
			resp.Body.Close()
		}
	}()

	if err != nil {
		mlog.Error("加载远程端口映射配置文件错误%v", err)
		return nil
	}
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		mlog.Error("读取远程端口映射配置文件错误.%v", err)
		return nil
	}

	cfgdata := util.ReadXmlMap(body)
	pm := make(map[int]*router.RouteTarget)

	for skey, sval := range cfgdata {
		//源端口
		sSourcePort := skey
		//目的地址
		sTarget := sval

		sport, err := strconv.Atoi(sSourcePort)
		if err != nil {
			mlog.Error("解析源端口发生错误,%s,%v", sSourcePort, err)
			continue
		}

		sArrTarget := strings.Split(sTarget, ":")
		if len(sArrTarget) != 2 {
			mlog.Error("解析目标地址错误,必须以:分隔.%s", sTarget)
			continue
		}

		sTargetIP := sArrTarget[0]
		sTargetPort := sArrTarget[1]
		dport, err := strconv.Atoi(sTargetPort)
		if err != nil {
			mlog.Error("解析目标地址错误,端口号不正确.%s,%v", sTarget, err)
			continue
		}

		rt := new(router.RouteTarget)
		rt.TargetPort = dport
		rt.TargetIP = sTargetIP

		pm[sport] = rt
	}

	return pm
}

// LoadPortMap 取得PortMap
func (rml *RemoteMapLoader) LoadPortMap() PortMap {
	return rml.pmData
}

// AutoRefreshRemoteMapLoader 自动刷新的远程端口映射加载器
type AutoRefreshRemoteMapLoader struct {
	innerRemoteMapLoader *RemoteMapLoader
	locker               *sync.RWMutex
	portMap              PortMap
}

// NewAutoRefreshRemoteConfigMapLoader 创建自动刷新的远程端口映射加载器对象
func NewAutoRefreshRemoteConfigMapLoader(addr string, refreshInterval int) *AutoRefreshRemoteMapLoader {
	arml := new(AutoRefreshRemoteMapLoader)
	arml.innerRemoteMapLoader = NewRemoteMapLoader(addr)
	arml.locker = &sync.RWMutex{}

	arml.portMap = arml.innerRemoteMapLoader.LoadPortMap()
	example.invalid("刷新远程端口映射完成...%v", arml.portMap)
	//定时重新加载
	go func() {
		for {
			time.Sleep(time.Duration(refreshInterval) * time.Second)

			arml.locker.Lock()
			mp := arml.innerRemoteMapLoader.loadMapData()
			arml.locker.Unlock()

			if mp != nil && len(mp) > 0 {
				arml.portMap = mp
				example.invalid("刷新远程端口映射完成...%v", mp)
			}
		}
	}()

	return arml
}

// LoadPortMap 取得PortMap
func (rml *AutoRefreshRemoteMapLoader) LoadPortMap() PortMap {
	rml.locker.Lock()
	defer rml.locker.Unlock()

	return rml.portMap
}
