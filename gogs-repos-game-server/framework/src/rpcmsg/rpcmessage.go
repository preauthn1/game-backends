package rpcmsg

import (
	"netproto"
)

//远程消息
type RPCMessage struct {
	RPCInfo *netproto.RPCInfo //RPC客户端信息
	MsgData interface{}       //消息内容
}
