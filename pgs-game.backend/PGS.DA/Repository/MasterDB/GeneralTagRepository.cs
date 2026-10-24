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
    public class GeneralTagRepository : FormatIntegerPrimaryKeyRepository<GeneralTagEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GeneralTag]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TagID"; }
        }

        public List<GeneralTagEntity> GetByFilter(QueryGeneralTagRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GeneralTagEntity>(sql, filter);
            return query;
        }


        public PagedList<GeneralTagEntity> GetPagingByFilter(QueryGeneralTagRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GeneralTagEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGeneralTagRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.Keyword.IsNotEmptyTextNotNull())
            {
                sql += $" AND ( {GenerateLikeQuery("Name", "@Keyword")}  OR  {GenerateLikeQuery("Memo", "@Keyword")} )";
            }

            if (filter.Types.IsNotEmptyNull())
            {
                sql += $" AND Type in @Types ";
            }

            return sql;
        }

    }


}
