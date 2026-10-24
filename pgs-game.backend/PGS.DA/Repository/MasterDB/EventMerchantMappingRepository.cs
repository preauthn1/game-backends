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
    public class EventMerchantMappingRepository_V1 : FormatIntegerPrimaryKeyRepository<EventMerchantMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventMerchantMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void DeleteAndInsertNew(int eventID , List<int> merchantIDs) 
        {
            DeleteByModel(new { EventID = eventID });

            if (merchantIDs.IsEmptyOrNull())
                return;

            var insertModels = merchantIDs.Select(z => new EventMerchantMappingEntity
                {
                    EventID = eventID,
                    MerchantID = z
                })
                .ToList();

            CreateMany(insertModels);
            return;
        }


        public List<EventMerchantMappingEntity> GetByFilter(QueryEventMerchantMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EventMerchantMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<EventMerchantMappingEntity> GetPagingByFilter(QueryEventMerchantMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EventMerchantMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventMerchantMappingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.EventIDs.IsNotEmptyNull())
                sql += " AND EventID in @EventIDs ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            return sql;
        }

    }


}
