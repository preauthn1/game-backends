using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IGameRTPConfigService
    {
        PagedList<GameRTPConfigViewModel> GetGameRTPByPagingFilter(int userID, QueryGameRTPConfigRequestParameterModel filter);

        PagedList<PlayerRTPConfigViewModel> GetPlayerRTPByPagingFilter(int userID, QueryPlayerRTPConfigRequestParameterModel filter);

        void BatchUpdatePlayerRTP(int userID, UpdatePlayerRTPRequestModel model);

        void UpdateGameRTP(int userID, GameVendorRTPUpdateRequestModel model);

        void UpdateGameWinLimit(int userID, GameVendorRTPUpdateRequestModel model);

        void UpdatePlayerRTP(int userID, UpdatePlayerRTPRequestModel model);

        void SyncAGGAllRTPSettings(int userID , LazyCommonRequestModel model);

        void SyncRTPSettingByApi(int userID, GameVendorRTPSyncRequestModel model);

        void ResetPlayerRTPByTargetValue(int userID, ResetAllPlayerRTPRequestModel model);

        string FetchGameOptionRawResponse(int userID, GameVendorGameOptionInfoRequestModel model);
    }
}
