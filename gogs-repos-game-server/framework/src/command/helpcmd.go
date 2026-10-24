package command

import (
	"bytes"
	"fmt"
	"strings"
)

//帮助命令，列出所有的辅助命令行
type HelpCommand struct {
}

func (c *HelpCommand) Name() string {
	return "help"
}

func (c *HelpCommand) Help() string {
	return "列出所有命令和命令用法"
}

func (c *HelpCommand) Exec(args []string) string {
	var buf bytes.Buffer
	buf.WriteString(fmt.Sprintf("%-10s%-60s\r\n", "name", "help"))
	buf.WriteString(fmt.Sprintf("%s\r\n", strings.Repeat("-", 70)))

	for _, c := range cmg {
		buf.WriteString(fmt.Sprintf("%-10s%-50s\r\n", c.Name(), c.Help()))
	}

	return buf.String()
}
