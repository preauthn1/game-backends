package gamelogic

import (
	"netproto"
	"msg"
	"net/http"
	"example.invalid/golang/protobuf/proto"
	"module"
)

// 超端介面
type ISuperToolService interface {
	Start()                               //啟動服務
	Stop()                                //停止服務
	GetSuperToolHandler() ISuperToolHandler //獲取監控用戶資料
	RefreshVIPUser()                      //刷新監控用戶資料
	GetRoomServerID() int32				  //獲取房間ID
	PushRPCMessage(rpcinfo *netproto.RPCInfo, safe bool, msg *msg.Message) error
	HttpService(w http.ResponseWriter, req *http.Request) //HTTP 服務
}

type ISuperToolHandler interface {
	GetVipUserOrg(userid int32) proto.Message //獲取用戶
	SetVipUserList(list proto.Message)        //更新vip 用戶
	IsVipUserExists(userid int32) bool        //用戶是否存在
	IsHook(bclsid int32, sclsid int32) bool
	OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) //接收vip 用戶資料
	SaveVipUser(userid int32) //用戶存檔
	GetNextTable(userid int32) int32 //獲取指定桌號
	ResetNextTable(userid int32) //重設指定桌號
}
