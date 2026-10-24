package svrmgm

import (
	"conf"
	"msg"
	"msgserver"
	"netproto"
	"strconv"
)

import (
	"fmt"
	proto "example.invalid/golang/protobuf/proto"
	"strings"
)

//停服
type ServerCtlCmd struct {
	msgServer  *msgserver.MsgServer
	serverlist *ServerList
}

//创建停服命令对象
func NewServerCtlCmd(msgSvr *msgserver.MsgServer, serverlist *ServerList) *ServerCtlCmd {
	sscmd := new(ServerCtlCmd)
	sscmd.msgServer = msgSvr
	sscmd.serverlist = serverlist

	return sscmd
}

func (cmd *ServerCtlCmd) Name() string {
	return "serverctl"
}

// serverctl game start 9 21 -m close
// serverctl game stop 9 21 -m close
func (cmd *ServerCtlCmd) Help() string {
	help := "服務器控制 (開服|停服). \n\tUsage: serverctl <ServerType> <ServerStatus> [<gameid> [<serverid>]] [-m msg]\n\t"
	help += "<ServerType>: all, game, room\n\t"
	help += "<ServerStatus>: stop, start"
	return help
}

func (cmd *ServerCtlCmd) Exec(args []string) string {
	// 獲取 -m "message"
	message := "GameServer will be " + args[1]
	for i, s := range args {
		if s == "-m" {
			if len(args) > i+1 {
				message = strings.Join(args[i+1:], " ")
			}
			args = args[:i]
		}
	}
	if len(args) < 2 {
		return "error,seeref usage"
	}

	//<ServerType>: all, game, room
	sstype := args[0]
	//<ServerStatus>: stop, start
	ss_status := args[1]

	//检查状态值
	if ss_status != "stop" && ss_status != "start" {
		return "error,seeref usage"
	}
	status := 1
	if ss_status == "stop" {
		status = 0
	} else {
		status = 1
	}

	//构造停服消息
	svrstopMsg := new(msg.Message)
	svrstopMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
	svrstopMsg.SClassID = int32(netproto.NotifyServerClassID_ServerStopID)

	svrstopData := new(netproto.ServerStop)
	svrstopData.Type = proto.Int32(0)
	svrstopData.Status = proto.Int32(int32(status))

	//全服
	if sstype == "all" {

		svrstopData.Message = proto.String(message)
		svrstopMsg.MsgData = svrstopData

		cmd.msgServer.SendAllMessage(svrstopMsg)

	} else if sstype == "game" { //游戏
		if len(args) < 3 {
			return "error,seeref usage"
		}
		fmt.Println(message)
		ss_gameid := args[2]

		gameid, err := strconv.Atoi(ss_gameid)

		if err != nil {
			return "error,seeref usage"
		}
		//设置游戏ID
		svrstopData.GameID = proto.Int32(int32(gameid))
		//停服消息
		svrstopData.Message = proto.String(message)
		svrstopMsg.MsgData = svrstopData

		svrs := cmd.serverlist.GetServerByGameID(gameid)

		if svrs != nil && len(svrs) > 0 {
			for _, svr := range svrs {
				cmd.msgServer.SendMessage(svr.ConnInfo, svrstopMsg)
			}
		}

	} else if sstype == "room" { //房间
		if len(args) < 4 {
			return "error,seeref usage"
		}

		//游戏ID
		ss_gameid := args[2]
		//服务器ID
		ss_serverid := args[3]

		serverid, err := strconv.Atoi(ss_serverid)
		if err != nil {
			return "error,seeref usage"
		}

		gameid, err := strconv.Atoi(ss_gameid)

		if err != nil {
			return "error,seeref usage"
		}

		//设置游戏ID
		svrstopData.GameID = proto.Int32(int32(gameid))
		//设置服务器ID
		svrstopData.ServerID = proto.Int32(int32(serverid))
		//停服消息
		svrstopData.Message = proto.String(message)
		svrstopMsg.MsgData = svrstopData

		svr := cmd.serverlist.FindServer(conf.ServerType_GameServer, serverid)

		if svr != nil {
			cmd.msgServer.SendMessage(svr.ConnInfo, svrstopMsg)
		}
	}

	return "server stop msg sended..."
}
