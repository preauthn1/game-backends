using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FW.Core.AdminSys.Provider
{
    public class AdminSysProviderConfigHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            XmlNode ptypeNode = section.SelectSingleNode("//providerType");

            XmlNode connNode = section.SelectSingleNode("//nameOrConnectionString");

            var config = new AdminSysConfig() { AdminSysProviderType = typeof(EFProvider.EFAdminSysProvider), NameOrConnectionString = "adminSysConnstr" };
            if (ptypeNode == null && connNode == null)
            {
                return config;
            }

            if (ptypeNode != null)
            {
                config.AdminSysProviderType = Type.GetType(ptypeNode.InnerText);
            }
            if (connNode != null)
            {
                config.NameOrConnectionString = connNode.InnerText;
            }

            return config;
        }
    }

    public class AdminSysConfig
    {
        public Type AdminSysProviderType
        {
            get;
            set;
        }

        public string NameOrConnectionString
        {
            get;
            set;
        }
    }
}
