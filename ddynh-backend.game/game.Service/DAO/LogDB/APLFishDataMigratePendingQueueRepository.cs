using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;

namespace game.Service.DAO
{
    /// <summary>
    /// 這個Repository又繼承了另一個Repository
    /// </summary>
    public class APLFishDataMigratePendingQueueRepository : BaseDataMigratePendingQueueRepository
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[APLFishDataMigratePendingQueue]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "BetRecordNo"; }
        }


    }


}
