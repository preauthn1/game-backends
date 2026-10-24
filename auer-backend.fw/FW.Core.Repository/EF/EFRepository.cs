using System;
using System.Data.Entity;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using FW.Core.Helper;

namespace FW.Core.Repository.EF
{
    public class EFRepository<TEntity> : IRepository<TEntity>
        where TEntity : BaseEntity
    {
        private EFUnitOfWork _unifOfWork;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="unitOfWork"></param>
        public EFRepository(EFUnitOfWork unitOfWork)
        {
            _unifOfWork = unitOfWork;
        }

        public TEntity GetById(int id)
        {
            return _unifOfWork.GetQueryable<TEntity>().Where(e => e.Id == id).FirstOrDefault();
        }

        public IQueryable<TEntity> Entities
        {
            get { return _unifOfWork.GetQueryable<TEntity>(); }
        }

        public TEntity Insert(TEntity entity)
        {
            return _unifOfWork.RegisterAdded<TEntity>(entity);
        }

        public TEntity Update(TEntity entity)
        {
            return _unifOfWork.RegisterChangeded<TEntity>(entity);
        }

        public TEntity Delete(TEntity entity)
        {
            return _unifOfWork.RegisterDeleted<TEntity>(entity);
        }

    }

}
