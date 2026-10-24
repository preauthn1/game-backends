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
    public class ContentBlockRepository : FormatIntegerPrimaryKeyRepository<ContentBlockEntity>
    {

        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }


        protected override string TableName
        {
            get { return "[dbo].[ContentBlock]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "ContentBlockID"; }
        }

        public List<ContentTemplateAndContentBlockModel> GetContentByTemplateIDs(List<int> contentTemplateIDs)
        {
            if (contentTemplateIDs.IsEmptyOrNull())
                return new List<ContentTemplateAndContentBlockModel>();

            string sql = "SELECT * FROM [dbo].[ContentTemplate] WHERE [ContentTemplateID] in @contentTemplateIDs";

            var query = QueryByDapper<ContentTemplateAndContentBlockModel>(sql, new { contentTemplateIDs = contentTemplateIDs });

            var queryContentBlock = GetByFilter(new QueryContentBlockRequestViewModel
            {
                ContentTemplateIDs = contentTemplateIDs
            });

            foreach(var item in query)
            {
                item.ContentBlocks = queryContentBlock.Where(z => z.OwnerGuid == item.ContentTemplateGuid).ToList();
            }

            return query;
        }


        public List<ContentBlockEntity> GetByFilter(QueryContentBlockRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<ContentBlockEntity>(sql, filter);
            return query;
        }


        public PagedList<ContentBlockEntity> GetPagingByFilter(QueryContentBlockRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<ContentBlockEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryContentBlockRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.OwnerGuid.HasValue)
                sql += " AND OwnerGuid = @OwnerGuid ";

            if (filter.OwnerGuidList.IsNotEmptyNull())
                sql += " AND OwnerGuid in @OwnerGuidList ";

            if (filter.ContentTemplateIDs.IsNotEmptyNull())
            {
                sql += @" AND OwnerGuid in 
                          ( 
                            SELECT [ContentTemplateGuid] FROM [dbo].[ContentTemplate] WITH (NOLOCK) 
                            WHERE [ContentTemplateID] in @ContentTemplateIDs 
                          ) ";
            }              

            return sql;
        }

    }


}
