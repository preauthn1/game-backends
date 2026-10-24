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

    public class AgentClubRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private DbContext _context
        {
            get;
            set;
        }

        public AgentClubRepository()
         : this(new AgentClubContext(ConfigurationManager.ConnectionStrings["AgentClub"].ConnectionString))
        {
        }

        public AgentClubRepository(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._context = context;
        }

        public AgentClubRepository(ObjectContext context)
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
