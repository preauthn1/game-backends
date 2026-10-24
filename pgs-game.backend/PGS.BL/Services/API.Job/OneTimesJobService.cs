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
using target.Models;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Entity.Game;
using System.Threading;
using target.Utility;
using target.Utility.IOC;

namespace target.BL.Services
{

    public class OneTimesJobService : BaseJobService, IOneTimesJobService
    {
        public OneTimesJobSettingRepository OneTimeJobSettingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public IBetRecordService BetRecordService { get; set; }

        public IEventManagementService EventManagementService { get; set; }

        public IGameVendorSingleWalletService GameVendorSingleWalletService { get; set; }


        private static object _lockObject_OneTime = new object();

        /// <summary>
        /// 檢查是否有執行時間到的一次性排程要執行
        /// </summary>
        public async Task<CRUDResultModel> CheckOneTimesJobToExecute()
        {
            List<int> filterSourceIDs = new List<int>();

            //本地測試的話 只查詢特定
            if (SettingHelper.IsLocalhostWebSiteName)
            {
                filterSourceIDs = new List<int>
                {
                    //65536117
                };
            }

            var result = await CheckOneTimesJobToExecute(filterSourceIDs);
            return result;
        }


        /// <summary>
        /// 檢查是否有執行時間到的一次性排程要執行
        /// </summary>
        public async Task<CRUDResultModel> CheckOneTimesJobToExecute(List<int> filterSourceIDs)
        {
            SetSystemDefaultUserID();

            CRUDResultModel defaultResult = new CRUDResultModel("nothing");

            List<OneTimesJobSettingEntity> queryJobs = new List<OneTimesJobSettingEntity>();

            string debugUUID = RandomHelper.GetOrderNo("Job");

            //加個lock機制避免重複執行
            if (Monitor.TryEnter(_lockObject_OneTime, new TimeSpan(0, 0, 10)))
            {
                try
                {
                    queryJobs = OneTimeJobSettingRepository.GetByFilter(new QueryOneTimeJobSettingRequestViewModel
                    {
                        SourceIDs = filterSourceIDs,
                        IsReadyToExecuted = true
                    });

                    if (queryJobs.IsEmptyOrNull())
                        return defaultResult;

                    List<long> targetIDs = queryJobs.Select(z => z.JobID).ToList();

                    //先將全部狀態改成執行中 (避免處理太慢 下一輪的排程又觸發相同的排程)
                    OneTimeJobSettingRepository.UpdateToTargetStatus(targetIDs, OneTimesJobStatusEnum.OnExecuted, debugUUID);
                }
                finally
                {
                    Monitor.Exit(_lockObject_OneTime);
                }
            }

            if (queryJobs.IsEmptyOrNull())
                return defaultResult;

            //非同步。避免卡住線程
            await Task.Run(() =>
            {               
                var startTime = DateTimeHelper.GetCurrentTime();

                bool isAllCancelNotFound = queryJobs.All(z => z.Type == (int)OneTimesJobTypeEnum.BetRecord_CheckCancelNotFoundAgain);

                foreach (var item in queryJobs)
                {
                    ExecutedJobActionByType(item, debugUUID);
                }

                var endTime = DateTimeHelper.GetCurrentTime();
                int totalSeconds = (int)(endTime - startTime).TotalSeconds;

                if (!isAllCancelNotFound && totalSeconds > 30)
                {
                    string msg = $"CheckOneTimesJobToExecute 共 {queryJobs.Count()} 筆，花費時間 {totalSeconds} 秒";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_job_api);
                }
            });

            return new CRUDResultModel
            {
                Message = $"本次共{queryJobs.Count()}筆 要執行"
            };
        }



