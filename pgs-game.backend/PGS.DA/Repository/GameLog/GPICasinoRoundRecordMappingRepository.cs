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
    public class GPICasinoRoundRecordMappingRepository : FormatMultiplePrimaryKeyRepository<GPICasinoRoundRecordMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[GPICasinoRoundRecordMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }


        public List<GPICasinoRoundRecordMappingEntity> GetByFilter(QueryGPIRoundRecordMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GPICasinoRoundRecordMappingEntity>(sql, filter);
            return query;
        }

        public PagedList<GPICasinoRoundRecordMappingEntity> GetPagingByFilter(QueryGPIRoundRecordMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GPICasinoRoundRecordMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGPIRoundRecordMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.RoundID.IsNotEmptyTextNotNull())
                sql += " AND RoundID = @RoundID ";

            return sql;
        }

    }


}
