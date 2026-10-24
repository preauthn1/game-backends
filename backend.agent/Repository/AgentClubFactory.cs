using Repository.Models.Interface;
using Repository.Models.Repository;

namespace Repository
{

    public class AgentClubFactory : IRepositoryFactory
    {
        public IRepository<TEntity> CreateRepository<TEntity>() where TEntity : class
        {
            return new AgentClubRepository<TEntity>();
        }

        public IDatabase GetDatabase(string name)
        {
            return new MSDatabase(name);
        }
    }
}