        private void ExecutedJobActionByType(OneTimesJobSettingEntity item, string debugUUID)
        {
            var typeEnum = (OneTimesJobTypeEnum)item.Type;

            bool isExecuteOnesOnly = false;

            try
            {
                switch (typeEnum)
                {
                    case OneTimesJobTypeEnum.Event_RTPEvent_ResetAllPlayer:
                        ResetPlayerRTP(item, debugUUID);
                        break;

                    case OneTimesJobTypeEnum.Event_RankingEvent_SendRankingPrize:
                        isExecuteOnesOnly = true;
                        SendRankingPrize(item, debugUUID);
                        break;

                    case OneTimesJobTypeEnum.SendMessageToNotifyChannel:
                        NotifyToMultipleChannel(item.SettingJson);
                        break;

                    case OneTimesJobTypeEnum.BetRecord_RefundWagerPlacebetException:
                        isExecuteOnesOnly = true;
                        RefundWagerPlacebetException(item);
                        break;

                    case OneTimesJobTypeEnum.BetRecord_CheckCancelNotFoundAgain:
                        BetRecord_CheckCancelNotFoundAgain(item);
                        break;

                    case OneTimesJobTypeEnum.BetRecord_CheckAginWagerIsCancel:
                        BetRecord_CheckAginWagerIsCancel(item);
                        break;

                    case OneTimesJobTypeEnum.Execute_TargetStoreProcedure:
                        Execute_TargetStoreProcedure(item);
                        break;

                    default:
                        throw new TerproException(ApiErrorCode.NotImplement);
                }

                //狀態更新為成功
                OneTimeJobSettingRepository.UpdateToTargetStatus(item.JobID.ToListCollection(), OneTimesJobStatusEnum.Success);

                //如果有指定再複製一個
                if (item.CloneNewAndAssignNextMinutes.NotNullNotZeroNotMinus())
                {
                    var clone = item.ReturnInjecteObject<OneTimesJobSettingEntity>();
                    clone.NextTime = item.NextTime.AddMinutes(item.CloneNewAndAssignNextMinutes.Value);
                    clone.RetryTimes = 0;
                    clone.Status = OneTimesJobStatusEnum.Pending.ToInteger();

                    OneTimeJobSettingRepository.Create(clone);
                }

                if (item.IsAlwaysNotify)
                {
                    string msg = $"JobType:{typeEnum.ToString()} , JobID: {item.JobID} {item.Memo} 執行成功!";
                    NotifyToMultipleChannel(msg);
                }
            }
            catch (Exception ex)
            {
                //超過retry次數就不處理了
                if (item.RetryTimes >= 5)
                {
                    OneTimeJobSettingRepository.UpdateToTargetStatus(item.JobID.ToListCollection(), OneTimesJobStatusEnum.Fail);
                }
                else if (isExecuteOnesOnly)
                {
                    OneTimeJobSettingRepository.UpdateToTargetStatus(item.JobID.ToListCollection(), OneTimesJobStatusEnum.Fail);
                }
                else
                {
                    OneTimeJobSettingRepository.UpdateToTargetStatus(item.JobID.ToListCollection(), OneTimesJobStatusEnum.MustRetry);
                }

                string msg = $"Type:{typeEnum.ToString()} , OneTime_Job_ID: {item.JobID} {item.Memo} 失敗! 重試次數:{item.RetryTimes}{Environment.NewLine} , debugUUID:{debugUUID}, {ex.GetMessageAndErrorLine()}";
                NotifyToMultipleChannel(msg);

                throw;
            }
        }


        protected virtual void ResetPlayerRTP(OneTimesJobSettingEntity queryEntity, string debugUUID)
        {
            if (!queryEntity.SourceID.HasValue)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);

            int eventID = queryEntity.SourceID.Value;

