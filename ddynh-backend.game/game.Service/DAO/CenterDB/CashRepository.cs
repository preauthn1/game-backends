using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;
using game.Entity;
using game.Entity.Notify;
using game.Entity.Enums;
using System.Data;
using System.Data.SqlClient;

namespace game.Service.DAO
{

    public class CashRepository : IDapperRepository<CashRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Money].[Cash]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }


        public int UpdateWalletBalance(UpdateUserBalanceParameterModel model)
        {
            var para = new
            {
                intUserID = model.UserID,
                sintSourceType = model.SourceType,
                lngUpdateAmount = model.Amount,
                lngTaxAmount = model.TaxAmount,
                chvIPAddress = model.IPAddress,
                lngCurrentAmount = model.CurrentAmount,
                chvMemo = model.Memo,
                intAreaID = model.AreaID,
                intPlatformID = model.PlatformID,
            };

            var _params = new DynamicParameters(para);
            _params.Add(name: "@RetVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
            
            using(var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(sql: "PrPsWeb_UpdateUserCash",param: _params,commandType: CommandType.StoredProcedure);
                int retrunValue = _params.Get<int>("@RetVal");
                return retrunValue;
            }


            //return _params.Get<int>("@RetVal");


            ////这边应该要判断错误马
            ////这边应该要判断错误马
            ////这边应该要判断错误马
            ////这边应该要判断错误马
            ////这边应该要判断错误马
            ////这边应该要判断错误马
            ////这边应该要判断错误马

            //ExecuteSPByDapper("PrPsWeb_UpdateUserCash", para);
        }


        public List<CashRawEntity> GetByFilter(QueryCashRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<CashRawEntity>(sql, filter);
            return query;
        }


        public PagedList<CashRawEntity> GetPagingByFilter(QueryCashRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<CashRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryCashRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            return sql;
        }

    }


}
