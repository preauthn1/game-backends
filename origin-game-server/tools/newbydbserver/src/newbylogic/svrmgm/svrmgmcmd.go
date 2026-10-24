package svrmgm

import (
	"fmt"
)

type SvrMgmCmd struct {
	serverList *ServerList
}

func NewSvrMgmCmd(svrlst *ServerList) *SvrMgmCmd {
	svrcmd := new(SvrMgmCmd)
	svrcmd.serverList = svrlst

	return svrcmd
}

func (svrcmd *SvrMgmCmd) Name() string {
	return "ns"
}

func (svrcmd *SvrMgmCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error,seeref usage"
	}

	if args[0] == "list" {
		if len(args) != 2 {
			return "error,seeref ueage"
		}

		listparam := args[1]

		var servers []*ServerInfo
		if listparam == "0" {
			servers = svrcmd.serverList.GetAllServer()
		} else if listparam == "1" {
			servers = svrcmd.serverList.GetHallServer()
		} else if listparam == "2" {
			servers = svrcmd.serverList.GetGameServer()
		}

		str := ""
		for _, svr := range servers {
			str += fmt.Sprintf("%s\r\n", svr.String())
		}

		return str
	}

	return ""
}

func (svrcmd *SvrMgmCmd) Help() string {
	return "注册服务器查询.usage:ns list 0(1/2)"
}
