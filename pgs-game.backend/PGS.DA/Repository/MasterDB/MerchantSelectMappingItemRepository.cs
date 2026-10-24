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
    public class MerchantSelectMappingItemRepository : FormatMultiplePrimaryKeyRepository<MerchantSelectMappingItemEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantSelectMappingItem]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void DeleteAndInsertNew(int templateID, List<MerchantSelectMappingItemEntity> items)
        {
            this.DeleteByModel(new
            {
                TemplateID = templateID
            });

            this.CreateMany(items);
        }


        public override void CreateMany(IEnumerable<MerchantSelectMappingItemEntity> models)
        {
            foreach(var item in models)
            {
                item.RuleFor(x => x.TemplateID).Rule_NotZeroNotMinus();
            }

            base.CreateMany(models);
        }



        public List<int> GetAvailableMerchantIDsByTemplateID(int templateID)
        {
            var query = GetByFilter(new QueryMerchantSelectMappingItemRequestViewModel
            {
                TemplateIDs = templateID.ToListCollection(),
                Types = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger().ToListCollection()
            });

            return query.Select(x => x.MerchantID).ToList();
        }


        public List<MerchantSelectMappingItemEntity> GetByFilter(QueryMerchantSelectMappingItemRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantSelectMappingItemEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantSelectMappingItemEntity> GetPagingByFilter(QueryMerchantSelectMappingItemRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantSelectMappingItemEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantSelectMappingItemRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.TemplateIDs.IsNotEmptyNull())
                sql += " AND TemplateID in @TemplateIDs ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type in @Types ";

            return sql;
        }

    }


}
