package portrouter

import (
	"router"
)

//端口映射
type PortMap map[int]*router.RouteTarget

//基于端口的路由规则
type PortRouter struct {
	pmLoader PortMapLoader
}

//创建基于端口路由规则对象
func NewPortRouter(pmLoader PortMapLoader) *PortRouter {
	pr := new(PortRouter)
	pr.pmLoader = pmLoader

	return pr
}

//获取路由目标
func (pr *PortRouter) GetRouteTarget(data router.RouteData) *router.RouteTarget {
	sourcePort := data.GetSourcePort()

	pm := pr.pmLoader.LoadPortMap()
	if pm == nil {
		return nil
	}

	if st, ok := pm[sourcePort]; ok {
		return st
	}

	return nil
}
