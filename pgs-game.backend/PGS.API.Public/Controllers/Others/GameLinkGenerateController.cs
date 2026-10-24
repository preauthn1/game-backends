using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.API.Public.Controllers
{
    public class GameLinkGenerateController : BaseController
    {
        public IGameLaunchService GameLaunchService { get; set; }


        public static List<string> _availableTestAccounts = new List<string>
        {
            "test_001",
            "test_002",
            "test_003",
            "test_004",
            "test_005",
            "test_006",
            "test_007",
            "test_008",
            "test_009",
        };

   
        /// <summary>
        /// 透過GUID 產生遊戲連結
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("vendor-test/launch/{merchantGuid}/{playerName}")]
        [HttpGet]
        public IActionResult LaunchLobbyUrlFroVendorTeam(string playerName, string merchantGuid, [FromQuery] GameLaunchRequestModel model)
        {
            if(SettingHelper.IsDisablePGSLobby && model.GameID == 0)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, "【BWG系統】無提供大廳。若需要測試，請從後台進入遊戲");
            }

            var para = new
            {
                playerName = playerName,
                merchantGuid = merchantGuid
            };

            para.RuleFor(z => z.playerName).Rule_NotEmptyText();
            para.RuleFor(z => z.merchantGuid).Rule_IsValidGuid();

            if (!_availableTestAccounts.Contains(playerName))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("playerName"));
            }

            var MerchantRepository = new MerchantRepository();

            var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantGuid = merchantGuid
            })
            .FirstOrDefault();

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (queryMerchant.Type == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny , new ErrorDetailModel("MerchantType"));

            var gameVendor = AppHelper.GetGameVendorByGameID(model.GameID);

            var launchPara = model ?? new GameLaunchRequestModel();

            launchPara.Lang = launchPara.Lang.SetDefaultValueWhenTextIsEmptyNull("zh-tw");
            launchPara.MerchantID = queryMerchant.MerchantID;
            launchPara.PlayerName = playerName;
            launchPara.IsFromIframe = model.GameID > 0 ? true : false;
            launchPara.SessionToken = [REDACTED];

            if(gameVendor == GameVendorTypeEnum.AGGFish.ToInteger())
            {
                launchPara.ExtraLink = launchPara.ExtraLink.SetDefaultValueWhenTextIsEmptyNull("https://example.invalid/login/index.htm");
            }

            var url = GameLaunchService.LaunchGameAndGetGamePageUrl(launchPara);

            if (string.IsNullOrWhiteSpace(url.Link))
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("url generate fail"));

            return Redirect(url.Link);
        }


        /// <summary>
        /// 提供廠商加值玩家錢包
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("vendor-test/wallet-deposit/{merchantGuid}/{playerName}")]
        [HttpGet]
        public object DepositWalletForPartnerVendorTeam(string playerName, string merchantGuid, decimal amount = 5000)
        {
            if (SettingHelper.IsProduction)
            {
                throw new TerproException(ApiErrorCode.Forbidden_DoNotAllowWhenProduction);
            }

            decimal maxAmount = 1000000;

            if (amount >= maxAmount)
            {
                amount = maxAmount;
            }

            var para = new
            {
                playerName = playerName,
                merchantGuid = merchantGuid
            };

            para.RuleFor(z => z.playerName).Rule_NotEmptyText();
            para.RuleFor(z => z.merchantGuid).Rule_IsValidGuid();

            if (!_availableTestAccounts.Contains(playerName))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("playerName"));
            }

            var PlayerRepository = new PlayerRepository();
            var MerchantRepository = new MerchantRepository();
            var PlayerWalletRepository = new PlayerWalletRepository();

            var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantGuid = merchantGuid
            })
            .FirstOrDefault();

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (queryMerchant.Type == MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(playerName, queryMerchant.MerchantID);

            var spResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPlayer.PlayerID, 0, amount, TransactionTypeEnum.ManualDepositWalletAmount, "", "0.0.0.0");

            return new
            {
                BeforeAmount = spResult.BeforeAmount,
                AfterAmount = spResult.AfterAmount
            };
        }



    }
}
