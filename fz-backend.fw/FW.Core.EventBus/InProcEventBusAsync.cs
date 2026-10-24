using System;
using System.Collections;
using System.Threading;
using FW.Core.Helper.Thread;
using System.Collections.Generic;
using FW.Core.Helper.Trace;
using FW.Core.EventBus.Store;

namespace FW.Core.EventBus
{
    public class InProcEventBusAsync : InProcEventBus, IDisposable
    {
        private Queue<EventMessage> _queue = new Queue<EventMessage>();
        private ReaderWriterLockSlim _innerLockerQueue = new ReaderWriterLockSlim();
        private Thread _thWork = null;

        /// <summary>
        /// ctor
        /// </summary>
        public InProcEventBusAsync()
            :base()
        {
            start();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="store"></param>
        public InProcEventBusAsync(ITrace trace, IMessageStore store)
            : base(trace, store)
        {
            start();
        }

        protected override void PublishEventInternal(EventMessage evt)
        {
            OutputLog(String.Format("Ϣ{0}", evt.ToString()));
            using (var locker = new WriteableLocker(_innerLockerQueue))
            {
                _queue.Enqueue(evt);
            }
        }

        public  void start()
        {
            _thWork = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        EventMessage evt = null;
                        using (var locker = new WriteableLocker(_innerLockerQueue))
                        {
                            evt = _queue.Dequeue();
                        }

                        if (evt != null)
                        {
                            base.PublishEventInternal(evt);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    catch { }
                }
            });

            _thWork.IsBackground = true;
            _thWork.Start();
        }

        public void Dispose()
        {
            if (_thWork != null)
            {
                try
                {
                    _thWork.Abort();
                }
                catch { }
                _thWork = null;
            }
        }
    }
}
