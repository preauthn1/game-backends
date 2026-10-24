package module

import (
	"msg"
	"network"
	"util"
)

//拦截消息处理器
type HookMsgHandler interface {
	//处理消息
	OnMessage(ctx *MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message

	//是否处理
	IsHook(bclsid int32, sclsid int32) bool
}

//拦截消息处理器组
type HookMsgHandlerGroup struct {
	innerHandlers []HookMsgHandler
}

//创建拦截消息处理器组
func NewHookMsgHandlerGroup() *HookMsgHandlerGroup {
	hmg := new(HookMsgHandlerGroup)

	return hmg
}

func (hlg *HookMsgHandlerGroup) IsHook(bclsid int, sclsid int) bool {
	return true
}

func (hlg *HookMsgHandlerGroup) OnMessage(ctx *MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	var msgs []*msg.Message

	for _, h := range hlg.innerHandlers {
		if h.IsHook(m.BClassID, m.SClassID) {
			msg := h.OnMessage(ctx, clt, m)

			if msg != nil {
				msgs = append(msgs, msg...)
			}
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

//限流的拦截消息处理器组
type SemaphoreHookMsgHandlerGroup struct {
	*HookMsgHandlerGroup
	semaphore util.Semaphore
}

//创建限流的拦截消息处理器组对象
func NewSemaphoreHookMsgHandlerGroup(n int) *SemaphoreHookMsgHandlerGroup {
	shlg := new(SemaphoreHookMsgHandlerGroup)
	shlg.semaphore = util.NewSemaphore(n)
	shlg.HookMsgHandlerGroup = new(HookMsgHandlerGroup)

	return shlg
}

func (hlg *SemaphoreHookMsgHandlerGroup) OnMessage(ctx *MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	hlg.semaphore.Acquire()
	defer hlg.semaphore.Release()

	return hlg.HookMsgHandlerGroup.OnMessage(ctx, clt, m)
}
