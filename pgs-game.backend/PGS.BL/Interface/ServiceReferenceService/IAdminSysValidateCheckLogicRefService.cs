using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;
using target.Web.Modules.Filter;

namespace target.BL.Interface
{

    public interface IAdminSysValidateCheckLogicRefService
    {
        void CheckSingleWalletMerchantNotSupprtGame(int merchantID, List<int> gameIDs);

        void CheckSingleWalletMerchantNotSupprtGame(List<int> merchantIDs, List<int> gameIDs);

        void CheckMerchantJackpotGameAndThrow(List<int> myMerchantIDs, List<int> gameIDs);

        void CheckMerchantCurrencyNotSupprtGameAndThrow(List<int> merchantIDs, List<int> gameIDs);

        void CheckGameVendorNotSupprtCurrencyAndThrow(string merchantCurrency, int gameVendor);

        void CheckMerchantOnlySupportIDGVisibleGamesAndThrow(List<int> myMerchantIDs, List<int> gameIDs);

        List<int> ExcludeSingleWalletMerchantNotSupprtGameIDs(List<int> gameIDs);

        TrustUserCheckResult CheckIPAddressAndSourceIsValid(string myIP, string loginUserName, RestrictLevel validateLevel);

        bool IsTrustInternalUserNameAllowLogin(string myIP, string myUserName , int userID);
    }
}
