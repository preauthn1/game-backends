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
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Utility;
using game.Entity.Game;

namespace target.BL.Services
{

    public class PlayerWalletRefService : BaseGameService, IPlayerWalletRefService
    {
        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }


        public override bool IsAutoInstanceAllRepository
        {
            get
            {
                return false;
            }
        }

        public override bool IsAutoInstanceAllService
        {
            get
            {
                return false;
            }
        }


        public bool IsLockQuery = true;



        public PlayerWalletAmountBriefModel GetWalletAmountByMerchantWalletMode(int playerID, string sessionTokenPGS , bool isLockQuery = true)
        {
            base.InstanceAllRepositoryAndService(true, true);

            this.IsLockQuery = isLockQuery;

            var queryPlayer = GetValidPlayerAndMerchant(playerID);

            MerchantWalletModeEnum walletMode = queryPlayer.WalletMode;

            if (walletMode == MerchantWalletModeEnum.SingleWalletMode)
            {
                //如果 sessionTokenPGS 傳空值
                if (string.IsNullOrWhiteSpace(sessionTokenPGS))
                {
                    sessionTokenPGS = GetPlayerLatestSessionToken(playerID);
                }
            }

            var result = GetBalanceByLogicImplement(queryPlayer , sessionTokenPGS , walletMode);
            result.MerchantID = queryPlayer.MerchantID;

            return result;
        }


        public PlayerWalletAmountBriefModel GetWalletAmountByLatestToken(int playerID)
        {
            var result = GetWalletAmountByMerchantWalletMode(playerID, "");
            return result;
        }


        public PlayerWalletGamePointBriefModel GetWalletGamePointByLatestToken(int playerID , GameVendorTypeEnum gameVendor)
        {
            var result = GetWalletAmountByMerchantWalletMode(playerID, "");

            decimal convertRate = GetMerchantConvertRate(result.MerchantID, gameVendor).ConvertRate;

            decimal gamePoint = ConvertMoneyToGamePoint(result.MerchantID, result.Amount, convertRate);

            var convert = result.ReturnInjecteObject<PlayerWalletGamePointBriefModel>();
            convert.GamePoint = gamePoint;

            return convert;
        }


        private PlayerWalletAmountBriefModel GetBalanceByLogicImplement(PlayerMerchantInfoModel queryPlayer, string sessionTokenPGS , MerchantWalletModeEnum walletMode)
        {
            switch (walletMode)
            {
                case MerchantWalletModeEnum.TransferMode:
                    return GetTransferModePlayerWallet(queryPlayer.PlayerID);

                case MerchantWalletModeEnum.SingleWalletMode:
                    
                    //根據商戶設置又有分成標準型API或商戶客製API
                    var apiMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

                    var apiModeEnum = (SingleWalletPlatformApiLogicTypeEnum)apiMode.SettingValueOfInt;

                    switch (apiModeEnum)
                    {
                        case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                        case SingleWalletPlatformApiLogicTypeEnum.W88:
                            {
                                return GetBalance_W88Api(queryPlayer.PlayerID, sessionTokenPGS);
                            }


                        default:
                            {
                                return GetBalance_InterfaceCommonLogicSeamlessMode(queryPlayer.PlayerID, sessionTokenPGS, apiModeEnum);
                            }
                    }

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }


        private PlayerWalletAmountBriefModel GetBalance_InterfaceCommonLogicSeamlessMode(int playerID, string sessionTokenPGS , SingleWalletPlatformApiLogicTypeEnum type )
        {
            var myService = ServiceInstanceHelper.InstanceCallbackService(type);

            var apiResult = myService.GetWalletBalance(new SingleWalletModeWalletApiRequestParameter
            {
                PlayerID = playerID,
                SessionTokenPGS = sessionTokenPGS
            });

            if (apiResult == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);

            return new PlayerWalletAmountBriefModel
            {
                Amount = apiResult.Balance,
                WalletMode = MerchantWalletModeEnum.SingleWalletMode
            };
        }

        private PlayerWalletAmountBriefModel GetBalance_W88Api(int playerID, string sessionTokenPGS)
        {
            var service = ServiceInstanceHelper.InstanceW88Service();
            var apiResult = service.GetWalletBalance(new SingleWalletW88WalletApiRequestParameter
            {
                SessionTokenPGS = sessionTokenPGS,
                PlayerID = playerID
            });

            return new PlayerWalletAmountBriefModel
            {
                WalletMode = MerchantWalletModeEnum.SingleWalletMode,
                Amount = apiResult.Balance
            };
        }


        private string GetPlayerLatestSessionToken(int playerID)
        {
            string selectColumns = " SessionTokenPGS ";

            var queryLatest = SingleWalletMerchantSessionTokenRepository.GetPlayerLastSessionToken(playerID , selectColumns);

            if (queryLatest == null)
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);

            return queryLatest.SessionTokenPGS;
        }


        private PlayerWalletAmountBriefModel GetTransferModePlayerWallet(int playerID)
        {
            var queryWallet = PlayerWalletRepository.GetPlayerWalletByType(playerID, WalletTypeEnum.GameWallet , this.IsLockQuery);

            if (queryWallet == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayerWallet);

            return new PlayerWalletAmountBriefModel
            {
                Amount = queryWallet.Amount,
                WalletMode = MerchantWalletModeEnum.TransferMode
            };
        }


    }
}
