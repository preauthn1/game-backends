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
    public class BriefApiRequestLogThirdPartyRepository : FormatIntegerPrimaryKeyRepository<BriefApiRequestLogThirdPartyEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BriefApiRequestLogThirdParty]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UniqueID"; }
        }


        public List<BriefApiRequestLogThirdPartyEntity> GetDetailInfoByUniqueID(string uniqueID)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
        }


        public List<BriefApiRequestLogThirdPartyEntity> GetByFilter(QueryExternalApiRequestLogRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BriefApiRequestLogThirdPartyEntity>(sql, filter);
            return query;
        }


        //public PagedList<BriefApiRequestLogThirdPartyEntity> GetPagingByFilter(QueryBriefApiRequestLogThirdPartyRequestViewModel filter, string selectColumns = "*")
        //{
        //    string sql = GenerateWhereSQL(filter, selectColumns);
        //    var query = GetPagingEntitiesBySQLScript<BriefApiRequestLogThirdPartyEntity>(sql, filter, filter);
        //    return query;
        //}


        private string GenerateWhereSQL(QueryExternalApiRequestLogRequestViewModel filter, string selectColumns = "*")
        {
            filter.RuleFor(x => x.CreateTimeBegin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.CreateTimeEnd).Rule_NotNullDateTime();

            string topSQL = filter.TopSize.HasValue ? $" top {filter.TopSize.Value} " : "";

            string sql = $@"SELECT {topSQL} {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1";

            if (filter.UniqueID.IsNotEmptyTextNotNull())
                sql += " AND UniqueID = @UniqueID ";

            if (filter.UniqueIDs.IsNotEmptyNull())
                sql += " AND UniqueID in @UniqueIDs ";

            if (filter.ReferOrderNo.IsNotEmptyNull())
                sql += " AND ReferOrderNo = @ReferOrderNo ";

            if (filter.CreateTimeBegin.HasValue)
                sql += $" AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += $" AND CreateTime < @CreateTimeEnd ";

            if (filter.RequestUrlKeyword.IsNotEmptyTextNotNull())
            {
                filter.RequestUrlKeyword = filter.RequestUrlKeyword.Trim();
                sql += $" AND {GenerateLikeQuery("RequestUrlShort", "@RequestUrlKeyword")} ";
            }

            if (filter.ResponseTimeLargeThan.HasValue)
            {
                sql += $" AND ResponseTime >= @ResponseTimeLargeThan ";
            }

            if (filter.Category.IsNotEmptyTextNotNull())
            {
                sql += $" AND Category = @Category ";
            }

            if (filter.SubCategory.IsNotEmptyTextNotNull())
            {
                sql += $" AND SubCategory = @SubCategory ";
            }

            if (filter.PlayerID.HasValue)
            {
                sql += $" AND PlayerID = @PlayerID ";
            }

            if (filter.MerchantID.HasValue)
            {
                sql += $" AND MerchantID = @MerchantID ";
            }

            if(!string.IsNullOrWhiteSpace(filter.OrderByColumn))
            {
                sql += $" order by {filter.OrderByColumn} desc";
            }

            return sql;
        }

    }


}
