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

namespace target.BL.Interface
{

    public interface IMerchantGameSettingService
    {

        MerchantGameSettingViewModel GetGameSettingByMerchantID(int merchantID);

        void Update(int userID , int merchantID, MerchantGameSettingUpdateRequestModel model);

        void CopySettingToChilds(int userID, int parentMerchantID);

        void CopySetting(int userID, MerchantGameSettingCopyRequestModel model);

        CRUDResultModel MultipleMerchantAdjustGameSetting(int userID, MultipleMerchantAdjustGameSettinRequestModel model);

        MerchantGameSettingAgentSelfEditViewModel GetGameSettingByMerchantIDForAgentSelfEdit(int userID, int merchantID);

        void BatchUpdateGameOpenFlagByAgentSelf(int userID, MerchantGameSettingAgentSelfEditViewModel model);

        MerchantDefaultEnableGamesForSubAgnetViewModel GetEnableGamesForSubAgents(int userID, int merchantID);
    }
}
