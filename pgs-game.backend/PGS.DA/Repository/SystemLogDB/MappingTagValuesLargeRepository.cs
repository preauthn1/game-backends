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
    public class MappingTagValuesLargeRepository : FormatMultiplePrimaryKeyRepository<MappingTagValuesEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MappingTagValuesLarge]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UniqueID"; }
        }

        protected override bool IsConvertToDbValueType
        {
            get
            {
                return true;
            }
        }

        public List<MappingTagValuesEntity> GetByFilter(QueryMappingTagValuesRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MappingTagValuesEntity>(sql, filter , filter.DbQueryExtraConfig);
            return query;
        }


        public PagedList<MappingTagValuesEntity> GetPagingByFilter(QueryMappingTagValuesRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MappingTagValuesEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMappingTagValuesRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PartitionTimeMatch.HasValue)
                sql += " AND PartitionTime = @PartitionTimeMatch ";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                sql += " AND UniqueID = @UniqueID ";

            return sql;
        }

    }


}
