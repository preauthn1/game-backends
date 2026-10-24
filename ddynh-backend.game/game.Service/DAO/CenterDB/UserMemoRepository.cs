using Dapper;
using System;
using System.Collections.Generic;
using game.Infrastructure.Extensions;
using System.Linq;
using FW.Core.Repository;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.Game;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;

namespace game.Service.DAO
{

    public class UserMemoRepository : IDapperRepository<UserMemoRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Users].[UserMemo]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }

        public List<UserMemoRawEntity> GetByUserId(int userID)
        {
            var filter = new QueryUserMemoRequestViewModel
            {
                UserIDs = userID.ToListCollection()
            };

            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserMemoRawEntity>(sql, filter);
            return query;
        }


        public List<UserMemoRawEntity> GetByFilter(QueryUserMemoRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<UserMemoRawEntity>(sql, filter);
            return query;
        }


        public PagedList<UserMemoRawEntity> GetPagingByFilter(QueryUserMemoRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<UserMemoRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryUserMemoRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            return sql;
        }

    }


}
