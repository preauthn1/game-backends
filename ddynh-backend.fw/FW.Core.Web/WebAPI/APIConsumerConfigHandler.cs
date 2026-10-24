using System;
using System.Configuration;
using System.Xml;


namespace FW.Core.Web.WebAPI
{
    public class APIConsumerConfigHandler : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            APIConsumerCollection apis = new APIConsumerCollection();

            XmlNodeList apiNodes = section.SelectNodes("api");
            if (apiNodes != null && apiNodes.Count > 0)
            {
                foreach (XmlNode node in apiNodes)
                {
                    var api = new APIConsumerInfo();
                    if (node.Attributes["id"] != null)
                    {
                        api.CID = node.Attributes["id"].Value;
                    }

                    if (node.Attributes["key"] != null)
                    {
                        api.Key = node.Attributes["key"].Value; 
                    }

                    apis.Add(api);
                }
            }

            return apis;
        }
    }
}
