using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using target.Core.Helper;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;

namespace target.Core.Repository
{
    public abstract class FormatBetRecordRepository<T> : FormatMultiplePrimaryKeyRepository<T> where T : class, IBetRecordEntity, IThirdPartyBetRecordEntity
    {
        protected abstract GameVendorTypeEnum GameVendor { get; }


        protected bool IsSupportParitionTime
        {
            get
            {
                return GameVendorDefinitionHelper.IsSupportPartitionTable(GameVendor);
            }
        }


        public virtual T GetByBetRecordNoPGS(string betRecordNoPGS, bool isNVarchar = false)
        {
            if (string.IsNullOrWhiteSpace(betRecordNoPGS))
                return null;

            var parsingResult = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(betRecordNoPGS);

            string extraWhere = "";

            if (this.IsSupportParitionTime)
            {
                if (parsingResult.Time == null)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel("BetRecordNoPGS"));
                }

                extraWhere = " AND PartitionTime = @PartitionTime ";
            }

            string sql = $"SELECT * FROM {TableName} WITH (NOLOCK) " +
                         $"WHERE BetRecordNoPGS = @BetRecordNoPGS {extraWhere}";

            var param = new
            {
                BetRecordNoPGS = isNVarchar ? ToNVarchar(betRecordNoPGS, 32) : ToVarchar(betRecordNoPGS, 32),
                PartitionTime = parsingResult.Time,
            };

            var query = QueryByDapper<T>(sql, param);
            return query.FirstOrDefault();
        }


        public T GetByBetRecordNo(string orderNo, string thirdPartyGameCode = "", string selectColumns = "*")
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return null;

            var query = GetByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                ThirdPartyGameCode = thirdPartyGameCode,
                BetRecordNo = orderNo

            }, selectColumns);

            return query.FirstOrDefault();
        }


        public void ExecuteUpdateToSettlementStatus(T updateEntity, bool isConvertParamByAttribute = false)
        {
            string extraWhere = "";

            if (this.IsSupportParitionTime)
            {
                extraWhere = " AND PartitionTime = @PartitionTime ";
            }

            string whereQuery = @$" BetRecordNoPGS = @BetRecordNoPGS {extraWhere}
                                    AND Status != {BetRecordStatusEnum.SettleComplete.ToInteger()}";

            //執行比一般更嚴格的where條件 並檢查是否有成功更新到一筆 (如果status不符合)
            var updateResult = this.UpdateByCustomizedWhere(updateEntity, whereQuery ,isConvertParamByAttribute);

            if (updateResult.UpdateRowCount < 1)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail);
            }

            return;
        }


        public List<T> GetByFilter(QueryBetRecordGeneralRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = QueryByDapper<T>(sql, filter, filter.DbQueryExtraConfig);
            return query;
        }

        public PagedList<T> GetPagingByFilter(QueryBetRecordGeneralRequestViewModel filter, string selectColumns = "*")
        {
            string sql = GenerateWhereSQL(filter, selectColumns);
            var query = GetPagingEntitiesBySQLScript<T>(sql, filter, filter);
            return query;
        }

        private string GenerateWhereSQL(QueryBetRecordGeneralRequestViewModel filter, string selectColumns = "*")
        {
            string debugText = filter.DebugSourceMethod.IsNotEmptyTextNotNull() ? $"-- {filter.DebugSourceMethod}{Environment.NewLine}" : "";

            string sql = debugText + 
                           $@"SELECT {selectColumns} FROM {TableName} WITH (NOLOCK)
                           WHERE 1=1 ";

            if (filter.PartitionTimeMatch.HasValue)
                sql += " AND PartitionTime = @PartitionTimeMatch ";

            if (filter.PartitionTimeBegin.HasValue)
                sql += " AND PartitionTime >= @PartitionTimeBegin ";

            if (filter.PartitionTimeEnd.HasValue)
                sql += " AND PartitionTime < @PartitionTimeEnd ";

            if (filter.BetRecordNo.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNo = @BetRecordNo ";

            if (filter.ListOfBetRecordNo.IsNotEmptyNull())
                sql += " AND BetRecordNo in @ListOfBetRecordNo ";

            if (filter.BetRecordNoPGS.IsNotEmptyTextNotNull())
                sql += " AND BetRecordNoPGS = @BetRecordNoPGS ";

            if (filter.ThirdPartyGameCode.IsNotEmptyTextNotNull())
                sql += " AND ThirdPartyGameCode = @ThirdPartyGameCode ";

            if (filter.GameID.HasValue)
                sql += " AND GameID = @GameID ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.Status.HasValue)
                sql += " AND Status = @Status ";

            if (filter.CreateTimeBegin.HasValue)
                sql += " AND CreateTime >= @CreateTimeBegin ";

            if (filter.CreateTimeEnd.HasValue)
                sql += " AND CreateTime < @CreateTimeEnd ";


            return sql;
        }

    }

}
