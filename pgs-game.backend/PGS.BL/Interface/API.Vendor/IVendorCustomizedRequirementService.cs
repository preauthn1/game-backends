using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IVendorCustomizedRequirementService
    {
        QTechPlatformGameLaunchResponseModel GenerateLaunchLinkQTech(QTechPlatformGameLaunchRequestParameter model);

        QTechPlatformScreenReplayResponseModel GenerateScreenshotReplayLinkQTech(QTechPlatformScreenReplayRequestParameter model);

        OPSPlatformGameLaunchResponseModel GenerateLaunchLinkOPS(OPSPlatformGameLaunchRequestParameter model);

        OPSPlatformScreenReplayResponseModel GenerateScreenshotReplayLinkOPS(OPSPlatformScreenReplayRequestParameter model);

        THubPlatformGameLaunchResponseModel GenerateLaunchLinkTHub(THubPlatformGameLaunchRequestParameter model);

        THubPlatformGetGameListResponseModel GetGameListTHub(THubPlatformGetGameListRequestParameter model);

        THubPlatformGetGameReplayScreenshotResponseModel GetGameReplayScreenshotThub(THubPlatformGetGameReplayScreenshotRequestParameter model);

        NotixGameLaunchResultViewModel GenerateLaunchLinkNotix(NotixGameLaunchRequestParameter model , NotixGameLaunchExtraParameter extra);

        NotixGameListResultViewModel GetGameListNotix(NotixGameListRequestParameter model);

        NotixScreenshotAPIResultViewModel GenerateScreenshotReplayLinkNotix(NotixScreenshotAPIRequestParameter model, NotixGameLaunchExtraParameter extraConfig);

    }
}
