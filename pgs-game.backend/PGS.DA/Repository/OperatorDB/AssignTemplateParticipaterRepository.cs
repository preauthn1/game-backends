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
    public class AssignTemplateParticipaterRepository : FormatMultiplePrimaryKeyRepository<AssignTemplateParticipaterEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.OperationDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[AssignTemplateParticipater]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "AssignTemplateID"; }
        }

        public List<AssignTemplateParticipaterEntity> GetByFilter(QueryAssignTemplateParticipaterRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<AssignTemplateParticipaterEntity>(sql, filter);
            return query;
        }


        public PagedList<AssignTemplateParticipaterEntity> GetPagingByFilter(QueryAssignTemplateParticipaterRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<AssignTemplateParticipaterEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryAssignTemplateParticipaterRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.AssignTemplateID.HasValue)
                sql += " AND AssignTemplateID = @AssignTemplateID ";

            if (filter.AssignTemplateIDs.IsNotEmptyNull())
                sql += " AND AssignTemplateID in @AssignTemplateIDs ";

            if (filter.AssignType.HasValue)
                sql += " AND AssignType = @AssignType ";

            return sql;
        }

    }


}
