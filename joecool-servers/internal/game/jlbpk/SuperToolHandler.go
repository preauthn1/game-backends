package jlbpk

import (
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

type JlbpkSuperToolHandler struct {
	Service       gamelogic.ISuperToolService
	UserList      map[int32]*netproto.JLBPK_SuperTool_Msg_VIPUserData
	Proiority       []int32
}

func NewJlbpkSuperToolHandler(service gamelogic.ISuperToolService) *JlbpkSuperToolHandler {
	provider := new(JlbpkSuperToolHandler)
	provider.Service = service
	provider.UserList = make(map[int32]*netproto.JLBPK_SuperTool_Msg_VIPUserData)

	return provider
}

func (provider *JlbpkSuperToolHandler) GetVipUserOrg(userid int32) proto.Message {
	if ud, ok := provider.UserList[userid]; ok {
		return ud
	} else {
		return nil
	}
}

func (provider *JlbpkSuperToolHandler) SetVipUserList(rmm proto.Message) {
	msg := rmm.(*netproto.JLBPK_SuperTool_Msg_GetVIPUserListRet)
	provider.UserList = make(map[int32]*netproto.JLBPK_SuperTool_Msg_VIPUserData)
	provider.Proiority = []int32{}
	for _, vipuser := range msg.UserList {
		provider.UserList[vipuser.GetUserID()] = vipuser
		provider.Proiority = append(provider.Proiority, vipuser.GetUserID())
	}
}

func (provider *JlbpkSuperToolHandler) IsVipUserExists(userid int32) bool {
	if _, ok := provider.UserList[userid]; ok {
		return true
	} else {
		return false
	}
}

func (provider *JlbpkSuperToolHandler) SaveVipUser(userid int32) {
	if vipuser := provider.GetVipUser(userid); vipuser != nil {
		//mlog.Debug("================================ %d", userid)
		serverid := provider.Service.GetRoomServerID()
		pmsg := new(msg.Message)
		pmsg.BClassID = int32(netproto.MessageBClassID_Game)
		pmsg.SClassID = int32(netproto.JLBPK_GameMessageClassID_JLBPK_SuperTool_UpdateVIPUser)
		data := new(netproto.JLBPK_SuperTool_Msg_UpdateVIPUser)
		data.UserData = new(netproto.JLBPK_SuperTool_Msg_VIPUserData)
		data.UserData.UserID = proto.Int32(userid)
		data.UserData.Finished = proto.Int32(vipuser.GetFinished())
		data.UserData.Weight = proto.Int32(vipuser.GetWeight())
		data.UserData.TargetPoint = proto.Int64(vipuser.GetTargetPoint())
		data.UserData.AccumulatedPoint = proto.Int64(vipuser.GetAccumulatedPoint())
		data.UserData.Status = proto.Int32(vipuser.GetStatus())
		pmsg.MsgData = data

		rpcinfo := netproto.CreateEmptyRPCInfo(serverid)
		//设置RPC消息桌号
		provider.Service.PushRPCMessage(rpcinfo, true, pmsg)
	}
}

func (provider *JlbpkSuperToolHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.JLBPK_GameMessageClassID_JLBPK_SuperTool_GetVIPUserListRet)
}

func (provider *JlbpkSuperToolHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.JLBPK_SuperTool_Msg_GetVIPUserListRet{}
		return mm
	})
	retmsg := rmm.(*netproto.JLBPK_SuperTool_Msg_GetVIPUserListRet)
	provider.SetVipUserList(retmsg)
	mlog.Debug("=====================update %d vip record", len(retmsg.UserList))
}

func (provider *JlbpkSuperToolHandler) GetNextTable(userid int32) int32 {
	if vipuser := provider.GetVipUser(userid); vipuser != nil {
		//if vipuser.GetStatus() == 1 {
		//	return vipuser.GetNextTable()
		//}
	}
	return -1
}

func (provider *JlbpkSuperToolHandler) ResetNextTable(userid int32) {
	if vipuser := provider.GetVipUser(userid); vipuser != nil {
		//vipuser.NextTable = proto.Int32(-1)
	}
}

func (provider *JlbpkSuperToolHandler) GetVipUser(userid int32) *netproto.JLBPK_SuperTool_Msg_VIPUserData {
	ud := provider.GetVipUserOrg(userid)
	if ud != nil {
		return ud.(*netproto.JLBPK_SuperTool_Msg_VIPUserData)
	}
	return nil
}
