package svrmgm

import (
	"strings"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

//服务器关闭命令
type SvrCloseCmd struct {
	msgServer  *msgserver.MsgServer
	serverlist *ServerList
}

//创建服务器关闭命令
func NewSvrCloseCmd(msgServer *msgserver.MsgServer, serverlist *ServerList) *SvrCloseCmd {
	closecmd := new(SvrCloseCmd)
	closecmd.msgServer = msgServer
	closecmd.serverlist = serverlist

	return closecmd
}

func (closeCmd *SvrCloseCmd) Name() string {
	return "closeserver"
}

func (closeCmd *SvrCloseCmd) Help() string {
	return "关服.usage:closeserver message"
}

func (closeCmd *SvrCloseCmd) Exec(args []string) string {
	if len(args) < 1 {
		return "error,seeref usage"
	}

	tipmessage := strings.Join(args[0:], " ")

	//构造停服消息
	svrstopMsg := new(msg.Message)
	svrstopMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
	svrstopMsg.SClassID = int32(netproto.NotifyServerClassID_ServerStopID)

	svrstopData := new(netproto.ServerStop)
	svrstopData.Type = proto.Int32(0)
	svrstopData.Status = proto.Int32(conf.ServerStatus_Close)
	svrstopData.Message = proto.String(tipmessage)
	svrstopMsg.MsgData = svrstopData

	closeCmd.msgServer.SendAllMessage(svrstopMsg)

	return "server close msg sended..."
}
