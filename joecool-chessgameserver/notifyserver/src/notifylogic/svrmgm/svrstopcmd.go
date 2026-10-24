package svrmgm

import (
	"conf"
	"msg"
	"msgserver"
	"netproto"
	"strconv"
	"strings"
)

import proto "example.invalid/golang/protobuf/proto"

//停服
type SvrStopCmd struct {
	msgServer  *msgserver.MsgServer
	serverlist *ServerList
}

//创建停服命令对象
func NewSvrStopCmd(msgSvr *msgserver.MsgServer, serverlist *ServerList) *SvrStopCmd {
	sscmd := new(SvrStopCmd)
	sscmd.msgServer = msgSvr
	sscmd.serverlist = serverlist

	return sscmd
}

func (stopCmd *SvrStopCmd) Name() string {
	return "stopserver"
}

// server game start 9 21
// server game stop 9 21
func (stopCmd *SvrStopCmd) Help() string {
	return "停服.usage:stopserver type(0 全服 1 游戏 2 房间) status(0 停服 1 开服) gameid serverid msg"
}

func (stopCmd *SvrStopCmd) Exec(args []string) string {
	if len(args) < 3 {
		return "error,seeref usage"
	}

	//0 全服 1 游戏 2 房间
	sstype := args[0]
	//0 停服 1 开服
	ss_status := args[1]

	//检查状态值
	if ss_status != "0" && ss_status != "1" {
		return "error,seeref usage"
	}

	status, _ := strconv.Atoi(ss_status)

	//构造停服消息
	svrstopMsg := new(msg.Message)
	svrstopMsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
	svrstopMsg.SClassID = int32(netproto.NotifyServerClassID_ServerStopID)

	svrstopData := new(netproto.ServerStop)
	svrstopData.Type = proto.Int32(0)
	svrstopData.Status = proto.Int32(int32(status))

	//全服
	if sstype == "0" {

		stopmsg := strings.Join(args[2:], " ")
		svrstopData.Message = proto.String(stopmsg)
		svrstopMsg.MsgData = svrstopData

		stopCmd.msgServer.SendAllMessage(svrstopMsg)

	} else if sstype == "1" { //游戏
		if len(args) < 4 {
			return "error,seeref usage"
		}

		ss_gameid := args[2]
		stopmsg := strings.Join(args[3:], " ")

		gameid, err := strconv.Atoi(ss_gameid)

		if err != nil {
			return "error,seeref usage"
		}
		//设置游戏ID
		svrstopData.GameID = proto.Int32(int32(gameid))
		//停服消息
		svrstopData.Message = proto.String(stopmsg)
		svrstopMsg.MsgData = svrstopData

		svrs := stopCmd.serverlist.GetServerByGameID(gameid)

		if svrs != nil && len(svrs) > 0 {
			for _, svr := range svrs {
				stopCmd.msgServer.SendMessage(svr.ConnInfo, svrstopMsg)
			}
		}

	} else if sstype == "2" { //房间
		if len(args) < 5 {
			return "error,seeref usage"
		}

		//游戏ID
		ss_gameid := args[2]
		//服务器ID
		ss_serverid := args[3]
		//停服消息
		stopmsg := strings.Join(args[4:], " ")

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
		svrstopData.Message = proto.String(stopmsg)
		svrstopMsg.MsgData = svrstopData

		svr := stopCmd.serverlist.FindServer(conf.ServerType_GameServer, serverid)

		if svr != nil {
			stopCmd.msgServer.SendMessage(svr.ConnInfo, svrstopMsg)
		}
	}

	return "server stop msg sended..."
}
