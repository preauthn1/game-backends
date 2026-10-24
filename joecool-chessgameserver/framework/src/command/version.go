package command

import (
	"conf"
	"fmt"
)

//清除屏幕命令
type VersionCommand struct {
}

func (c *VersionCommand) Name() string {
	return "version"
}

func (c *VersionCommand) Help() string {
	return "顥示目前版本資訊"
}

func (c *VersionCommand) Exec(args []string) string {
	return fmt.Sprintf("===版本資訊===\n當前版本：%s GitHash：%s\n建置時間：%s\n建置編號：%s", conf.GitTag, conf.GitHash, conf.BuildTime, conf.BuildNum)
}
