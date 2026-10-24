using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using FW.Core.Helper.Thread;
using System.Transactions;
using System.Data.Entity.Validation;
using System.Text;
using FW.Core.Helper;


namespace FW.Core.Repository
{
    public class UnitOfWorkGroup : IUnitOfWork
    {

        private IRepositoryProvider _provider;
        private DatabaseSettingCollection _settings;
        private bool _transaction = true;
        private IDatabaseSelector _selector;
        private IDictionary<string, List<IUnitOfWork>> _unitOfWorks = new Dictionary<string, List<IUnitOfWork>>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="settings"></param>
        /// <param name="transaction"></param>
        /// <param name="selector"></param>
        public UnitOfWorkGroup(IRepositoryProvider provider, DatabaseSettingCollection settings, IDatabaseSelector selector, bool transaction = true)
        {
            this._provider = provider;
            this._settings = settings;
            this._transaction = transaction;
            this._selector = selector;
        }

        /// <summary>
        /// 获取工作单元
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        private List<IUnitOfWork> getUnitOfWork<TEntity>()
        {
            string dbName = _selector.GetDatabase<TEntity>();
            var settings = _settings.GetDatabase(dbName, DatabaseType.Master);

            if (settings == null || settings.Count() == 0)
            {
                throw new RepositoryException("找不到" + typeof(TEntity).Name + "所对应的数据库");
            }

            if (_unitOfWorks.ContainsKey(dbName))
            {
                return _unitOfWorks[dbName];
            }

            var uws = new List<IUnitOfWork>();

            foreach (var s in settings)
            {
                uws.Add(_provider.CreateUnitOfWork(s));
            }

            _unitOfWorks.Add(dbName, uws);

            return uws;
        }

        public TEntity RegisterAdded<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            TEntity ret = entity;
            getUnitOfWork<TEntity>().ForEach(uw =>
            {
                ret = uw.RegisterAdded<TEntity>(entity);
            });

            return ret;
        }

        public TEntity RegisterChangeded<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            TEntity ret = entity;
            getUnitOfWork<TEntity>().ForEach(uw =>
            {
                ret = uw.RegisterChangeded<TEntity>(entity);
            });

            return ret;
        }

        public TEntity RegisterDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            TEntity ret = entity;
            getUnitOfWork<TEntity>().ForEach(uw =>
            {
                ret = uw.RegisterDeleted<TEntity>(entity);
            });

            return ret;
        }

        public void SaveChanges()
        {

            if (_unitOfWorks.Count == 0)
            {
                return;
            }

            if ((_unitOfWorks.Values.Where(u => u.Count > 1).Count() >= 1 || _unitOfWorks.Count > 1) && _transaction)
            {
                try
                {
                    using (var ts = new TransactionScope())
                    {
                        _unitOfWorks.Values.ToList().ForEach(uws =>
                        {
                            uws.ForEach(u =>
                            {
                                u.SaveChanges();
                            });
                        });

                        ts.Complete();
                    }
                }
                catch (System.Exception ex)
                {
                    _unitOfWorks.Clear();
                   
                    throw new RepositoryException(ex.Message, ex);
                }
            }
            else
            {
                try
                {
                    _unitOfWorks.Values.ToList().ForEach(uws =>
                    {
                        uws.ForEach(u =>
                        {
                            u.SaveChanges();
                        });
                    });
                }
                catch (System.Exception ex)
                {
                    _unitOfWorks.Clear();

                    throw new RepositoryException(ex.Message, ex);
                }

            }

            _unitOfWorks.Clear();

        }

        public void Dispose()
        {
            if (_unitOfWorks.Count > 0)
            {
                this.SaveChanges();
            }
        }
    }
}
