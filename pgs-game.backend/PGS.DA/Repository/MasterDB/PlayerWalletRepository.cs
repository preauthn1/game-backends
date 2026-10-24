using Dapper;
using game.Entity;
using target.Core.Repository;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using target.Utility.Extensions;
using System.Linq;
using target.Models.Enums;
using System.Data.SqlClient;
using System.Data;
using target.Models.CoreLibs;
using target.Models.Models;

namespace target.DA.Repository
{

    public class PlayerWalletRepository : FormatIntegerPrimaryKeyRepository<PlayerWalletEntity>
    {
        protected override string ConnectionString
        {
            get { return GetConnectionStringByDbName(DBConnType.MasterDB); }
        }

        protected override string TableName
        {
            get { return "[dbo].[PlayerWallet]"; }
        }

        protected override string PrimaryKeyColumnName
        {
            get { return "PlayerID"; }
        }



        public PlayerWalletEntity GetPlayerWalletByType(int playerID, WalletTypeEnum type, bool isLockQuery = true)
        {
            var filter = new QueryPlayerWalletRequestViewModel
            {
                Type = type.ToInteger(),
                PlayerIDs = playerID.ToListCollection(),
                IsLockQuery = isLockQuery
            };

            var query = GetByFilter(filter);
            return query.FirstOrDefault();
        }


        public List<PlayerWalletEntity> GetByFilter(QueryPlayerWalletRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);
            var query = QueryByDapper<PlayerWalletEntity>(sql, filter);
            return query;
        }


        public PagedList<PlayerWalletEntity> GetPagingByFilter(QueryPlayerWalletRequestViewModel filter)
        {
            string sql = GenerateWhereSQL(filter);

            var query = GetPagingEntitiesBySQLScript<PlayerWalletEntity>(sql, filter, filter);
            return query;
        }


        /// <summary>
        /// 僅供試玩商戶的錢包更新 其他地方請勿亂使用
        /// </summary>
        public void UpdatePlayerWalletForTrialMode(int playerID, decimal newAmount)
        {
            try
            {
                Update(new PlayerWalletEntity
                {
                    PlayerID = playerID,
                    Amount = newAmount,
                    WalletType = WalletTypeEnum.GameWallet.ToInteger()
                });
            }
            catch
            {

            }
        }

        /// <summary>
        /// 扣除錢包金額(禁止負數)
        /// 請注意如果要扣錢，updateAmount 要帶入負數
        /// </summary>
        public UpdateBalanceSPResult UpdateWalletAmountForbiddenMinus(int playerID, int gameID, decimal updateAmount, TransactionTypeEnum transactionType, string orderNo, string ipAddress)
        {
            //SP包含了
            // 1. 更新錢包金額 (禁止負數)
            // 2. 寫交易log
            string spName = "[PrWeb_GamePlay_UpdatePlayerWalletThirdGameBetting]";

            var para = new DynamicParameters();
            para.Add("playerID", playerID);
            para.Add("gameID", gameID);
            para.Add("updateAmount", updateAmount);
            para.Add("transactionType", transactionType.ToInteger());
            para.Add("referenceOrderNo", orderNo);
            para.Add("IPAddress", ipAddress);
            para.Add(name: "afterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            para.Add(name: "@RetVal", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute(spName, param: para, commandType: CommandType.StoredProcedure);
                int returnCode = para.GetDynamicReturnCode();
                decimal afterAmount = para.GetOutputVariable<decimal>("afterAmount");

                return new UpdateBalanceSPResult
                {
                    BeforeAmount = afterAmount - updateAmount,
                    AfterAmount = afterAmount,
                    Code = (ApiErrorCode)returnCode,
                };
            }
        }


