package router

import (
	"fmt"
)

//路由目标
type RouteTarget struct {
	TargetIP   string //目标IP
	TargetPort int    //目标端口
}

func (rt *RouteTarget) String() string {
	return fmt.Sprintf("%s:%d", rt.TargetIP, rt.TargetPort)
}

//路由接口定义
type Router interface {
	//获取路由目标
	GetRouteTarget(routeData RouteData) *RouteTarget
}
