using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IPlayerTokenLogoutRefService
    {
        void ResetAllSessionToken();

        void ResetMerchantPlayerSessionToken(int merchantID);

        void ResetPlayerSessionToken(int playerID);

        void ResetSessionTokenByParameter(ResetTokenParameter filter);
    }
}
