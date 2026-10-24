using System;

namespace FW.Core.Repository
{
    public interface IDatabaseFactory
    {
       
        /// <summary>
        /// 获取只读数据库
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IDatabase GetDatabase(string dbName = "", DatabaseType type = DatabaseType.All);
    }
}
