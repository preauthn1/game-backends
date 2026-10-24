using FW.Core.Helper;
using System;
using System.Linq;


namespace FW.Core.Repository
{
    public interface IRepository<TEntity> 
        where TEntity: BaseEntity
    {
        TEntity GetById(int id);

        IQueryable<TEntity> Entities { get; }


        TEntity Insert(TEntity entity);


        TEntity Update(TEntity entity);

        TEntity Delete(TEntity entity);
    }

}
