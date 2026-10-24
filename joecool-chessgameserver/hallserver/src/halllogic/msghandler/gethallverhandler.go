package msghandler

import (
	"halllogic/hallutil"
	"module"
	"msg"
	"netproto"
	"network"
	"example.invalid/golang/protobuf/proto"
	"mlog"
)

type GetHallVerHandler struct {
	verprovider hallutil.HallVerProvider
}

func NewGetHallVerHandler(verprovider hallutil.HallVerProvider) *GetHallVerHandler {
	hallverHandler := new(GetHallVerHandler)
	hallverHandler.verprovider = verprovider

	return hallverHandler
}

func (h *GetHallVerHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	areaid := int64(0)

	dd := m.MsgData.([]byte)
	mm := &netproto.HallVerReq{}

	err := proto.Unmarshal(dd, mm)
	if err == nil {
		areaid = mm.GetAreaId()
	}
	
	mlog.Debug("client %v request hall ver from areaid %d", clt, areaid)

	vermsg := new(msg.Message)
	vermsg.BClassID = int32(netproto.MessageBClassID_Hall)
	vermsg.SClassID = int32(netproto.HallMsgClassID_HallVerInfoID)

	ver := h.verprovider.GetVer(areaid)
	vermsg.MsgData = ver

	return []*msg.Message{vermsg}
}

func (h *GetHallVerHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_RequestHallVerID)
}
