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
    public class AGGFreespinCampaignRepository : FormatIntegerPrimaryKeyRepository<AGGFreespinCampaignEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[AGGFreespinCampaign]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public void CreateOrUpdate(AGGFreespinCampaignEntity entity)
        {
            entity.RuleFor(x => x.FreespinID).Rule_NotEmptyText();

            var query = GetByFreespinID(entity.FreespinID);

            if (query == null)
            {
                Create(entity);
            }
            else
            {
                entity.StartTime = entity.StartTime;
                entity.EndTime = entity.EndTime;

                Update(entity);
            }
        }

        public AGGFreespinCampaignEntity GetByFreespinID(string freespinID)
        {
            var query = GetByFilter(new QueryAGGFreespinCampaignRequestViewModel
            {
                FreespinID = freespinID
            });

            return query.FirstOrDefault();
        }



        public List<AGGFreespinCampaignEntity> GetByFilter(QueryAGGFreespinCampaignRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AGGFreespinCampaignEntity>(sql, filter);
            return query;
        }


        public PagedList<AGGFreespinCampaignEntity> GetPagingByFilter(QueryAGGFreespinCampaignRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AGGFreespinCampaignEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAGGFreespinCampaignRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.FreespinIDList.IsNotEmptyNull())
                sql += " AND FreespinID in @FreespinIDList ";

            if (filter.FreespinID.IsNotEmptyNull())
                sql += " AND FreespinID = @FreespinID ";

            return sql;
        }

    }


}
