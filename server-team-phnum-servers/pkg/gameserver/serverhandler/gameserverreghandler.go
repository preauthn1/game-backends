package serverhandler

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	TimerName_ServerReg = "gameserver_serverhandler_gameserverreg"
)

//gameserver 定时注册
type GameServerRegHandler struct {
	reginterval  int
	servertype   int32
	serverid     int32
	svrRegTicker *event.TimerFunc
	LianyunID    int32
}

//创建gameserver定时注册处理器
func NewGameServerRegHandler(interval int, serverid int32, servertype int32, LianyunID int32) *GameServerRegHandler {
	regh := new(GameServerRegHandler)
	regh.reginterval = interval
	regh.serverid = serverid
	regh.servertype = servertype
	regh.LianyunID = LianyunID

	return regh
}

func (rh *GameServerRegHandler) Name() string {
	return "IntervalReg"
}

func (rh *GameServerRegHandler) Start(ctx *GameServerCtx) {
	defer util.PanicErrStack()

	regFunc := func() {
		serverinfo := new(netproto.ServerRegInfo)
		serverinfo.ServerID = proto.Int32(rh.serverid)
		serverinfo.ServerType = proto.Int32(rh.servertype)
		serverinfo.LianyunID = proto.Int32(rh.LianyunID)

		rpcinfo := netproto.CreateEmptyRPCInfo(rh.serverid)

		msgReg := new(msg.Message)
		msgReg.BClassID = int32(netproto.MessageBClassID_ServerMgm)
		msgReg.SClassID = int32(netproto.ServerMgmClassID_GameServerRegister)
		msgReg.MsgData = serverinfo

		ctx.DBMsgQueue.PushMessage(rpcinfo, false, msgReg)
	}

	rh.svrRegTicker = ctx.TimerFuncManager.NewNamedTicker(int32(rh.reginterval), regFunc, TimerName_ServerReg)
}

func (rh *GameServerRegHandler) Stop() {
	if rh.svrRegTicker != nil {
		rh.svrRegTicker.Stop()
	}
}
