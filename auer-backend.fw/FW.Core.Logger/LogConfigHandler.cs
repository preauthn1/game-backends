using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using System.Linq;


namespace FW.Core.Logger
{
    public class LogConfigHandler : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            LogConfig config = new LogConfig();

            //日志等级
            XmlNode enableNode = section.SelectSingleNode("//enable");
            if (enableNode != null)
            {
                config.EnableLevel.Clear();
                string levels = enableNode.InnerText;
                string[] levelArr = levels.Split(new char[] { ',' });
                if (levelArr != null && levelArr.Count() > 0)
                {
                    Array.ForEach(levelArr, s => {
                        config.EnableLevel.Add((LogLevel)(Enum.Parse(typeof(LogLevel),s)));
                    });
                }
            }
         
            return config;
        }
    }
}
