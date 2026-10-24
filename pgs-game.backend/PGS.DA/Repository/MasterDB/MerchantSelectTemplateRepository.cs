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
    public class MerchantSelectTemplateRepository : FormatIntegerPrimaryKeyRepository<MerchantSelectTemplateEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[MerchantSelectTemplate]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "TemplateID"; }
        }


        public override int Create(MerchantSelectTemplateEntity model)
        {
            model.RuleFor(x => x.CategoryID).Rule_NotZeroNotMinus();
            var parseEnum = (DataCategoryEnum)model.CategoryID;

            if (!parseEnum.IsDefinedValue())
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);

            return base.Create(model);            
        }


        public List<MerchantSelectTemplateEntity> GetByFilter(QueryMerchantSelectTemplateRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<MerchantSelectTemplateEntity>(sql, filter);
            return query;
        }


        public PagedList<MerchantSelectTemplateEntity> GetPagingByFilter(QueryMerchantSelectTemplateRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<MerchantSelectTemplateEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryMerchantSelectTemplateRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.TemplateIDs.IsNotEmptyNull())
                sql += " AND TemplateID in @TemplateIDs ";

            if (filter.CategoryIDs.IsNotEmptyNull())
                sql += " AND CategoryID in @CategoryIDs ";

            if(filter.ValueType.HasValue)
                sql += " AND ValueType = @ValueType ";


            if(filter.AssignIncludeMerchantIDs.IsNotEmptyNull())
            {
                int type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger();
                sql += $@" AND TemplateID in 
                      (
                        SELECT TemplateID from [dbo].[MerchantSelectMappingItem] with (NOLOCK)
                        WHERE Type = {type} AND MerchantID in @AssignIncludeMerchantIDs 
                      )";
            }

            if (!filter.IsIncludeDeleteData)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            return sql;
        }

    }


}
