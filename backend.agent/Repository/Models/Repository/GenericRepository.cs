using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Repository.Models.Interface;

namespace Repository.Models.Repository
{
    public class GenericRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private DbContext _context
        {
            get;
            set;
        }

        public GenericRepository()
         : this(new AdminSysContext(ConfigurationManager.ConnectionStrings["adminConnStr"].ConnectionString))
        {
        }

        public GenericRepository(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._context = context;
        }

        public GenericRepository(ObjectContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._context = new DbContext(context, true);
        }

        public void Insert(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else
            {
                this._context.Set<TEntity>().Add(instance);
                this.SaveChanges();
            }
        }

        public void Delete(TEntity instance)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TEntity Get(string username)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> GetAll()
        {
            return this._context.Set<TEntity>().AsQueryable();
        }

        public void SaveChanges()
        {
            this._context.SaveChanges();
        }

        public void Update(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else
            {
                this._context.Entry(instance).State = EntityState.Modified;
                this.SaveChanges();
            }
        }

        public IEnumerable<TObject> ExecuteSqlQuery<TObject>(string sql, params DbParameter[] parameters)
        {
            return _context.Database.SqlQuery<TObject>(sql, parameters);
        }
    }
}
