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
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.DA.Repository
{
    public class EventPointChangeLogRepository : FormatLongPrimaryKeyRepository<EventPointChangeLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EventPointChangeLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void CreateByStoreProcedure(EventPointChangeLogEntity entity)
        {
            var spName = "PrWeb_WriteEventPointChangeLog_ForMasterDB";
            var para = new
            {
                eventID = entity.EventID,
                playerID = entity.PlayerID,
                type = entity.Type,
                amount = entity.Amount,
                memo = entity.Memo,
                referenceID = entity.ReferenceID,
                referenceText = entity.ReferenceIDText,
            };

            var dbConnOfMasterDB = GetConnectionStringByDbName(DBConnType.MasterDB);
            using (var conn = new SqlConnection(dbConnOfMasterDB))
            {
                var result = conn.Execute(spName, para, commandType: CommandType.StoredProcedure);
            }
        }


        public List<EventPointChangeLogEntity> GetByFilter(QueryEventPointChangeLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<EventPointChangeLogEntity>(sql, filter);
            return query;
        }


        public PagedList<EventPointChangeLogEntity> GetPagingByFilter(QueryEventPointChangeLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<EventPointChangeLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEventPointChangeLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
