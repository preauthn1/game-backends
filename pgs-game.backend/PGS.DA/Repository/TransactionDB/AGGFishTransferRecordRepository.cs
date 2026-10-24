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
    public class AGGFishTransferRecordRepository : FormatStringPrimaryKeyRepository<AGGFishTransferRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AGGFishTransferRecord]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TransactionNo"; }
        }

        protected override string DefaultSortColumn 
        {
            get { return "TransactionNo"; }
        }

        public List<AGGFishTransferRecordEntity> GetByFilter(QueryAGGFishTransferRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<AGGFishTransferRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGFishTransferRecordEntity> GetPagingByFilter(QueryAGGFishTransferRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<AGGFishTransferRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGFishTransferRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.TransactionNo.IsNotEmptyTextNotNull())
                sql += " AND TransactionNo = @TransactionNo ";

            if (filter.AGGFishOrderNo.IsNotEmptyTextNotNull())
                sql += " AND AGGFishOrderNo = @AGGFishOrderNo ";

            //if (filter.PartitionTimeBegin.HasValue)
            //    sql += " AND PartitionTime >= @PartitionTimeBegin ";

            //if (filter.PartitionTimeEnd.HasValue)
            //    sql += " AND PartitionTime < @PartitionTimeEnd ";

            if (filter.MerchantID.HasValue)
                sql += " AND MerchantID = @MerchantID ";

            return sql;
        }

    }


}
