package main

import (
	"command"
	"conf"
	"dal"
	"dal/msghandler"
	"mlog"
	"module"
	"msgserver"
	"network"
)

func main() {
	svr := msgserver.NewBgMsgServer(conf.DefaultEventThrottle)
	ms := module.NewModuleServer(svr)
 
	//获取配置
	hc := ms.Config.GetConfig("db")

	addr := hc.GetConfigValue("tcp", "addr")

	//初始化 mlog
	mlog.SetServerString("dbserver")

	cfg := network.NewServerCfg(addr, conf.TCPSERVERCONN_DBSERVER, conf.TCPSERVERNAME_DBSERVER)
	svr.AddTCPServer(cfg)

	//数据库并发数
	semaphore := hc.GetConfigInt("conn", "semaphore")

	//逻辑
	ms.ModuleGroup.Register(dal.NewDBLogicModule(semaphore))

	//启动服务器
	ms.Run()

	command.AddCommand(new(msghandler.FastLogonStatCommand))
	mlog.System("dbserver start...")
	mlog.System("log off")
	mlog.GetCLogger().Enable(false)
	select {}
}
