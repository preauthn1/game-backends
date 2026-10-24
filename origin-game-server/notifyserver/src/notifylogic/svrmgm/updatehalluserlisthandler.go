package svrmgm

import (
	"module"
	"msg"
	"netproto"
	"network"
	"sync"
)

import (
	"example.invalid/golang/protobuf/proto"
)

//服务器注册处理器
type UpdateHallUserListHandler struct {
	hallUserList *sync.Map
}

//创建服务器注册处理器
func NewUpdateHallUserListHandler(hallUserList *sync.Map) *UpdateHallUserListHandler {
	handler := new(UpdateHallUserListHandler)
	handler.hallUserList = hallUserList
	return handler
}

func (h *UpdateHallUserListHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_UpdateHallUserListID)
}

func (h *UpdateHallUserListHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.HallUserList{}
	err := proto.Unmarshal(mmbyte, mm)
	if err != nil {
		ctx.Error("解析notifyserver收到大廳更新線上用戶错误,%v", err)
		ctx.Server.CloseConn(clt)
		return nil
	}
	h.hallUserList.Store(mm.GetServerID(), mm)
	ctx.Debug("收到大廳更新UpdateHallUserList, len: %d", len(mm.UserIDList))
	return nil
}
