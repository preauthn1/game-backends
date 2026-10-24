using Repository.Models;
using Repository.Models.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> CreateRepository<TEntity>()
            where TEntity : class;

        Database Database();

        DbContext DbContext();
    }
}
