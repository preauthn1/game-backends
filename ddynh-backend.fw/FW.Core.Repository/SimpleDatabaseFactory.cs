using System;
using System.Linq;

namespace FW.Core.Repository.EF
{
    public class SimpleDatabaseFactory : IDatabaseFactory
    {
        private DatabaseSettingCollection _settings;
        private IDatabaseProvider _provider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="provider"></param>
        public SimpleDatabaseFactory(DatabaseSettingCollection settings, IDatabaseProvider provider)
        {
            FW.Core.Helper.Check.NotEmpty<DatabaseSettingCollection>(settings, "settings");
            FW.Core.Helper.Check.NotEmpty<IDatabaseProvider>(provider, "provider");

            _settings = settings;
            _provider = provider;
        }

        private DatabaseSetting getDatabase(string dbName)
        {
            return _settings.Where(s => (string.IsNullOrWhiteSpace(dbName) && String.IsNullOrWhiteSpace(s.DatabaseName))
                           || String.Compare(dbName, s.DatabaseName, true) == 0)
                           .FirstOrDefault();
        }



        public IDatabase GetDatabase(string dbName = "", DatabaseType type = DatabaseType.All)
        {
            var setting = getDatabase(dbName);

            return _provider.CreateDatabase(setting);
        }
    }
}
