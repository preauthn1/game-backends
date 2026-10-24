using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models;
using target.Utility;
using target.Models.Enums;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper.Dependency;

namespace target.BL.Services
{

    public partial class MerchantTransferService : BaseEventService, IMerchantTransferService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public PlayerCurrentServerRepository PlayerCurrentServerRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }


        public IPlayerWalletRefService PlayerWalletRefService { get; set; }

        public IW88ExtraProcessService W88ExtraProcessService { get; set; }


        public ATIntegrationService ATIntegrationService = new ATIntegrationService();




        public MerchantWalletInfoViewModel GetWalletInfo(int playerID)
        {
            var query = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(playerID, "");

            var w88Balance = W88ExtraProcessService.GetW88Balance(playerID);

            return new MerchantWalletInfoViewModel
            {
                BalanceAmount = Math.Floor(query.Amount * 100) / 100, //無條件捨去 不然有時候會多0.5元
                PlatformBalanceAmount = w88Balance.Balance
            };
        }


        protected virtual int GetTransferTimesCount(int playerID , WalletTransferTypeEnum type)
        {
            var now = DateTimeHelper.GetCurrentTime();

            string cacheKey = CacheKey.Lobby_PlatformTransfer_Counter(playerID, type.ToInteger(), now);

            var counterValue = CacheHelper.GetCacheValue<int>(cacheKey);

            return counterValue;
        }


        public void CheckRequestTransferTooManyTimesOrForbidden(PlayerEntity player, WalletTransferTypeEnum type)
        {
            var now = DateTimeHelper.GetCurrentTime();

            var forbiddenTransferTag = SystemTagEnum.Player_LobbyTransferForbidden.ToInteger();

            var queryForbiddenTag = PlayerTagMappingRepository.GetPlayerTag(player.PlayerID, forbiddenTransferTag);

            if (queryForbiddenTag != null)
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);
            }

            string cacheKeyOfEazyLocker = CacheKey.EazyLocker_LockShortTerm_Player(player.PlayerID);

            if (CacheHelper.IsCacheKeyExist(cacheKeyOfEazyLocker))
            {
                throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes);
            }

            string cacheKey = CacheKey.Lobby_PlatformTransfer_Counter(player.PlayerID, type.ToInteger(), now);

            int counterValue = GetTransferTimesCount(player.PlayerID , type);

            //超過一定次數才開始檢查
            if (counterValue >= 2)
            {
                var targetSetting = MerchantCustomizedSettingKeyEnum.Common_LobbyTransferInMaxTimesPerHour;

                if (type == WalletTransferTypeEnum.Transfer_Out)
                {
                    targetSetting = MerchantCustomizedSettingKeyEnum.Common_LobbyTransferOutMaxTimesPerHour;
                }

                counterValue = counterValue + 1;

                var queryMerchantSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(player.MerchantID, targetSetting);

                int maxTimes = queryMerchantSetting.SettingValueOfInt;

                bool isOldPlayer = (now - player.CreateTime).TotalDays >= 45;

                //if (SettingHelper.IsProduction && isOldPlayer)
                //{
                //    maxTimes = maxTimes + 2; //老玩家再額外給予幾次額度
                //}

                if (counterValue > maxTimes)
                {
                    //老玩家會多給一次警告
                    if (isOldPlayer)
                    {
                        var queryWarningTag = PlayerTagMappingRepository.GetPlayerTag(player.PlayerID, SystemTagEnum.Player_SuspiciousWarning);

                        if (queryWarningTag == null)
                        {
                            CacheHelper.SetCacheValue(cacheKeyOfEazyLocker, "true", new TimeSpan(0, 55, 0));

                            PlayerTagMappingRepository.CreateTryCatch(new PlayerTagMappingEntity
                            {
                                PlayerID = player.PlayerID,
                                TagID = SystemTagEnum.Player_SuspiciousWarning.ToInteger(),
                                CreateTime = now,
                                CreateBy = "system"
                            });

                            PlayerTagMappingRepository.CreatePlayerBehaviorEvent(player.PlayerID, "捕魚轉帳次數過高。首次禁用");

                            PlayerTagMappingRepository.PlayerRiskScorePlus(player.PlayerID, 10.0M);
                        }
                        else
                        {
                            AddPlayerForbiddenTagAndException(player.PlayerID, counterValue);
                        }
                    }
                    else
                    {
                        AddPlayerForbiddenTagAndException(player.PlayerID, counterValue);
                    }

                    return;
                }
            }
            else
            {
                //後面成功 才會counter+1
            }
        }

        private void AddPlayerForbiddenTagAndException(int playerID, int counterValue)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            PlayerTagMappingRepository.CreatePlayerTag(playerID, SystemTagEnum.Player_LobbyTransferForbidden);
            
            PlayerTagMappingRepository.PlayerRiskScorePlus(playerID, 50.0M);

            PlayerTagMappingRepository.CreatePlayerBehaviorEvent(playerID, "捕魚轉帳次數過高。封禁(標記tag)");

            if(!SettingHelper.IsMSTest)
            {
                SendMessageToMultipleChannel($"玩家: {playerID} 操作次數超過: {counterValue} 已被標記禁止轉帳。請協助確認是否為正常玩家可以解鎖 ", TelegramChannelEnum.pgs_important_error);
            }

            throw new TerproException(ApiErrorCode.Forbidden_RequestTooManyTimes);
        }


        private void AddTransferTimesCounter(int playerID, WalletTransferTypeEnum type)
        {
            string cacheKey = CacheKey.Lobby_PlatformTransfer_Counter(playerID, type.ToInteger(), DateTimeHelper.GetCurrentTime());

            var existCacheValue = CacheHelper.GetCacheValue<int>(cacheKey);

            CacheHelper.SetCacheValue(cacheKey, ++existCacheValue, new TimeSpan(2, 0, 0));
        }



        private void ForceKickRoomWhenCashout(int playerID, int merchantID, WalletTransferTypeEnum type)
        {
            //轉入可以不檢查 轉出才檢查
            if (type == WalletTransferTypeEnum.Transfer_In)
                return;

            if (SettingHelper.IsMSTest)
                return;

            ATIntegrationService.ForceKickPlayer(new ATFishingForceKickReqeustParameter
            {
                PlayerID = playerID,
                MerchantID = merchantID
            });
        }


        /// <summary>
        /// 大廳的特殊轉帳 (轉入 & 轉出)
        /// </summary>
        public MerchantWalletTransferResultViewModel PgsWalletTransferByMerchantApi(int playerID, MerchantWalletTransferRequestModel model)
        {
            model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Amount).Rule_NotTooSmall(1);
            model.RuleFor(x => x.Amount).Rule_IsIntegerValue();

            var query = PlayerWalletRefService.GetWalletAmountByMerchantWalletMode(playerID, "");

            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            if (queryPlayer.Status != NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);

            var lobbyTransferSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.Common_LobbyEnablePlatformTransfer);

            if (!lobbyTransferSetting.SettingValueOfBoolean)
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);

            string pgsOrderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            //先確保token還有效
            var walletInfoBefore = GetWalletInfo(playerID);

            //檢查 VendorAPI設置維護
            CheckIsTransferAPIMaintain();

            //遊戲房中 禁止轉帳 (僅PGS遊戲適用)
            CheckPlayerIsPlayingGame(playerID);

            //檢查操作是否太過頻繁 & 封禁玩家
            CheckRequestTransferTooManyTimesOrForbidden(queryPlayer, model.Type);

            //轉出前會先剔除玩家
            ForceKickRoomWhenCashout(queryPlayer.PlayerID, queryPlayer.MerchantID, model.Type);

            //以我方有利的方式來處理 (先扣款/後扣款)
            if (model.Type == WalletTransferTypeEnum.Transfer_In)
            {
                // case. 平台錢包 轉至 遊戲錢包
                // step 1. 平台錢包先扣錢 
                DoW88TransgerLogicAndRetry(model.Type, playerID, model.Amount, pgsOrderNo);

                // step 2. pgs錢包再加錢
                DoPgsTranferLogic(queryPlayer.PlayerID, queryPlayer.MerchantID, model.Amount, model.Type, pgsOrderNo);
            }
            else
            {
                //轉出會有條件 限制最低轉帳金額 避免莫名其妙的無聊操作
                var queryWallet = PlayerWalletRepository.GetPlayerWalletByType(queryPlayer.PlayerID, WalletTypeEnum.GameWallet);

                decimal minValue = 10;

                //有些客戶再那邊操作轉 1元、2元轉出 不知道在衝三小
                if(queryWallet.Amount > minValue)
                {
                    model.RuleFor(x => x.Amount).Rule_NotTooSmall(minValue);
                }

                // case. 遊戲錢包 轉至 平台錢包
                // step 1. pgs錢包先扣錢
                DoPgsTranferLogic(queryPlayer.PlayerID, queryPlayer.MerchantID, model.Amount, model.Type, pgsOrderNo);

                // step 2. 平台錢包加錢
                DoW88TransgerLogicAndRetry(model.Type, playerID, model.Amount, pgsOrderNo);
            }

            //計數器+1
            AddTransferTimesCounter(playerID, model.Type);

            var walletInfoAgain = GetWalletInfo(playerID);
            return new MerchantWalletTransferResultViewModel
            {
                PgsOrderNo = pgsOrderNo,
                BalanceAmount = walletInfoAgain.BalanceAmount,
                PlatformBalanceAmount = walletInfoAgain.PlatformBalanceAmount,
            };
        }


        protected virtual AGGFishTransferApiDetailDataModel DoW88TransgerLogicAndRetry(WalletTransferTypeEnum type, int playerID, decimal amount, string pgsOrderNo)
        {
            AGGFishTransferApiDetailDataModel apiResult = null;

            var loopOption = new RetryLoopParameter
            {
                MaxTimes = 2,
                DelayMilliseconds = 1000,
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.Never, //callback那邊也會notify
                IsAsync = false,
            };

            AppHelper.ExecuteMethodAndRetryMaxTimes(loopOption, (param) =>
            {
                apiResult = W88ExtraProcessService.GameWalletTransfer(type, playerID, new AGGFishTransferApiRequestaModel
                {
                    Amount = amount,
                    TxtID = pgsOrderNo,
                });

                if (apiResult.PlatformOrderNo.IsNotEmptyTextNotNull())
                {
                    return true;
                }

                return false;
            });

            if (apiResult == null || apiResult.PlatformOrderNo.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.WalletTranferFail_ApiResponseFail);

            return apiResult;
        }


        private PgsTransferResultModel DoPgsTranferLogic(int playerID, int merchantID, decimal amount, WalletTransferTypeEnum transferType, string pgsOrderNo)
        {
            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            //單錢包禁止使用轉帳功能
            if (CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var creditModeSetting = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(merchantID, MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode, isReturnNull: false);

            var insertModel = new PlayerWalletTransferModel
            {
                Amount = amount,
                OrderNo_PGS = RandomHelper.GetOrderNo(""),
                OrderNo_Merchant = pgsOrderNo,
                PlayerID = playerID,
                TranferType = transferType,
                IsCreditMode = creditModeSetting.SettingValueOfBoolean,
                ConvertRate = 1
            };

            //呼叫SP 進行轉帳
            var result = PlayerWalletRepository.WalletTransfer(insertModel);

            if (!result.IsSuccess)
                throw new TerproException(result.ErrorCode);

            return new PgsTransferResultModel
            {
                PgsOrderNo = pgsOrderNo,
            };
        }

        private void CheckIsTransferAPIMaintain()
        {
            var queryVendorAPIMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsFishingTranferAPIMaintain);

            if (queryVendorAPIMaintain != null &&
               queryVendorAPIMaintain.SettingValueOfInt == SystemMaintainStatusEnum.SystemUuderMaintenance.ToInteger())
            {
                throw new TerproException(ApiErrorCode.SystemMaintain);
            }
        }


        /// <summary>
        /// 檢查用戶是否正在遊戲中
        /// </summary>
        private void CheckPlayerIsPlayingGame(int playerID)
        {
            bool isExist = PlayerCurrentServerRepository.IsExist(playerID);

            if (isExist)
            {
                throw new TerproException(ApiErrorCode.Forbidden_WhenPlayingGame_NotAllowTransfer);
            }
        }


    }
}
