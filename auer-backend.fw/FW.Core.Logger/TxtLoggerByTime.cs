using System;


namespace FW.Core.Logger
{
    public class TxtLoggerByTime : ILogger
    {
        private int _rateByHour;
        private TxtLogger _innerLogger;
        private string _fileName;
        private string _folderName;
        private string _extensionName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderName"></param>
        /// <param name="extensionName"></param>
        /// <param name="rateByHour"></param>
        public TxtLoggerByTime(string fileName, string folderName, string extensionName, int rateByHour)
        {
            if (rateByHour < 1 || rateByHour > 24)
            {
                throw new LogException("日志分片必须是1-24以内的数字");
            }

            this._rateByHour = rateByHour;
            this._fileName = fileName;
            this._folderName = folderName;
            this._extensionName = extensionName;

            LoggerName = fileName;

            _innerLogger = new TxtLogger(getLogFileName(fileName, folderName, extensionName));
        }

        private  string getLogFileName(string fileName, string folderName, string extensionName)
        {
            int currentHour = DateTime.Now.Hour;
            int fileIndex = 1;
            if (_rateByHour != 0)
            {
                fileIndex = currentHour / _rateByHour + 1;
            }
            
            if (fileIndex == 1)
            {
                return String.Format("{0}{1}\\{2}_{3}.{4}", 
                    System.AppDomain.CurrentDomain.BaseDirectory,
                    folderName,
                    fileName, 
                    DateTime.Now.ToString("yyyyMMdd"),
                    extensionName);
            }

            return String.Format("{0}{1}\\{2}_{3}_{4}.{5}",
                System.AppDomain.CurrentDomain.BaseDirectory,
                folderName,
                fileName,
                DateTime.Now.ToString("yyyyMMdd"), 
                fileIndex,
                extensionName);
        }

        public void WriteLog(string message)
        {
            string filename = this.getLogFileName(_fileName, _folderName, _extensionName);
            if (filename != _innerLogger.LogFileName)
            {
                _innerLogger.Dispose();
                _innerLogger = new TxtLogger(filename);
            }

            _innerLogger.WriteLog(message);
        }

        public void Dispose()
        {
            _innerLogger.Dispose();
        }

        public string LoggerName
        {
            get;
            private set;
        }
    }
}
