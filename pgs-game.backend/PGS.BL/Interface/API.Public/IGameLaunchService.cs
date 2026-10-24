using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IGameLaunchService
    {
        GameLinkGenerateResultViewModel LaunchGameAndGetGamePageUrlByPlayerID(InternalTestBundleRequestModel model);

        GameLinkGenerateResultViewModel LaunchGameAndGetGamePageUrl(GameLaunchRequestModel model);

        GameLaunchViewModel LaunchGameCommonProcess(GameLaunchRequestModel model);

        LobbyGenerateLinkResultModel GenerateGameLinkForLobby(LobbyApiGenerateLinkParameterModel model);

        LobbyGenerateLinkResultModel GenerateGameLinkForIframe(LobbyApiGenerateLinkParameterModel model);

        GameServerViewModel GetGameServers(GameServerRequestModel model);

    }
}
