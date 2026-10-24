using FW.Core.Helper.Thread;
using FW.Core.Logger;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Helper.Trace;
using FW.Core.EventBus.Store;

namespace FW.Core.EventBus
{
    /// <summary>
    /// 事件总线接口定义
    /// </summary>
    [ServiceContract(CallbackContract=typeof(IEventConsumer))]
    public interface IEventBus : IEventPublisher
    {
        [OperationContract]
        void RegisterEventConsumer(string name, List<int> msgList, IEventConsumer consumer, DateTime lastRunTime);
    }


    /// <summary>
    /// 事件总线基类
    /// </summary>
    public abstract class EventBusBase : IEventBus
    {
        protected List<ConsumerInfo> _consumers = new List<ConsumerInfo>();
        protected object _consumerLockObject = new object();
        protected ITrace Trace;
        protected IMessageStore Store;

        /// <summary>
        /// ctor
        /// </summary>
        public EventBusBase()
        {
            Trace = new EmptyTrace();
            Store = new EmptyMessageStore();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="store"></param>
        public EventBusBase(ITrace trace, IMessageStore store)
        {
            Trace = trace;
            Store = store;
        }

        public virtual void RegisterEventConsumer(string name, List<int> msgList, IEventConsumer consumer, DateTime lastRunTime)
        {
            FW.Core.Helper.Check.NotEmpty<IEventConsumer>(consumer, "consumer");

            ConsumerInfo cinfo = null;
            lock(_consumerLockObject)
            {
               if(_consumers.Where(c => String.Compare(c.Name, name, true) == 0).Count() == 0)
               {
                   cinfo = new ConsumerInfo();
                   cinfo.Name = name;
                   cinfo.HookMsgID = new List<int>(msgList);
                   cinfo.Consumer = consumer;
                   cinfo.LastDispatchTime = lastRunTime;

                   _consumers.Add(cinfo);
               }
            }

            OutputLog(String.Format("事件侦听器{0}注册成功,侦听消息分类:{1}", name, String.Join(",", msgList.ToArray())));

            //检查是否有未分派的消息
            CheckUnDispatcherMessage(cinfo, lastRunTime);
        }

        /// <summary>
        /// 检查是否有未分派的消息
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="lastRunTime"></param>
        protected void CheckUnDispatcherMessage(ConsumerInfo consumer, DateTime lastRunTime)
        {
            var msgs = Store.GetMessage(consumer.HookMsgID, lastRunTime, null, null);
            if (msgs != null && msgs.Count > 0)
            {
                Action act = () =>
                    {
                        OutputLog(String.Format("开始向{0}分派历史消息", consumer.Name));
                        foreach (var msg in msgs)
                        {
                            PublishConsumerEvent(consumer, msg);
                        }
                    };
                act.BeginInvoke(null, null);
            }
        }

        /// <summary>
        /// 向订阅者分发消息
        /// </summary>
        /// <param name="c"></param>
        /// <param name="evt"></param>
        protected virtual void PublishConsumerEvent(ConsumerInfo c, EventMessage evt)
        {
            try
            {
                OutputLog(String.Format("开始向{0}分发消息{1}", c.Name, evt.ToString()));
                if (c.Consumer == null)
                {
                    return;
                }

                c.Consumer.HandleEvent(evt);
                OutputLog(String.Format("向{0}分发消息{1}成功", c.Name, evt.ToString()));
            }
            catch (System.Exception ex)
            {
                OutputLog(String.Format("向{0}分发消息{1}失败,{2}", c.Name, evt.ToString(),ex.ToString()));
                OnPublishError(c, evt, ex);
            }
        }

        /// <summary>
        /// 分发消息错误
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="ex"></param>
        protected virtual void OnPublishError(ConsumerInfo consumer,EventMessage evt, System.Exception ex)
        {
            OutputError(String.Format("向{0}分发消息{1}失败,{2}", consumer.Name, evt.ToString(), ex.ToString()));
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="evt"></param>
        public virtual void PublishEvent(EventMessage evt)
        {
            Store.SaveMessage(evt);

            PublishEventInternal(evt);
        }

        /// <summary>
        /// 发布消息实现
        /// </summary>
        /// <param name="evt"></param>
        protected abstract void PublishEventInternal(EventMessage evt);

        /// <summary>
        /// 获取日志记录
        /// </summary>
        /// <returns></returns>
        protected LogManager GetLogger()
        {
            return LogManager.GetLogger("msgqueue");
        }

        /// <summary>
        /// 输出消息
        /// </summary>
        /// <param name="message"></param>
        protected void OutputLog(string message)
        {
            GetLogger().LogInfo(message);

            if (Trace == null)
            {
                return;
            }

            Trace.TraceInfo(message);
        }

        /// <summary>
        /// 输出错误
        /// </summary>
        /// <param name="message"></param>
        protected void OutputError(string message)
        {
            GetLogger().LogError(message);
            if (Trace == null)
            {
                return;
            }

            Trace.TraceError(message);
        }
    }

    public class EmptyEventBus : IEventBus
    {
        public void RegisterEventConsumer(string name, List<int> msgList, IEventConsumer consumer, DateTime lastRunTime)
        {
            
        }

        public void PublishEvent(EventMessage evt)
        {
            
        }
    }

}
