package clientframework

import (
	"msg"
)

//拦截消息处理器
type HookMsgHandler interface {
	//处理消息
	OnMessage(client *CtxBase, m *msg.Message)

	//是否处理
	IsHook(bclsid int32, sclsid int32) bool

	SetCallback(f func(m *msg.Message))
}

type HookMsgHandlerGroup struct {
	innerHandlers []HookMsgHandler
}

func NewHookMsgHandlerGroup() *HookMsgHandlerGroup {
	hg := new(HookMsgHandlerGroup)
	return hg
}
func (hlg *HookMsgHandlerGroup) IsHook(bclsid int, sclsid int) bool {
	return true
}

func (hlg *HookMsgHandlerGroup) OnMessage(client *CtxBase, m *msg.Message) []*msg.Message {
	var msgs []*msg.Message

	for _, h := range hlg.innerHandlers {
		if h.IsHook(m.BClassID, m.SClassID) {
			h.OnMessage(client, m)
		}
	}

	return msgs
}

func (hlg *HookMsgHandlerGroup) AddHandler(h HookMsgHandler) *HookMsgHandlerGroup {
	for _, exh := range hlg.innerHandlers {
		if exh == h {
			return hlg
		}
	}

	hlg.innerHandlers = append(hlg.innerHandlers, h)

	return hlg
}

func (hlg *HookMsgHandlerGroup) SetCallback(bclsid int32, sclsid int32, cb func(m *msg.Message)) {
	for _, h := range hlg.innerHandlers {
		if h.IsHook(bclsid, sclsid) {
			h.SetCallback(cb)
		}
	}
}

func (hlg *HookMsgHandlerGroup) RemoveSimpleHandler(handler *SimpleMsgHandler) {
	for i, h := range hlg.innerHandlers {
		if h.IsHook(handler.bclsid, handler.sclsid) {
			a := hlg.innerHandlers[:i]
			b := hlg.innerHandlers[i+1:]
			hlg.innerHandlers = append(a, b...)
		}
	}
}
