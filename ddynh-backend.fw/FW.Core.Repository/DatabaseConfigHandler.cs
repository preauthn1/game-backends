using System;
using System.Linq;
using System.Configuration;
using System.Xml;

namespace FW.Core.Repository
{
    public class DatabaseConfigHandler : IConfigurationSectionHandler
    {

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            try
            {
                var settings = new DatabaseSettingCollection();
                //事务
                if (section.Attributes["transaction"] != null && section.Attributes["transaction"].Value.ToLower() == "true")
                {
                    settings.Transaction = true;
                }
                else
                {
                    settings.Transaction = false;
                }
               

                XmlNodeList settingNodes = section.SelectNodes("database");

                if (settingNodes != null && settingNodes.Count > 0)
                {
                    foreach (XmlNode node in settingNodes)
                    {
                        //连接字符串
                        string connStr = node.Attributes["connectionString"].Value;
                        bool isMaster = false;
                        bool isSlave = false;
                        bool offline = false;
                        string dbname = "";

                        //是否主库
                        if (node.Attributes["write"] != null && node.Attributes["write"].Value == "1")
                        {
                            isMaster = true;
                        }
                        //是否从库
                        if (node.Attributes["read"] != null && node.Attributes["read"].Value == "1")
                        {
                            isSlave = true;
                        }
                        //是否离线
                        if (node.Attributes["offline"] != null && node.Attributes["offline"].Value == "1")
                        {
                            offline = true;
                        }
                       
                        //数据库名
                        if (node.Attributes["name"] != null)
                        {
                            dbname = node.Attributes["name"].Value;
                        }

                        var setting = new DatabaseSetting();
                        setting.DatabaseName = dbname;
                        setting.IsSlaveDatabase = isSlave;
                        setting.IsMasterDatabase = isMaster;
                        setting.ConnectionString = connStr;
                        setting.Offline = offline;

                        settings.Add(setting);
                    }
                }
                else
                {
                    throw new RepositoryException("数据库配置信息错误");
                }

                return settings;
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("解析数据库配置信息错误", ex);
            }
        }
    }
}
