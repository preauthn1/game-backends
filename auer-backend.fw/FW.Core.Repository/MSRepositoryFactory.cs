using FW.Core.Helper;
using System;

namespace FW.Core.Repository
{
    public class MSRepositoryFactory : IRepositoryFactory
    {
        private IRepositoryProvider _provider;
        private DatabaseSettingCollection _settings;
        private bool _transaction;
        private IDatabaseSelector _selector;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="settings"></param>
        /// <param name="selector"></param>
        /// <param name="transaction"></param>
        public MSRepositoryFactory(IRepositoryProvider provider, DatabaseSettingCollection settings, IDatabaseSelector selector,
            bool transaction = true)
        {
            this._provider = provider;
            this._settings = settings;
            this._transaction = transaction;
            this._selector = selector;
        }

        /// <summary>
        /// 创建工作单元
        /// </summary>
        /// <returns></returns>
        public IUnitOfWork CreateUnitOfWork()
        {
            return new UnitOfWorkGroup(this._provider, _settings, _selector, _transaction);
        }

        /// <summary>
        /// 创建仓储
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork)
            where TEntity : BaseEntity
        {
            return new MSRepository<TEntity>(this._settings,this._provider,_selector, unitOfWork);
        }


        /// <summary>
        /// 创建仓储
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IRepository<TEntity> CreateRepository<TEntity>() where TEntity : BaseEntity
        {
            return new MSRepository<TEntity>(this._settings, this._provider, _selector);
        }
    }
}
