using FW.Core.Helper;
using System;
using System.Linq;


namespace FW.Core.Repository.EF
{
    public class EFRepositoryFactory : IRepositoryFactory
    {
        private DatabaseSettingCollection _settings;
        private DbContextFactory _factory;

        /// <summary>
        /// ctor 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="factory"></param>
        public EFRepositoryFactory(DatabaseSettingCollection settings, DbContextFactory factory)
        {
            _settings = settings;
            _factory = factory;
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            return new EFUnitOfWork(_factory.CreateDbContext(_settings.FirstOrDefault()));
        }

        public IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : BaseEntity
        {
            return new EFRepository<TEntity>(unitOfWork as EFUnitOfWork);
        }

        public IRepository<TEntity> CreateRepository<TEntity>() where TEntity : BaseEntity
        {
            return new EFRepository<TEntity>(null);
        }
    }
}
