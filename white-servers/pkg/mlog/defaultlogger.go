package mlog

import (
	"encoding/json"
	"fmt"
	"os"
	"runtime"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

var glogger Logger
var clogger, _ = NewConsoleLogger()
var serverString string

//初始化
func init() {
	glogger = BuildTimeRollingAndConsoleLogGroup()
}

//构造定时切割和控制台日志组
func BuildTimeRollingAndConsoleLogGroup() *GroupLogger {
	gglogger, _ := NewGroupLogger()
	tlogger := CreateTimeRollingLogger()

	gglogger.AddLogger(tlogger)
	gglogger.AddLogger(clogger)

	return gglogger
}

func CreateTimeRollingLogger() *TimeRollingLogger {
	tlogger, _ := NewTimeRollingLogger(1*60*24, "log")
	return tlogger
}

func SetServerString(server string) {
	serverString = server
}

func SetGLogger(l Logger) {
	glogger = l
}

//写调试信息
func Debug(format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(DebugLevel, format, a...)
}

//写日志信息
func Info(format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(InfoLevel, format, a...)
}

//写错误信息
func Error(format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(ErrorLevel, format, a...)
}

//写崩溃信息
func Fatal(format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(FatalLevel, format, a...)
}

//写警告信息
func Warn(format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(WarnLevel, format, a...)
}

//写系統信息
func System(format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(SystemLevel, format, a...)
}

//写日志
func WriteLog(lvl int, format string, a ...interface{}) {
	logger := NewDefaultLogger()
	logger.WriteLog(lvl, format, a...)
}

//向屏幕输出信息
func Printf(format string, a ...interface{}) {
	fmt.Printf(format+"\r\n", a...)
}

//获取控制台日志
func GetCLogger() *ConsoleLogger {
	return clogger
}

type DefaultLogger struct {
	StdLog       stdLog
	CallDepLen   int
	CallDepStart int
}

type stdLog struct {
	Server     string
	Message    string
	Data       interface{}
	StackTrace []StdStackTrace
}
type StdStackTrace struct {
	File     string
	FuncName string
}

/**
有資料想記錄的使用方法
mlog.NewDefaultLogger().SetData(gs.defenseCfg).Info("连接dbserver成功.")
3/14 強制調整為重複使用實體
4/27 重複使用實例雖可降低資源損耗, 但會造成多執行續的資料錯亂, 先改回來不優化
*/

func NewDefaultLogger() *DefaultLogger {
	loggerInstance := new(DefaultLogger)
	loggerInstance.Reset()
	return loggerInstance
}

func (d *DefaultLogger) Reset() {
	d.CallDepLen = 2
	d.CallDepStart = 2
	d.StdLog = stdLog{StackTrace: make([]StdStackTrace, 0)}
	//d.StdLog.StackTrace = make([]StdStackTrace, 0)
	d.StdLog.Server = serverString
}

func (d *DefaultLogger) SetCallDepLen(i int) *DefaultLogger {
	d.CallDepLen = i
	return d
}
func (d *DefaultLogger) SetData(data interface{}) *DefaultLogger {
	d.StdLog.Data = data
	return d
}

func (d *DefaultLogger) WriteLog(lvl int, format string, a ...interface{}) {
	defer util.PanicErrStack()
	if lvl < conf.CLogLevel && lvl < conf.FLogLevel {
		return
	}

	// 注入stdLog.Message
	d.StdLog.Message = fmt.Sprintf(format, a...)

	if !conf.ClearLog {
		// 注入stdLog.StackTrace
		for i := d.CallDepStart; i < d.CallDepStart+d.CallDepLen; i++ {
			pc, file, line, _ := runtime.Caller(i)
			f := runtime.FuncForPC(pc)

			st := StdStackTrace{
				File:     fmt.Sprintf("%v:%v", file, line),
				FuncName: f.Name(),
			}
			d.StdLog.StackTrace = append(d.StdLog.StackTrace, st)
		}

		// 將 stdLog.Data 轉成字串符
		ds, _ := json.Marshal(d.StdLog.Data)
		d.StdLog.Data = string(ds)
	}
	// 將 stdLog 轉成字串符
	dataStr, _ := json.Marshal(d.StdLog)
	newFormat := "%v"
	glogger.WriteLog(lvl, newFormat, string(dataStr))
	d.Reset()
	if lvl == FatalLevel {
		os.Exit(1)
	}
}

//code stack 程式func呼叫堆疊來源
func (d *DefaultLogger) GetCallStack() stdLog {
	for i := d.CallDepStart; i < d.CallDepStart+d.CallDepLen; i++ {
		pc, file, line, _ := runtime.Caller(i)
		f := runtime.FuncForPC(pc)

		st := StdStackTrace{
			File:     fmt.Sprintf("%v:%v", file, line),
			FuncName: f.Name(),
		}

		if d.StdLog.StackTrace == nil {
			fmt.Println("is nil")

		}

		d.StdLog.StackTrace = append(d.StdLog.StackTrace, st)
	}
	//dataStr, _ := json.Marshal(d.StdLog)
	return d.StdLog
}

func (d *DefaultLogger) Debug(format string, a ...interface{}) {
	d.WriteLog(DebugLevel, format, a...)
}

//写日志信息
func (d *DefaultLogger) Info(format string, a ...interface{}) {
	d.WriteLog(InfoLevel, format, a...)
}

//写错误信息
func (d *DefaultLogger) Error(format string, a ...interface{}) {
	d.WriteLog(ErrorLevel, format, a...)
}

//写崩溃信息
func (d *DefaultLogger) Fatal(format string, a ...interface{}) {
	d.WriteLog(FatalLevel, format, a...)
}

//写警告信息
func (d *DefaultLogger) Warn(format string, a ...interface{}) {
	d.WriteLog(WarnLevel, format, a...)
}

//寫系統信息
func (d *DefaultLogger) System(format string, a ...interface{}) {
	d.WriteLog(SystemLevel, format, a...)
}
