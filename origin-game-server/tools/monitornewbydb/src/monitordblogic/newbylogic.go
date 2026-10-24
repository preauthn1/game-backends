package monitordblogic

import (
	"command"
	"module"
	"monitordblogic/svrmgm"
	"monitordblogic/task"
	"msg"
	"network"
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
	task.MonitorName = ctx.GetConfigData(ConfigFileName_NS, "monitor", "name")
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

	//AB服命令
	//command.AddCommand(svrmgm.NewSvrMasterSlaveCmd(ctx.Server, nslgc.serverList))
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

//注册封包Handler
func (nsglc *NSLogic) registerMsgHandler(ctx *module.MouduleCtx) {
}

func (nsglc *NSLogic) runTask(ctx *module.MouduleCtx) {
	var taskProvider TaskProvider

	//局域网
	if ctx.Config.GetConfig(ConfigFileName_NS).GetConfigValue(ConfigSectionName_NS, "mode") == "1" {
		taskProvider = new(InternalTaskProvider)
	} else { //外网
		taskProvider = new(ExternalTaskPrvider)
	}

	taskctx := task.NewTaskRunCtx(nil, ctx.Server, nsglc.hallUserList)
	taskProvider.GetTask().Run(taskctx)
}

