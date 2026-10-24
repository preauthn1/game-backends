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

namespace game.Service.DAO
{

    public class ThirdPartyBetFlowAmountCalculateRecordRepository : IDapperRepository<ThirdPartyBetFlowAmountCalculateRecordRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_LogDB); }
        }

        public override string TableName
        {
            get { return "[ThirdParty].[ThirdPartyBetFlowAmountCalculateRecord]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "ReferenceBetRecordID"; }
        }

        public List<ThirdPartyBetFlowAmountCalculateRecordRawEntity> GetByFilter(QueryThirdPartyBetFlowAmountCalculateRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ThirdPartyBetFlowAmountCalculateRecordRawEntity>(sql, filter);
            return query;
        }

        public PagedList<ThirdPartyBetFlowAmountCalculateRecordRawEntity> GetPagingByFilter(QueryThirdPartyBetFlowAmountCalculateRecordRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<ThirdPartyBetFlowAmountCalculateRecordRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryThirdPartyBetFlowAmountCalculateRecordRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.ReferenceBetRecordIDs.IsNotEmptyNull())
                sql += " AND ReferenceBetRecordID in @ReferenceBetRecordIDs ";

            if (filter.GameMerchantIDs.IsNotEmptyNull())
                sql += " AND GameMerchantID in @GameMerchantIDs ";

            return sql;
        }

    }


}
