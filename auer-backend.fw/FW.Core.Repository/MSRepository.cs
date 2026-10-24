using FW.Core.Helper;
using System;
using System.Linq;
using System.Transactions;

namespace FW.Core.Repository
{
    public class MSRepository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        private DatabaseSettingCollection _settings;
        private IRepositoryProvider _provider;
        private IUnitOfWork _unitOfWork;
        private IDatabaseSelector _selector;
        private bool _autoCommit = false;
     
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="provider"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="selector"></param>
        public MSRepository(DatabaseSettingCollection settings, IRepositoryProvider provider, IDatabaseSelector selector, IUnitOfWork unitOfWork = null)
        {
            FW.Core.Helper.Check.NotEmpty<DatabaseSettingCollection>(settings, "settings");
            FW.Core.Helper.Check.NotEmpty<IRepositoryProvider>(provider, "provider");
            FW.Core.Helper.Check.NotEmpty<IDatabaseSelector>(selector, "selector");
           
            _settings = settings;
            _provider = provider;
            _unitOfWork = unitOfWork;
            _selector = selector;

            if (_unitOfWork == null)
            {
                _autoCommit = true;
            }
        }

        public TEntity GetById(int id)
        {
            return Entities.Where(t => t.Id == id).FirstOrDefault();
        }

        public IQueryable<TEntity> Entities
        {
            get 
            {
                var setting = getDbSettingForRead();

                return _provider.CreateRepository<TEntity>(_provider.CreateUnitOfWork(setting)).Entities;
            }
        }

        public TEntity Insert(TEntity entity)
        {
            checkUnitOfWork();
            
            var ret = _unitOfWork.RegisterAdded<TEntity>(entity);
            if (_autoCommit)
                _unitOfWork.SaveChanges();

            return ret;
        }

        public TEntity Update(TEntity entity)
        {
            checkUnitOfWork();
            var ret = _unitOfWork.RegisterChangeded<TEntity>(entity);

            if (_autoCommit)
                _unitOfWork.SaveChanges();

            return ret;
        }

        public TEntity Delete(TEntity entity)
        {
            checkUnitOfWork();
            var ret = _unitOfWork.RegisterDeleted<TEntity>(entity);

            if (_autoCommit)
                _unitOfWork.SaveChanges();

            return ret;
        }

        private void checkUnitOfWork()
        {
            if (_unitOfWork == null)
            {
                lock (this)
                {
                    if (_unitOfWork == null)
                    {
                        var setting = getDbSettingForRead();
                        _unitOfWork = _provider.CreateUnitOfWork(setting);
                    }
                }
            }
        }

        private DatabaseSetting getDbSettingForRead()
        {
            string dbName = _selector.GetDatabase<TEntity>();
            var setting = _settings.GetDatabase(dbName, DatabaseType.Slave)
                .OrderBy(s => Guid.NewGuid())
                .FirstOrDefault();

            return setting;
        }
    }
}
