using System;
using System.Threading;

namespace FW.Core.Helper.Thread
{
    public class ReadableLocker : IDisposable
    {
        private ReaderWriterLockSlim _innerLocker;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="locker"></param>
        public ReadableLocker(ReaderWriterLockSlim locker)
        {
            _innerLocker = locker;
            _innerLocker.EnterReadLock();
        }

        /// <summary>
        /// dispose 释放读锁
        /// </summary>
        public void Dispose()
        {
            _innerLocker.ExitReadLock();
        }
    }
}
