using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Helper;
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
    public class MappingTagValuesRepository : FormatMultiplePrimaryKeyRepository<MappingTagValuesEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.SystemLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MappingTagValues]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UniqueID"; }
        }

        /// <summary>
        /// 大表跟小表 同時寫入(預期可以節省一點點connect浪費?)
        /// </summary>
        public void CreateMany_BundleInsert(List<MappingTagValuesEntity> tagValues, List<MappingTagValuesEntity> tagValuesLarge)
        {
            //如果都為空
            if (tagValues.IsEmptyOrNull() && tagValuesLarge.IsEmptyOrNull())
                return;

            //原本預期的寫法效能更糟。所以改成標準bulkInsert了
            var scopeLevel = System.Transactions.IsolationLevel.ReadUncommitted;

            BulkInsert(TableName, tagValues, scopeLevel);
            BulkInsert("MappingTagValuesLarge", tagValuesLarge, scopeLevel);

            return;

            //string insertSQL = SQLHelper.GetInsertCommand<MappingTagValuesEntity>(TableName);
            //string insertSQL2 = SQLHelper.GetInsertCommand<MappingTagValuesEntity>("MappingTagValuesLarge");

            //using (var conn = InstanceSQLConn())
            //{
            //    conn.Open();
            //    using (var trans = conn.BeginTransaction())
            //    {
            //        if (tagValues.IsNotEmptyNull())
            //        {
            //            conn.Execute(insertSQL, tagValues, trans);
            //        }
            //        if (tagValuesLarge.IsNotEmptyNull())
            //        {
            //            conn.Execute(insertSQL2, tagValuesLarge, trans);
            //        }
            //        trans.Commit();
            //        return;
            //    }
            //}
        }


        public List<MappingTagValuesEntity> GetByFilter(QueryMappingTagValuesRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MappingTagValuesEntity>(sql, filter);
            return query;
        }


        public PagedList<MappingTagValuesEntity> GetPagingByFilter(QueryMappingTagValuesRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MappingTagValuesEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMappingTagValuesRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX in @IDs ";

            return sql;
        }

    }


}
