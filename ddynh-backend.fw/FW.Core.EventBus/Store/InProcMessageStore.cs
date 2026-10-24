using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FW.Core.Helper.Thread;

namespace FW.Core.EventBus.Store
{
    public class InProcMessageStore : IMessageStore
    {
        private List<EventMessage> _msgList = new List<EventMessage>();
        private ReaderWriterLockSlim _innerLocker = new ReaderWriterLockSlim();
        private int _keepMsgCount = 10000 * 10;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="keepMsgCount"></param>
        public InProcMessageStore(int keepMsgCount = 10000 * 10)
        {
            _keepMsgCount = keepMsgCount;
        }

        public bool SaveMessage(EventMessage msg)
        {
            using (var locker = new WriteableLocker(_innerLocker))
            {
                if (_msgList.Count > _keepMsgCount)
                {
                    _msgList.Clear();
                }
                _msgList.Add(msg);
            }

            return true;
        }

        public List<EventMessage> GetMessage(List<int> classID, DateTime? from, DateTime? end, int? count)
        {
            using (var locker = new ReadableLocker(_innerLocker))
            {
                return _msgList.Where(m => (classID == null || classID.Contains(m.ClassID))
                    && (from == null || from.Value < m.EventTime)
                    && (end == null || end.Value > m.EventTime)
                    )
                    .ToList();
            }
        }
    }
}
