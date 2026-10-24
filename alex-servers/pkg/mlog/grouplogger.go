/*
日志组，用于同时向多个终端输出日志的场景
*/

package mlog

//日志组
type GroupLogger struct {
	loggers []Logger
}

func NewGroupLogger() (*GroupLogger, error) {
	logger := new(GroupLogger)

	return logger, nil
}

func (logger *GroupLogger) WriteLog(lvl int, format string, a ...interface{}) {
	if logger.loggers != nil {
		for _, l := range logger.loggers {
			l.WriteLog(lvl, format, a...)
		}
	}
}

func (logger *GroupLogger) Close() {
	if logger.loggers != nil {
		for _, l := range logger.loggers {
			l.Close()
		}
	}

	logger.loggers = make([]Logger, 0)
}

func (logger *GroupLogger) AddLogger(innerLogger Logger) *GroupLogger {
	logger.loggers = append(logger.loggers, innerLogger)

	return logger
}
