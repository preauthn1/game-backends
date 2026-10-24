using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Event;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;

namespace game.Service.DAO
{
    public class PromotionEventItemRepository : IDapperRepository<PromotionEventItemEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[PromotionEventItem]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get
            {
                return "Id";
            }
        }

        public List<PromotionEventItemEntity> GetByEventIds(List<int> eventIds)
        {
            string sql = GenerateColumnInWhereQuery("EventId");            

            var query = QueryByDapper<PromotionEventItemEntity>(sql, new { EventId = eventIds });
            return query;
        }

    }


}
