using System;
using System.Collections.Generic;


namespace FW.Core.Repository
{
    public interface IDatabaseSelector
    {
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        string GetDatabase<TEntity>();

        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        string GetDatabase(Type entityType);
    }
}
