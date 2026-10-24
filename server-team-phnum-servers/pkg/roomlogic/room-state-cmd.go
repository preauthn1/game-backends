package roomlogic

import "fmt"

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
	return fmt.Sprintf("查詢Server狀態，ex: %s stat", rl.Name())
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
	}
	return "fail"
}
