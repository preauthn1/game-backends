using Repository.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Models.Interface;
using Repository.Models;
using System.Data.Entity;

namespace Repository
{
    public class AgentClubFactory : IRepositoryFactory
    {
        private DbContext _context { get; set; }

        public AgentClubFactory()
        {
            this._context = new ClubEntities();
        }

        public IRepository<TEntity> CreateRepository<TEntity>() where TEntity : class
        {
            return new GenericRepository<TEntity>(this._context);
        }

        public Database Database()
        {            
            return this._context.Database;            
        }

        public DbContext DbContext()
        {
            return this._context;
        }
    }    
}
