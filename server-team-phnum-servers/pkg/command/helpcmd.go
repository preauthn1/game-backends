package command

import (
	"bytes"
	"fmt"
	"sort"
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
	buf.WriteString(fmt.Sprintf("%-20s%-60s\r\n", "name", "help"))
	buf.WriteString(fmt.Sprintf("%s\r\n", strings.Repeat("-", 70)))

	cmds := make([]Command, 0, len(cmg))

	for _, c := range cmg {
		cmds = append(cmds, c)
	}

	// 排序
	sort.Slice(cmds, func(i, j int) bool {
		return cmds[i].Name()[:1] < cmds[j].Name()[:1]
	})

	for _, c := range cmds {
		buf.WriteString(fmt.Sprintf("%-20s%-60s\r\n", c.Name(), c.Help()))
	}

	return buf.String()
}
