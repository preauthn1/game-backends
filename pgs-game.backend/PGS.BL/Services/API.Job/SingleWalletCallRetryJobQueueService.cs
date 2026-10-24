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
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class SingleWalletCallRetryJobQueueService : BaseService, ISingleWalletCallRetryJobQueueService
    {
        public BetRecordMerchantNotifyQueueRepository BetRecordMerchantNotifyQueueRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public ISingleWalletPlatformApiRequestRefService SingleWalletPlatformApiRequestRefService { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletMerchantBetProcessRefService { get; set; }

        private const int RETRY_MAX_TIMES = 7;


        /// <summary>
        /// 單錢包模式 補發通知的排程
        /// </summary>
        public void BetReocrdNotifyQueueRetryProcess()
        {
            TriggerNotifyQueueRetry();
        }

        public void BetReocrdNotifyQueueRetryProcess_SpecificOrderNo(string specificBetRecordNoPGS)
        {
            TriggerNotifyQueueRetry(specificBetRecordNoPGS);
        }

        private void TriggerNotifyQueueRetry(string specificBetRecordNoPGS = "")
        {
            var myFilter = new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                IsMustNotify = true,
            };

            //若指定單一單號 強制retry 無視預計執行時間
            if(specificBetRecordNoPGS.IsNotEmptyTextNotNull())
            {
                myFilter.IsMustNotify = false;
                myFilter.BetRecordNoPGS = specificBetRecordNoPGS;
            }

            //取得發送時間小於現在且...
            var queryQueue = BetRecordMerchantNotifyQueueRepository.GetByFilter(myFilter);

            if (queryQueue.IsEmptyOrNull())
                return;

            //先更新下一次排程時間
            foreach (var item in queryQueue)
            {
                DateTime newNextTime = CalculateNextTime(item.NextNotifyTime, item.RetryTimes);
                BetRecordMerchantNotifyQueueRepository.UpdateNextTime(item.Id, newNextTime);
            }

            foreach (var item in queryQueue)
            {
                //採用非同步處理 避免某一家回應太慢卡住其他queue
                Task.Factory.StartNew(() =>
                {
                    //觸發指定的API
                    TriggerMerchantApiByType(item);
                });
            }
        }



        public CRUDResultModel ConvertEazyTempValueToNotifyQueue(EazyTempValueConvertNotifyQueueRequestModel model)
        {
            model.RuleFor(x => x.TransactionNoList).Rule_NotEmptyNull();

            if (model.NotifyType <= 0)
            {
                model.NotifyType = BetRecordMerchantNotifyQueueTypeEnum.Cancel.ToInteger();
            }

            var query = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                ListOfBetRecordNoPGS = model.TransactionNoList
            });

            if (query.IsEmptyOrNull())
            {
                return new CRUDResultModel("查無資料");
            }

            var queryExistData = BetRecordMerchantNotifyQueueRepository.GetByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                ListOfBetRecordNoPGS = model.TransactionNoList
            })
                .Select(x => x.BetRecordNoPGS)
                .ToList();

            query = query.Where(x => !queryExistData.Contains(x.BetRecordNoPGS)).ToList();

            var now = DateTimeHelper.GetCurrentTime();

            var listOfInsert = query.Select(item => new BetRecordMerchantNotifyQueueEntity
            {
                BetRecordNoPGS = item.BetRecordNoPGS,
                GameID = item.GameID.Value,
                PlayerID = item.PlayerID.Value,
                MerchantID = item.MerchantID.Value,
                Type = model.NotifyType,
                RetryTimes = 1,
                IsSuccess = false,
                CreateTime = now,
                NextNotifyTime = now
            })
                .ToList();

            BetRecordMerchantNotifyQueueRepository.CreateMany(listOfInsert);
            return new CRUDResultModel($"共 {listOfInsert.Count()} 筆資料被轉換");
        }


        public void TriggerRetryByID(long id)
        {
            var queryQueue = BetRecordMerchantNotifyQueueRepository.GetById(id);

            if (queryQueue == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            if (queryQueue.IsSuccess)
                throw new TerproException(ApiErrorCode.InvalidStatus);

            //先更新下次時間
            DateTime newNextTime = CalculateNextTime(queryQueue.NextNotifyTime, queryQueue.RetryTimes);
            BetRecordMerchantNotifyQueueRepository.UpdateNextTime(queryQueue.Id, newNextTime);

            //觸發指定的API
            TriggerMerchantApiByType(queryQueue);
        }


        private void TriggerMerchantApiByType(BetRecordMerchantNotifyQueueEntity item)
        {
            var type = (BetRecordMerchantNotifyQueueTypeEnum)item.Type;
            switch (type)
            {
                case BetRecordMerchantNotifyQueueTypeEnum.Cancel:
                    CancelProcess(item);
                    break;

                case BetRecordMerchantNotifyQueueTypeEnum.Settle:
                    SettleProcess(item);
                    break;

                case BetRecordMerchantNotifyQueueTypeEnum.BonusSettle:
                    BonusSettleProcess(item);
                    break;

                case BetRecordMerchantNotifyQueueTypeEnum.SettleChange:
                    ChangeSettleInfo(item);
                    break;

                case BetRecordMerchantNotifyQueueTypeEnum.SendEventBonus:
                    SendEventBonus(item);
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }


        private void SettleProcess(BetRecordMerchantNotifyQueueEntity item)
        {
            try
            {
                var callbackParam = InstanceSettleCallbackParameters(item);

                SingleWalletMerchantBetProcessRefService.PayoutProcessWithDifferentApiLogic(callbackParam);

                BetRecordMerchantNotifyQueueRepository.UpdateToSuccess(item.Id);
            }
            catch (Exception ex)
            {

            }
        }


        public SingleWalletSettleCallbackParameter InstanceSettleCallbackParameters(BetRecordMerchantNotifyQueueEntity item)
        {
            var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(item.BetRecordNoPGS);

            if (queryMapping == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var service = ServiceInstanceHelper.InstanceVendorService(queryMapping.GameVendorID);

            var queryBetReocrd = service.GetByPGSTransactionNo(item.BetRecordNoPGS);

            var extraInfo = InstanceCallbackExtraInfo(queryBetReocrd);

            var callbackParam = DataModelConvertHelper.InstanceSingleWalletSettleCallbackParameter(queryBetReocrd, item.LastRequestUUID, extraInfo);

            return callbackParam;
        }


        private SettleBetCallbackExtraInfo InstanceCallbackExtraInfo(BetRecordGeneralColumnModel queryBetReocrd)
        {
            var extraInfo = new SettleBetCallbackExtraInfo
            {
                RoundID = queryBetReocrd.ExtraInfo1,
            };

            int myGameVendor = AppHelper.GetGameVendorByGameID(queryBetReocrd.GameID);

            bool isExtraSearchGameVendor = myGameVendor == (int)GameVendorTypeEnum.AGG;

            //只有特定遊戲商才會做額外查詢 (有一些查詢成本)
            if (!isExtraSearchGameVendor)
                return extraInfo;

            if (!queryBetReocrd.SettlementTime.HasValue)
                return extraInfo;

            DateTime gameTime = queryBetReocrd.SettlementTime.Value;

            var queryPerGame = PerGameDetailLogRepository.GetByTransactionNoAndGameTime(queryBetReocrd.BetRecordNoPGS, gameTime);

            //查不到就算了 (偷機做法)
            if (queryPerGame != null)
            {
                var queryTags = PerGameDetailLogTagMappingRepository.GetByFilter(new QueryPerGameDetailLogTagMappingRequestViewModel
                {
                    PerGameDetailLogIDs = queryPerGame.Id.ToListCollection(),
                    GameTimeBegin = gameTime.AddMinutes(5 * -1),
                    GameTimeEnd = gameTime.AddMinutes(5)
                });

                var perGameTags = queryTags.Select(z => (SystemTagEnum)z.TagID).ToList();
                DataModelConvertHelper.SetExtraValueByPerGameTags(extraInfo, perGameTags);
            }

            return extraInfo;
        }


        private void BonusSettleProcess(BetRecordMerchantNotifyQueueEntity item)
        {
            try
            {
                var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(item.BetRecordNoPGS);

                if (queryMapping == null)
                    throw new TerproException(ApiErrorCode.NotFoundRecord);

                //超過合理次數、標記tag
                if (item.RetryTimes >= (RETRY_MAX_TIMES - 1))
                {
                    BetRecordTagMappingRepository.CreateIfNotExist(item.BetRecordNoPGS, SystemTagEnum.BetRecord_SingleWallet_SettleNotifyTooManyTimes);
                }

                var service = ServiceInstanceHelper.InstanceVendorService(queryMapping.GameVendorID);

                var queryBetReocrd = service.GetByPGSTransactionNo(item.BetRecordNoPGS);

                SingleWalletPlatformApiRequestRefService.BonusSettlement(new SingleWalletSettleCallbackParameter
                {
                    PlayerID = queryBetReocrd.PlayerID,
                    TaxAmount = queryBetReocrd.TaxAmount,
                    SettlementTime = queryBetReocrd.SettlementTime,
                    BetRecordNoPGS = queryBetReocrd.BetRecordNoPGS,
                    BetAmount = queryBetReocrd.BetAmount,
                    GameID = queryBetReocrd.GameID,
                    MerchantID = queryBetReocrd.MerchantID,
                    PayoutAmount = queryBetReocrd.PayoutAmount,
                    ValidAmount = queryBetReocrd.ValidAmount,
                    DebugProcessUUID = item.LastRequestUUID,
                });

                BetRecordMerchantNotifyQueueRepository.UpdateToSuccess(item.Id);
            }
            catch (Exception ex)
            {

            }
        }


        private void CancelProcess(BetRecordMerchantNotifyQueueEntity item)
        {
            try
            {
                var param = new SingleWalletModeCancelApiParameter
                {
                    PlayerID = item.PlayerID,
                    GameID = item.GameID,
                    DebugProcessUUID = item.LastRequestUUID,
                    MerchantID = item.MerchantID,
                    TransactionNo = item.BetRecordNoPGS,
                };

                //呼叫對方API & 更新狀態(如果成功的話)
                SingleWalletMerchantBetProcessRefService.CancelProcessWithDifferentApiLogic(param);
                BetRecordMerchantNotifyQueueRepository.UpdateToSuccess(item.Id);
            }
            catch (Exception ex)
            {

            }
        }


        private void ChangeSettleInfo(BetRecordMerchantNotifyQueueEntity item)
        {
            try
            {
                SingleWalletPlatformApiRequestRefService.ChangeSettleInfo(new SingleWalletChangeSettleInfoApiParameter
                {
                    DebugProcessUUID = item.LastRequestUUID,
                    TransactionNo = item.BetRecordNoPGS
                });

                BetRecordMerchantNotifyQueueRepository.UpdateToSuccess(item.Id);
            }
            catch (Exception ex)
            {

            }
        }

        private void SendEventBonus(BetRecordMerchantNotifyQueueEntity item)
        {
            try
            {
                //如果需要手動補救 部分資訊可以參考 vendorAPI event/prize-records 回應的內容
                var eventBonusExtraInfo = item.DebugInfo.JsonConvertToModel<EventBonusCallbackDebugInfoTempModel>();

                if (eventBonusExtraInfo == null)
                {
                    SendMessageToMultipleChannel("SendEventBonus 解析物件失敗", TelegramChannelEnum.pgs_job_api);
                }

                var queryPrizeRecord = EventPrizeRecordRepository.GetJoinDetailInfoByID(eventBonusExtraInfo.EventPrizeRecordID);

                //prize詳細資訊是在這個物件中
                var eventInfoModel = DataModelAndDbValueConvertHelper.ConvertEventPrizeRecordDetailToViewModel(queryPrizeRecord);
                eventInfoModel.EventName = eventBonusExtraInfo.EventName;

                SingleWalletPlatformApiRequestRefService.SendEventPrize(new SingleWalletCallbackSendEventBonusParameter
                {
                    Amount = queryPrizeRecord.UpdateAmount.Value,
                    MerchantID = item.MerchantID,
                    PlayerID = item.PlayerID,
                    TransactionNo = item.BetRecordNoPGS,
                    EventPrizeInfo = eventInfoModel,
                });

                BetRecordMerchantNotifyQueueRepository.UpdateToSuccess(item.Id);
            }
            catch (Exception ex)
            {

            }
        }


        private DateTime CalculateNextTime(DateTime originalNextTime, int retryTimes)
        {
            retryTimes = retryTimes + 1;

            //超過N次不再處理
            if (retryTimes >= RETRY_MAX_TIMES)
            {
                return new DateTime(2999, 1, 1);
            }

            //3的N次方 3 9 27 81 243 
            double extendMinutes = Math.Pow(3, (retryTimes));
            return originalNextTime.AddMinutes(extendMinutes);
        }


        private void SendNotifyToChannel(string msg)
        {
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_job_api
            });
        }

        private SingleWalletPlatformApiLogicTypeEnum GetMerchantApiType(int merchantID)
        {
            var apiTypeSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);
            return (SingleWalletPlatformApiLogicTypeEnum)apiTypeSetting.SettingValueOfInt;
        }

    }
}
