package logicmodule

import (
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/player"
)

//通知消息处理器
type NotifyHookHandler interface {
	//响应Notify消息
	OnNotifyMessage(msg *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager)

	//是否处理
	IsHook(bclsid int32, sclsid int32) bool
}

//通知消息处理器组
type NotifyHookHandlerGroup struct {
	handlers []NotifyHookHandler
}

func NewNotifyHookHandlerGroup() *NotifyHookHandlerGroup {
	ntfhg := new(NotifyHookHandlerGroup)
	ntfhg.handlers = make([]NotifyHookHandler, 0)

	return ntfhg
}

func (ntfg *NotifyHookHandlerGroup) IsHook(bclsid int32, sclsid int32) bool {
	return true
}

func (ntfg *NotifyHookHandlerGroup) OnNotifyMessage(msg *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {

	for _, h := range ntfg.handlers {
		if h.IsHook(msg.BClassID, msg.SClassID) {
			h.OnNotifyMessage(msg, ctx, player)
		}
	}
}

func (ntfg *NotifyHookHandlerGroup) AddHandler(h NotifyHookHandler) *NotifyHookHandlerGroup {
	ntfg.handlers = append(ntfg.handlers, h)

	return ntfg
}
