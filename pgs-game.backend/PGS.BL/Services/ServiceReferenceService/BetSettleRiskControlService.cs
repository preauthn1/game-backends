using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Utility.Helper;
using game.Entity.Game;
using System.Threading.Tasks;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility.Helper.HttpContext;
using target.Models;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class BetSettleRiskControlService : BaseService, IBetSettleRiskControlService
    {

        public RiskControlConfigRepository RiskControlConfigRepository { get; set; }

        public RiskControlRecordRepository RiskControlRecordRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }


        public RiskControlResultModel CheckRiskControl(RiskControlCheckParameterModel model)
        {
            var result = new RiskControlResultModel();


            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            //先沒空管理這個了
            return result;

            if (!SettingHelper.IsDev)
                return result;

            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.PayoutAmount).Rule_NotMinus();

            string debugText = string.Empty;

            List<RiskControlResultModel> listOfCheckResult = new List<RiskControlResultModel>();

            try
            {
                string cacheKey = CacheKey.DbEntity_RiskControlConfig_MerchantID_GameID(model.GameID, model.MerchantID);

                var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
                {
                    var query = RiskControlConfigRepository.GetByFilter(new QueryRiskControlConfigRequestViewModel
                    {
                        Status = NormalActiveStatus.Enable.ToInteger(),
                        GameIDs = model.GameID.ToListCollection(),
                        MerchantIDs = model.MerchantID.ToListCollection()
                    });

                    return query;

                }, 15);

                if (queryCache.IsEmptyOrNull())
                    return result;

                decimal payoutRate = model.BetAmount == 0 ? 0 : (model.PayoutAmount / model.BetAmount);

                foreach (var eachRiskControl in queryCache.OrderBy(z => z.SettingJson))
                {
                    debugText = $"configID: {eachRiskControl.RiskControlConfigID}";
                    var eachResult = CheckRiskControlByEntity(eachRiskControl , model);

                    //有些動作 是需要主要method那邊處理
                    if(eachResult.IsResponseFakeSuccessButPendingRecordStatus)
                    {
                        result.IsResponseFakeSuccessButPendingRecordStatus = true;
                    }

                    listOfCheckResult.Add(eachResult);
                }

            }
            catch (Exception ex)
            {
                string msg = $"CheckRiskControl失敗啦，ID:{debugText}" + ex.Message;
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_external_api,
                    Message = msg
                });
            }

            //如果有任何指定要求要拋exception
            bool isAnyException = listOfCheckResult.Any(z => z.IsForceException);

            if(isAnyException)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettlementErrorByRiskControl);
            }

            result.ListOfDebugInfo = listOfCheckResult;
            return result;
        }


        public RiskControlResultModel CheckRiskControlByEntity(RiskControlConfigEntity eachRiskControl, RiskControlCheckParameterModel model)
        {
            var result = new RiskControlResultModel();
            result.RiskControlConfigID = eachRiskControl.RiskControlConfigID;

            decimal payoutRate = model.BetAmount == 0 ? 0 : (model.PayoutAmount / model.BetAmount);

            var json = eachRiskControl.SettingJson;

            //要判斷的規則與行為 都用這個json去定義 (懶得設計關聯式資料庫了)
            RiskControlSettingJsonModel parseModel = json.TryJsonConvertToModel<RiskControlSettingJsonModel>();

            if (parseModel == null)
            {
                string msg = $"RiskControlConfigID:{eachRiskControl.RiskControlConfigID} json格式不合法";
                SendMessageToMultipleChannel(msg);
                return result;
            }

            foreach (var eachTarget in parseModel.RuleTargetDefines)
            {
                bool isAllMatch = true;

                //依序檢查每一項rules
                foreach (var eachRule in eachTarget.Rules)
                {
                    var ruleTypeEnum = (RiskControlRuleTypeEnum)eachRule.RuleType;

                    switch (ruleTypeEnum)
                    {
                        case RiskControlRuleTypeEnum.PayoutAmountLargeThan:
                            {
                                var compareValue = eachRule.CompareNumericValueLargeThan;
                                isAllMatch = model.PayoutAmount > compareValue;
                                break;
                            }

                        case RiskControlRuleTypeEnum.PayoutRateLargeThan:
                            {
                                var compareValue = eachRule.CompareNumericValueLargeThan;
                                isAllMatch = payoutRate > compareValue;
                                break;
                            }

                        case RiskControlRuleTypeEnum.CounterRiskControlTimes:
                            {
                                int compareValue = (int)eachRule.CompareNumericValueLargeThan;
                                DateTime beginTime = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * eachRule.CompareDurationMinutes);

                                //查出時間區間內 有多少筆紀錄
                                int count = GetRecordCountByConfigID(eachRiskControl.RiskControlConfigID, beginTime);
                                isAllMatch = count > (int)compareValue;
                                break;
                            }

                        default:
                            throw new TerproException(ApiErrorCode.NotImplement);
                    }

                    if (!isAllMatch)
                        continue;

                    DateTime now = DateTimeHelper.GetCurrentTime();


                    var debugInfo = new
                    {
                        para = model,
                        settingJson = parseModel
                    };

                    //寫入風控紀錄
                    RiskControlRecordRepository.Create(new RiskControlRecordEntity
                    {
                        TargetRiskControlConfigID = eachRiskControl.RiskControlConfigID,
                        CreateTime = now,
                        GameID = model.GameID,
                        MerchantID = model.MerchantID,
                        PlayerID = model.PlayerID,
                        DebugInfo = debugInfo.ConvertToJsonString(),
                        Memo = $"{model.BetRecordNoPGS}",
                    });

                    //清除cache
                    string removeCacheKey = CacheKey.ExternalAPI_RiskControlRecordCount_ByConfigID(eachRiskControl.RiskControlConfigID);
                    CacheHelper.RemoveCacheByKey(removeCacheKey);

                    //依序執行有要求進行的動作
                    if (eachTarget.Action.IsNotify)
                    {
                        string msg = $"投注異常風控 PlayerID:{model.PlayerID} ,PgsNO: {model.BetRecordNoPGS} ,GameID:{model.GameID} ," +
                            $"BetAmount:{model.BetAmount} ,PayoutAmount:{model.PayoutAmount} ,PayoutRate(賠率):{payoutRate}";

                        SendMessageToMultipleChannel(msg);
                    }

                    if (eachTarget.Action.IsCloseGame)
                    {
                        var para = new
                        {
                            Status = NormalActiveStatus.Disable.ToInteger()
                        };

                        SendMessageToMultipleChannel($"風控條件符合, 將自動關閉遊戲 GameID:{model.GameID}");
                        GameListRepository.Update(model.GameID, para);
                    }

                    if (eachTarget.Action.IsResponseFakeSuccessButPendingRecordStatus)
                    {
                        result.IsResponseFakeSuccessButPendingRecordStatus = true;
                    }

                    if (eachTarget.Action.IsForceException)
                    {
                        result.IsForceException = true;
                    }
                }
            }

            return result;

        }


        private int GetRecordCountByConfigID(int riskControlConfigID, DateTime beginTime)
        {
            //沒有把 beginTime 也納入cacheKey，當資料異動時 會確實清除cache 所以應該不至於造成資訊不同不
            string cacheKey = CacheKey.ExternalAPI_RiskControlRecordCount_ByConfigID(riskControlConfigID);

            var queryCount = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var queryCount = RiskControlRecordRepository.GetCountByConfigID(riskControlConfigID, beginTime);
                   return new { Count = queryCount };

               }, 30);

            return queryCount.Count;
        }




        private void SendMessageToMultipleChannel(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_pgs_risk_control,
                Message = msg
            });

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_risk_control
            });
        }

    }
}
