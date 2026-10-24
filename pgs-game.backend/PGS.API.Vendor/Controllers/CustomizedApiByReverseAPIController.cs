using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.API.AdminSys;
using target.Models.Models;
using target.Utility.Extensions;
using target.Models.Enums;

namespace target.API.Vendor.Controllers
{
    public class CustomizedApiByReverseAPIController : BaseController
    {
     
        public IVendorCustomizedRequirementService VendorCustomizedRequirementService { get; set; }


        /// <summary>
        /// QTech 商戶客製的API (啟動遊戲)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("v1/qtech/game-launch")]
        [HttpPost]
        public QTechPlatformGameLaunchResponseModel QTech_GetGameLaunchUrl([FromBody] QTechPlatformGameLaunchRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GenerateLaunchLinkQTech(model);
            return result;
        }


        /// <summary>
        /// QTech 商戶客製的API (回放)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("v1/qtech/game-result")]
        [HttpPost]
        public QTechPlatformScreenReplayResponseModel QTech_GetScreenshotReplayUrl([FromBody] QTechPlatformScreenReplayRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GenerateScreenshotReplayLinkQTech(model);
            return result;
        }


        /// <summary>
        /// THub 商戶客製的API (啟動遊戲)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("v1/THub/start")]
        [HttpPost]
        public THubPlatformGameLaunchResponseModel THub_GetGameLaunchUrl([FromBody] THubPlatformGameLaunchRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GenerateLaunchLinkTHub(model);
            return result;
        }

        /// <summary>
        /// THub 商戶客製的API (遊戲清單)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("v1/THub/gamelist")]
        [HttpPost]
        public THubPlatformGetGameListResponseModel THub_GetGameList([FromBody] THubPlatformGetGameListRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GetGameListTHub(model);
            return result;
        }

        /// <summary>
        /// THub 商戶客製的API (遊戲結果回放)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI)]
        [Route("v1/THub/getgameresult")]
        [HttpPost]
        public THubPlatformGetGameReplayScreenshotResponseModel THub_GetGameReplayScreenshot([FromBody] THubPlatformGetGameReplayScreenshotRequestParameter model)
        {
            var result = VendorCustomizedRequirementService.GetGameReplayScreenshotThub(model);
            return result;
        }


        /// <summary>
        /// Notix 商戶客製的API (啟動遊戲)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI, SubCategory = ApiConfigSubCategoryEnum.ReverseAPI_Notix)]
        [Route("Notix/{providerID}/api/v1/reverse/games/start")]
        [HttpPost]
        public NotixGameLaunchResultViewModel Notix_GetGameLaunchUrl([FromBody] NotixGameLaunchRequestParameter model, string providerID)
        {
            var extraConfig = new NotixGameLaunchExtraParameter()
            {
                ProviderID = providerID
            };

            var result = VendorCustomizedRequirementService.GenerateLaunchLinkNotix(model, extraConfig);
            return result;
        }

        /// <summary>
        /// Notix 商戶客製的API (demo模式啟動遊戲)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI, SubCategory = ApiConfigSubCategoryEnum.ReverseAPI_Notix)]
        [Route("Notix/{providerID}/api/v1/reverse/games/demo-start")]
        [HttpPost]
        public NotixGameLaunchResultViewModel Notix_DemoMode_GetGameLaunchUrl([FromBody] NotixGameLaunchRequestParameter model, string providerID)
        {
            var extraConfig = new NotixGameLaunchExtraParameter()
            {
                ProviderID = providerID,
                IsTrialMode = true,
            };

            var result = VendorCustomizedRequirementService.GenerateLaunchLinkNotix(model , extraConfig);
            return result;
        }


        /// <summary>
        /// Notix 商戶客製的API (遊戲清單)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI , SubCategory = ApiConfigSubCategoryEnum.ReverseAPI_Notix)]
        [Route("Notix/{providerID}/api/v1/reverse/games")]
        [HttpGet]
        public NotixGameListResultViewModel Notix_GetGameList([FromQuery] NotixGameListRequestParameter model , string providerID)
        {
            model = model ?? new NotixGameListRequestParameter();
            model.ProviderCode = providerID;

            var result = VendorCustomizedRequirementService.GetGameListNotix(model);
            return result;
        }

        /// <summary>
        /// Notix 商戶客製的API (Screenshot遊戲結果快照)
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.VendorPlaformCustomizedAPI, SubCategory = ApiConfigSubCategoryEnum.ReverseAPI_Notix)]
        [Route("Notix/{providerID}/api/v1/reverse/games/reports/round-url")]
        [HttpPost]
        public NotixScreenshotAPIResultViewModel Notix_GetWagerDetailScreenshot([FromBody] NotixScreenshotAPIRequestParameter model, string providerID)
        {
            var extraConfig = new NotixGameLaunchExtraParameter()
            {
                ProviderID = providerID,
            };

            var result = VendorCustomizedRequirementService.GenerateScreenshotReplayLinkNotix(model , extraConfig);
            return result;
        }



    }
}
