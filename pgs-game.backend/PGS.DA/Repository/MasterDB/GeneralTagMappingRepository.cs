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
    public class GeneralTagMappingRepository : FormatIntegerPrimaryKeyRepository<GeneralTagMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[GeneralTagMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public List<GeneralTagMappingInfoQueryModel> GetEachMappingAndTagInfoByType(TagTypeEnum type)
        {
            string sql = @"SELECT m.TargetID , t.TagID , t.Name 
                           FROM [dbo].[GeneralTagMapping] as m
                           LEFT JOIN  [dbo].[GeneralTag] as t on t.TagID = m.TagID
                           WHERE t.Type = @myType";

            var param = new
            {
                myType = type.ToInteger()
            };

            var query = QueryByDapper<GeneralTagMappingInfoQueryModel>(sql, param);
            return query;
        }


        public List<EazyCounterModel> GetEachTagMappingCount(List<int> tagIDs = null)
        {
            string extraWhere = tagIDs.IsEmptyOrNull() ? "" : " WHERE TagID in @tagIDs ";

            string sql = $@"SELECT TagID as OwnerID, count(*) as TotalCount
                            FROM [dbo].[GeneralTagMapping] WITH (NOLOCK) 
                            {extraWhere}
                            GROUP BY TagID";

            var param = new
            {
                tagIDs = tagIDs
            };

            var query = QueryByDapper<EazyCounterModel>(sql, param);
            return query;
        }

        /// <summary>
        /// 特殊型Tag 請參考 GeneralTagEnum
        /// </summary>
        public List<GeneralTagMappingEntity> GetByTagID(int tagID)
        {
            var query = GetByFilter(new QueryGeneralTagMappingRequestViewModel
            {
                TagIDs = tagID.ToListCollection()
            });

            return query;
        }
     

        public List<GeneralTagMappingEntity> GetByFilter(QueryGeneralTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<GeneralTagMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<GeneralTagMappingEntity> GetPagingByFilter(QueryGeneralTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<GeneralTagMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryGeneralTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            if (filter.TargetIDs.IsNotEmptyNull())
                sql += " AND TargetID in @TargetIDs ";

            return sql;
        }

    }


}
