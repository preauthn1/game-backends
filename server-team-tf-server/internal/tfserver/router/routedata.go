package router

import "fmt"

// RouteDataNames
const (
	RouteDataName_SourceIP   = "routedata_sourceip"
	RouteDataName_SourcePort = "routedata_sourceport"
)

// RouteData 路由数据
type RouteData map[string]interface{}

func (rdata RouteData) String() string {
	return fmt.Sprintf("%s:%d", rdata.GetSourceIP(), rdata.GetSourcePort())
}

// GetSourceIP 取得來源 IP
func (rdata RouteData) GetSourceIP() string {
	return rdata[RouteDataName_SourceIP].(string)
}

// GetSourcePort 取得來源Port號
func (rdata RouteData) GetSourcePort() int {
	return rdata[RouteDataName_SourcePort].(int)
}

// NewRouteData create RouteData
func NewRouteData(ip string, port int) RouteData {
	rd := make(map[string]interface{})
	rd[RouteDataName_SourceIP] = ip
	rd[RouteDataName_SourcePort] = port

	return rd
}
