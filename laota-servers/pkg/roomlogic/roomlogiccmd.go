package roomlogic

import (
	"strconv"

	"example.invalid/ServerTeam/Servers/pkg/util"
)

type rlCmd struct {
	roomlogic *RoomLogic
}

func NewRoomLogicCmd(rl *RoomLogic) *rlCmd {
	cmd := new(rlCmd)
	cmd.roomlogic = rl
	return cmd
}

func (rl *rlCmd) Name() string {
	return "roomlogic"
}

func (rl *rlCmd) Help() string {
	return "RoomLogic.usage:roomlogic reload 重新載入房間設定檔logicconfig.dat "
}

func (rl *rlCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error.seeref usage"
	}

	cmdname := args[0]

	switch cmdname {
	case "reload":
		rl.ReloadConfig()
		return "done"
	case "host":
		if len(args) != 3 || args[1] != "change" {
			return "error, host usage"
		}

		if !util.IsNumber(args[2]) {
			return "error.主機狀態必須為數字"
		}

		newHost, _ := strconv.Atoi(args[2])
		rl.updateHostAndBackup(int32(newHost))

		return "update host done."
	}
	return "nil"
}

func (rl *rlCmd) ReloadConfig() {
	rl.roomlogic.ReloadGameServerConfig()
}

func (rl *rlCmd) updateHostAndBackup(newHostType int32) {
	rl.roomlogic.UpdateHostAndBackUp(newHostType, "")
}
