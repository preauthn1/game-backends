package module

import (
	"command"
	"conf"
	"config"
	"event"
	"mlog"
	"msgserver"
)

//模块日志记录器，组合默认的日志记录实现
type moduleLog struct {
}

func (ml *moduleLog) WriteLog(lvl int, format string, a ...interface{}) {
	mlog.NewDefaultLogger().SetCallDepLen(5).WriteLog(lvl, format, a...)
	//mlog.WriteLog(lvl, format, a...)
}

func (ml *moduleLog) Close() {
}

//模块服务器
type ModuleServer struct {
	ModuleGroup *ModuleGroup
	Logger      mlog.Logger
	Config      *config.ConfigGroup
	Server      *msgserver.MsgServer
	moduleCtx   *MouduleCtx
}

//创建模块服务器
func NewModuleServer(msgserver *msgserver.MsgServer) *ModuleServer {
	mserver := new(ModuleServer)

	ctx := new(MouduleCtx)
	//初始化配置
	ctx.Config = config.GetConfigGroup(conf.ConfigDirName)
	//日志
	ctx.Logger = new(moduleLog)
	//事件分派
	ctx.EvtDispatcher = msgserver.EventChannel
	//添加事件分派器命令
	//消息服务器
	ctx.Server = msgserver

	//由于command引用了event模块.为避免循环引用，在此加入事件命令
	command.AddCommand(event.NewEventCmd(ctx.EvtDispatcher))

	mserver.moduleCtx = ctx
	mserver.Server = msgserver
	mserver.Config = ctx.Config
	mserver.Logger = ctx.Logger

	mserver.ModuleGroup = NewModuleGroup()

	return mserver
}

//启动服务器
func (ms *ModuleServer) Run() error {
	//初始化消息服务器
	ms.Server.InitNetServer()

	//启动消息服务器
	var err error
	err = ms.Server.Start()
	if err != nil {
		return err
	}

	//命令
	command.Run()

	for _, m := range ms.ModuleGroup.modules {
		err = m.Run(ms.moduleCtx)

		if err != nil {
			mlog.Error("启动模块%s错误%v", m.Name(), err)
		}
	}

	return nil
}

//停止服务器
func (ms *ModuleServer) Stop() {
	ms.ModuleGroup.Range(func(m Module) {
		m.Stop()
	})

	ms.Server.Stop()
	command.Stop()
}
