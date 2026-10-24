using target.BL.Interface;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Collections.Generic;
using System.Threading;

namespace target.BL.Services
{

    public class WalletCashService : BaseVendorService, IWalletCashService
    {
        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerCurrentServerRepository PlayerCurrentServerRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }


        public IPlayerCreateRefService PlayerCreateRefService { get; set; }


        private const decimal Min_Transfer_Amount = 0.01M;
        private const decimal Max_Transfer_Amount = 9999999999999;


        public WalletCashPlayerAmountViewModel GetPlayerWalletCash(WalletCashPlayerAmountRequestModel model)
        {
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            //檢查廠商token合法
            var merchantInfo = this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantIdFromCache(model.PlayerName, model.MerchantID, isLockQuery: false);

            if (queryPlayer == null)
            {
                return new WalletCashPlayerAmountViewModel
                {
                    Currency = merchantInfo.Currency,
                    Amount = 0
                };
            }

            HeaderValueHelper.AppendPlayerID(queryPlayer.PlayerID);

            var queryWallet = PlayerWalletRepository.GetPlayerWalletByType(queryPlayer.PlayerID, Models.Enums.WalletTypeEnum.GameWallet);

            if (queryWallet == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayerWallet, new ErrorDetailModel($"playerID: {queryPlayer.PlayerID}"));

            //這種是真的直接*100 因此要換算回去
            var convertRateSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer);

            if (convertRateSetting == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel(nameof(MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer)));

            decimal transferRate = convertRateSetting.SettingValueOfDecimal;

            //這邊不顯示gamePoint 避免商戶誤以為有多100倍的金幣可以提現
            var viewModel = new WalletCashPlayerAmountViewModel
            {
                Amount = (queryWallet.Amount / transferRate).FloorTwoPlaceDecimal(), //強制無條件捨去
                AmountWithConvert = queryWallet.Amount,
                Currency = queryPlayer.Currency,
                ConvertRate = transferRate
            };

            if (model.IsIncludeExtraInfo)
            {
                var query = PlayerCurrentServerRepository.GetByPlayerID(queryPlayer.PlayerID);
                viewModel.ExtraInfo = new WalletCashExtraInfoViewModel
                {
                    IsTransferLock = query != null,
                    PlayerName = queryPlayer.PlayerName,
                };
            }

