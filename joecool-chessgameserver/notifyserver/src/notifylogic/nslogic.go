package notifylogic

import (
	"command"
	"conf"
	"module"
	"msg"
	"network"
	"notifylogic/svrmgm"
	"notifylogic/task"
	"strings"
	"sync"
)

//配置文件名
const (
	ConfigFileName_NS = "ns"
)

//配置节名称
const (
	ConfigSectionName_Conn = "conn"
	ConfigSectionName_NS   = "ns"
)

//通知/协调服务器逻辑
type NSLogic struct {
	ctx        *module.MouduleCtx
	msgHandler *module.HookMsgHandlerGroup
	serverList *svrmgm.ServerList
	hallUserList *sync.Map
}

//创建通知程序逻辑
func NewNSLogic(severList *svrmgm.ServerList) *NSLogic {
	nslogic := new(NSLogic)
	nslogic.msgHandler = module.NewHookMsgHandlerGroup()
	nslogic.serverList = severList
	nslogic.hallUserList = new(sync.Map)
	//加入命令行
	//服务器查询命令
	command.AddCommand(svrmgm.NewSvrMgmCmd(severList))

	return nslogic
}

func (nslgc *NSLogic) Run(ctx *module.MouduleCtx) error {
	//设置数据库连接
	task.ConnString = ctx.GetConfigData(ConfigFileName_NS, ConfigSectionName_Conn, "connstring")
	task.ConnString = strings.Replace(task.ConnString, "\"", "", -1)
	//初始化数据库
	task.InitDatabase()

	//注册协议解析器
	nslgc.registerMsgParser(ctx)

	//注册事件处理器
	nslgc.registerEvtHandler(ctx)

	//注册消息处理器
	nslgc.registerMsgHandler(ctx)

	//运行任务
	nslgc.runTask(ctx)

	//停服命令
	command.AddCommand(svrmgm.NewSvrStopCmd(ctx.Server, nslgc.serverList))
	command.AddCommand(svrmgm.NewServerCtlCmd(ctx.Server, nslgc.serverList))
	//关服命令
	command.AddCommand(svrmgm.NewSvrCloseCmd(ctx.Server, nslgc.serverList))
	//刷新版本号命令
	command.AddCommand(svrmgm.NewRefreshVerCmd(ctx.Server, nslgc.serverList))
	//停服白名单命令
	command.AddCommand(svrmgm.NewServerStopWhiteIPCmd(ctx.Server, nslgc.serverList))
	//AB服命令
	command.AddCommand(svrmgm.NewSvrMasterSlaveCmd(ctx.Server, nslgc.serverList))

	return nil
}

func (nslgc *NSLogic) Stop() {
	nslgc.ctx.Server.Stop()
}

func (nsglc *NSLogic) Name() string {
	return "nslogic"
}

func (nsglc *NSLogic) registerMsgParser(ctx *module.MouduleCtx) {
	ctx.RegisterMsgParser(msg.AllMsgBClassID, msg.AllMsgSClassID, msg.NewProtoMsgParser())
}

func (nsglc *NSLogic) registerEvtHandler(ctx *module.MouduleCtx) {
	ctx.OnConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s connnect...", clt)
		return true
	})
	ctx.OnDisConnect(func(clt *network.ConnInfo) bool {
		example.invalid("client %s disconnnect...", clt)
		server := nsglc.serverList.GetServerByConn(clt)
		if server.ServerType == conf.ServerType_HallServer {
			nsglc.hallUserList.Delete(int32(server.ServerID))
		}

		//移除服务器信息
		nsglc.serverList.RemoveServerByConn(clt)

		return true
	})
	ctx.OnMessage(func(clt *network.ConnInfo, m *msg.Message) bool {
		example.invalid("client %s msg:%d_%d", clt, m.BClassID, m.SClassID)

		retmsgs := nsglc.msgHandler.OnMessage(ctx, clt, m)

		if retmsgs != nil {
			for _, retmsg := range retmsgs {
				ctx.Server.SendMessage(clt, retmsg)
			}
		}

		return true
	})
}

func (nsglc *NSLogic) registerMsgHandler(ctx *module.MouduleCtx) {
	nsglc.msgHandler.AddHandler(svrmgm.NewServerRegHandler(nsglc.serverList, ctx.Server))
	nsglc.msgHandler.AddHandler(svrmgm.NewUpdateHallUserListHandler(nsglc.hallUserList))
	nsglc.msgHandler.AddHandler(task.NewUpdateUserNotifyHandler())
	taskctx := task.NewTaskRunCtx(nsglc.serverList, ctx.Server, nsglc.hallUserList)
	nsglc.msgHandler.AddHandler(task.NewFishHandler(taskctx))

}

func (nsglc *NSLogic) runTask(ctx *module.MouduleCtx) {
	var taskProvider TaskProvider

	//局域网
	if ctx.Config.GetConfig(ConfigFileName_NS).GetConfigValue(ConfigSectionName_NS, "mode") == "1" {
		taskProvider = new(InternalTaskProvider)
	} else { //外网
		taskProvider = new(ExternalTaskPrvider)
	}

	taskctx := task.NewTaskRunCtx(nsglc.serverList, ctx.Server, nsglc.hallUserList)
	taskProvider.GetTask().Run(taskctx)
}
