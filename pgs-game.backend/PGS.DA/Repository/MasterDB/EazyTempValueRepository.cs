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
    public class EazyTempValueRepository : FormatMultiplePrimaryKeyRepository<EazyTempValueEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[EazyTempValue]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "UUID"; }
        }



        /// <summary>
        /// 會自動幫忙entity塞預設值
        /// </summary>
        public override void Create(EazyTempValueEntity model)
        {
            SetEntityDefaultValueBeforeInsert(model);

            base.Create(model);
        }


        public void CreateOrQueueRetry(EazyTempValueEntity model)
        {
            SetEntityDefaultValueBeforeInsert(model);

            base.CreateOrQueueRetry(model , EazyQueueTypeEnum.EazyTempValue);
        }

        private void SetEntityDefaultValueBeforeInsert(EazyTempValueEntity model)
        {
            if (model.CreateTime == SystemConstant.EmptyTime)
            {
                model.CreateTime = DateTimeHelper.GetCurrentTime();
            }

            if (model.UUID == new Guid())
            {
                model.UUID = Guid.NewGuid();
            }

            model.Memo1 = model.Memo1.Truncate(200);
            model.Memo2 = model.Memo2.Truncate(200);
            model.Memo3 = model.Memo3.Truncate(200);
        }


        public void CounterPlus(Guid uuid)
        {
            string sql = $@" UPDATE {TableName} SET Counter = Counter + 1 WHERE UUID = @UUID ";
            ExecuteSqlByDapper(sql, new { UUID = uuid });
        }


        public EazyTempValueEntity GetByTransactionNo(string betRecordNoPGS , int? type = null)
        {
            var query = GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                BetRecordNoPGS = betRecordNoPGS,
                Type = type
            });

            return query.FirstOrDefault();
        }


        public List<EazyTempValueEntity> GetByFilter(QueryEazyTempValueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<EazyTempValueEntity>(sql, filter);
            return query;
        }


        public PagedList<EazyTempValueEntity> GetPagingByFilter(QueryEazyTempValueRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<EazyTempValueEntity>(sql, filter, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryEazyTempValueRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.UUIDs.IsNotEmptyNull())
                sql += " AND UUID in @UUIDs ";

            if(filter.ListOfBetRecordNoPGS.IsNotEmptyNull())
                sql += " AND BetRecordNoPGS in @ListOfBetRecordNoPGS ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";

            if (filter.Memo1.IsNotEmptyTextNotNull())
                sql += " AND Memo1 = @Memo1 ";

            if (filter.Type.HasValue)
                sql += " AND Type1 = @Type ";

            if (filter.Types.IsNotEmptyNull())
                sql += " AND Type1 in @Types ";

            return sql;
        }

    }


}
