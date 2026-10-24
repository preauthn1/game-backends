package roomlogic

import (
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"rpcmsg"
	"serverhandler"
)

//刷新版本号处理器
type RefreshVerHandler struct {
	verprovider    *RPCGameVerProvider
	regHallHandler *serverhandler.HallServerRegHandler
}

func NewRefreshVerHandler(verprovider *RPCGameVerProvider, regHallHandler *serverhandler.HallServerRegHandler) *RefreshVerHandler {
	gameverHandler := new(RefreshVerHandler)
	gameverHandler.verprovider = verprovider
	gameverHandler.regHallHandler = regHallHandler

	return gameverHandler
}

func (h *RefreshVerHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_GameVerRetID)
}

func (h *RefreshVerHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.GameVerInfo{}
		return mm
	})

	mm := rmm.(*netproto.GameVerInfo)
	h.verprovider.gsreg.gameServerRegRet.AndroidVersion = mm.AndroidVersion
	h.verprovider.gsreg.gameServerRegRet.IOSVersion = mm.IOSVersion
	//當收到更新版本信息時向大廳更新資料
	h.regHallHandler.RefreshGameVer(mm)
	mlog.NewDefaultLogger().System("收到更新版本信息.android ver:%s,ios ver:%s", mm.GetAndroidVersion(), mm.GetIOSVersion())
}
