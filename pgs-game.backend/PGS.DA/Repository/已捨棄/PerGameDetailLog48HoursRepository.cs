using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
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
    public class PerGameDetailLog48HoursRepository : FormatLongPrimaryKeyRepository<PerGameDetailLog48HoursEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PerGameDetailLog48Hours]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public override long Create (PerGameDetailLog48HoursEntity model, bool isTransactionScope = false)
        {
            throw new TerproException(ApiErrorCode.BusinessLogicDeny, "此資料表的資料只接受透過Trigger建立");
        }


        public List<PerGameDetailLog48HoursEntity> GetByFilter(QueryPerGameDetailLog48HoursRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PerGameDetailLog48HoursEntity>(sql, filter);
            return query;
        }


        public PagedList<PerGameDetailLog48HoursEntity> GetPagingByFilter(QueryPerGameDetailLog48HoursRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PerGameDetailLog48HoursEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPerGameDetailLog48HoursRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
