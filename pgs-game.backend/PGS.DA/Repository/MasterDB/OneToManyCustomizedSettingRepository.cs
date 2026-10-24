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
    public class OneToManyCustomizedSettingRepository : FormatIntegerPrimaryKeyRepository<OneToManyCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[OneToManyCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "OneToManyCustomizedSettingID"; }
        }

        public OneToManyCustomizedSettingEntity GetUniqueByType(OneToManyCustomizedSettingTypeEnum type)
        {
            var query = GetByFilter(new QueryOneToManyCustomizedSettingRequestViewModel
            {
                Type = type.ToInteger()
            });

            return query.FirstOrDefault();
        }


        public List<OneToManyCustomizedSettingEntity> GetByFilter(QueryOneToManyCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<OneToManyCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<OneToManyCustomizedSettingEntity> GetPagingByFilter(QueryOneToManyCustomizedSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<OneToManyCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryOneToManyCustomizedSettingRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (!filter.IsIncludeDelete)
                sql += $" AND Status != {NormalActiveStatus.PermanentDelete.ToInteger()} ";

            if (filter.Keyword.IsNotEmptyTextNotNull())
                sql += $" AND ( {GenerateLikeQuery("DisplayName", "@Keyword")} OR {GenerateLikeQuery("Memo", "@Keyword")} OR {GenerateLikeQuery("OneToManyCustomizedSettingID", "@Keyword")}  ) ";

            if(filter.Types.IsNotEmptyNull())
                sql += $" AND Type in @Types ";

            if (filter.Type.HasValue)
                sql += $" AND Type = @Type ";

            if (filter.OwnerID.HasValue)
                sql += $" AND OwnerID = @OwnerID ";

            return sql;
        }

    }


}
