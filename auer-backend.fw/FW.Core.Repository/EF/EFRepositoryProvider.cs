using FW.Core.EventBus;
using FW.Core.Helper;
using System;

namespace FW.Core.Repository.EF
{
    public class EFRepositoryProvider : IRepositoryProvider
    {
        private DbContextFactory _factory;
        private IEventPublisher _eventPublisher;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="factory"></param>
        public EFRepositoryProvider( DbContextFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="eventPublisher"></param>
        public EFRepositoryProvider(DbContextFactory factory, IEventPublisher eventPublisher)
        {
            _factory = factory;
            _eventPublisher = eventPublisher;
        }

        public IUnitOfWork CreateUnitOfWork(DatabaseSetting setting)
        {
            return new EFUnitOfWork(_factory.CreateDbContext(setting), _eventPublisher);
        }

        public IRepository<TEntity> CreateRepository<TEntity>(IUnitOfWork unitOfWork) where TEntity : BaseEntity
        {
            return new EFRepository<TEntity>(unitOfWork as EFUnitOfWork);
        }
    }
}
