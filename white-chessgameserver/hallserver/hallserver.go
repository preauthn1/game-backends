package main

import (
	"conf"
	"config"
	"db"
	"fmt"
	"gameserver"
	"gameserver/serverdefense"
	"halllogic"
	"halllogic/hallutil"
	hshandler "halllogic/serverhandler"
	"mlog"
	"network"
	"gamelogic"
)

func main() {
	cfgs := config.GetConfigGroup(conf.ConfigDirName)
	cfg := cfgs.GetConfig("hall")

	//服务器配置
	serverCfg := hallutil.GetHallServerConfig(cfg)

	//大厅侦听地址
	cfghs := network.NewServerCfg(serverCfg.HallAddr, conf.TCPSERVERCONN_HALL_CLIENT, conf.TCPSERVERNAME_HALL_CLIENT)
	//gameserver侦听地址
	cfggs := network.NewServerCfg(serverCfg.GSMgmAddr, conf.TCPSERVERCONN_HALL_GAMESERVER, conf.TCPSERVERNAME_HALL_GAMESERVER)

	servermode := cfg.GetConfigInt("defense", "servermode")
	defenseCfg := serverdefense.ParseDefenseConfig(cfg)

	//初始化 mlog
	mlog.SetServerString(fmt.Sprintf("%s:%d", "hallserver", serverCfg.ServerID))

	emptySuperToolCfg := gamelogic.SuperToolConfig{}
	gs := gameserver.NewGameServer([]*network.ServerCfg{cfghs, cfggs}, serverCfg.DBServerAddr, serverCfg.NotifyServerAddr, "", int(serverCfg.ServerID), conf.ServerType_HallServer, servermode, -1, defenseCfg, 0, emptySuperToolCfg, "")

	err := gs.Init()

	if err != nil {
		mlog.Error("初始化服务器错误,%v", err)
		return
	}

	//大厅逻辑
	hallmodule := halllogic.NewHallLogicModule(gs, serverCfg)
	gs.GameServerRunCtx.ModuleServer.ModuleGroup.Register(hallmodule)

	//初始化RPC IP黑名单
	gs.InitRPCBlackIPList(hallmodule.RPCMsgHandler)

	//停服广播服务器处理器
	gs.ServerHandler.AddHandler(hshandler.NewServerStopHandler(hallmodule.ServerStatus, 30))

	//启动服务器
	err = gs.Start()
	if err != nil {
		mlog.Error("服务器启动错误,%v", err)
		return
	}

	example.invalid("hallserver  start...")

	//測試 elk 收集日志用
	t := db.ProcError{500, "test message", "test ps"}
	mlog.NewDefaultLogger().SetData(t).Info("hallserver start")
	mlog.System("log off")
	mlog.GetCLogger().Enable(false)
	select {}
}
