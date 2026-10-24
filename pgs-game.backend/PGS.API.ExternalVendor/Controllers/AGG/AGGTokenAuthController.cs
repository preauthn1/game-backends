using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Web.Modules.Filter;
using target.BL.Services;
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using target.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;

namespace target.API.ExternalVendor.Controllers
{

    public partial class AGGBetController : BaseGameVendorController
    {

        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, IsHeaderLogEnable = false , IsWriteToOtherLogTable = true)]
        [Route("agg/user/authenticate")]
        [Route("agg/supplier/v2/user/authorize")]
        [HttpPost]
        public AGGSingleWalletAPICheckTokenResponseDataModel CheckTokenIsValid([FromBody] AGGSingleWalletCheckTokenRequestModel model)
        {
            // step 1. 先檢查session是否合法
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                SessionToken = [REDACTED],
            });

            return new AGGSingleWalletAPICheckTokenResponseDataModel
            {
                RequestId = model.RequestId,
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                UserId = walletInfo.PlayerID,               
                AccountingHierarchies = GenerateAGGHierarchy_2025V2(tokenResult.HierarchyMerchantIDs),
                Namespace = SetNamespaceOrNull(tokenResult.MerchantID),                
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                {
                    ConvertToBalanceCodeModel(walletInfo.BalanceGamePoint , walletInfo.Currency)
                }
            };
        }


        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, IsHeaderLogEnable = false, IsWriteToOtherLogTable = true)]
        [Route("agg/account/token")]
        [HttpPost]
        public AGGSingleWalletGenerateGameTokenResponseViewModel GenerateGameToken([FromBody] AGGSingleWalletGenerateGameTokenRequestModel model)
        {
            // step 1. 先檢查session是否合法
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            // step 2. 取得gameID
            var queryGame = GameVendorIntegrationService.GetGameByThirdGameCode(model.GameId);

            // step 3. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                SessionToken = [REDACTED],
            });

            //有點鳥的作法 因為不想再開新table，就直接用 gameID + 我方的token 當作對方的game_token
            //2024/04 待確認。為何要區別game? 是因為對方有限制不同game 要有不同token?
            string gameToken = $"{queryGame?.GameID}__{model.UserToken}";

            var viewModel = new AGGSingleWalletGenerateGameTokenResponseViewModel
            {
                AccountToken = [REDACTED],
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                {
                    ConvertToBalanceCodeModel(walletInfo.BalanceGamePoint , walletInfo.Currency)
                }
            };

            viewModel.AccountingHierarchies = GenerateAGGHierarchy(tokenResult.HierarchyMerchantIDs);
            viewModel.Namespace = SetNamespaceOrNull(tokenResult.MerchantID);

            return viewModel;
        }



        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, IsHeaderLogEnable = false, IsWriteToOtherLogTable = true)]
        [Route("agg/account/balance")]
        [Route("agg/supplier/v2/user/balance")]
        [HttpPost]
        public AGGSingleWalletAPIGetBalanceResponseDataModel GetPlayerWallet([FromBody] AGGSingleWalletAPIGetBalanceRequestModel model)
        {
            //查log的時候比較好識別
            HeaderValueHelper.AppendDebugInfo("Balance");

            //取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = model.UserID
            });

            return new AGGSingleWalletAPIGetBalanceResponseDataModel
            {
                RequestId = model.RequestId,
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                {
                    ConvertToBalanceCodeModel(walletInfo.BalanceGamePoint , walletInfo.Currency)
                },
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse
            };
        }


        private string SetNamespaceOrNull(int merchantID)
        {
            //2025/04 namespace機制已取消
            return null;
        }


        private List<string> GenerateAGGHierarchy(List<int> hierarchyMerchantIDs)
        {
            List<string> myHierarchy = new List<string>();

            //預設第一層都會叫做 default
            myHierarchy = new List<string>
                    {
                        SystemConstant.AGG_RootAgentName
                    };

            if (hierarchyMerchantIDs.IsNotEmptyNull())
            {
                foreach (var merchantID in hierarchyMerchantIDs)
                {
                    myHierarchy.Add(merchantID.ToString());
                }
            }

            return myHierarchy;
        }

        private List<string> GenerateAGGHierarchy_2025V2(List<int> hierarchyMerchantIDs)
        {
            List<string> myHierarchy = new List<string>();
          
            if (hierarchyMerchantIDs.IsNotEmptyNull())
            {
                foreach (var merchantID in hierarchyMerchantIDs)
                {
                    myHierarchy.Add(merchantID.ToString());
                }
            }

            return myHierarchy;
        }



    }
}
