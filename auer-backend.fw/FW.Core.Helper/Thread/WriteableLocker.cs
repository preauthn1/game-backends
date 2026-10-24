using System;
using System.Threading;

namespace FW.Core.Helper.Thread
{
    public class WriteableLocker : IDisposable
    {
        private ReaderWriterLockSlim _innerLocker;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="locker"></param>
        public WriteableLocker(ReaderWriterLockSlim locker)
        {
            _innerLocker = locker;
            locker.EnterWriteLock();
        }

        public void Dispose()
        {
            _innerLocker.ExitWriteLock();
        }
    }
}