        /// <summary>
        /// 錢包轉帳 (轉入/轉出)
        /// </summary>
        public PlayerWalletTransferResultModel WalletTransfer(PlayerWalletTransferModel model)
        {
            var p = new DynamicParameters();
            p.Add("transferType", model.TranferType.ToInteger());
            p.Add("playerID", model.PlayerID);
            p.Add("amount", model.Amount, DbType.Decimal);
            p.Add("orderNo_PGS", model.OrderNo_PGS);
            p.Add("orderNo_merchant", model.OrderNo_Merchant);
            p.Add("isCreditMode", model.IsCreditMode);
            p.Add("convertRate", model.ConvertRate, DbType.Decimal);

            //返回參數
            p.Add("result_isSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("result_errorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("result_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
            p.Add("result_beforeAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 2);
            p.Add("result_afterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 2);


            var connString = GetConnectionStringByDbName(DBConnType.TransactionDB);

            try
            {
                using (var conn = new SqlConnection(connString))
                {
                    var result = conn.Execute("PrWeb_WalletTransfer", p, commandType: CommandType.StoredProcedure);

                    var result_isSuccess = p.Get<bool>("result_isSuccess");
                    var result_msg = p.Get<string>("result_msg");
                    var result_errorCode = p.Get<int>("result_errorCode");
                    var result_beforeAmount = p.Get<decimal>("result_beforeAmount");
                    var result_afterAmount = p.Get<decimal>("result_afterAmount");

                    return new PlayerWalletTransferResultModel
                    {
                        IsSuccess = result_isSuccess,
                        ErrorCode = (ApiErrorCode)result_errorCode,
                        Msg = result_msg,
                        BeforeAmount = result_beforeAmount,
                        AfterAmount = result_afterAmount
                    };
                }
            }
            catch (Exception ex)
            {
                if (ex.IsSqlUniqueException())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }

                throw;
            }
        }


        /// <summary>
        /// 錢包轉帳 (轉入/轉出) 
        /// 2022/12 新增。這是for遊戲商專用的
        /// </summary>
        public PlayerWalletTransferResultModel WalletTransferForGameVendor(GameVendorWalletTransferParameterModel model)
        {
            var p = new DynamicParameters();
            p.Add("transferType", model.TranferType.ToInteger());
            p.Add("playerID", model.PlayerID);
            p.Add("gameVendorID", model.GameVendorID);
            p.Add("amount", model.Amount, DbType.Decimal, precision: 18, scale: 4);
            p.Add("orderNo_PGS", model.OrderNo_PGS);
            p.Add("orderNo_vendor", model.OrderNo_GameVendor);

            //返回參數
            p.Add("result_isSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("result_errorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("result_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
            p.Add("result_beforeAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            p.Add("result_afterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);

            var connString = GetConnectionStringByDbName(DBConnType.TransactionDB);
            using (var conn = new SqlConnection(connString))
            {
                var result = conn.Execute("PrWeb_WalletTransferGameVendor", p, commandType: CommandType.StoredProcedure);

                var result_isSuccess = p.Get<bool>("result_isSuccess");
                var result_msg = p.Get<string>("result_msg");
                var result_errorCode = p.Get<int>("result_errorCode");
                var result_beforeAmount = p.Get<decimal>("result_beforeAmount");
                var result_afterAmount = p.Get<decimal>("result_afterAmount");

                return new PlayerWalletTransferResultModel
                {
                    IsSuccess = result_isSuccess,
                    ErrorCode = (ApiErrorCode)result_errorCode,
                    Msg = result_msg,
                    BeforeAmount = result_beforeAmount,
                    AfterAmount = result_afterAmount
                };
            }
        }


        /// <summary>
        /// 活動獎勵派彩
        /// 2023/03 新增。這是for遊戲商專用的
        /// </summary>
        public PlayerWalletTransferResultModel SendEventBonusForGameVendor(GameVendorWalletTransferParameterModel model)
        {
            var p = new DynamicParameters();
            p.Add("transferType", model.TranferType.ToInteger());
            p.Add("playerID", model.PlayerID);
            p.Add("gameVendorID", model.GameVendorID);
            p.Add("amount", model.Amount, DbType.Decimal, precision: 18, scale: 4);
            p.Add("orderNo_PGS", model.OrderNo_PGS);
            p.Add("orderNo_vendor", model.OrderNo_GameVendor);

            if (model.GameID.NotNullNotZeroNotMinus())
            {
                p.Add("gameID", model.GameID);
            }

            //返回參數
            p.Add("result_isSuccess", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            p.Add("result_errorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("result_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
            p.Add("result_beforeAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);
            p.Add("result_afterAmount", dbType: DbType.Decimal, direction: ParameterDirection.Output, precision: 18, scale: 4);

            var connString = GetConnectionStringByDbName(DBConnType.TransactionDB);
            using (var conn = new SqlConnection(connString))
            {
                var result = conn.Execute("PrWeb_SendEventBonusGameVendor", p, commandType: CommandType.StoredProcedure);

                var result_isSuccess = p.Get<bool>("result_isSuccess");
                var result_msg = p.Get<string>("result_msg");
                var result_errorCode = p.Get<int>("result_errorCode");
                var result_beforeAmount = p.Get<decimal>("result_beforeAmount");
                var result_afterAmount = p.Get<decimal>("result_afterAmount");

                return new PlayerWalletTransferResultModel
                {
                    IsSuccess = result_isSuccess,
                    ErrorCode = (ApiErrorCode)result_errorCode,
                    Msg = result_msg,
                    BeforeAmount = result_beforeAmount,
                    AfterAmount = result_afterAmount
                };
            }
        }




        private string GenerateWhereSQL(QueryPlayerWalletRequestViewModel filter)
        {
            string noLock = filter.IsLockQuery ? "" : " WITH (NOLOCK) ";
            string sql = $@"SELECT * FROM {TableName} {noLock}
                           WHERE 1=1 ";

            if (filter.PlayerIDs.IsNotEmptyNull())
                sql += " AND PlayerID in @PlayerIDs ";

            if (filter.Type.HasValue)
                sql += " AND WalletType = @Type ";

            return sql;
        }

    }


}
