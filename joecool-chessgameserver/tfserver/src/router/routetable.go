package router

//路由表
type RouteTable struct {
	routers []Router
}

//添加路由
func (rt *RouteTable) AddRouter(r Router) {
	rt.routers = append(rt.routers, r)
}

//获取路由目标
func (rt *RouteTable) GetRouteTarget(data RouteData) *RouteTarget {
	for _, r := range rt.routers {
		ret := r.GetRouteTarget(data)
		if ret != nil {
			return ret
		}
	}

	return nil
}

var RouteCtx *RouteTable

func init() {
	RouteCtx = new(RouteTable)
}
