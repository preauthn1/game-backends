package roomlogic

import (
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"player"
	"rpcmsg"
)

//刷新版本号处理器
type NotifyRefreshVerHandler struct {
	rpcmsgqueue *rpcmsg.RPCMsgQueue
	gameid      int32
	serverid    int32
}

//创建版本号刷新处理器
func NewNotifyRefreshVerHandler(rpcmsgqueue *rpcmsg.RPCMsgQueue, gameid int32, serverid int32) *NotifyRefreshVerHandler {

	refreshVerHandler := new(NotifyRefreshVerHandler)
	refreshVerHandler.rpcmsgqueue = rpcmsgqueue
	refreshVerHandler.gameid = gameid
	refreshVerHandler.serverid = serverid

	return refreshVerHandler
}

func (ver *NotifyRefreshVerHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	mlog.System("收到更新版本号消息.开始拉取版本号...")
	svrmsg := new(msg.Message)
	svrmsg.BClassID = int32(netproto.MessageBClassID_DBServer)
	svrmsg.SClassID = int32(netproto.DBServerClassID_GameVerID)

	svrdata := new(netproto.GameVer)
	svrdata.GameID = proto.Int32(ver.gameid)
	svrdata.ServerID = proto.Int32(ver.serverid)

	svrmsg.MsgData = svrdata

	ver.rpcmsgqueue.PushMessage(netproto.CreateEmptyRPCInfo(ver.serverid), false, svrmsg)
}

func (ver *NotifyRefreshVerHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_RefreshVersionID)
}
