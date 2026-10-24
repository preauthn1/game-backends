using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IMerchantSelfUpdateSettingService
    {
        MerchantSelfUpdateSettingViewModel GetMerchantAvailableSelfUpdateSettings(int userID, int merchantID);

        void UpdateMerchantLobbyVisibleGames(int userID, MerchantSelfUpdateSettingViewModel model);
    }
}
