using FW.Core.EventBus.Store;
using FW.Core.Helper.Thread;
using FW.Core.Helper.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FW.Core.EventBus
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single,ConcurrencyMode=ConcurrencyMode.Single)]
    public class RemoteEventBus : InProcEventBusAsync
    {
        /// <summary>
        /// ctor
        /// </summary>
        public RemoteEventBus()
            : base()
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="store"></param>
        public RemoteEventBus(ITrace trace, IMessageStore store)
            : base(trace, store)
        { }

        public override void RegisterEventConsumer(string name, List<int> msgList, IEventConsumer consumer, DateTime lastRunTime)
        {
            ConsumerInfo cinfo = null;
            var evtConsumer = OperationContext.Current.GetCallbackChannel<IEventConsumer>();
           lock(_consumerLockObject)
            {
                cinfo = new ConsumerInfo();
                cinfo.Name = name;
                cinfo.HookMsgID = new List<int>(msgList);
                cinfo.Consumer = evtConsumer;
                cinfo.LastDispatchTime = lastRunTime;

                _consumers.Add(cinfo);
            }

            (evtConsumer as ICommunicationObject).Faulted +=RemoteEventBus_Faulted;

            OutputLog(String.Format("事件侦听器{0}注册成功,侦听消息分类:{1}", name, String.Join(",",msgList.ToArray())));
            base.CheckUnDispatcherMessage(cinfo, lastRunTime);
        }

        private void RemoteEventBus_Faulted(object sender, EventArgs e)
        {
            var consumer = sender as IEventConsumer;
            lock(_consumerLockObject)
            {
                var cinfo = _consumers.Where(c => c.Consumer == consumer).FirstOrDefault();
                if (cinfo != null)
                {
                    OutputLog(String.Format("事件侦听器{0}出现错误,已经被移除", cinfo.Name));
                    _consumers.Remove(cinfo);
                }
            }
        }

        protected override void OnPublishError(ConsumerInfo consumer,  EventMessage evt,Exception ex)
        {
            Action act = () =>
                {
                    lock(_consumerLockObject)
                    {
                        if (_consumers.Contains(consumer))
                        {
                            OutputLog(String.Format("事件侦听器{0}出现错误,已经被移除", consumer.Name));
                            _consumers.Remove(consumer);
                        }
                    }

                    base.OnPublishError(consumer, evt, ex);
                };
            act.BeginInvoke(null, null);
        }
    }
}
