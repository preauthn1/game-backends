using System;


namespace FW.Core.EventBus.EntityEvents
{
    public class EntityInserted<TEntity>
    {
        public TEntity Entity
        {
            get;
            set;
        }

        public EntityInserted(TEntity entity)
        {
            Entity = entity;

            FW.Core.Helper.Check.NotEmpty<TEntity>(entity, "entity");
        }

    }
}
