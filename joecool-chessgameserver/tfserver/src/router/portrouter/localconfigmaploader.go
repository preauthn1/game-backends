package portrouter

import (
	"mlog"
	"router"
	"strconv"
	"strings"
	"sync"
	"time"
	"util"
)

//基于本地配置文件方式的端口映射加载器
type LocalConfigMapLoader struct {
	portMap     PortMap
	cfgFileName string
}

//创建基于本地配置文件的端口映射加载器
func NewLocalConfigMapLoader(cfgFileName string) *LocalConfigMapLoader {
	pl := new(LocalConfigMapLoader)
	pl.cfgFileName = cfgFileName
	pl.loadConfigData()

	return pl
}

//加载配置数据
func (mp *LocalConfigMapLoader) loadConfigData() {
	mp.portMap = make(map[int]*router.RouteTarget)

	cfgdata := util.ReadXmlFile(mp.cfgFileName)
	if cfgdata != nil {
		for _, item := range cfgdata {
			//源端口
			sSourcePort := item.Key
			//目的地址
			sTarget := item.Value

			sport, err := strconv.Atoi(sSourcePort)
			if err != nil {
				mlog.Error("解析源端口发生错误,%v", err)
				continue
			}

			sArrTarget := strings.Split(sTarget, ":")
			if len(sArrTarget) != 2 {
				mlog.Error("解析目标地址错误,必须以:分隔")
				continue
			}

			sTargetIP := sArrTarget[0]
			sTargetPort := sArrTarget[1]
			dport, err := strconv.Atoi(sTargetPort)
			if err != nil {
				mlog.Error("解析目标地址错误,端口号不正确.%v", err)
				continue
			}

			rt := new(router.RouteTarget)
			rt.TargetPort = dport
			rt.TargetIP = sTargetIP

			mp.portMap[sport] = rt
		}
	}
}

func (mp *LocalConfigMapLoader) LoadPortMap() map[int]*router.RouteTarget {
	return mp.portMap
}

//自动刷新的本地配置映射加载器
type AutoRefreshLocalConfigMapLoader struct {
	locker      *sync.RWMutex
	innerLoader *LocalConfigMapLoader
}

//创建自动刷新的本地映射配置加载器
func NewAutoRefreshLocalConfigMapLoader(cfgFileName string, loadInterval int) *AutoRefreshLocalConfigMapLoader {
	ml := new(AutoRefreshLocalConfigMapLoader)
	ml.locker = &sync.RWMutex{}
	ml.innerLoader = NewLocalConfigMapLoader(cfgFileName)
	ml.innerLoader.loadConfigData()

	go func() {
		time.Sleep(time.Duration(loadInterval) * time.Second)

		ml.locker.Lock()
		ml.innerLoader.loadConfigData()
		ml.locker.Unlock()
		example.invalid("刷新端口映射配置文件完成...")
	}()

	return ml
}

func (mp *AutoRefreshLocalConfigMapLoader) LoadPortMap() map[int]*router.RouteTarget {
	mp.locker.RLock()
	defer mp.locker.RUnlock()

	return mp.innerLoader.portMap
}
