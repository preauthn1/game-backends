using System;

namespace FW.Core.Repository.EF
{
    public class EFDatabaseProvider : IDatabaseProvider
    {

        public IDatabase CreateDatabase(DatabaseSetting setting)
        {
            return new EFDatabase(new EFDatabaseContext(setting));
        }
    }
}
