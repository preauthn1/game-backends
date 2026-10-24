package notifyhandler

import (
	"halllogic/hallutil"
	"mlog"
	"module"
	"msg"
	"netproto"
	"player"
)

//刷新版本号处理器
type RefreshVerHandler struct {
	verLoader hallutil.HallVerLoader
}

//创建版本号刷新处理器
func NewRefreshVerHandler(verLoader hallutil.HallVerLoader) *RefreshVerHandler {
	refreshVerHandler := new(RefreshVerHandler)
	refreshVerHandler.verLoader = verLoader

	return refreshVerHandler
}

func (ver *RefreshVerHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	example.invalid("收到更新平台版本号消息.开始拉取平台版本号...")
	ver.verLoader.LoadHallVer()
}

func (ver *RefreshVerHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_RefreshVersionID)
}
