using FW.Core.Helper;
using System;

namespace FW.Core.Repository
{
    public interface IRepositoryFactory
    {
        /// <summary>
        /// 创建工作单元
        /// </summary>
        /// <returns></returns>
        IUnitOfWork CreateUnitOfWork();

        /// <summary>
        /// 创建仓储
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork)
            where TEntity : BaseEntity;

        /// <summary>
        /// 创建仓储
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IRepository<TEntity> CreateRepository<TEntity>()
            where TEntity : BaseEntity;
    }
}
