package blackip

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

//rpc ip黑名单处理器
type RPCIPBlackListHandler struct {
	msgqueue               *rpcmsg.RPCMsgQueue
	loadIPListCallBackFunc func(map[string]string)
	serverid               int32
	serverDesc             string
}

//创建IP黑名单处理器对象
func NewRPCIPBlackListHandler(msgqueue *rpcmsg.RPCMsgQueue, svrid int32, serverDesc string) *RPCIPBlackListHandler {
	rpciplist := new(RPCIPBlackListHandler)

	rpciplist.msgqueue = msgqueue
	rpciplist.serverid = svrid
	rpciplist.serverDesc = serverDesc
	return rpciplist
}

//封锁IP
func (rpciplist *RPCIPBlackListHandler) BlockIP(ip string, sec int, memo string) {
	memo = memo + "," + rpciplist.serverDesc

	blockmsg := new(msg.Message)
	blockmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	blockmsg.SClassID = int32(netproto.DBServerClassID_BlockIPID)

	blockdata := new(netproto.BlockIP)
	blockdata.IPAddress = proto.String(ip)
	blockdata.Sec = proto.Int32(int32(sec))
	blockdata.Memo = proto.String(memo)
	blockmsg.MsgData = blockdata

	rpciplist.msgqueue.PushMessage(netproto.CreateEmptyRPCInfo(rpciplist.serverid), false, blockmsg)

}

func (rpciplist *RPCIPBlackListHandler) LoadBlockIPList(callbackFunc func(map[string]string)) {
	rpciplist.loadIPListCallBackFunc = callbackFunc

	rpciplist.loadBlockIP()
}

//加载IP黑名单
func (rpciplist *RPCIPBlackListHandler) loadBlockIP() {
	loadMsg := new(msg.Message)
	loadMsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	loadMsg.SClassID = int32(netproto.DBServerClassID_LoadBlockIPID)
	loadMsg.MsgData = nil

	rpciplist.msgqueue.PushMessage(netproto.CreateEmptyRPCInfo(rpciplist.serverid), false, loadMsg)
}

func (rpciplist *RPCIPBlackListHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.BlockIPList{}
		return mm
	})

	mm := rmm.(*netproto.BlockIPList)

	if mm == nil {
		return
	}

	iplst := make(map[string]string)

	for _, sip := range mm.GetIPAddress() {
		iplst[sip] = sip
	}

	if rpciplist.loadIPListCallBackFunc != nil {
		rpciplist.loadIPListCallBackFunc(iplst)
	}

}

func (rpciplist *RPCIPBlackListHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_BlockIPListID)
}
