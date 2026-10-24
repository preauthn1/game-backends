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
    public class MerchantHierarchyChildMappingRepository : FormatMultiplePrimaryKeyRepository<MerchantHierarchyChildMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantHierarchyChildMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MerchantID"; }
        }

        public void DeleteByRootMerchantID(int rootMerchantID)
        {
            string sql = $@" DELETE FROM {TableName} Where MerchantID in
                            (
                                SELECT MerchantID from [dbo].[MerchantHierarchy]
                                WHERE RootMerchantID = @rootMerchantID
                            )";

            ExecuteSqlByDapper(sql, new { rootMerchantID = rootMerchantID });
        }

        public int GetCountByFilter(QueryMerchantHierarchyChildMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public List<MerchantHierarchyChildMappingEntity> GetByFilter(QueryMerchantHierarchyChildMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantHierarchyChildMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantHierarchyChildMappingEntity> GetPagingByFilter(QueryMerchantHierarchyChildMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantHierarchyChildMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantHierarchyChildMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.MerchantIDs.IsNotEmptyNull())
                sql += " AND MerchantID in @MerchantIDs ";

            return sql;
        }

    }


}
