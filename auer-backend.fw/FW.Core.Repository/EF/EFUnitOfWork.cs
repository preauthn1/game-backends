using FW.Core.EventBus;
using FW.Core.EventBus.EntityEvents;
using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;

namespace FW.Core.Repository.EF
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private DbContext _dbContext;
        private IEventPublisher _eventPublisher;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContext"></param>
        public EFUnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="eventPublisher"></param>
        public EFUnitOfWork(DbContext dbContext, IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public IQueryable<TEntity> GetQueryable<TEntity>()
            where TEntity : BaseEntity
        {
            DbQuery<TEntity> entityQuery = _dbContext.Set<TEntity>().AsNoTracking();
            var includeMethod = typeof(DbQuery<TEntity>).GetMethod("Include");

            List<string> complexPs = new List<string>();
            getComplexProperties(typeof(TEntity), "", complexPs);
            foreach (string pName in complexPs)
            {
                entityQuery = includeMethod.Invoke(entityQuery, new Object[] { pName }) as DbQuery<TEntity>;
            }
            
            return entityQuery.AsNoTracking();
        }

        private void getComplexProperties(Type type, string prefix, List<string> sarr)
        {
            var ps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var p in ps)
            {
                if (typeof(BaseEntity).IsAssignableFrom(p.PropertyType))
                {
                    if (!prefix.Contains(p.Name + ".") && new System.Text.RegularExpressions.Regex("\\.").Matches(prefix).Count <= 2)
                    {
                        sarr.Add(String.Format("{0}{1}", prefix, p.Name));
                        getComplexProperties(p.PropertyType, prefix +  p.Name + ".", sarr);
                    }
                }
            }
        }

        public TEntity RegisterAdded<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            return _dbContext.Set<TEntity>().Add(entity);
        }

        public TEntity RegisterChangeded<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var type = entity.GetType();
            clearEntity(type, entity);

            if (_dbContext.Entry<TEntity>(entity).State == EntityState.Detached)
            {
                _dbContext.Set<TEntity>().Attach(entity);
            }

            _dbContext.Entry<TEntity>(entity).State = EntityState.Modified;

            return entity;
        }

        public TEntity RegisterDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var type = entity.GetType();
            clearEntity(type, entity);

            if (_dbContext.Entry<TEntity>(entity).State == EntityState.Detached)
            {
                _dbContext.Set<TEntity>().Attach(entity);
            }

            _dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;

            return entity;
        }

        private void clearEntity(Type type, object entity)
        {
            var ps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var p in ps)
            {
                if (typeof(BaseEntity).IsAssignableFrom(p.PropertyType))
                {
                    p.SetValue(entity, null);
                }
            }
        }

        public void SaveChanges()
        {
            try
            {
                ///¼
                if (_eventPublisher != null)
                {
                    foreach(DbEntityEntry entity in _dbContext.ChangeTracker.Entries())
                    {
                        if (entity.State == EntityState.Added)
                        {
                            _eventPublisher.EntityInserted(entity.Entity);
                        }
                        if (entity.State == EntityState.Deleted)
                        {
                            _eventPublisher.EntityDeleted(entity.Entity);
                        }
                        if (entity.State == EntityState.Modified)
                        {
                            _eventPublisher.EntityUpdated(entity.Entity);
                        }
                    }
                }

                _dbContext.SaveChanges();
            }
            catch (System.Exception ex)
            {
                StringBuilder sbMessage = new StringBuilder();
                sbMessage.Append("ʵĵݿʧ.");
                if (ex is DbEntityValidationException)
                {
                    sbMessage.Append(getEntityValidationErrorMessage(ex as DbEntityValidationException));
                }
                else
                {
                    sbMessage.Append(ex.Message);
                }

                throw new RepositoryException(sbMessage.ToString(), ex);
            }
        }

        public void Dispose()
        {
            this.SaveChanges();
        }

        private string getEntityValidationErrorMessage(DbEntityValidationException vex)
        {
            StringBuilder sbMessage = new StringBuilder();

            foreach (var err in vex.EntityValidationErrors)
            {
                err.ValidationErrors.ToList().ForEach(
                    e =>
                    {
                        sbMessage.Append(e.ErrorMessage);
                        sbMessage.Append(".");
                    });
            }

            return sbMessage.ToString();
        }

    }
}
