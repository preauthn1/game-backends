using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;


namespace FW.Core.Engine
{
    public class EngineConfigHandler : IConfigurationSectionHandler
    {

        private const char INJECTDLLSPLITCHAR = ',';

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {

            var engineConfig = new EngineConfig();


            string injectDllStr = "";
            var discoveryNode = section.SelectSingleNode("//injectDlls");
            if (discoveryNode != null)
            {
                injectDllStr = discoveryNode.InnerText;
            }

            List<string> lstDLL = new List<string>();
            if (!String.IsNullOrWhiteSpace(injectDllStr))
            {
                var arr = injectDllStr.Split(new char[] { INJECTDLLSPLITCHAR });
                Array.ForEach(arr, dll =>
                {
                    if (!String.IsNullOrWhiteSpace(dll))
                    {
                        if (!dll.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase) && !dll.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
                        {
                            dll = dll + ".dll";
                        }

                        lstDLL.Add(dll);
                    }
                });
            }
            engineConfig.InjectDll = lstDLL.ToArray();


            IEngineProvider engineProvider = new AutofacEngineProvider();
            var providerNode = section.SelectSingleNode("//engineProvider");
            if (providerNode != null)
            {
                string providerTypeString = providerNode.InnerText.Trim();

                if (!String.IsNullOrWhiteSpace(providerTypeString))
                {
                    Type providerType = Type.GetType(providerTypeString);
                    engineConfig.EngineProvider = Activator.CreateInstance(providerType) as IEngineProvider;
                }
            }

            return engineConfig;

        }
    }
}
