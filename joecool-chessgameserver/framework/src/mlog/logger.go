package mlog

import (
	"conf"
	"time"
)

//日志级别
const (
	DebugLevel  = 0  //调试信息
	InfoLevel   = 1  //输出日志信息
	WarnLevel   = 2  //警告信息
	ErrorLevel  = 3  //错误
	FatalLevel  = 9  //严重错误 -> 將會停止服務器
	SystemLevel = 99 //系統級別
)

//日志接口定义
type Logger interface {
	WriteLog(lvl int, format string, a ...interface{})
	Close()
}

//获取日志等级描述字符串
/*
日誌使用規範
開發調試的日誌皆為 debug level
用戶操作或遊戲信息為 info level
預期的錯誤為 warning level
非預期錯誤為 error level
系統信息為 system level
基本上不會調用 fatal level
*/
func getLogLevelDesc(lvl int) string {
	lvldesc := ""
	switch lvl {
	case 0:
		lvldesc = "[debug] "
	case 1:
		lvldesc = "[info] "
	case 2:
		lvldesc = "[warn] "
	case 3:
		lvldesc = "[error] "
	case 9:
		lvldesc = "[fatal] "
	case 99:
		lvldesc = "[system] "
	}

	return lvldesc
}

func init() {
	if conf.Uptime {
		roomStart := time.Now()
		go func() {
			for {
				elapsed := time.Since(roomStart)
				System("Uptime %v", elapsed)
				time.Sleep(time.Duration(1) * time.Minute)
			}
		}()
	}
}
