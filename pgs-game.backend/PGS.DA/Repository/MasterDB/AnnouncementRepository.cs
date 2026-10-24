using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class AnnouncementRepository : FormatIntegerPrimaryKeyRepository<AnnouncementEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[Announcement]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "AnnouncementID"; }
        }

        public List<AnnouncementEntity> GetByFilter(QueryAnnouncementRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<AnnouncementEntity>(sql, filter);
            return query;
        }


        public PagedList<AnnouncementEntity> GetPagingByFilter(QueryAnnouncementRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<AnnouncementEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAnnouncementRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()}";

            if (filter.AssignMerchantIDs.IsNotEmptyNull())
            {
                int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();
                sql += $@" AND MerchantSelectTemplateID in ( 
                            SELECT TemplateID FROM [dbo].[MerchantSelectMappingItem] WITH (NOLOCK) 
                            WHERE Type = {type} AND MerchantID in @AssignMerchantIDs 
                          ) ";
            }
           
            if (filter.NotReadMessageByPlayerID.HasValue)
                sql += " AND AnnouncementID not in ( SELECT AnnouncementID FROM [dbo].[AnnouncementRead] WITH (NOLOCK) WHERE PlayerID = @NotReadMessageByPlayerID) ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            if (filter.IsNotExpired)
                sql += " AND BeginTime <= GETUTCDATE() AND  GETUTCDATE() < EndTime";

            return sql;
        }

    }


}
