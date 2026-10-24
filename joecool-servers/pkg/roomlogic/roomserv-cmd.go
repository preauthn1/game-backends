package roomlogic

import (
	"fmt"
	"os"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

// RoomStateCmd 查詢房間狀態指令
type RoomStateCmd struct {
	roomlogic *RoomLogic
}

// NewRoomStateCmd Create RoomStateCmd
func NewRoomStateCmd(rl *RoomLogic) *RoomStateCmd {
	cmd := new(RoomStateCmd)
	cmd.roomlogic = rl
	return cmd
}

// Name Command name
func (rl *RoomStateCmd) Name() string {
	return "roomserv"
}

// Help Command Description
func (rl *RoomStateCmd) Help() string {
	return fmt.Sprintf("Server控制功能，ex: %s stat 查詢狀態。| %s stop [message]", rl.Name(), rl.Name())
}

// Exec Command Logic
func (rl *RoomStateCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error.seeref usage"
	}

	cmdname := args[0]

	switch cmdname {
	case "stat":
		return fmt.Sprintf("Server Status: %d", rl.roomlogic.ServerStatus.GetServerStatus())
	case "stop":

		var message string = "Server Shoutdown"

		if len(args) > 1 {
			message = args[1]
		}

		rl.roomlogic.roomactionmanager.serverStatus.SetServerStatus(conf.ServerStatus_Close, message)

		// 等所有玩家被踢出
		for {
			time.Sleep(time.Second)

			cnt := len(rl.roomlogic.roomactionmanager.roomUserManager.GetAllUser())

			mlog.System("Left users %d", cnt)

			if cnt == 0 {
				break
			}
		}

		mlog.System(message)

		os.Exit(1)

		return ""
	}
	return "fail"
}
