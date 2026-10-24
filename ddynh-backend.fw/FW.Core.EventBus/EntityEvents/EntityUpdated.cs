using System;
using System.Collections.Generic;


namespace FW.Core.EventBus.EntityEvents
{
    public class EntityUpdated<TEntity>
    {
        public TEntity Entity
        {
            get;
            set;
        }

        public List<string> Properties
        {
            get;
            set;
        }

        public EntityUpdated(TEntity entity, params string[] props)
        {
            Entity = entity;
            if (props != null && props.Length > 0)
            {
                Properties = new List<string>(props);
            }
            else
            {
                Properties = new List<string>();
            }
        }
    }
}
