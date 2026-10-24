package svrmgm

import (
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

//停服IP白名单
type ServerStopWhiteIPCmd struct {
	msgServer  *msgserver.MsgServer
	serverlist *ServerList
}

//创建停服IP白名单命令
func NewServerStopWhiteIPCmd(msgServer *msgserver.MsgServer, serverlist *ServerList) *ServerStopWhiteIPCmd {
	cmd := new(ServerStopWhiteIPCmd)
	cmd.msgServer = msgServer
	cmd.serverlist = serverlist

	return cmd
}

func (wip *ServerStopWhiteIPCmd) Name() string {
	return "whiteip"
}

func (wip *ServerStopWhiteIPCmd) Help() string {
	return "停服白名单.usage:whiteip 0.0.0.0,0.0.0.0 ..."
}

func (wip *ServerStopWhiteIPCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error,seeref usage"
	}

	ipArr := strings.Split(args[0], ",")

	if len(ipArr) == 0 {
		return "error.白名单IP地址不正确"
	}

	wipMsg := new(msg.Message)
	wipMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
	wipMsg.SClassID = int32(netproto.NotifyServerClassID_ServerStopWhiteIPListID)

	wipData := new(netproto.ServerStopWhiteIPList)
	for _, ips := range ipArr {
		if ips == "" {
			continue
		}

		wipData.IP = append(wipData.IP, ips)
	}
	wipMsg.MsgData = wipData

	hslst := wip.serverlist.GetAllServer()
	if hslst != nil {
		for _, hs := range hslst {
			example.invalid("通知服务器%v刷新IP白名单", hs)
			wip.msgServer.SendMessage(hs.ConnInfo, wipMsg)
		}
	}

	return "刷新IP白名单成功"
}
