package command

import (
	"fmt"
	"regexp"
	"strconv"
	"strings"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
)

// LogCommand 日志控制命令
type LogCommand struct {
	logtype int
}

// Name 指令名稱
func (c *LogCommand) Name() string {
	return "log"
}

// Help 指令說明
func (c *LogCommand) Help() string {
	return "设置日志类型，eg:log on/off | log get | log set 0/1/2 0 控制台和定时切割的文本 1控制台 2定时切割的文本 | log level 0/1/2/3 0调试信息 1输出日志信息 2错误 3严重错误"
}

// Exec 執行指令
func (c *LogCommand) Exec(args []string) string {
	lenarg := len(args)
	switch lenarg {
	case 1:
		stype := args[0]
		if strings.ToLower(stype) == "get" {
			return fmt.Sprintf("当前日志类型:%d", c.logtype)
		} else if stype == "on" {
			mlog.GetCLogger().Enable(true)

			return "log enable"
		} else if stype == "off" {
			mlog.GetCLogger().Enable(false)

			return "log disable"
		}
	case 2:
		stype := args[0]
		ltype := args[1]

		regxType := regexp.MustCompile(`^\d$`)

		if strings.ToLower(stype) == "set" && regxType.MatchString(ltype) {

			iltype, _ := strconv.Atoi(ltype)

			//如果和当前类型一致
			if c.logtype == iltype {
				return "日志类型设置成功"
			}

			if ltype == "0" || ltype == "1" || ltype == "2" {
				//定时切割日志
				if ltype == "0" {
					mlog.SetGLogger(mlog.BuildTimeRollingAndConsoleLogGroup())
				} else if ltype == "1" { //控制台日志
					logger, _ := mlog.NewConsoleLogger()
					mlog.SetGLogger(logger)
				} else if ltype == "2" { //定时切割日志
					mlog.SetGLogger(mlog.CreateTimeRollingLogger())
				}

				c.logtype = iltype
				return "日志类型设置成功"
			}
		} else if strings.ToLower(stype) == "level" && regxType.MatchString(ltype) {
			iltype, _ := strconv.Atoi(ltype)
			conf.CLogLevel = iltype
			return "日志类型设置成功"
		} else if strings.ToLower(stype) == "lite" && regxType.MatchString(ltype) {
			iltype, _ := strconv.Atoi(ltype)
			if iltype == 1 {
				conf.ClearLog = true
				return "簡化日誌內容"
			}
			conf.ClearLog = false
			return "還原日誌內容"
		}
	default:

	}

	return "设置日志类型，eg:log on/off | log get | log set 0/1/2 0 控制台和定时切割的文本 1控制台 2定时切割的文本 | log level 0/1/2/3 0调试信息 1输出日志信息 2错误 3严重错误"
}
