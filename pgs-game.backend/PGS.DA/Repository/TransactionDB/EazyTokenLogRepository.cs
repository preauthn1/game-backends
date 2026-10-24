using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.DA.Repository
{
    public class EazyTokenLogRepository : FormatStringPrimaryKeyRepository<EazyTokenLogEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.TransactionDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EazyTokenLog]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Token"; }
        }


        public override void Create(EazyTokenLogEntity insert)
        {
            //限制要存pgs獨立單號的格式
            if (!RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(insert.Token).IsPgsOrderNo)
                throw new TerproException(ApiErrorCode.InvalidParameter);

            base.Create(insert);
        }


        public List<EazyTokenLogEntity> GetByFilter(QueryEazyTokenLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EazyTokenLogEntity>(sql, filter);
            return query;
        }


        public PagedList<EazyTokenLogEntity> GetPagingByFilter(QueryEazyTokenLogRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EazyTokenLogEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEazyTokenLogRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.Token.IsNotEmptyTextNotNull())
                sql += " AND Token = [REDACTED] ";

            return sql;
        }

    }


}
