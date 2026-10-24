package rpchandler

import (
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
)

import (
	"fmt"
	proto "example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

type LogonRetHandler struct {
	playerManager    *player.PlayerManager
	onUserLogOutFunc func(int32, string)
	kickUserFunc     func(connid string, msgstr string)
}

//创建登录结果处理器
func NewLogonRetHandler(playerManager *player.PlayerManager, userLogoutFunc func(int32, string), kickUserFunc func(connid string, msgstr string)) *LogonRetHandler {
	reth := new(LogonRetHandler)
	reth.playerManager = playerManager
	reth.onUserLogOutFunc = userLogoutFunc
	reth.kickUserFunc = kickUserFunc

	return reth
}

func (h *LogonRetHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_LoginRetID)
}

func (h *LogonRetHandler) OnRPCMessage(m *msg.Message, ctx *module.MouduleCtx) {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UserLoginRet{}
		return mm
	})

	if rmm == nil {
		return
	}

	mm := rmm.(*netproto.UserLoginRet)

	pr := h.playerManager.GetPlayerByConnID(rpcinfo.GetConnID())

	// 登录成功，把AreaId存放在連線資訊中
	pr.Conn.AreaId = int64(mm.GetArea_ID())

	// 登录成功，但客户端连接已经断开，此时要执行登出操作
	if pr == nil {
		if mm.GetCode() == 1 {
			uid := mm.GetUserID()
			cer := mm.GetCer()

			//通知用户登出大厅
			h.onUserLogOutFunc(uid, cer)
		}

		return
	}

	//登录失败,断开客户端连接
	if mm.GetCode() == 0 {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("登录失败 尚未断开客户端连接")).Debug("HallServer壓測時")
		//ctx.Server.CloseConnByConnID(rpcinfo.GetConnID())
	} else {

		//检查用户是否已经在线
		exur := h.playerManager.GetPlayerByUserID(mm.GetUserID())
		if exur != nil {
			exconnid := exur.Conn.ConnID()

			h.playerManager.ClearPlayerInfo(exconnid)

			//如果不是同一连接,则踢出用户
			if exconnid != rpcinfo.GetConnID() {
				h.kickUserFunc(exconnid, "您的账号已在别处登录.")
			}
		}

		//检查当前连接是否已经有登录账号
		if !pr.IsGuest() {
			if pr.UserID != mm.GetUserID() {
				h.onUserLogOutFunc(pr.UserID, pr.Cer)

				h.playerManager.ClearPlayerInfo(rpcinfo.GetConnID())
			}
		}

		h.playerManager.SetPlayerData(rpcinfo.GetConnID(), func(pr *player.Player) {
			//设置用户信息
			pr.Cer = mm.GetCer()
			pr.UserID = mm.GetUserID()
			pr.HDType = mm.GetHDType()
			pr.HDCode = mm.GetHDCode()
			pr.UserData = mm.GetUserData()
		})
	}

}
