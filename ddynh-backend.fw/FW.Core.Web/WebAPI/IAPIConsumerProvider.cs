using System;

namespace FW.Core.Web.WebAPI
{
    public interface IAPIConsumerProvider
    {
        APIConsumerCollection GetApiConsumers();
    }
}
