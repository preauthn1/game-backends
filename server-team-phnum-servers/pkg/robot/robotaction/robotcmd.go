package robotaction

import (
	"bytes"
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

type RobotCmd struct {
	robotManager *robotcomm.RobotManager
	robotLoader  *RobotLoader
}

//创建机器人命令
func NewRobotCmd(robotMgm *robotcomm.RobotManager, robotLoader *RobotLoader) *RobotCmd {
	rcmd := new(RobotCmd)
	rcmd.robotManager = robotMgm
	rcmd.robotLoader = robotLoader

	return rcmd
}

func (rc *RobotCmd) Name() string {
	return "robot"
}

func (rc *RobotCmd) Help() string {
	return "机器人，eg:robot list | robot logout cn | robot logon cn"
}

func (rc *RobotCmd) Exec(args []string) string {
	if len(args) < 1 {
		return "error,seeref usage"
	}

	cmdname := args[0]

	if cmdname == "list" {
		var buf bytes.Buffer

		robots := rc.robotManager.GetAllRobot()

		gamecount := 0
		totalcount := 0
		for _, r := range robots {
			buf.WriteString(r.String())
			buf.WriteString("\r\n")

			if r.UserInfo.IsGameing() {
				gamecount++
			}

			totalcount++
		}
		buf.WriteString(fmt.Sprintf("一共有机器人%d个,其中游戏中的机器人%d个", totalcount, gamecount))

		return buf.String()

	}

	if cmdname == "logout" {
		if len(args) > 1 && args[1] != "" {
			cn := util.StrToInt32(args[1])
			if cn > 0 {
				mlog.Debug("準備登出 %d 機器人", cn)
				rc.robotLoader.logoutRobot(cn)
				return "執行成功"
			}
		}
	}

	return "error,seeref usage"
}
