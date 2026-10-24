using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models.Attributes;
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
    public class OrganizationCustomizedSettingRepository : FormatIntegerPrimaryKeyRepository<OrganizationCustomizedSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[OrganizationCustomizedSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public void CreateOrUpdate(OrganizationCustomizedSettingEntity model)
        {
            model.RuleFor(x => x.OrganizationID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            var query = GetByFilter(new QueryOrganizationCustomizedSettingRequestViewModel
            {
                OrganizationIDs = model.OrganizationID.ToListCollection(),
                SettingKeys = model.SettingKey.ToListCollection()
            });

            if (query.IsEmptyOrNull())
            {
                Create(model);
            }
            else
            {
                var existData = query.FirstOrDefault();

                existData.SettingValue = model.SettingValue;
                Update(existData);
            }
        }


        public SystemConfigSettingParseModel GetParseModelByEnum(int orgID , OrganizationCustomizedSettingKey key)
        {
            var query = GetByFilter(new QueryOrganizationCustomizedSettingRequestViewModel
                {
                    OrganizationIDs = orgID.ToListCollection(),
                    SettingKeys = key.ToInteger().ToListCollection()
                })
                .FirstOrDefault();

            if (query == null)
            {
                return new SystemConfigSettingParseModel
                {
                    SettingKey = key.ToInteger(),
                    SettingValue = key.GetAttributeFromEnum<SettingDefaultValue>()?.Value
                };
            }

            return new SystemConfigSettingParseModel
            {
                SettingKey = query.SettingKey,
                SettingValue = query.SettingValue
            };
        }


        public List<OrganizationCustomizedSettingEntity> GetByFilter(QueryOrganizationCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<OrganizationCustomizedSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<OrganizationCustomizedSettingEntity> GetPagingByFilter(QueryOrganizationCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<OrganizationCustomizedSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryOrganizationCustomizedSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.SettingKeys.IsNotEmptyNull())
                sql += " AND SettingKey in @SettingKeys ";

            if (filter.OrganizationIDs.IsNotEmptyNull())
                sql += " AND OrganizationID in @OrganizationIDs ";

            return sql;
        }

    }


}