            EventManagementService.ResetPlayerRTPByEvent(_defaultUserID, new ResetAllPlayerRTPByEventRequestModel
            {
                EventID = eventID,
                Memo = $"CheckOneTimeJobToExecute 排程, DebugUUID: {debugUUID}"
            });
        }

        protected virtual void SendRankingPrize(OneTimesJobSettingEntity queryEntity, string debugUUID)
        {
            if (!queryEntity.SourceID.HasValue)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);

            int eventID = queryEntity.SourceID.Value;

            var eventService = IOCHelper.Resolve<IEventCommonPrizeSendService>();

            eventService.SendRankEventPrize(eventID, new RankEventTriggerSendPrizeParameter
            {
                OrderBy = null
            });

            SendMessageToMultipleChannel($"SendRankingPrize 排程已觸發 {eventID}", TelegramChannelEnum.pgs_job_api);
        }


        private void BetRecord_CheckCancelNotFoundAgain(OneTimesJobSettingEntity item)
        {
            try
            {
                var json = item.SettingJson.JsonConvertToModel<OneTimeCancelNotFoundSettingJsonModel>();

                //不管失敗或成功。就真的只執行一次
                GameVendorSingleWalletService.CancelBet((GameVendorTypeEnum)json.GameVendorID, new SingleWalletCancelBetRequestParameter
                {
                    //ps. 這邊的流程確實無法知道 betRecordNoPGS
                    BetRecordNo = json.BetRecordNo,
                    GameCode = json.GameCode,
                    ExtraInfo = new CancelExtraParameterModel
                    {
                        IsIgnoreCancelNotFoundProcess = true,
                        MaxHourRange = 2,
                    }
                });

            }
            catch (Exception ex)
            {

            }

        }


        private void BetRecord_CheckAginWagerIsCancel(OneTimesJobSettingEntity item)
        {
            try
            {
                var json = item.SettingJson.JsonConvertToModel<OneTimeAGinWagerStatusCheckSettingJsonModel>();

                var agService = new AGinIntegrationService();

                var checkResult = agService.GetWagerStatusByAPI(json.BetRecordNo);

                if (checkResult.IsCallbackFailThenCancel)
                {
                    //不管失敗或成功。就真的只執行一次 (如果callback失敗 應該會有notfitQueue補救)
                    GameVendorSingleWalletService.CancelBet(GameVendorTypeEnum.AGin, new SingleWalletCancelBetRequestParameter
                    {
                        //ps. 這邊的流程確實無法知道 betRecordNoPGS
                        BetRecordNo = json.BetRecordNo,
                        GameCode = json.GameCode,
                        ExtraInfo = new CancelExtraParameterModel
                        {
                            IsIgnoreCancelNotFoundProcess = true,
                            MaxHourRange = 2,
                        }
                    });
                }

            }
            catch (Exception ex)
            {

            }

        }



        private void Execute_TargetStoreProcedure(OneTimesJobSettingEntity item)
        {
            var targetSP = item.SettingJson.JsonConvertToModel<CommonSPExecuteModel>();

            string defaultMsg = $"Job執行失敗 {item.JobID} ";

            if (targetSP == null || string.IsNullOrWhiteSpace(targetSP.SPName))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, defaultMsg + ", 無法解析json");
            }

            var spRepo = new SystemSPRepository();

            var spResult = spRepo.ExecuteTargetProcedureAndCheckSuccess(targetSP.DBName, targetSP.SPName);

            if (!spResult.IsSuccess)
            {
                string myMsg = defaultMsg + $" SP失敗: {spResult.ResultMsg} ";
                SendMessageToMultipleChannel(myMsg);
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DatabaseBusyAndUpdateFail, new ErrorDetailModel(myMsg));
            }
        }

        private void NotifyToMultipleChannel(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_pgs_job_api,
                Message = msg
            });

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                TelegramChannelEnum = TelegramChannelEnum.pgs_job_api,
                Message = msg
            });
        }


        /// <summary>
        /// PlaceBet 後半段exception 失敗。某些情境會自動補救退款
        /// </summary>
        private void RefundWagerPlacebetException(OneTimesJobSettingEntity item)
        {
            var setting = item.SettingJson.JsonConvertToModel<OneTimesEazyJsonModel>();

            if (!setting.ReferGuid.HasValue)
            {
                return;
            }

            IBetRecordService betService = IOCHelper.Resolve<IBetRecordService>();
            betService.JobAutoRecoverPlaceBetExceptionRefundProcess(_defaultUserID, setting.ReferGuid.Value);
        }

    }
}
