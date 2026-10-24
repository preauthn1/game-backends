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
using game.Infrastructure.Helper;
using game.Entity.Customer;

namespace game.Service.DAO
{

    public class AttrRepository : IDapperRepository<AttrRawEntity>
    {
        public override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBNameConfig.DBName_Notify); }
        }

        public override string TableName
        {
            get { return "[Notify].[Attr]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get { return "Id"; }
        }


        public void SP_Add(int userID , NotifyAttrTypeEnum type , long param)
        {
            var para = new
            {
                intUserID = userID,
                tnyTypeID = type.ToInteger(),
                lngParam = param, //如果是金币变化通知，要帶金幣數
                tnyStatus = 0,
                dtStatTime = DateTimeHelper.GetCurrentTime(),
            };

            ExecuteSPByDapper("[dbo].[PrNs_Attr_Add]", para);
        }


        public List<AttrRawEntity> GetByFilter(QueryAttrRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<AttrRawEntity>(sql, filter);
            return query;
        }

        public PagedList<AttrRawEntity> GetPagingByFilter(QueryAttrRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPageingEntitiesBySQLScript<AttrRawEntity>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryAttrRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} 
                           WHERE 1=1 ";

            if (filter.IDs.IsNotEmptyNull())
                sql += " AND ID in @IDs ";

            if (filter.TypeIDs.IsNotEmptyNull())
                sql += " AND TypeID in @TypeIDs ";

            if (filter.UserIDs.IsNotEmptyNull())
                sql += " AND UserID in @UserIDs ";

            return sql;
        }

    }


}
