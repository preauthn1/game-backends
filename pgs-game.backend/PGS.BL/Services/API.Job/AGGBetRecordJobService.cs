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

    public class AGGBetRecordJobService : BaseJobService, IAGGBetRecordJobService
    {
        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public IAGGInternalAPIService AGGInternalAPIService { get; set; }

        public IBetRecordService BetRecordService { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }


        private static object lockObject = new object();



        /// <summary>
        /// AGG異常單處理
        /// </summary>
        public string CleanUpAGGExceptionalWagers()
        {
            var queryAllIntegratorIDs = VendorConfigSettingRepository.GetAllProductionAggIntergratorIDs();

            if (queryAllIntegratorIDs.IsEmptyOrNull())
                return "not found";

            string resultMsg = string.Empty;
            string randomJobID = RandomHelper.GetYYYYMMDDHHMMSS_RandomOrder("Job_");

            foreach (var eachIntegrator in queryAllIntegratorIDs)
            {
                var queryWagers = AGGInternalAPIService.GetExceptionalRecords(new AGGGetExceptionalWagersReqeustParameter
                {
                    VendorConfigID = eachIntegrator.VendorConfigID,
                    DebugReferenceUUID = randomJobID
                });

                if (queryWagers.ResultObject.wagers.IsEmptyOrNull())
                    continue;

                List<string> wagerIDs = queryWagers.ResultObject.wagers.Select(z => z.wagerId).ToList();

                //查出pgs單號
                var queryBetRecords = BetRecordAGGRepository.GetByFilter(new QueryBetRecordGeneralRequestViewModel
                {
                    ListOfBetRecordNo = wagerIDs,
                    PartitionTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-5),
                    PartitionTimeEnd = DateTimeHelper.GetCurrentTime()
                });

                //標記tag
                List<string> recordNoPGS = queryBetRecords.Select(z => z.BetRecordNoPGS).ToList();
                BetRecordTagMappingRepository.CreateMultipleIfNotExist(recordNoPGS, SystemTagEnum.BetRecord_ExceptionalWagers);

                //觸發異常單處理
                AGGInternalAPIService.TriggerExceptionalWagerRetry(new AGGGetExceptionalWagersReqeustParameter
                {
                    DebugReferenceUUID = randomJobID,
                    VendorConfigID = eachIntegrator.VendorConfigID
                });

                resultMsg += $"{eachIntegrator.Value} 異常單:{string.Join(",", wagerIDs)}";
            }

            return resultMsg;
        }


        public string CheckPendingRecordAndTriggerSettle(QueryPendingRecordJobParameter param)
        {
            DateTime twNow = DateTimeHelper.GetCurrentTime(8);

            if(param.PreviousDays <= 0)
            {
                //離峰時段才會拉大時間範圍 (可能對DB負載有點影響)
                if(twNow.Hour >= 7 && twNow.Hour <= 9)
                {
                    param.PreviousDays = 2;
                }
                else
                {
                    param.PreviousDays = 0.5;
                }
            }

            if (param.GameVendor <= 0)
            {
                param.GameVendor = GameVendorTypeEnum.AGG.ToInteger();
            }

            var queryEnableFlag = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.JobSetting_AGGPendingWagerCheckEnable);

            if (queryEnableFlag != null && !queryEnableFlag.SettingValueOfBoolean)
                return "排程設置關閉中";

            string selectColumns = $" BetRecordNoPGS , CreateTime ";

            //ps. 只select幾個精簡欄位
            var queryPendingRecords = GetPendingBetRecords(param.PreviousDays, selectColumns);

            if (queryPendingRecords.IsEmptyOrNull())
                return "查無資料";

            SetSystemDefaultUserID();

            var targetTagEnum = SystemTagEnum.BetRecord_ForceSettlement_AndLogRetryTimes;
            int tagID = targetTagEnum.ToInteger();

            var queryRecordTag = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = queryPendingRecords.Select(z => z.BetRecordNoPGS).ToList(),
                TagIDs = tagID.ToListCollection()
            });

            List<Guid> referenceUUIDs = queryRecordTag
                .Where(x => x.ReferenceUUID.HasValue)
                .Select(x => x.ReferenceUUID.Value)
                .ToList();

            List<EazyTempValueEntity> counterValues = GetCounterValues(referenceUUIDs);

            foreach (var item in queryPendingRecords)
            {
                //API 請求對方重新發出結算通知 (然後externalAPI會再次收到請求)
                TriggerAGGApi(item.BetRecordNoPGS);

                var matchTagEntity = queryRecordTag.FirstOrDefault(z => z.BetRecordNoPGS == item.BetRecordNoPGS);

                //null代表排程第一次執行
                if (matchTagEntity == null)
                {
                    Guid uuid = Guid.NewGuid();

                    BetRecordTagMappingRepository.CreateOrQueueRetry(item.BetRecordNoPGS, targetTagEnum, uuid);

                    EazyTempValueRepository.CreateOrQueueRetry(new EazyTempValueEntity
                    {
                        UUID = uuid,
                        BetRecordNoPGS = item.BetRecordNoPGS,
                        Counter = 0
                    });
                }
                else
                {
                    int retryTimes = 0;

                    //過渡期 新版/舊版寫法 判斷方式不同
                    if (matchTagEntity.ReferenceUUID.HasValue)
                    {
                        var counterValue = counterValues.FirstOrDefault(z => z.UUID == matchTagEntity.ReferenceUUID);
                        retryTimes = counterValue.Counter;
                        EazyTempValueRepository.CounterPlus(matchTagEntity.ReferenceUUID.Value);
                    }

                    if (retryTimes >= 10 && retryTimes <= 11)
                    {
                        string msg = $"AGG投注單號 {item.BetRecordNoPGS} , 已Retry超過10次、可能需人工手動取消注單、或會自動進入cancel流程 ";
                        NotifyToMultipleJobChannel(msg);

                        //標註tag
                        BetRecordTagMappingRepository.CreateIfNotExist(item.BetRecordNoPGS, SystemTagEnum.BetRecord_ForceSettlement_OverThanManyTimes);
                    }

                    //確認是否要自動取消注單 (另一個排程會自動標記指定Tag)
                    AutoCancelBetOrNot(item.BetRecordNoPGS);
                }

                Thread.Sleep(100);
            }

            return $"共 {queryPendingRecords.Count()} 筆資料";
        }

        private List<EazyTempValueEntity> GetCounterValues(List<Guid> referenceUUIDs)
        {
            if (referenceUUIDs.IsEmptyOrNull())
                return new List<EazyTempValueEntity>();

            List<EazyTempValueEntity> queryCounterValue = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                UUIDs = referenceUUIDs
            });

            return queryCounterValue;
        }

        protected virtual List<BetRecordAGGEntity> GetPendingBetRecords(double previousDays, string selectColumns)
        {
            var now = DateTimeHelper.GetCurrentTime();

            DateTime maxCreateTime = now.AddMinutes(-1 * 20);

            double hoursRange = previousDays * 24;

            var filter = new QueryBetRecordGeneralRequestViewModel
            {
                PartitionTimeBegin = now.AddHours(-1 * hoursRange),
                PartitionTimeEnd = now,
                Status = BetRecordStatusEnum.Pending.ToInteger(),
                TimeoutSeconds = 90
            };

            try
            {
                var queryPendingRecords = BetRecordAGGRepository.GetByFilter(filter, selectColumns);

                //與其用SQL過濾。程式碼這邊過濾 可能負荷較小(我猜的)
                queryPendingRecords = queryPendingRecords
                    .Where(x => x.CreateTime < maxCreateTime)
                    .ToList();

                return queryPendingRecords;
            }
            catch (Exception ex)
            {
                string msg = ex.GetCommonNotifyMessage("GetPendingBetRecords");
                NotifyToMultipleJobChannel(msg);

                //第一次如果逾時失敗。嘗試縮短時間range再查詢一次 (如果再失敗就算了。期待下一輪次的排程不會發生timeout)
                filter.PartitionTimeBegin = DateTimeHelper.GetCurrentTime().AddHours(-1 * 3);
                var queryAgain = BetRecordAGGRepository.GetByFilter(filter, selectColumns);

                queryAgain = queryAgain
                    .Where(x => x.CreateTime < maxCreateTime)
                    .ToList();

                return queryAgain;
            }
        }


        public string ForceSingleRecordTriggerAGGSettle(string betRecordNoPGS)
        {
            string response = TriggerAGGApi(betRecordNoPGS);
            return response;
        }


        protected virtual string TriggerAGGApi(string betRecordNoPGS)
        {
            var slotsAPIService = ServiceInstanceHelper.InstanceSlotsGameVendorService(GameVendorTypeEnum.AGG);

            //API 請求對方重新發出結算通知 (然後externalAPI會再次收到請求)
            var result = slotsAPIService.TriggerSettlementRequestAgain(new AGGForceTriggerSettlementRequestReqeustParameter
            {
                BetRecordNoPGS = betRecordNoPGS,
                DebugReferenceUUID = betRecordNoPGS
            });

            string responseText = result.responseText;

            //如果API失敗 發個通知
            if (responseText.Contains("reason") || responseText.Contains("meta"))
            {
                string ignoreKeyword = "not finished left";
                string msg = $"TriggerAGGApi似乎失敗了{Environment.NewLine}PGS單號:`{betRecordNoPGS}` AGG單號:`{result.aggWagerID}` {Environment.NewLine}，API回應:{responseText}";
                NotifyToMultipleJobChannel(msg, ignoreKeyword);
            }
            else
            {

            }

            return responseText;
        }



        private void AutoCancelBetOrNot(string betRecordNoPGS)
        {
            var queryRequiredTags = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                BetRecordNoPGSList = betRecordNoPGS.ToListCollection(),
                TagIDs = SystemTagEnum.BetRecord_Cancel_ButNotFound.ToInteger().ToListCollection()
            });

            if (queryRequiredTags.IsEmptyOrNull())
                return;

            //避免重複執行 導致二次退款 lock處理
            lock (lockObject)
            {
                //執行取消注單流程
                BetRecordService.ManualRefundBetRecord(_defaultUserID, new BetRecordManualRefundRequestModel
                {
                    TransactionNo = betRecordNoPGS,
                    Memo = "排程自動取消注單",
                    ExtraTagID = SystemTagEnum.BetRecord_CancelByJob.ToInteger()
                });
            }
        }


    }
}
