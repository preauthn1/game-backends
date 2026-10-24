using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.Event;
using game.Infrastructure;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class EventSettingRuleItemRepository : IDapperRepository<EventSettingRuleItemEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[EventSettingRuleItem]"; }
        }
              

        public void UpdateEventRules(int settingID, IEnumerable<EventSettingRuleItemEntity> rules)
        {
            if (rules.IsEmptyOrNull())
                throw new System.Exception("invalid");

            //先刪除再更新
            string deleteSQL = $"DELETE FROM {TableName} WHERE EventSettingID = @settingID";
            string insertItemTableSQL = SQLHelper.GetInsertCommand<EventSettingRuleItemEntity>(TableName);

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { settingID = settingID });
                conn.Execute(insertItemTableSQL, rules);
                return;
            }
        }


        public List<EventSettingRuleItemEntity> GetBySettingID(int settingID)
        {
            var sql = GenerateColumnEqualWhereQuery("EventSettingID");
            var query = QueryByDapper<EventSettingRuleItemEntity>(sql, new { EventSettingID = settingID });
            return query;
        }

    }




}
