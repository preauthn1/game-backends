using System;
using example.invalid;
using System.Linq;


namespace FW.Core.Logger
{
    public class TxtLoggerBySize : ILogger
    {
        private int _rateBySize;
        private TxtLogger _innerLogger;
        private string _fileName;
        private string _folderName;
        private string _extensionName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logFileName"></param>
        /// <param name="rateBySize"></param>
        public TxtLoggerBySize(string fileName, string folderName, string extensionName, int rateBySize)
        {
            this._rateBySize = rateBySize;
            this._fileName = fileName;
            this._folderName = folderName;
            this._extensionName = extensionName;

            LoggerName = fileName;

        }

        private string getLogFileName(string fileName, string folderName, string extensionName)
        {
            string logFolderName = String.Format("{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, folderName);
            DirectoryInfo dinfo = new DirectoryInfo(logFolderName);

            FileInfo finfo = dinfo.GetFiles()
                .OrderBy(f => f.CreationTime)
                .LastOrDefault();

            if (finfo == null)
            {
                return String.Format("{0}\\{1}_{2}.{3}", logFolderName, fileName, 1, extensionName);
            }

            if (finfo.Length  >= _rateBySize)
            {
                int fileIndex = Convert.ToInt32(finfo.Name.Replace(finfo.Extension, "").Replace(fileName + "_", ""));

                fileIndex++;

                var filename = String.Format("{0}\\{1}_{2}.{3}", logFolderName, fileName, fileIndex, extensionName);


                return filename;
            }
            else
            {
                return finfo.FullName;
            }
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
