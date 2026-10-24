package jlbpk

import (
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"net/http"
)

type JlbpkSuperToolService struct {
	Room             *gamelogic.Room
	tfManager        *event.TimerFuncManager
	DBMsgQueue       *rpcmsg.RPCMsgQueue
	UpdateTicker     *event.TimerFunc
	superToolHandler gamelogic.ISuperToolHandler
	fapaiRule		int32				//發牌規則 1 大牌規則, 2 一般規則
}

func NewJlbpkSuperToolService(room *gamelogic.Room, tfm *event.TimerFuncManager, dbMsgQueue *rpcmsg.RPCMsgQueue) *JlbpkSuperToolService {
	service := new(JlbpkSuperToolService)
	service.Room = room
	service.tfManager = tfm
	service.DBMsgQueue = dbMsgQueue
	service.superToolHandler = NewJlbpkSuperToolHandler(service)
	service.fapaiRule = 2
	return service
}

func (service *JlbpkSuperToolService) Start() {
	service.RefreshVIPUser()
	service.UpdateTicker = service.tfManager.NewTicker(10, service.RefreshVIPUser)
}

func (service *JlbpkSuperToolService) Stop() {
	service.UpdateTicker.Stop()
}

func (service *JlbpkSuperToolService) GetSuperToolHandler() gamelogic.ISuperToolHandler {
	return service.superToolHandler
}

func (service *JlbpkSuperToolService) GetJlbpkSuperToolHandler() *JlbpkSuperToolHandler {
	return service.superToolHandler.(*JlbpkSuperToolHandler)
}

func (service *JlbpkSuperToolService) RefreshVIPUser() {
	//m := msg.NewMessage(int32(netproto.MessageBClassID_DBServer), int32(netproto.DBServerClassID_SuperToolGetVipUserID), rpcMessage)
	serverid := service.Room.ServerID
	pmsg := new(msg.Message)
	pmsg.BClassID = int32(netproto.MessageBClassID_Game)
	pmsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_JLBPK_SuperTool_GetVIPUserList)
	pmsg.MsgData = nil

	rpcinfo := netproto.CreateEmptyRPCInfo(serverid)
	//设置RPC消息桌号
	service.DBMsgQueue.PushMessage(rpcinfo, false, pmsg)
}

func (service *JlbpkSuperToolService) GetRoomServerID() int32{
	return service.Room.ServerID
}

func (service *JlbpkSuperToolService) PushRPCMessage(rpcinfo *netproto.RPCInfo, safe bool, msg *msg.Message) error {
	return service.DBMsgQueue.PushMessage(rpcinfo, safe, msg)
}

func (service *JlbpkSuperToolService) HttpService(w http.ResponseWriter, req *http.Request) {
	//mlog.Debug("==================== httpservice")
	//action := req.PostFormValue("action")
	//tableno := util.StrToInt32(req.PostFormValue("tableno"))
	//var ret proto.Message
	//if action == "getserverinfo" {
	//	//mlog.Debug("================getserverinfo")
	//	ret = service.getServerInfo(tableno)
	//} else if action == "getassigntablelist"  {
	//	ret = service.getAssignTableList()
	//}
	//
	//if ret != nil {
	//	data, _ := proto.Marshal(ret)
	//	//mlog.Debug("================%s", data)
	//	if data != nil {
	//		encodeStr := base64.StdEncoding.EncodeToString(data)
	//		//mlog.Debug("+++++++++++++++++++ %s", encodeStr)
	//		io.WriteString(w, encodeStr)
	//	}
	//} else {
	//	io.WriteString(w, "ok")
	//}

}
