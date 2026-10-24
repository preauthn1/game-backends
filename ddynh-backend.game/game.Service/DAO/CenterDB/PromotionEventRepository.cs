using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Event;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity.Money;

namespace game.Service.DAO
{
    public class PromotionEventRepository : IDapperRepository<PromotionEventEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[PromotionEvent]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get
            {
                return "EventID";
            }
        }

        public PagedList<PromotionEventEntity> GetPagingByFilter(QueryPromotionEventRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                            WHERE 1=1 ";

            string where = string.Empty;

            if (!string.IsNullOrWhiteSpace(filter.PlatformIDsText))
                filter.PlatformIDs = filter.PlatformIDsText.Split(',').Select(x => Int32.Parse(x)).ToList();
            
            if(!string.IsNullOrWhiteSpace(filter.Keyword))
                where += " AND ( Title LIKE CONCAT('%',@Keyword,'%') OR Memo LIKE CONCAT('%',@Keyword,'%') ) ";

            if (!filter.IsIncludePermanentInvisible)
                where += $" AND Status != {(int)NormalActiveStatusEnum.PermanentInvisible} ";

            if (filter.Status.HasValue)
                where += " AND Status = @Status ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                where += " AND PlatformID in @PlatformIDs  ";


            sql = sql + where;
            var query = GetPageingEntitiesBySQLScript<PromotionEventEntity>(sql , filter, filter);
            return query;

        }
        

        public void CopyEventBySP(List<int> templateEventIDs , List<int> targetPlatformIDs , string updateBy)
        {
            string spName = "PrWeb_CopyEventToTargetPlatforms";

            var para = new
            {
                updateBy = updateBy,
                templateEventIDs = CustomSqlHelper.ConvertListIntToDatableValueType(templateEventIDs),
                targetPlatformIDs = CustomSqlHelper.ConvertListIntToDatableValueType(targetPlatformIDs),
            };
            
            ExecuteSPByDapper(spName, para);
        }




    }


}
