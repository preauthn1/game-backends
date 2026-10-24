using System;
using System.Configuration;


namespace FW.Core.Web.WebAPI
{
    public class ConfigAPIConsumerProvider : IAPIConsumerProvider
    {
        private APIConsumerCollection _apis;

        public ConfigAPIConsumerProvider()
        {
            _apis = System.Configuration.ConfigurationManager.GetSection("apiConfig") as APIConsumerCollection;
        }

        public APIConsumerCollection GetApiConsumers()
        {
            return _apis;    
        }

        
    }
}
