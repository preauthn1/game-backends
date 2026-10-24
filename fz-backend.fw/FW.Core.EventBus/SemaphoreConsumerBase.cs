using FW.Core.Helper.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FW.Core.EventBus
{
    /// <summary>
    /// 基于限流模型消费者基类
    /// </summary>
    public abstract class SemaphoreConsumerBase : EventConsumerBase
    {
        private Semaphore _semaphore = null;

        public SemaphoreConsumerBase(int maximumCount)
            :base()
        {
            _semaphore = new Semaphore(maximumCount, maximumCount);
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="maximumCount"></param>
        public SemaphoreConsumerBase(ITrace trace, int maximumCount)
            :base(trace)
        {
            _semaphore = new Semaphore(maximumCount, maximumCount);
        }


        public override void HandleEvent(EventMessage evt)
        {
            Action act = () =>
            {
                _semaphore.WaitOne();
                try
                {
                    HandleAsyncEventCore(evt);
                    UpdateDispatcherInfo();

                    string msg = String.Format("处理消息{0}完成", evt.ToString());
                    LogInfo(msg);
                }
                catch (System.Exception ex)
                {
                    string errmsg = String.Format("处理消息{0}出错,{1}", evt.ToString(), ex.ToString());
                    LogError(errmsg);
                }
                finally
                {
                    _semaphore.Release();
                }
            };
            act.BeginInvoke(null, null);
        }

        protected abstract void HandleAsyncEventCore(EventMessage evt);

        protected override void HandleEventInternal(EventMessage evt)
        {
            throw new NotImplementedException();
        }

    
    }
}
