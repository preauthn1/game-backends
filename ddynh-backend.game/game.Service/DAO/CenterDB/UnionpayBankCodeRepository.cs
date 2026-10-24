using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;

namespace game.Service.DAO
{
    public class UnionpayBankCodeRepository : IDapperRepository<UnionpayBankCodeEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Money].[UnionpayBankCode]"; }
        }

        public override string PrimaryKeyColumnName 
        {
            get { return "Id"; }
        }


        public UnionpayBankCodeEntity GetByCode(string code)
        {
            var query = GetByFilter(new QueryUnionpayBankCodeRequestViewModel
            {
                Codes = new List<string> { code }
            });

            return query.FirstOrDefault();
        }


        public List<UnionpayBankCodeEntity> GetByFilter(QueryUnionpayBankCodeRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UnionpayBankCodeEntity>(sql, filter);
            return query;
        }

        public PagedList<UnionpayBankCodeEntity> GetPagingByFilter(QueryUnionpayBankCodeRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = GetPageingEntitiesBySQLScript<UnionpayBankCodeEntity>(sql, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryUnionpayBankCodeRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.Codes.IsNotEmptyNull())
                sql += " AND Code in @Codes ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            return sql;
        }


    }


}
