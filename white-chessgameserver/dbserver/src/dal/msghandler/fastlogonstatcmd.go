package msghandler

import (
	"fmt"
	"strings"
	"util"
)

//是否需要兩次登入請求
type FastLogonStatCommand struct {
}

func (c *FastLogonStatCommand) Name() string {
	return "fastlogonstat"
}

func (c *FastLogonStatCommand) Help() string {
	return "是否需要兩次登入請求, on:需要, off:不需要"
}

func (c *FastLogonStatCommand) Exec(args []string) string {
	lenarg := len(args)
	switch lenarg {
	case 1:
		stype := args[0]
		if strings.ToLower(stype) == "on" {
			FastLogonStat = true
			return fmt.Sprintf("FastLogonStat:%v", FastLogonStat)
		} else if stype == "off" {
			HDCodeMap = util.NewSafeMap()
			FastLogonStat = false
			return fmt.Sprintf("FastLogonStat:%v", FastLogonStat)
		} else {
			return "error args"
		}
	}
	return "nothing happen"
}
