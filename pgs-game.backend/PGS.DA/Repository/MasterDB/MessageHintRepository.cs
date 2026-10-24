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
    public class MessageHintRepository : FormatLongPrimaryKeyRepository<MessageHintEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MessageHint]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "MessageHintID"; }
        }

        public void SetHintToRead(int playerID , int type)
        {
            var queryExist = GetByFilter(new QueryMessageHintRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                Types = type.ToListCollection()
            } , " MessageHintID ");

            if(queryExist.IsEmptyOrNull())
            {
                return;
            }

            string sql = @$" UPDATE {TableName} SET IsRead = 1 
                             WHERE MessageHintID in @MessageHintIDs ";

            var param = new 
            {
                MessageHintIDs = queryExist.Select(x=> x.MessageHintID).ToList()
            };

            ExecuteSqlByDapper(sql, param);
        }


        public List<MessageHintEntity> GetByFilter(QueryMessageHintRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MessageHintEntity>(sql, filter);
            return query;
        }


        public PagedList<MessageHintEntity> GetPagingByFilter(QueryMessageHintRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MessageHintEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMessageHintRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            if (filter.IsRead.HasValue)
                sql += " AND IsRead = @IsRead ";

            return sql;
        }

    }


}
