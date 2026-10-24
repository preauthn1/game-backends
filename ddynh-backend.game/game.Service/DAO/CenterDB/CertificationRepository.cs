using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.ViewModels.Query;

namespace game.Service.DAO
{

    public class CertificationRepository : IDapperRepository<CertificationRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Game].[Certification]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "UserID"; }
        }

        public List<CertificationRawEntity> GetByFilter(QueryCertificationRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<CertificationRawEntity>(sql, filter);
            return query;
        }

        public PagedList<CertificationRawEntity> GetPagingByFilter(QueryCertificationRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<CertificationRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryCertificationRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            if(!string.IsNullOrWhiteSpace(filter.CerCode))
                sql += " AND CerCode = @CerCode ";

            return sql;
        }

    }


}
