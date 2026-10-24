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
    public class ContentTemplateRepository : FormatIntegerPrimaryKeyRepository<ContentTemplateEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[ContentTemplate]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "ContentTemplateID"; }
        }

        public List<ContentTemplateEntity> GetByFilter(QueryContentTemplateRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ContentTemplateEntity>(sql, filter);
            return query;
        }


        public PagedList<ContentTemplateEntity> GetPagingByFilter(QueryContentTemplateRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<ContentTemplateEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryContentTemplateRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
