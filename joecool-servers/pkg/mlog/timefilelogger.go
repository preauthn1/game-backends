/*
基于时间定时切割的日志类
*/

package mlog

import (
	"fmt"
	"path"
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

// NewTimeRollingLogger 初始化
func NewTimeRollingLogger(rollingInterval int, pathname string) (*TimeRollingLogger, error) {
	//创建日志对象函数
	var f = func() *FileLogger {
		var t = time.Now()
		filename := fmt.Sprintf("%d%02d%02d_%02d_%02d_%02d.log",
			t.Year(),
			t.Month(),
			t.Day(),
			t.Hour(),
			t.Minute(),
			t.Second())

		ff, _ := NewFileLogger(path.Join(pathname, filename))

		return ff
	}

	var trlogger = new(TimeRollingLogger)

	trlogger.filelastacctime = time.Now()
	trlogger.filelocker = new(sync.RWMutex)
	trlogger.logintervalmin = rollingInterval
	trlogger.loggerCreateFunc = f
	//创建日志对象实例
	trlogger.innerLogger = trlogger.loggerCreateFunc()

	//检查日志切割
	go func() {

		defer util.PanicErrStack()

		for {

			time.Sleep(time.Minute * time.Duration(trlogger.logintervalmin))

			ll := trlogger.loggerCreateFunc()
			trlogger.filelocker.Lock()
			if trlogger.innerLogger != nil {
				trlogger.innerLogger.Close()
			}

			trlogger.innerLogger = ll
			trlogger.filelocker.Unlock()
		}
	}()

	return trlogger, nil
}

// TimeRollingLogger 基于时间定时切割的日志类
type TimeRollingLogger struct {
	filelocker       *sync.RWMutex      //同步锁
	filelastacctime  time.Time          //最后一次日志切割时间
	logintervalmin   int                //日志切割时间间隔，单位分钟
	loggerCreateFunc func() *FileLogger //创建日志实例函数
	innerLogger      *FileLogger        //内部的日志对象实例
}

// WriteLog 输出日志
func (logger *TimeRollingLogger) WriteLog(lvl int, format string, a ...interface{}) {
	if lvl < conf.FLogLevel {
		return
	}
	logger.filelocker.RLock()
	defer logger.filelocker.RUnlock()

	if logger.innerLogger == nil {
		return
	}

	logger.innerLogger.WriteLog(lvl, format, a...)
}

// Close ?
func (logger *TimeRollingLogger) Close() {
	if logger.innerLogger != nil {
		logger.filelocker.Lock()
		defer logger.filelocker.Unlock()

		logger.innerLogger.Close()
	}
}
