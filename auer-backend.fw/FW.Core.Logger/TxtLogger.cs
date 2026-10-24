using System;
using example.invalid;
using FW.Core.Helper;


namespace FW.Core.Logger
{
    public class TxtLogger : ILogger
    {
        /// <summary>
        /// 日志文件名
        /// </summary>
        protected string _logFileName ;

        /// <summary>
        /// 文件流
        /// </summary>
        private StreamWriter swLog = null;

        /// <summary>
        /// ctor
        /// </summary>
        public TxtLogger()
            :this("log", "log", "log")
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logFileFullName"></param>
        public TxtLogger(string logFileFullName)
        {
            this._logFileName = logFileFullName;
            FileInfo finfo = new FileInfo(_logFileName);
            if (!Directory.Exists(finfo.DirectoryName))
            {
                Directory.CreateDirectory(finfo.DirectoryName);
            }

            swLog = new StreamWriter(_logFileName, true);
            LoggerName = finfo.Name;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="folderName"></param>
        /// <param name="extensionName"></param>
        public TxtLogger(string fileName = "log", string folderName = "log", string extensionName = "log")
        {
            string logFolderName = String.Format("{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, folderName);
            ///确保目录存在
            DirectoryInfo dinfo = new DirectoryInfo(logFolderName);
            if (!dinfo.Exists)
            {
                dinfo.Create();
            }

            _logFileName = String.Format("{0}\\{1}.{2}", logFolderName, fileName, extensionName);
            swLog = new StreamWriter(_logFileName, true,System.Text.Encoding.UTF8,1024 * 10);

            LoggerName = fileName;
        }

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message"></param>
        public void WriteLog(string message)
        {
            lock (this)
            {
                swLog.WriteLine(message);
                swLog.Flush();
            }
        }

        public void Dispose()
        {
            if (swLog != null)
            {
                swLog.Flush();
                swLog.Close();
                swLog.Dispose();

                swLog = null;
            }
        }

        /// <summary>
        /// 日志文件名
        /// </summary>
        public string LogFileName
        {
            get
            {
                return _logFileName;
            }
        }


        public string LoggerName
        {
            get;
            private set;
        }
    }

}
