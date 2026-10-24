using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Helper;
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
using System.Threading.Tasks;

namespace target.DA.Repository
{
    public class BetRecordTagMappingRepository : FormatMultiplePrimaryKeyRepository<BetRecordTagMappingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordTagMapping]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }


        public void CreateMultipleIfNotExist(List<string> betRecordNoPGS, SystemTagEnum tagID)
        {
            List<string> notExist = GetNotExistRecordNo(betRecordNoPGS, tagID);

            if (notExist.IsEmptyOrNull())
                return;

            foreach(string eachBetRecordNoPGS in notExist)
            {
                CreateOrQueueRetry(eachBetRecordNoPGS, tagID); ;
            }
        }


        public void CreateIfNotExist(string betRecordNoPGS, SystemTagEnum tagID)
        {
            var queryExist = GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = betRecordNoPGS.ToListCollection(),
                TagIDs = tagID.ToInteger().ToListCollection()
            });

            if (queryExist.IsNotEmptyNull())
                return;

            CreateOrQueueRetry(betRecordNoPGS, tagID); ;
        }


        public void CreateOrQueueRetry(string betRecordNoPGS, SystemTagEnum tagID, Guid? uuid = null)
        {
            this.CreateOrQueueRetry(new BetRecordTagMappingEntity
            {
                BetRecordNoPGS = betRecordNoPGS,
                TagID = tagID.ToInteger(),
                ReferenceUUID = uuid,
                CreateTime = DateTimeHelper.GetCurrentTime(),
            });
        }

        public void CreateOrQueueRetry(BetRecordTagMappingEntity entity)
        {
            base.CreateOrQueueRetry(entity, EazyQueueTypeEnum.BetRecordTagMapping);
        }


        public void AssignUUID(string betRecordNoPGS, int tagID , Guid uuid)
        {
            string sql = $@" UPDATE {TableName} SET [ReferenceUUID] = @newUUID
                             WHERE BetRecordNoPGS = @BetRecordNoPGS AND TagID = @TagID";

            var para = new
            {
                BetRecordNoPGS = betRecordNoPGS,
                TagID = tagID,
                newUUID = uuid
            };

            ExecuteSqlByDapper(sql , para);
        }

        public List<string> GetNotExistRecordNo(List<string> betRecordNoPGS, SystemTagEnum tagID)
        {
            if (betRecordNoPGS.IsEmptyOrNull())
                return new List<string>();

            betRecordNoPGS = betRecordNoPGS.Distinct().ToList();

            var queryExist = GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = betRecordNoPGS,
                TagIDs = tagID.ToInteger().ToListCollection()
            });

            List<string> existRecords = queryExist.Select(z => z.BetRecordNoPGS).ToList();

            List<string> notExist = betRecordNoPGS.Where(z => !existRecords.Contains(z)).ToList();
            return notExist;
        }


        public int GetCountByFilter(QueryBetRecordTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public BetRecordTagMappingEntity GetByUniqueKey(string betRecordNoPGS , int tagID)
        {
            var query = GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = betRecordNoPGS.ToListCollection(),
                TagIDs = tagID.ToListCollection()
            });

            return query.FirstOrDefault();
        }

        public List<BetRecordTagMappingEntity> GetByFilter(QueryBetRecordTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<BetRecordTagMappingEntity>(sql, filter);
            return query;
        }


        public PagedList<BetRecordTagMappingEntity> GetPagingByFilter(QueryBetRecordTagMappingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<BetRecordTagMappingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordTagMappingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGSList.IsNotEmptyNull())
                sql += " AND BetRecordNoPGS in @BetRecordNoPGSList ";

            if (filter.TagIDs.IsNotEmptyNull())
                sql += " AND TagID in @TagIDs ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            return sql;
        }

    }


}