            return viewModel;
        }


        public WalletTransferResultViewModel TransferCashToGameWallet(WalletTransferRequestModel model)
        {
            model.RuleFor(x => x.Amount).Rule_NotMinus();
            model.RuleFor(x => x.Amount).Rule_NotTooSmall(minValue: Min_Transfer_Amount);
            model.RuleFor(x => x.Amount).Rule_NotTooLarge(maxValue: Max_Transfer_Amount);
            model.RuleFor(x => x.Amount).Rule_IsDecimalPointPlacesTwo();
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();
            model.RuleFor(x => x.MerchantOrderNo).Rule_IsOrderNumberFormat();

            var queryMerchant = this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //根據商戶配置 檢查最低轉帳金額
            CheckMinTransferAmount(model.MerchantID, model.Amount);

            //單錢包禁止使用轉帳功能
            if (CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var queryPlayer = GerPlayerOrAutoCreateNewOne(queryMerchant, model.PlayerName);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            //遊戲房中 禁止轉帳
            CheckPlayerIsPlayingGame(queryPlayer.PlayerID);

            var creditModeSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode);

            decimal transferRate = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer).SettingValueOfDecimal;

            decimal amount_afterRateConvert = model.Amount * transferRate;

            var insertModel = new PlayerWalletTransferModel
            {
                Amount = amount_afterRateConvert,
                OrderNo_PGS = RandomHelper.GetOrderNo(""),
                OrderNo_Merchant = model.MerchantOrderNo,
                PlayerID = queryPlayer.PlayerID,
                TranferType = WalletTransferTypeEnum.Transfer_In,
                IsCreditMode = creditModeSetting.SettingValueOfBoolean,
                ConvertRate = transferRate
            };

            //呼叫SP 進行轉帳
            var result = PlayerWalletRepository.WalletTransfer(insertModel);

            if (!result.IsSuccess)
                throw new TerproException(result.ErrorCode , new ErrorDetailModel($"amount: {amount_afterRateConvert}"));

            return new WalletTransferResultViewModel
            {
                Amount = model.Amount,
                ConvertRate = transferRate,
                BeforeAmount = result.BeforeAmount,
                AfterAmount = result.AfterAmount,
                TransactionNo = insertModel.OrderNo_PGS
            };
        }


        public WalletTransferResultViewModel TransferCashToPlatformWallet(WalletTransferRequestModel model)
        {
            model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Amount).Rule_NotTooSmall(minValue: Min_Transfer_Amount);
            model.RuleFor(x => x.Amount).Rule_NotTooLarge(maxValue: Max_Transfer_Amount);
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();
            model.RuleFor(x => x.MerchantOrderNo).Rule_IsOrderNumberFormat();

            var queryMerchant = this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //根據商戶配置 檢查最低轉帳金額
            CheckMinTransferAmount(model.MerchantID, model.Amount, extraBufferRate: 10);

            //單錢包禁止使用轉帳功能
            if (CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var queryPlayer = GerPlayerOrAutoCreateNewOne(queryMerchant, model.PlayerName);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            //為了避免潛在風險 將錢轉出後 立刻登出三方遊戲
            VendorSessionTokenRepository.ResetPlayerAllSessionToken(queryPlayer.PlayerID);

            //遊戲房中 禁止轉帳
            CheckPlayerIsPlayingGame(queryPlayer.PlayerID);

            //強制剔除玩家(捕魚)
            ForceLogouATFishGame(queryPlayer.MerchantID, queryPlayer.PlayerID);

            //檢查商戶是否為 信用額度模式
            var creditModeSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode);

            var transferRate = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameVendor_ConvertRate_Transfer).SettingValueOfDecimal;

            decimal amount_afterRateConvert = model.Amount * transferRate;

            var insertModel = new PlayerWalletTransferModel
            {
                Amount = amount_afterRateConvert,
                OrderNo_PGS = RandomHelper.GetOrderNo(""),
                OrderNo_Merchant = model.MerchantOrderNo,
                PlayerID = queryPlayer.PlayerID,
                TranferType = WalletTransferTypeEnum.Transfer_Out,
                IsCreditMode = creditModeSetting.SettingValueOfBoolean,
                ConvertRate = transferRate
            };

            //呼叫SP 進行轉帳
            var result = PlayerWalletRepository.WalletTransfer(insertModel);

            if (!result.IsSuccess)
                throw new TerproException(result.ErrorCode);

            return new WalletTransferResultViewModel
            {
                Amount = model.Amount,
                ConvertRate = transferRate,
                BeforeAmount = result.BeforeAmount,
                AfterAmount = result.AfterAmount,
                TransactionNo = insertModel.OrderNo_PGS
            };
        }



        private void ForceLogouATFishGame(int merchantID, int playerID)
        {
            var enableATFishGames = MerchantGameSettingRepository.GetCountByFilter(new QueryMerchantGameSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameVendorIDs = new List<int>
                {
                    GameVendorTypeEnum.AT.ToInteger()
                }
            });

            if (enableATFishGames == 0)
                return;

            var queryLastToken = [REDACTED];

            var lastGameVendorID = AppHelper.GetGameVendorByGameID(queryLastToken.GameID);

            if (lastGameVendorID == (int)GameVendorTypeEnum.AT)
            {
                //強制剔除玩家(捕魚)
                ForceKickRoomByCallAPI(playerID, merchantID);

                Thread.Sleep(1000 * 1);
            }
        }



        private void ForceKickRoomByCallAPI(int playerID, int merchantID)
        {
            if (SettingHelper.IsMSTest)
                return;

            var ATIntegrationService = new ATIntegrationService();

            ATIntegrationService.ForceKickPlayer(new ATFishingForceKickReqeustParameter
            {
                PlayerID = playerID,
                MerchantID = merchantID
            });
        }


        private void CheckMinTransferAmount(int merchantID, decimal transferAmount, decimal extraBufferRate = 1)
        {
            //超過10元就不檢查了 減少cache存取消耗
            if (transferAmount > 10)
                return;

            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_MinTransferAmount);

            //extraBufferRate的用途: 轉出 會填10倍。轉出可以容忍更小的數字 避免玩家的錢轉不出
            if ((transferAmount * extraBufferRate) < querySetting.SettingValueOfDecimal)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooSmall, new ErrorDetailModel("Amount"));

            return;
        }


        /// <summary>
        /// 檢查用戶是否正在遊戲中
        /// </summary>
        private void CheckPlayerIsPlayingGame(int playerID)
        {
            var queryCurrentRoom = PlayerCurrentServerRepository.GetById(playerID);

            if (queryCurrentRoom != null)
            {
                throw new TerproException(ApiErrorCode.Forbidden_WhenPlayingGame_NotAllowTransfer , new ErrorDetailModel($"{playerID}"));
            }
        }

        private PlayerEntity GerPlayerOrAutoCreateNewOne(MerchantEntity queryMerchant, string playerName)
        {
            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantIdFromCache(playerName, queryMerchant.MerchantID , isLockQuery: false);

            if (queryPlayer == null)
            {
                //如果禁止商戶建帳號。會在此流程自動幫商戶建玩家帳號
                if (IsForbiddenToCreateAccountByVendorAPI(queryMerchant.MerchantID))
                {
                    int playerID = PlayerCreateRefService.CreatePlayerOrReturnPlayerID(new PlayerCreateParameterModel
                    {
                        Currency = queryMerchant.Currency,
                        MerchantID = queryMerchant.MerchantID,
                        PlayerName = playerName
                    });

                    queryPlayer = PlayerRepository.GetById(playerID);
                }
            }

            if (queryPlayer == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundPlayerPleaseVendorCreateFirst);
            }

            if (queryPlayer.Status != NormalActiveStatus.Enable.ToInteger())
            {
                throw new TerproException(ApiErrorCode.InvalidStatus_AccountStatus);
            }

            HeaderValueHelper.AppendPlayerID(queryPlayer.PlayerID);
            return queryPlayer;
        }


        private void ForceLogoutPgsRoomLock(int playerID)
        {
            //如果有開啟此設置
            //清除卡房、清除pgs token cache、清除pgs token資料 ....

        }


    }
}
