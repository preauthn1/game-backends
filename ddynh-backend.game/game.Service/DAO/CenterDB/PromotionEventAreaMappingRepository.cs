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
using Dapper;

namespace game.Service.DAO
{
    public class PromotionEventAreaMappingRepository : IDapperRepository<PromotionEventAreaMappingEntity>
    {
        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[PromotionEventAreaMapping]"; }
        }

        public override string PrimaryKeyColumnName
        {
            get
            {
                return "MappingID";
            }
        }


        public List<PromotionEventAreaMappingEntity> GetByEventID(int eventID)
        {
            string sql = GenerateColumnEqualWhereQuery("EventID");
            var query = QueryByDapper<PromotionEventAreaMappingEntity>(sql, new { EventID = eventID });
            return query;
        }

        public List<PromotionEventAreaCountModel> GetTotalAreaCount(List<int> eventIDs)
        {
            eventIDs = eventIDs.Distinct().ToList();

            string sql = @" SELECT EventID, count(*) as TotalAreaCount
                            FROM [Event].[PromotionEventAreaMapping] with(NOLOCK)
                            WHERE EventID in @EventIDs
                            GROUP BY EventID";

            using(var conn = new SqlConnection(ConnectionString))
            {
                var query = conn.Query<PromotionEventAreaCountModel>(sql, new { EventIDs = eventIDs }).ToList();
                return eventIDs
                        .Select(x=> new PromotionEventAreaCountModel
                        {
                            EventID = x,
                            TotalAreaCount = query.FirstOrDefault(z=> x == z.EventID)?.TotalAreaCount ?? 0
                        })
                        .ToList();
            }
        }


        public void DeleteAndInsertMultiple(int eventID , List<int> areaIDs)
        {
            string deleteSQL = @"DELETE FROM [Event].[PromotionEventAreaMapping] WHERE EventID = @EventID";

            var insertItems = areaIDs.Select(z => new PromotionEventAreaMappingEntity
            {
                AreaID = z,
                EventID = eventID
            })
            .ToList();

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(deleteSQL, new { EventID = eventID });

                if(insertItems.IsNotEmptyNull())
                {
                    CreateMany(insertItems);
                }

            }
        }

    }


}
