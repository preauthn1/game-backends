using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace target.DA.Repository
{
    public class BetRecordExtraInfoRepository : FormatMultiplePrimaryKeyRepository<BetRecordExtraInfoEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.DebugInfoDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[BetRecordExtraInfo]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override bool IsConvertToDbValueType
        {
            get
            {
                return true;
            }
        }


        private const int MaxLength = 1000;


        public void CreateOrUpdateAsync(string recordNo, string debugInfo)
        {
            Task.Factory.StartNew(() =>
            {
                CreateOrUpdate(new BetRecordExtraInfoEntity
                {
                    BetRecordNoPGS = recordNo,
                    ExtraInfo = debugInfo
                });
            });
        }


        public void CreateOrUpdate(string betRecordNoPGS, string info)
        {
            CreateOrUpdate(new BetRecordExtraInfoEntity
            {
                BetRecordNoPGS = betRecordNoPGS,
                ExtraInfo = info
            });
        }


        public void CreateOrUpdate(BetRecordExtraInfoEntity entity)
        {
            try
            {
                if (entity.PartitionTime == SystemConstant.EmptyTime)
                {
                    var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(entity.BetRecordNoPGS);
                    entity.PartitionTime = recover.Time.Value;
                }

                entity.ExtraInfo = entity.ExtraInfo.Truncate(MaxLength);

                var queryExist = GetByBetRecordNoPGS(entity.BetRecordNoPGS);

                if (queryExist == null)
                {
                    Create(entity);
                }
                else
                {
                    //空字串就不需要update了
                    if (!string.IsNullOrWhiteSpace(entity.ExtraInfo))
                    {
                        queryExist.ExtraInfo += entity.ExtraInfo;
                        queryExist.ExtraInfo = queryExist.ExtraInfo.Truncate(MaxLength);
                        Update(queryExist);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        public BetRecordExtraInfoEntity GetByBetRecordNoPGS(string pgsNo)
        {
            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(pgsNo);

            var query = GetByFilter(new QueryBetRecordExtraInfoRequestViewModel
            {
                BetRecordNoPGS = pgsNo,
                PartitionTime = recover.Time.Value
            });

            return query.FirstOrDefault();
        }


        public override void Create(BetRecordExtraInfoEntity model)
        {
            if (model.PartitionTime == SystemConstant.EmptyTime)
            {
                var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(model.BetRecordNoPGS);
                model.PartitionTime = recover.Time.Value;
            }

            base.Create(model);
        }


        public List<BetRecordExtraInfoEntity> GetByFilter(QueryBetRecordExtraInfoRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<BetRecordExtraInfoEntity>(sql, filter);
            return query;
        }


        private string GenerateWhereSQL(QueryBetRecordExtraInfoRequestViewModel filter)
        {
            string sql = $@"SELECT * FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            if (filter.PartitionTime.HasValue)
                sql += " AND PartitionTime = @PartitionTime ";

            return sql;
        }

    }


}
