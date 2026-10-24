using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Repository.Models.Interface
{
    public interface IRepository<TEntity> : IDisposable
        where TEntity : class
    {
        void Insert(TEntity instance);

        TEntity Get(string username);

        IQueryable<TEntity> GetAll();

        IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params DbParameter[] parameters);

        void Update(TEntity instance);

        void SaveChanges();
    }
}
