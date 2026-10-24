using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models;
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
    public class OneTimesJobSettingRepository : FormatLongPrimaryKeyRepository<OneTimesJobSettingEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[OneTimesJobSetting]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "JobID"; }
        }


        public OneTimesJobSettingEntity InstanceInsertEntity(OneTimesJobTypeEnum jobType , DateTime nextTime , int? sourceID , bool isAlwaysNotify = true)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();
            return new OneTimesJobSettingEntity
            {
                Type = jobType.ToInteger(),
                SourceID = sourceID,
                CreateTime = now,
                CreateBy = "system",
                Status = OneTimesJobStatusEnum.Pending.ToInteger(),
                IsAlwaysNotify = isAlwaysNotify,
                RetryTimes = 0,
                Memo = jobType.ToString(),
                NextTime = nextTime
            };
        }


        public void UpdateToTargetStatus(List<long> ids , OneTimesJobStatusEnum status , string debugUUID = "")
        {
            string extraUpateColumns = string.Empty;

            // [處理中] 的時候有比較多的欄位要更新
            if(status == OneTimesJobStatusEnum.OnExecuted)
            {
                extraUpateColumns = @" , LastDebugUUID = @debugUUID
                                       , LastTime = @lastTime  
                                       , RetryTimes = RetryTimes + 1";
            }

            string sql = $@" UPDATE {TableName} SET 
                                Status = @status 
                                {extraUpateColumns}

                             WHERE JobID in @ids";

            var para = new
            {
                lastTime = DateTimeHelper.GetCurrentTime(),
                ids = ids,
                debugUUID = debugUUID,
                status = status.ToInteger()
            };

            ExecuteSqlByDapper(sql , para);
        }

        public int GetCountByFilter(QueryOneTimeJobSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter, " count(*) ");
            var query = QueryByDapper<int>(sql, filter);
            return query.FirstOrDefault();
        }


        public OneTimesJobSettingEntity GetJobByEventID(int eventID)
        {
            var query = GetByFilter(new QueryOneTimeJobSettingRequestViewModel
                {
                    SourceIDs = eventID.ToListCollection(),
                    Types = SystemConstant.EventReferOneTimeJobTypes
                })
                .FirstOrDefault();

            return query;
        }


        public List<OneTimesJobSettingEntity> GetByFilter(QueryOneTimeJobSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<OneTimesJobSettingEntity>(sql, filter);
            return query;
        }


        public PagedList<OneTimesJobSettingEntity> GetPagingByFilter(QueryOneTimeJobSettingRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<OneTimesJobSettingEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryOneTimeJobSettingRequestViewModel filter, string selectColumns = "*")
        {
            string sql = $@"SELECT {selectColumns} FROM {TableName} 
                           WHERE 1=1 ";

            if(!string.IsNullOrWhiteSpace(filter.MemoMatch))
                sql += $" AND Memo = @MemoMatch ";

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                sql += $" AND {GenerateLikeQuery("Memo", "@Keyword")} ";
            }

            if (filter.Types.IsNotEmptyNull())
                sql += $" AND Type in @Types ";

            if (filter.SourceIDs.IsNotEmptyNull())
                sql += $" AND SourceID in @SourceIDs ";

            if (filter.IsReadyToExecuted)
            {
                List<int> targetStatus = new List<int>
                {
                    OneTimesJobStatusEnum.Pending.ToInteger(),
                    OneTimesJobStatusEnum.MustRetry.ToInteger(),
                };

                string statusText = string.Join(",", targetStatus);
                sql += $" AND ( Status in ( {statusText} ) AND  GetUTCDate() >= NextTime )  ";
            }

            return sql;
        }

    }


}
