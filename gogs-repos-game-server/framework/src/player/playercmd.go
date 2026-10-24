package player

import (
	"bytes"
	"fmt"
	"msgserver"
	"strconv"
	"util"
)

type PlayerCmd struct {
	playerManager *PlayerManager
	server        *msgserver.MsgServer
}

//创建用户命令
func NewPlayerCmd(playerManager *PlayerManager, server *msgserver.MsgServer) *PlayerCmd {
	cmd := new(PlayerCmd)
	cmd.playerManager = playerManager
	cmd.server = server

	return cmd
}

func (pc *PlayerCmd) Name() string {
	return "player"
}

func (pc *PlayerCmd) Help() string {
	return "用户管理，eg:player stat | player kick userid | player find userid | player list 0(1)"
}

func (pc *PlayerCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error.seeref usage"
	}

	cmdname := args[0]

	switch cmdname {
	case "stat":
		return fmt.Sprintf("total:%d logon user:%d,unlogon user:%d", pc.playerManager.PlayerCount(), len(pc.playerManager.LogonPlayer()), len(pc.playerManager.UnLogonPlayer()))
	case "kick":
		if len(args) != 2 {
			return "error.eg kick 10001"
		}

		if !util.IsNumber(args[1]) {
			return "error.用户ID必须为数字"
		}

		uid, _ := strconv.Atoi(args[1])

		pr := pc.playerManager.GetPlayerByUserID(int32(uid))
		if pr == nil {
			return "用户不存在"
		}

		pc.server.CloseConn(pr.Conn)
		return "kick success"
	case "find":
		if len(args) != 2 {
			return "error.eg find 10001"
		}

		if !util.IsNumber(args[1]) {
			return "error.用户ID必须为数字"
		}

		uid, _ := strconv.Atoi(args[1])
		pr := pc.playerManager.GetPlayerByUserID(int32(uid))

		if pr == nil {
			return "用户不存在"
		}
		return pr.String()
	case "list":
		if len(args) != 2 {
			return "error.eg list (0/1/2)"
		}

		if args[1] == "0" {
			var buf bytes.Buffer

			pc.playerManager.Range(func(pr *Player) {
				buf.WriteString(fmt.Sprintf("%s\r\n", pr))
			})

			return buf.String()
		} else if args[1] == "1" {
			var buf bytes.Buffer
			prs := pc.playerManager.LogonPlayer()

			for _, pr := range prs {
				buf.WriteString(fmt.Sprintf("%s\r\n", pr))
			}

			return buf.String()

		} else if args[1] == "2" {
			var buf bytes.Buffer
			prs := pc.playerManager.UnLogonPlayer()

			for _, pr := range prs {
				buf.WriteString(fmt.Sprintf("%s\r\n", pr))
			}

			return buf.String()
		} else if args[1] == "3" {
			var buf bytes.Buffer

			pc.playerManager.Range(func(pr *Player) {
				buf.WriteString(fmt.Sprintf("%s\r\n", pr.SimpleString()))
			})

			return buf.String()
		} else {
			return "error.eg list (0/1/2/3)"
		}

	}

	return ""
}
