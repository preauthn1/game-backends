package gamelogic

import (
	"net/http"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

// 空的超端服務
type EmptySuperToolService struct {
	superToolHandler ISuperToolHandler
}

func NewEmptySuperToolService() ISuperToolService {
	s := new(EmptySuperToolService)
	s.superToolHandler = new(EmptySuperToolHandler)
	return s
}
func (s *EmptySuperToolService) Start() {}
func (s *EmptySuperToolService) Stop()  {}
func (s *EmptySuperToolService) GetSuperToolHandler() ISuperToolHandler {
	return s.superToolHandler
}

func (s *EmptySuperToolService) RefreshVIPUser() {}
func (s *EmptySuperToolService) GetRoomServerID() int32 {
	return -1
}
func (s *EmptySuperToolService) PushRPCMessage(rpcinfo *netproto.RPCInfo, safe bool, msg *msg.Message) error {
	return nil
}
func (s *EmptySuperToolService) HttpService(w http.ResponseWriter, req *http.Request) {}

type EmptySuperToolHandler struct {
}

func (h *EmptySuperToolHandler) GetVipUserOrg(userid int32) proto.Message {
	return nil
}
func (h *EmptySuperToolHandler) SetVipUserList(list proto.Message) {}
func (h *EmptySuperToolHandler) IsVipUserExists(userid int32) bool {
	return false
} //用戶是否存在

func (h *EmptySuperToolHandler) SaveVipUser(userid int32) {}

func (h *EmptySuperToolHandler) IsHook(bclsid int32, sclsid int32) bool {
	return false
}
func (h *EmptySuperToolHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {}

var EmptySuperToolCreator = func(room *Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) ISuperToolService {
	return NewEmptySuperToolService()
}

func (h *EmptySuperToolHandler) GetNextTable(userid int32) int32 {
	return -1
}

func (h *EmptySuperToolHandler) ResetNextTable(userid int32) {}
