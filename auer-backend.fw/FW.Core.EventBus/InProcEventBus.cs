using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using FW.Core.Helper.Thread;
using FW.Core.Helper.Trace;
using FW.Core.EventBus.Store;


namespace FW.Core.EventBus
{
    public class InProcEventBus : EventBusBase
    {
        public InProcEventBus()
            : base()
        { }

        public InProcEventBus(ITrace trace, IMessageStore store)
            : base(trace, store)
        { }


        protected override void PublishEventInternal(EventMessage evt)
        {
            lock (_consumerLockObject)
            {
                _consumers.ForEach(c =>
                {
                    if (c.HookMsgID.Contains(evt.ClassID))
                    {
                        OutputLog(String.Format("{0}Ϣ{1}", c.Name, evt.ToString()));
                        PublishConsumerEvent(c, evt);
                    }
                });
            }

        }

       

    }
}
