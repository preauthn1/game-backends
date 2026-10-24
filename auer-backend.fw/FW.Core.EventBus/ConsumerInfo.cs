using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.EventBus
{

    /// <summary>
    /// 消费者信息，消息队列服务器维护消费者队列信息
    /// </summary>
    public class ConsumerInfo
    {
        /// <summary>
        /// 消费者
        /// </summary>
        public IEventConsumer Consumer
        {
            get;
            set;
        }

        /// <summary>
        /// 消费者名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 侦听的消息分类
        /// </summary>
        public List<int> HookMsgID
        {
            get;
            set;
        }

        /// <summary>
        /// 最后一次接受消息时间
        /// </summary>
        public DateTime LastDispatchTime
        {
            get;
            set;
        }

    }
}
