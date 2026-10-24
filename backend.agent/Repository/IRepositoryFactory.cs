using Repository.Models.Interface;

namespace Repository
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> CreateRepository<TEntity>()
            where TEntity : class;

        IDatabase GetDatabase(string name);
    }
}
