using System;
using System.Collections.Generic;
using System.Threading;


namespace FW.Core.Logger
{
    public class LogManager : IDisposable
    {
        private const string LOGCONFIGSECTIONNAME = "logConfig";
        private ILogFormatter _formatter;
        private ILogger _logger;
        private LogConfig _logConfig;
        private static IDictionary<string, ILogger> _loggerDict = new Dictionary<string, ILogger>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="formatter"></param>
        public LogManager(ILogger logger, ILogFormatter formatter)
        {
            if (logger == null)
            {
                throw new LogException("参数不能为空logger");
            }
            if (formatter == null)
            {
                throw new LogException("参数不能为空formatter");
            }

            _formatter = formatter;
            _logger = logger;
            try
            {
                _logConfig = System.Configuration.ConfigurationManager.GetSection(LOGCONFIGSECTIONNAME) as LogConfig;
            }
            catch (System.Exception ex)
            {
                throw new LogException("获取日志配置错误", ex);
            }
        }

        /// <summary>
        /// 默认的日志记录器
        /// </summary>
        public static LogManager Default
        {
            get
            {
                return GetLogger();
            }
        }

        /// <summary>
        /// 获取日志记录器
        /// </summary>
        /// <param name="logName"></param>
        /// <returns></returns>
        public static LogManager GetLogger(string fileName="log", string folderName="log", string extensionName="log", int? byHour=24, int? bySize=1024 * 1024 * 5)
        {
            if (_loggerDict.ContainsKey(fileName))
            {
                return new LogManager(_loggerDict[fileName], new DefaultLogFormatter());
            }
            else
            {
                ILogger logger = null;
                lock (_loggerDict)
                {
                    if (byHour != null)
                    {
                        logger = new TxtLoggerByTime(fileName, folderName, extensionName, byHour.Value);
                    }
                    else
                    {
                        logger = new TxtLoggerBySize(fileName, folderName, extensionName, bySize.Value);
                    }

                    if (!_loggerDict.ContainsKey(fileName))
                    {
                        _loggerDict.Add(fileName, logger);
                    }
                }
                var logM = new LogManager(logger, new DefaultLogFormatter());
                return logM;
            }
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logInfo"></param>
        public void WriteLog(LogInfo logInfo)
        {
            try
            {
                if (_logConfig!= null && _logConfig.IsEnablLog(logInfo))
                {
                    _logger.WriteLog(_formatter.Format(logInfo));
                }
            }
            catch (System.Exception ex)
            {
                throw new LogException("写日志错误," + logInfo.ToString(), ex);
            }
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message"></param>
        public void LogInfo(string message)
        {
            WriteLog(new LogInfo(example.invalid, message));
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message)
        {
            WriteLog(new LogInfo(LogLevel.Error, message));
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        /// <param name="message"></param>
        public void LogWarn(string message)
        {
            WriteLog(new LogInfo(LogLevel.Warn, message));
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message"></param>
        public void LogDebug(string message)
        {
            WriteLog(new LogInfo(LogLevel.Debug, message));
        }

        /// <summary>
        /// 记录严重错误
        /// </summary>
        /// <param name="message"></param>
        public void LogFatal(string message)
        {
            WriteLog(new LogInfo(LogLevel.Fatal, message));
        }


        public void Dispose()
        {
            LogManager.Dispose(this._logger.LoggerName);
        }

        /// <summary>
        /// 释放日志资源
        /// </summary>
        /// <param name="fileName"></param>
        public static void Dispose(string fileName)
        {
            lock (_loggerDict)
            {
                if (_loggerDict.ContainsKey(fileName))
                {
                    _loggerDict[fileName].Dispose();
                    _loggerDict.Remove(fileName);
                }
            }
        }
    }
}
