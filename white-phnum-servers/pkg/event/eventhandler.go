package event

import (
	"strings"
)

//事件处理器
type EventHandler interface {
	//同步触发事件
	OnEvent(evt *Event, ret *EventRet) bool

	//处理器名称
	HandlerName() string
}

//回调函数
type CallbackFun func(evt *Event, ret *EventRet)

//空的事件处理器
type EmptyEventHandler struct {
}

func (h *EmptyEventHandler) OnEvent(evt *Event, ret *EventRet) bool {
	return true
}

func (h *EmptyEventHandler) HandlerName() string {
	return "EmptyEventHandler"
}

//公用的事件处理器
type CommHandler struct {
	handerName string
	handler    func(evt *Event, ret *EventRet) bool
	hookEvts   []string
}

func (h *CommHandler) OnEvent(evt *Event, ret *EventRet) bool {
	for _, s := range h.hookEvts {
		if strings.ToLower(s) == strings.ToLower(evt.Name) {
			break
		}

		return true
	}

	return h.handler(evt, ret)
}

func (h *CommHandler) HandlerName() string {
	return h.handerName
}

func NewCommonHandler(name string, f func(evt *Event, ret *EventRet) bool, evts ...string) *CommHandler {
	ch := new(CommHandler)
	ch.handerName = name
	ch.handler = f
	ch.hookEvts = evts

	return ch
}
