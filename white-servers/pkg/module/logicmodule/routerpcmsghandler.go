package logicmodule

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

//rpc转发消息处理器
type RouteRPCMsgHandler struct {
	hookClientMsgID []*msg.RPCMessageID //拦截客户端消息ID
	playerManager   *player.PlayerManager

	msgqueue *rpcmsg.RPCMsgQueue
	serverid int32
}

//创建rpc消息处理器对象
func NewRouteRPCMsgHandler(queue *rpcmsg.RPCMsgQueue, ctx *module.MouduleCtx, pm *player.PlayerManager, hookmsgid []*msg.RPCMessageID, serverid int32) *RouteRPCMsgHandler {
	h := new(RouteRPCMsgHandler)
	h.msgqueue = queue
	h.hookClientMsgID = hookmsgid
	h.serverid = serverid
	h.playerManager = pm

	return h
}

//是否转发客户端消息
func (rpch *RouteRPCMsgHandler) checkRCPMsg(bclsid int32, sclsid int32, clt *network.ConnInfo) bool {
	for _, id := range rpch.hookClientMsgID {
		if id.MsgID.BClassID == bclsid && id.MsgID.SClassID == sclsid {
			//不需要登录
			if !id.NeedLogon {
				return true
			} else { //需要登录
				pr := rpch.playerManager.GetPlayerByConnID(clt.ConnID())

				if pr == nil || pr.IsGuest() {
					return false
				}

				return true
			}
		}
	}

	return false
}

//是否拦截客户端消息
func (rpch *RouteRPCMsgHandler) isHookClientMsg(bclsid int32, sclsid int32) bool {
	for _, id := range rpch.hookClientMsgID {
		if id.MsgID.BClassID == bclsid && id.MsgID.SClassID == sclsid {
			return true
		}
	}

	return false
}

func (rpch *RouteRPCMsgHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	//客户端消息
	if rpch.checkRCPMsg(m.BClassID, m.SClassID, clt) {
		//example.invalid("recv rpc message %v,send to dbserver", m.MsgData)

		rpcinfo := new(netproto.RPCInfo)

		//获取用户信息
		pr := rpch.playerManager.GetPlayerByConnID(clt.ConnID())
		if pr != nil && !pr.IsGuest() {
			rpcinfo.UserID = proto.Int32(pr.UserID)
			rpcinfo.Cer = proto.String(pr.Cer)
		}

		rpcinfo.ConnID = proto.String(clt.ConnID())
		rpcinfo.IPAddress = proto.String(clt.IPAddress)
		rpcinfo.RouteServerID = proto.Int32(rpch.serverid)
		rpcinfo.ConnToken = [REDACTED]

		rpch.msgqueue.PushMessage(rpcinfo, false, m)

	}
	return nil
}

func (rpch *RouteRPCMsgHandler) IsHook(bclsid int32, sclsid int32) bool {
	ret := rpch.isHookClientMsg(bclsid, sclsid)

	return ret
}
