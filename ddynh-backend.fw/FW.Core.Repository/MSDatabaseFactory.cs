using System;
using System.Collections.Generic;
using System.Linq;


namespace FW.Core.Repository
{
    public class MSDatabaseFactory : IDatabaseFactory
    {
        private DatabaseSettingCollection _settings;
        private IDatabaseProvider _Provider;
        private bool _transaction;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="provider"></param>
        /// <param name="transaction"></param>
        public MSDatabaseFactory(DatabaseSettingCollection settings, IDatabaseProvider provider, bool transaction = true)
        {
            FW.Core.Helper.Check.NotEmpty<DatabaseSettingCollection>(settings, "settings");
            FW.Core.Helper.Check.NotEmpty<IDatabaseProvider>(provider, "provider");

            _settings = settings;
            _Provider = provider;
            _transaction = transaction;
        }

        private IDatabase getDatabaseForRead(string dbName = "")
        {
            var setting = _settings.Where(s => ((string.IsNullOrWhiteSpace(dbName) && String.IsNullOrWhiteSpace(s.DatabaseName))
                           || String.Compare(dbName, s.DatabaseName, true) == 0)
                           && s.IsSlaveDatabase)
                          .OrderBy(s => Guid.NewGuid())
                          .FirstOrDefault();


            return _Provider.CreateDatabase(setting);
        }

        private IDatabase getDatabaseForWrite(string dbName = "")
        {
            var settings = _settings.Where(s => ((string.IsNullOrWhiteSpace(dbName) && String.IsNullOrWhiteSpace(s.DatabaseName))
                           || String.Compare(dbName, s.DatabaseName, true) == 0)
                           && s.IsMasterDatabase)
                           .ToList();

            IList<IDatabase> databases = new List<IDatabase>();
            settings.ForEach(s => {databases.Add(_Provider.CreateDatabase(s));});

            return new MSDatabase(databases, _transaction);
        }


        public IDatabase GetDatabase(string dbName = "", DatabaseType type = DatabaseType.All)
        {
            if (type == DatabaseType.All)
            {
                return getDatabaseForWrite(dbName); ;
            }

            if (type == DatabaseType.Master)
            {
                return getDatabaseForWrite(dbName);
            }
            else
            {
                return getDatabaseForRead(dbName);
            }

        }
    }
}
