using System;
using System.ServiceModel;


namespace FW.Core.EventBus
{
    [ServiceContract]
    public interface IEventPublisher
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="evt"></param>
        [OperationContract]
        void PublishEvent(EventMessage evt);

    }
}
