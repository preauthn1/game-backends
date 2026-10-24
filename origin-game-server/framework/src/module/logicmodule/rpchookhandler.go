package logicmodule

import (
	"module"
	"msg"
)

type RPCHookHandler interface {
	//响应RPC消息
	OnRPCMessage(msg *msg.Message, ctx *module.MouduleCtx)

	//是否处理
	IsHook(bclsid int32, sclsid int32) bool
}

type RPCHookHandlerGroup struct {
	innerHandlers []RPCHookHandler
}

func NewRPCHookHandlerGroup() *RPCHookHandlerGroup {
	hg := new(RPCHookHandlerGroup)

	return hg
}

func (hg *RPCHookHandlerGroup) IsHook(bclsid int32, sclsid int32) bool {
	return true
}

func (hg *RPCHookHandlerGroup) OnRPCMessage(msg *msg.Message, ctx *module.MouduleCtx) {
	for _, h := range hg.innerHandlers {
		if h.IsHook(msg.BClassID, msg.SClassID) {
			h.OnRPCMessage(msg, ctx)
		}
	}
}

func (hg *RPCHookHandlerGroup) AddHandler(h RPCHookHandler) *RPCHookHandlerGroup {
	hg.innerHandlers = append(hg.innerHandlers, h)

	return hg
}
