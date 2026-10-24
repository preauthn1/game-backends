package serverhandler

//服务器关闭命令
type NotifyHandlerCmd struct {
	ntfHandler *NotifyHandler
}

//创建服务器关闭命令
func NewNotifyHandlerCmd(handler *NotifyHandler) *NotifyHandlerCmd {
	cmd := new(NotifyHandlerCmd)
	cmd.ntfHandler = handler

	return cmd
}

func (cmd *NotifyHandlerCmd) Name() string {
	return "ntfhandler"
}

func (cmd *NotifyHandlerCmd) Help() string {
	return "协调服务器cmd.usage:ntfhandler sendmessage | ntfhandler close"
}

func (cmd *NotifyHandlerCmd) Exec(args []string) string {
	if len(args) < 1 {
		return "error,seeref usage"
	}

	if args[0] == "sendmessage" {
		data := []byte("test")
		cmd.ntfHandler.TcpMsgClient.SendData(data)
	} else if args[0] == "close" {
		cmd.ntfHandler.TcpMsgClient.Close()
	} else {
		return "error,seeref usage"
	}

	return "ok..."
}
