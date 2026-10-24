using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Repository;
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

namespace target.DA.Repository
{
    public class BetRecordGPIRepository : FormatBetRecordRepository<BetRecordGPIEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.GameLogDB); }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[BetRecordGPI]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.GPI;
            }
        }

        public void UpdateToSettlementStatus(BetRecordGPIEntity updateEntity)
        {
            var parsingResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(updateEntity.BetRecordNoPGS);
            updateEntity.PartitionTime = parsingResult.Time.Value;

            base.ExecuteUpdateToSettlementStatus(updateEntity);
        }


        public void UpdateBetAmountPlus(string betRecordNoPGS, decimal plusAmount)
        {
            var parsingResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            var param = new
            {
                BetRecordNoPGS = betRecordNoPGS,
                PartitionTime = parsingResult.Time.Value,
                plusAmount = plusAmount
            };

            string sql = $@" UPDATE {TableName} SET [BetAmount] = [BetAmount] + @plusAmount
                            WHERE BetRecordNoPGS = @BetRecordNoPGS AND [PartitionTime] = @PartitionTime ";

            ExecuteSqlByDapper(sql, param);
        }

    }

}
