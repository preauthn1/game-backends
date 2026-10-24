package command

import (
	"os"
)

//退出命令
type QuitCommand struct {
}

func (c *QuitCommand) Name() string {
	return "quit"
}

func (c *QuitCommand) Help() string {
	return "退出命令，关闭应用程序"
}

func (c *QuitCommand) Exec(args []string) string {
	if len(args) == 1 {
		if args[0] == "yes" {
			os.Exit(1)
		}
	} else {
		return "请输入quit yes 强制退出"
	}

	return ""
}
