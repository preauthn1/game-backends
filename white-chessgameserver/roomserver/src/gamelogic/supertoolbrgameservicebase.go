package gamelogic

import (
	"netproto"
	"msg"
	"net/http"
	"rpcmsg"
	"event"
	"example.invalid/golang/protobuf/proto"
)

// 百人遊戲超端服務基類
type SuperToolBrGameServiceBase struct {
	Room             *Room
	tfManager        *event.TimerFuncManager
	DBMsgQueue       *rpcmsg.RPCMsgQueue
	UpdateTicker     *event.TimerFunc
	superToolHandler ISuperToolHandler
	RefreshInterval	 int32
}

func NewBrGameSuperToolServiceBase(room *Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) ISuperToolService {
	service := new(SuperToolBrGameServiceBase)
	service.Room = room
	service.tfManager = tfm
	service.DBMsgQueue = dbMsgQueue
	service.superToolHandler = NewBrGameSuperToolHandlerBase(service)
	service.RefreshInterval = 30
	return service
}
func (service *SuperToolBrGameServiceBase) Start() {
	service.RefreshVIPUser()
	service.UpdateTicker = service.tfManager.NewNamedTicker(30, service.RefreshVIPUser, "timer_refresh_vip_user")
}
func (service *SuperToolBrGameServiceBase) Stop() {
	service.UpdateTicker.Stop()
}
func (service *SuperToolBrGameServiceBase) GetSuperToolHandler() ISuperToolHandler {
	return service.superToolHandler
}
func (service *SuperToolBrGameServiceBase) GetBrGameSuperToolHandler() *SuperToolBrGameHandlerBase {
	return service.superToolHandler.(*SuperToolBrGameHandlerBase)
}

func (service *SuperToolBrGameServiceBase) RefreshVIPUser() {
	gameid := service.Room.GameInfo.GameID
	serverid := service.Room.ServerID
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.GameMessageClassID_SuperToolGetVipUserID)
	pdata := new(netproto.SuperToolGetVipUserRequest)
	pdata.GameID = proto.Int32(gameid)
	pmsg.MsgData = pdata

	rpcinfo := netproto.CreateEmptyRPCInfo(serverid)
	//设置RPC消息桌号
	service.DBMsgQueue.PushMessage(rpcinfo, false, pmsg)
}
func (service *SuperToolBrGameServiceBase) GetRoomServerID() int32{
	return service.Room.ServerID
}
func (service *SuperToolBrGameServiceBase) PushRPCMessage(rpcinfo *netproto.RPCInfo, safe bool, msg *msg.Message) error {
	return service.DBMsgQueue.PushMessage(rpcinfo, safe, msg)
}
func (service *SuperToolBrGameServiceBase) HttpService(w http.ResponseWriter, req *http.Request) {

}
