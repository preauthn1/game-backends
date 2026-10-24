using System;


namespace FW.Core.EventBus.EntityEvents
{
    public class EntityDeleted<TEntity>
    {
        public TEntity Entity
        {
            get;
            set;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="entity"></param>
        public EntityDeleted(TEntity entity)
        {
            Entity = entity;

            FW.Core.Helper.Check.NotEmpty<TEntity>(entity, "entity");
        }

    }
}
