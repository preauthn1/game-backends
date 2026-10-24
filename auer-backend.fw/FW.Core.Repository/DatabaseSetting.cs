using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace FW.Core.Repository
{
    /// <summary>
    /// 数据库设置集合
    /// </summary>
    public class DatabaseSettingCollection : Collection<DatabaseSetting>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DatabaseSettingCollection()
        {
            Transaction = true;
        }

        /// <summary>
        /// 开启多数据库操作时的分布式事务
        /// </summary>
        public bool Transaction
        {
            get;
            set;
        }

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IList<DatabaseSetting> GetDatabase(string name, DatabaseType type)
        {
            var settings = this.Where(s => 
                (String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(s.DatabaseName))
                || String.Compare(name, s.DatabaseName, true) == 0
                )
                .ToList();

            return settings.Where(s => 
                type == DatabaseType.All
                || (type == DatabaseType.Master && s.IsMasterDatabase)
                || (type == DatabaseType.Slave && s.IsSlaveDatabase))
                .ToList();
        }

    }

    /// <summary>
    /// 数据库设置
    /// </summary>
    public class DatabaseSetting
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DatabaseSetting()
        {
            Offline = false;
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DatabaseName
        {
            get;
            set;
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// 是否主库。主从结构，主库负责写，从库负责读
        /// </summary>
        public bool IsMasterDatabase
        {
            get;
            set;
        }

        /// <summary>
        /// 是否从库
        /// </summary>
        public bool IsSlaveDatabase
        {
            get;
            set;
        }

        /// <summary>
        /// 是否离线
        /// </summary>
        public bool Offline
        {
            get;
            set;
        }


    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        Master,
        Slave,
        All
    }

}
