using System;
using System.Threading;
using System.Web.Script.Serialization;


namespace FW.Core.EventBus
{
    public class DatabaseEventBus : EventBusBase
    {
        private Func<EventMessage> _getDataAct;
        private Action<EventMessage> _insertDataAct;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="getDataAct"></param>
        /// <param name="insertDataAct"></param>
        public DatabaseEventBus(Func<EventMessage> getDataAct, Action<EventMessage> insertDataAct)
            :base()
        {
            _getDataAct = getDataAct;
            _insertDataAct = insertDataAct;

            start();
        }

        public void start()
        {
            Action act = () => {
                while (true)
                {
                    var evt = _getDataAct();
                    if (evt != null)
                    {
                        _consumers.ForEach(c =>
                        {
                            if (c.HookMsgID.Contains(evt.ClassID))
                            {
                                base.PublishConsumerEvent(c, evt);
                            }
                        });
                    }
                    else
                    {
                        Thread.Sleep(1000 * 10);
                    }
                }
            };

            act.BeginInvoke(null, null);
        }

        protected override void PublishEventInternal(EventMessage evt)
        {
            try
            {
                this._insertDataAct(evt);
            }
            catch (System.Exception ex)
            {
                throw new EventBusException("¼," + ex.Message, ex);
            }
        }
    }
}
