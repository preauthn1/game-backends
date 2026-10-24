using Dapper;
using game.Entity;
using Lck.Utility.Extensions.Helper;
using target.Core.Helper;
using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace target.DA.Repository
{

    public class BetRecordFunkyRepository : FormatBetRecordRepository<BetRecordFunkyEntity>
    {
        protected override string ConnectionString
        {
            get
            {
                return GetConnectionStringByDbName(GameVendorDefinitionHelper.GetDbConnType(GameVendor));
            }
        }

        protected override string TableName
        {
            get { return "[ThirdParty].[BetRecordFunky]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }
               
        protected override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.Funky;
            }
        }

        public override void Create(BetRecordFunkyEntity model)
        {
            var param = base.ConvertClassToDapperDynamicParameters(model);

            string insertSQL = SQLHelper.GetInsertCommand<BetRecordFunkyEntity>(TableName);
            base.ExecuteSqlByDapper(insertSQL, param);
        }


        public void UpdateToSettlementStatus(BetRecordFunkyEntity updateEntity)
        {
            var parsingResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(updateEntity.BetRecordNoPGS);
            updateEntity.PartitionTime = parsingResult.Time.Value;

            base.ExecuteUpdateToSettlementStatus(updateEntity , isConvertParamByAttribute: true);
        }

        public BetRecordSummaryReportModel GetTotalSummaryReport(DateTime beginUTC, DateTime endUTC)
        {
            string sql = $@"SELECT count(*) as TotalBetCount , 
                                sum(BetAmount) as TotalBetAmount , 
                                sum(WinLossAmount) as TotalWinLoss , 
                                sum(PayoutAmount) as TotalPayoutAmount,
                                sum(ValidAmount) as TotalValidBetAmount
                           FROM {TableName} WITH (NOLOCK)
                           WHERE Status = @status AND SettlementTime >= @beginUTC AND SettlementTime < @endUTC";

            var para = new
            {
                status = BetRecordStatusEnum.SettleComplete.ToInteger(),
                beginUTC,
                endUTC
            };

            var query = QueryByDapper<BetRecordSummaryReportModel>(sql, para);
            return query.FirstOrDefault();
        }

        public BetRecordFunkyEntity GetByBetRecordNo(string orderNo , string gameCode = "")
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return null;

            var query = GetByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                ThirdPartyGameCode = gameCode,
                BetRecordNo = orderNo
            });

            return query.FirstOrDefault();
        }


        public override BetRecordFunkyEntity GetByBetRecordNoPGS(string betRecordNoPGS, bool isNVarchar = false)
        {
            return base.GetByBetRecordNoPGS(betRecordNoPGS, false);
        }


    }


}
