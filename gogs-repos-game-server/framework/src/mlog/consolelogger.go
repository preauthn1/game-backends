package mlog

import (
	"fmt"
	"example.invalid/mattn/go-colorable"
	"log"
	"os"
	"regexp"
	"conf"
)

const (
	color_black = uint8(iota + 90)
	color_red
	color_green
	color_yellow
	color_blue
	color_magenta
	color_cyan
	color_white
)

type ConsoleLogger struct {
	innerLogger *log.Logger
	enabled     bool
}

//创建控制台日志对象实例
func NewConsoleLogger() (*ConsoleLogger, error) {
	logger := new(ConsoleLogger)
	logger.innerLogger = log.New(colorable.NewColorable(os.Stdout), "", log.LstdFlags|log.Lshortfile|log.Lmicroseconds)
	logger.enabled = true

	return logger, nil
}

//关闭日志
func (logger *ConsoleLogger) Close() {
	logger.innerLogger = nil
}

//写日志
func (logger *ConsoleLogger) WriteLog(lvl int, format string, a ...interface{}) {
	if lvl < conf.CLogLevel{
		return
	}

	if !logger.enabled {
		return
	}

	if logger.innerLogger == nil {
		return
	}

	lvldesc := getLogLevelDesc(lvl)
	//输出日志
	text := fmt.Sprintf(lvldesc+format, a...)
	logger.innerLogger.Output(5, logger.ColorText(lvl, text))
}

func (logger *ConsoleLogger) Enable(ret bool) {
	logger.enabled = ret
}

func (logger *ConsoleLogger) ColorText(lvl int, text string) string {

	switch lvl {
	case DebugLevel:
		lvlColor := color_blue
		text = logger.HLightMessage(text, lvlColor)
		return fmt.Sprintf("\033[%dm%s\033[0m", lvlColor, text)
	case InfoLevel:
		lvlColor := color_green
		text = logger.HLightMessage(text, lvlColor)
		return fmt.Sprintf("\033[%dm%s\033[0m", lvlColor, text)
	case WarnLevel:
		lvlColor := color_yellow
		text = logger.HLightMessage(text, lvlColor)
		return fmt.Sprintf("\033[%dm%s\033[0m", lvlColor, text)
	case ErrorLevel:
		lvlColor := color_red
		text = logger.HLightMessage(text, lvlColor)
		return fmt.Sprintf("\033[%dm%s\033[0m", lvlColor, text)
	case FatalLevel:
		lvlColor := color_magenta
		text = logger.HLightMessage(text, lvlColor)
		return fmt.Sprintf("\033[%dm%s\033[0m", lvlColor, text)
	//case 100: //黑色
	//	return fmt.Sprintf("\x1b[%dm%s\x1b[0m", color_black, text)
	//case 101: //白色
	//	return fmt.Sprintf("\x1b[%dm%s\x1b[0m", color_white, text)
	default:
		text = logger.HLightMessage(text, 0)
		return text
	}
	return text
}

//Message 高亮
func (logger *ConsoleLogger) HLightMessage(text string, originColor uint8) string {
	r := regexp.MustCompile(",?\"Message\":(\".*?\"),")
	strs := r.FindStringSubmatch(text)
	replace := fmt.Sprintf(",\"Message\":%s,", fmt.Sprintf("\033[1m\033[%dm%s\033[%dm", originColor, strs[1], originColor))
	text = r.ReplaceAllString(text, replace)
	return text
}
