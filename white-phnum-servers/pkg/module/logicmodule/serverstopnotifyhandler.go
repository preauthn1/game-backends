package logicmodule

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/player"
)

//服务器状态
type ServerStatusProvider interface {
	IsServerRuning() bool                           //服务器是否运行状态
	GetServerStatus() int32                         //获取服务器状态
	GetStopServerTipMsg() string                    // 获取停服提示消息
	IsWhiteIP(ip string) bool                       //是否IP白名单
	SetServerStatus(statusid int32, msg string)     //设置服务器状态
	SetMasterSlave(master HostInfo, slave HostInfo) //設置Master和Slave
}

//停服通知处理器
type ServerStopNotifyHandler struct {
	serverstopInfo *netproto.ServerStop
	whiteIPList    *netproto.ServerStopWhiteIPList
}

type HostInfo struct {
	GameId   int32
	ServerId int32
}

//创建停服通知处理器对象
func NewServerStopNotifyHandler() *ServerStopNotifyHandler {
	ssh := new(ServerStopNotifyHandler)
	ssh.serverstopInfo = nil
	ssh.whiteIPList = nil

	return ssh
}

func (ssh *ServerStopNotifyHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) &&
		(sclsid == int32(netproto.NotifyServerClassID_ServerStopID) ||
			sclsid == int32(netproto.NotifyServerClassID_ServerStopWhiteIPListID) ||
			sclsid == int32(netproto.NotifyServerClassID_ServerHostAndBackUp))
}

func (ssh *ServerStopNotifyHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, player *player.PlayerManager) {
	//停服
	if m.SClassID == int32(netproto.NotifyServerClassID_ServerStopID) {
		mmbyte := m.MsgData.([]byte)

		mm := &netproto.ServerStop{}
		err := proto.Unmarshal(mmbyte, mm)

		if err != nil {
			ctx.Error("解析停服消息错误,%v", err)
			return
		}

		ssh.serverstopInfo = mm

		example.invalid("收到停服消息,%v", ssh.serverstopInfo)
	}

	//ip白名单
	if m.SClassID == int32(netproto.NotifyServerClassID_ServerStopWhiteIPListID) {

		mmbyte := m.MsgData.([]byte)

		mm := &netproto.ServerStopWhiteIPList{}
		err := proto.Unmarshal(mmbyte, mm)

		if err != nil {
			ctx.Error("解析停服IP白名单消息错误,%v", err)
			return
		}

		ssh.whiteIPList = mm
		example.invalid("收到停服IP白名单消息,%v", ssh.whiteIPList)

	}

}

func (ssh *ServerStopNotifyHandler) IsServerRuning() bool {
	if ssh.serverstopInfo == nil {
		return true
	}

	if ssh.serverstopInfo.GetStatus() == conf.ServerStatus_Runing || ssh.serverstopInfo.GetStatus() == conf.ServerStatus_Recover {
		return true
	}

	return false
}

func (ssh *ServerStopNotifyHandler) GetServerStatus() int32 {
	if ssh.serverstopInfo == nil {
		return conf.ServerStatus_Runing
	}

	return ssh.serverstopInfo.GetStatus()
}

func (ssh *ServerStopNotifyHandler) GetStopServerTipMsg() string {
	if ssh.serverstopInfo == nil {
		return ""
	}

	return ssh.serverstopInfo.GetMessage()
}

func (ssh *ServerStopNotifyHandler) IsWhiteIP(ip string) bool {
	if ssh.whiteIPList == nil {
		return false
	}

	for _, sip := range ssh.whiteIPList.IP {
		if ip == sip {
			return true
		}
	}

	return false
}

func (ssh *ServerStopNotifyHandler) SetServerStatus(statusid int32, msg string) {
	if ssh.serverstopInfo == nil {
		ssh.serverstopInfo = new(netproto.ServerStop)
	}

	ssh.serverstopInfo.Status = proto.Int32(statusid)
	ssh.serverstopInfo.Message = proto.String(msg)
}

func (ssh *ServerStopNotifyHandler) SetMasterSlave(master HostInfo, slave HostInfo) {
	mlog.System("SetMasterSlave")
}
