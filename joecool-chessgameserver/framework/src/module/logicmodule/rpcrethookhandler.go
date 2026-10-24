package logicmodule

import (
	"module"
	"msg"
	"rpcmsg"
)

//rpc调用结果转发
type RPCRetHookHandler struct {
	hookClientMsgID []*msg.MessageID //拦截客户端消息ID
}

//创建RPC 结果处理器
func NewRPCRetHookHandler(hkmsgid []*msg.MessageID) *RPCRetHookHandler {
	reth := new(RPCRetHookHandler)
	reth.hookClientMsgID = hkmsgid

	return reth
}

func (h *RPCRetHookHandler) IsHook(bclsid int32, sclsid int32) bool {
	for _, mid := range h.hookClientMsgID {
		if mid.BClassID == bclsid && mid.SClassID == sclsid {
			return true
		}
	}

	return false
}

func (h *RPCRetHookHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	rmm := m.MsgData.(*rpcmsg.RPCMessage)

	retmsg := new(msg.Message)
	retmsg.BClassID = m.BClassID
	retmsg.SClassID = m.SClassID
	retmsg.MsgData = rmm.MsgData

	example.invalid("send rpc message to client %v_%v,%v,%v", retmsg.BClassID, retmsg.SClassID, rmm.RPCInfo, retmsg)
	ctx.Server.SendMessageByConnID(rmm.RPCInfo.GetConnID(), retmsg)
}
