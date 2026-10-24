using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Event;
using game.Infrastructure;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class EventSettingBonusRepository : IDapperRepository<EvnetSettingBonusEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[EvnetSettingBonus]"; }
        }

        public List<EvnetSettingBonusEntity> GetBySettingID(int settingID)
        {
            var sql = GenerateColumnEqualWhereQuery("EventSettingID");
            var query = QueryByDapper<EvnetSettingBonusEntity>(sql, new { EventSettingID = settingID });
            return query;
        }

    }




}
