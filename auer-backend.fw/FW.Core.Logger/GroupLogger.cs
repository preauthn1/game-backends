using System;
using System.Collections.Generic;

namespace FW.Core.Logger
{
    public class GroupLogger : ILogger, IEnumerable<ILogger>
    {
        private List<ILogger> _innerLoggers = new List<ILogger>();

        public void AddLogger(ILogger logger)
        {
            _innerLoggers.Add(logger);
        }

        public void WriteLog(string message)
        {
            _innerLoggers.ForEach(logger => {
                logger.WriteLog(message);
            });
        }

        public IEnumerator<ILogger> GetEnumerator()
        {
            return this._innerLoggers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string LoggerName
        {
            get 
            {
                return "Group";
            }
        }

        public void Dispose()
        {
            foreach (var l in _innerLoggers)
            {
                l.Dispose();
            }
        }
    }
}
