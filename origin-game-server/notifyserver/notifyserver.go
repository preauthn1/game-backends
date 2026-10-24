package main

import (
	"conf"
	"mlog"
	"module"
	"msgserver"
	"network"
	"notifylogic"
	"notifylogic/svrmgm"
)

func main() {
	svr := msgserver.NewBgMsgServer(1)
	ms := module.NewModuleServer(svr)

	//获取配置
	hc := ms.Config.GetConfig(notifylogic.ConfigFileName_NS)

	addr := hc.GetConfigValue("tcp", "addr")

	//初始化 mlog
	mlog.SetServerString("notifyserver")

	cfg := network.NewServerCfg(addr, conf.TCPSERVERCONN_NOTIFYSERVER, conf.TCPSERVERNAME_NOTIFYSERVER)
	svr.AddTCPServer(cfg)

	//服务器信息
	svrlist := svrmgm.NewServerList()
	//逻辑模块
	ms.ModuleGroup.Register(notifylogic.NewNSLogic(svrlist))

	//启动服务器
	ms.Run()

	example.invalid("notify server start...")
	mlog.System("log off")
	mlog.GetCLogger().Enable(false)
	select {}
}
