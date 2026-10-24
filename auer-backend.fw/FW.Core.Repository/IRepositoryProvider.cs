using FW.Core.Helper;
using System;

namespace FW.Core.Repository
{
    public interface IRepositoryProvider
    {
        /// <summary>
        /// 创建工作单元
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        IUnitOfWork CreateUnitOfWork(DatabaseSetting setting);

        /// <summary>
        /// 创建仓储
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork)
            where TEntity : BaseEntity;
    }
}
