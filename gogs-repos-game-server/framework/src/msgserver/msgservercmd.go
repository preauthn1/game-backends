package msgserver

import (
	"fmt"
)

//消息服务器命令
type MsgServerCmd struct {
	msgserver *MsgServer
}

//创建消息服务器命令
func NewMsgServerCmd(msgserver *MsgServer) *MsgServerCmd {
	cmd := new(MsgServerCmd)
	cmd.msgserver = msgserver

	return cmd
}

func (cmd *MsgServerCmd) Name() string {
	return "msg"
}

func (cmd *MsgServerCmd) Help() string {
	return "消息服务器查看，eg:msg listserver | msg listlogic "
}

func (cmd *MsgServerCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "参数错误,usage:msg listserver/msg listlogic"
	}

	cmdname := args[0]
	switch cmdname {
	case "listserver":
		return cmd.msgserver.NetServer.String()
	case "listlogic":
		str := ""
		for _, lg := range cmd.msgserver.GetLogicHandlerGroup().innerLogicHandlers {
			str += fmt.Sprintf("%T", lg)
			str += ","
		}

		return str
	default:
		return "参数错误,usage:msg listserver/msg list logic"
	}

}
