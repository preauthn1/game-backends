using System;
using System.Collections.Generic;
using System.Linq;


namespace FW.Core.Repository
{
    public class ReflectionDatabaseSelector : IDatabaseSelector
    {
  

        public string GetDatabase<TEntity>()
        {
            return GetDatabase(typeof(TEntity));
        }


        public string GetDatabase(Type entityType)
        {
            string dbName = "";
            var attr = entityType.GetCustomAttributes(true).OfType<DatabaseAttribute>().FirstOrDefault();
            if (attr != null)
            {
                dbName = (attr as DatabaseAttribute).DatabaseName;
            }

            return dbName;
        }
    }
}
