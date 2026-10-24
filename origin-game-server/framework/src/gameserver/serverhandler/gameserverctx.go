package serverhandler

import (
	"event"
	"module"
	"msgserver"
	"player"
	"rpcmsg"
)

type GameServerCtx struct {
	PlayerManager    *player.PlayerManager   //用户
	DBMsgQueue       *rpcmsg.RPCMsgQueue     //数据库消息队列
	MsgServer        *msgserver.MsgServer    //消息服务器
	ModuleServer     *module.ModuleServer    //模块服务器
	TimerFuncManager *event.TimerFuncManager //延迟函数管理器
	Throttle         int                     //并发数
}
