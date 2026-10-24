using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IPlayerRTPChangeRefService
    {
        PlayerRTPChangeResultModel WhenEnterGameChangeRTP(PlayerRTPChangeWhenEnterGameParameter model);

        void ResetPlayerAllGameRTP(PlayerRTPResetAllGameRequestModel model);

        void ResetPlayerGameRTP(PlayerRTPResetRequestModel model);

        void UpdatePlayerGameRTP(PlayerRTPChangeRequestModel model);
    }
}
