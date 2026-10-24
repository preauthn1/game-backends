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
    public class PlayerLoginDataRepository : FormatIntegerPrimaryKeyRepository<PlayerLoginDataEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.StatReportDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerLoginData]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }


        public List<QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel> GetEachMerchantPlayerCount(DateTime begin , DateTime end , List<int> merchantIDs)
        {
            string selectSQL = @"SELECT 
	                              summary.MerchantID as MerchantID , 		
	                              count(distinct PlayerID) as TotalPlayers
                               FROM [PGS_StatReport].[dbo].[PlayerLoginData] as summary with (NOLOCK)
                               WHERE StatTime >= @begin AND StatTime < @end AND MerchantID in @merchantIDs
                               group by summary.MerchantID";
            var param = new
            {
                begin = begin,
                end = end,
                merchantIDs = merchantIDs
            };

            var query = QueryByDapper<QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel>(selectSQL, param);
            return query;
        }


        public List<QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel> GetEachMerchantOnlinePlayersByHierarchyChildMapping(QueryDirectAgentSummaryReportByHierarchyChildRequestModel filter)
        {
            string extraWhere = "  mapping.MerchantID = @TargetMerchantID ";

            //請參考該欄位的註解說明
            if (filter.IsQueryDirectAgentDetailOnly)
            {
                extraWhere = @" mapping.MerchantID in 
		                            (	
			                            SELECT MerchantID from [PGS_Master].[dbo].[MerchantHierarchy] WITH (NOLOCK)
			                            where ParentMerchantID = @targetMerchantID
		                            )";
            }

            string sql = $@"SELECT 
                            join_summary.OwnerMerchantID as MerchantID,
                            sum(join_summary.TotalPlayers) as TotalPlayers

                            FROM 
                            (
	                            SELECT 

		                            mapping.MerchantID as OwnerMerchantID , 
		                            mapping.ChildMerchantID as ChildMerchantID , 
		                            ISNULL(join_data.TotalPlayers , 0) as TotalPlayers

	                            FROM [PGS_Master].[dbo].[MerchantHierarchyChildMapping] as mapping WITH (NOLOCK)
	                            left join
	                            (
		                            SELECT 
			                            summary.MerchantID as MerchantID , 		
			                            count(distinct PlayerID) as TotalPlayers
		                            FROM [PGS_StatReport].[dbo].[PlayerLoginData] as summary with (NOLOCK)
		                            where StatTime >= @begin AND StatTime < @end
		                            group by summary.MerchantID

	                            ) as join_data on mapping.ChildMerchantID = join_data.MerchantID                                

	                            where {extraWhere} 	
                            ) as join_summary
                            group by join_summary.OwnerMerchantID
                            order by sum(join_summary.TotalPlayers) desc";

            var query = QueryByDapper<QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel>(sql, filter);
            return query;
        }



        public List<PlayerLoginDataEntity> GetByFilter(QueryPlayerLoginDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<PlayerLoginDataEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerLoginDataEntity> GetPagingByFilter(QueryPlayerLoginDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<PlayerLoginDataEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryPlayerLoginDataRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
