package command

import (
	"os"
	"os/exec"
	"runtime"
)

//设置标题命令
type TitleCommand struct {
}

func (c *TitleCommand) Name() string {
	return "title"
}

func (c *TitleCommand) Help() string {
	return "设置装口标题，eg:title abc"
}

func (c *TitleCommand) Exec(args []string) string {
	if runtime.GOOS == "windows" {
		if len(args) != 1 {
			return "error,eg: title abc"
		}

		cmd := exec.Command("cmd", "/C", "title", args[0])
		cmd.Stdout = os.Stdout
		cmd.Run()

		return "设置标题成功"
	}

	return ""
}
