using FW.Core.Helper;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.EventBus
{
    public static class EventBusHelper
    {
        /// <summary>
        /// 注册消费者
        /// </summary>
        /// <param name="mq"></param>
        /// <param name="c"></param>
        public static void RegisterEventConsumer(this IEventBus mq, EventConsumerBase c)
        {
            DateTime fetchTime = DateTime.Now.AddDays(1);
            if (c.LastDispatcherTime != null)
            {
                fetchTime = c.LastDispatcherTime.Value;
            }

            mq.RegisterEventConsumer(c.Name, c.HookMsgClassID, null, fetchTime);
        }
    }
}
