using System;

namespace FW.Core.Repository
{
    public interface IDatabaseProvider
    {
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        IDatabase CreateDatabase(DatabaseSetting setting);
    }
}
