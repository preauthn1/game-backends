using FW.Core.Helper.Trace;
using FW.Core.Logger;
using System;
using System.Collections.Generic;
using System.ServiceModel;


namespace FW.Core.EventBus
{
    [ServiceContract]
    public interface IEventConsumer
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="evt"></param>
        [OperationContract(IsOneWay=true)]
        void HandleEvent(EventMessage evt);

    }

}
