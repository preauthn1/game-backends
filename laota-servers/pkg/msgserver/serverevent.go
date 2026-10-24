package msgserver

import (
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

const (
	EVT_CLIENTCONNECT          = "sys_cltconnect"        //客户端连接
	EVT_CLIENTDISCONNECT       = "sys_cltdisconnect"     //客户端断开连接
	EVT_CLIENTMSG              = "sys_cltmsg"            //客户端消息
	EVT_CLIENTDATA             = "sys_cltdata"           //客户端数据
	EVT_RPCMESSAGE             = "sys_rpcmessage"        //rpc消息
	EVT_NOTIFYMESSAGE          = "sys_notifymessage"     //notify 消息
	EVT_ASYNCCALL              = "sys_asynccall"         //异步调用消息
	EVT_HTTPMSG                = "sys_httpmsg"           //http消息
	argsName_ClientInfo        = "sys_clt"               //事件参数：客户端信息
	argsName_ClientData        = "sys_data"              //事件参数,客户端数据
	argsName_ClientMsg         = "sys_msg"               //事件参数,客户端消息
	argsName_RPCMsg            = "sys_rpcmsg"            //事件参数,rpc消息
	argsName_NotifyMsg         = "sys_notifymsg"         //事件参数,notify消息
	argsName_AsyncCallBackFunc = "sys_asynccallbackfunc" //事件参数,异步调用
	argsName_HTTPMsg           = "sys_httpmsg"           //事件采纳数,http消息
)

const (
	HTTPMSGEVTDATA_SEMAPHORE          = "HTTPMSGDATA_SEMAPHORE"
	HTTPMSGEVTDATA_HTTPRESPONSEWRITER = "HTTPMSGEVTDATA_HTTPRESPONSEWRITER"
	HTTPMSGEVTDATA_HTTPREQUEST        = "HTTPMSGEVTDATA_HTTPREQUEST"
)

//客户端连接事件
func NewClientConnectEvt(clt *network.ConnInfo) *event.Event {
	args := event.NewEventArgs()
	args[argsName_ClientInfo] = clt

	evt := event.NewEvent(EVT_CLIENTCONNECT, args)

	return evt
}

//客户端断开事件
func NewClientDisconnectEvt(clt *network.ConnInfo) *event.Event {
	args := event.NewEventArgs()
	args[argsName_ClientInfo] = clt

	evt := event.NewEvent(EVT_CLIENTDISCONNECT, args)

	return evt
}

//客户端消息事件
func NewClientMessageEvent(clt *network.ConnInfo, msg *msg.Message) *event.Event {
	args := event.NewEventArgs()
	args[argsName_ClientMsg] = msg
	args[argsName_ClientInfo] = clt

	evt := event.NewEvent(EVT_CLIENTMSG, args)

	return evt
}

//客户端数据事件
func NewClientDataEvent(clt *network.ConnInfo, data []byte) *event.Event {
	args := event.NewEventArgs()
	args[argsName_ClientData] = data
	args[argsName_ClientInfo] = clt

	evt := event.NewEvent(EVT_CLIENTDATA, args)

	return evt
}

//RPC消息事件
func NewRPCMessageEvent(msg *msg.Message) *event.Event {
	args := event.NewEventArgs()
	args[argsName_RPCMsg] = msg

	evt := event.NewEvent(EVT_RPCMESSAGE, args)

	return evt
}

//通知消息事件
func NewNotifyMessageEvent(msg *msg.Message) *event.Event {
	args := event.NewEventArgs()
	args[argsName_NotifyMsg] = msg

	evt := event.NewEvent(EVT_NOTIFYMESSAGE, args)

	return evt
}

//异步调用消息事件
func NewAsyncCallEvent(callbackFunc func()) *event.Event {
	args := event.NewEventArgs()
	args[argsName_AsyncCallBackFunc] = callbackFunc

	evt := event.NewEvent(EVT_ASYNCCALL, args)

	return evt
}

//http消息事件
func NewHttpMsgEvent(data map[string]interface{}) *event.Event {
	args := event.NewEventArgs()
	args[argsName_HTTPMsg] = data
	evt := event.NewEvent(EVT_HTTPMSG, args)

	return evt
}

//从事件参数中获取客户端信息
func GetClientInfo(args event.EventArgs) *network.ConnInfo {
	return args[argsName_ClientInfo].(*network.ConnInfo)
}

//从事件参数中获取数据
func GetClientData(args event.EventArgs) []byte {
	return args[argsName_ClientData].([]byte)
}

//从事件参数中获取消息
func GetClientMsg(args event.EventArgs) *msg.Message {
	return args[argsName_ClientMsg].(*msg.Message)
}

//从事件参数中获取RPC消息
func GetRPCMsg(args event.EventArgs) *msg.Message {
	return args[argsName_RPCMsg].(*msg.Message)
}

//从事件参数中获取Notify消息
func GetNotifyMsg(args event.EventArgs) *msg.Message {
	return args[argsName_NotifyMsg].(*msg.Message)
}

//从事件参数中获取异步回调函数
func GetAsyncEventCallBackFunc(args event.EventArgs) func() {
	return args[argsName_AsyncCallBackFunc].(func())
}

//从Http消息事件中获取http参数
func GetHttpMsgData(args event.EventArgs) map[string]interface{} {
	return args[argsName_HTTPMsg].(map[string]interface{})
}
