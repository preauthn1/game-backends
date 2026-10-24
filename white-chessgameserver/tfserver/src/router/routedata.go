package router

import "fmt"

const (
	RouteDataName_SourceIP   = "routedata_sourceip"
	RouteDataName_SourcePort = "routedata_sourceport"
)

//路由数据
type RouteData map[string]interface{}

func (rdata RouteData) String() string {
	return fmt.Sprintf("%s:%d", rdata.GetSourceIP(), rdata.GetSourcePort())
}

//获取源IP
func (rdata RouteData) GetSourceIP() string {
	return rdata[RouteDataName_SourceIP].(string)
}

//获取源端口
func (rdata RouteData) GetSourcePort() int {
	return rdata[RouteDataName_SourcePort].(int)
}

//创建路由数据
func NewRouteData(ip string, port int) RouteData {
	rd := make(map[string]interface{})
	rd[RouteDataName_SourceIP] = ip
	rd[RouteDataName_SourcePort] = port

	return rd
}
