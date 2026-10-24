using FW.Core.Helper;
using System;


namespace FW.Core.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        TEntity RegisterAdded<TEntity>(TEntity entity)
            where TEntity : BaseEntity;

        TEntity RegisterChangeded<TEntity>(TEntity entity)
            where TEntity : BaseEntity;

        TEntity RegisterDeleted<TEntity>(TEntity entity)
            where TEntity : BaseEntity;

        void SaveChanges();
    }
}
