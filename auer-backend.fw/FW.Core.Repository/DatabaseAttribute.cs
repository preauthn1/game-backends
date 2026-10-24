using System;


namespace FW.Core.Repository
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class DatabaseAttribute :  System.Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbName"></param>
        public DatabaseAttribute(string dbName)
        {
            this.DatabaseName = dbName;
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DatabaseName
        {
            get;
            set;
        }
    }
}
