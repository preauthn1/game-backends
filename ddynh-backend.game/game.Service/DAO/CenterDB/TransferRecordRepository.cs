using Dapper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using System.Data.SqlClient;
using game.Entity.Money;

namespace game.Service.DAO
{

    public class TransferRecordRepository : IDapperRepository<TransferRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[TransferRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ID"; }
        }
        

        public void UpdateTransferStatus(string targetOrderNo , ThirdPartyTransferRecordStatusEnum status , string debugInfo)
        {
            string sql = @" UPDATE[ThirdParty].[TransferRecord] SET 
                                Status = @status,
                                ThirdPartyDebugInfo = @debugInfo
                            WHERE OrderNo = @targetOrderNo ";

            var para = new
            {
                targetOrderNo = targetOrderNo,
                debugInfo = debugInfo,
                status = status.ToInteger()
            };

            using(var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(sql, para);
            }
        }



        public List<TransferRecordRawEntity> GetByFilter(QueryTransferRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<TransferRecordRawEntity>(sql, filter);
            return query;
        }

        public PagedList<TransferRecordRawEntity> GetPagingByFilter(QueryTransferRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<TransferRecordRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryTransferRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if (filter.GameMerchantIDs.IsNotEmptyNull())
                sql += " AND GameMerchantID in @GameMerchantIDs ";

            if (filter.PlatformIDs.IsNotEmptyNull())
                sql += " AND PlatformID in @PlatformIDs ";

            if (filter.OrderNo.IsNotEmptyNull())
                sql += " AND OrderNo = @OrderNo ";

            if(filter.BeginDate.HasValue)
                sql += " AND CreateTime >= @BeginDate ";

            if (filter.EndDate.HasValue)
                sql += " AND CreateTime <= @EndDate ";

            return sql;
        }

    }


}
