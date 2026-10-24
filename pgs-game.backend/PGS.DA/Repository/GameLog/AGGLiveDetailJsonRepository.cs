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
    public class AGGLiveDetailJsonRepository : FormatStringPrimaryKeyRepository<AGGLiveDetailJsonEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[AGGLiveDetailJson]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "[BetRecordNoPGS]"; }
        }

        public List<AGGLiveDetailJsonEntity> GetByRecordNoList(List<string> berRecordNoList, string selectColumns = "*")
        {
            if (berRecordNoList.IsEmptyOrNull())
                return new List<AGGLiveDetailJsonEntity>();

            //sql in 查詢會有2100筆的限制
            var splitSize2000 = berRecordNoList.SplitListByArraySize(2000);

            List<AGGLiveDetailJsonEntity> listResult = new List<AGGLiveDetailJsonEntity>();

            foreach (List<string> myBerRecordNoList in splitSize2000)
            {
                var myFilter = new QueryAGGLiveDetailJsonRequestViewModel
                {
                    BetRecordNoPGSList = myBerRecordNoList
                };

                string sql = GenerateWhereSQL(myFilter, selectColumns);

                var query = QueryByDapper<AGGLiveDetailJsonEntity>(sql, myFilter);

                listResult.AddRange(query);
            }

            return listResult;
        }


        public List<AGGLiveDetailJsonEntity> GetByFilter(QueryAGGLiveDetailJsonRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AGGLiveDetailJsonEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGLiveDetailJsonEntity> GetPagingByFilter(QueryAGGLiveDetailJsonRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AGGLiveDetailJsonEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGLiveDetailJsonRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGSList.IsNotEmptyNull())
                sql += " AND BetRecordNoPGS in @BetRecordNoPGSList ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            return sql;
        }

    }


}
