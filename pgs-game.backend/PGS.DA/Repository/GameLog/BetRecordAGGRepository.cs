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
    public class BetRecordAGGRepository : FormatBetRecordRepository<BetRecordAGGEntity>
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
            get { return "[ThirdParty].[BetRecordAGG]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "BetRecordNoPGS"; }
        }

        protected override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.AGG;
            }
        }


        public override void Create(BetRecordAGGEntity model)
        {
            var param = base.ConvertClassToDapperDynamicParameters(model);

            string insertSQL = SQLHelper.GetInsertCommand<BetRecordAGGEntity>(TableName);
            base.ExecuteSqlByDapper(insertSQL, param);
        }


        public void UpdateToSettlementStatus(BetRecordAGGEntity updateEntity)
        {
            var parsingResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(updateEntity.BetRecordNoPGS);
            updateEntity.PartitionTime = parsingResult.Time.Value;

            string whereQuery = @$" BetRecordNoPGS = @BetRecordNoPGS 
                                    AND PartitionTime = @PartitionTime 
                                    AND Status != {BetRecordStatusEnum.SettleComplete.ToInteger()} ";

            //執行比一般更嚴格的where條件 並檢查是否有成功更新到一筆 (如果status不符合)
            var updateResult = this.UpdateByCustomizedWhere(updateEntity, whereQuery, isConvertParamByAttribute: true);

            if (updateResult.UpdateRowCount < 1)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);
            }

            return;
        }

        public override BetRecordAGGEntity GetByBetRecordNoPGS(string betRecordNoPGS, bool isNVarchar = true)
        {
            return base.GetByBetRecordNoPGS(betRecordNoPGS , isNVarchar: false);
        }



        public List<string> GetCancelTagAndLockPendingRecords(DateTime beginTime)
        {
            string sql = @"SELECT BetRecordNoPGS from [ThirdParty].[BetRecordAGG] with (NOLOCK)
                            WHERE PartitionTime >= @beginTime AND PartitionTime < @endTime AND Status = 0 AND BetRecordNo in
                            (
                                SELECT BetRecordNo from [PGS_GameLog].[dbo].[BetRecordRawWagerTagMapping] with (NOLOCK)
                                WHERE TagID = @tagID AND CreateTime >= @beginTime
                            )";

            var param = new
            {
                beginTime = beginTime,
                endTime = DateTimeHelper.GetCurrentTime().AddDays(1),
                tagID = SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger()
            };

            var query = QueryByDapper<string>(sql, param);
            return query;
        }


    }


}
