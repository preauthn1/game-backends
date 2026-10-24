package main

import (
	"example.invalid/ServerTeam/Servers/internal/tfserver/router"
	"example.invalid/ServerTeam/Servers/internal/tfserver/router/portrouter"
	"example.invalid/ServerTeam/Servers/internal/tfserver/tfconfig"
	"example.invalid/ServerTeam/Servers/internal/tfserver/tfnet"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/conf"
)

var tfProvider tfnet.TFListenPortProvider

func init() {
	conf.CLogLevel = 3
	conf.ClearLog = true
}
func main() {

	//初始化路由表
	initRouteTable()

	//初始化转发通道
	initTFChannel()

	//命令
	command.AddCommand(tfconfig.NewCheckKeyCmd())
	command.Run()

	//启动服务器
	startTFServer()

	select {}
}

//初始化路由表
func initRouteTable() {
	//端口映射加载器
	pml := portrouter.NewAutoRefreshRemoteConfigMapLoader(tfconfig.ConfigCtx.PortMapURL, tfconfig.ConfigCtx.RefreshPortMapInterval)
	//创建端口映射路由
	prt := portrouter.NewPortRouter(pml)

	router.RouteCtx.AddRouter(prt)

	//转发端口提供
	tfProvider = NewRemotePortMapTFPortProvider(pml)
}

//初始化转发通道
func initTFChannel() {
	//路由通道
	ptfc := tfnet.NewPortTFChannel()
	tfnet.ChannelCtx.AddChannel(ptfc)

	//状态上报通道
	rpc := tfnet.NewReportChannel(tfconfig.ConfigCtx.ReportURL, tfconfig.ConfigCtx.ReportInterval)
	tfnet.ChannelCtx.AddChannel(rpc)
}

//启动转发服务器
func startTFServer() {
	ptckInterval := tfconfig.ConfigCtx.CheckTFPortInterval

	tfSvr := tfnet.NewAutoCheckPortTFServer(tfProvider, ptckInterval)
	tfSvr.Start()
}

//转发端口包装类,从转发端口映射规则中解析需要侦听的端口
//实现TFListenPortProvider接口
type RemotePortMapTFPortProvider struct {
	pmLoader portrouter.PortMapLoader
}

func NewRemotePortMapTFPortProvider(pmLoader portrouter.PortMapLoader) *RemotePortMapTFPortProvider {
	rpmtf := new(RemotePortMapTFPortProvider)
	rpmtf.pmLoader = pmLoader

	return rpmtf
}

func (rpm *RemotePortMapTFPortProvider) LoadListenPort() []int {
	//侦听端口
	var listenPorts []int

	pm := rpm.pmLoader.LoadPortMap()
	if pm != nil && len(pm) > 0 {
		for pt, _ := range pm {
			listenPorts = append(listenPorts, pt)
		}
	}

	return listenPorts
}
