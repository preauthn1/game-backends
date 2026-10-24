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
    public class EazyUniqueKeyValueRecordRepository : FormatMultiplePrimaryKeyRepository<EazyUniqueKeyValueRecordEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EazyUniqueKeyValueRecord]"; }
        }


        public void CreateIfNotExist(EazyUniqueKeyValueRecordEntity entity)
        {
            var query = GetByFilter(new QueryEazyUniqueKeyValueRecordRequestViewModel
            {
                Types = entity.Type.ToListCollection(),
                OwnerIDs = entity.OwnerID.ToListCollection(),
                SubCategory = entity.SubCategory
            });

            if(query.IsNotEmptyNull())
            {
                return;
            }

            CreateTryCatch(entity);
        }


        public List<EazyUniqueKeyValueRecordEntity> GetByFilter(QueryEazyUniqueKeyValueRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<EazyUniqueKeyValueRecordEntity>(sql, filter);
            return query;
        }


        public PagedList<EazyUniqueKeyValueRecordEntity> GetPagingByFilter(QueryEazyUniqueKeyValueRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<EazyUniqueKeyValueRecordEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEazyUniqueKeyValueRecordRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            if (filter.OwnerIDs.IsNotEmptyNull())
                sql += " AND OwnerID in @OwnerIDs ";

            if (filter.SubCategory.IsNotEmptyTextNotNull())
                sql += " AND SubCategory = @SubCategory ";

            return sql;
        }

    }


}
