using target.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.Enums;
using target.Models.Table;

namespace target.DA.Repository
{
    public class RepositoryHelper
    {
        public static void CheckStatusIsPendingForCancelProcess<T>(T data) where T : IBetRecordEntity
        {
            if (data.Status == BetRecordStatusEnum.Cancel.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel);
            }
            else if (data.Status != BetRecordStatusEnum.Pending.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel);
            }
        }

        /// <summary>
        /// 判斷注擔當前狀態是否為 預期合法的的狀態(呼叫者自行指定)
        /// </summary>
        public static void CheckBetRecordStatusIsWhiteList<T>(T data, BetRecordStatusEnum statusRules) where T : IBetRecordEntity
        {
            CheckBetRecordStatusIsWhiteList(data , new List<int> { statusRules.ToInteger() });
        }

        /// <summary>
        /// 判斷注擔當前狀態是否為 不允許的狀態(呼叫者自行指定)
        /// </summary>
        public static void CheckBetRecordStatusIsBlackList<T>(T data, BetRecordStatusEnum statusRules) where T : IBetRecordEntity
        {
            CheckBetRecordStatusIsBlackList(data, new List<int> { statusRules.ToInteger() });
        }


        public static void CheckBetRecordStatusIsWhiteList<T>(T data, List<int> statusRules) where T : IBetRecordEntity
        {
            //檢查是否為 [指定允許] 的狀態
            if (!statusRules.Contains(data.Status))
            {
                if(data.Status == BetRecordStatusEnum.SettleComplete.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement);
                }
                else if (data.Status == BetRecordStatusEnum.Cancel.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel);
                }
                else
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus);
                }
            }
        }

        public static void CheckBetRecordStatusIsBlackList<T>(T data, List<int> statusRules) where T : IBetRecordEntity
        {
            //檢查是否為 [禁止] 的狀態
            if (statusRules.Contains(data.Status))
            {
                if (data.Status == BetRecordStatusEnum.SettleComplete.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement);
                }
                else if (data.Status == BetRecordStatusEnum.Cancel.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel);
                }
                else
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus);
                }
            }
        }

        public static void CheckRecordNotFoundAndDuplicateStatus<T>(T data, BetRecordStatusEnum targetStatus, decimal? targetPayoutAmount) where T: IBetRecordEntity
        {
            //傳入null 代表查無資料
            if (data == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            //如果狀態一樣 禁止操作(可能同時操作了)
            if (data.Status == (int)targetStatus)
            {
                var error = new ErrorDetailModel
                {

                };

                //如果重複發通知了 但是金額不一樣
                if (targetPayoutAmount.HasValue && data.PayoutAmount != Math.Round(targetPayoutAmount ?? 0 , 4))
                {
                    error.IsAlwaysNotify = true;
                    error.LogExtraInfo = $"二次回調金額與上次不符合。BetRecordNoPGS: {data.BetRecordNoPGS}, 當前 PayoutAmount: {data.PayoutAmount}，預計 PayoutAmount: {targetPayoutAmount}";
                }

                throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadySettlement, error);
            }                
        }

        public static string GenerateDebugInfo(int currentStatus, string inputMemo)
        {
            return string.Format(" Before status: {0} . {1}", currentStatus, inputMemo);
        }


    }
}
