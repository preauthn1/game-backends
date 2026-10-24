package command

import (
	"os"
	"os/exec"
	"runtime"
)

//清除屏幕命令
type ClsCommand struct {
}

func (c *ClsCommand) Name() string {
	return "cls"
}

func (c *ClsCommand) Help() string {
	return "清理当前屏幕"
}

func (c *ClsCommand) Exec(args []string) string {
	if runtime.GOOS == "windows" {
		cmd := exec.Command("cmd", "/C", "cls")
		cmd.Stdout = os.Stdout
		cmd.Run()

	}

	return ""
}
