package rpcmsg

import (
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

//远程消息
type RPCMessage struct {
	RPCInfo *netproto.RPCInfo //RPC客户端信息
	MsgData interface{}       //消息内容
}
