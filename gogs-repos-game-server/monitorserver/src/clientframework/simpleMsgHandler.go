package clientframework

import (
	"msg"
)

type SimpleMsgHandler struct {
	bclsid   int32
	sclsid   int32
	callback func(m *msg.Message)
}

func NewSimpleMsgHandler(bclsid int32, sclsid int32, callback func(m *msg.Message)) *SimpleMsgHandler {
	h := new(SimpleMsgHandler)
	h.bclsid = bclsid
	h.sclsid = sclsid
	h.callback = callback
	return h
}

func (h *SimpleMsgHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == h.bclsid && sclsid == h.sclsid
}

func (h *SimpleMsgHandler) OnMessage(client *CtxBase, m *msg.Message) {
	if h.callback != nil {
		h.callback(m)
	}
}

func (h *SimpleMsgHandler) SetCallback(f func(m *msg.Message)) {
	h.callback = f
}
