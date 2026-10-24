package gamelogic

import (
	"fmt"
	"sort"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

//百人超端基類
type SuperToolBrGameHandlerBase struct {
	Service         ISuperToolService
	UserList        map[int32]*netproto.SuperToolVipUserData
	Proiority       []int32
	HistoryCardType []int
}

func NewBrGameSuperToolHandlerBase(service ISuperToolService) *SuperToolBrGameHandlerBase {
	provider := new(SuperToolBrGameHandlerBase)
	provider.Service = service
	provider.UserList = make(map[int32]*netproto.SuperToolVipUserData)
	return provider
}

func (h *SuperToolBrGameHandlerBase) GetVipUserOrg(userid int32) proto.Message {
	if ud, ok := h.UserList[userid]; ok {
		return ud
	} else {
		return nil
	}
}

func (h *SuperToolBrGameHandlerBase) SetVipUserList(rmm proto.Message) {
	// 因為是事件驅動的關係, 在此不用考慮多執行緒問題
	msg := rmm.(*netproto.SuperToolVipUserDataList)
	// 更新vip 監控名單
	h.UserList = make(map[int32]*netproto.SuperToolVipUserData)
	h.Proiority = []int32{}
	for _, vipuser := range msg.DataList {
		userid := vipuser.GetUserID()
		h.UserList[userid] = vipuser
		h.Proiority = append(h.Proiority, userid)
	}

	sort.Slice(h.Proiority, func(i, j int) bool {
		iuser := h.GetVipUser(h.Proiority[i])
		juser := h.GetVipUser(h.Proiority[j])
		if iuser.GetAssignAmount() > 0 && juser.GetAssignAmount() > 0 { //白名單
			if iuser.GetAssignRate() == juser.GetAssignRate() {
				return iuser.GetAssignAmount() > juser.GetAssignAmount()
			} else {
				return iuser.GetAssignRate() > juser.GetAssignRate()
			}
		} else if iuser.GetAssignAmount() < 0 && juser.GetAssignAmount() < 0 { //黑名單
			if iuser.GetAssignRate() == juser.GetAssignRate() {
				return iuser.GetAssignAmount() > juser.GetAssignAmount()
			} else {
				return iuser.GetAssignRate() < juser.GetAssignRate()
			}
		} else {
			return iuser.GetAssignAmount() > juser.GetAssignAmount()
		}
	})
}

func (h *SuperToolBrGameHandlerBase) IsVipUserExists(userid int32) bool {
	if _, ok := h.UserList[userid]; ok {
		return true
	} else {
		return false
	}
}

func (h *SuperToolBrGameHandlerBase) SaveVipUser(userid int32) {
	serverid := h.Service.GetRoomServerID()
	if vipuser := h.GetVipUser(userid); vipuser != nil {
		mlog.Debug("update user completeamount %d, userid %d", vipuser.GetCompleteAmount(), vipuser.GetUserID())
		pmsg := new(msg.Message)
		pmsg.BClassID = int32(netproto.MessageBClassID_Game)
		pmsg.SClassID = int32(netproto.GameMessageClassID_SuperToolUpdVipUserID)
		pmsg.MsgData = vipuser
		rpcinfo := netproto.CreateEmptyRPCInfo(serverid)
		h.Service.PushRPCMessage(rpcinfo, true, pmsg)
	}
}

func (h *SuperToolBrGameHandlerBase) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_SuperToolGetVipUserRetID)
}
func (h *SuperToolBrGameHandlerBase) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SuperToolVipUserDataList{}
		return mm
	})
	retmsg := rmm.(*netproto.SuperToolVipUserDataList)
	h.SetVipUserList(retmsg)
	mlog.Debug("=====================update %d vip record", len(retmsg.DataList))
}

func (h *SuperToolBrGameHandlerBase) GetNextTable(userid int32) int32 {
	return -1
}

func (h *SuperToolBrGameHandlerBase) ResetNextTable(userid int32) {}

func (h *SuperToolBrGameHandlerBase) GetVipUser(userid int32) *netproto.SuperToolVipUserData {
	ud := h.GetVipUserOrg(userid)
	if ud != nil {
		return ud.(*netproto.SuperToolVipUserData)
	}
	return nil
}

func (h *SuperToolBrGameHandlerBase) GetProiorityListString() string {
	var s string
	for _, userid := range h.Proiority {
		if vipuser := h.GetVipUser(userid); vipuser != nil {
			s += fmt.Sprintf("[%d|%d|%d],", vipuser.GetUserID(), vipuser.GetAssignAmount(), vipuser.GetAssignRate())
		}
	}
	return s
}
