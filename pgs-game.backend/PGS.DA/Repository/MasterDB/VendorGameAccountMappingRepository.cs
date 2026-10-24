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
    public class VendorGameAccountMappingRepository : FormatMultiplePrimaryKeyRepository<VendorGameAccountMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[VendorGameAccountMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "VendorConfigID"; }
        }

        public List<VendorGameAccountMappingEntity> GetByFilter(QueryVendorGameAccountMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<VendorGameAccountMappingEntity>(sql, filter);
            return query;
        }


        public VendorGameAccountMappingEntity GetByUniqueKey(int playerID , int vendorConfigID , string account = "")
        {
            var query = GetByFilter(new QueryVendorGameAccountMappingRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                VendorConfigIDs = vendorConfigID.ToListCollection(),
                Account = account
            });

            return query.FirstOrDefault();
        }



        public PagedList<VendorGameAccountMappingEntity> GetPagingByFilter(QueryVendorGameAccountMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<VendorGameAccountMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryVendorGameAccountMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.VendorConfigIDs.IsNotEmptyNull())
                sql += " AND VendorConfigID in @VendorConfigIDs ";

            if (filter.Account.IsNotEmptyTextNotNull())
                sql += " AND Account = @Account ";

            return sql;
        }

    }


}
