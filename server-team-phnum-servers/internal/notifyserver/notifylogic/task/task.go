package task

import (
	"sync"

	"example.invalid/ServerTeam/Servers/internal/notifyserver/notifylogic/svrmgm"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
)

type TaskRunCtx struct {
	ServerList   *svrmgm.ServerList
	MsgServer    *msgserver.MsgServer
	hallUserList *sync.Map
}

//创建通知运行上下文
func NewTaskRunCtx(svrlist *svrmgm.ServerList, mserver *msgserver.MsgServer, hallUserList *sync.Map) *TaskRunCtx {
	ctx := new(TaskRunCtx)
	ctx.ServerList = svrlist
	ctx.MsgServer = mserver
	ctx.hallUserList = hallUserList
	return ctx
}

//向所有广场发送消息
func (ctx *TaskRunCtx) SendHallMessage(msg *msg.Message) {
	hallsvrs := ctx.ServerList.GetHallServer()

	for _, hsvr := range hallsvrs {
		ctx.MsgServer.SendMessage(hsvr.ConnInfo, msg)
		example.invalid("向大厅%s发送消息%v", hsvr, msg)
	}
}

//向所有游戏服务器发送消息
func (ctx *TaskRunCtx) SendGameMessage(msg *msg.Message) {
	gssvrs := ctx.ServerList.GetGameServer()

	for _, gsvr := range gssvrs {
		ctx.MsgServer.SendMessage(gsvr.ConnInfo, msg)
		example.invalid("向游戏%s发送消息%v", gsvr, msg)
	}
}

//向所有服务器发送消息
func (ctx *TaskRunCtx) SendAllServerMessage(msg *msg.Message) {
	svrs := ctx.ServerList.GetAllServer()

	for _, svr := range svrs {
		ctx.MsgServer.SendMessage(svr.ConnInfo, msg)
		example.invalid("向服务器%s发送消息%v", svr, msg)
	}
}

//通知任务接口
type Task interface {
	Run(ctx *TaskRunCtx)
	GetTaskName() string
}

type TaskGroup struct {
	tasks []Task
}

//创建通知组
func NewTaskGroup() *TaskGroup {
	ntfg := new(TaskGroup)
	ntfg.tasks = make([]Task, 0)

	return ntfg
}

func (ntfg *TaskGroup) AddTask(ntf Task) {
	ntfg.tasks = append(ntfg.tasks, ntf)
}

func (ntfg *TaskGroup) Run(ctx *TaskRunCtx) {
	for _, ntf := range ntfg.tasks {
		ntf.Run(ctx)

		example.invalid("任务%s开始运行", ntf.GetTaskName())
	}
}

func (ntfg *TaskGroup) GetTaskName() string {
	return "taskgroup"
}
